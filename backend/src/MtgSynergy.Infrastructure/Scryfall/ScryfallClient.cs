using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using MtgSynergy.Core.Models;

namespace MtgSynergy.Infrastructure.Scryfall;

public class ScryfallClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ScryfallClient> _logger;

    public ScryfallClient(HttpClient httpClient, ILogger<ScryfallClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        _httpClient.BaseAddress = new Uri("https://api.scryfall.com/");
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("MtgSynergy/1.0");
        _httpClient.DefaultRequestHeaders.Accept.ParseAdd("application/json");
    }

    public async Task<List<Card>> FetchSetCardsAsync(string setCode, CancellationToken ct = default)
    {
        var cards = new List<Card>();
        string url = $"cards/search?q=set:{setCode}&order=set";

        while (!string.IsNullOrEmpty(url))
        {
            _logger.LogInformation("Fetching Scryfall page: {Url}", url);
            var response = await _httpClient.GetFromJsonAsync<ScryfallSearchResponse>(url, ct);

            if (response == null || response.Data.Count == 0)
                break;

            foreach (var dto in response.Data)
            {
                var card = MapToCard(dto);
                cards.Add(card);
            }

            if (response.HasMore && !string.IsNullOrEmpty(response.NextPage))
            {
                url = response.NextPage;
                // Scryfall asks for 50-100ms delay between requests
                await Task.Delay(100, ct);
            }
            else
            {
                break;
            }
        }

        _logger.LogInformation("Finished fetching set {SetCode}. Total cards: {Count}", setCode, cards.Count);
        return cards;
    }

    private static Card MapToCard(ScryfallCardDto dto)
    {
        string? manaCost = dto.ManaCost;
        string typeLine = dto.TypeLine ?? string.Empty;
        string? oracleText = dto.OracleText;
        string? power = dto.Power;
        string? toughness = dto.Toughness;
        var colors = dto.Colors ?? [];
        string? imgNormal = dto.ImageUris?.Normal;
        string? imgArtCrop = dto.ImageUris?.ArtCrop;

        // Double-faced cards (e.g. Werewolves, Disturb in Crimson Vow)
        if (dto.CardFaces != null && dto.CardFaces.Count > 0)
        {
            var face1 = dto.CardFaces[0];
            manaCost ??= face1.ManaCost;
            if (string.IsNullOrWhiteSpace(typeLine))
                typeLine = string.Join(" // ", dto.CardFaces.Select(f => f.TypeLine));

            if (string.IsNullOrWhiteSpace(oracleText))
                oracleText = string.Join("\n//\n", dto.CardFaces.Select(f => $"{f.Name}: {f.OracleText}"));

            power ??= face1.Power;
            toughness ??= face1.Toughness;
            if (colors.Count == 0 && face1.Colors != null)
                colors = face1.Colors;

            imgNormal ??= face1.ImageUris?.Normal;
            imgArtCrop ??= face1.ImageUris?.ArtCrop;
        }

        return new Card
        {
            ScryfallId = dto.Id,
            Name = dto.Name,
            ManaCost = manaCost,
            Cmc = dto.Cmc,
            TypeLine = typeLine,
            OracleText = oracleText,
            Power = power,
            Toughness = toughness,
            Colors = colors,
            ColorIdentity = dto.ColorIdentity ?? [],
            Keywords = dto.Keywords ?? [],
            Rarity = dto.Rarity,
            SetCode = dto.Set.ToLowerInvariant(),
            CollectorNumber = dto.CollectorNumber,
            ImageUrlNormal = imgNormal,
            ImageUrlArtCrop = imgArtCrop
        };
    }
}
