using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class PedidoDetalleCotizacionSeleccionada : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE `PedidoDetalles` ADD COLUMN IF NOT EXISTS `CotizacionSerigrafiaId` char(36) NULL COLLATE ascii_general_ci;");
            migrationBuilder.Sql("ALTER TABLE `PedidoDetalles` ADD INDEX IF NOT EXISTS `IX_PedidoDetalles_CotizacionSerigrafiaId` (`CotizacionSerigrafiaId`);");
            migrationBuilder.Sql("ALTER TABLE `PedidoDetalles` DROP FOREIGN KEY IF EXISTS `FK_PedidoDetalles_CotizacionesSerigrafia_CotizacionSerigrafiaId`;");
            migrationBuilder.Sql("ALTER TABLE `PedidoDetalles` ADD CONSTRAINT `FK_PedidoDetalles_CotizacionesSerigrafia_CotizacionSerigrafiaId` FOREIGN KEY (`CotizacionSerigrafiaId`) REFERENCES `CotizacionesSerigrafia` (`Id`) ON DELETE SET NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE `PedidoDetalles` DROP FOREIGN KEY IF EXISTS `FK_PedidoDetalles_CotizacionesSerigrafia_CotizacionSerigrafiaId`;");
            migrationBuilder.Sql("ALTER TABLE `PedidoDetalles` DROP INDEX IF EXISTS `IX_PedidoDetalles_CotizacionSerigrafiaId`;");
            migrationBuilder.Sql("ALTER TABLE `PedidoDetalles` DROP COLUMN IF EXISTS `CotizacionSerigrafiaId`;");
        }
    }
}
