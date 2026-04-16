# CoreTemplate — Plan de Implementación

> **Convención de estado:**
> - `[ ]` — Pendiente
> - `[x]` — Completado
> - `[-]` — En progreso
> - `[~]` — Omitido / Pospuesto

---

## RESUMEN GENERAL

| Fase | Descripción | Estado |
|---|---|---|
| 0 | Documentación | ✅ Completa |
| 1 | Solución y proyectos base | ✅ Completa |
| 2 | SharedKernel | ✅ Completa |
| 3 | Api.Common | ✅ Completa |
| 4 | Infrastructure base | ✅ Completa |
| 5 | Auth — Domain | ✅ Completa |
| 6 | Auth — Application | ✅ Completa |
| 7 | Auth — Infrastructure | ✅ Completa |
| 8 | Auth — Api | ✅ Completa |
| 9 | Módulo Catálogos completo | ✅ Completa |
| 10 | Host final | ✅ Completa |
| 11 | Script de renombrado | ✅ Completa |
| 12 | Tests | ✅ Completa |
| 13 | README final | ✅ Completa |

---

## FASE 0 — Documentación ✅

- [x] `docs/ALCANCE.md`
- [x] `docs/PLAN-IMPLEMENTACION.md`
- [x] `docs/architecture/ARQUITECTURA.md`
- [x] `docs/architecture/CONVENCIONES.md`
- [x] `docs/analysis/Auth/REQUISITOS.md`
- [x] `docs/analysis/Auth/CASOS-DE-USO.md`
- [x] `docs/analysis/Auth/MODELO-DATOS.md`
- [x] `docs/analysis/Catalogos/REQUISITOS-Y-CASOS-DE-USO.md`
- [x] `docs/diagrams/arquitectura.puml`
- [x] `docs/diagrams/auth-flow.puml`
- [x] `docs/diagrams/modelo-datos.puml`
- [x] `docs/Auth/README.md` — README principal del módulo Auth
- [x] `docs/Auth/01-EventStorming/README.md` — Leyenda y estructura Event Storming
- [x] `docs/Auth/02-Analisis-Tradicional/00-CHECKLIST-VALIDACION.md`
- [x] `docs/Auth/02-Analisis-Tradicional/01-CASOS-DE-USO-00-INDICE.md` — 45 casos de uso
- [x] `docs/Auth/02-Analisis-Tradicional/02-MODELO-DOMINIO.md` — 7 aggregates, 51+ invariantes
- [x] `docs/Auth/02-Analisis-Tradicional/04-REGLAS-NEGOCIO.md` — 30 reglas
- [x] `docs/Auth/02-Analisis-Tradicional/07-REQUERIMIENTOS-FUNCIONALES.md` — 22 RF
- [x] `docs/Auth/02-Analisis-Tradicional/09-GLOSARIO.md` — Ubiquitous Language
- [x] `docs/Auth/01-EventStorming/01-BIG-PICTURE.md`
- [x] `docs/Auth/01-EventStorming/02-PROCESS-LEVEL-PARTE1.md`
- [x] `docs/Auth/01-EventStorming/02-PROCESS-LEVEL-PARTE2.md`
- [x] `docs/Auth/01-EventStorming/02-PROCESS-LEVEL-PARTE3.md`
- [x] `docs/Auth/01-EventStorming/03-DESIGN-LEVEL-PARTE1.md`
- [x] `docs/Auth/01-EventStorming/03-DESIGN-LEVEL-PARTE2.md`
- [x] `docs/Auth/01-EventStorming/03-DESIGN-LEVEL-PARTE3.md`
- [x] `docs/Auth/01-EventStorming/04-BOUNDED-CONTEXT-CANVAS.md`
- [x] `docs/Auth/01-EventStorming/05-CONTEXT-MAPPING.md`
- [x] `docs/Auth/01-EventStorming/06-EVENT-STORMING-LEGEND.md`
- [x] `docs/Auth/01-EventStorming/07-HOTSPOTS-RESOLUTION.md`
- [x] `docs/Auth/02-Analisis-Tradicional/01-CASOS-DE-USO-01-AUTENTICACION.md`
- [x] `docs/Auth/02-Analisis-Tradicional/01-CASOS-DE-USO-02-SESIONES.md`
- [x] `docs/Auth/02-Analisis-Tradicional/01-CASOS-DE-USO-03-AUTORIZACION.md`
- [x] `docs/Auth/02-Analisis-Tradicional/01-CASOS-DE-USO-05-CONFIGURACION.md`
- [x] `docs/Auth/02-Analisis-Tradicional/03-EVENTOS-DOMINIO.md`
- [x] `docs/Auth/02-Analisis-Tradicional/05-CONTRATOS-API.md`
- [x] `docs/Auth/02-Analisis-Tradicional/06-ISSUES-CRITICOS.md`
- [x] `docs/Auth/02-Analisis-Tradicional/08-REQUERIMIENTOS-NO-FUNCIONALES.md`
- [x] `docs/Auth/02-Analisis-Tradicional/10-MODELO-DATOS.md`
- [x] `docs/Auth/02-Analisis-Tradicional/11-ARQUITECTURA.md`
- [x] `docs/Auth/02-Analisis-Tradicional/12-TESTING.md`
- [x] `docs/Auth/02-Analisis-Tradicional/13-DIAGRAMAS.md`
- [x] `docs/Auth/03-Implementacion/01-ESTRUCTURA-PROYECTOS.md`
- [x] `docs/Auth/03-Implementacion/02-GUIA-AGGREGATES.md`
- [x] `docs/Auth/03-Implementacion/03-GUIA-CONFIGURACION.md`
- [x] `docs/Auth/03-Implementacion/04-GUIA-MIGRACION.md`
- [ ] `README.md` raíz

