using System.Security.Cryptography;
using System.Text.Json;
using SefimMcp.Json;
using SefimMcp.Knowledge.Models;

namespace SefimMcp.Knowledge.Runtime;

public static class KnowledgePackCodec
{
    private static readonly byte[] Magic = "SEFIMKP1"u8.ToArray();
    private const int NonceLength = 12;
    private const int TagLength = 16;

    public static byte[] Encrypt(KnowledgePack pack, byte[] key)
    {
        ArgumentOutOfRangeException.ThrowIfNotEqual(key.Length, 32);
        var plaintext = JsonSerializer.SerializeToUtf8Bytes(pack, AppJsonSerializerContext.Default.KnowledgePack);
        var nonce = RandomNumberGenerator.GetBytes(NonceLength);
        var ciphertext = new byte[plaintext.Length];
        var tag = new byte[TagLength];
        using var aes = new AesGcm(key, TagLength);
        aes.Encrypt(nonce, plaintext, ciphertext, tag, Magic);

        var output = new byte[Magic.Length + NonceLength + TagLength + ciphertext.Length];
        Magic.CopyTo(output, 0);
        nonce.CopyTo(output, Magic.Length);
        tag.CopyTo(output, Magic.Length + NonceLength);
        ciphertext.CopyTo(output, Magic.Length + NonceLength + TagLength);
        CryptographicOperations.ZeroMemory(plaintext);
        return output;
    }

    public static KnowledgePack Decrypt(ReadOnlySpan<byte> input, byte[] key)
    {
        ArgumentOutOfRangeException.ThrowIfNotEqual(key.Length, 32);
        if (input.Length < Magic.Length + NonceLength + TagLength || !input[..Magic.Length].SequenceEqual(Magic))
            throw new CryptographicException("Knowledge pack header is invalid.");

        var nonce = input.Slice(Magic.Length, NonceLength);
        var tag = input.Slice(Magic.Length + NonceLength, TagLength);
        var ciphertext = input[(Magic.Length + NonceLength + TagLength)..];
        var plaintext = new byte[ciphertext.Length];
        using var aes = new AesGcm(key, TagLength);
        aes.Decrypt(nonce, ciphertext, tag, plaintext, Magic);
        try
        {
            return JsonSerializer.Deserialize(plaintext, AppJsonSerializerContext.Default.KnowledgePack)
                ?? throw new InvalidDataException("Knowledge pack is empty.");
        }
        finally
        {
            CryptographicOperations.ZeroMemory(plaintext);
        }
    }
}
