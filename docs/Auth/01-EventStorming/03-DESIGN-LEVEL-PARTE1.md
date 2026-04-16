# Event Storming — Design Level Parte 1

> **Aggregates:** Usuario, Sesion  
> **Bounded Context:** IAM  
> **Fecha:** 2026-04-15

---

## 🟡 AGGREGATE: Usuario

```
┌──────────────────────────────────────────────────────────────┐
│ 🟡 USUARIO (Aggregate Root)                                 │
├──────────────────────────────────────────────────────────────┤
│ Identidad:                                                   │
│   Id: Guid                                                   │
│   TenantId: Guid? (null si single-tenant)                   │
│                                                              │
│ Value Objects:                                               │
│   Email: Email (formato válido, normalizado)                 │
│   PasswordHash: PasswordHash (BCrypt, nunca texto plano)    │
│                                                              │
│ Propiedades:                                                 │
│   Nombre: string (max 100)                                   │
│   TipoUsuario: Humano | Sistema | Integracion               │
│   Estado: Pendiente | Activo | Inactivo | Bloqueado         │
│   IntentosFallidos: int                                      │
│   BloqueadoHasta: DateTime?                                  │
│   TwoFactorActivo: bool                                      │
│   TwoFactorSecretKey: string? (encriptado en BD)            │
│   UltimoAcceso: DateTime?                                    │
│   CreadoEn: DateTime                                         │
│   ModificadoEn: DateTime?                                    │
│                                                              │
│ Colecciones (backing fields):                                │
│   _roles: List<UsuarioRol>                                   │
│   _sucursales: List<UsuarioSucursal>                         │
│   _tokensRestablecimiento: List<TokenRestablecimiento>       │
│   _codigosRecuperacion: List<CodigoRecuperacion2FA>          │
├──────────────────────────────────────────────────────────────┤
│ Comandos que procesa:                                        │
│   Crear(email, nombre, hash, tenantId?, tipoUsuario)        │
│   Activar()                                                  │
│   Desactivar()                                               │
│   Bloquear(hasta: DateTime)                                  │
│   Desbloquear()                                              │
│   IncrementarIntentosFallidos(maxIntentos, minutos)         │
│   ResetearIntentosFallidos()                                 │
│   CambiarPassword(nuevoHash)                                 │
│   RegistrarAcceso()                                          │
│   AsignarRol(rolId)                                          │
│   QuitarRol(rolId)                                           │
│   AsignarSucursal(sucursalId)                                │
│   RemoverSucursal(sucursalId)                                │
│   CambiarSucursalPrincipal(sucursalId)                       │
│   ActivarDosFactores(secretKey, codigosHash[])              │
│   GuardarSecretKeyTemporal(secretKey)                        │
│   DesactivarDosFactores()                                    │
│   UsarCodigoRecuperacion(codigoHash) → bool                 │
│   PuedeAutenticarse() → bool                                 │
├──────────────────────────────────────────────────────────────┤
│ Eventos que emite:                                           │
│   🟠 UsuarioRegistradoEvent                                  │
│   🟠 UsuarioActivadoEvent                                    │
│   🟠 UsuarioDesactivadoEvent                                 │
│   🟠 UsuarioBloqueadoEvent                                   │
│   🟠 UsuarioDesbloqueadoEvent                                │
│   🟠 PasswordCambiadoEvent                                   │
│   🟠 RestablecimientoSolicitadoEvent                         │
│   🟠 DosFactoresActivadoEvent                                │
│   🟠 DosFactoresDesactivadoEvent                             │
│   🟠 SucursalAsignadaEvent                                   │
│   🟠 SucursalRemovidaEvent                                   │
└──────────────────────────────────────────────────────────────┘
```

### Entidades Hijas del Aggregate Usuario

```
┌─────────────────────────────────────┐
│ UsuarioRol (Entity)                 │
├─────────────────────────────────────┤
│ Id: Guid                            │
│ UsuarioId: Guid                     │
│ RolId: Guid                         │
│ AsignadoEn: DateTime                │
│                                     │
│ Crear(usuarioId, rolId) → static   │
└─────────────────────────────────────┘

┌─────────────────────────────────────┐
│ UsuarioSucursal (Entity)            │
├─────────────────────────────────────┤
│ Id: Guid                            │
│ UsuarioId: Guid                     │
│ SucursalId: Guid                    │
│ EsPrincipal: bool                   │
│ AsignadoEn: DateTime                │
│                                     │
│ Crear(usuarioId, sucursalId, bool)  │
│ MarcarComoPrincipal()               │
│ QuitarPrincipal()                   │
└─────────────────────────────────────┘

┌─────────────────────────────────────┐
│ TokenRestablecimiento (Entity)      │
├─────────────────────────────────────┤
│ Id: Guid                            │
│ UsuarioId: Guid                     │
│ Token: string (Base64, 64 bytes)    │
│ ExpiraEn: DateTime                  │
│ EsUsado: bool                       │
│ UsadoEn: DateTime?                  │
│ CreadoEn: DateTime                  │
│ EsValido: bool (computed)           │
│                                     │
│ Crear(usuarioId, token, horas)      │
│ Usar()                              │
└─────────────────────────────────────┘

┌─────────────────────────────────────┐
│ CodigoRecuperacion2FA (Entity)      │
├─────────────────────────────────────┤
│ Id: Guid                            │
│ UsuarioId: Guid                     │
│ CodigoHash: string (SHA256)         │
│ EsUsado: bool                       │
│ UsadoEn: DateTime?                  │
│ CreadoEn: DateTime                  │
│                                     │
│ Crear(usuarioId, codigoHash)        │
│ Usar()                              │
└─────────────────────────────────────┘
```

