using CoreTemplate.Email;
using CoreTemplate.Modules.EmailTemplates.Application;
using CoreTemplate.Modules.EmailTemplates.Application.Abstractions;
using CoreTemplate.Modules.EmailTemplates.Domain.Repositories;
using CoreTemplate.Modules.EmailTemplates.Infrastructure.Persistence;
using CoreTemplate.Modules.EmailTemplates.Infrastructure.Repositories;
using CoreTemplate.Modules.EmailTemplates.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CoreTemplate.Modules.EmailTemplates.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddEmailTemplatesModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Building block Email
        services.AddEmailService(configuration);

        // Application
        services.AddEmailTemplatesApplication(configuration);

        // DbContext
        var connectionString = configuration["DatabaseSettings:ConnectionString"]
            ?? throw new InvalidOperationException("No se encontró la cadena de conexión.");

        var provider = configuration["DatabaseSettings:Provider"] ?? "SqlServer";

        services.AddDbContext<EmailTemplatesDbContext>(options =>
        {
            if (provider.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase))
                options.UseNpgsql(connectionString,
                    sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "EmailTemplates"));
            else
                options.UseSqlServer(connectionString,
                    sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "EmailTemplates"));
        });

        // Repositorios y servicios
        services.AddScoped<IEmailTemplateRepository, EmailTemplateRepository>();
        services.AddScoped<IEmailTemplateSender, EmailTemplateSender>();
        services.AddScoped<ITemplateRenderer, TemplateRenderer>();
        services.AddSingleton<FallbackTemplateLoader>();

        return services;
    }
}
