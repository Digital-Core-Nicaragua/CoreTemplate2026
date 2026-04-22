using CoreTemplate.Modules.Auth.Domain.Enums;
using CoreTemplate.Modules.Auth.Domain.Events;
using CoreTemplate.Modules.Auth.Domain.ValueObjects;
using CoreTemplate.SharedKernel;
using CoreTemplate.SharedKernel.Domain;

namespace CoreTemplate.Modules.Auth.Domain.Aggregates;

/// <summary>
/// Aggregate Root que representa a un cliente externo con acceso al portal.
/// Solo se activa cuando <c>CustomerPortalSettings:EnableCustomerPortal = true</c>.
/// Email se almacena como string normalizado (lowercase) — la validacion de formato
/// se hace en el handler antes de llamar al factory method.
/// </summary>
public sealed class UsuarioCliente : AggregateRoot<Guid>, IAuditable
{
    public Guid? TenantId { get; private set; }

    /// <summary>Email normalizado (lowercase). Nullable cuando el registro es por telefono.</summary>
    public string? Email { get; private set; }

    /// <summary>Como se registro el cliente: Email, Telefono u OAuth.</summary>
    public TipoRegistro TipoRegistro { get; private set; }

    /// <summary>Hash BCrypt. Nullable para clientes OAuth o registrados por telefono.</summary>
    public string? PasswordHash { get; private set; }

    public string Nombre { get; private set; } = string.Empty;
    public string Apellido { get; private set; } = string.Empty;
    public string? Telefono { get; private set; }
    public EstadoUsuarioCliente Estado { get; private set; }
    public bool EmailVerificado { get; private set; }
    public bool TelefonoVerificado { get; private set; }
    public string? TokenVerificacionEmail { get; private set; }
    public string? CodigoVerificacionTelefono { get; private set; }
    public DateTime? TokenExpiraEn { get; private set; }
    public int IntentosFallidos { get; private set; }
    public DateTime? BloqueadoHasta { get; private set; }
    public DateTime CreadoEn { get; private set; }
    public DateTime? ModificadoEn { get; private set; }
    public string? TokenRestablecimiento { get; private set; }
    public DateTime? TokenRestablecimientoExpiraEn { get; private set; }

    private readonly List<ProveedorOAuthVinculado> _proveedores = [];
    public IReadOnlyList<ProveedorOAuthVinculado> Proveedores => _proveedores;

    private UsuarioCliente() { }

    // --- Factory methods ---

    /// <summary>Crea un cliente con email y contrasena. Estado inicial: Registered.</summary>
    public static Result<UsuarioCliente> Crear(
        string email,
        string passwordHash,
        string nombre,
        string apellido,
        Guid? tenantId = null,
        string? telefono = null)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Result<UsuarioCliente>.Failure("El email es requerido.");

        var validacion = ValidarNombreApellido(nombre, apellido);
        if (!validacion.IsSuccess) return Result<UsuarioCliente>.Failure(validacion.Error!);

