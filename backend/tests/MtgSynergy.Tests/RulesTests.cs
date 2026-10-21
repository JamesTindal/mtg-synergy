using MtgSynergy.Application.Interfaces.Repositories;
using MtgSynergy.Application.Services;
using MtgSynergy.Domain.Entities;
using MtgSynergy.Domain.Exceptions;
using MtgSynergy.Domain.Rules;
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

    [Fact]
    public async Task CardSynergyService_ShouldThrow_WhenCardDoesNotExist()
    {
        var fakeRepo = new FakeCardRepository([]);
        var engine = new SynergyEngine([]);
        var service = new CardSynergyService(fakeRepo, engine);

        await Assert.ThrowsAsync<CardNotFoundException>(() =>
            service.GetCardSynergiesAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task CardSynergyService_ShouldReturnSynergies_WhenCardExists()
    {
        var cardA = new Card
        {
            Id = Guid.NewGuid(),
            Name = "Vampire Alpha",
            TypeLine = "Creature — Vampire",
            SetCode = "vow",
            Colors = ["B"]
        };

        var cardB = new Card
        {
            Id = Guid.NewGuid(),
            Name = "Vampire Beta",
            TypeLine = "Creature — Vampire",
            SetCode = "vow",
            Colors = ["B"]
        };

        var fakeRepo = new FakeCardRepository([cardA, cardB]);
        var rules = new List<ISynergyRule> { new TribalSynergyRule(), new ColorSynergyRule() };
        var engine = new SynergyEngine(rules);
        var service = new CardSynergyService(fakeRepo, engine);

        var result = await service.GetCardSynergiesAsync(cardA.Id);

        Assert.NotNull(result);
        Assert.Equal(cardA.Name, result.SourceCard.Name);
        Assert.Single(result.Synergies);
        Assert.Equal(cardB.Name, result.Synergies[0].Card.Name);
    }

    private class FakeCardRepository : ICardRepository
    {
        private readonly List<Card> _cards;

        public FakeCardRepository(List<Card> cards) => _cards = cards;

        public Task<Card?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
            Task.FromResult(_cards.FirstOrDefault(c => c.Id == id || c.ScryfallId == id));

        public Task<List<Card>> SearchByNameAsync(string query, string? setCode, int limit, CancellationToken ct = default) =>
            Task.FromResult(_cards.Where(c => c.Name.Contains(query, StringComparison.OrdinalIgnoreCase)).Take(limit).ToList());

        public Task<List<Card>> GetCandidatesBySetAsync(string setCode, Guid excludeCardId, CancellationToken ct = default) =>
            Task.FromResult(_cards.Where(c => c.SetCode.Equals(setCode, StringComparison.OrdinalIgnoreCase) && c.Id != excludeCardId).ToList());

        public Task<int> UpsertBatchAsync(IEnumerable<Card> cards, CancellationToken ct = default) =>
            Task.FromResult(cards.Count());
    }
}
