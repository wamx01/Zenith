using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class PedidoSerigrafiaTallaProceso : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE TABLE IF NOT EXISTS `PedidoSerigrafiaTallaProcesos` (
    `Id` char(36) NOT NULL,
    `PedidoSerigrafiaId` char(36) NOT NULL,
    `PedidoSerigrafiaTallaId` char(36) NOT NULL,
    `TipoProcesoId` char(36) NOT NULL,
    `Completado` bit NOT NULL DEFAULT b'0',
    `FechaPaso` datetime(6) NULL,
    `CreatedBy` varchar(255) NULL,
    `UpdatedBy` varchar(255) NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NULL,
    `IsActive` bit NOT NULL DEFAULT b'1',
    CONSTRAINT `PK_PedidoSerigrafiaTallaProcesos` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_PedidoSerigrafiaTallaProcesos_PedidoSerigrafia` FOREIGN KEY (`PedidoSerigrafiaId`) REFERENCES `PedidosSerigrafia` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_PedidoSerigrafiaTallaProcesos_Tallas` FOREIGN KEY (`PedidoSerigrafiaTallaId`) REFERENCES `PedidoSerigrafiaTallas` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_PedidoSerigrafiaTallaProcesos_TipoProceso` FOREIGN KEY (`TipoProcesoId`) REFERENCES `TiposProceso` (`Id`) ON DELETE RESTRICT
);
CREATE UNIQUE INDEX IF NOT EXISTS `IX_PedidoSerigrafiaTallaProcesos_Unique` ON `PedidoSerigrafiaTallaProcesos` (`PedidoSerigrafiaId`, `PedidoSerigrafiaTallaId`, `TipoProcesoId`);
CREATE INDEX IF NOT EXISTS `IX_PedidoSerigrafiaTallaProcesos_TipoProcesoId` ON `PedidoSerigrafiaTallaProcesos` (`TipoProcesoId`);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP TABLE IF EXISTS `PedidoSerigrafiaTallaProcesos`;");
        }
    }
}
