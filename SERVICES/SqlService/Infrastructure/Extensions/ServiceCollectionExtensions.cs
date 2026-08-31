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
        var section = configuration.GetSection(sectionName);
        if (!section.Exists())
            throw new InvalidOperationException($"Configuration section '{sectionName}' not found or invalid.");

        var settings = ReadSettings(section);

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

    /// <summary>
    /// Configuration section'ı elle okur.
    /// </summary>
    /// <remarks>
    /// ConfigurationBinder.Get&lt;T&gt; yerine elle okuma: binder reflection kullanır,
    /// RequiresUnreferencedCode/RequiresDynamicCode ile işaretlidir ve trimlenmiş AOT
    /// derlemesinde IL2026/IL3050 uyarısı üretip sessizce varsayılan değer döndürebilir.
    /// </remarks>
    private static SqlConnectionSettings ReadSettings(IConfigurationSection section)
    {
        var settings = new SqlConnectionSettings();

        settings.ConnectionString = section["ConnectionString"] ?? settings.ConnectionString;
        settings.CommandTimeout = ReadInt32(section, "CommandTimeout", settings.CommandTimeout);
        settings.BatchInsertSize = ReadInt32(section, "BatchInsertSize", settings.BatchInsertSize);
        settings.BatchUpdateSize = ReadInt32(section, "BatchUpdateSize", settings.BatchUpdateSize);
        settings.BatchDeleteSize = ReadInt32(section, "BatchDeleteSize", settings.BatchDeleteSize);
        settings.EnableConnectionPooling = ReadBoolean(section, "EnableConnectionPooling", settings.EnableConnectionPooling);
        settings.MaxPoolSize = ReadInt32(section, "MaxPoolSize", settings.MaxPoolSize);
        settings.MinPoolSize = ReadInt32(section, "MinPoolSize", settings.MinPoolSize);
        settings.EnableLogging = ReadBoolean(section, "EnableLogging", settings.EnableLogging);

        return settings;
    }

    private static int ReadInt32(IConfigurationSection section, string key, int fallback)
    {
        var value = section[key];
        if (string.IsNullOrWhiteSpace(value))
            return fallback;

        if (!int.TryParse(value, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var parsed))
            throw new InvalidOperationException($"Configuration value '{section.Path}:{key}' is not a valid integer: {value}");

        return parsed;
    }

    private static bool ReadBoolean(IConfigurationSection section, string key, bool fallback)
    {
        var value = section[key];
        if (string.IsNullOrWhiteSpace(value))
            return fallback;

        if (!bool.TryParse(value, out var parsed))
            throw new InvalidOperationException($"Configuration value '{section.Path}:{key}' is not a valid boolean: {value}");

        return parsed;
    }
}
