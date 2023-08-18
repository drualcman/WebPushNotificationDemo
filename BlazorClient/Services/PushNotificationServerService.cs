using Entities;
using System.Net.Http.Json;

namespace BlazorClient.Services;

public class PushNotificationServerService
{
    readonly HttpClient Client;

    public PushNotificationServerService(HttpClient client) => Client = client;

    public async Task<bool> SendSubscription(WebPushsubscrition subscription)
    {
        HttpResponseMessage response = await Client.PostAsJsonAsync("subscribe", subscription);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> RequestExampleNotification()
    {
        HttpResponseMessage response = await Client.GetAsync("request-example-notification");
        return response.IsSuccessStatusCode;
    }
}
