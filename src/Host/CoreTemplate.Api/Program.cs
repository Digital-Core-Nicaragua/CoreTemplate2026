using CoreTemplate.Api.Common;
using CoreTemplate.Api.Extensions;
using CoreTemplate.Infrastructure;
using CoreTemplate.Modules.Auth.Application;
using CoreTemplate.Modules.Auth.Infrastructure;
using CoreTemplate.Modules.Auth.Infrastructure.Middleware;
using CoreTemplate.Modules.Catalogos.Application;
using CoreTemplate.Modules.Catalogos.Infrastructure;
using Serilog;

// ─── Serilog bootstrap logger ────────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Iniciando CoreTemplate API...");

    var builder = WebApplication.CreateBuilder(args);

    // ─── Serilog ─────────────────────────────────────────────────────────────
    builder.Host.UseSerilog((ctx, lc) => lc
        .ReadFrom.Configuration(ctx.Configuration)
        .WriteTo.Console()
        .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day));

    // ─── Controllers + módulos ────────────────────────────────────────────────
    builder.Services.AddControllers()
        .AddApplicationPart(typeof(CoreTemplate.Modules.Auth.Api.Controllers.AuthController).Assembly)
        .AddApplicationPart(typeof(CoreTemplate.Modules.Catalogos.Api.Controllers.CatalogosController).Assembly);

    // ─── Infraestructura base (ICurrentUser, ICurrentTenant, Settings) ────────
    builder.Services.AddInfrastructureBase(builder.Configuration);

    // ─── Manejo global de excepciones ─────────────────────────────────────────
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    // ─── Módulo Auth ──────────────────────────────────────────────────────────
    builder.Services.AddAuthApplication(builder.Configuration);
    builder.Services.AddAuthInfrastructure(builder.Configuration);

    // ─── Módulo Catálogos ─────────────────────────────────────────────────────
    builder.Services.AddCatalogosApplication(builder.Configuration);
    builder.Services.AddCatalogosInfrastructure(builder.Configuration);

    // ─── Swagger con soporte JWT ──────────────────────────────────────────────
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    // ─── Pipeline HTTP ────────────────────────────────────────────────────────

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
    Log.Fatal(ex, "La aplicación falló al iniciar.");
}
finally
{
    Log.CloseAndFlush();
}
