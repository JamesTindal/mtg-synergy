using MtgSynergy.Application.DTOs;
using MtgSynergy.Application.Interfaces.Services;
using MtgSynergy.Domain.Entities;
using MtgSynergy.Domain.Rules;

namespace MtgSynergy.Application.Services;

public class SynergyEngine : ISynergyEngine
{
    private readonly List<ISynergyRule> _rules;

    public SynergyEngine(IEnumerable<ISynergyRule> rules)
    {
        _rules = rules.OrderBy(r => r.Priority).ToList();
    }

    public List<SynergyMatchDto> FindSynergies(Card sourceCard, IEnumerable<Card> candidates, double minScore = 20, int limit = 30)
    {
        var matches = new List<SynergyMatchDto>();

        foreach (var candidate in candidates)
        {
            if (candidate.Id == sourceCard.Id ||
                (sourceCard.ScryfallId != Guid.Empty && candidate.ScryfallId == sourceCard.ScryfallId))
                continue;

            var contributions = new List<SynergyContributionDto>();
            double totalScore = 0;

            foreach (var rule in _rules)
            {
                var contribution = rule.Evaluate(sourceCard, candidate);
                if (contribution != null)
                {
                    contributions.Add(new SynergyContributionDto(
                        contribution.RuleName,
                        contribution.Score,
                        contribution.Reason,
                        contribution.Category
                    ));
                    totalScore += contribution.Score;
                }
            }

            // Cap score at 100
            totalScore = Math.Min(100, Math.Round(totalScore, 1));

            if (totalScore >= minScore)
            {
                matches.Add(new SynergyMatchDto(
                    CardDto.FromEntity(candidate),
                    totalScore,
                    contributions.Select(c => c.Reason).Distinct().ToList(),
                    contributions
                ));
            }
        }

        return matches
            .OrderByDescending(m => m.TotalScore)
            .Take(limit)
            .ToList();
    }
}
