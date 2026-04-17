# Stack Tecnológico

## Backend
- ASP.NET Core 10
- SQL Server
- Entity Framework Core 10
- MediatR (CQRS)
- FluentValidation (validaciones)
- Mapster o AutoMapper (mapeo DTOs)

## Frontend
- React JS 18+
- Vite
- TypeScript
- React Router (navegación)
- TanStack Query / React Query (estado servidor)
- Zustand o Redux Toolkit (estado global)
- Axios (HTTP client)
- React Hook Form + Zod (formularios y validación)
- TailwindCSS o Material-UI (UI)

# Arquitectura

## Patrón Arquitectónico
- Arquitectura Modular Monolítica
- Clean Architecture por módulo
- Domain-Driven Design (DDD)
- CQRS con MediatR
- Multi-Tenant SaaS

## Estructura de Capas por Módulo
Cada módulo sigue la estructura:
- **Domain**: Agregados, Entidades, Value Objects, Reglas de Negocio, Repositorios (interfaces)
- **Application**: Commands, Queries, DTOs, Handlers (MediatR), Validators
- **Infrastructure**: DbContext, Repositories (implementación), Migrations, Servicios externos
- **Api**: Controllers, Contracts (Request/Response)

## Organización del Proyecto
```
LunaERP/
├── src/
│   ├── BuildingBlocks/          # Infraestructura transversal
│   │   ├── SharedKernel/
│   │   ├── Auditing/
│   │   ├── Logging/
│   │   ├── Monitoring/
│   │   ├── Common/
│   │   └── Infrastructure/
│   ├── Host/
│   │   └── LunaERP.Api/         # Entry point
│   └── Modules/                 # Módulos de negocio
│       ├── Plataforma/
│       │   ├── Security/
│       │   ├── Configuracion/
│       │   ├── SaasAdmin/
│       │   └── Integraciones/
│       ├── VentasYCRM/
│       ├── CadenaSuministro/
│       ├── Finanzas/
│       └── [otros módulos]/
└── tests/
```

## Módulos del ERP

### Módulos Principales
1. **VentasYCRM**: Clientes, Leads, Cotizaciones, Pedidos, Facturación, POS
2. **CadenaSuministro**: Productos, Inventario, Compras, Proveedores, Logística
3. **Manufactura**: BOM, MRP, Producción
4. **Finanzas**: Contabilidad, CxC, CxP, Tesorería, Activos Fijos
5. **RecursosHumanos**: Personal, Asistencia, Nómina, Reclutamiento
6. **Analytics**: Reportes, BI, KPIs
7. **Proyectos**: Gestión de Proyectos, Timesheet, Órdenes de Servicio

### Módulo Plataforma (Transversal)
- **Security**: Autenticación JWT, Usuarios, Roles, Permisos, MFA
- **Configuracion**: Empresas, Sucursales, Catálogos, Parámetros
- **SaasAdmin**: Tenants, Planes, Límites, Facturación SaaS
- **Integraciones**: API REST, Webhooks

## Características Transversales

### Multi-tenancy
- Tenant por cliente SaaS
- 1 Tenant puede tener múltiples empresas (según plan)
- Aislamiento de datos por TenantId
- Aislamiento adicional por EmpresaId dentro del tenant
- Configuración por tenant (zona horaria, país, moneda)

### Tipos de Usuarios
- **Admin Principal**: EmpresaId = null, acceso a todas las empresas del tenant
- **Usuario Multi-Empresa**: EmpresaId = null, AccesoEmpresas = [1,2], acceso a empresas específicas
- **Usuario de Empresa**: EmpresaId = 1, solo accede a una empresa

### Auditoría
- Registro automático de todas las acciones (quién, qué, cuándo, dónde)
- Interceptor en SaveChanges (AuditSaveChangesInterceptor)
- Retención de 7 años
- Registro por propiedad modificada (valor anterior/nuevo)
- Filtrado por Tenant, Usuario, Entidad, Fecha

### Logging
- Logs estructurados (JSON)
- Niveles: Info, Warning, Error, Critical
- Correlation ID por request
- Enriquecimiento automático: TenantId, UserId, CorrelationId
- Rotación de logs

### Monitoring
- Health Checks (Database, Redis, External APIs)
- Métricas: Tiempo de respuesta, Requests por tenant, Errores
- Dashboard de infraestructura
- Alertas automáticas

### Time Management
- Proveedor centralizado de fecha/hora (IDateTimeProvider)
- Almacenamiento siempre en UTC
- Conversión a zona horaria del tenant solo en presentación
- Mockeable para testing
- NO usar DateTime.Now directamente

