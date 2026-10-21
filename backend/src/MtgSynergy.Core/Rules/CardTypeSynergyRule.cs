using MtgSynergy.Core.Models;

namespace MtgSynergy.Core.Rules;

public class CardTypeSynergyRule : ISynergyRule
{
    public string RuleName => "Card Type Synergy";
    public int Priority => 3;

    public SynergyContribution? Evaluate(Card sourceCard, Card candidateCard)
    {
        if (sourceCard.Id == candidateCard.Id ||
            (sourceCard.ScryfallId != Guid.Empty && sourceCard.ScryfallId == candidateCard.ScryfallId))
            return null;

        var sourceText = sourceCard.OracleText ?? string.Empty;
        var candidateText = candidateCard.OracleText ?? string.Empty;

        // 1. Creature + Equipment / Aura
        bool sourceIsCreature = sourceCard.IsCreature;
        bool candidateIsCreature = candidateCard.IsCreature;
        bool sourceIsEquipOrAura = sourceCard.Subtypes.Any(s => s.Equals("Equipment", StringComparison.OrdinalIgnoreCase) || s.Equals("Aura", StringComparison.OrdinalIgnoreCase));
        bool candidateIsEquipOrAura = candidateCard.Subtypes.Any(s => s.Equals("Equipment", StringComparison.OrdinalIgnoreCase) || s.Equals("Aura", StringComparison.OrdinalIgnoreCase));

        if ((sourceIsCreature && candidateIsEquipOrAura) || (sourceIsEquipOrAura && candidateIsCreature))
        {
            var itemType = (sourceIsEquipOrAura ? sourceCard.Subtypes : candidateCard.Subtypes)
                .FirstOrDefault(s => s.Equals("Equipment", StringComparison.OrdinalIgnoreCase) || s.Equals("Aura", StringComparison.OrdinalIgnoreCase)) ?? "Equipment";

            return new SynergyContribution(
                RuleName,
                25,
                $"Card type synergy: {itemType} attaches to and buffs creatures",
                "CardType"
            );
        }

        // 2. Instant / Sorcery + Spell-matters
        bool sourceIsSpell = sourceCard.IsInstant || sourceCard.IsSorcery;
        bool candidateIsSpell = candidateCard.IsInstant || candidateCard.IsSorcery;

        bool candidateCaresAboutSpells = candidateText.Contains("instant or sorcery", StringComparison.OrdinalIgnoreCase)
            || candidateText.Contains("noncreature spell", StringComparison.OrdinalIgnoreCase)
            || candidateText.Contains("spells you cast cost", StringComparison.OrdinalIgnoreCase);

        bool sourceCaresAboutSpells = sourceText.Contains("instant or sorcery", StringComparison.OrdinalIgnoreCase)
            || sourceText.Contains("noncreature spell", StringComparison.OrdinalIgnoreCase)
            || sourceText.Contains("spells you cast cost", StringComparison.OrdinalIgnoreCase);

        if ((sourceIsSpell && candidateCaresAboutSpells) || (sourceCaresAboutSpells && candidateIsSpell))
        {
            return new SynergyContribution(
                RuleName,
                35,
                "Spellcraft synergy: Spells trigger spell-matters payoffs",
                "CardType"
            );
        }

        // 3. Artifact + Artifact-matters
        bool sourceIsArtifact = sourceCard.IsArtifact;
        bool candidateIsArtifact = candidateCard.IsArtifact;

        bool candidateCaresAboutArtifacts = candidateText.Contains("artifact you control", StringComparison.OrdinalIgnoreCase)
            || candidateText.Contains("sacrifice an artifact", StringComparison.OrdinalIgnoreCase)
            || candidateText.Contains("whenever an artifact", StringComparison.OrdinalIgnoreCase);

        bool sourceCaresAboutArtifacts = sourceText.Contains("artifact you control", StringComparison.OrdinalIgnoreCase)
            || sourceText.Contains("sacrifice an artifact", StringComparison.OrdinalIgnoreCase)
            || sourceText.Contains("whenever an artifact", StringComparison.OrdinalIgnoreCase);

        if ((sourceIsArtifact && candidateCaresAboutArtifacts) || (sourceCaresAboutArtifacts && candidateIsArtifact))
        {
            return new SynergyContribution(
                RuleName,
                30,
                "Artifact synergy: Triggers or fuels artifact payoffs",
                "CardType"
            );
        }

        // 4. Enchantment + Enchantment-matters
        bool sourceIsEnchantment = sourceCard.IsEnchantment;
        bool candidateIsEnchantment = candidateCard.IsEnchantment;

        bool candidateCaresAboutEnchantments = candidateText.Contains("enchantment you control", StringComparison.OrdinalIgnoreCase)
            || candidateText.Contains("whenever you cast an enchantment", StringComparison.OrdinalIgnoreCase);

        bool sourceCaresAboutEnchantments = sourceText.Contains("enchantment you control", StringComparison.OrdinalIgnoreCase)
            || sourceText.Contains("whenever you cast an enchantment", StringComparison.OrdinalIgnoreCase);

        if ((sourceIsEnchantment && candidateCaresAboutEnchantments) || (sourceCaresAboutEnchantments && candidateIsEnchantment))
        {
            return new SynergyContribution(
                RuleName,
                30,
                "Enchantment synergy: Triggers enchantment payoffs",
                "CardType"
            );
        }

        return null;
    }
}
