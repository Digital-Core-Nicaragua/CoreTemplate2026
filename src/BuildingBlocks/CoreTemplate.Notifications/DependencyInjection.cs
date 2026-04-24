using CoreTemplate.Notifications.Abstractions;
using CoreTemplate.Notifications.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CoreTemplate.Notifications;

public static class DependencyInjection
{
    public static IServiceCollection AddNotificationsService(this IServiceCollection services)
    {
        services.AddSignalR();
        services.AddScoped<INotificationSender, SignalRNotificationSender>();
        return services;
    }
}
