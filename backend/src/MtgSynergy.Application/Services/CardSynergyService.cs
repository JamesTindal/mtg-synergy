using MtgSynergy.Application.DTOs;
using MtgSynergy.Application.Interfaces.Repositories;
using MtgSynergy.Application.Interfaces.Services;
using MtgSynergy.Domain.Exceptions;

namespace MtgSynergy.Application.Services;

public class CardSynergyService : ICardSynergyService
{
    private readonly ICardRepository _repository;
    private readonly ISynergyEngine _synergyEngine;

    public CardSynergyService(ICardRepository repository, ISynergyEngine synergyEngine)
    {
        _repository = repository;
        _synergyEngine = synergyEngine;
    }

    public async Task<List<CardSummaryDto>> SearchCardsAsync(string query, string? setCode, int limit = 10, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return [];

        int maxLimit = limit > 0 ? Math.Min(limit, 50) : 10;
        var cards = await _repository.SearchByNameAsync(query.Trim(), setCode, maxLimit, ct);

        return cards.Select(CardSummaryDto.FromEntity).ToList();
    }

    public async Task<CardDto?> GetCardByIdAsync(Guid id, CancellationToken ct = default)
    {
        var card = await _repository.GetByIdAsync(id, ct);
        return card != null ? CardDto.FromEntity(card) : null;
    }

    public async Task<SynergyResponseDto> GetCardSynergiesAsync(Guid id, double minScore = 20, int limit = 30, CancellationToken ct = default)
    {
        var sourceCard = await _repository.GetByIdAsync(id, ct);
        if (sourceCard == null)
            throw new CardNotFoundException(id);

        var candidates = await _repository.GetCandidatesBySetAsync(sourceCard.SetCode, sourceCard.Id, ct);

        double threshold = minScore > 0 ? minScore : 20.0;
        int maxLimit = limit > 0 ? Math.Min(limit, 50) : 30;

        var matches = _synergyEngine.FindSynergies(sourceCard, candidates, threshold, maxLimit);

        return new SynergyResponseDto(
            CardDto.FromEntity(sourceCard),
            matches.Count,
            matches
        );
    }
}
