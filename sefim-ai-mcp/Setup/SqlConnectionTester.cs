using Microsoft.Data.SqlClient;

namespace SefimMcp.Setup;

public sealed record SqlTestResult(bool Success, string Message);

public static class SqlConnectionTester
{
    public static async Task<SqlTestResult> TestAsync(string server, string database, string? userId, string? password, CancellationToken ct)
    {
        var auth = string.IsNullOrWhiteSpace(userId) && string.IsNullOrWhiteSpace(password)
            ? "Integrated Security=True;"
            : $"User Id={userId};Password={password};";
        var connectionString = $"Server={server};Database={database};{auth}" +
                                "TrustServerCertificate=True;Encrypt=True;Connection Timeout=8;Application Name=SefimMcpSetup;";

        await using var connection = new SqlConnection(connectionString);
        try
        {
            await connection.OpenAsync(ct);
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT DB_NAME()";
            var name = (string?)await command.ExecuteScalarAsync(ct) ?? database;
            return new SqlTestResult(true, $"{server} üzerindeki {name} veritabanına bağlanıldı.");
        }
        catch (Exception ex)
        {
            return new SqlTestResult(false, ex.Message);
        }
    }
}
