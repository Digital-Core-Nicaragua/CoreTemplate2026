# Guía de Configuración — Módulo Auth

> **Fecha:** 2026-04-15

---

## appsettings.json — Configuración Completa

```json
{
  "DatabaseSettings": {
    "Provider": "SqlServer",
    "ConnectionString": "Server=localhost;Database=MiSistemaDb;User Id=sa;Password=TuPassword;TrustServerCertificate=True;"
  },
  "TenantSettings": {
    "IsMultiTenant": false,
    "TenantResolutionStrategy": "Header",
    "EnableSessionLimitsPerTenant": false
  },
  "AuthSettings": {
    "JwtSecretKey": "CAMBIAR-EN-PRODUCCION-MINIMO-256-BITS",
    "JwtIssuer": "MiSistema",
    "JwtAudience": "MiSistema",
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7,
    "TwoFactorEnabled": false,
    "TwoFactorRequired": false,
    "PasswordResetTokenExpirationHours": 1,
    "MaxSesionesSimultaneas": 5,
    "AccionAlLlegarLimiteSesiones": "CerrarMasAntigua",
    "EnableTokenBlacklist": true,
    "UseActionCatalog": false
  },
  "LockoutSettings": {
    "MaxFailedAttempts": 5,
    "LockoutDurationMinutes": 15,
    "AutoUnlock": true
  },
  "PasswordPolicy": {
    "MinLength": 8,
    "RequireUppercase": true,
    "RequireLowercase": true,
    "RequireDigit": true,
    "RequireSpecialChar": false
  },
  "TokenBlacklistSettings": {
    "Provider": "InMemory",
    "RedisConnectionString": ""
  },
  "OrganizationSettings": {
    "EnableBranches": false
  }
}
```

---

## Referencia de Flags

### IsMultiTenant

| Valor | Comportamiento |
|---|---|
| `false` (default) | Single-tenant. TenantId ignorado. Sin TenantMiddleware. |
| `true` | Filtrado automático por TenantId. Header `X-Tenant-Id` requerido. |

### EnableBranches

| Valor | Comportamiento |
|---|---|
| `false` (default) | Sin sucursales. Roles globales por usuario. Sin claim `branch_id`. |
| `true` | Sucursales activas. Roles por sucursal. JWT incluye `branch_id`. |

**Cuando se activa por primera vez:**
```bash
dotnet ef migrations add Add_Sucursales \
  --project src/Modules/Auth/MiSistema.Modules.Auth.Infrastructure \
  --startup-project src/Host/MiSistema.Api \
  --context AuthDbContext
dotnet ef database update ...
```

### UseActionCatalog

| Valor | Comportamiento |
|---|---|
| `false` (default) | Permisos como strings `Modulo.Recurso.Accion`. |
| `true` | `Accion` como aggregate gestionable. Endpoints `/api/acciones` activos. |

### EnableTokenBlacklist

| Valor | Comportamiento |
|---|---|
| `false` | Sin blacklist. Logout no invalida el AccessToken. |
| `true` (default) | Blacklist activa. Logout invalida el AccessToken inmediatamente. |

### TokenBlacklistSettings.Provider

| Valor | Cuándo usar |
|---|---|
| `InMemory` (default) | Desarrollo o un solo servidor. Se pierde al reiniciar. |
| `Redis` | Producción con múltiples instancias. Requiere `RedisConnectionString`. |

### AccionAlLlegarLimiteSesiones

| Valor | Comportamiento |
|---|---|
| `CerrarMasAntigua` (default) | Cierra la sesión con menor `UltimaActividad`. |
| `BloquearNuevoLogin` | Rechaza el nuevo login con error descriptivo. |

### TwoFactorEnabled / TwoFactorRequired

| TwoFactorEnabled | TwoFactorRequired | Comportamiento |
|---|---|---|
| `false` | — | 2FA completamente deshabilitado |
| `true` | `false` | 2FA opcional por usuario |
| `true` | `true` | 2FA obligatorio para todos los usuarios Humano |

---

## Configuración para Producción

```json
{
  "AuthSettings": {
    "JwtSecretKey": "USAR-VARIABLE-DE-ENTORNO-O-KEY-VAULT",
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7,
    "EnableTokenBlacklist": true
  },
  "TokenBlacklistSettings": {
    "Provider": "Redis",
    "RedisConnectionString": "redis-server:6379,password=TuPassword"
  }
}
```

**Nunca hardcodear `JwtSecretKey` en producción.** Usar variables de entorno o Azure Key Vault:
```bash
# Variable de entorno
AuthSettings__JwtSecretKey=mi-clave-secreta-de-256-bits
```

---

## Configuración para Desarrollo

```json
{
  "AuthSettings": {
    "JwtSecretKey": "dev-secret-key-change-in-production-must-be-256-bits",
    "AccessTokenExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 30,
    "TwoFactorEnabled": true,
    "EnableTokenBlacklist": true
  },
  "TokenBlacklistSettings": {
    "Provider": "InMemory"
  }
}
```

---

## Registro de Dependencias (DependencyInjection.cs)

El módulo Auth registra sus dependencias en dos métodos:

```csharp
// Program.cs
builder.Services.AddAuthApplication(builder.Configuration);
builder.Services.AddAuthInfrastructure(builder.Configuration);
```

**AddAuthApplication** registra:
- MediatR con ValidationBehavior
- FluentValidation
- Settings: AuthSettings, LockoutSettings, PasswordPolicySettings, OrganizationSettings

**AddAuthInfrastructure** registra:
- AuthDbContext (SqlServer o PostgreSQL según Provider)
- Repositorios (siempre): IUsuarioRepository, IRolRepository, IPermisoRepository, ISesionRepository, IRegistroAuditoriaRepository, IConfiguracionTenantRepository
- Repositorios condicionales:
  - `EnableBranches = true` → ISucursalRepository, IAsignacionRolRepository
  - `UseActionCatalog = true` → IAccionRepository
- Servicios: IJwtService, IPasswordService, ISesionService, ITotpService
- Token Blacklist: InMemoryTokenBlacklistService o RedisTokenBlacklistService según Provider
- Autenticación JWT

---

## Middleware (Program.cs)

```csharp
// Orden correcto del pipeline
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    await app.SeedDatabaseAsync(); // Ejecuta AuthDataSeeder
}

// TenantMiddleware solo si IsMultiTenant = true
if (builder.Configuration.GetValue<bool>("TenantSettings:IsMultiTenant"))
    app.UseMiddleware<TenantMiddleware>();

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseAuthentication();

// TokenBlacklistMiddleware solo si EnableTokenBlacklist = true
if (builder.Configuration.GetValue<bool>("AuthSettings:EnableTokenBlacklist"))
    app.UseMiddleware<TokenBlacklistMiddleware>();

app.UseAuthorization();
app.MapControllers();
```

---

**Fecha:** 2026-04-15
