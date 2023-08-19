
using WebPush.Server;

namespace Microsoft.Extensions.DependencyInjection;
public static class DependencyContainer
{
    public static IServiceCollection AddWebPushService(this IServiceCollection services)
    {
        services.AddHttpClient<WebPushService>();
        services.AddScoped<WebPushService>();
        return services;
    }
}
