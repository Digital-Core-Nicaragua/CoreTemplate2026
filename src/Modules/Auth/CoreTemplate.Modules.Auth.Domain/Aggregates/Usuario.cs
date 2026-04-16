using CoreTemplate.Modules.Auth.Domain.Entities;
using CoreTemplate.Modules.Auth.Domain.Enums;
using CoreTemplate.Modules.Auth.Domain.Events;
using CoreTemplate.Modules.Auth.Domain.ValueObjects;
using CoreTemplate.SharedKernel;
using CoreTemplate.SharedKernel.Domain;

namespace CoreTemplate.Modules.Auth.Domain.Aggregates;

/// <summary>
/// Aggregate Root que representa un usuario del sistema.
/// <para>
/// Gestiona el ciclo de vida completo del usuario: registro, activación,
/// autenticación, bloqueo, cambio de contraseña, 2FA y roles asignados.
/// </para>
/// <para>
/// El dominio nunca trabaja con contraseñas en texto plano.
/// La capa de Infrastructure genera el hash y lo pasa como <see cref="PasswordHash"/>.
/// </para>
/// </summary>
public sealed class Usuario : AggregateRoot<Guid>
{
    /// <summary>ID del tenant al que pertenece. Null si single-tenant.</summary>
    public Guid? TenantId { get; private set; }

    /// <summary>Email del usuario (único por tenant).</summary>
    public Email Email { get; private set; } = null!;

    /// <summary>Nombre completo del usuario.</summary>
    public string Nombre { get; private set; } = string.Empty;

    /// <summary>Hash BCrypt de la contraseña.</summary>
    public PasswordHash PasswordHash { get; private set; } = null!;

    /// <summary>Tipo de usuario: Humano, Sistema o Integracion.</summary>
    public TipoUsuario TipoUsuario { get; private set; }

    /// <summary>Estado actual del usuario.</summary>
    public EstadoUsuario Estado { get; private set; }

    /// <summary>Contador de intentos de login fallidos consecutivos.</summary>
    public int IntentosFallidos { get; private set; }

    /// <summary>Fecha hasta la que la cuenta está bloqueada. Null si no está bloqueada.</summary>
    public DateTime? BloqueadoHasta { get; private set; }

    /// <summary>Indica si el usuario tiene 2FA configurado y activo.</summary>
    public bool TwoFactorActivo { get; private set; }

    /// <summary>Clave secreta TOTP para 2FA (encriptada en BD). Null si 2FA no está activo.</summary>
    public string? TwoFactorSecretKey { get; private set; }

    /// <summary>Fecha del último login exitoso.</summary>
    public DateTime? UltimoAcceso { get; private set; }

    /// <summary>Fecha de creación del usuario.</summary>
    public DateTime CreadoEn { get; private set; }

    /// <summary>Fecha de última modificación.</summary>
    public DateTime? ModificadoEn { get; private set; }

    private readonly List<UsuarioRol> _roles = [];
    private readonly List<TokenRestablecimiento> _tokensRestablecimiento = [];
    private readonly List<CodigoRecuperacion2FA> _codigosRecuperacion = [];
    private readonly List<UsuarioSucursal> _sucursales = [];

    /// <summary>Roles asignados al usuario.</summary>
    public IReadOnlyList<UsuarioRol> Roles => _roles.AsReadOnly();

    /// <summary>Tokens de restablecimiento de contraseña.</summary>
    public IReadOnlyList<TokenRestablecimiento> TokensRestablecimiento => _tokensRestablecimiento.AsReadOnly();

    /// <summary>Códigos de recuperación de 2FA.</summary>
    public IReadOnlyList<CodigoRecuperacion2FA> CodigosRecuperacion => _codigosRecuperacion.AsReadOnly();

    /// <summary>Sucursales asignadas al usuario (solo cuando EnableBranches = true).</summary>
    public IReadOnlyList<UsuarioSucursal> Sucursales => _sucursales.AsReadOnly();

    private Usuario() { }

    // ─── Factory method ───────────────────────────────────────────────────────

