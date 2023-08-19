using WebPush.Server.Extensions;

namespace WebPush.Server.Builders;
internal class WebPushHttpRequestBuilder
{
    public static HttpRequestMessage Build(WebPushsubscrition subscription, string payload, VapidInfo vapidInfo)
    {
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, subscription.Endpoint);

        /*
         * Obtener el encabezado de autorizacion que incluye:
         * Token
         * Vapid.PublickKey
         */

        string webPushAuthorizationToken = WebPushRequestTokenBuilder.Build(subscription.Endpoint, vapidInfo);
        WebPushBodyBuilderResult encryptedPayload = PayloadEncryptor.Encrypt(subscription, payload);

        const int TimeToLiveInSeconds = 604800;     // 7 days

        request.Headers.Add("Authorization", $"WebPush {webPushAuthorizationToken}");
        request.Headers.Add("Encryption", $"salt={encryptedPayload.Salt.ToBase64UrlString()}");
        request.Headers.Add("Crypto-Key", $"dh={encryptedPayload.PublicKey.ToBase64UrlString()};p256ecdsa={vapidInfo.PublicKey}");
        request.Headers.Add("TTL", TimeToLiveInSeconds.ToString());

        request.Content = new ByteArrayContent(encryptedPayload.Payload);
        request.Content.Headers.ContentLength = encryptedPayload.Payload.Length;
        request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
        request.Content.Headers.ContentEncoding.Add("aesgcm");

        return request;
    }
}
