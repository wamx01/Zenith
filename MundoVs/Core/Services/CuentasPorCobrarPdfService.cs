using MundoVs.Core.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace MundoVs.Core.Services;

public sealed class CuentasPorCobrarPdfService : ICuentasPorCobrarPdfService
{
    public Task<byte[]> GenerateReportePdfAsync(CuentasPorCobrarPdfReport report, CancellationToken cancellationToken = default)
    {
        var pdf = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter.Landscape());
                page.Margin(20);
                page.DefaultTextStyle(x => x.FontFamily(Fonts.Arial).FontSize(9));
                page.Content().Element(content => Compose(content, report));
            });
        }).GeneratePdf();

        return Task.FromResult(pdf);
    }

    private static void Compose(IContainer container, CuentasPorCobrarPdfReport report)
    {
        container.Column(column =>
        {
            column.Spacing(12);

            column.Item().Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("Reporte profesional de cuentas por cobrar").Bold().FontSize(18).FontColor("#0f172a");
                    col.Item().Text(report.Empresa).SemiBold().FontSize(11);
                    col.Item().Text($"Período: {report.Desde:dd/MM/yyyy} al {report.Hasta:dd/MM/yyyy}").FontSize(9);
                    col.Item().Text($"Cliente: {report.ClienteFiltro}").FontSize(9);
                });

                row.ConstantItem(180).Background("#0f172a").Padding(12).Column(col =>
                {
                    col.Item().Text("Saldo pendiente").FontColor(Colors.White).FontSize(9);
                    col.Item().Text(report.TotalPendiente.ToString("C2")).Bold().FontColor(Colors.White).FontSize(16);
                    col.Item().PaddingTop(4).Text($"Vencido: {report.MontoVencido:C2}").FontColor(Colors.White).FontSize(9);
                    col.Item().Text($"Clientes con adeudo: {report.ClientesConAdeudo}").FontColor(Colors.White).FontSize(9);
                });
            });

            column.Item().Element(c => ComposeMetricGrid(c, report));
            column.Item().Element(c => ComposeChart(c, report));
            column.Item().Element(c => ComposeAntiguedadTable(c, report.AntiguedadClientes));

            if (report.DocumentosVencidos.Count > 0)
                column.Item().Element(c => ComposeVencidasTable(c, report.DocumentosVencidos));

            column.Item().Element(c => ComposeEstadoCuenta(c, report.EstadoCuenta));

            if (report.Movimientos.Count > 0)
                column.Item().Element(c => ComposeMovimientos(c, report.Movimientos));
        });
    }

    private static void ComposeMetricGrid(IContainer container, CuentasPorCobrarPdfReport report)
    {
        var metrics = new (string Title, decimal Value, string Hint)[]
        {
            ("Por cobrar", report.TotalPendiente, "Saldo actual"),
            ("Cobrado", report.TotalCobrado, "Cobranza acumulada"),
            ("Vencido", report.MontoVencido, "Cartera vencida"),
            ("0-30", report.Bucket0a30, "Corriente"),
            ("31-60", report.Bucket31a60, "Seguimiento"),
            ("61-90", report.Bucket61a90, "Riesgo medio"),
            ("+90", report.BucketMas90, "Riesgo alto"),
            ("Clientes", report.ClientesConAdeudo, "Con saldo")
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

    private static void ComposeChart(IContainer container, CuentasPorCobrarPdfReport report)
    {
        var values = new[]
        {
            ("0-30", report.Bucket0a30, "#10b981"),
            ("31-60", report.Bucket31a60, "#f59e0b"),
            ("61-90", report.Bucket61a90, "#f97316"),
            ("+90", report.BucketMas90, "#ef4444")
        };
        var max = values.Max(x => x.Item2);
        if (max <= 0) max = 1;

        container.Border(1).BorderColor("#dbe3ee").Padding(10).Column(col =>
        {
            col.Item().Text("Gráfica de antigüedad de saldos").Bold().FontSize(11).FontColor("#0f172a");
            col.Item().PaddingTop(6).Column(chart =>
            {
                chart.Spacing(6);
                foreach (var item in values)
                {
                    var width = (float)(item.Item2 / max * 220m);
                    chart.Item().Row(row =>
                    {
                        row.ConstantItem(45).Text(item.Item1).FontSize(8);
                        row.ConstantItem(230).Height(14).Background("#e2e8f0").Layers(layers =>
                        {
                            layers.Layer().Background("#e2e8f0");
                            layers.PrimaryLayer().Width(width).Background(item.Item3);
                        });
                        row.ConstantItem(90).AlignRight().Text(item.Item2.ToString("C2")).SemiBold().FontSize(8);
                    });
                }
            });
        });
    }

    private static void ComposeAntiguedadTable(IContainer container, IReadOnlyList<CuentasPorCobrarAntiguedadPdfItem> items)
    {
        container.Column(col =>
        {
            col.Item().Text("Antigüedad por cliente").Bold().FontSize(11).FontColor("#0f172a");
            col.Item().PaddingTop(4).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(1.8f);
                    columns.ConstantColumn(78);
                    columns.ConstantColumn(78);
                    columns.ConstantColumn(78);
                    columns.ConstantColumn(78);
                    columns.ConstantColumn(82);
                });

                foreach (var header in new[] { "Cliente", "0-30", "31-60", "61-90", "+90", "Total" })
                    table.Cell().Background("#0f172a").Padding(4).Text(header).FontColor(Colors.White).FontSize(7.5f).Bold();

                foreach (var item in items)
                {
                    table.Cell().Padding(4).Text(item.Cliente).FontSize(8f);
                    table.Cell().Padding(4).AlignRight().Text(item.Rango0a30.ToString("C0")).FontSize(8f);
                    table.Cell().Padding(4).AlignRight().Text(item.Rango31a60.ToString("C0")).FontSize(8f);
                    table.Cell().Padding(4).AlignRight().Text(item.Rango61a90.ToString("C0")).FontSize(8f);
                    table.Cell().Padding(4).AlignRight().Text(item.RangoMas90.ToString("C0")).FontSize(8f).FontColor(item.RangoMas90 > 0 ? "#b91c1c" : "#0f172a");
                    table.Cell().Padding(4).AlignRight().Text(item.Total.ToString("C0")).FontSize(8f).SemiBold();
                }
            });
        });
    }

    private static void ComposeVencidasTable(IContainer container, IReadOnlyList<CuentasPorCobrarVencidaPdfItem> items)
    {
        container.Column(col =>
        {
            col.Item().Text("Documentos vencidos").Bold().FontSize(11).FontColor("#0f172a");
            col.Item().PaddingTop(4).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(1.6f);
                    columns.RelativeColumn(1.2f);
                    columns.ConstantColumn(70);
                    columns.ConstantColumn(70);
                    columns.ConstantColumn(45);
                    columns.ConstantColumn(80);
                });

                foreach (var header in new[] { "Cliente", "Documento", "Emisión", "Vencim.", "Días", "Saldo" })
                    table.Cell().Background("#334155").Padding(4).Text(header).FontColor(Colors.White).FontSize(7.5f).Bold();

                foreach (var item in items.Take(25))
                {
                    table.Cell().Padding(4).Text(item.Cliente).FontSize(8f);
                    table.Cell().Padding(4).Text(item.Documento).FontSize(8f);
                    table.Cell().Padding(4).Text(item.FechaEmision.ToString("dd/MM/yyyy")).FontSize(8f);
                    table.Cell().Padding(4).Text(item.FechaVencimiento?.ToString("dd/MM/yyyy") ?? "—").FontSize(8f);
                    table.Cell().Padding(4).AlignRight().Text(item.DiasAtraso.ToString()).FontSize(8f).FontColor("#b91c1c");
                    table.Cell().Padding(4).AlignRight().Text(item.Saldo.ToString("C2")).FontSize(8f).SemiBold().FontColor("#b91c1c");
                }
            });
        });
    }

    private static void ComposeEstadoCuenta(IContainer container, CuentasPorCobrarEstadoCuentaPdf estado)
    {
        container.Column(col =>
        {
            col.Item().Text($"Estado de cuenta · {estado.Cliente}").Bold().FontSize(11).FontColor("#0f172a");
            col.Item().PaddingTop(2).Text($"Saldo inicial: {estado.SaldoInicial:C2} · Saldo final: {estado.SaldoFinal:C2}").FontSize(8.5f).FontColor("#475569");
            col.Item().PaddingTop(4).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(70);
                    columns.ConstantColumn(58);
                    columns.RelativeColumn(1.3f);
                    columns.RelativeColumn(1.6f);
                    columns.ConstantColumn(75);
                    columns.ConstantColumn(75);
                    columns.ConstantColumn(80);
                });

                foreach (var header in new[] { "Fecha", "Tipo", "Referencia", "Concepto", "Cargo", "Abono", "Saldo" })
                    table.Cell().Background("#0f172a").Padding(4).Text(header).FontColor(Colors.White).FontSize(7.5f).Bold();

                foreach (var mov in estado.Movimientos.Take(40))
                {
                    table.Cell().Padding(4).Text(mov.Fecha.ToString("dd/MM/yyyy")).FontSize(8f);
                    table.Cell().Padding(4).Text(mov.Tipo).FontSize(8f);
                    table.Cell().Padding(4).Text(mov.Referencia).FontSize(8f);
                    table.Cell().Padding(4).Text(mov.Concepto).FontSize(8f);
                    table.Cell().Padding(4).AlignRight().Text(mov.Cargo == 0 ? "—" : mov.Cargo.ToString("C2")).FontSize(8f);
                    table.Cell().Padding(4).AlignRight().Text(mov.Abono == 0 ? "—" : mov.Abono.ToString("C2")).FontSize(8f).FontColor(mov.Abono > 0 ? "#15803d" : "#0f172a");
                    table.Cell().Padding(4).AlignRight().Text(mov.Saldo.ToString("C2")).FontSize(8f).SemiBold();
                }
            });
        });
    }

    private static void ComposeMovimientos(IContainer container, IReadOnlyList<CuentasPorCobrarMovimientoPdfItem> movimientos)
    {
        container.Column(col =>
        {
            col.Item().Text("Movimientos de cuentas por cobrar").Bold().FontSize(11).FontColor("#0f172a");
            col.Item().PaddingTop(4).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(62);
                    columns.RelativeColumn(1.2f);
                    columns.ConstantColumn(58);
                    columns.RelativeColumn(1.1f);
                    columns.RelativeColumn(1.5f);
                    columns.ConstantColumn(72);
                    columns.ConstantColumn(72);
                    columns.ConstantColumn(72);
                });

                foreach (var header in new[] { "Fecha", "Cliente", "Tipo", "Referencia", "Concepto", "Cargo", "Cobro", "Saldo" })
                    table.Cell().Background("#334155").Padding(4).Text(header).FontColor(Colors.White).FontSize(7.5f).Bold();

                foreach (var mov in movimientos.Take(50))
                {
                    table.Cell().Padding(4).Text(mov.Fecha.ToString("dd/MM/yyyy")).FontSize(8f);
                    table.Cell().Padding(4).Text(mov.Cliente).FontSize(8f);
                    table.Cell().Padding(4).Text(mov.Tipo).FontSize(8f);
                    table.Cell().Padding(4).Text(mov.Referencia).FontSize(8f);
                    table.Cell().Padding(4).Text(mov.Concepto).FontSize(8f);
                    table.Cell().Padding(4).AlignRight().Text(mov.Cargo == 0 ? "—" : mov.Cargo.ToString("C2")).FontSize(8f);
                    table.Cell().Padding(4).AlignRight().Text(mov.Abono == 0 ? "—" : mov.Abono.ToString("C2")).FontSize(8f).FontColor(mov.Abono > 0 ? "#15803d" : "#0f172a");
                    table.Cell().Padding(4).AlignRight().Text(mov.Saldo.ToString("C2")).FontSize(8f).SemiBold();
                }
            });
        });
    }
}
