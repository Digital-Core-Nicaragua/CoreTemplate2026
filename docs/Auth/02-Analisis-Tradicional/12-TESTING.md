# Testing — Módulo Auth

> **Tests actuales:** 92 (Auth.Tests) + 19 (SharedKernel.Tests) + 15 (Catalogos.Tests) = **126 total**  
> **Fallos:** 0  
> **Fecha:** 2026-04-15

---

## Pirámide de Testing

```
         /\
        /E2E\        10% — Flujos completos de API
       /──────\
      /Integr. \     30% — Handlers + Repositorios
     /────────────\
    /  Unit Tests  \  60% — Aggregates, Value Objects, Services
   /────────────────\
```

---

## Tests Unitarios Implementados

### UsuarioTests.cs (22 tests)
- Crear usuario con datos válidos → estado Pendiente
- Crear usuario dispara UsuarioRegistradoEvent
- Crear con nombre vacío → falla
- Crear con nombre > 100 chars → falla
- Activar usuario pendiente → Activo
- Activar usuario ya activo → falla
- Desactivar usuario activo → Inactivo
- Desactivar usuario ya inactivo → falla
- IncrementarIntentosFallidos al límite → Bloqueado
- IncrementarIntentosFallidos sin llegar al límite → no bloquea
- Desbloquear usuario bloqueado → Activo
- Desbloquear usuario no bloqueado → falla
- PuedeAutenticarse usuario activo → true
- PuedeAutenticarse usuario inactivo → false
- PuedeAutenticarse usuario bloqueado con tiempo expirado → desbloqueo automático
- CambiarPassword → actualiza hash
- AsignarRol nuevo → asignado
- AsignarRol ya asignado → falla
- QuitarRol con un solo rol → falla
- QuitarRol con múltiples roles → quita correctamente
- ActivarDosFactores → activo con códigos
- ActivarDosFactores ya activo → falla
- DesactivarDosFactores activo → desactivado

### SesionTests.cs (6 tests)
- Crear sesión → activa y válida
- Revocar → inactiva y no válida
- Renovar → actualiza hash y expiración
- Sesión expirada → EsValida = false
- Sesión revocada → EsValida = false
- Canales distintos → canal correcto registrado

### SesionLimitesTests.cs (5 tests)
- Bajo límite → permite nueva sesión
- Al límite + CerrarMasAntigua → permite y cierra la antigua
- Al límite + BloquearNuevoLogin → rechaza
- TipoUsuario.Sistema → siempre permite (sin contar sesiones)
- TipoUsuario.Integracion → siempre permite

### TokenBlacklistTests.cs (4 tests)
- Agregar y verificar → encuentra el token
- Token no agregado → false
- Token expirado → false
- Múltiples tokens → gestionados independientemente

### TipoUsuarioTests.cs (4 tests)
- Default es Humano
- Sistema se asigna correctamente
- Integracion se asigna correctamente
- Theory con los 3 tipos

### SucursalTests.cs (11 tests)
- Crear con datos válidos → activa
- Código se convierte a MAYÚSCULAS
- Código vacío → falla
- Nombre vacío → falla
- Desactivar/Activar
- Primera sucursal asignada es principal
- Segunda no es principal
- Duplicada → falla
- Única sucursal no se puede remover
- Remover principal → asigna nueva principal
- CambiarSucursalPrincipal → actualiza correctamente

### AsignacionRolTests.cs (3 tests)
- Crear con datos válidos
- Dos asignaciones tienen IDs únicos
- La invariante de unicidad se aplica en el handler

### RolYValueObjectsTests.cs (18 tests)
- Rol: crear, agregar permiso, quitar permiso, duplicado falla, sistema no eliminable
- Email: formato válido, inválido, normalización
- PasswordHash: crear válido, vacío falla

### LoginCommandHandlerTests.cs (6 tests)
- Credenciales válidas → LoginResponse
- Email no existe → credenciales inválidas
- Password incorrecto → incrementar intentos
- Cuenta bloqueada → error bloqueo
- Cuenta inactiva → error inactiva
- Usuario con 2FA → token temporal

### RegistrarUsuarioCommandHandlerTests.cs (4 tests)
- Registro exitoso → retorna id
- Email duplicado → error
- Password no cumple política → error
- Confirmación no coincide → error

---

## Herramientas

| Herramienta | Versión | Uso |
|---|---|---|
| xUnit v3 | 3 | Framework de tests |
| FluentAssertions | 8 | Assertions legibles |
| NSubstitute | 5 | Mocks de interfaces |
| coverlet | 8 | Cobertura de código |

---

## Ejecutar Tests

```bash
# Todos los tests
dotnet test

# Solo Auth
dotnet test tests/CoreTemplate.Modules.Auth.Tests

# Con cobertura
dotnet test --collect:"XPlat Code Coverage"
```

---

## Próximos Tests a Agregar (Fase 21 pendiente)

- `LogoutCommandHandlerTests` — verificar blacklist
- `RefreshTokenCommandHandlerTests` — rotación de token
- `CambiarPasswordCommandHandlerTests` — revocación de sesiones
- `SesionServiceTests` — jerarquía de límites con tenant

---

**Fecha:** 2026-04-15