---

## FASE 1 — Solución y proyectos base ✅

> Compilación verificada: ✅ 0 errores.

- [x] `CoreTemplate.sln` — 15 proyectos en carpetas de solución
- [x] `Directory.Packages.props` — versiones NuGet centralizadas
- [x] `.editorconfig`
- [x] `.gitignore`
- [x] Todos los `.csproj` con referencias correctas
- [x] `appsettings.json` + `appsettings.Development.json` + `launchSettings.json`
- [x] `Program.cs` — pipeline completo
- [x] `Extensions/ApplicationSeederExtension.cs`

---

## FASE 2 — SharedKernel ✅

> Compilación verificada: ✅ 0 errores.

- [x] `IDomainEvent.cs`
- [x] `Result.cs` — `Result` y `Result<T>`
- [x] `PagedResult.cs`
- [x] `Domain/Entity.cs`
- [x] `Domain/AggregateRoot.cs`
- [x] `Domain/ValueObject.cs`
- [x] `Constants/CommonErrorMessages.cs`
- [x] `Constants/CommonSuccessMessages.cs`

---

## FASE 3 — Api.Common ✅

> Compilación verificada: ✅ 0 errores.

- [x] `ApiResponse.cs`
- [x] `BaseApiController.cs` — 10 métodos helper
- [x] `GlobalExceptionHandler.cs`
- [x] `Behaviors/ValidationBehavior.cs`

---

## FASE 4 — Infrastructure base ✅

> Compilación verificada: ✅ 0 errores.

- [x] `Services/ICurrentUser.cs` + `CurrentUserService.cs`
- [x] `Services/ICurrentTenant.cs` + `CurrentTenantService.cs`
- [x] `Settings/TenantSettings.cs`
- [x] `Settings/DatabaseSettings.cs`
- [x] `Persistence/IHasTenant.cs`
- [x] `Persistence/BaseDbContext.cs`
- [x] `Middleware/TenantMiddleware.cs`
- [x] `DependencyInjection.cs` — `AddInfrastructureBase()`

---