        var emailNorm = email.Trim().ToLowerInvariant();
        var cliente = new UsuarioCliente
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Email = emailNorm,
            TipoRegistro = TipoRegistro.Email,
            PasswordHash = passwordHash,
            Nombre = nombre.Trim(),
            Apellido = apellido.Trim(),
            Telefono = telefono?.Trim(),
            Estado = EstadoUsuarioCliente.Registered,
            CreadoEn = DateTime.UtcNow
        };

        cliente._proveedores.Add(new ProveedorOAuthVinculado(
            TipoProveedorOAuth.Local, string.Empty, emailNorm, DateTime.UtcNow));

        cliente.RaiseDomainEvent(new ClienteRegistradoEvent(cliente.Id, emailNorm, nombre, tenantId));

        return Result<UsuarioCliente>.Success(cliente);
    }

    /// <summary>Crea un cliente identificado por telefono (sin email ni contrasena). Estado inicial: Registered.</summary>
    public static Result<UsuarioCliente> CrearPorTelefono(
        string telefono,
        string nombre,
        string apellido,
        Guid? tenantId = null)
    {
        if (string.IsNullOrWhiteSpace(telefono))
            return Result<UsuarioCliente>.Failure("El telefono es requerido.");

        var validacion = ValidarNombreApellido(nombre, apellido);
        if (!validacion.IsSuccess) return Result<UsuarioCliente>.Failure(validacion.Error!);

        var cliente = new UsuarioCliente
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Email = null,
            TipoRegistro = TipoRegistro.Telefono,
            PasswordHash = null,
            Nombre = nombre.Trim(),
            Apellido = apellido.Trim(),
            Telefono = telefono.Trim(),
            Estado = EstadoUsuarioCliente.Registered,
            CreadoEn = DateTime.UtcNow
        };

        return Result<UsuarioCliente>.Success(cliente);
    }

    /// <summary>Crea un cliente desde OAuth. Email ya verificado por el proveedor -> estado Active.</summary>
    public static Result<UsuarioCliente> CrearDesdeOAuth(
        string email,
        string nombre,
        string apellido,
        TipoProveedorOAuth proveedor,
        string externalId,
        Guid? tenantId = null)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Result<UsuarioCliente>.Failure("El email es requerido.");

        var validacion = ValidarNombreApellido(nombre, apellido);
        if (!validacion.IsSuccess) return Result<UsuarioCliente>.Failure(validacion.Error!);

        var emailNorm = email.Trim().ToLowerInvariant();
        var cliente = new UsuarioCliente
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Email = emailNorm,
            TipoRegistro = TipoRegistro.OAuth,
            PasswordHash = null,
            Nombre = nombre.Trim(),
            Apellido = apellido.Trim(),
            Estado = EstadoUsuarioCliente.Active,
            EmailVerificado = true,
            CreadoEn = DateTime.UtcNow
        };

        cliente._proveedores.Add(new ProveedorOAuthVinculado(proveedor, externalId, emailNorm, DateTime.UtcNow));
        cliente.RaiseDomainEvent(new ClienteRegistradoEvent(cliente.Id, emailNorm, nombre, tenantId));
        cliente.RaiseDomainEvent(new ClienteActivadoEvent(cliente.Id, emailNorm, tenantId));

        return Result<UsuarioCliente>.Success(cliente);
    }

    // --- Verificacion de email ---

    public void GenerarTokenVerificacionEmail()
    {
        TokenVerificacionEmail = Guid.NewGuid().ToString("N");
        TokenExpiraEn = DateTime.UtcNow.AddHours(24);
        ModificadoEn = DateTime.UtcNow;
    }

    public Result VerificarEmail(string token, bool requiereVerificacionTelefono)
    {
        if (EmailVerificado) return Result.Failure("El email ya esta verificado.");
        if (string.IsNullOrEmpty(TokenVerificacionEmail)) return Result.Failure("No hay token de verificacion generado.");
        if (TokenVerificacionEmail != token) return Result.Failure("El token de verificacion es invalido.");
        if (TokenExpiraEn < DateTime.UtcNow) return Result.Failure("El token de verificacion ha expirado. Solicita uno nuevo.");

        EmailVerificado = true;
        TokenVerificacionEmail = null;
        TokenExpiraEn = null;
        ModificadoEn = DateTime.UtcNow;

        RaiseDomainEvent(new ClienteEmailVerificadoEvent(Id, Email ?? string.Empty, TenantId));

        if (!requiereVerificacionTelefono)
        {
            Estado = EstadoUsuarioCliente.Active;
            RaiseDomainEvent(new ClienteActivadoEvent(Id, Email ?? string.Empty, TenantId));
        }
        else
        {
            Estado = EstadoUsuarioCliente.Verified;
        }

        return Result.Success();
    }

    // --- Verificacion de telefono / OTP ---

    public Result GenerarOtpTelefono(int expirationMinutes = 10)
    {
        if (string.IsNullOrEmpty(Telefono))
            return Result.Failure("El cliente no tiene telefono registrado.");

        if (TipoRegistro == TipoRegistro.Email && !EmailVerificado)
            return Result.Failure("Debe verificar el email antes de verificar el telefono.");

        CodigoVerificacionTelefono = Random.Shared.Next(100000, 999999).ToString();
        TokenExpiraEn = DateTime.UtcNow.AddMinutes(expirationMinutes);
        ModificadoEn = DateTime.UtcNow;

        return Result.Success();
    }

    public Result GenerarCodigoVerificacionTelefono() => GenerarOtpTelefono();

    public Result VerificarOtpTelefono(string codigo)
    {
        if (TelefonoVerificado) return Result.Failure("El telefono ya esta verificado.");
        if (string.IsNullOrEmpty(CodigoVerificacionTelefono)) return Result.Failure("No hay codigo de verificacion generado.");
        if (CodigoVerificacionTelefono != codigo) return Result.Failure("El codigo de verificacion es invalido.");
        if (TokenExpiraEn < DateTime.UtcNow) return Result.Failure("El codigo ha expirado. Solicita uno nuevo.");

        TelefonoVerificado = true;
        CodigoVerificacionTelefono = null;
        TokenExpiraEn = null;
        Estado = EstadoUsuarioCliente.Active;
        ModificadoEn = DateTime.UtcNow;

        RaiseDomainEvent(new ClienteTelefonoVerificadoEvent(Id, Email ?? string.Empty, TenantId));
        RaiseDomainEvent(new ClienteActivadoEvent(Id, Email ?? string.Empty, TenantId));

        return Result.Success();
    }

    public Result VerificarTelefono(string codigo) => VerificarOtpTelefono(codigo);

    // --- OAuth ---

    public Result VincularProveedor(TipoProveedorOAuth proveedor, string externalId, string emailProveedor)
    {
        if (_proveedores.Any(p => p.Proveedor == proveedor))
            return Result.Failure($"El proveedor {proveedor} ya esta vinculado a esta cuenta.");

        _proveedores.Add(new ProveedorOAuthVinculado(proveedor, externalId, emailProveedor, DateTime.UtcNow));
        ModificadoEn = DateTime.UtcNow;

        RaiseDomainEvent(new ClienteProveedorVinculadoEvent(Id, Email ?? string.Empty, proveedor, TenantId));
        return Result.Success();
    }

    public Result DesvincularProveedor(TipoProveedorOAuth proveedor)
    {
        var vinculado = _proveedores.FirstOrDefault(p => p.Proveedor == proveedor);
        if (vinculado is null) return Result.Failure($"El proveedor {proveedor} no esta vinculado a esta cuenta.");
        if (_proveedores.Count == 1 && string.IsNullOrEmpty(PasswordHash))
            return Result.Failure("No puedes desvincular el unico metodo de acceso. Establece una contrasena primero.");

        _proveedores.Remove(vinculado);
        ModificadoEn = DateTime.UtcNow;
        return Result.Success();
    }

    // --- Contrasena ---

    public Result CambiarPassword(string nuevoHash)
    {
        if (string.IsNullOrWhiteSpace(nuevoHash))
            return Result.Failure("El hash de contrasena no puede estar vacio.");

        PasswordHash = nuevoHash;
        ModificadoEn = DateTime.UtcNow;

        if (!_proveedores.Any(p => p.Proveedor == TipoProveedorOAuth.Local))
            _proveedores.Add(new ProveedorOAuthVinculado(
                TipoProveedorOAuth.Local, string.Empty, Email ?? string.Empty, DateTime.UtcNow));

        return Result.Success();
    }

    // --- Intentos fallidos ---

    public void IncrementarIntentosFallidos(int maxIntentos, int minutosBloqueado)
    {
        IntentosFallidos++;
        ModificadoEn = DateTime.UtcNow;

        if (IntentosFallidos >= maxIntentos)
        {
            BloqueadoHasta = DateTime.UtcNow.AddMinutes(minutosBloqueado);
            Estado = EstadoUsuarioCliente.Blocked;
            RaiseDomainEvent(new ClienteBloqueadoEvent(Id, Email ?? string.Empty, TenantId));
        }
    }

    public void ResetearIntentosFallidos()
    {
        IntentosFallidos = 0;
        BloqueadoHasta = null;
        ModificadoEn = DateTime.UtcNow;
    }

    // --- Gestion por admin ---

    public Result Bloquear()
    {
        if (Estado == EstadoUsuarioCliente.Blocked)
            return Result.Failure("El cliente ya esta bloqueado.");

        Estado = EstadoUsuarioCliente.Blocked;
        ModificadoEn = DateTime.UtcNow;
        RaiseDomainEvent(new ClienteBloqueadoEvent(Id, Email ?? string.Empty, TenantId));
        return Result.Success();
    }

    public Result Reactivar()
    {
        if (Estado != EstadoUsuarioCliente.Blocked)
            return Result.Failure("El cliente no esta bloqueado.");

        Estado = (EmailVerificado && TelefonoVerificado) || (EmailVerificado && string.IsNullOrEmpty(Telefono))
            ? EstadoUsuarioCliente.Active
            : EstadoUsuarioCliente.Verified;

        BloqueadoHasta = null;
        IntentosFallidos = 0;
        ModificadoEn = DateTime.UtcNow;
        RaiseDomainEvent(new ClienteReactivadoEvent(Id, Email ?? string.Empty, TenantId));
        return Result.Success();
    }

    // --- Validaciones de estado ---

    public bool PuedeAutenticarse()
    {
        if (Estado == EstadoUsuarioCliente.Blocked && BloqueadoHasta.HasValue
            && DateTime.UtcNow >= BloqueadoHasta.Value)
        {
            Estado = EstadoUsuarioCliente.Active;
            BloqueadoHasta = null;
            IntentosFallidos = 0;
        }

        return Estado is EstadoUsuarioCliente.Verified or EstadoUsuarioCliente.Active;
    }

    // --- Restablecimiento de contrasena ---

    public void GenerarTokenRestablecimiento(int horasExpiracion = 1)
    {
        TokenRestablecimiento = Guid.NewGuid().ToString("N");
        TokenRestablecimientoExpiraEn = DateTime.UtcNow.AddHours(horasExpiracion);
        ModificadoEn = DateTime.UtcNow;
    }

    public Result RestablecerPassword(string token, string nuevoHash)
    {
        if (string.IsNullOrEmpty(TokenRestablecimiento))
            return Result.Failure("No hay solicitud de restablecimiento pendiente.");
        if (TokenRestablecimiento != token)
            return Result.Failure("El token de restablecimiento es invalido.");
        if (TokenRestablecimientoExpiraEn < DateTime.UtcNow)
            return Result.Failure("El token de restablecimiento ha expirado. Solicita uno nuevo.");

        PasswordHash = nuevoHash;
        TokenRestablecimiento = null;
        TokenRestablecimientoExpiraEn = null;
        ModificadoEn = DateTime.UtcNow;

        if (!_proveedores.Any(p => p.Proveedor == TipoProveedorOAuth.Local))
            _proveedores.Add(new ProveedorOAuthVinculado(
                TipoProveedorOAuth.Local, string.Empty, Email ?? string.Empty, DateTime.UtcNow));

        return Result.Success();
    }

    // --- Helpers privados ---

    private static Result ValidarNombreApellido(string nombre, string apellido)
    {
        if (string.IsNullOrWhiteSpace(nombre)) return Result.Failure("El nombre del cliente es requerido.");
        if (nombre.Trim().Length > 100) return Result.Failure("El nombre no puede superar los 100 caracteres.");
        if (string.IsNullOrWhiteSpace(apellido)) return Result.Failure("El apellido del cliente es requerido.");
        if (apellido.Trim().Length > 100) return Result.Failure("El apellido no puede superar los 100 caracteres.");
        return Result.Success();
    }
}

/// <summary>Value Object que representa un proveedor OAuth vinculado a un cliente.</summary>
public sealed record ProveedorOAuthVinculado(
    TipoProveedorOAuth Proveedor,
    string ExternalId,
    string Email,
    DateTime VinculadoEn);
