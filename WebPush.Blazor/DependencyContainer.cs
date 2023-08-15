using WebPush.Blazor;

namespace Microsoft.Extensions.DependencyInjection;
public static class DependencyContainer
{
    public static IServiceCollection AddPushNotificatoinService(this IServiceCollection services, Action<WebPushNotificationOptions> configurePushNotificationOptions)
    {
        services.AddScoped<WebPushNotificationOptions>()
            .Configure(configurePushNotificationOptions);
        services.AddScoped<WebPushService>();
        return services;
    }
}
