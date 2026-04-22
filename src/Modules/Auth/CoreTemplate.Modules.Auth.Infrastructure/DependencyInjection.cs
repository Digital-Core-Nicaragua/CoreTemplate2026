using CoreTemplate.Modules.Auth.Application;
using CoreTemplate.Modules.Auth.Application.Abstractions;
using CoreTemplate.Modules.Auth.Domain.Repositories;
using CoreTemplate.Modules.Auth.Infrastructure.Persistence;
using CoreTemplate.Modules.Auth.Infrastructure.Repositories;
using CoreTemplate.Modules.Auth.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.Text;

namespace CoreTemplate.Modules.Auth.Infrastructure;

/// <summary>
/// Registro de dependencias de la capa Infrastructure del módulo Auth.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registra DbContext, repositorios, servicios JWT/Password/TOTP y autenticación JWT.
    /// </summary>
    public static IServiceCollection AddAuthInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // DbContext
        var connectionString = configuration["DatabaseSettings:ConnectionString"]
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("No se encontró la cadena de conexión.");

        var provider = configuration["DatabaseSettings:Provider"] ?? "SqlServer";

        services.AddDbContext<AuthDbContext>(options =>
        {
            if (provider.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase))
            {
                options.UseNpgsql(connectionString,
                    sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "Auth"));
            }
            else
            {
                options.UseSqlServer(connectionString,
                    sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "Auth"));
            }
        });

        // Repositorios
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IRolRepository, RolRepository>();
        services.AddScoped<IPermisoRepository, PermisoRepository>();
        services.AddScoped<ISesionRepository, SesionRepository>();
        services.AddScoped<IRegistroAuditoriaRepository, RegistroAuditoriaRepository>();
        services.AddScoped<IConfiguracionTenantRepository, ConfiguracionTenantRepository>();

        // Repositorios opcionales — siempre registrados para que MediatR pueda resolver
        // los handlers. El flag EnableBranches / UseActionCatalog controla el comportamiento
        // en runtime dentro de cada handler.
        services.AddScoped<ISucursalRepository, SucursalRepository>();
        services.AddScoped<IAsignacionRolRepository, AsignacionRolRepository>();
        services.AddScoped<IAccionRepository, AccionRepository>();

        // Portal de clientes — siempre registrado, el flag EnableCustomerPortal
        // controla el comportamiento en runtime dentro de cada handler.
        services.AddScoped<IUsuarioClienteRepository, UsuarioClienteRepository>();

        // INotificacionClienteService — implementacion no-op por defecto (loguea el OTP en desarrollo).
        // El sistema que use CoreTemplate puede sobreescribirla registrando su propia implementacion
        // (Twilio, AWS SNS, etc.) ANTES de llamar a AddAuthModule().
        services.TryAddScoped<INotificacionClienteService, NullNotificacionClienteService>();

        // OAuth services — registrados directamente por tipo concreto.
        // OAuthServiceFactory resuelve el correcto según el proveedor en runtime,
        // evitando ambigüedad de múltiples implementaciones de IProveedorOAuthService.
        services.AddHttpClient<FacebookOAuthService>();
        services.AddScoped<GoogleOAuthService>();
        services.AddScoped<IOAuthServiceFactory, OAuthServiceFactory>();

        // Servicios
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<ISesionService, SesionService>();
        services.AddSingleton<ITotpService, TotpService>();

        // Token Blacklist
        services.Configure<TokenBlacklistSettings>(
            configuration.GetSection(TokenBlacklistSettings.SectionName));

        var blacklistProvider = configuration["TokenBlacklistSettings:Provider"] ?? "InMemory";
        if (blacklistProvider.Equals("Redis", StringComparison.OrdinalIgnoreCase))
        {
            var redisConn = configuration["TokenBlacklistSettings:RedisConnectionString"]
                ?? throw new InvalidOperationException("RedisConnectionString no configurado.");
            services.AddSingleton<IConnectionMultiplexer>(_ =>
                ConnectionMultiplexer.Connect(redisConn));
            services.AddSingleton<ITokenBlacklistService, RedisTokenBlacklistService>();
        }
        else
        {
            services.AddSingleton<ITokenBlacklistService, InMemoryTokenBlacklistService>();
        }

        // Autenticación JWT
        var jwtSecretKey = configuration["AuthSettings:JwtSecretKey"]
            ?? throw new InvalidOperationException("JwtSecretKey no configurado.");

        var jwtIssuer = configuration["AuthSettings:JwtIssuer"] ?? "CoreTemplate";
        var jwtAudience = configuration["AuthSettings:JwtAudience"] ?? "CoreTemplate";

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSecretKey)),
                    ValidateIssuer = true,
                    ValidIssuer = jwtIssuer,
                    ValidateAudience = true,
                    ValidAudience = jwtAudience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorization();

        return services;
    }

    /// <summary>
    /// Registra el modulo Auth completo: Application + Infrastructure.
    /// Usar este metodo desde Program.cs.
    /// </summary>
    public static IServiceCollection AddAuthModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAuthApplication(configuration);
        services.AddAuthInfrastructure(configuration);
        return services;
    }
}
