# Requerimientos No Funcionales — Módulo Auth

> **Total:** 15 RNF  
> **Fecha:** 2026-04-15

---

## Seguridad (7 RNF)

### RNF-AUTH-001: Hash de Contraseñas
- BCrypt con work factor 12
- Nunca almacenar ni transmitir contraseñas en texto plano
- Nunca incluir contraseñas en logs

### RNF-AUTH-002: Tokens JWT
- Firmados con HMAC SHA256 (mínimo 256 bits de clave)
- AccessToken: 15 minutos (configurable)
- RefreshToken: 7 días (configurable), almacenado como hash SHA256
- Claims mínimos: `sub`, `email`, `name`, `jti`, `tipo_usuario`

### RNF-AUTH-003: Token Blacklist
- Verificación en O(1) con Redis
- TTL automático basado en expiración del token
- Sin impacto en rendimiento si está desactivado (`EnableTokenBlacklist = false`)

### RNF-AUTH-004: Política de Contraseñas
- Configurable: longitud mínima, mayúsculas, minúsculas, dígitos, especiales
- Validada en registro y cambio de contraseña

### RNF-AUTH-005: Bloqueo de Cuenta
- Configurable: intentos máximos, duración del bloqueo
- Solo aplica a `TipoUsuario.Humano`
- Desbloqueo automático configurable

### RNF-AUTH-006: Auditoría
- Todos los eventos críticos registrados con: fecha UTC, IP, user agent, canal, resultado
- Registros inmutables (solo se agregan, nunca se modifican)
- Incluye intentos fallidos de login

### RNF-AUTH-007: HTTPS
- Toda comunicación debe ser sobre HTTPS en producción
- Configurado en `launchSettings.json` para desarrollo

---

## Performance (3 RNF)

### RNF-AUTH-008: Tiempo de Login
- Login exitoso: < 500ms (p95) bajo carga normal
- Incluye: verificación de contraseña BCrypt + creación de sesión + generación de JWT

### RNF-AUTH-009: Verificación de Blacklist
- Con Redis: < 5ms
- Con InMemory: < 1ms
- No debe impactar el tiempo de respuesta de los endpoints

### RNF-AUTH-010: Consulta de Sesiones
- Listar sesiones activas de un usuario: < 100ms
- Índice en `(UsuarioId, EsActiva)` en tabla `Sesiones`

---

## Escalabilidad (2 RNF)

### RNF-AUTH-011: Token Blacklist Distribuida
- Con `Provider = Redis`: válida para múltiples instancias del servidor
- Con `Provider = InMemory`: solo válida para un servidor (desarrollo)

### RNF-AUTH-012: Multi-tenant
- Filtrado automático por `TenantId` sin impacto en el código de negocio
- `QueryFilter` global en `BaseDbContext`

---

## Mantenibilidad (2 RNF)

### RNF-AUTH-013: Configurabilidad
- Todas las políticas de seguridad configurables en `appsettings.json`
- Sin necesidad de recompilar para cambiar políticas
- Features opcionales activables/desactivables con flags

### RNF-AUTH-014: Tests
- Cobertura mínima del módulo Auth: 80%
- Tests unitarios para todos los aggregates
- Tests de handlers para flujos críticos (login, logout, refresh)
- Estado actual: 92 tests, 0 fallos ✅

---

## Compatibilidad (1 RNF)

### RNF-AUTH-015: Bases de Datos
- SQL Server: ✅ Completo
- PostgreSQL: ✅ Completo
- Seleccionable con `DatabaseSettings:Provider`

---

## Métricas Objetivo

| Métrica | Objetivo | Estado |
|---|---|---|
| Login (p95) | < 500ms | ✅ |
| Verificación blacklist Redis | < 5ms | ✅ |
| Cobertura tests Auth | > 80% | ✅ (92 tests) |
| Uptime | 99.9% | Depende del hosting |
| Escalabilidad | Multi-instancia con Redis | ✅ |

---

**Fecha:** 2026-04-15
