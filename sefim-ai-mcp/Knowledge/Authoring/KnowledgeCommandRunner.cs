using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SefimMcp.Infrastructure.Metadata;
using SefimMcp.Knowledge.Models;
using SefimMcp.Knowledge.Runtime;
using SefimMcp.Knowledge.Security;
using SqlService.Infrastructure.Extensions;

namespace SefimMcp.Knowledge.Authoring;

public static class KnowledgeCommandRunner
{
    public static async Task<bool> TryRunAsync(string[] args, IConfiguration configuration, CancellationToken cancellationToken)
    {
        if (args.Length < 2 || !args[0].Equals("knowledge", StringComparison.OrdinalIgnoreCase))
            return false;

        var sourceDirectory = Path.Combine(Directory.GetCurrentDirectory(), "knowledge", "private");
        switch (args[1].ToLowerInvariant())
        {
            case "validate":
                var validation = KnowledgeDocumentParser.ValidateDirectory(sourceDirectory);
                WriteValidation(validation);
                Environment.ExitCode = validation.IsValid ? 0 : 1;
                return true;
            case "pack":
                await PackAsync(sourceDirectory, GetOption(args, "--output") ?? Path.Combine(Directory.GetCurrentDirectory(), "knowledge.pack"), cancellationToken);
                return true;
            case "stats":
                WriteStats(sourceDirectory);
                return true;
            case "generate-table-doc" when args.Length >= 3:
                await GenerateTableDocumentAsync(args[2], sourceDirectory, configuration, cancellationToken);
                return true;
            default:
                Console.Error.WriteLine("Usage: knowledge validate | knowledge stats | knowledge pack [--output path] | knowledge generate-table-doc schema.table");
                Environment.ExitCode = 2;
                return true;
        }
    }

    private static async Task PackAsync(string sourceDirectory, string outputPath, CancellationToken cancellationToken)
    {
        var validation = KnowledgeDocumentParser.ValidateDirectory(sourceDirectory);
        if (!validation.IsValid)
        {
            WriteValidation(validation);
            Environment.ExitCode = 1;
            return;
        }

        var documents = ReadDocuments(sourceDirectory);
        if (documents.Length == 0)
        {
            Console.Error.WriteLine($"No knowledge documents were found under {sourceDirectory}. Refusing to write an empty pack.");
            Environment.ExitCode = 1;
            return;
        }

        // Falls back to the key compiled into the server, so a pack produced here always
        // opens in a shipped build. SEFIM_KNOWLEDGE_KEY still overrides it when set.
        var provider = new EmbeddedKnowledgeKeyProvider();
        if (!provider.TryGetKey(out var key))
        {
            Console.Error.WriteLine("SEFIM_KNOWLEDGE_KEY must contain a base64-encoded 32-byte key before packing.");
            Environment.ExitCode = 1;
            return;
        }

        try
        {
            var pack = new KnowledgePack(documents, DateTimeOffset.UtcNow, SourceFingerprint: Fingerprint(documents));
            await File.WriteAllBytesAsync(outputPath, KnowledgePackCodec.Encrypt(pack, key), cancellationToken);
            Console.WriteLine($"Packed {documents.Length} document(s) to {outputPath}.");
            Console.WriteLine($"Fingerprint: {pack.SourceFingerprint}");
        }
        finally { System.Security.Cryptography.CryptographicOperations.ZeroMemory(key); }
    }

    private static KnowledgeDocument[] ReadDocuments(string sourceDirectory) =>
        Directory.Exists(sourceDirectory)
            ? Directory.EnumerateFiles(sourceDirectory, "*.md", SearchOption.AllDirectories)
                .OrderBy(file => file, StringComparer.Ordinal)
                .Select(file => KnowledgeDocumentParser.TryParse(Path.GetRelativePath(sourceDirectory, file), File.ReadAllText(file), out var document, out _) ? document : null)
                .Where(document => document is not null)
                .Select(document => document!)
                .ToArray()
            : [];

