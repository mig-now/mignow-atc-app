using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Ports.Execution;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Application.Ports.Secrets;
using Mignow.Atc.Infra.Database.Context;
using Mignow.Atc.Infra.Database.Queue;
using Mignow.Atc.Infra.Database.Secrets;
using Mignow.Atc.Infra.Database.Seeding;

namespace Mignow.Atc.Infra.Database;

public static class DatabaseServiceCollectionExtensions
{
    public static IServiceCollection AddDatabaseInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<DatabaseOptions>().Bind(configuration.GetSection(DatabaseOptions.Section));
        services.AddOptions<SecretOptions>().Bind(configuration.GetSection(SecretOptions.Section));
        services.AddScoped<AuditSaveChangesInterceptor>();
        services.AddDbContext<AtcDbContext>((sp, o) =>
        {
            var cs = configuration.GetConnectionString("Default");
            if (string.IsNullOrWhiteSpace(cs) || cs.Contains("CHANGE_ME", StringComparison.Ordinal))
                throw new InvalidOperationException("ConnectionStrings:Default não configurada (user-secrets em Dev, variável de ambiente nos demais).");
            o.UseNpgsql(cs, npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", AtcDbContext.Schema));
            o.AddInterceptors(sp.GetRequiredService<AuditSaveChangesInterceptor>());
        });
        services.AddScoped<EfDataStore>();
        services.AddScoped<IDataStore>(sp => sp.GetRequiredService<EfDataStore>());
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<EfDataStore>());
        services.AddScoped<IRunQueue, PostgresRunQueue>();
        services.AddScoped<ISecretStore, EncryptedSecretStore>();
        services.AddScoped<DatabaseMigrator>();
        services.AddScoped<DatabaseSeeder>();
        return services;
    }
}
