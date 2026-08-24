using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class FusionPrenominaNomina_Additive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CerradaCapturaPor",
                table: "nominas",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaCierreCaptura",
                table: "nominas",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SnapshotConfiguracionJson",
                table: "nominas",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "ComplementoSalarioMinimoSugerido",
                table: "nominadetalles",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "DiasFestivoTrabajadoFija",
                table: "nominadetalles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DiasPorHorasTrabajados",
                table: "nominadetalles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "DiasVacacionesDisponibles",
                table: "nominadetalles",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DiasVacacionesRestantes",
                table: "nominadetalles",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "HorasBancoAcumuladas",
                table: "nominadetalles",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "HorasBancoConsumidas",
                table: "nominadetalles",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "HorasBancoSaldoActual",
                table: "nominadetalles",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "HorasDescansoNoPagado",
                table: "nominadetalles",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "HorasDescansoPagado",
                table: "nominadetalles",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "HorasDescansoTomado",
                table: "nominadetalles",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "MinutosPorHorasFestivoNetos",
                table: "nominadetalles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MinutosPorHorasNetos",
                table: "nominadetalles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "MontoDestajoInformativo",
                table: "nominadetalles",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            // Backfill — copia el snapshot congelado y los campos de asistencia delta desde
            // Prenomina/PrenominaDetalle a Nomina/NominaDetalle para las nóminas ya vinculadas
            // (PrenominaId no null). Así las nóminas existentes quedan autosuficientes y la
            // ruta nueva puede leer de NominaDetalle sin perder el histórico congelado.
            // English: backfill — copies the frozen snapshot and the delta attendance fields
            // from Prenomina/PrenominaDetalle to Nomina/NominaDetalle for already-linked
            // payrolls (PrenominaId not null). Existing payrolls become self-sufficient and
            // the new path can read NominaDetalle without losing the frozen history.

            // 1) Cabecera: snapshot de config + sello de cierre de captura (sólo si la
            //    prenómina estaba Cerrada(2) o Aplicada(3) con FechaCierre). HorasBancoSaldoActual
            //    no tiene contraparte en PrenominaDetalle (saldo vivo) → queda en 0 (default).
            migrationBuilder.Sql(@"
UPDATE nominas n
JOIN prenominas p ON p.Id = n.PrenominaId
SET n.SnapshotConfiguracionJson = p.SnapshotConfiguracionJson,
    n.FechaCierreCaptura = CASE WHEN p.Estatus IN (2, 3) AND p.FechaCierre IS NOT NULL THEN p.FechaCierre ELSE NULL END,
    n.CerradaCapturaPor   = CASE WHEN p.Estatus IN (2, 3) AND p.FechaCierre IS NOT NULL THEN p.CerradaPor   ELSE NULL END
WHERE n.PrenominaId IS NOT NULL;
");

            // 2) Detalle: los 13 campos delta de asistencia (DiasPorHorasTrabajados,
            //    MinutosPorHorasNetos, MinutosPorHorasFestivoNetos, DiasFestivoTrabajadoFija,
            //    HorasBanco*×2, HorasDescanso*×3, MontoDestajoInformativo, DiasVacaciones*×2,
            //    ComplementoSalarioMinimoSugerido) emparejando por PrenominaId + EmpleadoId.
            migrationBuilder.Sql(@"
UPDATE nominadetalles nd
JOIN nominas n            ON n.Id = nd.NominaId
JOIN prenominadetalles pd ON pd.PrenominaId = n.PrenominaId AND pd.EmpleadoId = nd.EmpleadoId
SET nd.DiasPorHorasTrabajados       = pd.DiasPorHorasTrabajados,
    nd.MinutosPorHorasNetos         = pd.MinutosPorHorasNetos,
    nd.MinutosPorHorasFestivoNetos  = pd.MinutosPorHorasFestivoNetos,
    nd.DiasFestivoTrabajadoFija     = pd.DiasFestivoTrabajadoFija,
    nd.HorasBancoAcumuladas         = pd.HorasBancoAcumuladas,
    nd.HorasBancoConsumidas         = pd.HorasBancoConsumidas,
    nd.HorasDescansoTomado          = pd.HorasDescansoTomado,
    nd.HorasDescansoPagado          = pd.HorasDescansoPagado,
    nd.HorasDescansoNoPagado        = pd.HorasDescansoNoPagado,
    nd.MontoDestajoInformativo      = pd.MontoDestajoInformativo,
    nd.DiasVacacionesDisponibles    = pd.DiasVacacionesDisponibles,
    nd.DiasVacacionesRestantes      = pd.DiasVacacionesRestantes,
    nd.ComplementoSalarioMinimoSugerido = pd.ComplementoSalarioMinimoSugerido
WHERE n.PrenominaId IS NOT NULL;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CerradaCapturaPor",
                table: "nominas");

            migrationBuilder.DropColumn(
                name: "FechaCierreCaptura",
                table: "nominas");

            migrationBuilder.DropColumn(
                name: "SnapshotConfiguracionJson",
                table: "nominas");

            migrationBuilder.DropColumn(
                name: "ComplementoSalarioMinimoSugerido",
                table: "nominadetalles");

            migrationBuilder.DropColumn(
                name: "DiasFestivoTrabajadoFija",
                table: "nominadetalles");

            migrationBuilder.DropColumn(
                name: "DiasPorHorasTrabajados",
                table: "nominadetalles");

            migrationBuilder.DropColumn(
                name: "DiasVacacionesDisponibles",
                table: "nominadetalles");

            migrationBuilder.DropColumn(
                name: "DiasVacacionesRestantes",
                table: "nominadetalles");

            migrationBuilder.DropColumn(
                name: "HorasBancoAcumuladas",
                table: "nominadetalles");

            migrationBuilder.DropColumn(
                name: "HorasBancoConsumidas",
                table: "nominadetalles");

            migrationBuilder.DropColumn(
                name: "HorasBancoSaldoActual",
                table: "nominadetalles");

            migrationBuilder.DropColumn(
                name: "HorasDescansoNoPagado",
                table: "nominadetalles");

            migrationBuilder.DropColumn(
                name: "HorasDescansoPagado",
                table: "nominadetalles");

            migrationBuilder.DropColumn(
                name: "HorasDescansoTomado",
                table: "nominadetalles");

            migrationBuilder.DropColumn(
                name: "MinutosPorHorasFestivoNetos",
                table: "nominadetalles");

            migrationBuilder.DropColumn(
                name: "MinutosPorHorasNetos",
                table: "nominadetalles");

            migrationBuilder.DropColumn(
                name: "MontoDestajoInformativo",
                table: "nominadetalles");
        }
    }
}
