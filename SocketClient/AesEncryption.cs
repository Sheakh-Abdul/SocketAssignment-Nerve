using System.Security.Cryptography;
using System.Text;

public static class AesEncryption
{
    private const int NonceSize = 12;
    private const int TagSize = 16;

    public static byte[] Encrypt(string plainText, string passphrase)
    {
        byte[] key = SHA256.HashData(Encoding.UTF8.GetBytes(passphrase));
        byte[] nonce = RandomNumberGenerator.GetBytes(NonceSize);
        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
        byte[] cipherBytes = new byte[plainBytes.Length];
        byte[] tag = new byte[TagSize];

        using var aes = new AesGcm(key, TagSize);
        aes.Encrypt(nonce, plainBytes, cipherBytes, tag);
        return [.. nonce, .. tag, .. cipherBytes];
    }

    public static string Decrypt(byte[] payload, string passphrase)
    {
        if (payload.Length < NonceSize + TagSize)
            throw new CryptographicException("Encrypted payload is too short.");

        byte[] key = SHA256.HashData(Encoding.UTF8.GetBytes(passphrase));
        ReadOnlySpan<byte> nonce = payload.AsSpan(0, NonceSize);
        ReadOnlySpan<byte> tag = payload.AsSpan(NonceSize, TagSize);
        ReadOnlySpan<byte> cipherBytes = payload.AsSpan(NonceSize + TagSize);
        byte[] plainBytes = new byte[cipherBytes.Length];

        using var aes = new AesGcm(key, TagSize);
        aes.Decrypt(nonce, cipherBytes, tag, plainBytes);
        return Encoding.UTF8.GetString(plainBytes);
    }
}
