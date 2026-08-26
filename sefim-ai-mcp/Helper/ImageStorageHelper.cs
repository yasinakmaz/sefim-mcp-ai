namespace SefimMcp.Helper;

public static class ImageStorageHelper
{
    public static IDisposable OpenConnection(string imageRoot, string? username, string? password)
    {
        if (!OperatingSystem.IsWindows() || !IsUncPath(imageRoot) || string.IsNullOrWhiteSpace(username))
        {
            return NoopDisposable.Instance;
        }

        return new WindowsNetworkShareConnection(GetUncShareRoot(imageRoot), username, password);
    }

    public static string NormalizeImageRoot(string imageLocation)
    {
        if (string.IsNullOrWhiteSpace(imageLocation))
        {
            throw new InvalidOperationException("SEFIM:ImageLocation is empty.");
        }

        string value = imageLocation.Trim();

        if (value.StartsWith("smb://", StringComparison.OrdinalIgnoreCase))
        {
            if (!OperatingSystem.IsWindows())
            {
                throw new InvalidOperationException("Linux cannot use smb:// directly with File APIs. Mount the SMB share with cifs-utils and set SEFIM:ImageLocation to the mounted folder, for example /mnt/proimages.");
            }

            var uri = new Uri(value);
            string[] segments = uri.AbsolutePath
                .Split('/', StringSplitOptions.RemoveEmptyEntries)
                .Select(Uri.UnescapeDataString)
                .ToArray();

            if (string.IsNullOrWhiteSpace(uri.Host) || segments.Length == 0)
            {
                throw new InvalidOperationException("Invalid smb:// image location. Expected format: smb://server/share");
            }

            value = @"\\" + uri.Host + @"\" + string.Join(@"\", segments);
        }

        if (!OperatingSystem.IsWindows() && IsUncPath(value))
        {
            throw new InvalidOperationException("Linux cannot use a Windows UNC path directly. Mount the SMB share first, then set SEFIM:ImageLocation to the mount path, for example /mnt/proimages.");
        }

        return value.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }

    public static string BuildImagePath(string imageRoot, string imageName)
    {
        string fileName = imageName
            .Replace('\\', '/')
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .LastOrDefault() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new InvalidOperationException("Image file name is empty.");
        }

        return Path.Combine(imageRoot, fileName);
    }

    public static Content.McpToolResponse TextResponse(string text)
    {
        return new Content.McpToolResponse(
        [
            new Content.McpContent(Type: "text", Text: text)
        ]);
    }

    public static string GetMimeType(string path)
    {
        return Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".bmp" => "image/bmp",
            ".svg" => "image/svg+xml",
            _ => "application/octet-stream"
        };
    }

    public static string GetImageExtensionFromUrl(string imageUrl)
    {
        if (Uri.TryCreate(imageUrl, UriKind.Absolute, out var uri))
        {
            return NormalizeImageExtension(Path.GetExtension(uri.AbsolutePath));
        }

        return ".png";
    }

    public static byte[] DecodeBase64Image(string imageBase64, out string extension)
    {
        string base64Data = imageBase64.Trim();
        extension = ".png";

        if (base64Data.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
        {
            int commaIndex = base64Data.IndexOf(',');

            if (commaIndex < 0)
            {
                throw new FormatException("Invalid data URL. Missing comma separator.");
            }

            string header = base64Data[..commaIndex];
            extension = GetImageExtensionFromMime(header);
            base64Data = base64Data[(commaIndex + 1)..];
        }

        return Convert.FromBase64String(base64Data);
    }

    private static bool IsUncPath(string path)
    {
        return path.StartsWith(@"\\", StringComparison.Ordinal);
    }

    private static string GetUncShareRoot(string path)
    {
        string[] parts = path
            .TrimStart('\\')
            .Split('\\', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length < 2)
        {
            throw new InvalidOperationException("Invalid UNC image location. Expected format: \\\\server\\share");
        }

        return @"\\" + parts[0] + @"\" + parts[1];
    }

    private static string GetImageExtensionFromMime(string dataUrlHeader)
    {
        return dataUrlHeader.ToLowerInvariant() switch
        {
            var header when header.Contains("image/jpeg") => ".jpg",
            var header when header.Contains("image/png") => ".png",
            var header when header.Contains("image/gif") => ".gif",
            var header when header.Contains("image/webp") => ".webp",
            var header when header.Contains("image/bmp") => ".bmp",
            var header when header.Contains("image/svg+xml") => ".svg",
            _ => ".png"
        };
    }

    private static string NormalizeImageExtension(string? extension)
    {
        return extension?.ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => ".jpg",
            ".png" => ".png",
            ".gif" => ".gif",
            ".webp" => ".webp",
            ".bmp" => ".bmp",
            ".svg" => ".svg",
            _ => ".png"
        };
    }
}

internal sealed class NoopDisposable : IDisposable
{
    public static readonly NoopDisposable Instance = new();

    private NoopDisposable()
    {
    }

    public void Dispose()
    {
    }
}

internal sealed class WindowsNetworkShareConnection : IDisposable
{
    private const int ResourceTypeDisk = 1;

    private readonly string _remoteName;
    private bool _connected;

    public WindowsNetworkShareConnection(string remoteName, string username, string? password)
    {
        _remoteName = remoteName;

        var netResource = new NetResource
        {
            dwType = ResourceTypeDisk,
            lpRemoteName = remoteName
        };

        int result = WNetAddConnection2(ref netResource, password, username, 0);

        if (result != 0)
        {
            throw new InvalidOperationException($"Windows SMB connection failed for {remoteName}. {GetWindowsNetworkErrorMessage(result)}");
        }

        _connected = true;
    }

    public void Dispose()
    {
        if (!_connected)
        {
            return;
        }

        WNetCancelConnection2(_remoteName, 0, false);
        _connected = false;
    }

    private static string GetWindowsNetworkErrorMessage(int errorCode)
    {
        return errorCode switch
        {
            5 => "Access denied. Check SEFIM:ImageUsername and SEFIM:ImagePassword.",
            53 => "Network path was not found. Check the server IP/name and share name.",
            67 => "Network name was not found. Check the share name.",
            86 => "The username or password is incorrect.",
            1219 => "Windows already has a connection to this server/share with different credentials. Disconnect the old connection or use the same credentials.",
            _ => $"Windows error code: {errorCode}."
        };
    }

    [System.Runtime.InteropServices.DllImport("mpr.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
    private static extern int WNetAddConnection2(
        ref NetResource netResource,
        string? password,
        string username,
        int flags);

    [System.Runtime.InteropServices.DllImport("mpr.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
    private static extern int WNetCancelConnection2(
        string name,
        int flags,
        bool force);

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
    private struct NetResource
    {
        public int dwScope;
        public int dwType;
        public int dwDisplayType;
        public int dwUsage;
        public string? lpLocalName;
        public string? lpRemoteName;
        public string? lpComment;
        public string? lpProvider;
    }
}
