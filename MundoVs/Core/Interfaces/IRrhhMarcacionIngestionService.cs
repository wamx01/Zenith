using MundoVs.Core.Models;
using MundoVs.Infrastructure.Data;
using Zenith.Contracts.Asistencia;

namespace MundoVs.Core.Interfaces;

public interface IRrhhMarcacionIngestionService
{
    Task<RrhhMarcacionIngestionResult> IngerirLoteAsync(CrmDbContext db, MarcacionSyncBatchDto batch, CancellationToken cancellationToken = default);
}
