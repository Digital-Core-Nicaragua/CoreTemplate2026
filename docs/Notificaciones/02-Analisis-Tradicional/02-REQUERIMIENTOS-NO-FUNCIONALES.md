# Notificaciones — Requerimientos No Funcionales

> **Fecha:** 2026-04-22

---

## RNF-NOT-001: Sin paquetes NuGet adicionales
SignalR está incluido en ASP.NET Core 10. No se requieren paquetes extra.
Solo agregar `app.MapHub<NotificationHub>("/hubs/notificaciones")` en Program.cs.

## RNF-NOT-002: Fallback a BD — sin pérdida de notificaciones
Si el usuario no está conectado, la notificación se guarda en BD.
Se entrega al reconectarse. No se pierden notificaciones.

## RNF-NOT-003: Escalabilidad horizontal
Para múltiples instancias del servidor → SignalR Redis Backplane.
Configurable: `"NotificationSettings": { "UseRedisBackplane": false }`.
Reutiliza la conexión Redis existente del TokenBlacklist.
En desarrollo y servidor único → sin Redis, sin configuración extra.

## RNF-NOT-004: Rendimiento del conteo
`GET /api/notificaciones/no-leidas/count` debe responder en < 50ms.
Índice en BD: `(UsuarioId, EsLeida)` garantiza esto.

## RNF-NOT-005: Retención de notificaciones
Notificaciones antiguas se limpian automáticamente.
Política configurable: `"NotificationSettings": { "RetenciónDías": 90 }`.
Implementar con job programado (Hangfire en el futuro).

## RNF-NOT-006: Aislamiento multi-tenant
`Notificacion` implementa `IHasTenant`.
El QueryFilter de `BaseDbContext` garantiza aislamiento automático.
Un tenant nunca ve notificaciones de otro tenant.

---

**Fecha:** 2026-04-22
