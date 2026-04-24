using CoreTemplate.Modules.Configuracion.Domain.Aggregates;
using CoreTemplate.Modules.Configuracion.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CoreTemplate.Modules.Configuracion.Infrastructure.Persistence;

public static class ConfiguracionDataSeeder
{
    private static readonly (string Clave, string Valor, TipoValor Tipo, string Descripcion, string Grupo)[] _items =
    [
        // Sistema
        ("sistema.nombre",          "Mi Sistema",           TipoValor.String,  "Nombre del sistema o empresa que aparece en documentos",    "Sistema"),
        ("sistema.moneda",          "USD",                  TipoValor.String,  "Moneda principal (código ISO 4217)",                         "Sistema"),
        ("sistema.zona-horaria",    "America/Managua",      TipoValor.String,  "Zona horaria del sistema (IANA)",                            "Sistema"),
        ("sistema.fecha-formato",   "dd/MM/yyyy",           TipoValor.String,  "Formato de fechas en documentos",                            "Sistema"),
        ("sistema.logo-url",        "",                     TipoValor.String,  "URL del logo de la empresa (puede venir de Storage)",        "Sistema"),
        ("sistema.direccion",       "",                     TipoValor.String,  "Dirección física de la empresa",                             "Sistema"),
        ("sistema.telefono",        "",                     TipoValor.String,  "Teléfono de contacto",                                       "Sistema"),
        ("sistema.email-contacto",  "",                     TipoValor.String,  "Email de contacto",                                          "Sistema"),
        ("sistema.sitio-web",       "",                     TipoValor.String,  "Sitio web de la empresa",                                    "Sistema"),

        // Facturación
        ("facturacion.serie",               "001",  TipoValor.String,  "Serie de facturas",                                "Facturacion"),
        ("facturacion.numero-actual",        "0",   TipoValor.Number,  "Último número de factura usado",                   "Facturacion"),
        ("facturacion.prefijo",             "FAC-", TipoValor.String,  "Prefijo del número de factura",                    "Facturacion"),
        ("facturacion.dias-vencimiento",    "30",   TipoValor.Number,  "Días de vencimiento por defecto",                  "Facturacion"),
        ("facturacion.impuesto-porcentaje", "15",   TipoValor.Number,  "Porcentaje de impuesto por defecto",               "Facturacion"),

        // Nómina
        ("nomina.dia-pago-quincenal", "15", TipoValor.Number, "Día de pago quincenal",          "Nomina"),
        ("nomina.dia-pago-mensual",   "30", TipoValor.Number, "Día de pago mensual",             "Nomina"),
        ("nomina.horas-jornada",       "8", TipoValor.Number, "Horas de jornada laboral diaria", "Nomina"),

        // RRHH
        ("rrhh.dias-vacaciones-anuales", "15", TipoValor.Number, "Días de vacaciones por año",      "RRHH"),
        ("rrhh.meses-periodo-prueba",     "3", TipoValor.Number, "Meses de período de prueba",      "RRHH"),
    ];

    public static async Task SeedAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<ConfiguracionDbContext>();

        var pendientes = await db.Database.GetPendingMigrationsAsync();
        if (pendientes.Any()) await db.Database.MigrateAsync();

        foreach (var (clave, valor, tipo, descripcion, grupo) in _items)
        {
            if (await db.Items.IgnoreQueryFilters()
                    .AnyAsync(i => i.Clave == clave && i.TenantId == null))
                continue;

            var result = ConfiguracionItem.Crear(clave, valor, tipo, descripcion, grupo,
                esEditable: true, tenantId: null);

            if (result.IsSuccess)
                await db.Items.AddAsync(result.Value!);
        }

        await db.SaveChangesAsync();
    }
}
