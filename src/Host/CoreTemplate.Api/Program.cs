using CoreTemplate.Api.Common;
using CoreTemplate.Api.Extensions;
using CoreTemplate.Infrastructure;
using CoreTemplate.Logging.Configuration;
using CoreTemplate.Monitoring.Configuration;
using CoreTemplate.Modules.Auth.Infrastructure;
using CoreTemplate.Modules.Auth.Infrastructure.Middleware;
using CoreTemplate.Modules.Catalogos.Infrastructure;
using CoreTemplate.Modules.Archivos.Infrastructure;
using CoreTemplate.Modules.EmailTemplates.Infrastructure;
using CoreTemplate.Modules.PdfTemplates.Infrastructure;
using CoreTemplate.Modules.Configuracion.Infrastructure;
using CoreTemplate.Modules.Notificaciones.Infrastructure;
using CoreTemplate.Modules.Auditoria.Application;
using Microsoft.OpenApi.Models;
using Serilog;

// Serilog bootstrap logger
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Iniciando CoreTemplate API...");

    var builder = WebApplication.CreateBuilder(args);

    // Serilog
    builder.Host.UseSerilog((ctx, lc) => lc
        .ReadFrom.Configuration(ctx.Configuration)
        .WriteTo.Console());

    // Controllers + modulos
    builder.Services.AddControllers()
        .AddApplicationPart(typeof(CoreTemplate.Modules.Auth.Api.Controllers.AuthController).Assembly)
        .AddApplicationPart(typeof(CoreTemplate.Modules.Catalogos.Api.Controllers.CatalogosController).Assembly)
        .AddApplicationPart(typeof(CoreTemplate.Modules.EmailTemplates.Api.Controllers.EmailTemplatesController).Assembly)
        .AddApplicationPart(typeof(CoreTemplate.Modules.Archivos.Api.Controllers.ArchivosController).Assembly)
        .AddApplicationPart(typeof(CoreTemplate.Modules.PdfTemplates.Api.Controllers.PdfTemplatesController).Assembly)
        .AddApplicationPart(typeof(CoreTemplate.Modules.Configuracion.Api.Controllers.ConfiguracionController).Assembly)
        .AddApplicationPart(typeof(CoreTemplate.Modules.Notificaciones.Api.Controllers.NotificacionesController).Assembly)
        .AddApplicationPart(typeof(CoreTemplate.Modules.Auditoria.Api.Controllers.AuditoriaController).Assembly);

    // Infraestructura base (incluye Logging, Auditing, Monitoring)
    builder.Services.AddInfrastructureBase(builder.Configuration);

    // Manejo global de excepciones
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    // Modulo Auth
    builder.Services.AddAuthModule(builder.Configuration);

    // Modulo Catalogos
    builder.Services.AddCatalogosModule(builder.Configuration);

    // Modulo EmailTemplates (incluye building block Email)
    builder.Services.AddEmailTemplatesModule(builder.Configuration);

    // Modulo Archivos (incluye building block Storage)
    builder.Services.AddArchivosModule(builder.Configuration);

    // Modulo PdfTemplates (incluye building block Pdf con QuestPDF)
    builder.Services.AddPdfTemplatesModule(builder.Configuration);

    // Modulo Configuracion del Sistema
    builder.Services.AddConfiguracionModule(builder.Configuration);

    // Modulo Notificaciones (SignalR)
    builder.Services.AddNotificacionesModule(builder.Configuration);

    // Modulo Auditoria (consulta de logs — reutiliza AuditDbContext)
    builder.Services.AddAuditoriaApplication();

    // Swagger con soporte JWT
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "CoreTemplate API", Version = "v1" });

        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Ingresa el token JWT obtenido del login. Ejemplo: eyJhbGci..."
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });

    var app = builder.Build();

    app.UseExceptionHandler();

    // Servir archivos estáticos del storage local (igual que Rancho Santana)
    var storagePath = builder.Configuration["LocalStorageSettings:BasePath"] ?? "archivos";
    var storageRequestPath = builder.Configuration["LocalStorageSettings:RequestPath"] ?? "/archivos";
    var storageFullPath = Path.IsPathRooted(storagePath)
        ? storagePath
        : Path.Combine(Directory.GetCurrentDirectory(), storagePath);

    if (!Directory.Exists(storageFullPath))
        Directory.CreateDirectory(storageFullPath);

    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(storageFullPath),
        RequestPath = storageRequestPath
    });

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "CoreTemplate API v1");
            c.RoutePrefix = "swagger";
        });

        await app.SeedDatabaseAsync();
    }

    if (builder.Configuration.GetValue<bool>("TenantSettings:IsMultiTenant"))
    {
        app.UseMiddleware<CoreTemplate.Infrastructure.Middleware.TenantMiddleware>();
    }

    app.UseSerilogRequestLogging();
    app.UseCorrelationMiddleware();
    app.UseHealthCheckEndpoints(builder.Configuration);
    app.UseHttpsRedirection();
    app.UseAuthentication();

    if (builder.Configuration.GetValue<bool>("AuthSettings:EnableTokenBlacklist"))
    {
        app.UseMiddleware<TokenBlacklistMiddleware>();
    }

    app.UseAuthorization();
    app.MapControllers();

    // Hub de SignalR para notificaciones en tiempo real
    app.MapHub<CoreTemplate.Notifications.Hubs.NotificationHub>("/hubs/notificaciones");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "La aplicacion fallo al iniciar.");
}
finally
{
    Log.CloseAndFlush();
}
