using System.Text.Json;

namespace CoreTemplate.Modules.Configuracion.Application.Abstractions;

/// <summary>
/// Servicio público para leer y actualizar parámetros de configuración del sistema.
/// Implementa cache con TTL de 10 minutos para evitar consultas repetidas a BD.
/// Jerarquía de resolución: tenant actual → global (TenantId = null) → valorPorDefecto.
/// Nunca lanza excepciones — siempre retorna un valor.
/// </summary>
public interface IConfiguracionService
{
    /// <summary>Obtiene un parámetro como string. Retorna valorPorDefecto si no existe.</summary>
    Task<string> ObtenerStringAsync(string clave, string valorPorDefecto = "", CancellationToken ct = default);

    /// <summary>Obtiene un parámetro como entero. Retorna valorPorDefecto si no existe o no es parseable.</summary>
    Task<int> ObtenerIntAsync(string clave, int valorPorDefecto = 0, CancellationToken ct = default);

    /// <summary>Obtiene un parámetro como decimal.</summary>
    Task<decimal> ObtenerDecimalAsync(string clave, decimal valorPorDefecto = 0, CancellationToken ct = default);

    /// <summary>Obtiene un parámetro como bool. Retorna valorPorDefecto si no existe.</summary>
    Task<bool> ObtenerBoolAsync(string clave, bool valorPorDefecto = false, CancellationToken ct = default);

    /// <summary>Obtiene un parámetro JSON deserializado. Retorna null si no existe o no es JSON válido.</summary>
    Task<T?> ObtenerJsonAsync<T>(string clave, CancellationToken ct = default) where T : class;

    /// <summary>Actualiza el valor de un parámetro e invalida su cache.</summary>
    Task ActualizarAsync(string clave, string valor, Guid modificadoPor, CancellationToken ct = default);

    /// <summary>Invalida el cache de una clave específica.</summary>
    void InvalidarCache(string clave);
}
