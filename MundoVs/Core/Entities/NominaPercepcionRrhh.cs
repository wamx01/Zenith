namespace MundoVs.Core.Entities;

public enum CategoriaPercepcionNomina
{
    Comision = 1,
    DestajoManual = 2,
    BonoManual = 3,
    OtroIngreso = 4
}

public enum OrigenPercepcionNomina
{
    Manual = 1,
    Produccion = 2,
    Ajuste = 3,
    Importacion = 4
}

public class NominaPercepcionTipo : BaseEntity
{
    public string Clave { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public CategoriaPercepcionNomina Categoria { get; set; } = CategoriaPercepcionNomina.OtroIngreso;
    public bool AfectaBaseImss { get; set; } = true;
    public bool AfectaBaseIsr { get; set; } = true;
    public int Orden { get; set; }

    public ICollection<NominaPercepcion> Percepciones { get; set; } = [];
}

public class NominaPercepcion : BaseEntity
{
    public Guid EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public Guid NominaDetalleId { get; set; }
    public NominaDetalle NominaDetalle { get; set; } = null!;

    public Guid TipoPercepcionId { get; set; }
    public NominaPercepcionTipo TipoPercepcion { get; set; } = null!;

    public string? Descripcion { get; set; }
    public decimal Importe { get; set; }
    public OrigenPercepcionNomina Origen { get; set; } = OrigenPercepcionNomina.Manual;
    public string? Referencia { get; set; }
    public string? Observaciones { get; set; }
}
