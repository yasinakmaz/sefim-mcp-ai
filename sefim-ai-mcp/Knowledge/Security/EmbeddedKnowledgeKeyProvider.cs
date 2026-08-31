namespace SefimMcp.Knowledge.Security;

/// <summary>
/// Supplies the AES-256 key that opens <c>knowledge.pack</c>.
/// </summary>
/// <remarks>
/// The key is compiled into the binary so that every installation uses the same one and
/// the setup no longer asks for it. The bytes are stored XOR masked, which only keeps the
/// key out of a plain <c>strings</c> dump: anyone who can run the binary can recover it.
/// The pack therefore protects the knowledge against casual copying, not against an
/// attacker with the executable. Rotating the key means rebuilding the server and the
/// pack together, so a matching pack must ship with every build.
/// The environment variable still wins when it is set, which keeps packing with a
/// throwaway key possible during development.
/// </remarks>
public sealed class EmbeddedKnowledgeKeyProvider(string? environmentVariable = "SEFIM_KNOWLEDGE_KEY") : IKnowledgeKeyProvider
{
    private static readonly byte[] MaskedKey =
    [
        0x36, 0x23, 0x83, 0x98, 0xB3, 0x53, 0xB1, 0x00, 0xA6, 0xB0, 0xA7, 0x14, 0x0E, 0x8A,
        0x74, 0x20, 0xEC, 0x01, 0x9C, 0x95, 0xFC, 0x0A, 0x1F, 0xF1, 0x6A, 0x06, 0x0F, 0x67,
        0xBA, 0x7C, 0x3D, 0x7F
    ];

    private static readonly byte[] Mask =
    [
        0xF7, 0x41, 0xF5, 0x3A, 0x22, 0x7A, 0xE6, 0x16, 0x4F, 0x3B, 0x92, 0xD7, 0x3D, 0x4F,
        0xEB, 0x36, 0xE6, 0x85, 0xDD, 0xDD, 0x26, 0x68, 0x83, 0x3F, 0xB0, 0x69, 0xAA, 0xE4,
        0x75, 0x9D, 0xDC, 0x68
    ];

    public bool TryGetKey(out byte[] key)
    {
        if (!string.IsNullOrWhiteSpace(environmentVariable)
            && new EnvironmentKnowledgeKeyProvider(environmentVariable).TryGetKey(out key))
        {
            return true;
        }

        key = new byte[MaskedKey.Length];
        for (var index = 0; index < key.Length; index++)
            key[index] = (byte)(MaskedKey[index] ^ Mask[index]);

        return true;
    }
}
