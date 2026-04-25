using MundoVs.Core.Models;
using MundoVs.Infrastructure.Data;

namespace MundoVs.Core.Interfaces;

public interface IRrhhMarcacionZonaHorariaService
{
    Task<RrhhMarcacionZonaHorariaCorrectionResult> CorregirMarcacionesGuardadasComoHoraLocalAsync(CrmDbContext db, RrhhMarcacionZonaHorariaCorrectionRequest request, CancellationToken cancellationToken = default);
    Task<RrhhMarcacionZonaHorariaCorrectionResult> ReconstruirHoraLocalDesdeUtcAsync(CrmDbContext db, RrhhMarcacionZonaHorariaCorrectionRequest request, CancellationToken cancellationToken = default);
}
