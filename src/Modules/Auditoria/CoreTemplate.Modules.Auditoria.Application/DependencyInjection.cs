using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace CoreTemplate.Modules.Auditoria.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddAuditoriaApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        return services;
    }
}
