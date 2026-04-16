using CoreTemplate.Modules.Auth.Domain.Aggregates;
using CoreTemplate.Modules.Auth.Domain.Entities;
using CoreTemplate.Modules.Auth.Domain.Enums;
using CoreTemplate.SharedKernel;

namespace CoreTemplate.Modules.Auth.Domain.Repositories;

/// <summary>
/// Contrato del repositorio de usuarios.
/// </summary>
public interface IUsuarioRepository
{
    /// <summary>Obtiene un usuario por su ID incluyendo roles y refresh tokens.</summary>
    Task<Usuario?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Obtiene un usuario por su email (y tenant si es multi-tenant).</summary>
    Task<Usuario?> GetByEmailAsync(string email, Guid? tenantId = null, CancellationToken ct = default);

    /// <summary>Verifica si existe un usuario con el email indicado.</summary>
    Task<bool> ExistsByEmailAsync(string email, Guid? tenantId = null, CancellationToken ct = default);

    /// <summary>Obtiene el usuario que tiene el token de restablecimiento indicado (válido y no expirado).</summary>
    Task<Usuario?> GetByTokenRestablecimientoAsync(string token, CancellationToken ct = default);

    /// <summary>Obtiene una página de usuarios con filtros opcionales.</summary>
    Task<PagedResult<Usuario>> GetPagedAsync(int pagina, int tamanoPagina, EstadoUsuario? estado = null, CancellationToken ct = default);

    /// <summary>Agrega un nuevo usuario y persiste los cambios.</summary>
    Task AddAsync(Usuario usuario, CancellationToken ct = default);

    /// <summary>Actualiza un usuario existente y persiste los cambios.</summary>
    Task UpdateAsync(Usuario usuario, CancellationToken ct = default);
}

/// <summary>
/// Contrato del repositorio de roles.
/// </summary>
public interface IRolRepository
{
    /// <summary>Obtiene un rol por su ID incluyendo sus permisos.</summary>
    Task<Rol?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Obtiene todos los roles (del tenant si es multi-tenant).</summary>
    Task<List<Rol>> GetAllAsync(Guid? tenantId = null, CancellationToken ct = default);

    /// <summary>Verifica si existe un rol con el nombre indicado.</summary>
    Task<bool> ExistsByNombreAsync(string nombre, Guid? tenantId = null, CancellationToken ct = default);

    /// <summary>Agrega un nuevo rol y persiste los cambios.</summary>
    Task AddAsync(Rol rol, CancellationToken ct = default);

    /// <summary>Actualiza un rol existente y persiste los cambios.</summary>
    Task UpdateAsync(Rol rol, CancellationToken ct = default);

    /// <summary>Elimina un rol y persiste los cambios.</summary>
    Task DeleteAsync(Rol rol, CancellationToken ct = default);

    /// <summary>Verifica si el rol tiene usuarios asignados.</summary>
    Task<bool> TieneUsuariosAsync(Guid rolId, CancellationToken ct = default);
}

/// <summary>
/// Contrato del repositorio de permisos.
/// </summary>
public interface IPermisoRepository
{
    /// <summary>Obtiene un permiso por su ID.</summary>
    Task<Permiso?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Obtiene un permiso por su código.</summary>
    Task<Permiso?> GetByCodigoAsync(string codigo, CancellationToken ct = default);

    /// <summary>Obtiene todos los permisos del sistema.</summary>
    Task<List<Permiso>> GetAllAsync(CancellationToken ct = default);

    /// <summary>Obtiene los permisos de un módulo específico.</summary>
    Task<List<Permiso>> GetByModuloAsync(string modulo, CancellationToken ct = default);

    /// <summary>Agrega un nuevo permiso y persiste los cambios.</summary>
    Task AddAsync(Permiso permiso, CancellationToken ct = default);
}

/// <summary>
/// Contrato del repositorio de sesiones.
/// </summary>
public interface ISesionRepository
{
    /// <summary>Obtiene una sesión por su ID.</summary>
    Task<Sesion?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Obtiene una sesión activa por el hash del refresh token.</summary>
    Task<Sesion?> GetActivaByRefreshTokenHashAsync(string refreshTokenHash, CancellationToken ct = default);

    /// <summary>Obtiene todas las sesiones activas de un usuario.</summary>
    Task<List<Sesion>> GetActivasByUsuarioAsync(Guid usuarioId, CancellationToken ct = default);

