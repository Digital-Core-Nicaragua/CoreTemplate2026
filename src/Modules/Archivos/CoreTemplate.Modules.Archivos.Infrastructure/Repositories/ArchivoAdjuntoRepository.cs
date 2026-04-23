using CoreTemplate.Modules.Archivos.Domain.Aggregates;
using CoreTemplate.Modules.Archivos.Domain.Repositories;
using CoreTemplate.Modules.Archivos.Infrastructure.Persistence;
using CoreTemplate.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace CoreTemplate.Modules.Archivos.Infrastructure.Repositories;

internal sealed class ArchivoAdjuntoRepository(ArchivosDbContext db) : IArchivoAdjuntoRepository
{
    public async Task<ArchivoAdjunto?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default) =>
        await db.Archivos.FirstOrDefaultAsync(a => a.Id == id && a.EsActivo, ct);

    public async Task<PagedResult<ArchivoAdjunto>> ListarAsync(
        string? moduloOrigen, Guid? entidadId, int pagina, int tamano, CancellationToken ct = default)
    {
        var query = db.Archivos.Where(a => a.EsActivo).AsQueryable();

        if (moduloOrigen is not null)
            query = query.Where(a => a.ModuloOrigen == moduloOrigen);

        if (entidadId is not null)
            query = query.Where(a => a.EntidadId == entidadId);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(a => a.FechaSubida)
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .ToListAsync(ct);

        return new PagedResult<ArchivoAdjunto>(items, pagina, tamano, total);
    }

    public async Task GuardarAsync(ArchivoAdjunto archivo, CancellationToken ct = default)
    {
        await db.Archivos.AddAsync(archivo, ct);
        await db.SaveChangesAsync(ct);
        archivo.ClearDomainEvents();
    }

    public async Task ActualizarAsync(ArchivoAdjunto archivo, CancellationToken ct = default)
    {
        db.Archivos.Update(archivo);
        await db.SaveChangesAsync(ct);
        archivo.ClearDomainEvents();
    }
}
