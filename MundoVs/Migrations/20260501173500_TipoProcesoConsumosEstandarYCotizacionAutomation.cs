using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using MundoVs.Infrastructure.Data;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(CrmDbContext))]
    [Migration("20260501173500_TipoProcesoConsumosEstandarYCotizacionAutomation")]
    public partial class TipoProcesoConsumosEstandarYCotizacionAutomation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "GeneraConsumosAutomaticos",
                table: "tiposproceso",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "MinutosEstandar",
                table: "tiposproceso",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MultiplicadorDefault",
                table: "tiposproceso",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 1m);

            migrationBuilder.AddColumn<bool>(
                name: "PermiteMultiplicador",
                table: "tiposproceso",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MinutosEstandarAplicados",
                table: "cotizacionserigrafiaprocesos",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Multiplicador",
                table: "cotizacionserigrafiaprocesos",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 1m);

            migrationBuilder.AddColumn<int>(
                name: "Orden",
                table: "cotizacionserigrafiaprocesos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "TiempoTotal",
                table: "cotizacionserigrafiaprocesos",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "CotizacionSerigrafiaProcesoId",
                table: "cotizaciondetalles",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<int>(
                name: "OrigenDetalle",
                table: "cotizaciondetalles",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<Guid>(
                name: "TipoProcesoConsumoId",
                table: "cotizaciondetalles",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.Sql(@"
CREATE TABLE `tiposprocesoconsumos` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `TipoProcesoId` char(36) COLLATE ascii_general_ci NOT NULL,
    `Origen` int NOT NULL,
    `CategoriaCotizacion` int NOT NULL,
    `MateriaPrimaId` char(36) COLLATE ascii_general_ci NULL,
    `InsumoId` char(36) COLLATE ascii_general_ci NULL,
    `CantidadBase` decimal(18,4) NOT NULL,
    `Orden` int NOT NULL,
    `Activo` tinyint(1) NOT NULL,
    `Observaciones` varchar(300) NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NULL,
    `CreatedBy` longtext NULL,
    `UpdatedBy` longtext NULL,
    `IsActive` tinyint(1) NOT NULL,
    CONSTRAINT `PK_tiposprocesoconsumos` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_tiposprocesoconsumos_tiposproceso_TipoProcesoId` FOREIGN KEY (`TipoProcesoId`) REFERENCES `tiposproceso` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_tiposprocesoconsumos_materiasprimas_MateriaPrimaId` FOREIGN KEY (`MateriaPrimaId`) REFERENCES `materiasprimas` (`Id`) ON DELETE SET NULL,
    CONSTRAINT `FK_tiposprocesoconsumos_insumos_InsumoId` FOREIGN KEY (`InsumoId`) REFERENCES `insumos` (`Id`) ON DELETE SET NULL
) CHARACTER SET=utf8mb4;");

            migrationBuilder.CreateIndex(
                name: "IX_cotizaciondetalles_CotizacionSerigrafiaProcesoId",
                table: "cotizaciondetalles",
                column: "CotizacionSerigrafiaProcesoId");

            migrationBuilder.CreateIndex(
                name: "IX_cotizaciondetalles_TipoProcesoConsumoId",
                table: "cotizaciondetalles",
                column: "TipoProcesoConsumoId");

            migrationBuilder.CreateIndex(
                name: "IX_tiposprocesoconsumos_TipoProcesoId",
                table: "tiposprocesoconsumos",
                column: "TipoProcesoId");

            migrationBuilder.CreateIndex(
                name: "IX_tiposprocesoconsumos_TipoProcesoId_Orden",
                table: "tiposprocesoconsumos",
                columns: new[] { "TipoProcesoId", "Orden" });

            migrationBuilder.CreateIndex(
                name: "IX_tiposprocesoconsumos_MateriaPrimaId",
                table: "tiposprocesoconsumos",
                column: "MateriaPrimaId");

            migrationBuilder.CreateIndex(
                name: "IX_tiposprocesoconsumos_InsumoId",
                table: "tiposprocesoconsumos",
                column: "InsumoId");

            migrationBuilder.AddForeignKey(
                name: "FK_cotizaciondetalles_cotizacionserigrafiaprocesos_CotizacionSerigrafiaProcesoId",
                table: "cotizaciondetalles",
                column: "CotizacionSerigrafiaProcesoId",
                principalTable: "cotizacionserigrafiaprocesos",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_cotizaciondetalles_tiposprocesoconsumos_TipoProcesoConsumoId",
                table: "cotizaciondetalles",
                column: "TipoProcesoConsumoId",
                principalTable: "tiposprocesoconsumos",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.Sql(@"
UPDATE `tiposproceso`
SET `PermiteMultiplicador` = 1,
    `MultiplicadorDefault` = 1
WHERE `MultiplicadorDefault` = 0;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_cotizaciondetalles_cotizacionserigrafiaprocesos_CotizacionSerigrafiaProcesoId",
                table: "cotizaciondetalles");

            migrationBuilder.DropForeignKey(
                name: "FK_cotizaciondetalles_tiposprocesoconsumos_TipoProcesoConsumoId",
                table: "cotizaciondetalles");

            migrationBuilder.DropIndex(
                name: "IX_cotizaciondetalles_CotizacionSerigrafiaProcesoId",
                table: "cotizaciondetalles");

            migrationBuilder.DropIndex(
                name: "IX_cotizaciondetalles_TipoProcesoConsumoId",
                table: "cotizaciondetalles");

            migrationBuilder.DropColumn(
                name: "GeneraConsumosAutomaticos",
                table: "tiposproceso");

            migrationBuilder.DropColumn(
                name: "MinutosEstandar",
                table: "tiposproceso");

            migrationBuilder.DropColumn(
                name: "MultiplicadorDefault",
                table: "tiposproceso");

            migrationBuilder.DropColumn(
                name: "PermiteMultiplicador",
                table: "tiposproceso");

            migrationBuilder.DropColumn(
                name: "MinutosEstandarAplicados",
                table: "cotizacionserigrafiaprocesos");

            migrationBuilder.DropColumn(
                name: "Multiplicador",
                table: "cotizacionserigrafiaprocesos");

            migrationBuilder.DropColumn(
                name: "Orden",
                table: "cotizacionserigrafiaprocesos");

            migrationBuilder.DropColumn(
                name: "TiempoTotal",
                table: "cotizacionserigrafiaprocesos");

            migrationBuilder.DropColumn(
                name: "CotizacionSerigrafiaProcesoId",
                table: "cotizaciondetalles");

            migrationBuilder.DropColumn(
                name: "OrigenDetalle",
                table: "cotizaciondetalles");

            migrationBuilder.DropColumn(
                name: "TipoProcesoConsumoId",
                table: "cotizaciondetalles");

            migrationBuilder.Sql("DROP TABLE IF EXISTS `tiposprocesoconsumos`;");
        }
    }
}
