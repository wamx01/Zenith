using MundoVs.Core.Entities;
using MundoVs.Core.Services;

namespace MundoVs.Core.Interfaces;

public interface INominaReciboBuilder
{
    NominaReciboResult Build(NominaDetalle detalle, IReadOnlyList<RrhhBancoHorasMovimiento>? movimientosBancoHoras = null, decimal saldoActualBancoHoras = 0m);
}
