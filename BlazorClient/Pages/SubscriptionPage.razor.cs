using BlazorClient.Services;
using Microsoft.AspNetCore.Components;
using WebPush.Blazor;

namespace BlazorClient.Pages;

public partial class SubscriptionPage
{
    [Inject] WebPushService PushNotificationService { get; set; }
    [Inject] PushNotificationServerService PushNotificationServerService { get; set; }

    PushSubscrition Subscription;
    string Message;

    async Task GetSubscription()
    {
        Message = string.Empty;
        Subscription = await PushNotificationService.GetSubscriptionAsync();
        if(Subscription == default)
        {
            Message = "No se pudo obtener la subscripcion";
        }
    }

    async Task SendSubscription()
    {
        bool success = await PushNotificationServerService.SendSubscription(new Entities.WebPushsubscrition
        {
            Endpoint = Subscription.Endpoint,
            P256dh = Subscription.P256dh,
            Auth = Subscription.Auth
        });

        if(success)
            Message = "Datos enviados con exito.";
        else
            Message = "Error al enviar los datos.";
    }

    async Task RequestExampleNotification()
    {
        if(await PushNotificationServerService.RequestExampleNotification())
            Message = "Solicitud de notificacion enviada.";
        else
            Message = "Error de solicitud de notificacion";
    }
}
