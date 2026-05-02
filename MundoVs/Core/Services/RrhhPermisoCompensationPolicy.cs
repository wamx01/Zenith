using MundoVs.Core.Entities;

namespace MundoVs.Core.Services;

public static class RrhhPermisoCompensationPolicy
{
    public static int ObtenerMinutosRecuperablesAprobables(RrhhAsistencia asistencia, int minutosMinimosTiempoExtra)
    {
        if (asistencia.HoraEntradaProgramada is not TimeSpan entradaProgramada || asistencia.HoraSalidaProgramada is not TimeSpan salidaProgramada)
        {
            return 0;
        }

        minutosMinimosTiempoExtra = ObtenerMinutosMinimosTiempoExtra(minutosMinimosTiempoExtra);

        var minutosEntradaAnticipada = asistencia.HoraEntradaReal.HasValue
            ? Math.Max(0, (int)Math.Round((entradaProgramada - asistencia.HoraEntradaReal.Value).TotalMinutes))
            : 0;
        var minutosSalidaPosterior = asistencia.HoraSalidaReal.HasValue
            ? Math.Max(0, (int)Math.Round((asistencia.HoraSalidaReal.Value - salidaProgramada).TotalMinutes))
            : 0;

        if (minutosEntradaAnticipada >= minutosMinimosTiempoExtra)
        {
            minutosEntradaAnticipada = 0;
        }

        if (minutosSalidaPosterior >= minutosMinimosTiempoExtra)
        {
            minutosSalidaPosterior = 0;
        }

        return Math.Max(0, minutosEntradaAnticipada + minutosSalidaPosterior);
    }

    private static int ObtenerMinutosMinimosTiempoExtra(int minutosMinimosTiempoExtra)
        => minutosMinimosTiempoExtra > 0 ? minutosMinimosTiempoExtra : 30;
}
