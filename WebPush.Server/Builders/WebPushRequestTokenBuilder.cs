using WebPush.Server.Extensions;

namespace WebPush.Server.Builders;
internal class WebPushRequestTokenBuilder
{
    public static string Build(string endpoint, VapidInfo vapidInfo)
    {
        Uri uri = new Uri(endpoint);
        string audience = $"{uri.Scheme}://{uri.Host}";

        long expiration = ((DateTimeOffset)DateTime.UtcNow.AddHours(12)).ToUnixTimeSeconds();

        byte[] privateKeyBytes = vapidInfo.PrivateKey.ToBytesFromBase64Url();

        Dictionary<string, string> header = new Dictionary<string, string>
        {
            { "typ", "JWT" },
            { "alg", "ES256" }
        };

        Dictionary<string, object> payload = new Dictionary<string, object>
        {
            { "aud", audience },
            { "exp", expiration },
            { "sub", vapidInfo.Subject }
        };

        string encodedHeader = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(header)).ToBase64UrlString();
        string encodedPayload = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(payload)).ToBase64UrlString();
        string unsignedToken = $"{encodedHeader}.{encodedPayload}";
        string signature = ECDSA.Sign(unsignedToken, privateKeyBytes).ToBase64UrlString();

        return $"{unsignedToken}.{signature}";
    }
}