## FASE 5 — Auth Domain ✅

> Compilación verificada: ✅ 0 errores.

- [x] `Enums/EstadoUsuario.cs` + `EventoAuditoria.cs`
- [x] `ValueObjects/Email.cs` + `PasswordHash.cs`
- [x] `Entities/AuthEntities.cs` — UsuarioRol, RolPermiso, RefreshToken, TokenRestablecimiento, CodigoRecuperacion2FA, RegistroAuditoria
- [x] `Events/AuthEvents.cs` — 13 eventos
- [x] `Aggregates/Usuario.cs` — 15+ métodos
- [x] `Aggregates/Rol.cs`
- [x] `Aggregates/Permiso.cs`
- [x] `Repositories/AuthRepositories.cs` — 5 interfaces

---

## FASE 6 — Auth Application ✅

> Compilación verificada: ✅ 0 errores.

- [x] `Constants/AuthErrorMessages.cs` + `AuthSuccessMessages.cs`
- [x] `Abstractions/IAuthServices.cs` — IJwtService, IPasswordService, ITotpService
- [x] `Abstractions/AuthSettings.cs` — AuthSettings, LockoutSettings, PasswordPolicySettings
- [x] `DTOs/AuthDtos.cs` — 8 DTOs
- [x] 19 Commands + Handlers (Login, Registro, RefreshToken, Logout, CambiarPassword, SolicitarRestablecimiento, RestablecerPassword, Activar2FA, Confirmar2FA, Verificar2FA, Desactivar2FA, ActivarUsuario, DesactivarUsuario, DesbloquearUsuario, AsignarRol, QuitarRol, CrearRol, ActualizarRol, EliminarRol)
- [x] 5 Queries + Handlers (GetUsuarioById, GetUsuarios, GetMiPerfil, GetRoles, GetRolById)
- [x] `DependencyInjection.cs` — `AddAuthApplication()`

---

## FASE 7 — Auth Infrastructure ✅

> Compilación verificada: ✅ 0 errores. Migración: ✅ InitialCreate_Auth.

- [x] `Services/JwtService.cs` — AccessToken, RefreshToken, TokenTemporal2FA
- [x] `Services/PasswordService.cs` — BCrypt work factor 12, ValidarPolitica
- [x] `Services/TotpService.cs` — Otp.NET, QR URI, ventana ±1, códigos recuperación SHA256
- [x] `Persistence/AuthDbContext.cs` — schema "Auth", UsePropertyAccessMode global
- [x] `Persistence/AuthDbContextFactory.cs`
- [x] `Persistence/AuthDataSeeder.cs` — 11 permisos, 3 roles, usuario admin@coretemplate.com
- [x] `Persistence/Configurations/UsuarioConfiguration.cs` + `RolPermisoConfiguration.cs`
- [x] `Repositories/UsuarioRepository.cs` + `AuthRepositories.cs`
- [x] `Migrations/InitialCreate_Auth`
- [x] `DependencyInjection.cs` — `AddAuthInfrastructure()`

---

## FASE 8 — Auth Api ✅

> Compilación verificada: ✅ 0 errores.

- [x] `Controllers/AuthController.cs` — 10 endpoints (login, registro, refresh, logout, 2FA x4, restablecimiento x2)
- [x] `Controllers/UsuariosController.cs` — 7 endpoints
- [x] `Controllers/RolesController.cs` — 5 endpoints
- [x] `Controllers/PerfilController.cs` — 2 endpoints
- [x] `Contracts/AuthContracts.cs` — 13 request DTOs

---

## FASE 9 — Módulo Catálogos ✅

> Compilación verificada: ✅ 0 errores. Migración: ✅ InitialCreate_Catalogos.

### Domain
- [x] `Aggregates/CatalogoItem.cs` — Crear, Activar, Desactivar
- [x] `Events/CatalogosEvents.cs`
- [x] `Repositories/ICatalogoItemRepository.cs`

