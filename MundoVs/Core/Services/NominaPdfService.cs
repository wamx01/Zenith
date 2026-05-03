using Microsoft.EntityFrameworkCore;
using MundoVs.Core.Entities;
using MundoVs.Core.Interfaces;
using MundoVs.Infrastructure.Data;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace MundoVs.Core.Services;

public sealed class NominaPdfService : INominaPdfService
{
    private readonly IDbContextFactory<CrmDbContext> _dbFactory;
    private readonly INominaReciboBuilder _nominaReciboBuilder;

    public NominaPdfService(IDbContextFactory<CrmDbContext> dbFactory, INominaReciboBuilder nominaReciboBuilder)
    {
        _dbFactory = dbFactory;
        _nominaReciboBuilder = nominaReciboBuilder;
    }

    public async Task<byte[]> GenerateReciboPdfAsync(Guid nominaDetalleId, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var detalle = await CargarDetalleAsync(db, d => d.Id == nominaDetalleId, cancellationToken);
        if (detalle == null)
            throw new InvalidOperationException("No se encontró el recibo de nómina.");

        return GeneratePdf([detalle]);
    }

    public async Task<byte[]> GenerateRecibosPdfAsync(Guid nominaId, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var detalles = await CargarDetallesAsync(db, d => d.NominaId == nominaId, cancellationToken);
        if (detalles.Count == 0)
            throw new InvalidOperationException("No hay recibos de nómina para generar PDF.");

        return GeneratePdf(detalles);
    }

