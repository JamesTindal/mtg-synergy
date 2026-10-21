namespace MtgSynergy.Core.Models;

public class Card
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ScryfallId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ManaCost { get; set; }
    public decimal Cmc { get; set; }
    public string TypeLine { get; set; } = string.Empty;
    public string? OracleText { get; set; }
    public string? Power { get; set; }
    public string? Toughness { get; set; }
    public List<string> Colors { get; set; } = [];
    public List<string> ColorIdentity { get; set; } = [];
    public List<string> Keywords { get; set; } = [];
    public string Rarity { get; set; } = "common";
    public string SetCode { get; set; } = string.Empty;
    public string CollectorNumber { get; set; } = string.Empty;
    public string? ImageUrlNormal { get; set; }
    public string? ImageUrlArtCrop { get; set; }

    // Computed card types & subtypes
    public List<string> MainTypes => ExtractMainTypes(TypeLine);
    public List<string> Subtypes => ExtractSubtypes(TypeLine);

    public bool IsCreature => MainTypes.Contains("Creature");
    public bool IsInstant => MainTypes.Contains("Instant");
    public bool IsSorcery => MainTypes.Contains("Sorcery");
    public bool IsArtifact => MainTypes.Contains("Artifact");
    public bool IsEnchantment => MainTypes.Contains("Enchantment");
    public bool IsPlaneswalker => MainTypes.Contains("Planeswalker");
    public bool IsLand => MainTypes.Contains("Land");

    private static List<string> ExtractMainTypes(string typeLine)
    {
        if (string.IsNullOrWhiteSpace(typeLine)) return [];
        var parts = typeLine.Split('—', '–', '-');
        var left = parts[0].Trim();
        var known = new[] { "Creature", "Artifact", "Enchantment", "Instant", "Sorcery", "Planeswalker", "Land", "Battle", "Kindred" };
        return known.Where(k => left.Contains(k, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    private static List<string> ExtractSubtypes(string typeLine)
    {
        if (string.IsNullOrWhiteSpace(typeLine)) return [];
        var parts = typeLine.Split('—', '–', '-');
        if (parts.Length < 2) return [];
        return parts[1]
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();
    }
}
