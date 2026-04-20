using CoreTemplate.Modules.Auth.Application.Abstractions;
using CoreTemplate.Modules.Auth.Application.Constants;
using CoreTemplate.Modules.Auth.Domain.Aggregates;
using CoreTemplate.Modules.Auth.Domain.Enums;
using CoreTemplate.Modules.Auth.Domain.Repositories;
using CoreTemplate.Modules.Auth.Domain.ValueObjects;
using CoreTemplate.SharedKernel;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Options;

namespace CoreTemplate.Modules.Auth.Application.Commands.Registro;

public sealed record RegistrarUsuarioCommand(
    string Email,
    string Nombre,
    string Password,
    string ConfirmPassword,
    Guid? TenantId = null,
    TipoUsuario TipoUsuario = TipoUsuario.Humano) : IRequest<Result<Guid>>;

internal sealed class RegistrarUsuarioCommandValidator : AbstractValidator<RegistrarUsuarioCommand>
{
    public RegistrarUsuarioCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("El email no es válido.")
            .MaximumLength(200);
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(100)
            .WithMessage("El nombre es requerido y no puede superar 100 caracteres.");
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8)
            .WithMessage("La contraseña debe tener al menos 8 caracteres.");
        RuleFor(x => x.ConfirmPassword).Equal(x => x.Password)
            .WithMessage(AuthErrorMessages.PasswordsNoCoinciden);
    }
}

internal sealed class RegistrarUsuarioCommandHandler(
    IUsuarioRepository _usuarioRepo,
    IRolRepository _rolRepo,
    IRegistroAuditoriaRepository _auditoriaRepo,
    IPasswordService _passwordService,
    IOptions<PasswordPolicySettings> _policy) : IRequestHandler<RegistrarUsuarioCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(RegistrarUsuarioCommand cmd, CancellationToken ct)
    {
        // Validar política de contraseña
        var erroresPolicy = _passwordService.ValidarPolitica(cmd.Password);
        if (erroresPolicy.Count > 0)
        {
            return Result<Guid>.Failure([.. erroresPolicy]);
        }

        // Verificar email único
        if (await _usuarioRepo.ExistsByEmailAsync(cmd.Email, cmd.TenantId, ct))
        {
            return Result<Guid>.Failure(AuthErrorMessages.EmailYaRegistrado);
        }

        var emailResult = Email.Crear(cmd.Email);
        if (!emailResult.IsSuccess)
        {
            return Result<Guid>.Failure(emailResult.Error!);
        }

        var hash = _passwordService.HashPassword(cmd.Password);
        var passwordHashResult = PasswordHash.Crear(hash);
        if (!passwordHashResult.IsSuccess)
        {
            return Result<Guid>.Failure(passwordHashResult.Error!);
        }

        var usuarioResult = Usuario.Crear(emailResult.Value!, cmd.Nombre, passwordHashResult.Value!, cmd.TenantId, cmd.TipoUsuario);
        if (!usuarioResult.IsSuccess)
        {
            return Result<Guid>.Failure(usuarioResult.Error!);
        }

        var usuario = usuarioResult.Value!;

        // Asignar rol User por defecto
        var roles = await _rolRepo.GetAllAsync(cmd.TenantId, ct);
        var rolUser = roles.FirstOrDefault(r => r.Nombre == "User");
        if (rolUser is not null)
        {
            usuario.AsignarRol(rolUser.Id);
        }

        await _usuarioRepo.AddAsync(usuario, ct);

        await _auditoriaRepo.AddAsync(Domain.Entities.RegistroAuditoria.Crear(
            usuario.TenantId, usuario.Id, usuario.Email.Valor,
            EventoAuditoria.UsuarioRegistrado, string.Empty, string.Empty), ct);

        return Result<Guid>.Success(usuario.Id, AuthSuccessMessages.UsuarioRegistrado);
    }
}
