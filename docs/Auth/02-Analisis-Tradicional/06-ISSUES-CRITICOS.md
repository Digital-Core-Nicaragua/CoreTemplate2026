# Issues Críticos — Módulo Auth

> **Total:** 6 issues críticos — todos resueltos ✅  
> **Fecha:** 2026-04-15

---

## IC-001: Invalidación Inmediata de Tokens JWT

**Problema:** JWT es stateless. Un token válido no puede invalidarse sin infraestructura adicional. Al hacer logout, el token sigue siendo válido hasta su expiración natural.

**Impacto:** Alto — riesgo de seguridad si un token es robado después del logout.

**Solución implementada:** Token Blacklist con backend configurable.
- `InMemory`: `ConcurrentDictionary<string, DateTime>` con limpieza automática por TTL.
- `Redis`: `SET jti "1" EX {ttl}` — TTL gestionado por Redis automáticamente.
- `TokenBlacklistMiddleware` extrae el JTI del token y verifica la blacklist en cada request.
- TTL = `expiracion - DateTime.UtcNow` — cuando el token expira naturalmente, la entrada también expira.

```csharp
// Middleware ejecuta antes de UseAuthorization
app.UseAuthentication();
if (config.GetValue<bool>("AuthSettings:EnableTokenBlacklist"))
    app.UseMiddleware<TokenBlacklistMiddleware>();
app.UseAuthorization();
```

---

## IC-002: Seguridad del Refresh Token en Base de Datos

**Problema:** Almacenar el refresh token en texto plano en la BD es un riesgo. Si la BD es comprometida, todos los tokens son utilizables.

**Solución implementada:** Hash SHA256 del token.
```csharp
private static string ComputarHash(string valor)
{
    var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(valor));
    return Convert.ToHexString(bytes).ToLowerInvariant();
}
```
- El token se genera como 64 bytes aleatorios → Base64
- Se almacena `SHA256(token)` en `Sesion.RefreshTokenHash`
- Al verificar: `SHA256(tokenRecibido)` y comparar con el hash almacenado

---

## IC-003: Flujo de 2FA sin Romper la Sesión

**Problema:** El login con 2FA requiere dos requests. ¿Cómo mantener el estado entre el primer y segundo paso sin crear una sesión prematura?

**Solución implementada:** Token temporal JWT de 5 minutos.
- Paso 1: Login exitoso con 2FA → `JwtService.GenerarTokenTemporal2FA(usuarioId)` → JWT con claim `tipo: "2fa_temp"`, expira en 5 min
- Paso 2: `POST /2fa/verificar { tokenTemporal, codigo }` → `JwtService.ValidarTokenTemporal2FA(token)` → retorna `usuarioId` si válido
- Si el token temporal expira → usuario debe hacer login nuevamente
- No se crea sesión hasta que el código TOTP sea verificado

---

## IC-004: Límites de Sesiones con Múltiples Niveles de Configuración

**Problema:** El límite de sesiones puede venir de 3 fuentes distintas (tenant, global, default). ¿Cómo implementar la jerarquía sin acoplar el dominio a la infraestructura?

**Solución implementada:** `ISesionService` como domain service con jerarquía encapsulada.
```csharp
private async Task<int> ObtenerLimiteAsync(CancellationToken ct)
{
    if (_tenantSettings.Value.IsMultiTenant
        && _tenantSettings.Value.EnableSessionLimitsPerTenant
        && _currentTenant.TenantId.HasValue)
    {
        var config = await _configTenantRepo.GetByTenantIdAsync(_currentTenant.TenantId.Value, ct);
        if (config?.MaxSesionesSimultaneas.HasValue == true)
            return config.MaxSesionesSimultaneas.Value;
    }
    return _authSettings.Value.MaxSesionesSimultaneas; // default: 5
}
```

---

## IC-005: Features Opcionales sin Código Condicional en el Dominio

**Problema:** Sucursales, catálogo de acciones y límites por tenant son opcionales. ¿Cómo implementarlos sin llenar el código de `if (EnableBranches)` en todas partes?

**Solución implementada:** Registro condicional en DI + verificación en handlers.
```csharp
// DependencyInjection.cs — solo registra si está habilitado
if (configuration.GetValue<bool>("OrganizationSettings:EnableBranches"))
{
    services.AddScoped<ISucursalRepository, SucursalRepository>();
    services.AddScoped<IAsignacionRolRepository, AsignacionRolRepository>();
}

// Handler — verifica el flag antes de operar
if (!_orgSettings.Value.EnableBranches)
    return Result.Failure("Las sucursales no están habilitadas.");
```
Los aggregates opcionales (`Sucursal`, `AsignacionRol`, `Accion`) siempre existen en el dominio pero sus repositorios solo se registran cuando el flag está activo.

---

## IC-006: Comportamiento Diferenciado por TipoUsuario

**Problema:** `Sistema` e `Integracion` no deben tener límite de sesiones, bloqueo por intentos ni 2FA. ¿Cómo implementarlo sin duplicar lógica?

**Solución implementada:** Verificaciones explícitas en los handlers y servicios.
```csharp
// LoginCommandHandler — no bloquear Sistema/Integracion
if (usuario.TipoUsuario == TipoUsuario.Humano)
{
    usuario.IncrementarIntentosFallidos(lockout.MaxFailedAttempts, lockout.LockoutDurationMinutes);
}

// SesionService — eximir de límite
if (tipoUsuario != TipoUsuario.Humano)
    return true; // siempre permitido

// LoginCommandHandler — no requerir 2FA
if (usuario.TwoFactorActivo && usuario.TipoUsuario == TipoUsuario.Humano)
{
    // flujo 2FA
}
```

---

**Estado:** ✅ Todos los issues resueltos  
**Fecha:** 2026-04-15
