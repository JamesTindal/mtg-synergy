using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MtgSynergy.Application.Interfaces.Repositories;
using MtgSynergy.Application.Interfaces.Services;
using MtgSynergy.Infrastructure.Data;
using MtgSynergy.Infrastructure.Persistence.Repositories;
using MtgSynergy.Infrastructure.Scryfall;

namespace MtgSynergy.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<ICardRepository, CardRepository>();

        services.AddHttpClient<IScryfallClient, ScryfallClient>();
        services.AddScoped<ISetIngestionService, ScryfallIngestionService>();

        return services;
    }
}
