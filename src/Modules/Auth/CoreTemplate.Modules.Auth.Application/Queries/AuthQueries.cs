using CoreTemplate.Modules.Auth.Application.Constants;
using CoreTemplate.Modules.Auth.Application.DTOs;
using CoreTemplate.Modules.Auth.Domain.Enums;
using CoreTemplate.Modules.Auth.Domain.Repositories;
using CoreTemplate.Infrastructure.Services;
using CoreTemplate.SharedKernel;
using MediatR;

namespace CoreTemplate.Modules.Auth.Application.Queries;

// ─── GetUsuarioById ───────────────────────────────────────────────────────────

public sealed record GetUsuarioByIdQuery(Guid UsuarioId) : IRequest<Result<UsuarioDto>>;

internal sealed class GetUsuarioByIdQueryHandler(
    IUsuarioRepository _usuarioRepo,
    IRolRepository _rolRepo) : IRequestHandler<GetUsuarioByIdQuery, Result<UsuarioDto>>
{
    public async Task<Result<UsuarioDto>> Handle(GetUsuarioByIdQuery query, CancellationToken ct)
    {
        var usuario = await _usuarioRepo.GetByIdAsync(query.UsuarioId, ct);
        if (usuario is null)
        {
            return Result<UsuarioDto>.Failure(AuthErrorMessages.UsuarioNoEncontrado);
        }

        var roles = await _rolRepo.GetAllAsync(usuario.TenantId, ct);
        var nombresRoles = usuario.Roles
            .Select(ur => roles.FirstOrDefault(r => r.Id == ur.RolId)?.Nombre ?? ur.RolId.ToString())
            .ToList();

        return Result<UsuarioDto>.Success(new UsuarioDto(
            usuario.Id, usuario.TenantId, usuario.Email.Valor, usuario.Nombre,
            usuario.Estado, usuario.TwoFactorActivo, usuario.UltimoAcceso,
            usuario.CreadoEn, nombresRoles));
    }
}

// ─── GetUsuarios (paginado) ───────────────────────────────────────────────────

public sealed record GetUsuariosQuery(
    int Pagina = 1,
    int TamanoPagina = 20,
    EstadoUsuario? Estado = null) : IRequest<Result<PagedResult<UsuarioResumenDto>>>;

internal sealed class GetUsuariosQueryHandler(
    IUsuarioRepository _usuarioRepo,
    IRolRepository _rolRepo,
    ICurrentTenant _currentTenant) : IRequestHandler<GetUsuariosQuery, Result<PagedResult<UsuarioResumenDto>>>
{
    public async Task<Result<PagedResult<UsuarioResumenDto>>> Handle(GetUsuariosQuery query, CancellationToken ct)
    {
        var paged = await _usuarioRepo.GetPagedAsync(query.Pagina, query.TamanoPagina, query.Estado, ct);
        var roles = await _rolRepo.GetAllAsync(_currentTenant.TenantId, ct);

        var items = paged.Items.Select(u =>
        {
            var nombresRoles = u.Roles
                .Select(ur => roles.FirstOrDefault(r => r.Id == ur.RolId)?.Nombre ?? ur.RolId.ToString())
                .ToList();

            return new UsuarioResumenDto(u.Id, u.Email.Valor, u.Nombre, u.Estado, u.UltimoAcceso, nombresRoles);
        }).ToList();

        return Result<PagedResult<UsuarioResumenDto>>.Success(
            new PagedResult<UsuarioResumenDto>(items, paged.Pagina, paged.TamanoPagina, paged.Total));
    }
}

// ─── GetMiPerfil ──────────────────────────────────────────────────────────────

public sealed record GetMiPerfilQuery : IRequest<Result<UsuarioDto>>;

internal sealed class GetMiPerfilQueryHandler(
    IUsuarioRepository _usuarioRepo,
    IRolRepository _rolRepo,
    ICurrentUser _currentUser) : IRequestHandler<GetMiPerfilQuery, Result<UsuarioDto>>
{
    public async Task<Result<UsuarioDto>> Handle(GetMiPerfilQuery query, CancellationToken ct)
    {
        if (!_currentUser.Id.HasValue)
        {
            return Result<UsuarioDto>.Failure(AuthErrorMessages.UsuarioNoEncontrado);
        }

        var usuario = await _usuarioRepo.GetByIdAsync(_currentUser.Id.Value, ct);
        if (usuario is null)
        {
            return Result<UsuarioDto>.Failure(AuthErrorMessages.UsuarioNoEncontrado);
        }

        var roles = await _rolRepo.GetAllAsync(usuario.TenantId, ct);
        var nombresRoles = usuario.Roles
            .Select(ur => roles.FirstOrDefault(r => r.Id == ur.RolId)?.Nombre ?? ur.RolId.ToString())
            .ToList();

        return Result<UsuarioDto>.Success(new UsuarioDto(
            usuario.Id, usuario.TenantId, usuario.Email.Valor, usuario.Nombre,
            usuario.Estado, usuario.TwoFactorActivo, usuario.UltimoAcceso,
            usuario.CreadoEn, nombresRoles));
    }
}

// ─── GetRoles ─────────────────────────────────────────────────────────────────

public sealed record GetRolesQuery : IRequest<Result<List<RolResumenDto>>>;

internal sealed class GetRolesQueryHandler(
    IRolRepository _rolRepo,
    ICurrentTenant _currentTenant) : IRequestHandler<GetRolesQuery, Result<List<RolResumenDto>>>
{
    public async Task<Result<List<RolResumenDto>>> Handle(GetRolesQuery query, CancellationToken ct)
    {
        var roles = await _rolRepo.GetAllAsync(_currentTenant.TenantId, ct);
        var dtos = roles.Select(r => new RolResumenDto(
            r.Id, r.Nombre, r.Descripcion, r.EsSistema, r.Permisos.Count)).ToList();

        return Result<List<RolResumenDto>>.Success(dtos);
    }
}

// ─── GetRolById ───────────────────────────────────────────────────────────────

public sealed record GetRolByIdQuery(Guid RolId) : IRequest<Result<RolDto>>;

internal sealed class GetRolByIdQueryHandler(
    IRolRepository _rolRepo,
    IPermisoRepository _permisoRepo) : IRequestHandler<GetRolByIdQuery, Result<RolDto>>
{
    public async Task<Result<RolDto>> Handle(GetRolByIdQuery query, CancellationToken ct)
    {
        var rol = await _rolRepo.GetByIdAsync(query.RolId, ct);
        if (rol is null)
        {
            return Result<RolDto>.Failure(AuthErrorMessages.RolNoEncontrado);
        }

        var todosPermisos = await _permisoRepo.GetAllAsync(ct);
        var codigosPermisos = rol.Permisos
            .Select(rp => todosPermisos.FirstOrDefault(p => p.Id == rp.PermisoId)?.Codigo ?? rp.PermisoId.ToString())
            .ToList();

        return Result<RolDto>.Success(new RolDto(
            rol.Id, rol.Nombre, rol.Descripcion, rol.EsSistema, codigosPermisos));
    }
}
