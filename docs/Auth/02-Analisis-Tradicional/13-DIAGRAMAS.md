# Diagramas — Módulo Auth

> **Fecha:** 2026-04-15

---

## Diagrama 1: Flujo de Login Normal

```
Usuario          AuthController      LoginHandler        Usuario(Agg)    Sesion(Agg)    JwtService
  │                    │                   │                  │               │               │
  │──POST /login──────>│                   │                  │               │               │
  │                    │──Send(LoginCmd)──>│                  │               │               │
  │                    │                   │──GetByEmail()───>│               │               │
  │                    │                   │<─────────────────│               │               │
  │                    │                   │──PuedeAutenticar()               │               │
  │                    │                   │──VerifyPassword()                │               │
  │                    │                   │──ResetearIntentos()              │               │
  │                    │                   │──VerificarLimite()──────────────>│               │
  │                    │                   │──Sesion.Crear()─────────────────>│               │
  │                    │                   │──GenerarAccessToken()────────────────────────────>│
  │                    │                   │<─────────────────────────────────────────────────│
  │                    │<──Result<object>──│                  │               │               │
  │<───200 + tokens────│                   │                  │               │               │
```

---

## Diagrama 2: Flujo de Login con 2FA

```
Usuario          AuthController      LoginHandler        Verificar2FAHandler
  │                    │                   │                      │
  │──POST /login──────>│                   │                      │
  │                    │──Send(LoginCmd)──>│                      │
  │                    │                   │── (credenciales OK)  │
  │                    │                   │── TwoFactorActivo=true│
  │                    │                   │── GenerarTokenTemporal│
  │<──200 {requires2FA,│                   │                      │
  │   tokenTemporal}───│                   │                      │
  │                    │                   │                      │
  │──POST /2fa/verificar────────────────────────────────────────>│
  │  {tokenTemporal,   │                   │                      │
  │   codigo}          │                   │                      │
  │                    │                   │                      │──ValidarTokenTemporal
  │                    │                   │                      │──ValidarCodigoTOTP
  │                    │                   │                      │──CrearSesion
  │                    │                   │                      │──GenerarTokens
  │<──200 + tokens──────────────────────────────────────────────│
```

---

## Diagrama 3: Estados del Usuario

```
                    ┌─────────────┐
                    │  PENDIENTE  │
                    └──────┬──────┘
                           │ Activar()
                           ▼
              ┌────────────────────────┐
              │         ACTIVO         │◄──────────────────┐
              └────────────────────────┘                   │
                    │           │                          │
          Desactivar()    IncrementarIntentos()            │
                    │     (>= MaxIntentos)                 │
                    ▼           ▼                          │
              ┌──────────┐ ┌──────────┐                   │
              │ INACTIVO │ │BLOQUEADO │──Desbloquear()────►│
              └──────────┘ └──────────┘
                                │
                    BloqueadoHasta < UtcNow
                    (desbloqueo automático en
                     PuedeAutenticarse())
```

---

## Diagrama 4: Jerarquía de Límites de Sesiones

```
┌─────────────────────────────────────────────────────────┐
│              VERIFICAR LÍMITE DE SESIONES               │
├─────────────────────────────────────────────────────────┤
│                                                         │
│  TipoUsuario == Sistema | Integracion?                  │
│  ├── SÍ → Permitir siempre (sin límite)                │
│  └── NO → Continuar                                     │
│                                                         │
│  IsMultiTenant=true AND EnableSessionLimitsPerTenant=true│
│  AND TenantId != null?                                  │
│  ├── SÍ → Consultar ConfiguracionTenant.MaxSesiones     │
│  │        ├── != null → usar ese límite                 │
│  │        └── null → siguiente nivel                    │
│  └── NO → siguiente nivel                               │
│                                                         │
│  AuthSettings.MaxSesionesSimultaneas (default: 5)       │
│                                                         │
│  Sesiones activas >= límite?                            │
│  ├── NO → Permitir nueva sesión                         │
│  └── SÍ → AccionAlLlegarLimiteSesiones?                 │
│           ├── CerrarMasAntigua → Revocar + Permitir     │
│           └── BloquearNuevoLogin → Rechazar             │
└─────────────────────────────────────────────────────────┘
```