    /// <summary>Cuenta las sesiones activas y no expiradas de un usuario.</summary>
    Task<int> ContarActivasAsync(Guid usuarioId, CancellationToken ct = default);

    /// <summary>Obtiene la sesión activa más antigua de un usuario.</summary>
    Task<Sesion?> GetMasAntiguaActivaAsync(Guid usuarioId, CancellationToken ct = default);

    /// <summary>Agrega una nueva sesión.</summary>
    Task AddAsync(Sesion sesion, CancellationToken ct = default);

    /// <summary>Actualiza una sesión existente.</summary>
    Task UpdateAsync(Sesion sesion, CancellationToken ct = default);

    /// <summary>Revoca todas las sesiones activas de un usuario.</summary>
    Task RevocarTodasAsync(Guid usuarioId, CancellationToken ct = default);

    /// <summary>Elimina sesiones expiradas más antiguas de X días.</summary>
    Task LimpiarExpiradosAsync(int diasAntiguedad = 30, CancellationToken ct = default);
}

/// <summary>
/// Contrato del repositorio de sucursales.
/// Solo se usa cuando OrganizationSettings:EnableBranches = true.
/// </summary>
public interface ISucursalRepository
{
    Task<Sucursal?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Sucursal?> GetByCodigoAsync(string codigo, Guid? tenantId = null, CancellationToken ct = default);
    Task<List<Sucursal>> GetAllAsync(Guid? tenantId = null, CancellationToken ct = default);
    Task<bool> ExistsByCodigoAsync(string codigo, Guid? tenantId = null, CancellationToken ct = default);
    Task AddAsync(Sucursal sucursal, CancellationToken ct = default);
    Task UpdateAsync(Sucursal sucursal, CancellationToken ct = default);
}

/// <summary>
/// Contrato del repositorio de asignaciones de rol por sucursal.
/// Solo se usa cuando OrganizationSettings:EnableBranches = true.
/// </summary>
public interface IAsignacionRolRepository
{
    Task<AsignacionRol?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<AsignacionRol>> GetByUsuarioSucursalAsync(Guid usuarioId, Guid sucursalId, CancellationToken ct = default);
    Task<List<AsignacionRol>> GetByUsuarioAsync(Guid usuarioId, CancellationToken ct = default);
    Task<bool> ExisteAsync(Guid usuarioId, Guid sucursalId, Guid rolId, CancellationToken ct = default);
    Task AddAsync(AsignacionRol asignacion, CancellationToken ct = default);
    Task DeleteAsync(AsignacionRol asignacion, CancellationToken ct = default);
}

/// <summary>
/// Contrato del repositorio de acciones del catálogo.
/// Solo se usa cuando AuthSettings:UseActionCatalog = true.
/// </summary>
public interface IAccionRepository
{
    Task<Accion?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Accion?> GetByCodigoAsync(string codigo, CancellationToken ct = default);
    Task<List<Accion>> GetAllAsync(string? modulo = null, CancellationToken ct = default);
    Task<bool> ExistsByCodigoAsync(string codigo, CancellationToken ct = default);
    Task AddAsync(Accion accion, CancellationToken ct = default);
    Task UpdateAsync(Accion accion, CancellationToken ct = default);
}

/// <summary>
/// Contrato del repositorio de configuración por tenant.
/// Solo se usa cuando TenantSettings:EnableSessionLimitsPerTenant = true.
/// </summary>
public interface IConfiguracionTenantRepository
{
    Task<ConfiguracionTenant?> GetByTenantIdAsync(Guid tenantId, CancellationToken ct = default);
    Task AddAsync(ConfiguracionTenant config, CancellationToken ct = default);
    Task UpdateAsync(ConfiguracionTenant config, CancellationToken ct = default);
}

/// <summary>
/// Contrato del repositorio de registros de auditoría.
/// </summary>
public interface IRegistroAuditoriaRepository
{
    /// <summary>
    /// Agrega un registro de auditoría.
    /// Los registros de auditoría son inmutables — solo se agregan, nunca se modifican.
    /// </summary>
    Task AddAsync(RegistroAuditoria registro, CancellationToken ct = default);

    /// <summary>Obtiene los registros de auditoría de un usuario (paginado).</summary>
    Task<PagedResult<RegistroAuditoria>> GetByUsuarioAsync(Guid usuarioId, int pagina, int tamanoPagina, CancellationToken ct = default);
}
