using SefimMcp.Application.Policies;
using Xunit;

namespace SefimMcp.Tests;

public sealed class ReadOnlySqlGuardTests
{
    [Theory]
    [InlineData("SELECT Id, Name FROM dbo.Product WHERE Id = @Id")]
    [InlineData("WITH Sales AS (SELECT ProductId FROM dbo.Bill) SELECT * FROM Sales")]
    [InlineData("SELECT Name FROM dbo.Product WHERE Name = 'DROP TABLE x'")]
    public void Accepts_read_only_statements(string query) =>
        Assert.Null(ReadOnlySqlGuard.Validate(query));

    [Theory]
    [InlineData("DELETE FROM dbo.Product")]
    [InlineData("SELECT 1; DROP TABLE dbo.Product")]
    [InlineData("SELECT * INTO dbo.Copy FROM dbo.Product")]
    [InlineData("SELECT * FROM OPENROWSET('SQLNCLI', 'x', 'SELECT 1')")]
    [InlineData("SELECT name FROM sys.tables")]
    [InlineData("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES")]
    [InlineData("SELECT * FROM master.dbo.sysdatabases")]
    [InlineData("EXEC sp_who")]
    [InlineData("SELECT UserName, Password FROM dbo.[User]")]
    [InlineData("")]
    [InlineData("   ")]
    public void Rejects_unsafe_statements(string query) =>
        Assert.NotNull(ReadOnlySqlGuard.Validate(query));

    [Fact]
    public void Rejects_statements_hidden_behind_comments()
    {
        // The old space-padded blocklist missed this: the comment breaks the keyword padding.
        Assert.NotNull(ReadOnlySqlGuard.Validate("SELECT 1 FROM dbo.Product/**/WHERE 1=1; DROP TABLE dbo.Product"));
        Assert.NotNull(ReadOnlySqlGuard.Validate("--x\nDELETE FROM dbo.Product"));
    }

    [Fact]
    public void Rejects_statement_that_only_looks_like_a_select()
    {
        Assert.NotNull(ReadOnlySqlGuard.Validate("/* SELECT */ UPDATE dbo.Product SET Price = 0"));
    }
}
