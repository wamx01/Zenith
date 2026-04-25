using MundoVs.Core.Entities;

namespace MundoVs.Core.Entities.Serigrafia;

public class TipoProceso : BaseEntity
{
    public Guid EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;
    public Guid? PosicionId { get; set; }

    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool Activo { get; set; } = true;
    public int Orden { get; set; } // Para ordenar en la UI
    
    // Navegación
    public Posicion? Posicion { get; set; }
    public ICollection<PedidoSerigrafiaProcesoDetalle> PedidosProcesos { get; set; } = new List<PedidoSerigrafiaProcesoDetalle>();
    public ICollection<RegistroDestajoProceso> RegistrosDestajo { get; set; } = new List<RegistroDestajoProceso>();
}

// Enum para tipos de proceso predefinidos
public enum TipoProcesoEnum
{
    Mesa = 1,
    Pulpo = 2,
    Transfer = 3,
    Sublimacion = 4,
    Plancha = 5,
    Frecuencia = 6,
    Ploteo = 7,
    UV = 8,
    Plastisol = 9
}
