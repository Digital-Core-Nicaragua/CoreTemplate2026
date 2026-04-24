using CoreTemplate.Modules.Notificaciones.Application;
using CoreTemplate.Modules.Notificaciones.Domain.Repositories;
using CoreTemplate.Modules.Notificaciones.Infrastructure.Persistence;
using CoreTemplate.Modules.Notificaciones.Infrastructure.Repositories;
using CoreTemplate.Modules.Notificaciones.Infrastructure.Services;
using CoreTemplate.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CoreTemplate.Modules.Notificaciones.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddNotificacionesModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Building block (SignalR + INotificationSender)
        services.AddNotificationsService();

        // Application
        services.AddNotificacionesApplication();

        // DbContext
        var connectionString = configuration["DatabaseSettings:ConnectionString"]
            ?? throw new InvalidOperationException("No se encontró la cadena de conexión.");

        var provider = configuration["DatabaseSettings:Provider"] ?? "SqlServer";

        services.AddDbContext<NotificacionesDbContext>(options =>
        {
            if (provider.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase))
                options.UseNpgsql(connectionString,
                    sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "Notificaciones"));
            else
                options.UseSqlServer(connectionString,
                    sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "Notificaciones"));
        });

        services.AddScoped<INotificacionRepository, NotificacionRepository>();
        services.AddScoped<NotificacionPendienteService>();

        return services;
    }
}
