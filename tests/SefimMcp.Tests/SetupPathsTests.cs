using SefimMcp.Setup;
using Xunit;

namespace SefimMcp.Tests;

public class SetupPathsTests
{
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
