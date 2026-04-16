# Casos de Uso — Sucursales y Configuración

> **Grupos:** Sucursales (CU-AUTH-031 a CU-AUTH-039), Configuración (CU-AUTH-040 a CU-AUTH-045)  
> **Fecha:** 2026-04-15

---

## Sucursales (EnableBranches = true)

## CU-AUTH-031: Crear Sucursal

**Endpoint:** `POST /api/sucursales`  
**Flujo:** Verificar código único → Crear sucursal activa (código en MAYÚSCULAS).

## CU-AUTH-032: Activar / Desactivar Sucursal

**Endpoints:** `PUT /api/sucursales/{id}/activar`, `PUT /api/sucursales/{id}/desactivar`

## CU-AUTH-033: Asignar Sucursal a Usuario

**Endpoint:** `POST /api/sucursales/usuarios/{usuarioId}`  
**Flujo:** Verificar sucursal activa → `Usuario.AsignarSucursal()` (primera = principal automáticamente).

## CU-AUTH-034: Remover Sucursal de Usuario

**Endpoint:** `DELETE /api/sucursales/usuarios/{usuarioId}/{sucursalId}`  
**Flujo:** `Usuario.RemoverSucursal()` (valida al menos una sucursal, reasigna principal si era la principal).

## CU-AUTH-035: Cambiar Sucursal Activa (Perfil)

**Endpoint:** `PUT /api/perfil/sucursal-activa`  
**Flujo:** `Usuario.CambiarSucursalPrincipal(sucursalId)` → Retornar nueva sucursal activa.

## CU-AUTH-036: Ver Sucursales de un Usuario

**Endpoint:** `GET /api/sucursales/usuarios/{usuarioId}`  
**Flujo:** Retornar lista de `UsuarioSucursalDto` con flag `EsPrincipal`.

## CU-AUTH-037: Listar Sucursales

**Endpoint:** `GET /api/sucursales`  
**Flujo:** Retornar todas las sucursales del tenant actual.

## CU-AUTH-038: Asignar Rol por Sucursal

**Endpoint:** `POST /api/usuarios/{id}/sucursales/{sucursalId}/roles`  
**Flujo:** Ver CU-AUTH-030.

## CU-AUTH-039: Quitar Rol por Sucursal

**Endpoint:** `DELETE /api/usuarios/{id}/sucursales/{sucursalId}/roles/{rolId}`  
**Flujo:** Buscar `AsignacionRol` → Eliminar.

---

## Configuración y Administración

## CU-AUTH-040: Activar Usuario

**Endpoint:** `PUT /api/usuarios/{id}/activar`  
**Flujo:** `Usuario.Activar()`.

## CU-AUTH-041: Desactivar Usuario

**Endpoint:** `PUT /api/usuarios/{id}/desactivar`  
**Flujo:** Verificar no es SuperAdmin → `Usuario.Desactivar()`.

## CU-AUTH-042: Desbloquear Usuario

**Endpoint:** `PUT /api/usuarios/{id}/desbloquear`  
**Flujo:** `Usuario.Desbloquear()`.

## CU-AUTH-043: Listar Usuarios (paginado)

**Endpoint:** `GET /api/usuarios?pagina=1&tamanoPagina=20&estado=Activo`  
**Flujo:** Retornar `PagedResult<UsuarioResumenDto>` con filtro opcional por estado.

## CU-AUTH-044: Ver Perfil Propio

**Endpoint:** `GET /api/perfil`  
**Flujo:** Retornar `UsuarioDto` del usuario autenticado con roles resueltos por nombre.

## CU-AUTH-045: Ver Usuario por ID

**Endpoint:** `GET /api/usuarios/{id}`  
**Flujo:** Retornar `UsuarioDto` con roles resueltos por nombre.

---

**Fecha:** 2026-04-15
