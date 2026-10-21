using Microsoft.Extensions.DependencyInjection;
using MtgSynergy.Application.Interfaces.Services;
using MtgSynergy.Application.Services;
using MtgSynergy.Domain.Rules;

namespace MtgSynergy.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Domain Synergy Rules
        services.AddScoped<ISynergyRule, ColorSynergyRule>();
        services.AddScoped<ISynergyRule, TribalSynergyRule>();
        services.AddScoped<ISynergyRule, CardTypeSynergyRule>();

        // Application Services
        services.AddScoped<ISynergyEngine, SynergyEngine>();
        services.AddScoped<ICardSynergyService, CardSynergyService>();

        return services;
    }
}
