# Configuración del Sistema — Requerimientos No Funcionales

> **Fecha:** 2026-04-22

---

## RNF-CFG-001: Cache obligatorio
Los parámetros se leen en cada request (nombre empresa en PDFs, moneda en facturas).
Sin cache → consulta a BD en cada operación.
`IMemoryCache` con TTL de 10 minutos. Al actualizar → invalidar esa clave.

## RNF-CFG-002: Nunca lanza excepción al consumidor
`IConfiguracionService` siempre retorna un valor.
Si la clave no existe → retorna `valorPorDefecto`.
Los módulos consumidores no necesitan try/catch.

## RNF-CFG-003: Parámetros sensibles NO van aquí
Esta tabla es para parámetros de negocio visibles en la UI.
Credenciales, API keys, secrets → variables de entorno o AWS Secrets Manager.

## RNF-CFG-004: Multi-tenant transparente
El consumidor no sabe si el valor viene del tenant o del global.
`IConfiguracionService` resuelve la jerarquía internamente.

## RNF-CFG-005: Extensibilidad
Agregar un nuevo parámetro = agregar una fila al seeder.
No requiere cambios en el código del servicio ni migraciones adicionales.

---

**Fecha:** 2026-04-22
