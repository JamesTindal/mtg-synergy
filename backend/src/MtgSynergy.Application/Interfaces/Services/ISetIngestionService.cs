namespace MtgSynergy.Application.Interfaces.Services;

public interface ISetIngestionService
{
    Task<int> IngestSetAsync(string setCode, CancellationToken ct = default);
}
