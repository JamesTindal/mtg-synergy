using MtgSynergy.Domain.Entities;

namespace MtgSynergy.Application.Interfaces.Repositories;

public interface ICardRepository
{
    Task<Card?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Card>> SearchByNameAsync(string query, string? setCode, int limit, CancellationToken ct = default);
    Task<List<Card>> GetCandidatesBySetAsync(string setCode, Guid excludeCardId, CancellationToken ct = default);
    Task<int> UpsertBatchAsync(IEnumerable<Card> cards, CancellationToken ct = default);
}
