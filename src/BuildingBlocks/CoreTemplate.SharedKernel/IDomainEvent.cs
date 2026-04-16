namespace CoreTemplate.SharedKernel;

/// <summary>
/// Contrato base para todos los eventos de dominio del sistema.
/// <para>
/// Un evento de dominio representa algo que ocurrió en el dominio y que
/// otras partes del sistema pueden necesitar conocer. Se publican después
/// de que la transacción se completa exitosamente.
/// </para>
/// <para>
/// Uso típico:
/// <code>
/// public record UsuarioRegistradoEvent(Guid UsuarioId, string Email) : IDomainEvent;
/// </code>
/// </para>
/// </summary>
public interface IDomainEvent;
