using System.ComponentModel.DataAnnotations.Schema;

namespace MundoVs.Core.Entities;

public class TurnoBase : BaseEntity
{
    public Guid EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }

    public ICollection<TurnoBaseDetalle> Detalles { get; set; } = [];
    public ICollection<Empleado> Empleados { get; set; } = [];
    public ICollection<RrhhEmpleadoTurno> EmpleadosVigencia { get; set; } = [];
}

public class TurnoBaseDetalle : BaseEntity
{
    public Guid TurnoBaseId { get; set; }
    public TurnoBase TurnoBase { get; set; } = null!;

    public DiaSemanaTurno DiaSemana { get; set; }
    public bool Labora { get; set; } = true;
    public TimeSpan? HoraEntrada { get; set; }
    public TimeSpan? HoraSalida { get; set; }
    public ICollection<TurnoBaseDetalleDescanso> Descansos { get; set; } = [];

    [NotMapped]
    public byte CantidadDescansos
    {
        get => (byte)Descansos.Count;
        set => AjustarCantidadDescansos(value);
    }

    [NotMapped]
    public TimeSpan? Descanso1Inicio
    {
        get => ObtenerDescanso(1)?.HoraInicio;
        set => AsegurarDescanso(1).HoraInicio = value;
    }

    [NotMapped]
    public TimeSpan? Descanso1Fin
    {
        get => ObtenerDescanso(1)?.HoraFin;
        set => AsegurarDescanso(1).HoraFin = value;
    }

    [NotMapped]
    public bool Descanso1EsPagado
    {
        get => ObtenerDescanso(1)?.EsPagado ?? false;
        set => AsegurarDescanso(1).EsPagado = value;
    }

    [NotMapped]
    public TimeSpan? Descanso2Inicio
    {
        get => ObtenerDescanso(2)?.HoraInicio;
        set => AsegurarDescanso(2).HoraInicio = value;
    }

    [NotMapped]
    public TimeSpan? Descanso2Fin
    {
        get => ObtenerDescanso(2)?.HoraFin;
        set => AsegurarDescanso(2).HoraFin = value;
    }

    [NotMapped]
    public bool Descanso2EsPagado
    {
        get => ObtenerDescanso(2)?.EsPagado ?? false;
        set => AsegurarDescanso(2).EsPagado = value;
    }

    private void AjustarCantidadDescansos(byte cantidad)
    {
        while (Descansos.Count > cantidad)
        {
            var ultimo = Descansos.OrderByDescending(d => d.Orden).First();
            Descansos.Remove(ultimo);
        }

        while (Descansos.Count < cantidad)
        {
            Descansos.Add(new TurnoBaseDetalleDescanso
            {
                Id = Guid.NewGuid(),
                Orden = (byte)(Descansos.Count + 1),
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            });
        }
    }

    private TurnoBaseDetalleDescanso? ObtenerDescanso(byte orden)
        => Descansos.FirstOrDefault(d => d.Orden == orden);

    private TurnoBaseDetalleDescanso AsegurarDescanso(byte orden)
    {
        var descanso = ObtenerDescanso(orden);
        if (descanso != null)
        {
            return descanso;
        }

        AjustarCantidadDescansos(orden);
        return ObtenerDescanso(orden)!;
    }
}

public class TurnoBaseDetalleDescanso : BaseEntity
{
    public Guid TurnoBaseDetalleId { get; set; }
    public TurnoBaseDetalle TurnoBaseDetalle { get; set; } = null!;

    public byte Orden { get; set; }
    public TimeSpan? HoraInicio { get; set; }
    public TimeSpan? HoraFin { get; set; }
    public bool EsPagado { get; set; }
}

public enum DiaSemanaTurno
{
    Lunes = 1,
    Martes = 2,
    Miercoles = 3,
    Jueves = 4,
    Viernes = 5,
    Sabado = 6,
    Domingo = 7
}
