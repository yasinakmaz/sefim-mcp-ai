namespace SefimMcp.Knowledge.Security;

public interface IKnowledgeKeyProvider
{
    bool TryGetKey(out byte[] key);
}
