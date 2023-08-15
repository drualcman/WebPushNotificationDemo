using Microsoft.Extensions.Options;
using Microsoft.JSInterop;

namespace WebPush.Blazor;
public class WebPushService : IAsyncDisposable
{
    readonly Lazy<Task<IJSObjectReference>> ModuleTask;
    readonly string ServerPublicKey;

    public WebPushService(IJSRuntime jsRuntime, IOptions<WebPushNotificationOptions> options)
    {
        ServerPublicKey = options.Value.ServerPublicKey;
        ModuleTask = new Lazy<Task<IJSObjectReference>>(() => 
            jsRuntime.InvokeAsync<IJSObjectReference>("import", $"./_content/{this.GetType().Assembly.GetName().Name}/js/push-notifications.js").AsTask());
    }

    public async ValueTask DisposeAsync() 
    {
        if(ModuleTask.IsValueCreated)
        {              
            IJSObjectReference module = await ModuleTask.Value;
            await module.DisposeAsync();
        }
    }

    public async Task<Pushsubscrition> GetSubscriptionAsync()
    {
        Pushsubscrition subscription = default;

        try
        {
            IJSObjectReference module = await ModuleTask.Value;
            subscription = await module.InvokeAsync<Pushsubscrition>("getSubscription", ServerPublicKey);
        }
        catch(Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.Message}");
            Console.WriteLine($"ERROR DETAIL: {ex}");
        }
        return subscription;
    }
}
