namespace SefimMcp.Knowledge.Security;

public sealed class EnvironmentKnowledgeKeyProvider(string environmentVariable) : IKnowledgeKeyProvider
{
    public bool TryGetKey(out byte[] key)
    {
        key = [];
        var encodedKey = Environment.GetEnvironmentVariable(environmentVariable);
        if (string.IsNullOrWhiteSpace(encodedKey))
            return false;

        try
        {
            key = Convert.FromBase64String(encodedKey);
            if (key.Length == 32)
                return true;
        }
        catch (FormatException)
        {
        }

        key = [];
        return false;
    }
}
