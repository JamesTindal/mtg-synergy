using Microsoft.EntityFrameworkCore;
using MtgSynergy.Application.Interfaces.Repositories;
using MtgSynergy.Domain.Entities;
using MtgSynergy.Infrastructure.Data;

namespace MtgSynergy.Infrastructure.Persistence.Repositories;

public class CardRepository : ICardRepository
{
    private readonly AppDbContext _db;

    public CardRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Card?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.Cards
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id || c.ScryfallId == id, ct);
    }

    public async Task<List<Card>> SearchByNameAsync(string query, string? setCode, int limit, CancellationToken ct = default)
    {
        var queryable = _db.Cards.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(setCode))
        {
            queryable = queryable.Where(c => c.SetCode == setCode.ToLower());
        }

        string lowerQuery = query.ToLower();

        return await queryable
            .Where(c => EF.Functions.ILike(c.Name, $"%{query}%"))
            .OrderBy(c => c.Name.ToLower().StartsWith(lowerQuery) ? 0 : 1)
            .ThenBy(c => c.Name)
            .Take(limit)
            .ToListAsync(ct);
    }

    public async Task<List<Card>> GetCandidatesBySetAsync(string setCode, Guid excludeCardId, CancellationToken ct = default)
    {
        return await _db.Cards
            .AsNoTracking()
            .Where(c => c.SetCode == setCode.ToLower() && c.Id != excludeCardId)
            .ToListAsync(ct);
    }

    public async Task<int> UpsertBatchAsync(IEnumerable<Card> cards, CancellationToken ct = default)
    {
        var cardList = cards.ToList();
        if (cardList.Count == 0) return 0;

        string setCode = cardList[0].SetCode.ToLower();

        var existingCards = await _db.Cards
            .Where(c => c.SetCode == setCode)
            .ToDictionaryAsync(c => c.ScryfallId, ct);

        int updatedCount = 0;
        int addedCount = 0;

        foreach (var card in cardList)
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
        return addedCount + updatedCount;
    }
}
