# Contratos API — Módulo Auth

> **Total endpoints:** 52  
> **Fecha:** 2026-04-15

---

## Auth (`/api/auth`)

### POST /api/auth/login
```json
// Request
{
  "email": "admin@coretemplate.com",
  "password": "Admin@1234!",
  "canal": "Web",
  "dispositivo": "Chrome en Windows"
}

// Response 200 — Login exitoso
{
  "success": true,
  "data": {
    "accessToken": "eyJ...",
    "refreshToken": "abc123...",
    "accessTokenExpiraEn": "2026-04-15T10:15:00Z",
    "usuario": {
      "id": "guid",
      "email": "admin@coretemplate.com",
      "nombre": "Administrador",
      "estado": "Activo",
      "twoFactorActivo": false,
      "roles": ["SuperAdmin"]
    }
  }
}

// Response 200 — 2FA requerido
{
  "success": true,
  "data": {
    "tokenTemporal": "eyJ...",
    "requires2FA": true
  }
}

// Response 401 — Credenciales inválidas
{ "success": false, "errors": ["Las credenciales son inválidas."] }

// Response 401 — Cuenta bloqueada
{ "success": false, "errors": ["La cuenta está bloqueada temporalmente."] }
```

### POST /api/auth/registro
```json
// Request
{
  "email": "nuevo@sistema.com",
  "nombre": "Juan Pérez",
  "password": "MiPassword@123",
  "confirmPassword": "MiPassword@123",
  "tipoUsuario": "Humano"
}

// Response 201
{ "success": true, "data": "guid-del-usuario" }
```

### POST /api/auth/refresh
```json
// Request
{ "refreshToken": "abc123..." }

// Response 200
{
  "success": true,
  "data": {
    "accessToken": "eyJ...",
    "refreshToken": "xyz789...",
    "accessTokenExpiraEn": "2026-04-15T10:30:00Z"
  }
}
```

### POST /api/auth/logout
```json
// Request (+ Authorization: Bearer {accessToken})
{ "refreshToken": "abc123..." }

// Response 200
{ "success": true, "data": true }
```

### POST /api/auth/solicitar-restablecimiento
```json
// Request
{ "email": "usuario@sistema.com" }

// Response 200 (siempre, no revelar si email existe)
{ "success": true, "data": true }
```

### POST /api/auth/restablecer-password
```json
// Request
{
  "token": "base64token...",
  "nuevoPassword": "NuevoPassword@123",
  "confirmPassword": "NuevoPassword@123"
}
// Response 200
{ "success": true, "data": true }
```

### POST /api/auth/2fa/activar
```json
// Response 200
{
  "success": true,
  "data": {
    "qrCodeUri": "otpauth://totp/CoreTemplate:admin@...",
    "secretKey": "BASE32SECRET",
    "codigosRecuperacion": ["abc123", "def456", ...]
  }
}
```

### POST /api/auth/2fa/confirmar
```json
// Request
{ "codigo": "123456" }
// Response 200
{ "success": true, "data": true }
```

### POST /api/auth/2fa/verificar
```json
// Request
{ "tokenTemporal": "eyJ...", "codigo": "123456" }
// Response 200 — igual que login exitoso
```

### POST /api/auth/2fa/desactivar
```json
// Request
{ "codigo": "123456" }
// Response 200
{ "success": true, "data": true }
```

---

## Perfil (`/api/perfil`)

### GET /api/perfil
```json
// Response 200
{
  "success": true,
  "data": {
    "id": "guid",
    "email": "usuario@sistema.com",
    "nombre": "Juan Pérez",
    "estado": "Activo",
    "twoFactorActivo": false,
    "ultimoAcceso": "2026-04-15T09:00:00Z",
    "roles": ["Admin", "User"]
  }
}
```

### PUT /api/perfil/cambiar-password
```json
// Request
{
  "passwordActual": "OldPassword@123",
  "nuevoPassword": "NewPassword@456",
  "confirmPassword": "NewPassword@456"
}
```

