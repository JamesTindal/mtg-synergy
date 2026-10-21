using Microsoft.AspNetCore.Mvc;
using MtgSynergy.Application.Interfaces.Services;
using MtgSynergy.Domain.Exceptions;

namespace MtgSynergy.Api.Endpoints;

public static class CardEndpoints
{
    public static IEndpointRouteBuilder MapCardEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/cards")
            .WithTags("Cards");

        // Autocomplete search
        group.MapGet("/search", async (
            [FromQuery] string? q,
            [FromQuery] string? set,
            [FromQuery] int? limit,
            ICardSynergyService cardService,
            CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(q))
                return Results.Ok(Array.Empty<object>());

            var results = await cardService.SearchCardsAsync(q, set, limit ?? 10, ct);
            return Results.Ok(results);
        });

        // Get single card by ID
        group.MapGet("/{id:guid}", async (
            Guid id,
            ICardSynergyService cardService,
            CancellationToken ct) =>
        {
            var card = await cardService.GetCardByIdAsync(id, ct);
            return card != null ? Results.Ok(card) : Results.NotFound();
        });

        // Get card synergies
        group.MapGet("/{id:guid}/synergies", async (
            Guid id,
            [FromQuery] double? minScore,
            [FromQuery] int? limit,
            ICardSynergyService cardService,
            CancellationToken ct) =>
        {
            try
            {
                var response = await cardService.GetCardSynergiesAsync(id, minScore ?? 20.0, limit ?? 30, ct);
                return Results.Ok(response);
            }
            catch (CardNotFoundException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
        });

        return routes;
    }
}
