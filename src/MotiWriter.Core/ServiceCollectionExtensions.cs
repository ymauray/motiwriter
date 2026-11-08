using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MotiWriter.Core.Dashboard;

namespace MotiWriter.Core;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Enregistre les services métier. L'implémentation de <see cref="Journal.IWritingJournal"/>
    /// est fournie par l'hôte (Web, API REST, MCP…).
    /// </summary>
    public static IServiceCollection AddMotiWriterCore(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<IDashboardService, DashboardService>();
        return services;
    }
}
