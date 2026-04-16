namespace CoreTemplate.SharedKernel;

/// <summary>
/// Representa el resultado paginado de una consulta de listado.
/// <para>
/// Encapsula los ítems de la página actual junto con la información
/// de paginación necesaria para que el cliente navegue entre páginas.
/// </para>
/// <para>
/// Uso típico en un QueryHandler:
/// <code>
/// var total = await _db.Usuarios.CountAsync(ct);
/// var items = await _db.Usuarios
///     .Skip((query.Pagina - 1) * query.TamanoPagina)
///     .Take(query.TamanoPagina)
///     .Select(u => u.ToResumenDto())
///     .ToListAsync(ct);
///
/// return Result&lt;PagedResult&lt;UsuarioResumenDto&gt;&gt;.Success(
///     new PagedResult&lt;UsuarioResumenDto&gt;(items, query.Pagina, query.TamanoPagina, total));
/// </code>
/// </para>
/// </summary>
/// <typeparam name="T">Tipo de los ítems en la página actual.</typeparam>
public sealed class PagedResult<T>
{
    /// <summary>Ítems de la página actual.</summary>
    public IReadOnlyList<T> Items { get; }

    /// <summary>Número de página actual (base 1).</summary>
    public int Pagina { get; }

    /// <summary>Cantidad de ítems por página.</summary>
    public int TamanoPagina { get; }

    /// <summary>Total de ítems en toda la colección (sin paginar).</summary>
    public int Total { get; }

    /// <summary>Total de páginas disponibles.</summary>
    public int TotalPaginas => (int)Math.Ceiling((double)Total / TamanoPagina);

    /// <summary>Indica si existe una página anterior.</summary>
    public bool TienePaginaAnterior => Pagina > 1;

    /// <summary>Indica si existe una página siguiente.</summary>
    public bool TienePaginaSiguiente => Pagina < TotalPaginas;

    /// <summary>
    /// Crea un nuevo resultado paginado.
    /// </summary>
    /// <param name="items">Ítems de la página actual.</param>
    /// <param name="pagina">Número de página actual (base 1).</param>
    /// <param name="tamanoPagina">Cantidad de ítems por página.</param>
    /// <param name="total">Total de ítems en toda la colección.</param>
    public PagedResult(IReadOnlyList<T> items, int pagina, int tamanoPagina, int total)
    {
        Items = items;
        Pagina = pagina;
        TamanoPagina = tamanoPagina;
        Total = total;
    }
}