    public Task<byte[]> GenerateDashboardCostosPdfAsync(NominaDashboardCostosReport report, CancellationToken cancellationToken = default)
    {
        var pdf = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter.Landscape());
                page.Margin(20);
                page.DefaultTextStyle(x => x.FontFamily(Fonts.Arial).FontSize(9));
                page.Content().Element(content => ComposeDashboardCostos(content, report));
            });
        }).GeneratePdf();

        return Task.FromResult(pdf);
    }

    private byte[] GeneratePdf(IReadOnlyList<NominaDetalle> detalles)
    {
        var recibos = detalles.ToDictionary(d => d.Id, _nominaReciboBuilder.Build);

        return Document.Create(container =>
        {
            foreach (var detalle in detalles)
            {
                var recibo = recibos[detalle.Id];
                container.Page(page =>
                {
                    page.Size(PageSizes.Letter);
                    page.Margin(20);
                    page.DefaultTextStyle(x => x.FontFamily(Fonts.Arial).FontSize(9));
                    page.Content().Element(content => ComposeRecibo(content, detalle, recibo));
                });
            }
        }).GeneratePdf();
    }

    private static void ComposeDashboardCostos(IContainer container, NominaDashboardCostosReport report)
    {
        container.Column(column =>
        {
            column.Spacing(12);

            column.Item().Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("Reporte ejecutivo de costos de nómina").Bold().FontSize(18).FontColor("#0f172a");
                    col.Item().Text(report.Empresa).SemiBold().FontSize(11);
                    col.Item().Text($"Período: {report.Desde:dd/MM/yyyy} al {report.Hasta:dd/MM/yyyy}").FontSize(9);
                    col.Item().Text($"Periodicidad: {report.Periodicidad} · Empleado: {report.EmpleadoFiltro}").FontSize(9);
                });

                row.ConstantItem(170).Background("#0f172a").Padding(12).Column(col =>
                {
                    col.Item().Text("Costo empresa").FontColor(Colors.White).FontSize(9);
                    col.Item().Text(report.CostoEmpresa.ToString("C2")).Bold().FontColor(Colors.White).FontSize(16);
                    col.Item().PaddingTop(6).Text($"Neto pagado: {report.NetoPagado:C2}").FontColor(Colors.White).FontSize(9);
                });
            });

            column.Item().Element(c => ComposeMetricGrid(c, report));

            if (report.PrestacionesAplicadas.Count > 0 || report.PercepcionesAplicadas.Count > 0 || report.DescuentosAplicados.Count > 0)
            {
                column.Item().Row(row =>
                {
                    row.RelativeItem().Element(c => ComposeConceptList(c, "Percepciones aplicadas", report.PercepcionesAplicadas));
                    row.RelativeItem().Element(c => ComposeConceptList(c, "Prestaciones y cargas", report.PrestacionesAplicadas));
                    row.RelativeItem().Element(c => ComposeConceptList(c, "Descuentos aplicados", report.DescuentosAplicados));
                });
            }

            column.Item().Element(c => ComposeDepartmentTable(c, report.CostosPorDepartamento));

            if (report.EmpleadosTop.Count > 0)
            {
                column.Item().Element(c => ComposeTopEmployees(c, report.EmpleadosTop));
            }
        });
    }

    private static void ComposeMetricGrid(IContainer container, NominaDashboardCostosReport report)
    {
        var metrics = new (string Title, decimal Value, string Hint)[]
        {
            ("Neto pagado", report.NetoPagado, "Pago al empleado"),
            ("IMSS obrero", report.ImssObrero, "Retenido al trabajador"),
            ("IMSS patronal", report.ImssPatronal, "Costo empresa"),
            ("ISR", report.Isr, "Retención acumulada"),
            ("Infonavit", report.Infonavit, "Descuento aplicado"),
            ("Provisiones", report.Provisiones, "Reserva acumulada"),
            ("Horas extra", report.HorasExtra, "Monto del período"),
            ("Bonos", report.Bonos, "Bonos aplicados")
        };

        container.Grid(grid =>
        {
            grid.Columns(4);
            grid.Spacing(8);

            foreach (var metric in metrics)
            {
                grid.Item().Border(1).BorderColor("#dbe3ee").Background("#f8fafc").Padding(8).Column(col =>
                {
                    col.Item().Text(metric.Title).FontSize(8).FontColor("#64748b");
                    col.Item().PaddingTop(2).Text(metric.Value.ToString("C2")).Bold().FontSize(12).FontColor("#0f172a");
                    col.Item().PaddingTop(2).Text(metric.Hint).FontSize(7).FontColor("#64748b");
                });
            }
        });
    }

    private static void ComposeConceptList(IContainer container, string title, IReadOnlyList<NominaDashboardConceptoItem> items)
    {
        container.Border(1).BorderColor("#dbe3ee").Padding(8).Column(col =>
        {
            col.Item().Text(title).Bold().FontSize(10).FontColor("#0f172a");
            col.Item().PaddingTop(4).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn();
                    columns.ConstantColumn(85);
                });

                foreach (var item in items)
                {
                    table.Cell().PaddingVertical(3).Text(item.Nombre).FontSize(8.5f);
                    table.Cell().PaddingVertical(3).AlignRight().Text(item.Total.ToString("C2")).SemiBold().FontSize(8.5f);
                }
            });
        });
    }

    private static void ComposeDepartmentTable(IContainer container, IReadOnlyList<NominaDashboardDepartamentoItem> items)
    {
        container.Column(col =>
        {
            col.Item().Text("Costo por departamento").Bold().FontSize(11).FontColor("#0f172a");
            col.Item().PaddingTop(4).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(1.4f);
                    columns.ConstantColumn(42);
                    columns.ConstantColumn(74);
                    columns.ConstantColumn(80);
                    columns.ConstantColumn(68);
                    columns.ConstantColumn(60);
                    columns.ConstantColumn(60);
                    columns.ConstantColumn(68);
                    columns.ConstantColumn(62);
                    columns.ConstantColumn(62);
                    columns.ConstantColumn(68);
                    columns.ConstantColumn(68);
                });

                var headers = new[] { "Departamento", "Emp.", "Neto", "Costo emp.", "IMSS", "ISR", "Info.", "Prov.", "Extra", "Bonos", "Deduc.", "Desc. min." };
                foreach (var header in headers)
                {
                    table.Cell().Background("#0f172a").Padding(4).Text(header).FontColor(Colors.White).FontSize(7.5f).Bold();
                }

                foreach (var item in items)
                {
                    table.Cell().Padding(4).Text(item.Departamento).FontSize(8f);
                    table.Cell().Padding(4).AlignRight().Text(item.Empleados.ToString()).FontSize(8f);
                    table.Cell().Padding(4).AlignRight().Text(item.Neto.ToString("C0")).FontSize(8f);
                    table.Cell().Padding(4).AlignRight().Text(item.CostoEmpresa.ToString("C0")).FontSize(8f).SemiBold();
                    table.Cell().Padding(4).AlignRight().Text(item.ImssTotal.ToString("C0")).FontSize(8f);
                    table.Cell().Padding(4).AlignRight().Text(item.Isr.ToString("C0")).FontSize(8f);
                    table.Cell().Padding(4).AlignRight().Text(item.Infonavit.ToString("C0")).FontSize(8f);
                    table.Cell().Padding(4).AlignRight().Text(item.Provisiones.ToString("C0")).FontSize(8f);
                    table.Cell().Padding(4).AlignRight().Text(item.HorasExtra.ToString("C0")).FontSize(8f);
                    table.Cell().Padding(4).AlignRight().Text(item.Bonos.ToString("C0")).FontSize(8f);
                    table.Cell().Padding(4).AlignRight().Text(item.Deducciones.ToString("C0")).FontSize(8f);
                    table.Cell().Padding(4).AlignRight().Text(item.DescuentoMinutos.ToString("C0")).FontSize(8f);
                }
            });
        });
    }

    private static void ComposeTopEmployees(IContainer container, IReadOnlyList<NominaDashboardEmpleadoItem> items)
    {
        container.Border(1).BorderColor("#dbe3ee").Padding(8).Column(col =>
        {
            col.Item().Text("Empleados con mayor costo empresa").Bold().FontSize(10).FontColor("#0f172a");
            col.Item().PaddingTop(4).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(1.5f);
                    columns.RelativeColumn();
                    columns.ConstantColumn(85);
                    columns.ConstantColumn(85);
                });

                foreach (var item in items)
                {
                    table.Cell().PaddingVertical(3).Text(item.Empleado).FontSize(8.5f);
                    table.Cell().PaddingVertical(3).Text(item.Departamento).FontSize(8.5f).FontColor("#64748b");
                    table.Cell().PaddingVertical(3).AlignRight().Text(item.Neto.ToString("C2")).FontSize(8.5f);
                    table.Cell().PaddingVertical(3).AlignRight().Text(item.CostoEmpresa.ToString("C2")).SemiBold().FontSize(8.5f);
                }
            });
        });
    }

    private static void ComposeRecibo(IContainer container, NominaDetalle detalle, NominaReciboResult recibo)
    {
        var nombreEmpresa = string.IsNullOrWhiteSpace(detalle.Nomina.Empresa.NombreComercial)
            ? detalle.Nomina.Empresa.RazonSocial
            : detalle.Nomina.Empresa.NombreComercial!;
        var iniciales = string.Join(string.Empty,
            nombreEmpresa.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(2).Select(p => char.ToUpperInvariant(p[0])));
        var puesto = !string.IsNullOrWhiteSpace(detalle.Empleado.Puesto)
            ? detalle.Empleado.Puesto
            : detalle.Empleado.Posicion?.Nombre ?? "—";
        var departamento = string.IsNullOrWhiteSpace(detalle.Empleado.Departamento) ? "—" : detalle.Empleado.Departamento;

        container.Column(column =>
        {
            column.Spacing(10);

            column.Item().Row(row =>
            {
                row.RelativeItem(1.15f).MinHeight(92).Column(col =>
                {
                    col.Item().Text(nombreEmpresa).SemiBold().FontSize(10);
                    col.Item().Text($"RFC: {(string.IsNullOrWhiteSpace(detalle.Nomina.Empresa.Rfc) ? "—" : detalle.Nomina.Empresa.Rfc)}").FontSize(8.5f);
                    col.Item().Text($"Código empresa: {(string.IsNullOrWhiteSpace(detalle.Nomina.Empresa.Codigo) ? "—" : detalle.Nomina.Empresa.Codigo)}").FontSize(8.5f);
                    col.Item().Text($"Régimen trabajador: {detalle.TipoPago}").FontSize(8.5f);
                });

                row.RelativeItem(.95f).AlignCenter().Column(col =>
                {
                    col.Item().Width(92).Height(92).Border(3).BorderColor("#0b2a68").AlignCenter().AlignMiddle().Text(iniciales).FontSize(26).SemiBold().FontColor("#0b2a68");
                    col.Item().PaddingTop(4).AlignCenter().Text(nombreEmpresa.ToUpperInvariant()).Bold().FontSize(15);
                    if (!string.IsNullOrWhiteSpace(detalle.Nomina.Empresa.Slogan))
                        col.Item().AlignCenter().Text(detalle.Nomina.Empresa.Slogan).FontSize(8).FontColor(Colors.Grey.Darken1);
                });

                row.RelativeItem(1.05f).MinHeight(92).Background("#0b2a68").Padding(12).AlignMiddle().AlignCenter().Text("RECIBO DE NÓMINA").Bold().FontSize(16).FontColor(Colors.White);
            });

            column.Item().Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text($"No. Trab.: {(string.IsNullOrWhiteSpace(detalle.Empleado.NumeroEmpleado) ? "—" : detalle.Empleado.NumeroEmpleado)}").FontSize(8.8f);
                    col.Item().Text($"Nombre: {detalle.Empleado.NombreCompleto}").FontSize(8.8f);
                    col.Item().Text($"CURP: {(string.IsNullOrWhiteSpace(detalle.Empleado.Curp) ? "—" : detalle.Empleado.Curp)}").FontSize(8.8f);
                    col.Item().Text($"RFC: {(string.IsNullOrWhiteSpace(detalle.Nomina.Empresa.Rfc) ? "—" : detalle.Nomina.Empresa.Rfc)}").FontSize(8.8f);
                    col.Item().Text($"R. IMSS: {(string.IsNullOrWhiteSpace(detalle.Empleado.Nss) ? "—" : detalle.Empleado.Nss)}").FontSize(8.8f);
                });

                row.RelativeItem().Column(col =>
                {
                    col.Item().Text($"Depto.: {departamento}").FontSize(8.8f);
                    col.Item().Text($"Puesto: {puesto}").FontSize(8.8f);
                    col.Item().Text($"No. Nómina: {(string.IsNullOrWhiteSpace(detalle.Nomina.Folio) ? "—" : detalle.Nomina.Folio)}").FontSize(8.8f);
                    col.Item().Text($"Período: {detalle.Nomina.FechaInicio:dd/MM/yyyy} al {detalle.Nomina.FechaFin:dd/MM/yyyy}").FontSize(8.8f);
                    col.Item().Text($"Días trabajados: {detalle.DiasTrabajados}").FontSize(8.8f);
                    col.Item().Text($"Faltas: {detalle.DiasFaltaInjustificada}").FontSize(8.8f);
                });
            });

            column.Item().Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Background("#0b2a68").PaddingVertical(4).AlignCenter().Text("PERCEPCIONES").Bold().FontSize(10).FontColor(Colors.White);
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(46);
                            columns.RelativeColumn();
                            columns.ConstantColumn(100);
                        });

                        for (var i = 0; i < recibo.Percepciones.Count; i++)
                        {
                            var concepto = recibo.Percepciones[i];
                            table.Cell().PaddingVertical(2).Text(concepto.Codigo).SemiBold().FontColor("#0b2a68");
                            table.Cell().PaddingVertical(2).Text(concepto.Concepto);
                            table.Cell().PaddingVertical(2).AlignRight().Text(concepto.Importe.ToString("C2")).SemiBold();
                        }
                    });
                });

                row.ConstantItem(18);

                row.RelativeItem().Column(col =>
                {
                    col.Item().Background("#0b2a68").PaddingVertical(4).AlignCenter().Text("DEDUCCIONES").Bold().FontSize(10).FontColor(Colors.White);
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(46);
                            columns.RelativeColumn();
                            columns.ConstantColumn(100);
                        });

                        for (var i = 0; i < recibo.Deducciones.Count; i++)
                        {
                            var concepto = recibo.Deducciones[i];
                            table.Cell().PaddingVertical(2).Text(concepto.Codigo).SemiBold().FontColor("#0b2a68");
                            table.Cell().PaddingVertical(2).Text(concepto.Concepto);
                            table.Cell().PaddingVertical(2).AlignRight().Text(concepto.Importe.ToString("C2")).SemiBold();
                        }
                    });
                });
            });

            column.Item().PaddingTop(8).LineHorizontal(1).LineColor("#a5b4fc");

            column.Item().Row(row =>
            {
                row.RelativeItem(1.15f).Column(col =>
                {
                    col.Item().Row(r => { r.RelativeItem().Text("Total Percepciones"); r.ConstantItem(120).AlignRight().Text(recibo.TotalPercepciones.ToString("C2")).SemiBold(); });
                    col.Item().Row(r => { r.RelativeItem().Text("Total Deducciones"); r.ConstantItem(120).AlignRight().Text(recibo.TotalDeducciones.ToString("C2")).SemiBold(); });
                    col.Item().Row(r => { r.RelativeItem().Text("Neto Pagado"); r.ConstantItem(120).AlignRight().Text(recibo.NetoPagar.ToString("C2")).SemiBold(); });
                    col.Item().Row(r => { r.RelativeItem().Text("Total en Efectivo"); r.ConstantItem(120).AlignRight().Text(recibo.NetoPagar.ToString("C2")).SemiBold(); });
                });

                row.RelativeItem(.85f).PaddingLeft(20).PaddingTop(28).Column(col =>
                {
                    col.Item().Text("FIRMA:").SemiBold();
                    col.Item().PaddingTop(12).LineHorizontal(1);
                });
            });

            column.Item().PaddingTop(10).Column(col =>
            {
                col.Item().Background("#0b2a68").PaddingVertical(4).PaddingHorizontal(8).Text("Comprobante interno de nómina").Bold().FontSize(9).FontColor(Colors.White);
                col.Item().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(8).Row(row =>
                {
                    row.RelativeItem().Column(left =>
                    {
                        left.Item().Text($"Folio interno: {(string.IsNullOrWhiteSpace(detalle.Nomina.Folio) ? detalle.Id.ToString("N")[..8].ToUpperInvariant() : detalle.Nomina.Folio)}").FontSize(8.2f);
                        left.Item().Text($"Fecha y hora de emisión: {DateTime.Now:dd/MM/yyyy HH:mm:ss}").FontSize(8.2f);
                        left.Item().Text($"Serie y folio interno: NOMINA {detalle.Nomina.Periodo}").FontSize(8.2f);
                        left.Item().Text("Claves de percepción/deducción conforme a catálogo SAT configurado.").FontSize(8.2f);
                        left.Item().Text($"Total percepciones: {recibo.TotalPercepciones:C2}").FontSize(8.2f);
                        left.Item().Text($"Total deducciones: {recibo.TotalDeducciones:C2}").FontSize(8.2f);
                        left.Item().Text($"Total: {recibo.NetoPagar:C2}").FontSize(8.2f).SemiBold();
                    });

                    row.ConstantItem(110).AlignCenter().AlignMiddle().Column(qr =>
                    {
                        qr.Item().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(6).Width(92).Height(92).AlignCenter().AlignMiddle().Text("QR").SemiBold().FontSize(16).FontColor(Colors.Grey.Darken1);
                        qr.Item().PaddingTop(4).AlignCenter().Text("Comprobante interno").FontSize(7.2f).FontColor(Colors.Grey.Darken1);
                    });
                });
            });

            if (!string.IsNullOrWhiteSpace(detalle.Notas))
                column.Item().PaddingTop(8).Text($"Observaciones: {detalle.Notas}").FontSize(8).FontColor(Colors.Grey.Darken2);
        });
    }

    private static Task<NominaDetalle?> CargarDetalleAsync(CrmDbContext db, System.Linq.Expressions.Expression<Func<NominaDetalle, bool>> predicate, CancellationToken cancellationToken)
        => db.NominaDetalles
            .AsNoTracking()
            .Include(d => d.Empleado)
                .ThenInclude(e => e.Posicion)
            .Include(d => d.BonosEstructurados.Where(b => b.IsActive))
                .ThenInclude(b => b.Detalles.Where(x => x.IsActive))
                    .ThenInclude(x => x.BonoRubroRrhh)
            .Include(d => d.PercepcionesManuales.Where(p => p.IsActive))
                .ThenInclude(p => p.TipoPercepcion)
            .Include(d => d.DeduccionesEstructuradas.Where(x => x.IsActive))
                .ThenInclude(x => x.TipoDeduccion)
            .Include(d => d.Nomina)
                .ThenInclude(n => n.Empresa)
            .FirstOrDefaultAsync(predicate, cancellationToken);

    private static Task<List<NominaDetalle>> CargarDetallesAsync(CrmDbContext db, System.Linq.Expressions.Expression<Func<NominaDetalle, bool>> predicate, CancellationToken cancellationToken)
        => db.NominaDetalles
            .AsNoTracking()
            .Include(d => d.Empleado)
                .ThenInclude(e => e.Posicion)
            .Include(d => d.BonosEstructurados.Where(b => b.IsActive))
                .ThenInclude(b => b.Detalles.Where(x => x.IsActive))
                    .ThenInclude(x => x.BonoRubroRrhh)
            .Include(d => d.PercepcionesManuales.Where(p => p.IsActive))
                .ThenInclude(p => p.TipoPercepcion)
            .Include(d => d.DeduccionesEstructuradas.Where(x => x.IsActive))
                .ThenInclude(x => x.TipoDeduccion)
            .Include(d => d.Nomina)
                .ThenInclude(n => n.Empresa)
            .Where(predicate)
            .OrderBy(d => d.Empleado.Nombre)
            .ThenBy(d => d.Empleado.ApellidoPaterno)
            .ToListAsync(cancellationToken);
}