### Seguridad
- Autenticación JWT
- Autorización basada en roles y permisos
- Políticas de contraseña
- MFA opcional
- Middleware de validación Tenant + Empresa
- Header: X-Empresa-Id para selección de empresa

### Validaciones Dinámicas por País
- Strategy Pattern para validaciones nacionales
- INationalIdValidator por país
- Resolver dinámico por código de país
- Extensible sin modificar código (Open/Closed)
- Ejemplo: NicaraguaNationalIdValidator, CostaRicaNationalIdValidator

### Manejo de Errores
- Result Pattern (Result<T>) para operaciones de dominio
- NO usar excepciones para flujo de negocio
- Excepciones solo para errores técnicos inesperados
- Middleware global de manejo de excepciones
- Códigos de error estandarizados

### Transacciones
- Unit of Work pattern
- Transacciones por comando (CommandHandler)
- Eventos de dominio dentro de la misma transacción
- Outbox pattern para eventos externos (futuro)

### Caché
- Redis para caché distribuido (futuro)
- Memory Cache para datos de configuración
- Cache por tenant
- Invalidación automática en cambios

## Convenciones de Nombres

### Proyectos
- `LunaERP.Modules.{Modulo}.{Capa}`
- Ejemplo: `LunaERP.Modules.Plataforma.Configuracion.Domain`
- BuildingBlocks: `LunaERP.{Componente}` (ej: LunaERP.Auditing)

### Namespaces
- Seguir estructura de carpetas
- Ejemplo: `LunaERP.Modules.Plataforma.Configuracion.Domain.Aggregates.Empresa`

### Base de Datos
- Tablas: PascalCase (Empresas, Sucursales)
- Columnas: PascalCase (EmpresaId, Nombre)
- Schema por módulo: [Configuracion].[Empresas]
- Todas las fechas en UTC (datetime2)
- Todas las tablas con TenantId
- Índices en TenantId + EmpresaId
- Soft Delete: IsDeleted, DeletedAt, DeletedBy
- Auditoría automática: CreatedAt, CreatedBy, UpdatedAt, UpdatedBy
- Claves primarias: int o Guid según necesidad
- Relaciones: FK con ON DELETE RESTRICT (nunca CASCADE)

## Reglas de Dependencias

1. Domain NO depende de nadie (solo SharedKernel)
2. Application solo depende de Domain
3. Infrastructure depende de Domain y Application
4. Api depende de Application
5. Host referencia Api e Infrastructure de cada módulo
6. Módulos NO se referencian directamente entre sí (usar eventos de dominio)
7. BuildingBlocks NO dependen de módulos funcionales
8. Todos los módulos pueden depender de BuildingBlocks

## Comunicación entre Módulos
- Eventos de Dominio (MediatR INotification)
- Servicios de Aplicación (interfaces en SharedKernel)
- NO referencias directas entre módulos
- Ejemplo: EmpresaCreadaEvent → VentasYCRM crea configuración inicial

## Gestión de Planes y Límites

### Planes SaaS
- Básico: 1 empresa, 10 usuarios, 5 sucursales
- Profesional: 3 empresas, 50 usuarios, 10 sucursales
- Enterprise: Ilimitado

### Validación de Límites
- Validar ANTES de crear recursos (empresa, usuario, sucursal)
- Mostrar uso actual vs límite en dashboard
- Alertar al 80% del límite
- Bloquear creación si se alcanza el límite

### Facturación
- 1 Tenant = 1 Factura mensual
- Precio según plan contratado
- Independiente del número de empresas (dentro del límite)

## Registro de Módulos (DI)

```csharp
// Program.cs
builder.Services.AddAuditing(configuration);
builder.Services.AddLogging(configuration);
builder.Services.AddMonitoring(configuration);
builder.Services.AddSecurityModule(configuration);
builder.Services.AddConfiguracionModule(configuration);
```

## Fases de Implementación

### Fase 1A - Fundación MVP (Semanas 1-2)
**Objetivo:** Base técnica funcional

**BuildingBlocks:**
- SharedKernel: Result Pattern, IDateTimeProvider, ITenantContext
- Auditing: Registro básico (quién, qué, cuándo) - sin UI
- Logging: Console + File (Serilog básico)

**Módulo Security MVP:**
- Login/Logout con JWT
- CRUD Usuarios (sin permisos granulares)
- 3 Roles fijos: SuperAdmin, TenantAdmin, Usuario
- Validación básica de contraseña (min 8 caracteres)
- Middleware Tenant/Empresa

**Entregable:** Puedes hacer login y crear usuarios

---

### Fase 1B - Configuración MVP (Semanas 3-4)
**Objetivo:** Multi-tenant funcional

