using Microsoft.Extensions.Options;
using PushServerApi.Options;
using System.Text.Json;
using WebPush.Server;
using WebPush.Server.Models;

namespace PushServerApi.Services;

public class PushNotificationService
{
    readonly IServiceScopeFactory ServiceScopeFactory;
    readonly VapidInfoOptions VapidInfoOptions;
    readonly string DataFileName;

    public PushNotificationService(IWebHostEnvironment enviroment, IServiceScopeFactory serviceScopeFactory, IOptions<VapidInfoOptions> vapidInfoOptions)
    {
        ServiceScopeFactory = serviceScopeFactory;
        VapidInfoOptions = vapidInfoOptions.Value;
        DataFileName = $"{enviroment.ContentRootPath}\\data.json";
    }

    public async Task Subscribe(Entities.WebPushsubscrition subscrition)
    {
        using FileStream fs = new FileStream(DataFileName, FileMode.Create);
        await JsonSerializer.SerializeAsync(fs, subscrition);
    }

    static async Task SendNotification(WebPushService webPushService, ILogger<PushNotificationService> logger, WebPushsubscrition subscrition, VapidInfo vapidInfo, string message)
    {
        var payload = new
        {
            message = message,
            url = "/counter"
        };

        string serializePayload = JsonSerializer.Serialize(payload);
        try
        {
            await webPushService.SendNotificationAsync(subscrition, serializePayload, vapidInfo, CancellationToken.None);
            logger.LogInformation("*** send '{0}' notification. ***", message);
        }
        catch(Exception ex)
        {
            logger.LogError("Error: {0}", ex.Message);
            throw;
        }
    }

    public void SendExampleNotification()
    {
        Task.Run(async () => 
        {
            int delay = 45000;
            using IServiceScope scope = ServiceScopeFactory.CreateScope();            
            WebPushService webpushService = scope.ServiceProvider.GetRequiredService<WebPushService>();
            ILogger<PushNotificationService> logger = scope.ServiceProvider.GetRequiredService<ILogger<PushNotificationService>>();
            FileStream stream = new FileStream(DataFileName, FileMode.Open, FileAccess.Read);
            Entities.WebPushsubscrition subscriptionData = await JsonSerializer.DeserializeAsync<Entities.WebPushsubscrition>(stream);
            WebPushsubscrition subscription = new WebPushsubscrition(subscriptionData.Endpoint, subscriptionData.P256dh, subscriptionData.Auth);
            await stream.DisposeAsync();

            VapidInfo vapidInfo = new VapidInfo(VapidInfoOptions.Subject, VapidInfoOptions.PublicKey, VapidInfoOptions.PrivateKey);
            
            await SendNotification(webpushService, logger, subscription, vapidInfo, "When I grow, I want to be a watermelon.");

            await Task.Delay(delay);

            await SendNotification(webpushService, logger, subscription, vapidInfo, "When I grow, I want to be a apple.");
            
            await Task.Delay(delay);

            await SendNotification(webpushService, logger, subscription, vapidInfo, "When I grow, I want to be a pinapple.");
        });
    }
}
