using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class NormalizarPedidoSerigrafiaYCotizacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
ALTER TABLE `PedidosSerigrafia`
    DROP COLUMN IF EXISTS `ProcesoFrecuencia`,
    DROP COLUMN IF EXISTS `ProcesoMesa`,
    DROP COLUMN IF EXISTS `ProcesoPlancha`,
    DROP COLUMN IF EXISTS `ProcesoPlastisol`,
    DROP COLUMN IF EXISTS `ProcesoPoteo`,
    DROP COLUMN IF EXISTS `ProcesoPulpo`,
    DROP COLUMN IF EXISTS `ProcesoSublimacion`,
    DROP COLUMN IF EXISTS `ProcesoTransfer`,
    DROP COLUMN IF EXISTS `ProcesoUV`,
    DROP COLUMN IF EXISTS `Talla15`,
    DROP COLUMN IF EXISTS `Talla16`,
    DROP COLUMN IF EXISTS `Talla17`,
    DROP COLUMN IF EXISTS `Talla18`,
    DROP COLUMN IF EXISTS `Talla19`,
    DROP COLUMN IF EXISTS `Talla20`,
    DROP COLUMN IF EXISTS `Talla21`,
    DROP COLUMN IF EXISTS `Talla22`,
    DROP COLUMN IF EXISTS `Talla23`,
    DROP COLUMN IF EXISTS `Talla24`,
    DROP COLUMN IF EXISTS `Talla25`,
    DROP COLUMN IF EXISTS `Talla26`,
    DROP COLUMN IF EXISTS `Talla27`,
    DROP COLUMN IF EXISTS `Talla28`,
    DROP COLUMN IF EXISTS `Talla29`,
    DROP COLUMN IF EXISTS `Talla30`;
