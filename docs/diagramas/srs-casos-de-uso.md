# SRS — Diagramas de Casos de Uso

> Complementa: `docs/Auth/02-Analisis-Tradicional/01-CASOS-DE-USO-*.md`  
> Fecha: 2026-04-15

---

## Actores del Sistema

| Actor | Descripción |
|---|---|
| **UsuarioAnónimo** | No autenticado — solo puede registrarse o hacer login |
| **UsuarioHumano** | Autenticado — persona real, aplican todas las reglas |
| **Admin** | Autenticado con rol Admin — gestiona usuarios, roles, sucursales |
| **SuperAdmin** | Autenticado con rol SuperAdmin — acceso total, no puede desactivarse |
| **SistemaIntegracion** | Usuario de tipo Sistema o Integración — sin 2FA, sin bloqueo, sin límite de sesiones |

---

## Diagrama 1: Autenticación

```mermaid
graph TD
    UA([UsuarioAnónimo])
    UH([UsuarioHumano])
    SI([SistemaIntegracion])

    UA --> CU001[Registrar Usuario]
    UA --> CU002[Iniciar Sesión]
    UA --> CU005[Solicitar Reset Password]
    UA --> CU006[Restablecer Password]
    UA --> CU010[Verificar Código TOTP]

    UH --> CU003[Renovar Access Token]
    UH --> CU004[Cerrar Sesión]
    UH --> CU007[Cambiar Password]
    UH --> CU008[Activar 2FA]
    UH --> CU009[Confirmar Activación 2FA]
    UH --> CU011[Desactivar 2FA]
    UH --> CU012[Usar Código Recuperación 2FA]

    SI --> CU002
    SI --> CU003
    SI --> CU004
```

---

## Diagrama 2: Gestión de Sesiones

```mermaid
graph TD
    UH([UsuarioHumano])
    ADM([Admin])

    UH --> CU013[Ver Mis Sesiones Activas]
    UH --> CU014[Cerrar Sesión Específica]
    UH --> CU015[Cerrar Todas Excepto la Actual]

    ADM --> CU016[Ver Sesiones de un Usuario]
    ADM --> CU017[Cerrar Todas las Sesiones de un Usuario]
    ADM --> CU019[Configurar Límite de Sesiones por Tenant]
    ADM --> CU020[Ver Configuración de Tenant]

    CU014 -.incluye.-> CU018[Verificar Token en Blacklist]
    CU015 -.incluye.-> CU018
    CU017 -.incluye.-> CU018
```

---

## Diagrama 3: Autorización — Roles y Permisos

```mermaid
graph TD
    ADM([Admin])
    SA([SuperAdmin])

    ADM --> CU021[Crear Rol]
    ADM --> CU022[Actualizar Rol]
    ADM --> CU023[Eliminar Rol]
    ADM --> CU024[Asignar Rol Global a Usuario]
    ADM --> CU025[Quitar Rol Global de Usuario]
    ADM --> CU026[Obtener Permisos Efectivos]
    ADM --> CU030[Asignar Rol por Sucursal]

    SA --> CU027[Crear Acción en Catálogo]
    SA --> CU028[Activar / Desactivar Acción]
    SA --> CU029[Listar Acciones por Módulo]

    CU023 -.extiende.-> CU026
```

---

## Diagrama 4: Sucursales (EnableBranches = true)

```mermaid
graph TD
    ADM([Admin])
    UH([UsuarioHumano])

    ADM --> CU031[Crear Sucursal]
    ADM --> CU032[Activar / Desactivar Sucursal]
    ADM --> CU033[Asignar Sucursal a Usuario]
    ADM --> CU034[Remover Sucursal de Usuario]
    ADM --> CU036[Ver Sucursales de un Usuario]
    ADM --> CU037[Listar Sucursales]
    ADM --> CU038[Asignar Rol por Sucursal]
    ADM --> CU039[Quitar Rol por Sucursal]

    UH --> CU035[Cambiar Sucursal Activa]
```

---

## Diagrama 5: Administración de Usuarios

```mermaid
graph TD
    ADM([Admin])
    SA([SuperAdmin])
    UH([UsuarioHumano])

    ADM --> CU040[Activar Usuario]
    ADM --> CU041[Desactivar Usuario]
    ADM --> CU042[Desbloquear Usuario]
    ADM --> CU043[Listar Usuarios]
    ADM --> CU045[Ver Usuario por ID]

    SA --> CU040
    SA --> CU041
    SA --> CU042

    UH --> CU044[Ver Perfil Propio]
```

---

## Diagrama 6: Catálogos

```mermaid
graph TD
    UH([UsuarioHumano])
    ADM([Admin])

    UH --> CC001[Listar Ítems]
    UH --> CC002[Ver Ítem por ID]

    ADM --> CC003[Crear Ítem]
    ADM --> CC004[Activar Ítem]
    ADM --> CC005[Desactivar Ítem]
```