### Invariantes del Aggregate Usuario

| # | Invariante | Implementación |
|---|---|---|
| 1 | Email único por tenant | Validado en repositorio antes de crear |
| 2 | Nombre requerido, max 100 chars | Validado en `Crear()` |
| 3 | Usuario bloqueado no puede autenticarse | `PuedeAutenticarse()` |
| 4 | Usuario inactivo/pendiente no puede autenticarse | `PuedeAutenticarse()` |
| 5 | Bloqueo con fecha pasada → desbloqueo automático | `PuedeAutenticarse()` |
| 6 | Solo una sucursal puede ser principal | `AsignarSucursal()`, `CambiarSucursalPrincipal()` |
| 7 | Usuario debe tener al menos una sucursal | `RemoverSucursal()` |
| 8 | Al remover principal → asignar siguiente | `RemoverSucursal()` |
| 9 | Usuario debe tener al menos un rol | `QuitarRol()` |
| 10 | No duplicar mismo rol | `AsignarRol()` |
| 11 | No duplicar misma sucursal | `AsignarSucursal()` |
| 12 | SuperAdmin no puede desactivarse | Handler `DesactivarUsuarioCommandHandler` |
| 13 | Permisos SuperAdmin no modificables | Handler `QuitarRolCommandHandler` |
| 14 | Token restablecimiento de un solo uso | `UsarTokenRestablecimiento()` |
| 15 | Códigos recuperación 2FA de un solo uso | `UsarCodigoRecuperacion()` |
| 16 | Sistema/Integracion no requieren 2FA | `LoginCommandHandler` |
| 17 | Sistema/Integracion no tienen límite sesiones | `SesionService` |
| 18 | Bloqueo solo aplica a Humano | `IncrementarIntentosFallidos()` |

---

## 🟡 AGGREGATE: Sesion

```
┌──────────────────────────────────────────────────────────────┐
│ 🟡 SESION (Aggregate Root)                                  │
├──────────────────────────────────────────────────────────────┤
│ Identidad:                                                   │
│   Id: Guid                                                   │
│   UsuarioId: Guid                                            │
│   TenantId: Guid?                                            │
│                                                              │
│ Propiedades:                                                 │
│   RefreshTokenHash: string (SHA256 hex, 64 chars)           │
│   Canal: Web | Mobile | Api | Desktop                       │
│   Dispositivo: string (max 200)                              │
│   Ip: string (max 50)                                        │
│   UserAgent: string (max 500)                                │
│   UltimaActividad: DateTime                                  │
│   ExpiraEn: DateTime                                         │
│   CreadoEn: DateTime                                         │
│   EsActiva: bool                                             │
│                                                              │
│ Computed:                                                    │
│   EsValida: EsActiva && UtcNow < ExpiraEn                   │
├──────────────────────────────────────────────────────────────┤
│ Comandos que procesa:                                        │
│   Crear(usuarioId, tenantId, hash, expira, canal, ip, ua)   │
│   Renovar(nuevoHash, nuevaExpiracion)                        │
│   Revocar()                                                  │
├──────────────────────────────────────────────────────────────┤
│ Eventos que emite:                                           │
│   🟠 SesionRevocadaEvent                                     │
│   🟠 TodasSesionesRevocadasEvent                             │
└──────────────────────────────────────────────────────────────┘
```

### Invariantes del Aggregate Sesion

| # | Invariante | Implementación |
|---|---|---|
| 1 | RefreshToken almacenado como hash SHA256 | `LoginCommandHandler.ComputarHash()` |
| 2 | Sesión expirada no es válida aunque esté activa | `EsValida` computed property |
| 3 | Sesión revocada no puede reactivarse | `Revocar()` solo pone `EsActiva = false` |
| 4 | RefreshToken de un solo uso (rotación) | `Sesion.Renovar()` reemplaza el hash |
| 5 | Límite de sesiones simultáneas | `ISesionService.VerificarYAplicarLimite()` |
| 6 | Sistema/Integracion sin límite | `SesionService` verifica `TipoUsuario` |
| 7 | Al cambiar password → revocar todas | `CambiarPasswordCommandHandler` |
| 8 | Al restablecer password → revocar todas | `RestablecerPasswordCommandHandler` |
| 9 | Al logout → AccessToken a blacklist | `LogoutCommandHandler` |
| 10 | TTL blacklist = tiempo restante del token | `LogoutCommandHandler.ComputarTtl()` |

---

## Repositorios del Bounded Context IAM

```csharp
// IUsuarioRepository — operaciones de lectura y escritura
GetByIdAsync(Guid id)                          // Incluye roles, sucursales, tokens
GetByEmailAsync(string email, Guid? tenantId)
ExistsByEmailAsync(string email, Guid? tenantId)
GetByTokenRestablecimientoAsync(string token)
GetPagedAsync(pagina, tamanoPagina, estado?)
AddAsync(Usuario usuario)
UpdateAsync(Usuario usuario)

// ISesionRepository — operaciones de sesiones
GetByIdAsync(Guid id)
GetActivaByRefreshTokenHashAsync(string hash)
GetActivasByUsuarioAsync(Guid usuarioId)
ContarActivasAsync(Guid usuarioId)
GetMasAntiguaActivaAsync(Guid usuarioId)
AddAsync(Sesion sesion)
UpdateAsync(Sesion sesion)
RevocarTodasAsync(Guid usuarioId)
LimpiarExpiradosAsync(int diasAntiguedad)
```

---

**Estado:** ✅ Completo  
**Fecha:** 2026-04-15