    /// <summary>
    /// Crea un nuevo usuario en estado <see cref="EstadoUsuario.Pendiente"/>.
    /// </summary>
    /// <param name="email">Email validado del usuario.</param>
    /// <param name="nombre">Nombre completo.</param>
    /// <param name="passwordHash">Hash BCrypt de la contraseña.</param>
    /// <param name="tenantId">ID del tenant. Null si single-tenant.</param>
    public static Result<Usuario> Crear(
        Email email,
        string nombre,
        PasswordHash passwordHash,
        Guid? tenantId = null,
        TipoUsuario tipoUsuario = TipoUsuario.Humano)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            return Result<Usuario>.Failure("El nombre del usuario es requerido.");
        }

        if (nombre.Trim().Length > 100)
        {
            return Result<Usuario>.Failure("El nombre no puede superar los 100 caracteres.");
        }

        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Email = email,
            Nombre = nombre.Trim(),
            PasswordHash = passwordHash,
            TipoUsuario = tipoUsuario,
            Estado = EstadoUsuario.Pendiente,
            IntentosFallidos = 0,
            TwoFactorActivo = false,
            CreadoEn = DateTime.UtcNow
        };

        usuario.RaiseDomainEvent(new UsuarioRegistradoEvent(
            usuario.Id, email.Valor, usuario.Nombre, tenantId));

        return Result<Usuario>.Success(usuario);
    }

    // ─── Estado ───────────────────────────────────────────────────────────────

    /// <summary>Activa el usuario permitiéndole autenticarse.</summary>
    public Result Activar()
    {
        if (Estado == EstadoUsuario.Activo)
        {
            return Result.Failure("El usuario ya está activo.");
        }

        Estado = EstadoUsuario.Activo;
        ModificadoEn = DateTime.UtcNow;
        RaiseDomainEvent(new UsuarioActivadoEvent(Id, Email.Valor));
        return Result.Success();
    }

    /// <summary>Desactiva el usuario impidiéndole autenticarse.</summary>
    public Result Desactivar()
    {
        if (Estado == EstadoUsuario.Inactivo)
        {
            return Result.Failure("El usuario ya está inactivo.");
        }

        Estado = EstadoUsuario.Inactivo;
        ModificadoEn = DateTime.UtcNow;
        RaiseDomainEvent(new UsuarioDesactivadoEvent(Id, Email.Valor));
        return Result.Success();
    }

    /// <summary>
    /// Bloquea el usuario hasta la fecha indicada.
    /// Se llama automáticamente al superar el límite de intentos fallidos.
    /// </summary>
    public Result Bloquear(DateTime hasta)
    {
        if (hasta <= DateTime.UtcNow)
        {
            return Result.Failure("La fecha de bloqueo debe ser futura.");
        }

        Estado = EstadoUsuario.Bloqueado;
        BloqueadoHasta = hasta;
        ModificadoEn = DateTime.UtcNow;
        RaiseDomainEvent(new UsuarioBloqueadoEvent(Id, Email.Valor, hasta));
        return Result.Success();
    }

    /// <summary>Desbloquea el usuario y resetea el contador de intentos.</summary>
    public Result Desbloquear()
    {
        if (Estado != EstadoUsuario.Bloqueado)
        {
            return Result.Failure("El usuario no está bloqueado.");
        }

        Estado = EstadoUsuario.Activo;
        BloqueadoHasta = null;
        IntentosFallidos = 0;
        ModificadoEn = DateTime.UtcNow;
        RaiseDomainEvent(new UsuarioDesbloqueadoEvent(Id, Email.Valor));
        return Result.Success();
    }

    // ─── Intentos fallidos ────────────────────────────────────────────────────

    /// <summary>
    /// Incrementa el contador de intentos fallidos.
    /// Si alcanza <paramref name="maxIntentos"/>, bloquea la cuenta automáticamente.
    /// </summary>
    /// <param name="maxIntentos">Límite de intentos antes del bloqueo.</param>
    /// <param name="minutosBloqueado">Duración del bloqueo en minutos.</param>
    public void IncrementarIntentosFallidos(int maxIntentos, int minutosBloqueado)
    {
        IntentosFallidos++;
        ModificadoEn = DateTime.UtcNow;

        if (IntentosFallidos >= maxIntentos)
        {
            Bloquear(DateTime.UtcNow.AddMinutes(minutosBloqueado));
        }
    }

    /// <summary>Resetea el contador de intentos fallidos tras un login exitoso.</summary>
    public void ResetearIntentosFallidos()
    {
        IntentosFallidos = 0;
        ModificadoEn = DateTime.UtcNow;
    }

    // ─── Contraseña ───────────────────────────────────────────────────────────

    /// <summary>Cambia la contraseña del usuario. Las sesiones activas se revocan desde el handler.</summary>
    public Result CambiarPassword(PasswordHash nuevoHash)
    {
        PasswordHash = nuevoHash;
        ModificadoEn = DateTime.UtcNow;
        RaiseDomainEvent(new PasswordCambiadoEvent(Id, Email.Valor));
        return Result.Success();
    }

    // ─── Acceso ───────────────────────────────────────────────────────────────

    /// <summary>Registra la fecha del último acceso exitoso.</summary>
    public void RegistrarAcceso()
    {
        UltimoAcceso = DateTime.UtcNow;
        ModificadoEn = DateTime.UtcNow;
    }

    // ─── Restablecimiento de contraseña ──────────────────────────────────────

    /// <summary>Agrega un token de restablecimiento de contraseña.</summary>
    public void AgregarTokenRestablecimiento(string token, int horasExpiracion)
    {
        _tokensRestablecimiento.Add(
            TokenRestablecimiento.Crear(Id, token, horasExpiracion));

        RaiseDomainEvent(new RestablecimientoSolicitadoEvent(
            Id,
            Email.Valor,
            token,
            DateTime.UtcNow.AddHours(horasExpiracion)));
    }

    /// <summary>
    /// Usa el token de restablecimiento indicado marcándolo como usado.
    /// </summary>
    /// <returns>True si el token es válido y fue marcado como usado.</returns>
    public bool UsarTokenRestablecimiento(string token)
    {
        var tokenRestablecimiento = _tokensRestablecimiento
            .FirstOrDefault(t => t.Token == token && t.EsValido);

        if (tokenRestablecimiento is null)
        {
            return false;
        }

        tokenRestablecimiento.Usar();
        return true;
    }

    // ─── 2FA ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Activa el 2FA para el usuario con la clave secreta TOTP indicada.
    /// </summary>
    /// <param name="secretKey">Clave secreta TOTP (se almacena encriptada en BD).</param>
    /// <param name="codigosRecuperacionHash">Hashes de los 8 códigos de recuperación.</param>
    public Result ActivarDosFactores(string secretKey, IEnumerable<string> codigosRecuperacionHash)
    {
        if (TwoFactorActivo)
        {
            return Result.Failure("El 2FA ya está activo para este usuario.");
        }

        TwoFactorActivo = true;
        TwoFactorSecretKey = secretKey;
        ModificadoEn = DateTime.UtcNow;

        foreach (var hash in codigosRecuperacionHash)
        {
            _codigosRecuperacion.Add(CodigoRecuperacion2FA.Crear(Id, hash));
        }

        RaiseDomainEvent(new DosFactoresActivadoEvent(Id, Email.Valor));
        return Result.Success();
    }

    /// <summary>
    /// Guarda la clave secreta TOTP temporalmente antes de confirmar la activación del 2FA.
    /// La activación definitiva ocurre en <see cref="ActivarDosFactores"/>.
    /// </summary>
    public void GuardarSecretKeyTemporal(string secretKey)
    {
        TwoFactorSecretKey = secretKey;
        ModificadoEn = DateTime.UtcNow;
    }

    /// <summary>Desactiva el 2FA y elimina los códigos de recuperación.</summary>
    public Result DesactivarDosFactores()
    {
        if (!TwoFactorActivo)
        {
            return Result.Failure("El 2FA no está activo para este usuario.");
        }

        TwoFactorActivo = false;
        TwoFactorSecretKey = null;
        ModificadoEn = DateTime.UtcNow;

        RaiseDomainEvent(new DosFactoresDesactivadoEvent(Id, Email.Valor));
        return Result.Success();
    }

    /// <summary>
    /// Usa un código de recuperación de 2FA marcándolo como usado.
    /// </summary>
    /// <returns>True si el código es válido y fue marcado como usado.</returns>
    public bool UsarCodigoRecuperacion(string codigoHash)
    {
        var codigo = _codigosRecuperacion
            .FirstOrDefault(c => c.CodigoHash == codigoHash && !c.EsUsado);

        if (codigo is null)
        {
            return false;
        }

        codigo.Usar();
        return true;
    }

    // ─── Roles ────────────────────────────────────────────────────────────────

    /// <summary>Asigna un rol al usuario si no lo tiene ya.</summary>
    public Result AsignarRol(Guid rolId)
    {
        if (_roles.Any(r => r.RolId == rolId))
        {
            return Result.Failure("El usuario ya tiene asignado este rol.");
        }

        _roles.Add(UsuarioRol.Crear(Id, rolId));
        ModificadoEn = DateTime.UtcNow;
        return Result.Success();
    }

    /// <summary>Quita un rol del usuario.</summary>
    public Result QuitarRol(Guid rolId)
    {
        var rol = _roles.FirstOrDefault(r => r.RolId == rolId);
        if (rol is null)
        {
            return Result.Failure("El usuario no tiene asignado este rol.");
        }

        if (_roles.Count == 1)
        {
            return Result.Failure("El usuario debe tener al menos un rol.");
        }

        _roles.Remove(rol);
        ModificadoEn = DateTime.UtcNow;
        return Result.Success();
    }

    // ─── Sucursales ──────────────────────────────────────────────────────────

    /// <summary>Asigna una sucursal al usuario. La primera asignada se marca como principal.</summary>
    public Result AsignarSucursal(Guid sucursalId)
    {
        if (_sucursales.Any(s => s.SucursalId == sucursalId))
            return Result.Failure("El usuario ya tiene asignada esta sucursal.");

        var esPrincipal = _sucursales.Count == 0;
        _sucursales.Add(UsuarioSucursal.Crear(Id, sucursalId, esPrincipal));
        ModificadoEn = DateTime.UtcNow;
        return Result.Success();
    }

    /// <summary>Remueve una sucursal del usuario.</summary>
    public Result RemoverSucursal(Guid sucursalId)
    {
        var asignacion = _sucursales.FirstOrDefault(s => s.SucursalId == sucursalId);
        if (asignacion is null)
            return Result.Failure("El usuario no tiene asignada esta sucursal.");

        if (_sucursales.Count == 1)
            return Result.Failure("El usuario debe tener al menos una sucursal asignada.");

        _sucursales.Remove(asignacion);

        // Si era la principal, asignar la siguiente como principal
        if (asignacion.EsPrincipal && _sucursales.Count > 0)
            _sucursales[0].MarcarComoPrincipal();

        ModificadoEn = DateTime.UtcNow;
        return Result.Success();
    }

    /// <summary>Cambia la sucursal principal del usuario.</summary>
    public Result CambiarSucursalPrincipal(Guid sucursalId)
    {
        var nueva = _sucursales.FirstOrDefault(s => s.SucursalId == sucursalId);
        if (nueva is null)
            return Result.Failure("El usuario no tiene asignada esta sucursal.");

        foreach (var s in _sucursales)
            s.QuitarPrincipal();

        nueva.MarcarComoPrincipal();
        ModificadoEn = DateTime.UtcNow;
        return Result.Success();
    }

    // ─── Validaciones de estado ───────────────────────────────────────────────

    /// <summary>Indica si el usuario puede autenticarse (activo y no bloqueado).</summary>
    public bool PuedeAutenticarse()
    {
        if (Estado == EstadoUsuario.Bloqueado && BloqueadoHasta.HasValue)
        {
            // Desbloqueo automático si ya pasó el tiempo
            if (DateTime.UtcNow >= BloqueadoHasta.Value)
            {
                Estado = EstadoUsuario.Activo;
                BloqueadoHasta = null;
                IntentosFallidos = 0;
            }
        }

        return Estado == EstadoUsuario.Activo;
    }
}
