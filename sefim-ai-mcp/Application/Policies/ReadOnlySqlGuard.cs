using System.Text.RegularExpressions;

namespace SefimMcp.Application.Policies;

/// <summary>
/// Single defence-in-depth gate for every ad-hoc or catalog SQL statement the server is willing to run.
/// It is not a substitute for a read-only SQL login; the deployment account stays the real boundary.
/// </summary>
public static partial class ReadOnlySqlGuard
{
    public static bool IsReadOnlySelect(string? query) => Validate(query) is null;

    /// <summary>Returns null when the statement is acceptable, otherwise the reason it was rejected.</summary>
    public static string? Validate(string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return "Query is empty.";

        var stripped = StripStrings(StripComments(query)).Trim();
        if (stripped.Length == 0)
            return "Query contains no executable statement.";

        if (!stripped.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase) &&
            !stripped.StartsWith("WITH", StringComparison.OrdinalIgnoreCase))
            return "Only a single SELECT or CTE SELECT statement is allowed.";

        if (stripped.TrimEnd().TrimEnd(';').Contains(';', StringComparison.Ordinal))
            return "Multiple statements are not allowed.";

        var forbidden = ForbiddenStatementRegex().Match(stripped);
        if (forbidden.Success)
            return $"Token '{forbidden.Value}' is not allowed in a read-only query.";

        var restricted = RestrictedObjectRegex().Match(stripped);
        if (restricted.Success)
            return $"Access to '{restricted.Value}' is not allowed.";

        var sensitive = SensitiveIdentifierRegex().Match(stripped);
        if (sensitive.Success)
            return $"Identifier '{sensitive.Value}' is credential-related and cannot be read.";

        return null;
    }

    private static string StripComments(string query)
    {
        var withoutBlocks = BlockCommentRegex().Replace(query, " ");
        return LineCommentRegex().Replace(withoutBlocks, " ");
    }

    /// <summary>Blanks out string literals so that data never trips the keyword scanner, and vice versa.</summary>
    private static string StripStrings(string query) => StringLiteralRegex().Replace(query, "''");

    [GeneratedRegex(@"/\*.*?\*/", RegexOptions.Singleline)]
    private static partial Regex BlockCommentRegex();

    [GeneratedRegex(@"--[^\n]*")]
    private static partial Regex LineCommentRegex();

    [GeneratedRegex(@"'(?:[^']|'')*'")]
    private static partial Regex StringLiteralRegex();

    [GeneratedRegex(
        @"\b(INSERT|UPDATE|DELETE|MERGE|EXEC|EXECUTE|CREATE|ALTER|DROP|TRUNCATE|GRANT|REVOKE|DENY|INTO|BACKUP|RESTORE|RECONFIGURE|SHUTDOWN|WAITFOR|OPENROWSET|OPENQUERY|OPENDATASOURCE|OPENXML|BULK)\b",
        RegexOptions.IgnoreCase)]
    private static partial Regex ForbiddenStatementRegex();

    /// <summary>
    /// Server catalog, cross-database and extended-procedure surfaces stay closed: a report never needs them and
    /// they are the usual path from "read-only SELECT" to credential or configuration disclosure.
    /// </summary>
    [GeneratedRegex(
        @"\b(sys\.|sysobjects|syscolumns|syslogins|sysusers|INFORMATION_SCHEMA\.|master\.|msdb\.|tempdb\.|model\.|xp_[a-z_]+|sp_[a-z_]+)",
        RegexOptions.IgnoreCase)]
    private static partial Regex RestrictedObjectRegex();

    /// <summary>
    /// Credential-shaped identifiers stay unreadable even inside an otherwise valid SELECT.
    /// String literals are already blanked before this runs, so ordinary data cannot trip it.
    /// </summary>
    [GeneratedRegex(
        @"\b\w*(PASSWORD|PASSWD|SECRET|APIKEY|PRIVATEKEY|CONNECTIONSTRING|CREDENTIAL|TOKEN)\w*\b",
        RegexOptions.IgnoreCase)]
    private static partial Regex SensitiveIdentifierRegex();
}