");

            migrationBuilder.Sql(@"
ALTER TABLE `EscalasSerigrafia`
    DROP COLUMN IF EXISTS `Talla15`,
    DROP COLUMN IF EXISTS `Talla16`,
    DROP COLUMN IF EXISTS `Talla17`,
    DROP COLUMN IF EXISTS `Talla18`,
    DROP COLUMN IF EXISTS `Talla19`,
    DROP COLUMN IF EXISTS `Talla20`,
    DROP COLUMN IF EXISTS `Talla21`,
    DROP COLUMN IF EXISTS `Talla22`,
    DROP COLUMN IF EXISTS `Talla23`,
    DROP COLUMN IF EXISTS `Talla24`,
    DROP COLUMN IF EXISTS `Talla25`,
    DROP COLUMN IF EXISTS `Talla26`,
    DROP COLUMN IF EXISTS `Talla27`,
    DROP COLUMN IF EXISTS `Talla28`,
    DROP COLUMN IF EXISTS `Talla29`,
    DROP COLUMN IF EXISTS `Talla30`;
");

            migrationBuilder.Sql(@"
ALTER TABLE `CotizacionesSerigrafia`
    DROP COLUMN IF EXISTS `CostoCentradoMaterial`,
    DROP COLUMN IF EXISTS `CostoLavadoPantalla`,
    DROP COLUMN IF EXISTS `CostoRecogidoAmarrado`,
    DROP COLUMN IF EXISTS `CostoSerigrafista1`,
    DROP COLUMN IF EXISTS `CostoSerigrafista2`,
    DROP COLUMN IF EXISTS `CostoTiradoMaterial`,
    DROP COLUMN IF EXISTS `GastoAdministracion`,
    DROP COLUMN IF EXISTS `GastoElectricidadPlancha`,
    DROP COLUMN IF EXISTS `GastoElectricidadPulpo`,
    DROP COLUMN IF EXISTS `GastoLuzGeneral`,
    DROP COLUMN IF EXISTS `GastosFijos`,
    DROP COLUMN IF EXISTS `InsumoCiclo1RACantidad`,
    DROP COLUMN IF EXISTS `InsumoCiclo1RACosto`,
    DROP COLUMN IF EXISTS `InsumoCiclo1RAPrecio`,
    DROP COLUMN IF EXISTS `InsumoCicloEcoCantidad`,
    DROP COLUMN IF EXISTS `InsumoCicloEcoCosto`,
    DROP COLUMN IF EXISTS `InsumoCicloEcoPrecio`,
    DROP COLUMN IF EXISTS `InsumoDrySiliconCantidad`,
    DROP COLUMN IF EXISTS `InsumoDrySiliconCosto`,
    DROP COLUMN IF EXISTS `InsumoDrySiliconPrecio`,
    DROP COLUMN IF EXISTS `InsumoPTWaxCantidad`,
    DROP COLUMN IF EXISTS `InsumoPTWaxCosto`,
    DROP COLUMN IF EXISTS `InsumoPTWaxPrecio`,
    DROP COLUMN IF EXISTS `InsumoTrapoBlancoCantidad`,
    DROP COLUMN IF EXISTS `InsumoTrapoBlancoCosto`,
    DROP COLUMN IF EXISTS `InsumoTrapoBlancoPrecio`,
    DROP COLUMN IF EXISTS `InsumoX6Cantidad`,
    DROP COLUMN IF EXISTS `InsumoX6Costo`,
    DROP COLUMN IF EXISTS `InsumoX6Precio`,
    DROP COLUMN IF EXISTS `PrecioCentradoMaterial`,
    DROP COLUMN IF EXISTS `PrecioLavadoPantalla`,
    DROP COLUMN IF EXISTS `PrecioRecogidoAmarrado`,
    DROP COLUMN IF EXISTS `PrecioSerigrafista1`,
    DROP COLUMN IF EXISTS `PrecioSerigrafista2`,
    DROP COLUMN IF EXISTS `PrecioTiradoMaterial`,
    DROP COLUMN IF EXISTS `TiempoCentradoMaterial`,
    DROP COLUMN IF EXISTS `TiempoLavadoPantalla`,
    DROP COLUMN IF EXISTS `TiempoRecogidoAmarrado`,
    DROP COLUMN IF EXISTS `TiempoSerigrafista1`,
    DROP COLUMN IF EXISTS `TiempoSerigrafista2`,
    DROP COLUMN IF EXISTS `TiempoTiradoMaterial`,
    DROP COLUMN IF EXISTS `Tinta1Consumo`,
    DROP COLUMN IF EXISTS `Tinta1Costo`,
    DROP COLUMN IF EXISTS `Tinta1Nombre`,
    DROP COLUMN IF EXISTS `Tinta1PesoFinal`,
    DROP COLUMN IF EXISTS `Tinta1PesoInicial`,
    DROP COLUMN IF EXISTS `Tinta1PrecioUnitario`,
    DROP COLUMN IF EXISTS `Tinta2Consumo`,
    DROP COLUMN IF EXISTS `Tinta2Costo`,
    DROP COLUMN IF EXISTS `Tinta2Nombre`,
    DROP COLUMN IF EXISTS `Tinta2PesoFinal`,
    DROP COLUMN IF EXISTS `Tinta2PesoInicial`,
    DROP COLUMN IF EXISTS `Tinta2PrecioUnitario`,
    DROP COLUMN IF EXISTS `Tinta3Consumo`,
    DROP COLUMN IF EXISTS `Tinta3Costo`,
    DROP COLUMN IF EXISTS `Tinta3Nombre`,
    DROP COLUMN IF EXISTS `Tinta3PesoFinal`,
    DROP COLUMN IF EXISTS `Tinta3PesoInicial`,
    DROP COLUMN IF EXISTS `Tinta3PrecioUnitario`,
    DROP COLUMN IF EXISTS `Tinta4Consumo`,
    DROP COLUMN IF EXISTS `Tinta4Costo`,
    DROP COLUMN IF EXISTS `Tinta4Nombre`,
    DROP COLUMN IF EXISTS `Tinta4PesoFinal`,
    DROP COLUMN IF EXISTS `Tinta4PesoInicial`,
    DROP COLUMN IF EXISTS `Tinta4PrecioUnitario`,
    DROP COLUMN IF EXISTS `Tinta5Consumo`,
    DROP COLUMN IF EXISTS `Tinta5Costo`,
    DROP COLUMN IF EXISTS `Tinta5Nombre`,
    DROP COLUMN IF EXISTS `Tinta5PesoFinal`,
    DROP COLUMN IF EXISTS `Tinta5PesoInicial`,
    DROP COLUMN IF EXISTS `Tinta5PrecioUnitario`,
    DROP COLUMN IF EXISTS `Tinta6Consumo`,
    DROP COLUMN IF EXISTS `Tinta6Costo`,
    DROP COLUMN IF EXISTS `Tinta6Nombre`,
    DROP COLUMN IF EXISTS `Tinta6PesoFinal`,
    DROP COLUMN IF EXISTS `Tinta6PesoInicial`,
    DROP COLUMN IF EXISTS `Tinta6PrecioUnitario`,
    DROP COLUMN IF EXISTS `Tinta7Consumo`,
    DROP COLUMN IF EXISTS `Tinta7Costo`,
    DROP COLUMN IF EXISTS `Tinta7Nombre`,
    DROP COLUMN IF EXISTS `Tinta7PesoFinal`,
    DROP COLUMN IF EXISTS `Tinta7PesoInicial`,
    DROP COLUMN IF EXISTS `Tinta7PrecioUnitario`,
    DROP COLUMN IF EXISTS `Tinta8Consumo`,
    DROP COLUMN IF EXISTS `Tinta8Costo`,
    DROP COLUMN IF EXISTS `Tinta8Nombre`,
    DROP COLUMN IF EXISTS `Tinta8PesoFinal`,
    DROP COLUMN IF EXISTS `Tinta8PesoInicial`,
    DROP COLUMN IF EXISTS `Tinta8PrecioUnitario`;
");

            migrationBuilder.Sql(@"
CREATE TABLE IF NOT EXISTS `CotizacionDetalles` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `CotizacionSerigrafiaId` char(36) COLLATE ascii_general_ci NOT NULL,
    `Categoria` int NOT NULL,
    `Concepto` varchar(200) NOT NULL,
    `Orden` int NOT NULL,
    `Consumo` decimal(18,4) NOT NULL,
    `PrecioUnitario` decimal(18,4) NOT NULL,
    `Costo` decimal(18,4) NOT NULL,
    `PesoInicial` decimal(18,4) NULL,
    `PesoFinal` decimal(18,4) NULL,
    `Tiempo` decimal(18,2) NULL,
    `Precio` decimal(18,2) NULL,
    `MateriaPrimaId` char(36) COLLATE ascii_general_ci NULL,
    `InsumoId` char(36) COLLATE ascii_general_ci NULL,
    `ActividadManoObraId` char(36) COLLATE ascii_general_ci NULL,
    `GastoFijoId` char(36) COLLATE ascii_general_ci NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NULL,
    `CreatedBy` longtext NULL,
    `UpdatedBy` longtext NULL,
    `IsActive` tinyint(1) NOT NULL,
    PRIMARY KEY (`Id`),
    KEY `IX_CotizacionDetalles_CotizacionSerigrafiaId` (`CotizacionSerigrafiaId`),
    KEY `IX_CotizacionDetalles_Categoria` (`Categoria`),
    KEY `IX_CotizacionDetalles_ActividadManoObraId` (`ActividadManoObraId`),
    KEY `IX_CotizacionDetalles_GastoFijoId` (`GastoFijoId`),
    KEY `IX_CotizacionDetalles_InsumoId` (`InsumoId`),
    KEY `IX_CotizacionDetalles_MateriaPrimaId` (`MateriaPrimaId`),
    CONSTRAINT `FK_CotDet_CotSer` FOREIGN KEY (`CotizacionSerigrafiaId`) REFERENCES `CotizacionesSerigrafia` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_CotDet_ActMO` FOREIGN KEY (`ActividadManoObraId`) REFERENCES `ActividadesManoObra` (`Id`) ON DELETE SET NULL,
    CONSTRAINT `FK_CotDet_GasFix` FOREIGN KEY (`GastoFijoId`) REFERENCES `GastosFijos` (`Id`) ON DELETE SET NULL,
    CONSTRAINT `FK_CotDet_Insumo` FOREIGN KEY (`InsumoId`) REFERENCES `Insumos` (`Id`) ON DELETE SET NULL,
    CONSTRAINT `FK_CotDet_MatPri` FOREIGN KEY (`MateriaPrimaId`) REFERENCES `MateriasPrimas` (`Id`) ON DELETE SET NULL
) CHARACTER SET=utf8mb4;
");

            migrationBuilder.Sql(@"
CREATE TABLE IF NOT EXISTS `EscalaSerigrafiaTallas` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `EscalaSerigrafiaId` char(36) COLLATE ascii_general_ci NOT NULL,
    `Talla` varchar(10) NOT NULL,
    `Cantidad` int NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NULL,
    `CreatedBy` longtext NULL,
    `UpdatedBy` longtext NULL,
    `IsActive` tinyint(1) NOT NULL,
    PRIMARY KEY (`Id`),
    KEY `IX_EscalaSerigrafiaTallas_EscalaSerigrafiaId` (`EscalaSerigrafiaId`),
    CONSTRAINT `FK_EscalaTall_Escala` FOREIGN KEY (`EscalaSerigrafiaId`) REFERENCES `EscalasSerigrafia` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;
");

            migrationBuilder.Sql(@"
CREATE TABLE IF NOT EXISTS `PedidoSerigrafiaTallas` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `PedidoSerigrafiaId` char(36) COLLATE ascii_general_ci NOT NULL,
    `Talla` varchar(10) NOT NULL,
    `Cantidad` int NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NULL,
    `CreatedBy` longtext NULL,
    `UpdatedBy` longtext NULL,
    `IsActive` tinyint(1) NOT NULL,
    PRIMARY KEY (`Id`),
    KEY `IX_PedidoSerigrafiaTallas_PedidoSerigrafiaId` (`PedidoSerigrafiaId`),
    CONSTRAINT `FK_PedTall_Pedido` FOREIGN KEY (`PedidoSerigrafiaId`) REFERENCES `PedidosSerigrafia` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;
");


        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CotizacionDetalles");

            migrationBuilder.DropTable(
                name: "EscalaSerigrafiaTallas");

            migrationBuilder.DropTable(
                name: "PedidoSerigrafiaTallas");


            migrationBuilder.AddColumn<bool>(
                name: "ProcesoFrecuencia",
                table: "PedidosSerigrafia",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ProcesoMesa",
                table: "PedidosSerigrafia",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ProcesoPlancha",
                table: "PedidosSerigrafia",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ProcesoPlastisol",
                table: "PedidosSerigrafia",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ProcesoPoteo",
                table: "PedidosSerigrafia",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ProcesoPulpo",
                table: "PedidosSerigrafia",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ProcesoSublimacion",
                table: "PedidosSerigrafia",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ProcesoTransfer",
                table: "PedidosSerigrafia",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ProcesoUV",
                table: "PedidosSerigrafia",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Talla15",
                table: "PedidosSerigrafia",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Talla16",
                table: "PedidosSerigrafia",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Talla17",
                table: "PedidosSerigrafia",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Talla18",
                table: "PedidosSerigrafia",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Talla19",
                table: "PedidosSerigrafia",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Talla20",
                table: "PedidosSerigrafia",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Talla21",
                table: "PedidosSerigrafia",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Talla22",
                table: "PedidosSerigrafia",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Talla23",
                table: "PedidosSerigrafia",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Talla24",
                table: "PedidosSerigrafia",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Talla25",
                table: "PedidosSerigrafia",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Talla26",
                table: "PedidosSerigrafia",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Talla27",
                table: "PedidosSerigrafia",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Talla28",
                table: "PedidosSerigrafia",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Talla29",
                table: "PedidosSerigrafia",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Talla30",
                table: "PedidosSerigrafia",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Talla15",
                table: "EscalasSerigrafia",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Talla16",
                table: "EscalasSerigrafia",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Talla17",
                table: "EscalasSerigrafia",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Talla18",
                table: "EscalasSerigrafia",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Talla19",
                table: "EscalasSerigrafia",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Talla20",
                table: "EscalasSerigrafia",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Talla21",
                table: "EscalasSerigrafia",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Talla22",
                table: "EscalasSerigrafia",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Talla23",
                table: "EscalasSerigrafia",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Talla24",
                table: "EscalasSerigrafia",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Talla25",
                table: "EscalasSerigrafia",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Talla26",
                table: "EscalasSerigrafia",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Talla27",
                table: "EscalasSerigrafia",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Talla28",
                table: "EscalasSerigrafia",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Talla29",
                table: "EscalasSerigrafia",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Talla30",
                table: "EscalasSerigrafia",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "CostoCentradoMaterial",
                table: "CotizacionesSerigrafia",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CostoLavadoPantalla",
                table: "CotizacionesSerigrafia",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CostoRecogidoAmarrado",
                table: "CotizacionesSerigrafia",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CostoSerigrafista1",
                table: "CotizacionesSerigrafia",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CostoSerigrafista2",
                table: "CotizacionesSerigrafia",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CostoTiradoMaterial",
                table: "CotizacionesSerigrafia",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "GastoAdministracion",
                table: "CotizacionesSerigrafia",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "GastoElectricidadPlancha",
                table: "CotizacionesSerigrafia",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "GastoElectricidadPulpo",
                table: "CotizacionesSerigrafia",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "GastoLuzGeneral",
                table: "CotizacionesSerigrafia",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "GastosFijos",
                table: "CotizacionesSerigrafia",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "InsumoCiclo1RACantidad",
                table: "CotizacionesSerigrafia",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "InsumoCiclo1RACosto",
                table: "CotizacionesSerigrafia",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "InsumoCiclo1RAPrecio",
                table: "CotizacionesSerigrafia",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "InsumoCicloEcoCantidad",
                table: "CotizacionesSerigrafia",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "InsumoCicloEcoCosto",
                table: "CotizacionesSerigrafia",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "InsumoCicloEcoPrecio",
                table: "CotizacionesSerigrafia",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "InsumoDrySiliconCantidad",
                table: "CotizacionesSerigrafia",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "InsumoDrySiliconCosto",
                table: "CotizacionesSerigrafia",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "InsumoDrySiliconPrecio",
                table: "CotizacionesSerigrafia",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "InsumoPTWaxCantidad",
                table: "CotizacionesSerigrafia",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "InsumoPTWaxCosto",
                table: "CotizacionesSerigrafia",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "InsumoPTWaxPrecio",
                table: "CotizacionesSerigrafia",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "InsumoTrapoBlancoCantidad",
                table: "CotizacionesSerigrafia",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "InsumoTrapoBlancoCosto",
                table: "CotizacionesSerigrafia",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "InsumoTrapoBlancoPrecio",
                table: "CotizacionesSerigrafia",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "InsumoX6Cantidad",
                table: "CotizacionesSerigrafia",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "InsumoX6Costo",
                table: "CotizacionesSerigrafia",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "InsumoX6Precio",
                table: "CotizacionesSerigrafia",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PrecioCentradoMaterial",
                table: "CotizacionesSerigrafia",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PrecioLavadoPantalla",
                table: "CotizacionesSerigrafia",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PrecioRecogidoAmarrado",
                table: "CotizacionesSerigrafia",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PrecioSerigrafista1",
                table: "CotizacionesSerigrafia",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PrecioSerigrafista2",
                table: "CotizacionesSerigrafia",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PrecioTiradoMaterial",
                table: "CotizacionesSerigrafia",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TiempoCentradoMaterial",
                table: "CotizacionesSerigrafia",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TiempoLavadoPantalla",
                table: "CotizacionesSerigrafia",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TiempoRecogidoAmarrado",
                table: "CotizacionesSerigrafia",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TiempoSerigrafista1",
                table: "CotizacionesSerigrafia",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TiempoSerigrafista2",
                table: "CotizacionesSerigrafia",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TiempoTiradoMaterial",
                table: "CotizacionesSerigrafia",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Tinta1Consumo",
                table: "CotizacionesSerigrafia",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Tinta1Costo",
                table: "CotizacionesSerigrafia",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tinta1Nombre",
                table: "CotizacionesSerigrafia",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "Tinta1PesoFinal",
                table: "CotizacionesSerigrafia",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Tinta1PesoInicial",
                table: "CotizacionesSerigrafia",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Tinta1PrecioUnitario",
                table: "CotizacionesSerigrafia",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Tinta2Consumo",
                table: "CotizacionesSerigrafia",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Tinta2Costo",
                table: "CotizacionesSerigrafia",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tinta2Nombre",
                table: "CotizacionesSerigrafia",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "Tinta2PesoFinal",
                table: "CotizacionesSerigrafia",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Tinta2PesoInicial",
                table: "CotizacionesSerigrafia",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Tinta2PrecioUnitario",
                table: "CotizacionesSerigrafia",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Tinta3Consumo",
                table: "CotizacionesSerigrafia",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Tinta3Costo",
                table: "CotizacionesSerigrafia",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tinta3Nombre",
                table: "CotizacionesSerigrafia",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "Tinta3PesoFinal",
                table: "CotizacionesSerigrafia",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Tinta3PesoInicial",
                table: "CotizacionesSerigrafia",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Tinta3PrecioUnitario",
                table: "CotizacionesSerigrafia",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Tinta4Consumo",
                table: "CotizacionesSerigrafia",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Tinta4Costo",
                table: "CotizacionesSerigrafia",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tinta4Nombre",
                table: "CotizacionesSerigrafia",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "Tinta4PesoFinal",
                table: "CotizacionesSerigrafia",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Tinta4PesoInicial",
                table: "CotizacionesSerigrafia",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Tinta4PrecioUnitario",
                table: "CotizacionesSerigrafia",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Tinta5Consumo",
                table: "CotizacionesSerigrafia",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Tinta5Costo",
                table: "CotizacionesSerigrafia",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tinta5Nombre",
                table: "CotizacionesSerigrafia",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "Tinta5PesoFinal",
                table: "CotizacionesSerigrafia",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Tinta5PesoInicial",
                table: "CotizacionesSerigrafia",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Tinta5PrecioUnitario",
                table: "CotizacionesSerigrafia",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Tinta6Consumo",
                table: "CotizacionesSerigrafia",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Tinta6Costo",
                table: "CotizacionesSerigrafia",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tinta6Nombre",
                table: "CotizacionesSerigrafia",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "Tinta6PesoFinal",
                table: "CotizacionesSerigrafia",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Tinta6PesoInicial",
                table: "CotizacionesSerigrafia",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Tinta6PrecioUnitario",
                table: "CotizacionesSerigrafia",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Tinta7Consumo",
                table: "CotizacionesSerigrafia",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Tinta7Costo",
                table: "CotizacionesSerigrafia",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tinta7Nombre",
                table: "CotizacionesSerigrafia",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "Tinta7PesoFinal",
                table: "CotizacionesSerigrafia",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Tinta7PesoInicial",
                table: "CotizacionesSerigrafia",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Tinta7PrecioUnitario",
                table: "CotizacionesSerigrafia",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Tinta8Consumo",
                table: "CotizacionesSerigrafia",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Tinta8Costo",
                table: "CotizacionesSerigrafia",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tinta8Nombre",
                table: "CotizacionesSerigrafia",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "Tinta8PesoFinal",
                table: "CotizacionesSerigrafia",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Tinta8PesoInicial",
                table: "CotizacionesSerigrafia",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Tinta8PrecioUnitario",
                table: "CotizacionesSerigrafia",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 21, 17, 11, 16, 682, DateTimeKind.Utc).AddTicks(7440));

            migrationBuilder.UpdateData(
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000002"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 21, 17, 11, 16, 682, DateTimeKind.Utc).AddTicks(8676));
        }
    }
}
