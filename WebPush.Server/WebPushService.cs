namespace WebPush.Server;
public class WebPushService
{
    readonly HttpClient Client;

    public WebPushService(HttpClient client) => Client = client;

    public async Task SendNotificationAsync(WebPushsubscrition subscription, string payload, VapidInfo vapidInfo, CancellationToken cancellationToken = default)
    {
        HttpRequestMessage request = WebPushHttpRequestBuilder.Build(subscription, payload, vapidInfo);
        HttpResponseMessage response = await Client.SendAsync(request, cancellationToken);
        if(!response.IsSuccessStatusCode)
        {
            await HandleResponseError(response);
        }
    }

    private async Task HandleResponseError(HttpResponseMessage response)
    {
        string responseCodeMessage = $"Received unexpected response code: {(int)response.StatusCode}";

        switch(response.StatusCode)
        {
            case HttpStatusCode.BadRequest:
                responseCodeMessage = "Bad request";
                break;
            case HttpStatusCode.RequestEntityTooLarge:
                responseCodeMessage = "Payload too large";
                break;
            case HttpStatusCode.TooManyRequests:
                responseCodeMessage = "Too many requests";
                break;
            case HttpStatusCode.NotFound:
            case HttpStatusCode.Gone:
                responseCodeMessage = "Subscription no longet valid";
                break;
        }
        string details = string.Empty;
        if(response.Content != null)
        {
            details = await response.Content.ReadAsStringAsync();
        }

        string message = string.IsNullOrEmpty(details) ? responseCodeMessage : $"{responseCodeMessage}. Details: {details}";
        throw new Exception(message);
    }
}
