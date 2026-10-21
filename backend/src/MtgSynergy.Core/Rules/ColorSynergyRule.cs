using MtgSynergy.Core.Models;

namespace MtgSynergy.Core.Rules;

public class ColorSynergyRule : ISynergyRule
{
    public string RuleName => "Color Synergy";
    public int Priority => 1;

    public SynergyContribution? Evaluate(Card sourceCard, Card candidateCard)
    {
        // Don't compare a card to itself
        if (sourceCard.Id == candidateCard.Id ||
            (sourceCard.ScryfallId != Guid.Empty && sourceCard.ScryfallId == candidateCard.ScryfallId))
            return null;

        var sourceColors = sourceCard.Colors.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var candidateColors = candidateCard.Colors.ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Colorless candidate (e.g. colorless artifacts) can fit into any deck
        if (candidateColors.Count == 0)
        {
            return new SynergyContribution(
                RuleName,
                10,
                "Colorless card fits universally into any deck",
                "Color"
            );
        }

        // Colorless source
        if (sourceColors.Count == 0)
        {
            return new SynergyContribution(
                RuleName,
                10,
                "Universally playable alongside colorless cards",
                "Color"
            );
        }

        // Shared colors
        var intersection = sourceColors.Intersect(candidateColors).ToList();
        if (intersection.Count == 0)
            return null;

        // Exact match
        if (sourceColors.SetEquals(candidateColors))
        {
            var colorNames = string.Join("/", intersection);
            return new SynergyContribution(
                RuleName,
                25,
                $"Exact {colorNames} color alignment",
                "Color"
            );
        }

        // Partial overlap
        var overlapNames = string.Join("/", intersection);
        return new SynergyContribution(
            RuleName,
            15,
            $"Shared {overlapNames} color identity",
            "Color"
        );
    }
}
