using CoreTemplate.Modules.EmailTemplates.Domain.Aggregates;

namespace CoreTemplate.Modules.EmailTemplates.Domain.Repositories;

public interface IEmailTemplateRepository
{
    Task<EmailTemplate?> ObtenerPorCodigoAsync(string codigo, Guid? tenantId = null, CancellationToken ct = default);
    Task<EmailTemplate?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<EmailTemplate>> ListarAsync(string? modulo = null, bool? soloActivos = null, CancellationToken ct = default);
    Task<bool> ExisteCodigoAsync(string codigo, Guid? tenantId = null, CancellationToken ct = default);
    Task GuardarAsync(EmailTemplate template, CancellationToken ct = default);
}
