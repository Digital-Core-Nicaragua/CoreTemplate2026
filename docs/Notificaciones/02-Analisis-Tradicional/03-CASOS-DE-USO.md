# Notificaciones — Casos de Uso

> **Fecha:** 2026-04-22

---

## Actores

| Actor | Tipo | Descripción |
|---|---|---|
| **Usuario** | Humano | Recibe y gestiona sus notificaciones |
| **Módulo Consumidor** | Sistema | Auth, Nómina, RRHH — envían notificaciones |
| **SignalR Hub** | Infraestructura | Canal WebSocket de entrega |

---

## CU-NOT-001: Usuario recibe notificación en tiempo real

**Actor:** Usuario (pasivo — recibe)
**Precondición:** Usuario conectado al Hub con JWT válido

**Flujo:**
1. Módulo Nómina genera comprobante de pago
2. Llama `INotificationSender.EnviarAsync(new NotificationMessage(empleadoId, "Tu comprobante está listo", ...))`
3. El sender guarda la notificación en BD
4. Detecta que el usuario está conectado al Hub
5. Envía el mensaje al grupo `"user-{empleadoId}"` via SignalR
6. El frontend recibe el evento `RecibirNotificacion` y muestra el toast + badge

---

## CU-NOT-002: Usuario recibe notificaciones acumuladas al conectarse

**Actor:** Usuario
**Precondición:** Usuario tenía notificaciones no leídas mientras estaba desconectado

**Flujo:**
1. Usuario abre la aplicación y se conecta al Hub
2. El Hub detecta la conexión y consulta notificaciones no leídas en BD
3. Envía todas las notificaciones pendientes al usuario
4. El badge 🔔 muestra el conteo correcto

---

## CU-NOT-003: Usuario consulta su historial de notificaciones

**Actor:** Usuario
**Flujo:**
```
GET /api/notificaciones?soloNoLeidas=false&pagina=1&tamano=20
Authorization: Bearer {token}

→ Retorna lista paginada de notificaciones del usuario
→ Incluye: titulo, mensaje, tipo, url, esLeida, creadaEn
```

---

## CU-NOT-004: Usuario marca notificaciones como leídas

**Actor:** Usuario

**Flujo A — una notificación:**
```
PUT /api/notificaciones/{id}/leer
→ Marca esa notificación como leída
→ El badge 🔔 se actualiza en tiempo real via SignalR
```

**Flujo B — todas:**
```
PUT /api/notificaciones/leer-todas
→ Marca todas como leídas
→ Badge 🔔 vuelve a 0
```

---

## CU-NOT-005: Módulo consumidor envía notificación

**Actor:** Módulo Consumidor (Auth, Nómina, RRHH)

**Flujo:**
```csharp
// En cualquier handler
await notifications.EnviarAsync(new NotificationMessage(
    UsuarioId: cmd.EmpleadoId,
    Titulo: "Tu comprobante está listo",
    Mensaje: "El comprobante de enero 2025 está disponible para descarga",
    Tipo: TipoNotificacion.Exito,
    Url: $"/nomina/comprobantes/{cmd.ComprobanteId}",
    TenantId: currentTenant.TenantId
), ct);
```

---

## CU-NOT-006: Notificación automática de seguridad (Auth)

**Actor:** Sistema (automático via evento de dominio)

**Flujo — nueva sesión:**
1. Usuario hace login desde nuevo dispositivo
2. Auth publica `SesionCreadaEvent`
3. `SesionCreadaNotificationHandler` recibe el evento
4. Envía notificación de seguridad al usuario:
   - Título: "Nueva sesión iniciada"
   - Mensaje: "Sesión desde Chrome en Windows (192.168.1.1)"
   - Tipo: Seguridad
   - URL: "/perfil/sesiones"
5. Si el usuario está conectado → ve el toast inmediatamente
6. Si no → lo ve al próximo login

---

## Cómo se conecta el frontend

```javascript
// Ejemplo JavaScript/TypeScript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/notificaciones", {
        accessTokenFactory: () => localStorage.getItem("accessToken")
    })
    .withAutomaticReconnect()
    .build();

// Recibir notificación en tiempo real
connection.on("RecibirNotificacion", (notificacion) => {
    actualizarBadge(notificacion.conteoNoLeidas);
    mostrarToast(notificacion.titulo, notificacion.mensaje, notificacion.tipo);
});

await connection.start();
```

---

**Fecha:** 2026-04-22
