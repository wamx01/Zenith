using MundoVs.Core.Services;

namespace MundoVs.Tests;

public sealed class RrhhTurnoValidatorTests
{
    public static IEnumerable<object[]> DescansosValidosData()
    {
        yield return [1];
        yield return [2];
        yield return [3];
        yield return [4];
    }

    [Theory]
    [MemberData(nameof(DescansosValidosData))]
    public void Validate_AceptaTurnosConEntreUnoYCuatroDescansos(int cantidadDescansos)
    {
        var dia = CrearDiaConDescansos(cantidadDescansos);

        var resultado = RrhhTurnoValidator.Validate([dia]);

        Assert.Null(resultado);
    }

    [Fact]
    public void Validate_CuandoDescansosSeTraslapan_RegresaMensajeClaro()
    {
        var dia = new RrhhTurnoValidator.TurnoDiaValidacionInput(
            1,
            "Lunes",
            true,
            "08:00",
            "17:00",
            [
                new RrhhTurnoValidator.TurnoDescansoValidacionInput(1, "12:00", "12:30", false),
                new RrhhTurnoValidator.TurnoDescansoValidacionInput(2, "12:20", "12:40", false),
                new RrhhTurnoValidator.TurnoDescansoValidacionInput(3, "14:00", "14:15", false)
            ]);

        var resultado = RrhhTurnoValidator.Validate([dia]);

        Assert.Equal("Lunes: el descanso 2 se traslapa con el descanso 1.", resultado);
    }

    [Fact]
    public void Validate_CuandoDescansoQuedaFueraDeJornada_RegresaMensajeClaro()
    {
        var dia = new RrhhTurnoValidator.TurnoDiaValidacionInput(
            1,
            "Lunes",
            true,
            "08:00",
            "17:00",
            [
                new RrhhTurnoValidator.TurnoDescansoValidacionInput(1, "16:50", "17:10", false)
            ]);

        var resultado = RrhhTurnoValidator.Validate([dia]);

        Assert.Equal("Lunes: el descanso 1 debe quedar dentro del horario 08:00-17:00.", resultado);
    }

    private static RrhhTurnoValidator.TurnoDiaValidacionInput CrearDiaConDescansos(int cantidadDescansos)
    {
        var descansos = new List<RrhhTurnoValidator.TurnoDescansoValidacionInput>();
        var horarios = new (string Inicio, string Fin)[]
        {
            ("10:00", "10:15"),
            ("12:00", "12:30"),
            ("14:00", "14:15"),
            ("16:00", "16:10")
        };

        for (byte i = 0; i < cantidadDescansos; i++)
        {
            descansos.Add(new RrhhTurnoValidator.TurnoDescansoValidacionInput(
                (byte)(i + 1),
                horarios[i].Inicio,
                horarios[i].Fin,
                false));
        }

        return new RrhhTurnoValidator.TurnoDiaValidacionInput(
            1,
            "Lunes",
            true,
            "08:00",
            "17:00",
            descansos);
    }
}
