# Guía: Cómo usar el módulo Notificaciones

> **Módulo:** CoreTemplate.Modules.Notificaciones
> **Tecnología:** SignalR (incluido en ASP.NET Core)
> **Fecha:** 2026-04-22

---

## Parte 1 — Enviar notificaciones desde un módulo del backend

### Paso 1: Inyectar INotificationSender

Cualquier handler o servicio puede enviar notificaciones inyectando `INotificationSender`:

```csharp
using CoreTemplate.Notifications.Abstractions;

public class AprobacionSolicitudHandler(INotificationSender notifications)
{
    public async Task Handle(AprobarSolicitudCommand cmd, CancellationToken ct)
    {
        // ... lógica de negocio ...

        await notifications.EnviarAsync(new NotificationMessage(
            UsuarioId: cmd.EmpleadoId,
            Titulo: "Solicitud aprobada",
            Mensaje: "Tu solicitud de vacaciones fue aprobada.",
            Tipo: TipoNotificacion.Exito,
            Url: $"/solicitudes/{cmd.SolicitudId}"
        ), ct);
    }
}
```

### Tipos de notificación disponibles

| Tipo | Uso | Ícono sugerido en UI |
|---|---|---|
| `TipoNotificacion.Info` | Información general | ℹ️ azul |
| `TipoNotificacion.Exito` | Operación completada | ✅ verde |
| `TipoNotificacion.Advertencia` | Atención requerida | ⚠️ amarillo |
| `TipoNotificacion.Error` | Algo falló | ❌ rojo |
| `TipoNotificacion.Seguridad` | Actividad de seguridad | 🔒 gris |

### Enviar a todo un tenant

```csharp
await notifications.EnviarATenantAsync(
    tenantId: currentTenant.TenantId!.Value,
    titulo: "Mantenimiento programado",
    mensaje: "El sistema estará en mantenimiento el domingo de 2am a 4am.",
    tipo: TipoNotificacion.Advertencia
);
```

---

## Parte 2 — Conectarse desde el frontend (JavaScript/TypeScript)

### Instalación del cliente SignalR

```bash
# npm
npm install @microsoft/signalr

# yarn
yarn add @microsoft/signalr
```

### Conexión básica

```javascript
import * as signalR from "@microsoft/signalr";

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/notificaciones", {
        // El token JWT se pasa como query string (requerido por SignalR para WebSockets)
        accessTokenFactory: () => localStorage.getItem("accessToken") ?? ""
    })
    .withAutomaticReconnect([0, 2000, 5000, 10000]) // reintentos: inmediato, 2s, 5s, 10s
    .configureLogging(signalR.LogLevel.Warning)
    .build();

// Escuchar notificaciones en tiempo real
connection.on("RecibirNotificacion", (notificacion) => {
    console.log("Nueva notificación:", notificacion);
    // notificacion = { titulo, mensaje, tipo, url, creadaEn }
    
    mostrarToast(notificacion);
    actualizarBadge();
});

// Manejar reconexión
connection.onreconnecting(() => {
    console.log("Reconectando al hub de notificaciones...");
});

connection.onreconnected(() => {
    console.log("Reconectado. Actualizando notificaciones pendientes...");
    cargarConteoNoLeidas();
});

// Iniciar conexión
async function conectar() {
    try {
        await connection.start();
        console.log("Conectado al hub de notificaciones.");
        cargarConteoNoLeidas();
    } catch (err) {
        console.error("Error al conectar:", err);
        setTimeout(conectar, 5000); // reintentar en 5 segundos
    }
}

conectar();
```

### Estructura del objeto notificación recibido

```typescript
interface NotificacionRecibida {
    titulo: string;
    mensaje: string;
    tipo: "Info" | "Exito" | "Advertencia" | "Error" | "Seguridad";
    url: string | null;    // link de acción opcional
    creadaEn: string;      // ISO 8601
}
```

---

## Parte 3 — API REST para gestionar notificaciones

### Autenticación

Todos los endpoints requieren el header:
```
Authorization: Bearer {accessToken}
```

---

### GET /api/notificaciones — Listar mis notificaciones

```http
GET /api/notificaciones?soloNoLeidas=false&pagina=1&tamano=20
Authorization: Bearer {token}
```

**Respuesta:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "3f2a1b4c-...",
        "titulo": "Tu comprobante está listo",
        "mensaje": "El comprobante de enero 2025 está disponible.",
        "tipo": "Exito",
        "url": "/nomina/comprobantes/123",
        "esLeida": false,
        "creadaEn": "2025-01-15T10:30:00Z",
        "leidaEn": null
      }
    ],
    "pagina": 1,
    "tamanoPagina": 20,
    "total": 5
  }
}
```

**Parámetros:**
| Parámetro | Tipo | Descripción |
|---|---|---|
| `soloNoLeidas` | bool? | `true` = solo no leídas, `false` o vacío = todas |
| `pagina` | int | Número de página (default: 1) |
| `tamano` | int | Tamaño de página (default: 20) |

---

### GET /api/notificaciones/no-leidas/count — Conteo para el badge 🔔

```http
GET /api/notificaciones/no-leidas/count
Authorization: Bearer {token}
```

**Respuesta:**
```json
{
  "success": true,
  "data": { "count": 3 }
}
```

Llamar este endpoint al cargar la app y después de cada `RecibirNotificacion` para mantener el badge actualizado.

---

### PUT /api/notificaciones/{id}/leer — Marcar una como leída

```http
PUT /api/notificaciones/3f2a1b4c-.../leer
Authorization: Bearer {token}
```

**Respuesta:**
```json
{
  "success": true,
  "message": "Notificación marcada como leída."
}
```

---

### PUT /api/notificaciones/leer-todas — Marcar todas como leídas

```http
PUT /api/notificaciones/leer-todas
Authorization: Bearer {token}
```

**Respuesta:**
```json
{
  "success": true,
  "message": "Todas las notificaciones marcadas como leídas."
}
```

---

## Parte 4 — Ejemplo completo en React/Vue

### Componente de notificaciones (React)

```tsx
import { useEffect, useState } from "react";
import * as signalR from "@microsoft/signalr";

