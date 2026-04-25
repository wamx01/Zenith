namespace MundoVs.Core.Entities;

public enum TipoCapturaBonoNomina
{
    PorRubroImporte = 1,
    TotalDistribuido = 2
}

public class BonoRubroRrhh : BaseEntity
{
    public Guid EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public string Clave { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int Orden { get; set; }

    public ICollection<BonoEstructuraDetalleRrhh> EstructurasDetalle { get; set; } = [];
    public ICollection<NominaBonoDetalle> BonosDetalle { get; set; } = [];
}

public class BonoEstructuraRrhh : BaseEntity
{
    public Guid EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public TipoCapturaBonoNomina TipoCaptura { get; set; } = TipoCapturaBonoNomina.PorRubroImporte;

    public ICollection<BonoEstructuraDetalleRrhh> Detalles { get; set; } = [];
}

public class BonoEstructuraDetalleRrhh : BaseEntity
{
    public Guid BonoEstructuraRrhhId { get; set; }
    public BonoEstructuraRrhh BonoEstructuraRrhh { get; set; } = null!;

    public Guid BonoRubroRrhhId { get; set; }
    public BonoRubroRrhh BonoRubroRrhh { get; set; } = null!;

    public int Orden { get; set; }
    public decimal Porcentaje { get; set; }
}

public class NominaBono : BaseEntity
{
    public Guid EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public Guid NominaDetalleId { get; set; }
    public NominaDetalle NominaDetalle { get; set; } = null!;

    public TipoCapturaBonoNomina TipoCaptura { get; set; } = TipoCapturaBonoNomina.PorRubroImporte;
    public decimal MontoTotal { get; set; }
    public string? Observaciones { get; set; }

    public ICollection<NominaBonoDetalle> Detalles { get; set; } = [];
}

public class NominaBonoDetalle : BaseEntity
{
    public Guid NominaBonoId { get; set; }
    public NominaBono NominaBono { get; set; } = null!;

    public Guid BonoRubroRrhhId { get; set; }
    public BonoRubroRrhh BonoRubroRrhh { get; set; } = null!;

    public decimal Porcentaje { get; set; }
    public decimal Importe { get; set; }
    public int Orden { get; set; }
}
