namespace MundoVs.Core.Services;

public static class RrhhTurnoValidator
{
    public static string? Validate(IReadOnlyList<TurnoDiaValidacionInput> dias)
    {
        foreach (var dia in dias.Where(d => d.Labora).OrderBy(d => d.DiaSemana))
        {
            if (!TryParseHora(dia.HoraEntradaTexto, out var entrada))
            {
                return $"{dia.NombreDia}: captura una hora de entrada válida.";
            }

            if (!TryParseHora(dia.HoraSalidaTexto, out var salida))
            {
                return $"{dia.NombreDia}: captura una hora de salida válida.";
            }

            if (salida <= entrada)
            {
                return $"{dia.NombreDia}: la salida debe ser mayor a la entrada.";
            }

            var descansosCapturados = new List<DescansoCapturado>();
            foreach (var descanso in dia.Descansos.OrderBy(d => d.Orden))
            {
                var inicioVacio = string.IsNullOrWhiteSpace(descanso.HoraInicioTexto);
                var finVacio = string.IsNullOrWhiteSpace(descanso.HoraFinTexto);
                if (inicioVacio && finVacio)
                {
                    continue;
                }

                if (inicioVacio || finVacio)
                {
                    return $"{dia.NombreDia}: completa inicio y fin del descanso {descanso.Orden}.";
                }

                if (!TryParseHora(descanso.HoraInicioTexto, out var inicio))
                {
                    return $"{dia.NombreDia}: captura una hora válida en el inicio del descanso {descanso.Orden}.";
                }

                if (!TryParseHora(descanso.HoraFinTexto, out var fin))
                {
                    return $"{dia.NombreDia}: captura una hora válida en el fin del descanso {descanso.Orden}.";
                }

                if (fin <= inicio)
                {
                    return $"{dia.NombreDia}: el descanso {descanso.Orden} debe terminar después de iniciar.";
                }

                if (inicio < entrada || fin > salida)
                {
                    return $"{dia.NombreDia}: el descanso {descanso.Orden} debe quedar dentro del horario {FormatearHora(entrada)}-{FormatearHora(salida)}.";
                }

                descansosCapturados.Add(new DescansoCapturado(descanso.Orden, inicio, fin));
            }

            var descansosOrdenados = descansosCapturados.OrderBy(d => d.Inicio).ToList();
            for (var index = 1; index < descansosOrdenados.Count; index++)
            {
                var anterior = descansosOrdenados[index - 1];
                var actual = descansosOrdenados[index];
                if (actual.Inicio < anterior.Fin)
                {
                    return $"{dia.NombreDia}: el descanso {actual.Orden} se traslapa con el descanso {anterior.Orden}.";
                }
            }
        }

        return null;
    }

    private static bool TryParseHora(string? valor, out TimeSpan hora)
        => TimeSpan.TryParse(valor, out hora);

    private static string FormatearHora(TimeSpan hora)
        => hora.ToString("hh':'mm");

    public sealed record TurnoDiaValidacionInput(
        int DiaSemana,
        string NombreDia,
        bool Labora,
        string HoraEntradaTexto,
        string HoraSalidaTexto,
        IReadOnlyList<TurnoDescansoValidacionInput> Descansos);

    public sealed record TurnoDescansoValidacionInput(
        byte Orden,
        string HoraInicioTexto,
        string HoraFinTexto,
        bool EsPagado);

    private sealed record DescansoCapturado(byte Orden, TimeSpan Inicio, TimeSpan Fin);
}
