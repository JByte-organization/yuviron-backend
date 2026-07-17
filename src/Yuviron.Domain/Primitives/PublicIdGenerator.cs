using System.Security.Cryptography;

namespace Yuviron.Domain.Common;

public static class PublicIdGenerator
{
    public const int DefaultLength = 22;

    private const string Alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

    public static string NewId(int length = DefaultLength)
    {
        if (length <= 0) throw new ArgumentOutOfRangeException(nameof(length));

        Span<byte> bytes = stackalloc byte[length];
        RandomNumberGenerator.Fill(bytes);

        var chars = new char[length];
        for (var i = 0; i < bytes.Length; i++)
        {
            chars[i] = Alphabet[bytes[i] % Alphabet.Length];
        }

        return new string(chars);
    }
}