**Módulo Configuración MVP:**
- CRUD Empresas (Nombre, RUC, País)
- CRUD Sucursales (Nombre, Dirección)
- Catálogos básicos: Países, Departamentos, Monedas
- Validación de cédula Nicaragua (INationalIdValidator)
- Selector de empresa en UI

**Entregable:** Multi-empresa funciona, puedes crear empresas y sucursales

---

### Fase 2 - Núcleo Comercial MVP (Semanas 5-8)
**Objetivo:** Gestión básica de inventario y compras

**Módulo CadenaSuministro MVP:**
- CRUD Clientes (Nombre, Email, Teléfono, Tipo)
- CRUD Productos (Código, Nombre, Precio, Categoría)
- Inventario simple: Entradas/Salidas manuales (un solo almacén)
- CRUD Proveedores (Nombre, Email, Teléfono)
- Orden de Compra básica (sin aprobaciones)

**Entregable:** Puedes registrar productos, clientes y hacer compras

---

### Fase 3 - Ventas MVP (Semanas 9-12) 🎯 SISTEMA USABLE
**Objetivo:** Facturación funcional

**Módulo VentasYCRM MVP:**
- Factura simple (Cliente, Productos, Total)
- POS básico: Venta rápida (sin impresora fiscal)
- Devoluciones simples
- 3 Reportes esenciales:
  - Ventas del día
  - Productos más vendidos
  - Top 10 clientes

**Entregable:** 🎉 ERP FUNCIONAL - Puedes vender y facturar

---

### Fase 4A - SaaS Admin (Semanas 13-14)
**Objetivo:** Monetización

**Módulo SaasAdmin MVP:**
- CRUD Tenants
- 3 Planes: Básico, Profesional, Enterprise
- Validación de límites (MaxEmpresas, MaxUsuarios)
- Dashboard de uso por tenant
- Suspensión/Reactivación de tenants

**Entregable:** Puedes vender el ERP como SaaS

---

### Fase 4B - Estabilización (Semanas 15-16)
**Objetivo:** Calidad y confiabilidad

**Mejoras Técnicas:**
- Monitoring: Health Checks + Application Insights
- Tests: Unit tests para Domain (cobertura 60%+)
- Performance: Índices en queries lentas
- Security: Recuperación de contraseña
- UI: Mejoras de UX basadas en feedback

**Entregable:** Sistema estable para producción

---

### Fase 5 - Ventas Funcional (Semanas 17-20)
**Objetivo:** Ventas completas

**Módulo VentasYCRM Funcional:**
- Cotizaciones (con conversión a factura)
- Pedidos (con estados: Pendiente, Aprobado, Entregado)
- Notas de crédito
- Descuentos y promociones
- 10 reportes adicionales
- Integración con factura electrónica (país específico)

**Entregable:** Proceso de ventas completo

---

### Fase 6 - Inventario Funcional (Semanas 21-24)
**Objetivo:** Inventario multi-almacén

**Módulo CadenaSuministro Funcional:**
- Múltiples almacenes
- Transferencias entre almacenes
- Kardex por producto
- Inventario físico (conteo)
- Alertas de stock mínimo
- Lotes y fechas de vencimiento
- Trazabilidad

**Entregable:** Control de inventario profesional

---

### Fase 7 - Finanzas MVP (Semanas 25-28)
**Objetivo:** Contabilidad básica

**Módulo Finanzas MVP:**
- Plan de cuentas
- Asientos contables manuales
- Libro diario
- Balance general
- Estado de resultados
- CxC: Registro de facturas por cobrar + pagos
- CxP: Registro de facturas por pagar + pagos

**Entregable:** Contabilidad funcional

---

### Fase 8 - CRM Funcional (Semanas 29-32)
**Objetivo:** Gestión comercial

**Módulo VentasYCRM - CRM:**
- Leads (prospección)
- Oportunidades de venta
- Pipeline comercial
- Seguimiento de actividades
- Conversión de lead a cliente
- Reportes de conversión

**Entregable:** CRM integrado con ventas

---

### Fase 9+ - Expansión (Mes 9+)
**Priorizar según demanda de clientes:**

**Opción A - RRHH MVP:**
- Empleados
- Asistencia
- Nómina básica

**Opción B - Manufactura MVP:**
- BOM (Lista de materiales)
- Órdenes de producción
- Consumo de materiales

**Opción C - Proyectos MVP:**
- Proyectos
- Tareas
- Timesheet
- Facturación por proyecto

**Opción D - Analytics:**
- BI Dashboard
- KPIs configurables
- Reportes personalizados

---

