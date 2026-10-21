using MtgSynergy.Application.DTOs;

namespace MtgSynergy.Application.Interfaces.Services;

public interface ICardSynergyService
{
    Task<List<CardSummaryDto>> SearchCardsAsync(string query, string? setCode, int limit = 10, CancellationToken ct = default);
    Task<CardDto?> GetCardByIdAsync(Guid id, CancellationToken ct = default);
    Task<SynergyResponseDto> GetCardSynergiesAsync(Guid id, double minScore = 20, int limit = 30, CancellationToken ct = default);
}
