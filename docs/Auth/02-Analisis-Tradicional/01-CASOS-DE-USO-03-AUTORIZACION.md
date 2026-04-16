# Casos de Uso — Autorización

> **Grupo:** Autorización  
> **Códigos:** CU-AUTH-021 a CU-AUTH-030  
> **Fecha:** 2026-04-15

---

## CU-AUTH-021: Crear Rol

**Endpoint:** `POST /api/roles`  
**Flujo:** Verificar nombre único → Crear rol con permisos → Retornar id.

## CU-AUTH-022: Actualizar Rol

**Endpoint:** `PUT /api/roles/{id}`  
**Flujo:** Verificar nombre único (excluyendo el actual) → Actualizar nombre/descripción → Sincronizar permisos.

## CU-AUTH-023: Eliminar Rol

**Endpoint:** `DELETE /api/roles/{id}`  
**Flujo:** Verificar no es de sistema → Verificar sin usuarios → Eliminar.

## CU-AUTH-024: Asignar Rol Global a Usuario

**Endpoint:** `POST /api/usuarios/{id}/roles`  
**Flujo:** Verificar usuario existe → Verificar rol existe → `Usuario.AsignarRol(rolId)`.

## CU-AUTH-025: Quitar Rol Global de Usuario

**Endpoint:** `DELETE /api/usuarios/{id}/roles/{rolId}`  
**Flujo:** Verificar usuario existe → `Usuario.QuitarRol(rolId)` (valida al menos un rol).

## CU-AUTH-026: Obtener Permisos Efectivos

**Endpoint:** (interno, query)  
**Flujo:** Si `EnableBranches = false` → permisos de roles globales. Si `EnableBranches = true` → permisos de roles en sucursal activa del JWT.

## CU-AUTH-027: Crear Acción en Catálogo

**Endpoint:** `POST /api/acciones`  
**Requiere:** `UseActionCatalog = true`  
**Flujo:** Verificar código único y formato → Crear acción activa.

## CU-AUTH-028: Activar / Desactivar Acción

**Endpoints:** `PUT /api/acciones/{id}/activar`, `PUT /api/acciones/{id}/desactivar`  
**Flujo:** Buscar acción → Activar/Desactivar.

## CU-AUTH-029: Listar Acciones por Módulo

**Endpoint:** `GET /api/acciones?modulo={modulo}`  
**Flujo:** Retornar acciones filtradas por módulo (o todas si no se especifica).

## CU-AUTH-030: Asignar Rol por Sucursal

**Endpoint:** `POST /api/usuarios/{id}/sucursales/{sucursalId}/roles`  
**Requiere:** `EnableBranches = true`  
**Flujo:** Verificar usuario tiene la sucursal → Verificar unicidad → Crear `AsignacionRol`.

---

**Fecha:** 2026-04-15
