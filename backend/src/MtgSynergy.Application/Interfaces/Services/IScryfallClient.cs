using MtgSynergy.Domain.Entities;

namespace MtgSynergy.Application.Interfaces.Services;

public interface IScryfallClient
{
    Task<List<Card>> FetchSetCardsAsync(string setCode, CancellationToken ct = default);
}
