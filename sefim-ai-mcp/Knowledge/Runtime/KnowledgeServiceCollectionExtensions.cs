using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SefimMcp.Knowledge.Models;
using SefimMcp.Knowledge.Security;

namespace SefimMcp.Knowledge.Runtime;

public static class KnowledgeServiceCollectionExtensions
{
    public static IServiceCollection AddKnowledgeSystem(this IServiceCollection services, IConfiguration configuration)
    {
        var packPath = Path.Combine(AppContext.BaseDirectory, "knowledge.pack");
        var keyVariable = configuration["Knowledge:KeyEnvironmentVariable"] ?? "SEFIM_KNOWLEDGE_KEY";
        services.AddSingleton(new KnowledgePackOptions(packPath, keyVariable));
        services.AddSingleton<IKnowledgeKeyProvider>(_ => new EmbeddedKnowledgeKeyProvider(keyVariable));
        services.AddSingleton<IKnowledgeService, KnowledgeService>();
        return services;
    }
}
