using SefimMcp.Setup;
using Xunit;

namespace SefimMcp.Tests;

public class SetupCommandRunnerTests
{
    [Fact]
    public async Task TryRunAsync_ReturnsFalse_WhenFirstArgIsNotSetup()
    {
        var handled = await SetupCommandRunner.TryRunAsync(new[] { "knowledge", "stats" }, CancellationToken.None);
        Assert.False(handled);
    }

    [Fact]
    public async Task TryRunAsync_Configure_WritesAppSettingsAndHostConfig()
    {
        var dir = Directory.CreateTempSubdirectory("sefim-setup-configure");
        try
        {
            var exeName = OperatingSystem.IsWindows() ? "sefim-ai-mcp.exe" : "sefim-ai-mcp";
            File.WriteAllText(Path.Combine(dir.FullName, exeName), "stub");
            File.WriteAllText(Path.Combine(dir.FullName, "appsettings.json"), """{ "SqlService": {}, "SEFIM": {} }""");
            var hostConfig = Path.Combine(dir.FullName, "host.json");

            var args = new[]
            {
                "setup", "configure",
                "--install-dir", dir.FullName,
                "--host-config", hostConfig,
                "--server", "SQLHOST",
                "--database", "SefimDb",
                "--pro-images", dir.FullName,
                "--tool-profile", "full",
                "--server-key", "sefim",
            };

            var handled = await SetupCommandRunner.TryRunAsync(args, CancellationToken.None);

            Assert.True(handled);
            Assert.Contains("Server=SQLHOST", File.ReadAllText(Path.Combine(dir.FullName, "appsettings.json")));
            Assert.Contains("\"sefim\"", File.ReadAllText(hostConfig));
        }
        finally
        {
            dir.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task TryRunAsync_Configure_MissingServer_FailsWithNonZeroExitCode()
    {
        var args = new[] { "setup", "configure", "--install-dir", Path.GetTempPath(), "--host-config", Path.GetTempFileName() };

        await SetupCommandRunner.TryRunAsync(args, CancellationToken.None);

        Assert.NotEqual(0, Environment.ExitCode);
        Environment.ExitCode = 0; // reset for subsequent tests in the same process
    }
}
