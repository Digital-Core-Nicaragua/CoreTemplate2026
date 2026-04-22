# Guía de Estructura de Proyectos — Módulo Auth

> **Fecha:** 2026-04-15

---

## Estructura Física de Archivos

```
src/Modules/Auth/
│
├── CoreTemplate.Modules.Auth.Domain/
│   ├── Aggregates/
│   │   ├── Usuario.cs
│   │   ├── Sesion.cs
│   │   ├── Rol.cs
│   │   ├── AsignacionRol.cs      ← EnableBranches
│   │   ├── Accion.cs             ← UseActionCatalog
│   │   ├── Sucursal.cs           ← EnableBranches
│   │   └── Permiso.cs
│   ├── Entities/
│   │   ├── AuthEntities.cs       ← UsuarioRol, RolPermiso, TokenRestablecimiento, CodigoRecuperacion2FA, RegistroAuditoria
│   │   ├── UsuarioSucursal.cs    ← EnableBranches
│   │   └── ConfiguracionTenant.cs
│   ├── Enums/
│   │   ├── EstadoUsuario.cs
│   │   ├── EventoAuditoria.cs
│   │   ├── TipoUsuario.cs
│   │   └── CanalAcceso.cs
│   ├── Events/
│   │   └── AuthEvents.cs
│   ├── Repositories/
│   │   └── AuthRepositories.cs   ← Todas las interfaces
│   └── ValueObjects/
│       ├── Email.cs
│       └── PasswordHash.cs
│
├── CoreTemplate.Modules.Auth.Application/
│   ├── Abstractions/
│   │   ├── AuthSettings.cs       ← AuthSettings, LockoutSettings, PasswordPolicySettings, TokenBlacklistSettings, OrganizationSettings
│   │   └── IAuthServices.cs      ← IJwtService, IPasswordService, ITotpService, ITokenBlacklistService, ISesionService
│   ├── Commands/
│   │   ├── Login/LoginCommand.cs
│   │   ├── Logout/LogoutCommand.cs
│   │   ├── RefreshToken/RefreshTokenCommand.cs
│   │   ├── Registro/RegistrarUsuarioCommand.cs
│   │   ├── CambiarPassword/CambiarPasswordCommand.cs
│   │   ├── RestablecerPassword/RestablecerPasswordCommand.cs
│   │   ├── DosFactores/DosFactoresCommands.cs
│   │   ├── Roles/RolesCommands.cs
│   │   ├── Usuarios/UsuariosCommands.cs
│   │   ├── Sesiones/SesionesCommands.cs
│   │   ├── Sucursales/SucursalesCommands.cs    ← EnableBranches
│   │   ├── AsignacionRoles/AsignacionRolesCommands.cs  ← EnableBranches
│   │   ├── Acciones/AccionesCommands.cs        ← UseActionCatalog
│   │   └── ConfiguracionTenant/ConfigurarLimiteSesionesTenantCommand.cs
│   ├── Constants/
│   │   ├── AuthErrorMessages.cs
│   │   └── AuthSuccessMessages.cs
│   ├── DTOs/
│   │   ├── AuthDtos.cs
│   │   ├── SucursalDtos.cs
│   │   └── AccionDtos.cs
│   ├── Queries/
│   │   ├── AuthQueries.cs        ← GetUsuarioById, GetUsuarios, GetMiPerfil, GetRoles, GetRolById
│   │   ├── GetMisSesiones/
│   │   ├── GetSesionesUsuario/
│   │   ├── GetSucursales/        ← EnableBranches
│   │   ├── GetSucursalesUsuario/ ← EnableBranches
│   │   ├── GetPermisosEfectivos/
│   │   ├── GetAcciones/          ← UseActionCatalog
│   │   └── GetConfiguracionTenant/
│   ├── AssemblyInfo.cs           ← InternalsVisibleTo para tests
│   └── DependencyInjection.cs
│
├── CoreTemplate.Modules.Auth.Infrastructure/
│   ├── Middleware/
│   │   └── TokenBlacklistMiddleware.cs
│   ├── Migrations/
│   │   ├── InitialCreate_Auth
│   │   ├── Add_Sesiones_TipoUsuario
│   │   ├── Add_Sucursales
│   │   ├── Add_AsignacionesRol
│   │   ├── Add_CatalogoAcciones
│   │   ├── Add_ConfiguracionTenant
│   │   └── Add_SeveridadAuditoria
│   ├── Persistence/
│   │   ├── Configurations/
│   │   │   ├── UsuarioConfiguration.cs
│   │   │   ├── RolPermisoConfiguration.cs
│   │   │   ├── SesionConfiguration.cs
│   │   │   ├── SucursalConfiguration.cs      ← EnableBranches
│   │   │   ├── AsignacionRolConfiguration.cs ← EnableBranches
│   │   │   ├── AccionConfiguration.cs        ← UseActionCatalog
│   │   │   └── ConfiguracionTenantConfiguration.cs
│   │   ├── AuthDataSeeder.cs
│   │   ├── AuthDbContext.cs
│   │   └── AuthDbContextFactory.cs
│   ├── Repositories/
│   │   ├── UsuarioRepository.cs
│   │   ├── AuthRepositories.cs   ← RolRepository, PermisoRepository, RegistroAuditoriaRepository
│   │   ├── SesionRepository.cs
│   │   ├── SucursalRepository.cs         ← EnableBranches
│   │   ├── AsignacionRolRepository.cs    ← EnableBranches
│   │   ├── AccionRepository.cs           ← UseActionCatalog
│   │   └── ConfiguracionTenantRepository.cs
│   ├── Services/
│   │   ├── JwtService.cs
│   │   ├── PasswordService.cs
│   │   ├── TotpService.cs
│   │   ├── SesionService.cs
│   │   ├── InMemoryTokenBlacklistService.cs
│   │   └── RedisTokenBlacklistService.cs
│   ├── AssemblyInfo.cs           ← InternalsVisibleTo para tests
│   └── DependencyInjection.cs
│
└── CoreTemplate.Modules.Auth.Api/
    ├── Contracts/
    │   └── AuthContracts.cs      ← Todos los Request DTOs
    └── Controllers/
        ├── AuthController.cs
        ├── UsuariosController.cs
        ├── PerfilController.cs
        ├── RolesController.cs
        ├── SucursalesController.cs   ← EnableBranches
        ├── AccionesController.cs     ← UseActionCatalog
        └── TenantsController.cs
```

---

## Dependencias NuGet por Proyecto

### Domain
```xml
<PackageReference Include="CoreTemplate.SharedKernel" />
```

### Application
```xml
<PackageReference Include="MediatR" />
<PackageReference Include="FluentValidation" />
<PackageReference Include="FluentValidation.DependencyInjectionExtensions" />
<PackageReference Include="Microsoft.Extensions.Options" />
```

### Infrastructure
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" />
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" />
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" />
<PackageReference Include="System.IdentityModel.Tokens.Jwt" />
<PackageReference Include="BCrypt.Net-Next" />
<PackageReference Include="Otp.NET" />
<PackageReference Include="StackExchange.Redis" />
```

### Api
```xml
<PackageReference Include="CoreTemplate.Api.Common" />
```

---

**Fecha:** 2026-04-15
