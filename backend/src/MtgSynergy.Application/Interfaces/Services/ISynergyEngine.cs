using MtgSynergy.Application.DTOs;
using MtgSynergy.Domain.Entities;

namespace MtgSynergy.Application.Interfaces.Services;

public interface ISynergyEngine
{
    List<SynergyMatchDto> FindSynergies(Card sourceCard, IEnumerable<Card> candidates, double minScore = 20, int limit = 30);
}
