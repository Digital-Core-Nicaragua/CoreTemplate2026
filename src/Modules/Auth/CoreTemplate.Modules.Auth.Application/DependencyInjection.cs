using CoreTemplate.Api.Common.Behaviors;
using CoreTemplate.Modules.Auth.Application.Abstractions;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CoreTemplate.Modules.Auth.Application;

/// <summary>
/// Registro de dependencias de la capa Application del módulo Auth.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registra MediatR, ValidationBehavior, FluentValidation y settings del módulo Auth.
    /// </summary>
    public static IServiceCollection AddAuthApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, includeInternalTypes: true);

        services.Configure<AuthSettings>(configuration.GetSection(AuthSettings.SectionName));
        services.Configure<LockoutSettings>(configuration.GetSection(LockoutSettings.SectionName));
        services.Configure<PasswordPolicySettings>(configuration.GetSection(PasswordPolicySettings.SectionName));
        services.Configure<OrganizationSettings>(configuration.GetSection(OrganizationSettings.SectionName));

        return services;
    }
}
