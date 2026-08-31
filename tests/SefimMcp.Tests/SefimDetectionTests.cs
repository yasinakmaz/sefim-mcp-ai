using SefimMcp.Setup;
using Xunit;

namespace SefimMcp.Tests;

public class SefimDetectionTests
{
    [Fact]
    public void BuildConnectionString_WithCredentials_IncludesUserAndPassword()
    {
        var cs = SefimDetection.BuildConnectionString("SQLHOST", "SefimDb", "sa", "P@ss");

        Assert.Contains("User Id=sa;", cs);
        Assert.Contains("Password=P@ss;", cs);
        Assert.DoesNotContain("Integrated Security", cs);
    }

    [Fact]
    public void BuildConnectionString_WithoutCredentials_UsesIntegratedSecurity()
    {
        var cs = SefimDetection.BuildConnectionString("SQLHOST", "SefimDb", null, null);

        Assert.Contains("Integrated Security=True;", cs);
        Assert.DoesNotContain("User Id=", cs);
        Assert.DoesNotContain("Password=", cs);
    }

    [Fact]
    public void ReadConnectionString_ParsesKnownFields()
    {
        var dir = Directory.CreateTempSubdirectory("sefim-detect-test");
        try
        {
            File.WriteAllText(Path.Combine(dir.FullName, "connectionstring.txt"),
                "Data Source=SQLHOST;Initial Catalog=SefimDb;User ID=sa;Password=P@ss;");

            var fields = SefimDetection.ReadConnectionString(dir.FullName);

            Assert.Equal("SQLHOST", fields.Server);
            Assert.Equal("SefimDb", fields.Database);
            Assert.Equal("sa", fields.UserId);
            Assert.Equal("P@ss", fields.Password);
        }
        finally
        {
            dir.Delete(recursive: true);
        }
    }

    [Fact]
    public void FindProImages_ReturnsDirectDirectory_WhenPresent()
    {
        var dir = Directory.CreateTempSubdirectory("sefim-detect-images");
        try
        {
            var images = Directory.CreateDirectory(Path.Combine(dir.FullName, "proimages"));

            var found = SefimDetection.FindProImages(dir.FullName);

            Assert.Equal(images.FullName, found);
        }
        finally
        {
            dir.Delete(recursive: true);
        }
    }
}
