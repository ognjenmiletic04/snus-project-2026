using Microsoft.EntityFrameworkCore;
using SNUS.Persistence;
using SNUS.Persistence.Entities;
using SNUS.Shared.DTOs;
using SNUS.Shared.Enums;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Concurrent;

namespace SNUS.IngestionService.Services
{
    public class SensorIngestionService : ISensorIngestionService
    {
        private readonly AppDbContext dbContext;
        private readonly ILogger<SensorIngestionService> logger;

        private static readonly ConcurrentDictionary<string, List<DateTime>> _requestHistory = new();
        private const int MAX_REQUESTS_PER_SECOND = 10;
        private const int BLOCK_DURATION_SECONDS = 30;

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
                return new IngestResponseDto { Success = false, Message = "SensorId is required." };
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
                logger.LogWarning($"[BLOKADA] Zahtev odbačen. Senzor {externalSensorId} je blokiran do: {sensor.BlockedUntilUtc.Value:HH:mm:ss}");
                return new IngestResponseDto
                {
                    Success = false,
                    SensorId = externalSensorId,
                    Message = $"Sensor is temporarily blocked due to DoS protection until {sensor.BlockedUntilUtc.Value:O}."
                };
            }

            var timestamps = _requestHistory.GetOrAdd(externalSensorId, _ => new List<DateTime>());

            lock (timestamps)
            {

                timestamps.RemoveAll(t => t < now.AddSeconds(-1));

                if (timestamps.Count >= MAX_REQUESTS_PER_SECOND)
                {
                    logger.LogCritical($"[DoS NAPAD DETEKTOVAN] Senzor {externalSensorId} je poslao preko {MAX_REQUESTS_PER_SECOND} poruka u sekundi! Pokrećem blokadu od {BLOCK_DURATION_SECONDS}s.");

                    if (sensor is not null)
                    {
                        sensor.BlockedUntilUtc = now.AddSeconds(BLOCK_DURATION_SECONDS);
                        sensor.IsActive = false;
                    }

                    timestamps.Clear();

                    dbContext.SaveChanges();

                    return new IngestResponseDto
                    {
                        Success = false,
                        SensorId = externalSensorId,
                        Message = $"DoS attack detected! Sensor is now blocked for {BLOCK_DURATION_SECONDS} seconds."
                    };
                }

                timestamps.Add(now);
            }



            if (string.IsNullOrEmpty(request.PublicKey) ||
                string.IsNullOrEmpty(request.DigitalSignature) ||
                string.IsNullOrEmpty(request.EncryptedPayload))
            {
                logger.LogWarning($"[BEZBEDNOST] Odbijen zahtev za senzor {externalSensorId}. Nedostaju kriptografski podaci!");
                return new IngestResponseDto
                {
                    Success = false,
                    SensorId = externalSensorId,
                    Message = "Security violation: Cryptographic fields are required."
                };
            }

            if (!VerifyAndDecrypt(request, out double decryptedTemperature))
            {
                logger.LogError($"[BEZBEDNOST] KRITIČNO: Neuspešna verifikacija potpisa ili dekripcija za senzor {externalSensorId}!");
                return new IngestResponseDto
                {
                    Success = false,
                    SensorId = externalSensorId,
                    Message = "Security violation: Invalid digital signature or tampered data."
                };
            }

            logger.LogInformation($"[BEZBEDNOST] Uspešan potpis i dekripcija za {externalSensorId}. Sigurna Temp: {decryptedTemperature}°C");
            request.Temperature = decryptedTemperature;


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
                Message = "Reading verified, decrypted and stored successfully."
            };
        }

        private bool VerifyAndDecrypt(SensorReadingRequestDto dto, out double decryptedTemperature)
        {
            decryptedTemperature = 0;
            try
            {
                byte[] publicKeyBytes = Convert.FromBase64String(dto.PublicKey!);
                using (ECDsa ecdsa = ECDsa.Create())
                {
                    ecdsa.ImportSubjectPublicKeyInfo(publicKeyBytes, out _);
                    string timestampStr = dto.TimestampUtc.ToString("o");
                    string dataToVerify = $"{dto.SensorId}|{dto.MessageId}|{dto.EncryptedPayload}|{timestampStr}";

                    byte[] dataBytes = Encoding.UTF8.GetBytes(dataToVerify);
                    byte[] signatureBytes = Convert.FromBase64String(dto.DigitalSignature!);

                    bool isSignatureValid = ecdsa.VerifyData(dataBytes, signatureBytes, HashAlgorithmName.SHA256);
                    if (!isSignatureValid)
                    {
                        return false;
                    }
                }
                byte[] AesKey = SHA256.HashData(Encoding.UTF8.GetBytes("SuperTajniKljuZaSNUS2026"));
                byte[] AesIv = new byte[16] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };

                using (Aes aes = Aes.Create())
                {
                    aes.Key = AesKey;
                    aes.IV = AesIv;
                    ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                    byte[] cipherBytes = Convert.FromBase64String(dto.EncryptedPayload!);
                    using (MemoryStream ms = new MemoryStream(cipherBytes))
                    {
                        using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader sr = new StreamReader(cs))
                            {
                                string plainText = sr.ReadToEnd();
                                decryptedTemperature = double.Parse(plainText, System.Globalization.CultureInfo.InvariantCulture);
                                return true;
                            }
                        }
                    }
                }
            }
            catch
            {
                return false;
            }
        }
    }
}