### Application
- [x] `Commands/CatalogosCommands.cs` — CrearItem + Handler + Validator, ActivarItem + Handler, DesactivarItem + Handler
- [x] `Queries/CatalogosQueries.cs` — GetItems (paginado + filtro + búsqueda) + Handler, GetItemById + Handler
- [x] `DTOs/CatalogoDtos.cs` — CatalogoItemDto, CatalogoItemResumenDto
- [x] `Constants/CatalogosMessages.cs`
- [x] `DependencyInjection.cs` — `AddCatalogosApplication()`

### Infrastructure
- [x] `Persistence/CatalogosDbContext.cs` — schema "Catalogos"
- [x] `Persistence/CatalogosDbContextFactory.cs`
- [x] `Persistence/CatalogosDataSeeder.cs` — 3 ítems de ejemplo
- [x] `Persistence/Configurations/CatalogoItemConfiguration.cs`
- [x] `Repositories/CatalogoItemRepository.cs`
- [x] `Migrations/InitialCreate_Catalogos`
- [x] `DependencyInjection.cs` — `AddCatalogosInfrastructure()`

### Api
- [x] `Controllers/CatalogosController.cs` — GET (paginado+filtro+búsqueda), GET {id}, POST, PUT {id}/activar, PUT {id}/desactivar
- [x] `Contracts/CatalogosContracts.cs` — CrearCatalogoItemRequest

---

## FASE 10 — Host final ✅

> Compilación verificada: ✅ 0 errores. Ambos módulos activos.

- [x] `Program.cs` — Auth + Catálogos activos (AddApplicationPart, AddXApplication, AddXInfrastructure, TenantMiddleware condicional)
- [x] `Extensions/ApplicationSeederExtension.cs` — AuthDataSeeder + CatalogosDataSeeder activos

---

## FASE 11 — Script de renombrado ✅

- [x] `rename.ps1` — reemplaza "CoreTemplate" en namespaces, archivos, carpetas, .sln, .csproj

---

## FASE 12 — Tests ✅

> Compilación verificada: ✅ 0 errores. Tests ejecutados: ✅ 90/90 pasan.

- [x] `SharedKernel.Tests/GlobalUsings.cs`
- [x] `SharedKernel.Tests/ResultTests.cs` — 7 tests
- [x] `SharedKernel.Tests/PagedResultTests.cs` — 7 tests
- [x] `SharedKernel.Tests/ValueObjectTests.cs` — 5 tests
- [x] `Auth.Tests/GlobalUsings.cs`
- [x] `Auth.Tests/UsuarioTests.cs` — 22 tests (Crear, Activar, Bloqueo, Password, Roles, 2FA)
- [x] `Auth.Tests/RolYValueObjectsTests.cs` — 18 tests (Rol, Email, PasswordHash)
- [x] `Auth.Tests/LoginCommandHandlerTests.cs` — 6 tests
- [x] `Auth.Tests/RegistrarUsuarioCommandHandlerTests.cs` — 4 tests
- [x] `Catalogos.Tests/GlobalUsings.cs`
- [x] `Catalogos.Tests/CatalogoItemTests.cs` — 21 tests (aggregate + handler)
- [x] `Auth.Application/AssemblyInfo.cs` — InternalsVisibleTo para tests
- [x] `Catalogos.Application/AssemblyInfo.cs` — InternalsVisibleTo para tests

---

## FASE 13 — README final ✅

- [x] `README.md` — Inicio rápido, configuración completa, todos los endpoints, flujos de auth, cómo agregar módulos y catálogos, arquitectura, tecnologías

---

## FASE 14 — Sesiones como aggregate + límites ✅

> Compilación verificada: ✅ 0 errores. Tests: ✅ 90/90.

> Convierte `RefreshToken` en una `Sesion` gestionable. Base para las fases 15-16.

