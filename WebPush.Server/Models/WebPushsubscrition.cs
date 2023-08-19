namespace WebPush.Server.Models;

public class WebPushsubscrition
{
    public string Endpoint { get; }
    public string P256dh { get; }
    public string Auth { get; }

    public WebPushsubscrition(string endpoint, string p256dh, string auth)
    {
        Endpoint = endpoint;
        P256dh = p256dh;
        Auth = auth;
    }
}
