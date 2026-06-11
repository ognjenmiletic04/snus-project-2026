using Microsoft.EntityFrameworkCore;
using SNUS.Persistence;
using SNUS.Persistence.Entities;
using SNUS.Shared.DTOs;
using SNUS.Shared.Enums;

namespace SNUS.IngestionService.Services
{
    public class SensorIngestionService : ISensorIngestionService
    {
        private readonly AppDbContext dbContext;
        private readonly ILogger<SensorIngestionService> logger;

        public SensorIngestionService(
            AppDbContext dbContext,
            ILogger<SensorIngestionService> logger)
        {
            this.dbContext = dbContext;
            this.logger = logger;
        }

        public async Task<IngestResponseDto> IngestAsync(
            SensorReadingRequestDto request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.SensorId))
            {
                return new IngestResponseDto
                {
                    Success = false,
                    Message = "SensorId is required."
                };
            }

            if (request.MessageId <= 0)
            {
                return new IngestResponseDto
                {
                    Success = false,
                    SensorId = request.SensorId,
                    Message = "MessageId must be greater than zero."
                };
            }

            DateTime now = DateTime.UtcNow;
            string externalSensorId = request.SensorId.Trim();

            if (request.TimestampUtc == default)
            {
                request.TimestampUtc = now;
            }

            Sensor? sensor = await dbContext.Sensors
                .FirstOrDefaultAsync(x => x.ExternalId == externalSensorId, cancellationToken);

            if (sensor is not null &&
                sensor.BlockedUntilUtc.HasValue &&
                sensor.BlockedUntilUtc.Value > now)
            {
                return new IngestResponseDto
                {
                    Success = false,
                    SensorId = externalSensorId,
                    Message = $"Sensor is temporarily blocked until {sensor.BlockedUntilUtc.Value:O}."
                };
            }

            if (sensor is not null)
            {
                bool alreadyProcessed = await dbContext.ProcessedMessages
                    .AnyAsync(
                        x => x.SensorId == sensor.Id && x.MessageId == request.MessageId,
                        cancellationToken);

                if (alreadyProcessed)
                {
                    return new IngestResponseDto
                    {
                        Success = false,
                        SensorId = externalSensorId,
                        Message = "Duplicate message detected."
                    };
                }
            }

            if (sensor is null)
            {
                sensor = new Sensor
                {
                    ExternalId = externalSensorId,
                    CreatedAtUtc = now
                };

                dbContext.Sensors.Add(sensor);
            }

            sensor.DataQuality = request.DataQuality;
            sensor.LastMessageAtUtc = now;
            sensor.IsActive = true;

            SensorReading reading = new SensorReading
            {
                Sensor = sensor,
                Temperature = request.Temperature,
                MeasuredAtUtc = request.TimestampUtc,
                ReceivedAtUtc = now,
                MessageId = request.MessageId,
                DataQuality = request.DataQuality,
                AlarmPriority = request.AlarmPriority,
                IsConsensus = false
            };

            dbContext.SensorReadings.Add(reading);

            ProcessedMessage processedMessage = new ProcessedMessage
            {
                Sensor = sensor,
                MessageId = request.MessageId,
                MessageTimestampUtc = request.TimestampUtc,
                ReceivedAtUtc = now
            };

            dbContext.ProcessedMessages.Add(processedMessage);

            if (request.AlarmPriority != AlarmPriority.None)
            {
                Alarm alarm = new Alarm
                {
                    Sensor = sensor,
                    SensorReading = reading,
                    Priority = request.AlarmPriority,
                    Temperature = request.Temperature,
                    OccurredAtUtc = request.TimestampUtc,
                    CreatedAtUtc = now
                };

                dbContext.Alarms.Add(alarm);
            }

            try
            {
                await dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex)
            {
                logger.LogWarning(ex, "Failed to save sensor reading. Possible duplicate message.");

                return new IngestResponseDto
                {
                    Success = false,
                    SensorId = externalSensorId,
                    Message = "Failed to store reading. Possible duplicate message."
                };
            }

            return new IngestResponseDto
            {
                Success = true,
                SensorId = externalSensorId,
                ReadingId = reading.Id,
                AlarmPriority = request.AlarmPriority,
                Message = "Reading stored successfully."
            };
        }
    }
}
