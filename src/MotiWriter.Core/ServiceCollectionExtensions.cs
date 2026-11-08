using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MotiWriter.Core.Accounts;
using MotiWriter.Core.Dashboard;
using MotiWriter.Core.Journal;

namespace MotiWriter.Core;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Enregistre les services métier. Les implémentations de <see cref="Journal.IWritingJournal"/>
    /// et de <see cref="Accounts.IUserAccountStore"/> sont fournies par l'hôte (Web, API REST, MCP…).
    /// </summary>
    public static IServiceCollection AddMotiWriterCore(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IWritingDayService, WritingDayService>();
        services.AddScoped<IAccountService, AccountService>();
        return services;
    }
}