## 📊 Resumen de Hitos

| Fase | Semanas | Hito | Estado |
|------|---------|------|--------|
| 1A | 1-2 | Login funciona | Fundación |
| 1B | 3-4 | Multi-empresa funciona | Fundación |
| 2 | 5-8 | Inventario y compras | Core |
| 3 | 9-12 | **🎯 SISTEMA USABLE** | **MVP Completo** |
| 4A | 13-14 | SaaS monetizable | Monetización |
| 4B | 15-16 | Sistema estable | Calidad |
| 5 | 17-20 | Ventas completas | Funcional |
| 6 | 21-24 | Inventario avanzado | Funcional |
| 7 | 25-28 | Contabilidad | Expansión |
| 8 | 29-32 | CRM | Expansión |
| 9+ | 33+ | Según demanda | Crecimiento |

## Testing

### Estrategia de Testing
- **Unit Tests**: Lógica de dominio, handlers, validators
- **Integration Tests**: Repositorios, DbContext, APIs
- **Architecture Tests**: NetArchTest para validar reglas de dependencias
- **E2E Tests**: Flujos críticos de usuario (Playwright)

### Cobertura Mínima
- Domain: 80%+
- Application: 70%+
- Infrastructure: 50%+

### Herramientas
- xUnit
- FluentAssertions
- Moq o NSubstitute
- Testcontainers (SQL Server para integration tests)
- NetArchTest.Rules

## Performance

### Optimizaciones
- Paginación obligatoria en listados (PagedResult<T>)
- Proyecciones en queries (Select solo campos necesarios)
- AsNoTracking() en queries de solo lectura
- Índices en columnas de búsqueda frecuente
- Caché para catálogos y configuraciones

### Monitoreo
- Application Insights o similar
- Queries lentas (> 1 segundo)
- Endpoints lentos (> 500ms)
- Memory leaks

## Seguridad Adicional

### Protección
- Rate Limiting por tenant
- CORS configurado correctamente
- HTTPS obligatorio
- SQL Injection: usar siempre parámetros
- XSS: sanitizar inputs en frontend
- CSRF: tokens en formularios

### Datos Sensibles
- Encriptar contraseñas (BCrypt o Argon2)
- NO guardar datos de tarjetas (usar gateway de pago)
- Enmascarar datos sensibles en logs
- Secrets en Azure Key Vault o similar

## Internacionalización (i18n)

### Backend
- Recursos por módulo (.resx files)
- IStringLocalizer<T> para traducciones
- Cultura por tenant (es-NI, es-CR, en-US)
- Mensajes de error traducidos
- Validaciones en idioma del usuario

### Frontend
- react-i18next o similar
- Archivos de traducción por módulo (JSON)
- Detección automática de idioma del navegador
- Selector de idioma en UI
- Formato de números/fechas/moneda por cultura

### Formatos por Cultura
- Fechas: dd/MM/yyyy (Nicaragua), MM/dd/yyyy (USA)
- Números: 1.234,56 (Nicaragua), 1,234.56 (USA)
- Moneda: C$ 1,234.56, $ 1,234.56
- Zona horaria por tenant

## Migraciones y Versionado

### Migraciones de Base de Datos
- Migraciones por módulo (separadas)
- Nombrar: YYYYMMDDHHMMSS_DescripcionCambio
- Script de migración para producción (SQL)
- Rollback plan para cada migración
- Ejecutar migraciones en orden de dependencias

### Versionado de API
- Versionado por URL: /api/v1/empresas, /api/v2/empresas
- Mantener v1 mientras haya clientes usándola
- Deprecation warnings en headers
- Documentación por versión (Swagger)
- Breaking changes solo en versiones mayores

