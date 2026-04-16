# Event Storming — Context Mapping

> **Fecha:** 2026-04-15

---

## Relaciones entre Bounded Contexts

```
┌─────────────────────────────────────────────────────────────┐
│                    CONTEXT MAP                              │
│                                                             │
│   ┌─────────┐    Partnership    ┌───────────────┐          │
│   │   IAM   │ ←──────────────→ │ Authorization │          │
│   └─────────┘                  └───────────────┘          │
│        │                              │                    │
│        │ Customer/Supplier            │ Customer/Supplier  │
│        ↓                              ↓                    │
│   ┌─────────────────────────────────────────────┐         │
│   │              Configuration                  │         │
│   └─────────────────────────────────────────────┘         │
│                                                             │
│   ┌─────────────────────────────────────────────┐         │
│   │         Servicios de Infraestructura        │         │
│   │  TokenBlacklist | JWT | BCrypt | TOTP       │         │
│   └─────────────────────────────────────────────┘         │
│              ↑ Conformist (todos los BC usan)              │
└─────────────────────────────────────────────────────────────┘
```

## Relaciones Detalladas

| Upstream | Downstream | Tipo | Descripción |
|---|---|---|---|
| Authorization | IAM | Partnership | Usuario necesita roles y sucursales para autenticarse |
| IAM | Authorization | Partnership | Roles y sucursales se asignan a usuarios |
| IAM | Configuration | Customer/Supplier | SesionService consulta límites de ConfiguracionTenant |
| JWT Service | IAM | Conformist | IAM usa JWT para generar tokens |
| JWT Service | Authorization | Conformist | Authorization incluye roles y branch_id en JWT |
| TokenBlacklist | IAM | Conformist | IAM agrega tokens a blacklist al logout |

---

**Fecha:** 2026-04-15