---

## Diagrama 5: Arquitectura de Capas

```
┌─────────────────────────────────────────────────────────┐
│                      API LAYER                          │
│  AuthController  UsuariosController  PerfilController   │
│  RolesController SucursalesController AccionesController│
└──────────────────────────┬──────────────────────────────┘
                           │ MediatR
┌──────────────────────────▼──────────────────────────────┐
│                  APPLICATION LAYER                      │
│  Commands: Login, Logout, Refresh, CambiarPassword...   │
│  Queries: GetMiPerfil, GetSesiones, GetRoles...         │
│  Services: ISesionService, ITokenBlacklistService       │
└──────────────────────────┬──────────────────────────────┘
                           │ Interfaces
┌──────────────────────────▼──────────────────────────────┐
│                    DOMAIN LAYER                         │
│  Aggregates: Usuario, Sesion, Rol, Sucursal...          │
│  ValueObjects: Email, PasswordHash                      │
│  Events: UsuarioRegistrado, SesionRevocada...           │
└──────────────────────────┬──────────────────────────────┘
                           │ Implementaciones
┌──────────────────────────▼──────────────────────────────┐
│                INFRASTRUCTURE LAYER                     │
│  AuthDbContext (schema Auth)                            │
│  Repositories: UsuarioRepository, SesionRepository...  │
│  Services: JwtService, PasswordService, TotpService     │
│  TokenBlacklist: InMemory / Redis                       │
│  Migrations: 6 migraciones EF Core                     │
└─────────────────────────────────────────────────────────┘
```

---

## Diagrama 6: Token Blacklist

```
POST /logout
    │
    ├── Revocar Sesion (RefreshToken)
    │
    └── Si EnableTokenBlacklist = true:
            │
            ├── Extraer JTI del AccessToken
            ├── Calcular TTL = expiracion - UtcNow
            │
            ├── Provider = InMemory:
            │       ConcurrentDictionary[jti] = expiracion
            │
            └── Provider = Redis:
                    SET "token_blacklist:{jti}" "1" EX {ttl}

Cada request con Bearer token:
    │
    └── TokenBlacklistMiddleware:
            ├── Extraer JTI del token
            ├── EstaEnBlacklist(jti)?
            │   ├── SÍ → HTTP 401 "Token revocado"
            │   └── NO → Continuar
            └── UseAuthentication → UseAuthorization
```

---

## Diagrama 7: Estructura de Sucursales (EnableBranches = true)

```
Usuario
  ├── UsuarioSucursal { SucursalId: A, EsPrincipal: true }
  ├── UsuarioSucursal { SucursalId: B, EsPrincipal: false }
  └── UsuarioSucursal { SucursalId: C, EsPrincipal: false }

JWT claims:
  branch_id: A  (sucursal principal o la activa seleccionada)

AsignacionRol (roles por sucursal):
  { UsuarioId, SucursalId: A, RolId: Admin }
  { UsuarioId, SucursalId: B, RolId: Operativo }
  { UsuarioId, SucursalId: C, RolId: Vendedor }

Permisos efectivos en Sucursal A:
  → Roles de AsignacionRol donde SucursalId = branch_id del JWT
  → Unión de permisos de todos esos roles
```

---

## Diagrama 8: Flujo de Configuración Multi-tenant

```
appsettings.json
  TenantSettings:
    IsMultiTenant: true
    TenantResolutionStrategy: Header
    EnableSessionLimitsPerTenant: true

Request HTTP:
  Header: X-Tenant-Id: {guid}
      │
      ▼
  TenantMiddleware → ICurrentTenant.TenantId = guid
      │
      ▼
  BaseDbContext → QueryFilter: WHERE TenantId = guid
      │
      ▼
  SesionService → ConfiguracionTenant.MaxSesiones (para ese tenant)
```

---

**Fecha:** 2026-04-15
