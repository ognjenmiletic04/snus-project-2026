using Microsoft.AspNetCore.Mvc;
using SNUS.IngestionService.Services;
using SNUS.Shared.DTOs;

namespace SNUS.IngestionService.Controllers
{
    [ApiController]
    [Route("api/ingest")]
    public class IngestController : ControllerBase
    {
        private readonly ISensorIngestionService sensorIngestionService;

        public IngestController(ISensorIngestionService sensorIngestionService)
        {
            this.sensorIngestionService = sensorIngestionService;
        }

        [HttpPost]
        public async Task<ActionResult<IngestResponseDto>> Ingest(
            [FromBody] SensorReadingRequestDto request,
            CancellationToken cancellationToken)
        {
            IngestResponseDto response = await sensorIngestionService
                .IngestAsync(request, cancellationToken);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
    }
}