### Auth — Domain
- [ ] `Enums/TipoUsuario.cs` — `Humano`, `Sistema`, `Integracion`
- [ ] `Enums/CanalAcceso.cs` — `Web`, `Mobile`, `Api`, `Desktop`
- [ ] `Aggregates/Sesion.cs` — aggregate con: `RefreshTokenHash`, `Canal`, `Dispositivo`, `Ip`, `UserAgent`, `UltimaActividad`, `ExpiraEn`, `EsActiva`; métodos: `Renovar()`, `Revocar()`
- [ ] Eliminar `RefreshToken` como entidad hija de `Usuario` — reemplazar por referencia a `Sesion`
- [ ] `Repositories/ISesionRepository.cs` — `GetActivasByUsuario()`, `GetById()`, `ContarActivas()`, `GetMasAntigua()`
- [ ] Actualizar `AuthEvents.cs` — agregar `SesionRevocadaEvent`, `TodasSesionesRevocadasEvent`

### Auth — Application
- [ ] `Abstractions/AuthSettings.cs` — agregar `MaxSesionesSimultaneas`, `AccionAlLlegarLimiteSesiones`
- [ ] `Abstractions/IAuthServices.cs` — agregar `ISesionService` con lógica de límites
- [ ] `DTOs/SesionDto.cs`
- [ ] Actualizar `LoginCommandHandler` — crear `Sesion` en lugar de `RefreshToken` directo; verificar límite
- [ ] Actualizar `RefreshTokenCommandHandler` — renovar `Sesion`
- [ ] Actualizar `LogoutCommandHandler` — revocar `Sesion`
- [ ] Actualizar `CambiarPasswordCommandHandler` — revocar todas las sesiones del usuario
- [ ] Actualizar `RestablecerPasswordCommandHandler` — revocar todas las sesiones del usuario
- [ ] `Queries/GetMisSesionesQuery.cs` + Handler
- [ ] `Commands/CerrarSesionCommand.cs` + Handler — cerrar sesión específica
- [ ] `Commands/CerrarOtrasSesionesCommand.cs` + Handler — cerrar todas excepto la actual
- [ ] `Queries/GetSesionesUsuarioQuery.cs` + Handler — admin
- [ ] `Commands/CerrarTodasSesionesUsuarioCommand.cs` + Handler — admin

### Auth — Infrastructure
- [ ] `Persistence/Configurations/SesionConfiguration.cs`
- [ ] `Repositories/SesionRepository.cs`
- [ ] `Services/SesionService.cs` — lógica de límites (`CerrarMasAntigua` / `BloquearNuevoLogin`)
- [ ] Actualizar `AuthDbContext.cs` — agregar `DbSet<Sesion>`, quitar `RefreshTokens`
- [ ] Migración: `Add_Sesiones`
- [ ] Actualizar `AuthDataSeeder.cs` si aplica

### Auth — Api
- [ ] `Controllers/PerfilController.cs` — agregar: `GET /sesiones`, `DELETE /sesiones/{id}`, `DELETE /sesiones/otras`
- [ ] `Controllers/UsuariosController.cs` — agregar: `GET /{id}/sesiones`, `DELETE /{id}/sesiones`
- [ ] `Contracts/AuthContracts.cs` — agregar request DTOs de sesiones si aplica

---

## FASE 15 — Tipos de usuario + Canales de acceso ✅

> Compilación verificada: ✅ 0 errores. Tests: ✅ 90/90. Migración: ✅ Add_Sesiones_TipoUsuario.

> Depende de Fase 14 (enums ya creados). Aplica comportamiento diferenciado.

### Auth — Domain
- [ ] `Aggregates/Usuario.cs` — agregar propiedad `TipoUsuario`; ajustar invariantes: `Sistema`/`Integracion` no requieren 2FA

