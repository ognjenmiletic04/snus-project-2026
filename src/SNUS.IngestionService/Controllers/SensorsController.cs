using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SNUS.Persistence;
using SNUS.Shared.DTOs;

namespace SNUS.IngestionService.Controllers
{
    [ApiController]
    [Route("api/sensors")]
    public class SensorsController : ControllerBase
    {
        private readonly AppDbContext dbContext;

        public SensorsController(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SensorStatusDto>>> GetSensors(
            CancellationToken cancellationToken)
        {
            var sensors = await dbContext.Sensors
                .OrderBy(x => x.ExternalId)
                .Select(x => new SensorStatusDto
                {
                    SensorId = x.ExternalId,
                    DataQuality = x.DataQuality,
                    IsActive = x.IsActive,
                    LastMessageAtUtc = x.LastMessageAtUtc,
                    BlockedUntilUtc = x.BlockedUntilUtc
                })
                .ToListAsync(cancellationToken);

            return Ok(sensors);
        }

        [HttpPost("{externalId}/block")]
        public async Task<IActionResult> BlockSensor(
            string externalId,
            CancellationToken cancellationToken)
        {
            var sensor = await dbContext.Sensors
                .FirstOrDefaultAsync(x => x.ExternalId == externalId, cancellationToken);

            if (sensor is null)
            {
                return NotFound($"Sensor '{externalId}' was not found.");
            }

            sensor.BlockedUntilUtc = DateTime.UtcNow.AddSeconds(30);
            sensor.IsActive = false;

            await dbContext.SaveChangesAsync(cancellationToken);

            return Ok($"Sensor '{externalId}' is blocked for 30 seconds.");
        }

        [HttpPost("{externalId}/unblock")]
        public async Task<IActionResult> UnblockSensor(
            string externalId,
            CancellationToken cancellationToken)
        {
            var sensor = await dbContext.Sensors
                .FirstOrDefaultAsync(x => x.ExternalId == externalId, cancellationToken);

            if (sensor is null)
            {
                return NotFound($"Sensor '{externalId}' was not found.");
            }

            sensor.BlockedUntilUtc = null;

            await dbContext.SaveChangesAsync(cancellationToken);

            return Ok($"Sensor '{externalId}' is unblocked.");
        }
    }
}
