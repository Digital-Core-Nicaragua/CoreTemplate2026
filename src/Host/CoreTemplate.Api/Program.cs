using CoreTemplate.Api.Common;
using CoreTemplate.Api.Extensions;
using CoreTemplate.Infrastructure;
using CoreTemplate.Logging.Configuration;
using CoreTemplate.Monitoring.Configuration;
using CoreTemplate.Modules.Auth.Infrastructure;
using CoreTemplate.Modules.Auth.Infrastructure.Middleware;
using CoreTemplate.Modules.Catalogos.Infrastructure;
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
        .AddApplicationPart(typeof(CoreTemplate.Modules.Catalogos.Api.Controllers.CatalogosController).Assembly);

    // Infraestructura base (incluye Logging, Auditing, Monitoring)
    builder.Services.AddInfrastructureBase(builder.Configuration);

    // Manejo global de excepciones
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    // Modulo Auth
    builder.Services.AddAuthModule(builder.Configuration);

    // Modulo Catalogos
    builder.Services.AddCatalogosModule(builder.Configuration);

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