### Auth — Application
- [ ] `Commands/RegistrarUsuarioCommandHandler` — aceptar `TipoUsuario` opcional (default `Humano`)
- [ ] `LoginCommandHandler` — pasar `CanalAcceso` al crear la sesión; omitir 2FA para `Sistema`/`Integracion`; omitir límite de sesiones para `Sistema`/`Integracion`
- [ ] `DTOs/AuthDtos.cs` — incluir `TipoUsuario` y `Canal` en respuestas donde aplique
- [ ] `Abstractions/AuthSettings.cs` — agregar `TwoFactorRequired` ya existe; verificar que se respete por `TipoUsuario`

### Auth — Infrastructure
- [ ] `Persistence/Configurations/UsuarioConfiguration.cs` — mapear `TipoUsuario`
- [ ] `Services/JwtService.cs` — incluir claims `tipo_usuario` y `canal` en el AccessToken
- [ ] Migración: `Add_TipoUsuario_Canal`

### Auth — Api
- [ ] `Contracts/AuthContracts.cs` — agregar `CanalAcceso` en `LoginRequest`; `TipoUsuario` en `RegistrarUsuarioRequest`

---

## FASE 16 — Token Blacklist (Redis + InMemory) ✅

> Compilación verificada: ✅ 0 errores. Tests: ✅ 90/90.

> Independiente de fases 14-15. Puede implementarse en paralelo.

### Auth — Application
- [ ] `Abstractions/ITokenBlacklistService.cs` — `AgregarAsync(jti, ttl)`, `EstaEnBlacklistAsync(jti)`
- [ ] `Abstractions/AuthSettings.cs` — agregar `EnableTokenBlacklist`, `TokenBlacklistSettings` (Provider, RedisConnectionString)

### Auth — Infrastructure
- [ ] `Services/InMemoryTokenBlacklistService.cs` — `ConcurrentDictionary` con limpieza por TTL
- [ ] `Services/RedisTokenBlacklistService.cs` — StackExchange.Redis, `SET jti EX ttl`
- [ ] `DependencyInjection.cs` — registrar según `TokenBlacklistSettings:Provider`
- [ ] Agregar `StackExchange.Redis` a `Directory.Packages.props` (opcional, solo si Provider=Redis)

### Infrastructure (BuildingBlocks)
- [ ] `Middleware/TokenBlacklistMiddleware.cs` — extrae `jti` del token, verifica blacklist antes de llegar al controller

### Host
- [ ] `Program.cs` — registrar `TokenBlacklistMiddleware` si `EnableTokenBlacklist = true`
- [ ] `appsettings.json` — agregar sección `TokenBlacklistSettings`

### Auth — Application (actualizar handlers)
- [ ] `LogoutCommandHandler` — llamar `ITokenBlacklistService.AgregarAsync()`
- [ ] `CerrarSesionCommandHandler` — llamar `ITokenBlacklistService.AgregarAsync()`
- [ ] `CerrarOtrasSesionesCommandHandler` — agregar tokens a blacklist
- [ ] `CambiarPasswordCommandHandler` — agregar token actual a blacklist
- [ ] `RestablecerPasswordCommandHandler` — agregar token actual a blacklist

---

## FASE 17 — Sucursales por usuario (configurable) ✅

> Compilación verificada: ✅ 0 errores. Tests: ✅ 90/90. Migración: ✅ Add_Sucursales.

> Requiere `OrganizationSettings:EnableBranches = true`. Cuando está desactivado, no genera tablas ni endpoints.

### Auth — Domain
- [ ] `Aggregates/Sucursal.cs` — `Nombre`, `Codigo`, `EsActiva`; métodos: `Activar()`, `Desactivar()`
- [ ] `Entities/UsuarioSucursal.cs` — relación usuario-sucursal con flag `EsPrincipal`
- [ ] Actualizar `Usuario.cs` — agregar `Sucursales: IReadOnlyList<UsuarioSucursal>`; métodos: `AsignarSucursal()`, `RemoverSucursal()`, `CambiarSucursalPrincipal()`; invariantes: al menos una sucursal, solo una principal
- [ ] `Repositories/ISucursalRepository.cs`
- [ ] Actualizar `AuthEvents.cs` — `SucursalAsignadaEvent`, `SucursalRemovidaEvent`

