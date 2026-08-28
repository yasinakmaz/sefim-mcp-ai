using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SqlService.Core.Interfaces;
using SqlService.Core.Models;
using SqlService.Core.Sprocs;
using SqlService.Infrastructure.Builders;
using SqlService.Infrastructure.Factories;
using SqlService.Infrastructure.Mappers;
using SqlService.Infrastructure.Sprocs;

namespace SqlService.Infrastructure.Extensions;

/// <summary>
/// Dependency Injection extension methods
/// AOT-Compatible: No reflection in registration
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// SqlService'i DI container'a kaydeder (Single registration)
    /// </summary>
    /// <param name="services">IServiceCollection</param>
    /// <param name="configuration">IConfiguration</param>
    /// <param name="sectionName">Configuration section name (varsayılan: "SqlService")</param>
    /// <returns>IServiceCollection</returns>
    public static IServiceCollection AddSqlService(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = "SqlService")
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // SqlConnectionSettings'i configuration'dan oku
        var settings = configuration
            .GetSection(sectionName)
            .Get<SqlConnectionSettings>()
            ?? throw new InvalidOperationException($"Configuration section '{sectionName}' not found or invalid.");

        // Secrets must be provisioned outside source-controlled JSON. This explicit
        // variable also works when the app is launched by an MCP host.
        var environmentConnectionString = Environment.GetEnvironmentVariable("SEFIM_SQL_CONNECTION_STRING");
        if (!string.IsNullOrWhiteSpace(environmentConnectionString))
            settings.ConnectionString = environmentConnectionString;

        // Validate settings
        settings.Validate();

        // Register settings as singleton
        services.TryAddSingleton(settings);

        // Register factories
        services.TryAddSingleton<IConnectionFactory, SqlConnectionFactory>();

        // Register mappers (FIRST - required by builders)
        services.TryAddSingleton<IEntityMapper, EntityMapper>();

        // Register builders (depend on EntityMapper)
        services.TryAddSingleton<ISqlCommandBuilder, SqlCommandBuilder>();
        services.TryAddSingleton<IBatchOperation, BatchOperationBuilder>();

        // Register generic ISqlService<T>
        services.TryAddSingleton(typeof(ISqlService<>), typeof(Services.SqlService<>));

        // Register ISprocService for stored procedures
        services.TryAddSingleton<ISprocService, SprocService>();

        return services;
    }

    /// <summary>
    /// SqlService'i manuel settings ile DI container'a kaydeder
    /// </summary>
    /// <param name="services">IServiceCollection</param>
    /// <param name="settings">SqlConnectionSettings</param>
    /// <returns>IServiceCollection</returns>
    public static IServiceCollection AddSqlService(
        this IServiceCollection services,
        SqlConnectionSettings settings)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(settings);

        // Validate settings
        settings.Validate();

        // Register settings as singleton
        services.TryAddSingleton(settings);

        // Register factories
        services.TryAddSingleton<IConnectionFactory, SqlConnectionFactory>();

        // Register mappers (FIRST - required by builders)
        services.TryAddSingleton<IEntityMapper, EntityMapper>();

        // Register builders (depend on EntityMapper)
        services.TryAddSingleton<ISqlCommandBuilder, SqlCommandBuilder>();
        services.TryAddSingleton<IBatchOperation, BatchOperationBuilder>();

        // Register generic ISqlService<T>
        services.TryAddSingleton(typeof(ISqlService<>), typeof(Services.SqlService<>));

        // Register ISprocService for stored procedures
        services.TryAddSingleton<ISprocService, SprocService>();

        return services;
    }

    /// <summary>
    /// SqlService'i connection string ile DI container'a kaydeder
    /// </summary>
    /// <param name="services">IServiceCollection</param>
    /// <param name="connectionString">SQL Server connection string</param>
    /// <param name="configureSettings">Settings yapılandırma (opsiyonel)</param>
    /// <returns>IServiceCollection</returns>
    public static IServiceCollection AddSqlService(
        this IServiceCollection services,
        string connectionString,
        Action<SqlConnectionSettings>? configureSettings = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        var settings = new SqlConnectionSettings
        {
            ConnectionString = connectionString
        };

        // Apply custom configuration
        configureSettings?.Invoke(settings);

        return AddSqlService(services, settings);
    }
}