    /// <summary>
    /// Content hash of the packed documents so a running server can be matched to the sources it was built from.
    /// It carries no secret: it is derived from text the operator already holds.
    /// </summary>
    private static string Fingerprint(IReadOnlyList<KnowledgeDocument> documents)
    {
        var builder = new System.Text.StringBuilder();
        foreach (var document in documents)
            builder.Append(document.Kind).Append('|').Append(document.Id).Append('|').Append(document.Body).Append('\n');

        return Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(builder.ToString())))[..16];
    }

    private static void WriteStats(string sourceDirectory)
    {
        var documents = ReadDocuments(sourceDirectory);
        if (documents.Length == 0)
        {
            Console.Error.WriteLine($"No knowledge documents were found under {sourceDirectory}.");
            Environment.ExitCode = 1;
            return;
        }

        Console.WriteLine($"Source: {sourceDirectory}");
        Console.WriteLine($"Documents: {documents.Length}");
        Console.WriteLine($"Fingerprint: {Fingerprint(documents)}");
        foreach (var group in documents.GroupBy(document => document.Kind).OrderBy(group => group.Key, StringComparer.Ordinal))
        {
            var sections = group.Sum(document => KnowledgeChunker.Split(document).Count);
            Console.WriteLine($"  {group.Key}: {group.Count()} document(s), {sections} searchable section(s)");
            foreach (var document in group.OrderBy(document => document.Id, StringComparer.Ordinal))
            {
                var missingSummary = string.IsNullOrWhiteSpace(document.Summary) ? "  [no summary]" : string.Empty;
                var missingAliases = document.Aliases is null || document.Aliases.Count == 0 ? "  [no aliases]" : string.Empty;
                Console.WriteLine($"    {document.Id} ({document.Status}, {document.Exposure}){missingSummary}{missingAliases}");
            }
        }
    }

    private static async Task GenerateTableDocumentAsync(string qualifiedTable, string sourceDirectory, IConfiguration configuration, CancellationToken cancellationToken)
    {
        var parts = qualifiedTable.Split('.', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2)
        {
            Console.Error.WriteLine("Table must be in schema.table form.");
            Environment.ExitCode = 2;
            return;
        }

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSqlService(configuration);
        services.AddSingleton<IDatabaseMetadataService, SqlServerDatabaseMetadataService>();
        await using var provider = services.BuildServiceProvider();
        var metadata = await provider.GetRequiredService<IDatabaseMetadataService>().DescribeTableAsync(parts[0], parts[1], cancellationToken);
        if (metadata is null)
        {
            Console.Error.WriteLine("Table was not found.");
            Environment.ExitCode = 1;
            return;
        }

        var destination = Path.Combine(sourceDirectory, "tables", $"{metadata.Schema}.{metadata.Name}.md");
        Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
        if (File.Exists(destination))
        {
            Console.Error.WriteLine($"Refusing to overwrite existing document: {destination}");
            Environment.ExitCode = 1;
            return;
        }

        var lines = new List<string>
        {
            "---", "kind: table", $"id: {metadata.Schema}.{metadata.Name}", "status: draft", "exposure: model", "---", "",
            $"# {metadata.Schema}.{metadata.Name}", "", "## Purpose", "", "<!-- USER: Bu tablonun Şefim uygulamasındaki business amacını yaz. -->", "",
            "## One Row Represents", "", "<!-- USER: Her satırın business olarak neyi temsil ettiğini yaz. -->", "",
            "## Primary Identifier", "", "<!-- USER: Business açıdan kaydın nasıl tanındığını yaz. -->", "",
            "## Read Guidance", "", "<!-- USER: AI bu tablodan bilgi okurken dikkat etmesi gereken business kuralları yaz. -->", "",
            "## Write Guidance", "", "<!-- USER: AI bu tabloyla işlem yapmadan önce gerekli business kontrollerini yaz. -->", "",
            "## Important Relations", "", "<!-- USER: SQL ilişkilerinin business anlamlarını yaz. -->", "",
            "## Business Rules", "", "<!-- USER: Bu tablo için önemli business kurallarını yaz. -->", "",
            "## Common Operations", "", "<!-- USER: Sık yapılan business işlemlerini yaz. -->", "",
            "## Warnings", "", "<!-- USER: Riskli veya yanlış yapılmaması gereken işlemleri yaz. -->", "",
            "## Columns", ""
        };
        foreach (var column in metadata.Columns)
        {
            lines.AddRange([ $"### {column.Name}", "", "<!-- USER: Bu kolonun Şefim'deki business anlamını yaz. -->", "", "#### Create davranışı", "", "<!-- USER: Yeni kayıtta zorunlu mu, sistem mi üretir, kullanıcı mı girer? -->", "", "#### Update davranışı", "", "<!-- USER: Sonradan değiştirilebilir mi? -->", "", "#### Özel kurallar", "", "<!-- USER: Varsa business kurallarını yaz. -->", "" ]);
        }
        await File.WriteAllLinesAsync(destination, lines, cancellationToken);
        Console.WriteLine($"Created {destination}.");
    }

    private static string? GetOption(IReadOnlyList<string> args, string option) => args.SkipWhile(a => !a.Equals(option, StringComparison.OrdinalIgnoreCase)).Skip(1).FirstOrDefault();
    private static void WriteValidation(KnowledgeValidationResult result)
    {
        if (result.IsValid) { Console.WriteLine("Knowledge validation: OK"); return; }
        foreach (var issue in result.Issues) Console.Error.WriteLine($"{issue.SourceName}: {issue.Code}: {issue.Message}");
    }
}