### Auth — Application
- [ ] `DTOs/SucursalDto.cs`
- [ ] `Commands/CrearSucursalCommand.cs` + Handler + Validator
- [ ] `Commands/AsignarSucursalUsuarioCommand.cs` + Handler
- [ ] `Commands/RemoverSucursalUsuarioCommand.cs` + Handler
- [ ] `Commands/CambiarSucursalActivaCommand.cs` + Handler — actualiza claim `branch_id` en sesión activa
- [ ] `Queries/GetSucursalesQuery.cs` + Handler
- [ ] `Queries/GetSucursalesUsuarioQuery.cs` + Handler

### Auth — Infrastructure
- [ ] `Persistence/Configurations/SucursalConfiguration.cs`
- [ ] `Repositories/SucursalRepository.cs`
- [ ] `Services/JwtService.cs` — incluir claim `branch_id` cuando `EnableBranches = true`
- [ ] Migración: `Add_Sucursales` — condicional, solo si `EnableBranches = true` en tiempo de diseño
- [ ] `DependencyInjection.cs` — registrar servicios de sucursales condicionalmente

### Auth — Api
- [ ] `Controllers/SucursalesController.cs` — CRUD de sucursales + asignación a usuarios
- [ ] `Controllers/PerfilController.cs` — agregar `PUT /sucursal-activa`

### Infrastructure (BuildingBlocks)
- [ ] `Services/ICurrentBranch.cs` + `CurrentBranchService.cs` — extrae `branch_id` del JWT

---

## FASE 18 — Roles por sucursal ✅

> Compilación verificada: ✅ 0 errores. Tests: ✅ 90/90. Migración: ✅ Add_AsignacionesRol.

> Requiere Fase 17 completada (`EnableBranches = true`).

### Auth — Domain
- [ ] `Aggregates/AsignacionRol.cs` — aggregate con `UsuarioId`, `SucursalId`, `RolId`; invariante: no duplicar misma combinación
- [ ] Actualizar `Usuario.cs` — reemplazar `UsuarioRoles` simples por `AsignacionesRol` cuando `EnableBranches = true`
- [ ] `Repositories/IAsignacionRolRepository.cs`

### Auth — Application
- [ ] `Commands/AsignarRolSucursalCommand.cs` + Handler
- [ ] `Commands/QuitarRolSucursalCommand.cs` + Handler
- [ ] `Queries/GetPermisosEfectivosQuery.cs` + Handler — calcula permisos según sucursal activa
- [ ] Actualizar `LoginCommandHandler` — incluir permisos de la sucursal activa en el token

### Auth — Infrastructure
- [ ] `Persistence/Configurations/AsignacionRolConfiguration.cs`
- [ ] `Repositories/AsignacionRolRepository.cs`
- [ ] `Services/JwtService.cs` — incluir permisos filtrados por `branch_id`
- [ ] Migración: `Add_AsignacionRoles`

### Auth — Api
- [ ] `Controllers/UsuariosController.cs` — agregar endpoints de roles por sucursal

---

## FASE 19 — Catálogo de Acciones (configurable) ✅

> Compilación verificada: ✅ 0 errores. Tests: ✅ 90/90. Migración: ✅ Add_CatalogoAcciones.

> Requiere `AuthSettings:UseActionCatalog = true`. Extiende el modelo de permisos.

### Auth — Domain
- [ ] `Aggregates/Accion.cs` — `Codigo`, `Nombre`, `Modulo`, `Descripcion`, `EsActiva`; métodos: `Activar()`, `Desactivar()`
- [ ] `Entities/AccionSucursal.cs` — habilitación por sucursal (si `EnableBranches = true`)
- [ ] `Entities/AccionCanal.cs` — restricción por canal de acceso
- [ ] `Repositories/IAccionRepository.cs`

