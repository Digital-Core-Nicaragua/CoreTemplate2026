using CoreTemplate.Modules.EmailTemplates.Application.DTOs;
using CoreTemplate.Modules.EmailTemplates.Domain.Repositories;
using CoreTemplate.Modules.EmailTemplates.Domain.Aggregates;
using CoreTemplate.SharedKernel;
using MediatR;

namespace CoreTemplate.Modules.EmailTemplates.Application.Commands;

// ─── Crear ───────────────────────────────────────────────────────────────────

public record CrearPlantillaCommand(
    string Codigo,
    string Nombre,
    string Modulo,
    string Asunto,
    string CuerpoHtml,
    List<string>? Variables,
    bool UsarLayout = true) : IRequest<Result<EmailTemplateDto>>;

internal sealed class CrearPlantillaHandler(IEmailTemplateRepository repo)
    : IRequestHandler<CrearPlantillaCommand, Result<EmailTemplateDto>>
{
    public async Task<Result<EmailTemplateDto>> Handle(CrearPlantillaCommand cmd, CancellationToken ct)
    {
        if (await repo.ExisteCodigoAsync(cmd.Codigo, null, ct))
            return Result<EmailTemplateDto>.Failure("Ya existe una plantilla con ese código.");

        var result = EmailTemplate.Crear(cmd.Codigo, cmd.Nombre, cmd.Modulo, cmd.Asunto, cmd.CuerpoHtml, cmd.Variables, cmd.UsarLayout);
        if (!result.IsSuccess) return Result<EmailTemplateDto>.Failure(result.Error!);

        await repo.GuardarAsync(result.Value!, ct);
        return Result<EmailTemplateDto>.Success(result.Value!.ToDto(), "Plantilla creada correctamente.");
    }
}

// ─── Actualizar ───────────────────────────────────────────────────────────────

public record ActualizarPlantillaCommand(
    Guid Id,
    string Asunto,
    string CuerpoHtml,
    List<string>? Variables,
    Guid ModificadoPor) : IRequest<Result<EmailTemplateDto>>;

internal sealed class ActualizarPlantillaHandler(IEmailTemplateRepository repo)
    : IRequestHandler<ActualizarPlantillaCommand, Result<EmailTemplateDto>>
{
    public async Task<Result<EmailTemplateDto>> Handle(ActualizarPlantillaCommand cmd, CancellationToken ct)
    {
        var template = await repo.ObtenerPorIdAsync(cmd.Id, ct);
        if (template is null) return Result<EmailTemplateDto>.Failure("Plantilla no encontrada.");

        var result = template.Actualizar(cmd.Asunto, cmd.CuerpoHtml, cmd.Variables, cmd.ModificadoPor);
        if (!result.IsSuccess) return Result<EmailTemplateDto>.Failure(result.Error!);

        await repo.GuardarAsync(template, ct);
        return Result<EmailTemplateDto>.Success(template.ToDto(), "Plantilla actualizada correctamente.");
    }
}

// ─── Activar / Desactivar ─────────────────────────────────────────────────────

public record ActivarPlantillaCommand(Guid Id) : IRequest<Result>;
public record DesactivarPlantillaCommand(Guid Id) : IRequest<Result>;

internal sealed class ActivarPlantillaHandler(IEmailTemplateRepository repo)
    : IRequestHandler<ActivarPlantillaCommand, Result>
{
    public async Task<Result> Handle(ActivarPlantillaCommand cmd, CancellationToken ct)
    {
        var template = await repo.ObtenerPorIdAsync(cmd.Id, ct);
        if (template is null) return Result.Failure("Plantilla no encontrada.");
        var result = template.Activar();
        if (!result.IsSuccess) return result;
        await repo.GuardarAsync(template, ct);
        return Result.Success();
    }
}

internal sealed class DesactivarPlantillaHandler(IEmailTemplateRepository repo)
    : IRequestHandler<DesactivarPlantillaCommand, Result>
{
    public async Task<Result> Handle(DesactivarPlantillaCommand cmd, CancellationToken ct)
    {
        var template = await repo.ObtenerPorIdAsync(cmd.Id, ct);
        if (template is null) return Result.Failure("Plantilla no encontrada.");
        var result = template.Desactivar();
        if (!result.IsSuccess) return result;
        await repo.GuardarAsync(template, ct);
        return Result.Success();
    }
}

// ─── Enviar prueba ────────────────────────────────────────────────────────────

public record EnviarPruebaCommand(
    Guid TemplateId,
    string Destinatario,
    Dictionary<string, string> Variables) : IRequest<Result>;

internal sealed class EnviarPruebaHandler(
    IEmailTemplateRepository repo,
    Abstractions.IEmailTemplateSender sender)
    : IRequestHandler<EnviarPruebaCommand, Result>
{
    public async Task<Result> Handle(EnviarPruebaCommand cmd, CancellationToken ct)
    {
        var template = await repo.ObtenerPorIdAsync(cmd.TemplateId, ct);
        if (template is null) return Result.Failure("Plantilla no encontrada.");

        var emailResult = await sender.EnviarAsync(new Abstractions.EnviarConPlantillaRequest(
            template.Codigo, cmd.Destinatario, cmd.Variables), ct);

        return emailResult.Exitoso
            ? Result.Success()
            : Result.Failure($"Fallo al enviar prueba: {emailResult.MensajeError}");
    }
}

// ─── Extensión de mapeo ───────────────────────────────────────────────────────

internal static class EmailTemplateExtensions
{
    public static EmailTemplateDto ToDto(this EmailTemplate t) => new(
        t.Id, t.Codigo, t.Nombre, t.Modulo, t.Asunto, t.CuerpoHtml,
        t.VariablesDisponibles, t.UsarLayout, t.EsDeSistema, t.EsActivo,
        t.CreadoEn, t.ModificadoEn);
}
