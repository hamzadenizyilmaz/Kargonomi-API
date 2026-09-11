using System.Security.Cryptography;

namespace Kargonomi.Client.Webhooks;

public enum WebhookSignatureEncoding
{
    Hex,
    Base64
}
public static class WebhookSignatureVerifier
{
    public static bool Verify(ReadOnlySpan<byte> rawBody, string signature, ReadOnlySpan<byte> secret, WebhookSignatureEncoding encoding)
    {
        if (string.IsNullOrWhiteSpace(signature) || secret.IsEmpty)
            return false;

        byte[] supplied;
        try
        {
            supplied = encoding == WebhookSignatureEncoding.Hex
                ? Convert.FromHexString(signature)
                : Convert.FromBase64String(signature);
        }
        catch (FormatException)
        {
            return false;
        }

        var expected = HMACSHA256.HashData(secret, rawBody);
        return supplied.Length == expected.Length && CryptographicOperations.FixedTimeEquals(supplied, expected);
    }
}
