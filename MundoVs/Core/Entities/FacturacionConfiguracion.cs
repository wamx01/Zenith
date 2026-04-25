namespace MundoVs.Core.Entities;

public class FacturacionConfiguracion
{
    public bool Habilitado { get; set; }
    public string Proveedor { get; set; } = "Facturapi";
    public bool ModoPruebas { get; set; } = true;
    public string SerieIngreso { get; set; } = "F";
    public string SerieEgreso { get; set; } = "E";
    public string SeriePago { get; set; } = "P";
    public string LugarExpedicionCp { get; set; } = string.Empty;
    public string MonedaDefault { get; set; } = "MXN";
    public string MetodoPagoContado { get; set; } = "PUE";
    public string FormaPagoContado { get; set; } = "03";
    public string MetodoPagoCredito { get; set; } = "PPD";
    public string FormaPagoCredito { get; set; } = "99";
    public string? ApiKeyPruebas { get; set; }
    public string? ApiKeyProduccion { get; set; }
}

public static class ClavesConfiguracionFacturacion
{
    public const string Habilitado = "Facturacion:Habilitado";
    public const string Proveedor = "Facturacion:Proveedor";
    public const string ModoPruebas = "Facturacion:ModoPruebas";
    public const string SerieIngreso = "Facturacion:Serie:Ingreso";
    public const string SerieEgreso = "Facturacion:Serie:Egreso";
    public const string SeriePago = "Facturacion:Serie:Pago";
    public const string LugarExpedicionCp = "Facturacion:LugarExpedicionCp";
    public const string MonedaDefault = "Facturacion:MonedaDefault";
    public const string MetodoPagoContado = "Facturacion:MetodoPago:Contado";
    public const string FormaPagoContado = "Facturacion:FormaPago:Contado";
    public const string MetodoPagoCredito = "Facturacion:MetodoPago:Credito";
    public const string FormaPagoCredito = "Facturacion:FormaPago:Credito";
    public const string ApiKeyPruebas = "Facturacion:ApiKey:Pruebas";
    public const string ApiKeyProduccion = "Facturacion:ApiKey:Produccion";
}