### Control de Versiones
- Git con GitFlow o GitHub Flow
- Branches: main, develop, feature/*, hotfix/*
- Commits semánticos: feat:, fix:, refactor:, docs:
- Tags para releases: v1.0.0, v1.1.0
- Changelog automático

## Deployment

### Entorno IIS
- Windows Server 2019+
- IIS 10+
- .NET 8 Runtime instalado
- Application Pool dedicado por ambiente
- Identity: ApplicationPoolIdentity o cuenta de servicio

### Ambientes

#### Development (Local)
- appsettings.Development.json
- SQL Server LocalDB o instancia local
- Logs en consola
- Hot reload habilitado

#### QA/Staging (IIS)
- appsettings.Staging.json
- SQL Server dedicado (QA)
- Logs en archivos + Application Insights
- Datos de prueba
- URL: https://qa.lunaerp.com

#### Production (IIS)
- appsettings.Production.json
- SQL Server en alta disponibilidad
- Logs en Application Insights
- Secrets en Azure Key Vault
- URL: https://app.lunaerp.com

### Configuración IIS

```xml
<!-- web.config -->
<configuration>
  <system.webServer>
    <handlers>
      <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" />
    </handlers>
    <aspNetCore processPath="dotnet" 
                arguments=".\LunaERP.Api.dll" 
                stdoutLogEnabled="true" 
                stdoutLogFile=".\logs\stdout" 
                hostingModel="inprocess" />
  </system.webServer>
</configuration>
```

### Application Pool Settings
- .NET CLR Version: No Managed Code
- Managed Pipeline Mode: Integrated
- Start Mode: AlwaysRunning
- Idle Timeout: 0 (no timeout)
- Regular Time Interval: 1740 (29 horas)

### CI/CD Pipeline

#### Build
```bash
1. Restore packages: dotnet restore
2. Build: dotnet build --configuration Release
3. Run tests: dotnet test
4. Publish: dotnet publish -c Release -o ./publish
```

#### Deploy a IIS
```bash
1. Detener Application Pool
2. Backup de archivos actuales
3. Copiar nuevos archivos a wwwroot
4. Ejecutar migraciones de BD
5. Iniciar Application Pool
6. Smoke tests
```

### Herramientas de Deploy
- Web Deploy (MSDeploy)
- PowerShell scripts
- Azure DevOps Pipelines
- GitHub Actions
- Jenkins

### Estrategia de Deploy
- Deploy fuera de horario laboral (madrugada)
- Ventana de mantenimiento comunicada
- Rollback automático si falla smoke test
- Monitoreo post-deploy (30 minutos)

## Backup y Recuperación
### Backup de Base de Datos

#### Frecuencia
- **Full Backup**: Diario a las 2:00 AM
- **Differential Backup**: Cada 6 horas
- **Transaction Log Backup**: Cada 15 minutos

#### Retención
- Últimos 7 días: Todos los backups
- Últimos 30 días: Backups diarios
- Últimos 12 meses: Backups mensuales (primer día del mes)
- Legal: 7 años (backups anuales)

#### Ubicación
- Primaria: Disco local del servidor SQL
- Secundaria: NAS o almacenamiento en red
- Terciaria: Azure Blob Storage o S3 (offsite)

### Backup de Archivos
- Archivos subidos por usuarios
- Logs de aplicación
- Configuraciones (appsettings, web.config)
- Certificados SSL

### Plan de Recuperación ante Desastres (DRP)

#### RTO (Recovery Time Objective)
- Crítico: 4 horas
- Alta prioridad: 8 horas
- Media prioridad: 24 horas

#### RPO (Recovery Point Objective)
- Máximo 15 minutos de pérdida de datos

#### Procedimiento de Recuperación
1. Evaluar alcance del desastre
2. Notificar a stakeholders
3. Restaurar último backup válido
4. Aplicar transaction logs
5. Verificar integridad de datos
6. Ejecutar smoke tests
7. Comunicar restauración exitosa

### Testing de Backups
- Restauración de prueba: Mensual
- Simulacro de desastre: Trimestral
- Documentar resultados y tiempos
- Actualizar procedimientos según aprendizajes

### Monitoreo de Backups
- Alertas si backup falla
- Verificación automática de integridad
- Dashboard de estado de backups
- Notificaciones a equipo de infraestructura

## Monitoreo y Observabilidad

### Métricas Clave (KPIs Técnicos)
- Uptime: 99.9% (objetivo)
- Response Time: < 500ms (p95)
- Error Rate: < 0.1%
- Requests por segundo
- Usuarios concurrentes
- Uso de CPU/RAM/Disco

### Alertas
- Error rate > 1%: Alerta inmediata
- Response time > 2s: Alerta warning
- CPU > 80%: Alerta warning
- Disco > 85%: Alerta crítica
- Backup fallido: Alerta crítica
- Certificado SSL próximo a vencer: 30 días antes

### Herramientas
- Application Insights (Azure)
- Seq o ELK Stack (logs)
- Grafana + Prometheus (métricas)
- UptimeRobot (monitoreo externo)
- PagerDuty (alertas)

## Documentación

### Documentación Técnica
- Arquitectura (diagramas C4)
- Decisión de diseño (ADRs)
- Guías de desarrollo
- Convenciones de código
- Procedimientos de deploy

### Documentación de API
- Swagger/OpenAPI automático
- Ejemplos de requests/responses
- Códigos de error
- Rate limits
- Autenticación

### Documentación de Usuario
- Manual de usuario por módulo
- Videos tutoriales
- FAQs
- Release notes
- Changelog