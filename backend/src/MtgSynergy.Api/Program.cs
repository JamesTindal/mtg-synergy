using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MtgSynergy.Core.Models;
using MtgSynergy.Core.Rules;
using MtgSynergy.Core.Services;
using MtgSynergy.Infrastructure.Data;
using MtgSynergy.Infrastructure.Scryfall;

var builder = WebApplication.CreateBuilder(args);

// Database configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// Scryfall HTTP Client & Ingestion Service
builder.Services.AddHttpClient<ScryfallClient>();
builder.Services.AddScoped<ScryfallIngestionService>();

// Extensible Synergy Rules Pipeline
builder.Services.AddScoped<ISynergyRule, ColorSynergyRule>();
builder.Services.AddScoped<ISynergyRule, TribalSynergyRule>();
builder.Services.AddScoped<ISynergyRule, CardTypeSynergyRule>();
builder.Services.AddScoped<SynergyEngine>();

// CORS policy for React Vite dev server
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowViteDev", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000", "http://127.0.0.1:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors("AllowViteDev");

// Auto-create database tables on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync();
}

// ----------------- Endpoints -----------------

// Health Check
app.MapGet("/api/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

// Search cards for autocomplete bar
app.MapGet("/api/cards/search", async (
    [FromQuery] string? q,
    [FromQuery] string? set,
    [FromQuery] int? limit,
    AppDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(q))
        return Results.Ok(Array.Empty<object>());

    int maxResults = limit.HasValue && limit.Value > 0 ? Math.Min(limit.Value, 50) : 10;
    string query = q.Trim().ToLower();

    var queryable = db.Cards.AsNoTracking();

    if (!string.IsNullOrWhiteSpace(set))
    {
        queryable = queryable.Where(c => c.SetCode == set.ToLower());
    }

    var results = await queryable
        .Where(c => EF.Functions.ILike(c.Name, $"%{query}%"))
        .OrderBy(c => c.Name.ToLower().StartsWith(query) ? 0 : 1)
        .ThenBy(c => c.Name)
        .Take(maxResults)
        .Select(c => new
        {
            c.Id,
            c.ScryfallId,
            c.Name,
            c.ManaCost,
            c.TypeLine,
            c.ImageUrlNormal,
            c.ImageUrlArtCrop,
            c.Colors,
            c.Rarity
        })
        .ToListAsync();

    return Results.Ok(results);
});

// Get specific card by ID
app.MapGet("/api/cards/{id:guid}", async (Guid id, AppDbContext db) =>
{
    var card = await db.Cards.AsNoTracking()
        .FirstOrDefaultAsync(c => c.Id == id || c.ScryfallId == id);

    return card != null ? Results.Ok(card) : Results.NotFound();
});

// Calculate and retrieve synergies for a specific card
app.MapGet("/api/cards/{id:guid}/synergies", async (
    Guid id,
    [FromQuery] double? minScore,
    [FromQuery] int? limit,
    AppDbContext db,
    SynergyEngine engine) =>
{
    var sourceCard = await db.Cards.AsNoTracking()
        .FirstOrDefaultAsync(c => c.Id == id || c.ScryfallId == id);

    if (sourceCard == null)
        return Results.NotFound(new { message = $"Card with id {id} not found." });

    // Fetch potential candidates (same set for now)
    var candidates = await db.Cards.AsNoTracking()
        .Where(c => c.SetCode == sourceCard.SetCode && c.Id != sourceCard.Id)
        .ToListAsync();

    double threshold = minScore.HasValue && minScore.Value > 0 ? minScore.Value : 20.0;
    int maxLimit = limit.HasValue && limit.Value > 0 ? Math.Min(limit.Value, 50) : 30;

    var synergies = engine.FindSynergies(sourceCard, candidates, threshold, maxLimit);

    return Results.Ok(new
    {
        sourceCard = new
        {
            sourceCard.Id,
            sourceCard.Name,
            sourceCard.ManaCost,
            sourceCard.TypeLine,
            sourceCard.OracleText,
            sourceCard.ImageUrlNormal,
            sourceCard.ImageUrlArtCrop,
            sourceCard.Colors,
            sourceCard.Power,
            sourceCard.Toughness
        },
        totalMatches = synergies.Count,
        synergies
    });
});

// Admin endpoint: Sync set cards from Scryfall
app.MapPost("/api/admin/sync/{setCode}", async (
    string setCode,
    ScryfallIngestionService ingestor) =>
{
    try
    {
        int count = await ingestor.IngestSetAsync(setCode);
        return Results.Ok(new { message = $"Successfully synced {count} cards for set '{setCode}'.", count });
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, title: "Scryfall Ingestion Failed");
    }
});

app.Run();
