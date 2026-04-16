# Catálogos — Requisitos y Casos de Uso

---

## Propósito

El módulo Catálogos provee un **catálogo de ejemplo completamente implementado** que sirve como patrón de referencia para crear nuevos catálogos en cualquier sistema que use CoreTemplate.

El catálogo de ejemplo es `CatalogoItem` — una entidad genérica con código, nombre y descripción.

---

## Requisitos Funcionales

### RF-CAT-001: Crear ítem de catálogo
- El código debe ser único (por tenant si es multi-tenant)
- El nombre es requerido
- Al crear, el ítem queda activo por defecto

### RF-CAT-002: Consultar catálogo
- Listar todos los ítems con paginación
- Filtrar por estado (activo/inactivo)
- Buscar por código o nombre
- Obtener ítem por ID

### RF-CAT-003: Activar / Desactivar
- Un ítem activo puede desactivarse
- Un ítem inactivo puede activarse
- No se puede eliminar un ítem (solo desactivar)

---

## Casos de Uso

### CU-CAT-001: Crear ítem
**Actor**: Usuario con permiso `Catalogos.Items.Crear`  
**Flujo**:
1. Enviar: código, nombre, descripción (opcional)
2. Validar código único
3. Crear ítem activo
4. Retornar ID creado con 201

### CU-CAT-002: Listar ítems
**Actor**: Usuario autenticado  
**Flujo**:
1. Enviar: página, tamaño, filtro estado, búsqueda (opcional)
2. Retornar `PagedResult<CatalogoItemDto>`

### CU-CAT-003: Obtener por ID
**Actor**: Usuario autenticado  
**Flujo**:
1. Enviar: ID
2. Si no existe → 404
3. Retornar `CatalogoItemDto`

### CU-CAT-004: Activar
**Actor**: Usuario con permiso `Catalogos.Items.Gestionar`  
**Flujo**:
1. Enviar: ID
2. Si no existe → 404
3. Si ya está activo → 409
4. Activar y retornar 200

### CU-CAT-005: Desactivar
**Actor**: Usuario con permiso `Catalogos.Items.Gestionar`  
**Flujo**:
1. Enviar: ID
2. Si no existe → 404
3. Si ya está inactivo → 409
4. Desactivar y retornar 200

---

## Modelo de Datos

### CatalogoItem
| Campo | Tipo | Descripción |
|---|---|---|
| Id | Guid | PK |
| TenantId | Guid? | FK — null si single-tenant |
| Codigo | string(50) | Único por tenant |
| Nombre | string(200) | Requerido |
| Descripcion | string(500) | Opcional |
| EsActivo | bool | Estado |
| CreadoEn | DateTime | Fecha de creación |
| ModificadoEn | DateTime? | Fecha de última modificación |

---

## Cómo usar este módulo como patrón

Para crear un nuevo catálogo en tu sistema (ej: `TipoCliente`):

1. Copiar el aggregate `CatalogoItem` → renombrar a `TipoCliente`
2. Agregar propiedades específicas del negocio
3. Copiar los commands/queries → renombrar
4. Copiar el repositorio → renombrar
5. Copiar el controller → renombrar y ajustar ruta
6. Agregar al DbContext y crear migración

El patrón es siempre el mismo — solo cambia el nombre y las propiedades.