### Auth — Application
- [ ] `DTOs/AccionDto.cs`
- [ ] `Commands/CrearAccionCommand.cs` + Handler + Validator
- [ ] `Commands/ActivarAccionCommand.cs` + Handler
- [ ] `Commands/DesactivarAccionCommand.cs` + Handler
- [ ] `Queries/GetAccionesQuery.cs` + Handler — paginado + filtro por módulo
- [ ] Actualizar `Permiso` / `RolPermiso` — referenciar `AccionId` en lugar de string
- [ ] `Services/PermisosEfectivosService.cs` — calcula permisos considerando canal y sucursal activa

### Auth — Infrastructure
- [ ] `Persistence/Configurations/AccionConfiguration.cs`
- [ ] `Repositories/AccionRepository.cs`
- [ ] `AuthDataSeeder.cs` — seed de acciones del sistema
- [ ] Migración: `Add_CatalogoAcciones`

### Auth — Api
- [ ] `Controllers/AccionesController.cs` — CRUD de acciones

---

## FASE 20 — Límites de sesiones por tenant ✅

> Compilación verificada: ✅ 0 errores. Tests: ✅ 90/90. Migración: ✅ Add_ConfiguracionTenant.

> Requiere Fase 14 completada. Solo aplica cuando `IsMultiTenant = true`.

### Auth — Domain
- [ ] `Entities/ConfiguracionTenant.cs` — entidad con `TenantId`, `MaxSesionesSimultaneas` (nullable)
- [ ] `Repositories/IConfiguracionTenantRepository.cs`

### Auth — Application
- [ ] `Abstractions/ISesionService.cs` — actualizar para consultar límite por tenant antes del global
- [ ] `Commands/ConfigurarLimiteSesionesTenantCommand.cs` + Handler
- [ ] `Queries/GetConfiguracionTenantQuery.cs` + Handler

### Auth — Infrastructure
- [ ] `Persistence/Configurations/ConfiguracionTenantConfiguration.cs`
- [ ] `Repositories/ConfiguracionTenantRepository.cs`
- [ ] `Services/SesionService.cs` — actualizar jerarquía: Tenant > Global > Default (5)
- [ ] Migración: `Add_ConfiguracionTenant`

### Auth — Api
- [ ] Endpoint admin para configurar límite por tenant (puede ir en un futuro `TenantsController`)

---

## FASE 21 — Tests de nuevas features ✅

> Tests: ✅ 126/126 (SharedKernel: 19, Auth: 92, Catálogos: 15).

### Auth.Tests
- [ ] `SesionTests.cs` — aggregate Sesion: crear, renovar, revocar, expiración
- [ ] `SesionLimitesTests.cs` — límites: cerrar más antigua, bloquear nuevo login, exención Sistema/Integracion
- [ ] `TokenBlacklistTests.cs` — InMemory: agregar, verificar, TTL
- [ ] `TipoUsuarioTests.cs` — comportamiento diferenciado por tipo
- [ ] `LoginCommandHandlerTests.cs` — actualizar tests existentes para Sesion + nuevos casos
- [ ] `SucursalTests.cs` — aggregate Sucursal + invariantes UsuarioSucursal (si EnableBranches)
- [ ] `AsignacionRolTests.cs` — invariante no duplicar (si EnableBranches)

---

## FASE 22 — Actualizar documentación y README ✅

- [x] `README.md` — sesiones, tipos de usuario, canales, token blacklist, sucursales, catálogo de acciones, todos los endpoints nuevos
- [x] `docs/ALCANCE.md` — fases marcadas como completadas
- [x] `docs/PLAN-IMPLEMENTACION.md` — actualizado con estado de cada fase

---

**Última actualización**: 2026-04-15
**Compilación**: ✅ Toda la solución — 0 errores
**Tests**: ✅ 126/126
**Estado general**:
- ✅ Fases 0-22 — PROYECTO COMPLETO ✅
