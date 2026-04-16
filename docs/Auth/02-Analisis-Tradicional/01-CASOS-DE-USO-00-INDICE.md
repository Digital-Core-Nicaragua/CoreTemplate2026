# Casos de Uso — Índice Maestro

> **Módulo:** Auth  
> **Total:** 45 casos de uso  
> **Fecha:** 2026-04-15

---

## Grupos de Casos de Uso

| Grupo | Documento | Cantidad |
|---|---|---|
| Autenticación | 01-CASOS-DE-USO-01-AUTENTICACION.md | 12 |
| Sesiones | 01-CASOS-DE-USO-02-SESIONES.md | 8 |
| Autorización | 01-CASOS-DE-USO-03-AUTORIZACION.md | 10 |
| Sucursales | 01-CASOS-DE-USO-04-SUCURSALES.md | 9 |
| Configuración | 01-CASOS-DE-USO-05-CONFIGURACION.md | 6 |
| **Total** | | **45** |

---

## Índice Completo

### 🔐 Autenticación (CU-AUTH-001 a CU-AUTH-012)

| Código | Nombre | Prioridad |
|---|---|---|
| CU-AUTH-001 | Registrar Usuario | Crítica |
| CU-AUTH-002 | Iniciar Sesión con Credenciales | Crítica |
| CU-AUTH-003 | Renovar Access Token (Refresh) | Crítica |
| CU-AUTH-004 | Cerrar Sesión (Logout) | Crítica |
| CU-AUTH-005 | Solicitar Restablecimiento de Contraseña | Alta |
| CU-AUTH-006 | Restablecer Contraseña con Token | Alta |
| CU-AUTH-007 | Cambiar Contraseña (usuario autenticado) | Alta |
| CU-AUTH-008 | Activar 2FA (generar QR) | Media |
| CU-AUTH-009 | Confirmar Activación de 2FA | Media |
| CU-AUTH-010 | Verificar Código TOTP en Login | Media |
| CU-AUTH-011 | Desactivar 2FA | Media |
| CU-AUTH-012 | Usar Código de Recuperación 2FA | Media |

### 📱 Sesiones (CU-AUTH-013 a CU-AUTH-020)

| Código | Nombre | Prioridad |
|---|---|---|
| CU-AUTH-013 | Ver Mis Sesiones Activas | Alta |
| CU-AUTH-014 | Cerrar Sesión Específica | Alta |
| CU-AUTH-015 | Cerrar Todas las Sesiones Excepto la Actual | Alta |
| CU-AUTH-016 | Ver Sesiones de un Usuario (Admin) | Alta |
| CU-AUTH-017 | Cerrar Todas las Sesiones de un Usuario (Admin) | Alta |
| CU-AUTH-018 | Verificar Token en Blacklist | Crítica |
| CU-AUTH-019 | Configurar Límite de Sesiones por Tenant | Media |
| CU-AUTH-020 | Ver Configuración de Tenant | Media |

### 🛡️ Autorización (CU-AUTH-021 a CU-AUTH-030)

| Código | Nombre | Prioridad |
|---|---|---|
| CU-AUTH-021 | Crear Rol | Alta |
| CU-AUTH-022 | Actualizar Rol | Alta |
| CU-AUTH-023 | Eliminar Rol | Alta |
| CU-AUTH-024 | Asignar Rol Global a Usuario | Alta |
| CU-AUTH-025 | Quitar Rol Global de Usuario | Alta |
| CU-AUTH-026 | Obtener Permisos Efectivos | Alta |
| CU-AUTH-027 | Crear Acción en Catálogo | Media |
| CU-AUTH-028 | Activar / Desactivar Acción | Media |
| CU-AUTH-029 | Listar Acciones por Módulo | Media |
| CU-AUTH-030 | Asignar Rol por Sucursal | Alta |

### 🏢 Sucursales (CU-AUTH-031 a CU-AUTH-039)

| Código | Nombre | Prioridad |
|---|---|---|
| CU-AUTH-031 | Crear Sucursal | Alta |
| CU-AUTH-032 | Activar / Desactivar Sucursal | Alta |
| CU-AUTH-033 | Asignar Sucursal a Usuario | Alta |
| CU-AUTH-034 | Remover Sucursal de Usuario | Alta |
| CU-AUTH-035 | Cambiar Sucursal Activa (Perfil) | Alta |
| CU-AUTH-036 | Ver Sucursales de un Usuario | Media |
| CU-AUTH-037 | Listar Sucursales | Media |
| CU-AUTH-038 | Asignar Rol por Sucursal | Alta |
| CU-AUTH-039 | Quitar Rol por Sucursal | Alta |

### ⚙️ Configuración y Administración (CU-AUTH-040 a CU-AUTH-045)

| Código | Nombre | Prioridad |
|---|---|---|
| CU-AUTH-040 | Activar Usuario | Alta |
| CU-AUTH-041 | Desactivar Usuario | Alta |
| CU-AUTH-042 | Desbloquear Usuario | Alta |
| CU-AUTH-043 | Listar Usuarios (paginado) | Alta |
| CU-AUTH-044 | Ver Perfil Propio | Alta |
| CU-AUTH-045 | Ver Usuario por ID | Alta |

---

## Trazabilidad

| Caso de Uso | Aggregate | Endpoint | RF |
|---|---|---|---|
| CU-AUTH-001 | Usuario | POST /api/auth/registro | RF-AUTH-001 |
| CU-AUTH-002 | Usuario, Sesion | POST /api/auth/login | RF-AUTH-002 |
| CU-AUTH-003 | Sesion | POST /api/auth/refresh | RF-AUTH-003 |
| CU-AUTH-004 | Sesion | POST /api/auth/logout | RF-AUTH-004 |
| CU-AUTH-013 | Sesion | GET /api/perfil/sesiones | RF-AUTH-008 |
| CU-AUTH-021 | Rol | POST /api/roles | RF-AUTH-010 |
| CU-AUTH-031 | Sucursal | POST /api/sucursales | RF-AUTH-015 |

---

**Total Casos de Uso:** 45  
**Fecha:** 2026-04-15
