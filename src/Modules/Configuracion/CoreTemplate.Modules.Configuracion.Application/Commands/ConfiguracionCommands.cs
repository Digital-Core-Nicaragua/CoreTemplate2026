using CoreTemplate.Modules.Configuracion.Application.Abstractions;
using CoreTemplate.Modules.Configuracion.Application.DTOs;
using CoreTemplate.Modules.Configuracion.Domain.Aggregates;
using CoreTemplate.Modules.Configuracion.Domain.Repositories;
using CoreTemplate.SharedKernel;
using MediatR;

namespace CoreTemplate.Modules.Configuracion.Application.Commands;

// ─── Actualizar ───────────────────────────────────────────────────────────────

public record ActualizarConfiguracionCommand(
    string Clave,
    string Valor,
    Guid ModificadoPor) : IRequest<Result<ConfiguracionItemDto>>;

internal sealed class ActualizarConfiguracionHandler(
    IConfiguracionItemRepository repo,
    IConfiguracionService configuracionService)
    : IRequestHandler<ActualizarConfiguracionCommand, Result<ConfiguracionItemDto>>
{
    public async Task<Result<ConfiguracionItemDto>> Handle(ActualizarConfiguracionCommand cmd, CancellationToken ct)
    {
        // Buscar primero para el tenant actual, luego global
        var item = await repo.ObtenerPorClaveAsync(cmd.Clave, null, ct);
        if (item is null)
            return Result<ConfiguracionItemDto>.Failure($"Parámetro '{cmd.Clave}' no encontrado.");

        var result = item.Actualizar(cmd.Valor, cmd.ModificadoPor);
        if (!result.IsSuccess)
            return Result<ConfiguracionItemDto>.Failure(result.Error!);

        await repo.ActualizarAsync(item, ct);
        configuracionService.InvalidarCache(cmd.Clave);

        return Result<ConfiguracionItemDto>.Success(item.ToDto(), "Parámetro actualizado correctamente.");
    }
}

// ─── Mapeo ────────────────────────────────────────────────────────────────────

internal static class ConfiguracionExtensions
{
    public static ConfiguracionItemDto ToDto(this ConfiguracionItem i) => new(
        i.Id, i.Clave, i.Valor, i.Tipo.ToString(),
        i.Descripcion, i.Grupo, i.EsEditable, i.CreadoEn, i.ModificadoEn);
}
