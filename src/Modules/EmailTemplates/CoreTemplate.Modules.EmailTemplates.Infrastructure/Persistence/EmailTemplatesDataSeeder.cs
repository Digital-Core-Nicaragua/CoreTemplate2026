using CoreTemplate.Modules.EmailTemplates.Domain.Aggregates;
using CoreTemplate.Modules.EmailTemplates.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CoreTemplate.Modules.EmailTemplates.Infrastructure.Persistence;

public static class EmailTemplatesDataSeeder
{
    private static readonly (string Codigo, string Nombre, string Modulo, string[] Variables, bool UsarLayout)[] _plantillas =
    [
        ("sistema.layout",        "Layout base del sistema",              "Sistema", ["SistemaNombre","SistemaUrl","SistemaLogoUrl","AnioActual","Contenido"], false),
        ("auth.reset-password",   "Restablecimiento de contraseña",       "Auth",    ["NombreUsuario","LinkReset","ExpiraEn"],                                true),
        ("auth.cuenta-bloqueada", "Cuenta bloqueada",                     "Auth",    ["NombreUsuario","BloqueadaHasta"],                                      true),
        ("auth.bienvenida",       "Bienvenida al sistema",                "Auth",    ["NombreUsuario","LinkAcceso"],                                          true),
        ("auth.password-cambiado","Contraseña cambiada",                  "Auth",    ["NombreUsuario","FechaCambio"],                                         true),
        ("auth.2fa-activado",     "2FA activado",                         "Auth",    ["NombreUsuario","FechaActivacion"],                                     true),
    ];

    public static async Task SeedAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<EmailTemplatesDbContext>();
        var fallback = services.GetRequiredService<FallbackTemplateLoader>();

        var pendientes = await db.Database.GetPendingMigrationsAsync();
        if (pendientes.Any()) await db.Database.MigrateAsync();

        foreach (var (codigo, nombre, modulo, variables, usarLayout) in _plantillas)
        {
            // Usar IgnoreQueryFilters para que el seeder vea todas las plantillas
            // independientemente del tenant activo en el contexto
            if (await db.Plantillas.IgnoreQueryFilters()
                    .AnyAsync(t => t.Codigo == codigo && t.TenantId == null))
                continue;

            var fallbackContent = fallback.Cargar(codigo);
            if (fallbackContent is null) continue;

            var result = EmailTemplate.Crear(
                codigo, nombre, modulo,
                fallbackContent.Asunto,
                fallbackContent.CuerpoHtml,
                variables,
                usarLayout,
                esDeSistema: true);

            if (result.IsSuccess)
                await db.Plantillas.AddAsync(result.Value!);
        }

        await db.SaveChangesAsync();
    }
}
