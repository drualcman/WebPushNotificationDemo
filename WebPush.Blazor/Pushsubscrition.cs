namespace WebPush.Blazor;

public class Pushsubscrition
{
    public string Endpoint { get; set; }
    public string P256dh { get; set; }
    public string Auth { get; set; }
    public bool IsNewSubscription { get; set; }
}