### GET /api/perfil/sesiones
```json
// Response 200
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "canal": "Web",
      "dispositivo": "Chrome en Windows",
      "ip": "192.168.1.1",
      "userAgent": "Mozilla/5.0...",
      "ultimaActividad": "2026-04-15T09:30:00Z",
      "expiraEn": "2026-04-22T09:00:00Z",
      "creadoEn": "2026-04-15T09:00:00Z"
    }
  ]
}
```

### DELETE /api/perfil/sesiones/{id}
```json
// Response 200
{ "success": true, "data": true }
```

### DELETE /api/perfil/sesiones/otras?sesionActualId={id}
```json
// Response 200
{ "success": true, "data": true }
```

### PUT /api/perfil/sucursal-activa
```json
// Request
{ "sucursalId": "guid" }
// Response 200
{
  "success": true,
  "data": { "id": "guid", "codigo": "SUC001", "nombre": "Sucursal Central", "esActiva": true }
}
```

---

## Usuarios (`/api/usuarios`)

| Método | Ruta | Descripción |
|---|---|---|
| GET | `/` | Listar (paginado, filtro por estado) |
| GET | `/{id}` | Obtener por ID |
| PUT | `/{id}/activar` | Activar |
| PUT | `/{id}/desactivar` | Desactivar |
| PUT | `/{id}/desbloquear` | Desbloquear |
| POST | `/{id}/roles` | Asignar rol global |
| DELETE | `/{id}/roles/{rolId}` | Quitar rol global |
| GET | `/{id}/sesiones` | Ver sesiones (admin) |
| DELETE | `/{id}/sesiones` | Cerrar todas las sesiones (admin) |
| POST | `/{id}/sucursales/{sucursalId}/roles` | Asignar rol por sucursal |
| DELETE | `/{id}/sucursales/{sucursalId}/roles/{rolId}` | Quitar rol por sucursal |

---

## Roles (`/api/roles`)

| Método | Ruta | Descripción |
|---|---|---|
| GET | `/` | Listar roles |
| GET | `/{id}` | Obtener por ID |
| POST | `/` | Crear rol |
| PUT | `/{id}` | Actualizar rol |
| DELETE | `/{id}` | Eliminar rol |

---

## Sucursales (`/api/sucursales`) — EnableBranches = true

| Método | Ruta | Descripción |
|---|---|---|
| GET | `/` | Listar sucursales |
| POST | `/` | Crear sucursal |
| GET | `/usuarios/{usuarioId}` | Ver sucursales de usuario |
| POST | `/usuarios/{usuarioId}` | Asignar sucursal a usuario |
| DELETE | `/usuarios/{usuarioId}/{sucursalId}` | Remover sucursal de usuario |

---

## Acciones (`/api/acciones`) — UseActionCatalog = true

| Método | Ruta | Descripción |
|---|---|---|
| GET | `/` | Listar (filtro por módulo) |
| POST | `/` | Crear acción |
| PUT | `/{id}/activar` | Activar |
| PUT | `/{id}/desactivar` | Desactivar |

---

## Tenants (`/api/tenants`) — IsMultiTenant = true

| Método | Ruta | Descripción |
|---|---|---|
| GET | `/{tenantId}/configuracion` | Ver configuración |
| PUT | `/{tenantId}/limite-sesiones` | Configurar límite de sesiones |

---

## Formato de Respuesta Estándar

```json
// Éxito
{
  "success": true,
  "message": "Operación exitosa.",
  "data": { ... },
  "errors": []
}

// Error
{
  "success": false,
  "message": "El email ya está registrado.",
  "data": null,
  "errors": ["El email ya está registrado."]
}

// Paginado
{
  "success": true,
  "data": {
    "items": [...],
    "pagina": 1,
    "tamanoPagina": 20,
    "total": 45,
    "totalPaginas": 3
  }
}
```

---

## Códigos HTTP Usados

| Código | Cuándo |
|---|---|
| 200 | Éxito general |
| 201 | Recurso creado |
| 400 | Validación fallida |
| 401 | No autenticado o token revocado |
| 403 | Sin permisos |
| 404 | Recurso no encontrado |
| 409 | Conflicto (duplicado, estado inválido) |

---

**Fecha:** 2026-04-15
