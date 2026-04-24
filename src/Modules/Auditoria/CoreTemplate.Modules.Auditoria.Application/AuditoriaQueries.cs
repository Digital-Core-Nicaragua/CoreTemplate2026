using CoreTemplate.Auditing.Models;
using CoreTemplate.Auditing.Persistence;
using CoreTemplate.SharedKernel;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoreTemplate.Modules.Auditoria.Application;

// ─── DTOs ─────────────────────────────────────────────────────────────────────

public record AuditLogDto(
    Guid Id,
    Guid? TenantId,
    Guid? UsuarioId,
    string NombreEntidad,
    string EntidadId,
    string Accion,
    string? ValoresAnteriores,
    string? ValoresNuevos,
    DateTime OcurridoEn,
    string? DireccionIp,
    string? CorrelationId);

// ─── Filtros ──────────────────────────────────────────────────────────────────

public record AuditLogFiltros(
    string? Entidad = null,
    string? EntidadId = null,
    Guid? UsuarioId = null,
    string? Accion = null,
    DateTime? FechaDesde = null,
    DateTime? FechaHasta = null,
    int Pagina = 1,
    int Tamano = 20);

// ─── Queries ──────────────────────────────────────────────────────────────────

public record GetAuditLogsQuery(AuditLogFiltros Filtros)
    : IRequest<Result<PagedResult<AuditLogDto>>>;

public record GetAuditLogByIdQuery(Guid Id)
    : IRequest<Result<AuditLogDto>>;

public record GetAuditLogsPorEntidadQuery(string EntidadId, int Pagina = 1, int Tamano = 20)
    : IRequest<Result<PagedResult<AuditLogDto>>>;

public record GetAuditLogsPorUsuarioQuery(
    Guid UsuarioId,
    DateTime? FechaDesde,
    DateTime? FechaHasta,
    int Pagina = 1,
    int Tamano = 20) : IRequest<Result<PagedResult<AuditLogDto>>>;

// ─── Handlers ─────────────────────────────────────────────────────────────────

internal sealed class GetAuditLogsHandler(AuditDbContext db)
    : IRequestHandler<GetAuditLogsQuery, Result<PagedResult<AuditLogDto>>>
{
    public async Task<Result<PagedResult<AuditLogDto>>> Handle(
        GetAuditLogsQuery q, CancellationToken ct)
    {
        var f = q.Filtros;
        var query = db.AuditLogs.AsQueryable();

        if (!string.IsNullOrWhiteSpace(f.Entidad))
            query = query.Where(l => l.NombreEntidad == f.Entidad);
        if (!string.IsNullOrWhiteSpace(f.EntidadId))
            query = query.Where(l => l.EntidadId == f.EntidadId);
        if (f.UsuarioId.HasValue)
            query = query.Where(l => l.UsuarioId == f.UsuarioId);
        if (!string.IsNullOrWhiteSpace(f.Accion))
            query = query.Where(l => l.Accion.ToString() == f.Accion);
        if (f.FechaDesde.HasValue)
            query = query.Where(l => l.OcurridoEn >= f.FechaDesde);
        if (f.FechaHasta.HasValue)
            query = query.Where(l => l.OcurridoEn <= f.FechaHasta);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(l => l.OcurridoEn)
            .Skip((f.Pagina - 1) * f.Tamano)
            .Take(f.Tamano)
            .ToListAsync(ct);

        return Result<PagedResult<AuditLogDto>>.Success(
            new PagedResult<AuditLogDto>(
                items.Select(AuditLogMapper.ToDto).ToList(), f.Pagina, f.Tamano, total));
    }
}

internal sealed class GetAuditLogByIdHandler(AuditDbContext db)
    : IRequestHandler<GetAuditLogByIdQuery, Result<AuditLogDto>>
{
    public async Task<Result<AuditLogDto>> Handle(GetAuditLogByIdQuery q, CancellationToken ct)
    {
        var log = await db.AuditLogs.FindAsync([q.Id], ct);
        return log is null
            ? Result<AuditLogDto>.Failure("Registro de auditoría no encontrado.")
            : Result<AuditLogDto>.Success(AuditLogMapper.ToDto(log));
    }
}

internal sealed class GetAuditLogsPorEntidadHandler(AuditDbContext db)
    : IRequestHandler<GetAuditLogsPorEntidadQuery, Result<PagedResult<AuditLogDto>>>
{
    public async Task<Result<PagedResult<AuditLogDto>>> Handle(
        GetAuditLogsPorEntidadQuery q, CancellationToken ct)
    {
        var total = await db.AuditLogs.CountAsync(l => l.EntidadId == q.EntidadId, ct);
        var items = await db.AuditLogs
            .Where(l => l.EntidadId == q.EntidadId)
            .OrderByDescending(l => l.OcurridoEn)
            .Skip((q.Pagina - 1) * q.Tamano)
            .Take(q.Tamano)
            .ToListAsync(ct);

        return Result<PagedResult<AuditLogDto>>.Success(
            new PagedResult<AuditLogDto>(
                items.Select(AuditLogMapper.ToDto).ToList(), q.Pagina, q.Tamano, total));
    }
}

internal sealed class GetAuditLogsPorUsuarioHandler(AuditDbContext db)
    : IRequestHandler<GetAuditLogsPorUsuarioQuery, Result<PagedResult<AuditLogDto>>>
{
    public async Task<Result<PagedResult<AuditLogDto>>> Handle(
        GetAuditLogsPorUsuarioQuery q, CancellationToken ct)
    {
        var query = db.AuditLogs.Where(l => l.UsuarioId == q.UsuarioId);

        if (q.FechaDesde.HasValue) query = query.Where(l => l.OcurridoEn >= q.FechaDesde);
        if (q.FechaHasta.HasValue) query = query.Where(l => l.OcurridoEn <= q.FechaHasta);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(l => l.OcurridoEn)
            .Skip((q.Pagina - 1) * q.Tamano)
            .Take(q.Tamano)
            .ToListAsync(ct);

        return Result<PagedResult<AuditLogDto>>.Success(
            new PagedResult<AuditLogDto>(
                items.Select(AuditLogMapper.ToDto).ToList(), q.Pagina, q.Tamano, total));
    }
}

// ─── Mapeo ────────────────────────────────────────────────────────────────────

internal static class AuditLogMapper
{
    public static AuditLogDto ToDto(AuditLog l) => new(
        l.Id, l.TenantId, l.UsuarioId,
        l.NombreEntidad, l.EntidadId, l.Accion.ToString(),
        l.ValoresAnteriores, l.ValoresNuevos,
        l.OcurridoEn, l.DireccionIp, l.CorrelationId);
}
