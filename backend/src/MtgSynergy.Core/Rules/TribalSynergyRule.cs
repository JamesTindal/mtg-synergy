using MtgSynergy.Core.Models;

namespace MtgSynergy.Core.Rules;

public class TribalSynergyRule : ISynergyRule
{
    public string RuleName => "Tribal Synergy";
    public int Priority => 2;

    public SynergyContribution? Evaluate(Card sourceCard, Card candidateCard)
    {
        if (sourceCard.Id == candidateCard.Id ||
            (sourceCard.ScryfallId != Guid.Empty && sourceCard.ScryfallId == candidateCard.ScryfallId))
            return null;

        var sourceSubtypes = sourceCard.Subtypes.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var candidateSubtypes = candidateCard.Subtypes.ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Filter out known non-creature subtypes
        var nonCreatureSubtypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Equipment", "Aura", "Vehicle", "Clue", "Food", "Treasure", "Blood",
            "Saga", "Curse", "Class", "Case", "Plains", "Island", "Swamp", "Mountain", "Forest"
        };

        sourceSubtypes.ExceptWith(nonCreatureSubtypes);
        candidateSubtypes.ExceptWith(nonCreatureSubtypes);

        // Find shared creature subtypes
        var shared = sourceSubtypes.Intersect(candidateSubtypes).ToList();

        // Check if candidate mentions source subtypes in its oracle text
        string? textPayoffType = null;
        bool candidateMentionsSourceType = false;
        bool sourceMentionsCandidateType = false;

        var candidateText = candidateCard.OracleText ?? string.Empty;
        var sourceText = sourceCard.OracleText ?? string.Empty;

        foreach (var subtype in sourceSubtypes)
        {
            if (candidateText.Contains(subtype, StringComparison.OrdinalIgnoreCase))
            {
                candidateMentionsSourceType = true;
                textPayoffType = subtype;
                break;
            }
        }

        foreach (var subtype in candidateSubtypes)
        {
            if (sourceText.Contains(subtype, StringComparison.OrdinalIgnoreCase))
            {
                sourceMentionsCandidateType = true;
                textPayoffType ??= subtype;
                break;
            }
        }

        // Case 1: Direct tribal payoff (one card explicitly mentions or buffs the other's type)
        if (candidateMentionsSourceType || sourceMentionsCandidateType)
        {
            var payoffSubtype = textPayoffType ?? (shared.Count > 0 ? shared[0] : "creature");
            double score = shared.Count > 0 ? 55 : 40;
            return new SynergyContribution(
                RuleName,
                score,
                $"Tribal Payoff: Explicitly buffs/interacts with {payoffSubtype}s",
                "Tribal"
            );
        }

        // Case 2: Shared creature subtype
        if (shared.Count > 0)
        {
            var primarySubtype = shared[0];
            return new SynergyContribution(
                RuleName,
                30,
                $"Shared creature tribe: {primarySubtype}",
                "Tribal"
            );
        }

        return null;
    }
}
