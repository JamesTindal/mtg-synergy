using MtgSynergy.Core.Models;
using MtgSynergy.Core.Rules;
using MtgSynergy.Core.Services;
using Xunit;

namespace MtgSynergy.Tests;

public class RulesTests
{
    [Fact]
    public void TribalSynergyRule_ShouldAwardBonus_WhenBothAreVampires()
    {
        var rule = new TribalSynergyRule();
        var anje = new Card
        {
            Id = Guid.NewGuid(),
            Name = "Anje, Maid of Dishonor",
            TypeLine = "Legendary Creature — Vampire Blood",
            OracleText = "Whenever Anje enters or attacks, create a Blood token.",
            Colors = ["B", "R"]
        };

        var strefan = new Card
        {
            Id = Guid.NewGuid(),
            Name = "Strefan, Maurer Progenitor",
            TypeLine = "Legendary Creature — Vampire Noble",
            OracleText = "At the beginning of your end step, if an opponent lost life this turn, create a Blood token. Whenever you attack, you may sacrifice two Blood tokens. If you do, you may put a Vampire card from your hand onto the battlefield tapped and attacking.",
            Colors = ["B", "R"]
        };

        var contribution = rule.Evaluate(anje, strefan);

        Assert.NotNull(contribution);
        Assert.Equal("Tribal Synergy", contribution.RuleName);
        Assert.True(contribution.Score >= 50); // Explicitly mentions Vampire payoff!
        Assert.Contains("Vampire", contribution.Reason);
    }

    [Fact]
    public void ColorSynergyRule_ShouldDetectExactMatch()
    {
        var rule = new ColorSynergyRule();
        var cardA = new Card { Id = Guid.NewGuid(), Name = "Card A", Colors = ["B", "R"] };
        var cardB = new Card { Id = Guid.NewGuid(), Name = "Card B", Colors = ["B", "R"] };

        var contribution = rule.Evaluate(cardA, cardB);

        Assert.NotNull(contribution);
        Assert.Equal(25, contribution.Score);
        Assert.Contains("Exact", contribution.Reason);
    }

    [Fact]
    public void CardTypeSynergyRule_ShouldDetectCreatureAndEquipment()
    {
        var rule = new CardTypeSynergyRule();
        var creature = new Card
        {
            Id = Guid.NewGuid(),
            Name = "Vampire Slayer",
            TypeLine = "Creature — Human Soldier"
        };

        var equipment = new Card
        {
            Id = Guid.NewGuid(),
            Name = "Ceremonial Knife",
            TypeLine = "Artifact — Equipment",
            OracleText = "Equipped creature gets +1/+0.\nEquip {1}"
        };

        var contribution = rule.Evaluate(creature, equipment);

        Assert.NotNull(contribution);
        Assert.Equal(25, contribution.Score);
        Assert.Contains("Equipment", contribution.Reason);
    }

    [Fact]
    public void SynergyEngine_ShouldAggregateScoresAndSort()
    {
        var rules = new List<ISynergyRule>
        {
            new ColorSynergyRule(),
            new TribalSynergyRule(),
            new CardTypeSynergyRule()
        };

        var engine = new SynergyEngine(rules);

        var vampireA = new Card
        {
            Id = Guid.NewGuid(),
            Name = "Vampire Lord",
            TypeLine = "Creature — Vampire",
            OracleText = "Other Vampires get +1/+1.",
            Colors = ["B"]
        };

        var vampireB = new Card
        {
            Id = Guid.NewGuid(),
            Name = "Vampire Lackey",
            TypeLine = "Creature — Vampire",
            OracleText = "Flying",
            Colors = ["B"]
        };

        var unrelatedCard = new Card
        {
            Id = Guid.NewGuid(),
            Name = "Island",
            TypeLine = "Basic Land — Island",
            Colors = ["U"]
        };

        var results = engine.FindSynergies(vampireA, [vampireB, unrelatedCard], minScore: 10);

        Assert.Single(results);
        Assert.Equal("Vampire Lackey", results[0].Card.Name);
        Assert.True(results[0].TotalScore > 50); // Color + Tribal Payoff
        Assert.Equal(2, results[0].Contributions.Count);
    }
}
