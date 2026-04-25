namespace MundoVs.Core.Security;

public static class ModuloAccesoCatalog
{
    public const string Plataforma = "plataforma";
    public const string Administracion = "administracion";
    public const string Catalogos = "catalogos";
    public const string Manufactura = "manufactura";
    public const string Logistica = "logistica";
    public const string Contabilidad = "contabilidad";
    public const string Rrhh = "rrhh";
    public const string Configuracion = "configuracion";

    public static IReadOnlyList<ModuloAccesoDef> Todos { get; } =
    [
        new(Plataforma, "Plataforma", "Administración global y multiempresa", 1, true),
        new(Administracion, "Administración", "Usuarios, roles y permisos por empresa", 2, false),
        new(Catalogos, "Catálogos", "Clientes, productos y compras base", 10, false),
        new(Manufactura, "Manufactura", "Pedidos, cotizaciones y producción", 20, false),
        new(Logistica, "Logística", "Inventario, insumos y finished goods", 30, false),
        new(Contabilidad, "Contabilidad", "Finanzas, facturación y cuentas por pagar", 40, false),
        new(Rrhh, "Recursos Humanos", "Empleados, prenómina y nómina", 50, false),
        new(Configuracion, "Configuración", "Parámetros operativos por empresa", 60, false)
    ];

    private static readonly IReadOnlyList<(string Prefix, string ModuloClave)> RouteMap =
    [
        ("superadmin/", Plataforma),
        ("admin/", Administracion),
        ("configuracion", Configuracion),
        ("rrhh/", Rrhh),
        ("contabilidad/", Contabilidad),
        ("dashboard", Contabilidad),
        ("inventario/finished-goods", Logistica),
        ("inventario", Logistica),
        ("serigrafia/materias-primas", Logistica),
        ("serigrafia/insumos", Logistica),
        ("serigrafia/pedidos", Manufactura),
        ("serigrafia/cotizaciones", Manufactura),
        ("serigrafia/tipos-proceso", Manufactura),
        ("serigrafia/posiciones", Manufactura),
        ("serigrafia/pantallas", Manufactura),
        ("serigrafia/disenos", Manufactura),
        ("serigrafia/gastos-fijos", Manufactura),
        ("calzado/hormas", Manufactura),
        ("clientes", Catalogos),
        ("productos", Catalogos),
        ("proveedores", Catalogos)
    ];

    public static string? ResolverModuloRuta(string? route)
    {
        if (string.IsNullOrWhiteSpace(route))
            return null;

        var normalized = route.Trim('/');
        return RouteMap
            .OrderByDescending(item => item.Prefix.Length)
            .FirstOrDefault(item => normalized.StartsWith(item.Prefix, StringComparison.OrdinalIgnoreCase))
            .ModuloClave;
    }

    public sealed record ModuloAccesoDef(string Clave, string Nombre, string Descripcion, int Orden, bool EsGlobal);
}
