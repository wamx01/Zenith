using MundoVs.Core.Entities;
using MundoVs.Core.Models;

namespace MundoVs.Core.Interfaces;

public interface IRrhhAsistenciaCorreccionAdvisor
{
    RrhhAsistenciaCorreccionAdvice Analizar(
        RrhhAsistencia asistencia,
        RrhhAusencia? permisoDia,
        bool bancoHorasHabilitado,
        bool puedeAprobarTiempoExtra,
        decimal factorTiempoExtra,
        int saldoBancoHorasMinutos);
}
