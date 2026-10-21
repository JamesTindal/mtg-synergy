using Microsoft.Extensions.Logging;
using MtgSynergy.Application.Interfaces.Repositories;
using MtgSynergy.Application.Interfaces.Services;

namespace MtgSynergy.Infrastructure.Scryfall;

public class ScryfallIngestionService : ISetIngestionService
{
    private readonly IScryfallClient _client;
    private readonly ICardRepository _repository;
    private readonly ILogger<ScryfallIngestionService> _logger;

    public ScryfallIngestionService(IScryfallClient client, ICardRepository repository, ILogger<ScryfallIngestionService> logger)
    {
        _client = client;
        _repository = repository;
        _logger = logger;
    }

    public async Task<int> IngestSetAsync(string setCode, CancellationToken ct = default)
    {
        _logger.LogInformation("Starting ingestion for set: {SetCode}", setCode);
        var fetchedCards = await _client.FetchSetCardsAsync(setCode, ct);

        int count = await _repository.UpsertBatchAsync(fetchedCards, ct);
        _logger.LogInformation("Ingestion complete for {SetCode}. Upserted cards: {Count}", setCode, count);
        return count;
    }
}
