using MtgSynergy.Core.Models;

namespace MtgSynergy.Core.Rules;

public record SynergyContribution(
    string RuleName,
    double Score,
    string Reason,
    string Category
);

public interface ISynergyRule
{
    string RuleName { get; }
    int Priority { get; }

    /// <summary>
    /// Evaluates synergy between sourceCard and candidateCard.
    /// Returns null if no meaningful synergy is detected by this rule.
    /// </summary>
    SynergyContribution? Evaluate(Card sourceCard, Card candidateCard);
}
