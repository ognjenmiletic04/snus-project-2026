using SNUS.Shared.DTOs;

namespace SNUS.IngestionService.Services
{
    public interface ISensorIngestionService
    {
        Task<IngestResponseDto> IngestAsync(
            SensorReadingRequestDto request,
            CancellationToken cancellationToken);
    }
}