interface Notificacion {
    id: string;
    titulo: string;
    mensaje: string;
    tipo: string;
    url: string | null;
    esLeida: boolean;
    creadaEn: string;
}

export function useNotificaciones(token: string) {
    const [conteoNoLeidas, setConteoNoLeidas] = useState(0);
    const [notificaciones, setNotificaciones] = useState<Notificacion[]>([]);

    useEffect(() => {
        const connection = new signalR.HubConnectionBuilder()
            .withUrl("/hubs/notificaciones", {
                accessTokenFactory: () => token
            })
            .withAutomaticReconnect()
            .build();

        // Recibir notificación en tiempo real
        connection.on("RecibirNotificacion", (n) => {
            setConteoNoLeidas(prev => prev + 1);
            setNotificaciones(prev => [
                {
                    id: crypto.randomUUID(),
                    titulo: n.titulo,
                    mensaje: n.mensaje,
                    tipo: n.tipo,
                    url: n.url,
                    esLeida: false,
                    creadaEn: n.creadaEn
                },
                ...prev
            ]);
        });

        connection.start().then(() => {
            // Cargar conteo inicial
            fetch("/api/notificaciones/no-leidas/count", {
                headers: { Authorization: `Bearer ${token}` }
            })
            .then(r => r.json())
            .then(data => setConteoNoLeidas(data.data.count));
        });

        return () => { connection.stop(); };
    }, [token]);

    const marcarComoLeida = async (id: string) => {
        await fetch(`/api/notificaciones/${id}/leer`, {
            method: "PUT",
            headers: { Authorization: `Bearer ${token}` }
        });
        setNotificaciones(prev =>
            prev.map(n => n.id === id ? { ...n, esLeida: true } : n)
        );
        setConteoNoLeidas(prev => Math.max(0, prev - 1));
    };

    const marcarTodasComoLeidas = async () => {
        await fetch("/api/notificaciones/leer-todas", {
            method: "PUT",
            headers: { Authorization: `Bearer ${token}` }
        });
        setNotificaciones(prev => prev.map(n => ({ ...n, esLeida: true })));
        setConteoNoLeidas(0);
    };

    return { conteoNoLeidas, notificaciones, marcarComoLeida, marcarTodasComoLeidas };
}
```

### Badge en el header

```tsx
function NotificacionBadge({ token }: { token: string }) {
    const { conteoNoLeidas } = useNotificaciones(token);

    return (
        <button className="relative">
            🔔
            {conteoNoLeidas > 0 && (
                <span className="badge">{conteoNoLeidas}</span>
            )}
        </button>
    );
}
```

---

## Parte 5 — Notificaciones automáticas del sistema

Estas notificaciones se generan automáticamente sin código adicional:

| Evento | Notificación | Tipo | Configurable |
|---|---|---|---|
| Cuenta bloqueada | "Cuenta bloqueada temporalmente" | ⚠️ Advertencia | `NotificationSettings:Handlers:UsuarioBloqueado` |
| Contraseña cambiada | "Tu contraseña fue cambiada" | 🔒 Seguridad | `NotificationSettings:Handlers:PasswordCambiado` |

Para desactivar alguna:
```json
{
  "NotificationSettings": {
    "Handlers": {
      "UsuarioBloqueado": false,
      "PasswordCambiado": true
    }
  }
}
```

---

## Parte 6 — Configuración en appsettings

```json
{
  "NotificationSettings": {
    "Handlers": {
      "UsuarioBloqueado": true,
      "PasswordCambiado": true
    }
  }
}
```

---

## Parte 7 — Checklist de integración

```
Backend:
  ✅ INotificationSender registrado en DI (AddNotificacionesModule)
  ✅ Hub mapeado en /hubs/notificaciones (Program.cs)
  ✅ JWT configurado con claim "tenant_id" para grupos de tenant

Frontend:
  □ Instalar @microsoft/signalr
  □ Conectar al hub con accessTokenFactory
  □ Escuchar evento "RecibirNotificacion"
  □ Llamar GET /api/notificaciones/no-leidas/count al iniciar
  □ Actualizar badge al recibir notificación
  □ Implementar panel de notificaciones con GET /api/notificaciones
  □ Implementar marcar como leída
```

---

## Parte 8 — Solución de problemas comunes

### El hub rechaza la conexión (401)
- Verificar que el token JWT no esté expirado
- Verificar que se pasa como `accessTokenFactory`, no como header
- El token debe tener el claim `sub` con el userId

### Las notificaciones no llegan en tiempo real
- Verificar que el usuario está en el grupo correcto: `user-{userId}`
- Verificar que el Hub está mapeado en Program.cs: `app.MapHub<NotificationHub>("/hubs/notificaciones")`
- Las notificaciones siempre se guardan en BD — si no llegan en tiempo real, aparecerán al consultar la API

### El badge no se actualiza
- Llamar `GET /api/notificaciones/no-leidas/count` después de cada `RecibirNotificacion`
- Verificar que `withAutomaticReconnect()` está configurado para reconexiones

### Múltiples instancias del servidor
- Activar Redis Backplane: `"NotificationSettings": { "UseRedisBackplane": true }`
- Reutiliza la conexión Redis del TokenBlacklist

---

**Fecha:** 2026-04-22
**Versión:** 1.0
