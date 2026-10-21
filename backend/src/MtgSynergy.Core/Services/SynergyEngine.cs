using MtgSynergy.Core.Models;
using MtgSynergy.Core.Rules;

namespace MtgSynergy.Core.Services;

public class SynergyEngine
{
    private readonly List<ISynergyRule> _rules;

    public SynergyEngine(IEnumerable<ISynergyRule> rules)
    {
        _rules = rules.OrderBy(r => r.Priority).ToList();
    }

    public List<SynergyMatch> FindSynergies(Card sourceCard, IEnumerable<Card> candidates, double minScore = 20, int limit = 30)
    {
        var matches = new List<SynergyMatch>();

        foreach (var candidate in candidates)
        {
            if (candidate.Id == sourceCard.Id ||
                (sourceCard.ScryfallId != Guid.Empty && candidate.ScryfallId == sourceCard.ScryfallId))
                continue;

            var contributions = new List<SynergyContribution>();
            double totalScore = 0;

            foreach (var rule in _rules)
            {
                var contribution = rule.Evaluate(sourceCard, candidate);
                if (contribution != null)
                {
                    contributions.Add(contribution);
                    totalScore += contribution.Score;
                }
            }

            // Cap score at 100
            totalScore = Math.Min(100, Math.Round(totalScore, 1));

            if (totalScore >= minScore)
            {
                matches.Add(new SynergyMatch
                {
                    Card = candidate,
                    TotalScore = totalScore,
                    Contributions = contributions,
                    Reasons = contributions.Select(c => c.Reason).Distinct().ToList()
                });
            }
        }

        return matches
            .OrderByDescending(m => m.TotalScore)
            .Take(limit)
            .ToList();
    }
}
