using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MtgSynergy.Core.Models;
using MtgSynergy.Infrastructure.Data;

namespace MtgSynergy.Infrastructure.Scryfall;

public class ScryfallIngestionService
{
    private readonly ScryfallClient _client;
    private readonly AppDbContext _db;
    private readonly ILogger<ScryfallIngestionService> _logger;

    public ScryfallIngestionService(ScryfallClient client, AppDbContext db, ILogger<ScryfallIngestionService> logger)
    {
        _client = client;
        _db = db;
        _logger = logger;
    }

    public async Task<int> IngestSetAsync(string setCode, CancellationToken ct = default)
    {
        _logger.LogInformation("Starting ingestion for set: {SetCode}", setCode);
        var fetchedCards = await _client.FetchSetCardsAsync(setCode, ct);

        int addedCount = 0;
        int updatedCount = 0;

        var existingCards = await _db.Cards
            .Where(c => c.SetCode == setCode.ToLowerInvariant())
            .ToDictionaryAsync(c => c.ScryfallId, ct);

        foreach (var card in fetchedCards)
        {
            if (existingCards.TryGetValue(card.ScryfallId, out var existing))
            {
                existing.Name = card.Name;
                existing.ManaCost = card.ManaCost;
                existing.Cmc = card.Cmc;
                existing.TypeLine = card.TypeLine;
                existing.OracleText = card.OracleText;
                existing.Power = card.Power;
                existing.Toughness = card.Toughness;
                existing.Colors = card.Colors;
                existing.ColorIdentity = card.ColorIdentity;
                existing.Keywords = card.Keywords;
                existing.Rarity = card.Rarity;
                existing.ImageUrlNormal = card.ImageUrlNormal;
                existing.ImageUrlArtCrop = card.ImageUrlArtCrop;
                updatedCount++;
            }
            else
            {
                _db.Cards.Add(card);
                addedCount++;
            }
        }

        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Ingestion complete for {SetCode}. Added: {Added}, Updated: {Updated}", setCode, addedCount, updatedCount);
        return addedCount + updatedCount;
    }
}
