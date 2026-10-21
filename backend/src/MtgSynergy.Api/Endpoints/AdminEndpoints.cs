using Microsoft.AspNetCore.Mvc;
using MtgSynergy.Application.Interfaces.Services;

namespace MtgSynergy.Api.Endpoints;

public static class AdminEndpoints
{
    public static IEndpointRouteBuilder MapAdminEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/admin")
            .WithTags("Admin");

        // Sync card set from Scryfall
        group.MapPost("/sync/{setCode}", async (
            string setCode,
            ISetIngestionService ingestionService,
            CancellationToken ct) =>
        {
            try
            {
                int count = await ingestionService.IngestSetAsync(setCode, ct);
                return Results.Ok(new
                {
                    message = $"Successfully synced {count} cards for set '{setCode}'.",
                    count
                });
            }
            catch (Exception ex)
            {
                return Results.Problem(detail: ex.Message, title: "Scryfall Ingestion Failed");
            }
        });

        return routes;
    }
}
