using SefimMcp.Setup;
using Xunit;

namespace SefimMcp.Tests;

public class SetupPathsTests
{
    [Fact]
    public void InstallRoot_IsUnderUserProfile_NotProgramFiles()
    {
        var root = SetupPaths.InstallRoot("SEFIM-MCP", "0.1.0-beta");

        Assert.DoesNotContain("Program Files", root);
        Assert.Contains("0.1.0-beta", root);
    }

    [Fact]
    public void ProImagesCandidates_IncludesMntProimages_OnNonWindows()
    {
        var candidates = SetupPaths.ProImagesCandidates(sefimDir: null);

        if (!OperatingSystem.IsWindows())
            Assert.Contains("/mnt/proimages", candidates);
    }

    [Fact]
    public void SefimDirCandidates_IncludesVegaSefim_OnWindows()
    {
        var candidates = SetupPaths.SefimDirCandidates();

        if (OperatingSystem.IsWindows())
            Assert.Contains(candidates, c => c.Contains("Vega") && c.Contains("Sefim"));
    }
}
