# Storage — Requerimientos No Funcionales

> **Building Block:** CoreTemplate.Storage
> **Fecha:** 2026-04-22

---

## RNF-STORAGE-001: Intercambiabilidad de proveedor
**Categoría:** Mantenibilidad

- Cambiar de proveedor requiere solo modificar `appsettings.json`
- Cero cambios en módulos consumidores
- La abstracción `IStorageService` no expone detalles de ningún proveedor
- Agregar un nuevo proveedor requiere solo: implementar `IStorageService` + registrar en DI

---

## RNF-STORAGE-002: Unicidad de nombres almacenados
**Categoría:** Integridad

- El nombre de almacenamiento se genera con GUID para evitar colisiones
- El nombre original se preserva en los metadatos (módulo Archivos), no en el path físico
- Dos archivos con el mismo nombre original nunca se sobreescriben entre sí
- Formato: `{guid}{extension}` → `3f2a1b4c-...-.pdf`

---

## RNF-STORAGE-003: Seguridad de acceso
**Categoría:** Seguridad

- Los archivos NO son públicos por defecto
- Para S3: acceso solo via URLs firmadas con expiración
- Para Firebase: acceso solo via tokens de descarga
- Para Local: acceso solo via endpoint autenticado del sistema
- Las credenciales del proveedor NUNCA van en código — solo en configuración/variables de entorno

---

## RNF-STORAGE-004: Rendimiento de subida
**Categoría:** Rendimiento

- La subida es siempre asíncrona (`async/await`)
- Usa streaming — no carga el archivo completo en memoria antes de subir
- Para S3: usa multipart upload para archivos mayores a 5 MB (automático via SDK)
- No bloquea el hilo del request HTTP del módulo consumidor

---

## RNF-STORAGE-005: Aislamiento de fallos
**Categoría:** Resiliencia

- Un fallo en el almacenamiento retorna `StorageResult { Exitoso = false }`
- No lanza excepciones al consumidor
- El consumidor decide si el fallo es crítico o puede reintentarse
- Si el proveedor externo no está disponible → fallo rápido con mensaje claro

---

## RNF-STORAGE-006: Organización lógica por contexto
**Categoría:** Mantenibilidad

- El `contexto` define la estructura de carpetas: `"rrhh/candidatos/cv"`
- Todos los proveedores respetan la misma estructura lógica de carpetas
- Migrar de Local a S3 preserva la misma organización de archivos
- El contexto es libre — cada módulo define su propia jerarquía

---

## RNF-STORAGE-007: Observabilidad
**Categoría:** Operabilidad

- Cada operación genera entrada en log estructurado
- Compatible con `X-Correlation-Id` de `CoreTemplate.Logging`
- Métricas básicas: archivos subidos, tamaño total, fallos (preparado para CloudWatch/Prometheus)

---

## RNF-STORAGE-008: Extensibilidad
**Categoría:** Mantenibilidad

- Agregar Azure Blob Storage no requiere cambiar la abstracción ni los consumidores
- Agregar compresión automática no requiere cambiar los consumidores
- Agregar virus scanning (ClamAV, etc.) se puede agregar como decorador de `IStorageService`
- Agregar CDN (CloudFront) se puede configurar a nivel de proveedor S3 sin cambiar contratos

---

**Fecha:** 2026-04-22
