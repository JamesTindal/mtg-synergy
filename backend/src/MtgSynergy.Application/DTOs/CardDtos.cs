using MtgSynergy.Domain.Entities;

namespace MtgSynergy.Application.DTOs;

public record CardSummaryDto(
    Guid Id,
    Guid ScryfallId,
    string Name,
    string? ManaCost,
    string TypeLine,
    string? ImageUrlNormal,
    string? ImageUrlArtCrop,
    List<string> Colors,
    string Rarity
)
{
    public static CardSummaryDto FromEntity(Card card) => new(
        card.Id,
        card.ScryfallId,
        card.Name,
        card.ManaCost,
        card.TypeLine,
        card.ImageUrlNormal,
        card.ImageUrlArtCrop,
        card.Colors,
        card.Rarity
    );
}

public record CardDto(
    Guid Id,
    Guid ScryfallId,
    string Name,
    string? ManaCost,
    decimal Cmc,
    string TypeLine,
    string? OracleText,
    string? Power,
    string? Toughness,
    List<string> Colors,
    List<string> ColorIdentity,
    List<string> Keywords,
    string Rarity,
    string SetCode,
    string CollectorNumber,
    string? ImageUrlNormal,
    string? ImageUrlArtCrop
)
{
    public static CardDto FromEntity(Card card) => new(
        card.Id,
        card.ScryfallId,
        card.Name,
        card.ManaCost,
        card.Cmc,
        card.TypeLine,
        card.OracleText,
        card.Power,
        card.Toughness,
        card.Colors,
        card.ColorIdentity,
        card.Keywords,
        card.Rarity,
        card.SetCode,
        card.CollectorNumber,
        card.ImageUrlNormal,
        card.ImageUrlArtCrop
    );
}
