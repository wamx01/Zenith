namespace MundoVs.Core.Entities;

public class DeduccionTipoRrhh : BaseEntity
{
    public string Clave { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool EsLegal { get; set; }
    public bool AfectaRecibo { get; set; } = true;
    public int Orden { get; set; }

    public ICollection<NominaDeduccion> Deducciones { get; set; } = [];
}

public class NominaDeduccion : BaseEntity
{
    public Guid EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public Guid NominaDetalleId { get; set; }
    public NominaDetalle NominaDetalle { get; set; } = null!;

    public Guid TipoDeduccionId { get; set; }
    public DeduccionTipoRrhh TipoDeduccion { get; set; } = null!;

    public string? Descripcion { get; set; }
    public decimal Importe { get; set; }
    public bool EsRetencionLegal { get; set; }
    public string? Observaciones { get; set; }
}
