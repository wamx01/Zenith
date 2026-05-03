using Microsoft.EntityFrameworkCore;
using MundoVs.Core.Entities;
using MundoVs.Core.Interfaces;
using MundoVs.Infrastructure.Data;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace MundoVs.Core.Services;

public class NotaEntregaPdfService : INotaEntregaPdfService
{
    private readonly IDbContextFactory<CrmDbContext> _dbFactory;
    private readonly ITenantFileStorageService _fileStorage;
    private readonly IWebHostEnvironment _env;

    public NotaEntregaPdfService(IDbContextFactory<CrmDbContext> dbFactory, ITenantFileStorageService fileStorage, IWebHostEnvironment env)
    {
        _dbFactory = dbFactory;
        _fileStorage = fileStorage;
        _env = env;
    }

    public async Task<string> GenerateAndStoreAsync(Guid notaEntregaId, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var nota = await db.NotasEntrega
            .Include(n => n.Empresa)
            .Include(n => n.Cliente)
            .Include(n => n.Pedido)
            .Include(n => n.PedidosRelacionados)
                .ThenInclude(r => r.Pedido)
            .Include(n => n.Detalles)
                .ThenInclude(d => d.Tallas)
            .FirstOrDefaultAsync(n => n.Id == notaEntregaId, cancellationToken);

        if (nota == null)
            throw new InvalidOperationException("No se encontró la nota de entrega para generar el PDF.");

        var clienteDireccion = FormatearDireccion(nota.Cliente);
        var importeEnLetra = ConvertirImporteALetras(nota.Total);
        var elaboradoPor = string.IsNullOrWhiteSpace(nota.CreatedBy) ? "—" : nota.CreatedBy.Trim();
        var fechaEntrega = nota.Pedido.FechaEntregaEstimada?.ToString("dd/MM/yyyy") ?? "—";
        var pedidosRelacionadosTexto = nota.PedidosRelacionados.Any()
            ? string.Join(", ", nota.PedidosRelacionados.OrderBy(r => r.Orden).Select(r => r.Pedido.NumeroPedido))
            : nota.Pedido.NumeroPedido;
        var fechaVencimiento = ObtenerFechaVencimiento(nota);
        var lugarPago = ObtenerLugarPago(nota);
        var nombreEmpresa = nota.Empresa.NombreComercial ?? nota.Empresa.RazonSocial;
        var pagareTexto = string.IsNullOrWhiteSpace(nota.TextoPagare)
            ? ConstruirTextoPagare(nota, nombreEmpresa, importeEnLetra, fechaVencimiento, lugarPago)
            : nota.TextoPagare.Trim();
        var logoEmpresa = await CargarLogoEmpresaAsync(db, nota.EmpresaId, cancellationToken);

        var pdfBytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(24);
                page.Size(PageSizes.Letter);
                page.DefaultTextStyle(x => x.FontSize(8.8f).FontFamily(Fonts.Arial));

                page.Header().Column(header =>
                {
                    header.Item().Row(row =>
                    {
                        row.ConstantItem(68).Height(58).Border(1).BorderColor(Colors.Grey.Lighten1).Padding(4).AlignCenter().AlignMiddle().Element(box =>
                        {
                            if (logoEmpresa is not null)
                                box.Image(logoEmpresa).FitArea();
                            else
                                box.AlignCenter().AlignMiddle().Text(ObtenerInicialesEmpresa(nombreEmpresa)).SemiBold().FontSize(18).FontColor(Colors.Blue.Darken3);
                        });

                        row.RelativeItem().PaddingLeft(10).Column(center =>
                        {
                            center.Item().AlignCenter().Text(nombreEmpresa).Bold().FontSize(15).FontColor(Colors.Blue.Darken3);
                            center.Item().AlignCenter().Text("NOTA DE ENTREGA").SemiBold().FontSize(10.5f);
                            if (!string.IsNullOrWhiteSpace(nota.Empresa.Rfc))
                                center.Item().AlignCenter().Text($"RFC EMISOR: {nota.Empresa.Rfc}").FontSize(7.5f).FontColor(Colors.Grey.Darken2);
                        });

                        row.ConstantItem(195).Border(1).BorderColor(Colors.Grey.Lighten1).Padding(8).Column(right =>
                        {
                            right.Item().AlignCenter().Text("CONTROL DOCUMENTAL").SemiBold().FontSize(9.5f);
                            right.Item().PaddingTop(2).Row(info =>
                            {
                                info.RelativeItem().Text("NOTA No.").SemiBold();
                                info.RelativeItem().AlignRight().Text(nota.NumeroNota).SemiBold();
                            });
                            right.Item().Row(info =>
                            {
                                info.RelativeItem().Text("PEDIDO(S)").SemiBold();
                                info.RelativeItem().AlignRight().Text(pedidosRelacionadosTexto);
                            });
                            right.Item().Row(info =>
                            {
                                info.RelativeItem().Text("Fecha doc.").SemiBold();
                                info.RelativeItem().AlignRight().Text(nota.FechaNota.ToString("dd/MM/yyyy"));
                            });
                            right.Item().Row(info =>
                            {
                                info.RelativeItem().Text("Tipo").SemiBold();
                                info.RelativeItem().AlignRight().Text("COPIA");
                            });
                        });
                    });

                    header.Item().PaddingTop(8).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                });

                page.Content().Column(content =>
                {
                    content.Spacing(10);

                    content.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(block =>
                    {
                        block.Item().Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Row(x => { x.ConstantItem(82).Text("Cliente").SemiBold(); x.ConstantItem(10).Text(":"); x.RelativeItem().Text(ValorOpcion(nota.Cliente.RazonSocial)); });
                                col.Item().Row(x => { x.ConstantItem(82).Text("Dirección").SemiBold(); x.ConstantItem(10).Text(":"); x.RelativeItem().Text(clienteDireccion); });
                                col.Item().Row(x => { x.ConstantItem(82).Text("Población").SemiBold(); x.ConstantItem(10).Text(":"); x.RelativeItem().Text(ValorOpcion(nota.Cliente.Ciudad)); });
                                col.Item().Row(x => { x.ConstantItem(82).Text("R.F.C.").SemiBold(); x.ConstantItem(10).Text(":"); x.RelativeItem().Text(ValorOpcion(nota.Cliente.RfcCif)); });
                                col.Item().Row(x => { x.ConstantItem(82).Text("Teléfono").SemiBold(); x.ConstantItem(10).Text(":"); x.RelativeItem().Text(ValorOpcion(nota.Cliente.Telefono)); });
                                col.Item().Row(x => { x.ConstantItem(82).Text("Enviar a").SemiBold(); x.ConstantItem(10).Text(":"); x.RelativeItem().Text(ValorOpcion(nota.Cliente.NombreComercial)); });
                            });

                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Row(x => { x.ConstantItem(92).Text("Fecha").SemiBold(); x.ConstantItem(10).Text(":"); x.RelativeItem().Text(nota.FechaNota.ToString("dd/MM/yyyy")); });
                                col.Item().Row(x => { x.ConstantItem(92).Text("Fecha Ent.").SemiBold(); x.ConstantItem(10).Text(":"); x.RelativeItem().Text(fechaEntrega); });
                                col.Item().Row(x => { x.ConstantItem(92).Text("Fecha Doc.").SemiBold(); x.ConstantItem(10).Text(":"); x.RelativeItem().Text(nota.FechaNota.ToString("dd/MM/yyyy")); });
                                col.Item().Row(x => { x.ConstantItem(92).Text("Pedidos").SemiBold(); x.ConstantItem(10).Text(":"); x.RelativeItem().Text(pedidosRelacionadosTexto); });
                                col.Item().Row(x => { x.ConstantItem(92).Text("Vendedor").SemiBold(); x.ConstantItem(10).Text(":"); x.RelativeItem().Text(elaboradoPor); });
                                col.Item().Row(x => { x.ConstantItem(92).Text("Condición").SemiBold(); x.ConstantItem(10).Text(":"); x.RelativeItem().Text(nota.Pedido.TipoPrecio == TipoPrecioEnum.Credito ? "Crédito" : "Contado"); });
                                col.Item().Row(x => { x.ConstantItem(92).Text("CFDI").SemiBold(); x.ConstantItem(10).Text(":"); x.RelativeItem().Text(nota.NoRequiereFactura ? "No requerida" : "Requerida"); });
                            });
                        });

                        if (!string.IsNullOrWhiteSpace(nota.Observaciones))
                            block.Item().PaddingTop(4).Text($"Observaciones: {nota.Observaciones}").FontSize(7.8f).FontColor(Colors.Grey.Darken2);
                    });

                    content.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1.2f);
                            columns.RelativeColumn(5.6f);
                            columns.RelativeColumn(1.6f);
                            columns.RelativeColumn(1.6f);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(CellHeader).AlignCenter().Text("Cant.");
                            header.Cell().Element(CellHeader).Text("Descripción");
                            header.Cell().Element(CellHeader).AlignRight().Text("P. Unit.");
                            header.Cell().Element(CellHeader).AlignRight().Text("Importe");
                        });

                        foreach (var detalle in nota.Detalles.OrderBy(d => d.CreatedAt))
                        {
                            var resumenTallas = detalle.Tallas.Any()
                                ? string.Join(" · ", detalle.Tallas.OrderBy(t => t.Orden).ThenBy(t => t.Talla).Select(t => $"{t.Talla}: {t.Cantidad:0.##}"))
                                : null;

                            table.Cell().Element(CellBody).AlignCenter().Text($"{detalle.Cantidad:0.##}");
                            table.Cell().Element(CellBody).Column(col =>
                            {
                                col.Item().Text(detalle.Descripcion);
                                if (!string.IsNullOrWhiteSpace(resumenTallas))
                                    col.Item().Text($"Detalle por talla: {resumenTallas}").FontSize(7.5f).FontColor(Colors.Grey.Darken1);
                            });
                            table.Cell().Element(CellBody).AlignRight().Text(detalle.PrecioUnitario.ToString("C2"));
                            table.Cell().Element(CellBody).AlignRight().Text(detalle.Importe.ToString("C2"));
                        }
                    });

                    content.Item().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().PaddingTop(8).Text($"Importe con letra: {importeEnLetra}")
                                .SemiBold()
                                .FontSize(7.8f)
                                .FontColor(Colors.Grey.Darken3);
                        });

                        row.ConstantItem(220).Border(1).BorderColor(Colors.Grey.Lighten1).Padding(6).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            table.Cell().Element(CellSummaryLabel).Text("Subtotal");
                            table.Cell().Element(CellSummaryValue).Text(nota.Subtotal.ToString("C2"));
                            table.Cell().Element(CellSummaryLabel).Text("Impuestos");
                            table.Cell().Element(CellSummaryValue).Text(nota.Impuestos.ToString("C2"));
                            table.Cell().Element(CellSummaryLabel).Text("Total").SemiBold().FontSize(11);
                            table.Cell().Element(CellSummaryValue).Text(nota.Total.ToString("C2")).SemiBold().FontSize(11);
                        });
                    });

                });

                page.Footer().ShowOnce().Column(footer =>
                {
                    footer.Item().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(8).Column(col =>
                    {
                        col.Item().Text("PAGARÉ").SemiBold().FontSize(9.2f).FontColor(Colors.Grey.Darken3);
                        col.Item().PaddingTop(3).Text(pagareTexto)
                            .FontSize(6.85f)
                            .FontColor(Colors.Grey.Darken2);
                        col.Item().PaddingTop(3).Text($"Importe: {nota.Total:C2}   |   Vencimiento: {fechaVencimiento}   |   Lugar de pago: {lugarPago}")
                            .FontSize(6.85f)
                            .SemiBold()
                            .FontColor(Colors.Grey.Darken2);
                        col.Item().PaddingTop(8).Row(line =>
                        {
                            line.RelativeItem().PaddingRight(10).Column(sign =>
                            {
                                sign.Item().PaddingBottom(2).LineHorizontal(1);
                                sign.Item().AlignCenter().Text("Nombre y firma del suscriptor / cliente").FontSize(7f);
                            });
                            line.RelativeItem().PaddingLeft(10).Column(sign =>
                            {
                                sign.Item().PaddingBottom(2).LineHorizontal(1);
                                sign.Item().AlignCenter().Text("Lugar y fecha de suscripción").FontSize(7f);
                            });
                        });
                    });

                    footer.Item().PaddingTop(5).Row(row =>
                    {
                        row.RelativeItem().Text($"Powered by Zenith · Arzmec · {DateTime.Now:dd/MM/yyyy HH:mm}")
                            .FontSize(6.6f)
                            .FontColor(Colors.Grey.Darken1);
                        row.ConstantItem(120).AlignRight().Text("Nota comercial")
                            .FontSize(6.6f)
                            .FontColor(Colors.Grey.Darken1);
                    });
                });
            });
        }).GeneratePdf();

        await using var stream = new MemoryStream(pdfBytes);
        var pdfUrl = await _fileStorage.SaveAsync(
            new TenantFileStorageRequest
            {
                EmpresaId = nota.EmpresaId,
                Area = "adjuntos",
                Modulo = "contabilidad",
                Entidad = "notas-entrega",
                ClienteId = nota.ClienteId,
                ClienteNombre = nota.Cliente.RazonSocial,
                DocumentoId = nota.Id,
                DocumentoFolio = nota.NumeroNota,
                FileNamePrefix = $"nota-{nota.NumeroNota}",
                Extension = ".pdf"
            },
            stream,
            nota.PdfUrl,
            cancellationToken);

        nota.PdfUrl = pdfUrl;
        nota.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        return pdfUrl;
    }

    private static IContainer CellHeader(IContainer container)
        => container.Background(Colors.BlueGrey.Lighten4).BorderBottom(1).BorderColor(Colors.Grey.Lighten1).PaddingVertical(6).PaddingHorizontal(5);

    private static IContainer CellBody(IContainer container)
        => container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).PaddingHorizontal(4);

    private static IContainer CellSummaryLabel(IContainer container)
        => container.PaddingVertical(3).AlignLeft();

    private static IContainer CellSummaryValue(IContainer container)
        => container.PaddingVertical(3).AlignRight();

    private static string FormatearDireccion(Cliente cliente)
    {
        var partes = new[] { cliente.Direccion, cliente.Estado, cliente.CodigoPostal, cliente.Pais }
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x!.Trim());

        return string.Join(", ", partes).Trim().Length == 0 ? "—" : string.Join(", ", partes);
    }

    private static string ValorOpcion(string? valor)
        => string.IsNullOrWhiteSpace(valor) ? "—" : valor.Trim();

    private static string ConvertirImporteALetras(decimal total)
    {
        var entero = (int)Math.Floor(total);
        var centavos = (int)Math.Round((total - entero) * 100, MidpointRounding.AwayFromZero);
        return $"{NumeroALetras(entero)} pesos {centavos:00}/100 M.N.".ToUpperInvariant();
    }

    private static string ObtenerFechaVencimiento(NotaEntrega nota)
    {
        if (nota.Pedido.TipoPrecio == TipoPrecioEnum.Credito && nota.Cliente.DiasCredito.GetValueOrDefault() > 0)
            return nota.FechaNota.AddDays(nota.Cliente.DiasCredito!.Value).ToString("dd/MM/yyyy");

        return "A LA VISTA";
    }

    private static string ObtenerLugarPago(NotaEntrega nota)
        => string.IsNullOrWhiteSpace(nota.Cliente.Ciudad) ? "________________" : nota.Cliente.Ciudad.Trim().ToUpperInvariant();

    private static string ConstruirTextoPagare(NotaEntrega nota, string acreedor, string importeEnLetra, string fechaVencimiento, string lugarPago)
    {
        var cliente = ValorOpcion(nota.Cliente.RazonSocial).ToUpperInvariant();
        var vencimiento = fechaVencimiento == "A LA VISTA" ? "A LA VISTA" : $"EL DÍA {fechaVencimiento}";

        return $"Debo y pagaré incondicionalmente a la orden de {acreedor.ToUpperInvariant()}, en {lugarPago}, {vencimiento}, la cantidad de {importeEnLetra} ({nota.Total:C2}), valor recibido a mi entera satisfacción según nota {nota.NumeroNota} y pedido {nota.Pedido.NumeroPedido}. En caso de mora, acepto cubrir los accesorios y gastos de cobranza que legalmente procedan conforme a la legislación mercantil mexicana aplicable.";
    }

    private async Task<byte[]?> CargarLogoEmpresaAsync(CrmDbContext db, Guid empresaId, CancellationToken cancellationToken)
    {
        var logoPath = await db.AppConfigs
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(c => c.EmpresaId == empresaId && c.Clave == "CompanyLogo")
            .Select(c => c.Valor)
            .FirstOrDefaultAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(logoPath))
            return null;

        if (logoPath.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase))
        {
            var physicalPath = Path.Combine(_env.WebRootPath, logoPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            return File.Exists(physicalPath) ? await File.ReadAllBytesAsync(physicalPath, cancellationToken) : null;
        }

        if (TryParseTenantFilePath(logoPath, out var logoEmpresaId, out var storagePath))
        {
            var file = await _fileStorage.OpenReadAsync(logoEmpresaId, storagePath, cancellationToken);
            if (file == null)
                return null;

            await using var stream = file.Content;
            using var memory = new MemoryStream();
            await stream.CopyToAsync(memory, cancellationToken);
            return memory.ToArray();
        }

        return null;
    }

    private static bool TryParseTenantFilePath(string? publicPath, out Guid empresaId, out string storagePath)
    {
        empresaId = Guid.Empty;
        storagePath = string.Empty;

        if (string.IsNullOrWhiteSpace(publicPath) || !publicPath.StartsWith("/tenant-files/", StringComparison.OrdinalIgnoreCase))
            return false;

        var segments = publicPath.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length < 3 || !string.Equals(segments[0], "tenant-files", StringComparison.OrdinalIgnoreCase))
            return false;

        if (!Guid.TryParseExact(segments[1], "N", out empresaId))
            return false;

        storagePath = string.Join('/', segments.Skip(2));
        return !string.IsNullOrWhiteSpace(storagePath);
    }

    private static string ObtenerInicialesEmpresa(string nombreEmpresa)
    {
        var partes = nombreEmpresa
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Take(2)
            .Select(x => char.ToUpperInvariant(x[0]));

        return string.Concat(partes);
    }

    private static string NumeroALetras(int numero)
    {
        if (numero == 0) return "cero";
        if (numero < 0) return $"menos {NumeroALetras(Math.Abs(numero))}";

        return (numero switch
        {
            <= 15 => Unidades(numero),
            < 20 => $"dieci{Unidades(numero - 10)}",
            20 => "veinte",
            < 30 => $"veinti{Unidades(numero - 20)}",
            < 100 => Decenas(numero),
            100 => "cien",
            < 1000 => Centenas(numero),
            < 2000 => $"mil {NumeroALetras(numero % 1000)}".Trim(),
            < 1000000 => $"{NumeroALetras(numero / 1000)} mil {NumeroALetras(numero % 1000)}".Trim(),
            < 2000000 => $"un millón {NumeroALetras(numero % 1000000)}".Trim(),
            _ => $"{NumeroALetras(numero / 1000000)} millones {NumeroALetras(numero % 1000000)}".Trim()
        }).Replace("  ", " ").Trim();
    }

    private static string Decenas(int numero)
    {
        var decenas = new[] { "", "", "veinte", "treinta", "cuarenta", "cincuenta", "sesenta", "setenta", "ochenta", "noventa" };
        var decena = numero / 10;
        var unidad = numero % 10;
        return unidad == 0 ? decenas[decena] : $"{decenas[decena]} y {Unidades(unidad)}";
    }

    private static string Centenas(int numero)
    {
        var centenas = new[] { "", "ciento", "doscientos", "trescientos", "cuatrocientos", "quinientos", "seiscientos", "setecientos", "ochocientos", "novecientos" };
        var centena = numero / 100;
        var resto = numero % 100;
        return resto == 0 ? centenas[centena] : $"{centenas[centena]} {NumeroALetras(resto)}";
    }

    private static string Unidades(int numero)
        => numero switch
        {
            1 => "uno",
            2 => "dos",
            3 => "tres",
            4 => "cuatro",
            5 => "cinco",
            6 => "seis",
            7 => "siete",
            8 => "ocho",
            9 => "nueve",
            10 => "diez",
            11 => "once",
            12 => "doce",
            13 => "trece",
            14 => "catorce",
            15 => "quince",
            _ => string.Empty
        };
}
