using CoreTemplate.Modules.PdfTemplates.Application.DTOs;
using CoreTemplate.Modules.PdfTemplates.Domain.Aggregates;
using CoreTemplate.Modules.PdfTemplates.Domain.Repositories;
using CoreTemplate.SharedKernel;
using MediatR;

namespace CoreTemplate.Modules.PdfTemplates.Application.Commands;

// ─── Crear ────────────────────────────────────────────────────────────────────

public record CrearPdfPlantillaCommand(
    string Codigo, string Nombre, string Modulo, string CodigoTemplate,
    string NombreEmpresa, string? LogoUrl,
    string ColorEncabezado, string ColorTextoHeader, string ColorAcento,
    string? TextoSecundario, string? TextoPiePagina,
    bool MostrarNumeroPagina, bool MostrarFechaGeneracion,
    string? MarcaDeAgua) : IRequest<Result<PdfPlantillaDto>>;

internal sealed class CrearPdfPlantillaHandler(IPdfPlantillaRepository repo)
    : IRequestHandler<CrearPdfPlantillaCommand, Result<PdfPlantillaDto>>
{
    public async Task<Result<PdfPlantillaDto>> Handle(CrearPdfPlantillaCommand cmd, CancellationToken ct)
    {
        if (await repo.ExisteCodigoAsync(cmd.Codigo, null, ct))
            return Result<PdfPlantillaDto>.Failure("Ya existe una plantilla con ese código.");

        var result = PdfPlantilla.Crear(
            cmd.Codigo, cmd.Nombre, cmd.Modulo, cmd.CodigoTemplate,
            cmd.NombreEmpresa, cmd.LogoUrl,
            cmd.ColorEncabezado, cmd.ColorTextoHeader, cmd.ColorAcento,
            cmd.TextoSecundario, cmd.TextoPiePagina,
            cmd.MostrarNumeroPagina, cmd.MostrarFechaGeneracion, cmd.MarcaDeAgua);

        if (!result.IsSuccess) return Result<PdfPlantillaDto>.Failure(result.Error!);

        await repo.GuardarAsync(result.Value!, ct);
        return Result<PdfPlantillaDto>.Success(result.Value!.ToDto(), "Plantilla PDF creada correctamente.");
    }
}

// ─── Actualizar ───────────────────────────────────────────────────────────────

public record ActualizarPdfPlantillaCommand(
    Guid Id, string Nombre, string CodigoTemplate,
    string NombreEmpresa, string? LogoUrl,
    string ColorEncabezado, string ColorTextoHeader, string ColorAcento,
    string? TextoSecundario, string? TextoPiePagina,
    bool MostrarNumeroPagina, bool MostrarFechaGeneracion,
    string? MarcaDeAgua, Guid ModificadoPor) : IRequest<Result<PdfPlantillaDto>>;

internal sealed class ActualizarPdfPlantillaHandler(IPdfPlantillaRepository repo)
    : IRequestHandler<ActualizarPdfPlantillaCommand, Result<PdfPlantillaDto>>
{
    public async Task<Result<PdfPlantillaDto>> Handle(ActualizarPdfPlantillaCommand cmd, CancellationToken ct)
    {
        var plantilla = await repo.ObtenerPorIdAsync(cmd.Id, ct);
        if (plantilla is null) return Result<PdfPlantillaDto>.Failure("Plantilla no encontrada.");

        var result = plantilla.Actualizar(
            cmd.Nombre, cmd.CodigoTemplate, cmd.NombreEmpresa, cmd.LogoUrl,
            cmd.ColorEncabezado, cmd.ColorTextoHeader, cmd.ColorAcento,
            cmd.TextoSecundario, cmd.TextoPiePagina,
            cmd.MostrarNumeroPagina, cmd.MostrarFechaGeneracion, cmd.MarcaDeAgua, cmd.ModificadoPor);

        if (!result.IsSuccess) return Result<PdfPlantillaDto>.Failure(result.Error!);

        await repo.ActualizarAsync(plantilla, ct);
        return Result<PdfPlantillaDto>.Success(plantilla.ToDto(), "Plantilla PDF actualizada correctamente.");
    }
}

// ─── Activar / Desactivar ─────────────────────────────────────────────────────

public record ActivarPdfPlantillaCommand(Guid Id) : IRequest<Result>;
public record DesactivarPdfPlantillaCommand(Guid Id) : IRequest<Result>;

internal sealed class ActivarPdfPlantillaHandler(IPdfPlantillaRepository repo)
    : IRequestHandler<ActivarPdfPlantillaCommand, Result>
{
    public async Task<Result> Handle(ActivarPdfPlantillaCommand cmd, CancellationToken ct)
    {
        var p = await repo.ObtenerPorIdAsync(cmd.Id, ct);
        if (p is null) return Result.Failure("Plantilla no encontrada.");
        var r = p.Activar();
        if (!r.IsSuccess) return r;
        await repo.ActualizarAsync(p, ct);
        return Result.Success();
    }
}

internal sealed class DesactivarPdfPlantillaHandler(IPdfPlantillaRepository repo)
    : IRequestHandler<DesactivarPdfPlantillaCommand, Result>
{
    public async Task<Result> Handle(DesactivarPdfPlantillaCommand cmd, CancellationToken ct)
    {
        var p = await repo.ObtenerPorIdAsync(cmd.Id, ct);
        if (p is null) return Result.Failure("Plantilla no encontrada.");
        var r = p.Desactivar();
        if (!r.IsSuccess) return r;
        await repo.ActualizarAsync(p, ct);
        return Result.Success();
    }
}

// ─── Mapeo ────────────────────────────────────────────────────────────────────

internal static class PdfPlantillaExtensions
{
    public static PdfPlantillaDto ToDto(this PdfPlantilla p) => new(
        p.Id, p.Codigo, p.Nombre, p.Modulo, p.CodigoTemplate,
        p.NombreEmpresa, p.LogoUrl,
        p.ColorEncabezado, p.ColorTextoHeader, p.ColorAcento,
        p.TextoSecundario, p.TextoPiePagina,
        p.MostrarNumeroPagina, p.MostrarFechaGeneracion,
        p.MarcaDeAgua, p.EsDeSistema, p.EsActivo, p.CreadoEn, p.ModificadoEn);
}
