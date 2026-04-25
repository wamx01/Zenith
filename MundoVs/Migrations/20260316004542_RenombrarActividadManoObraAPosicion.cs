using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class RenombrarActividadManoObraAPosicion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE `CotizacionDetalles` DROP FOREIGN KEY IF EXISTS `FK_CotizacionDetalles_ActividadesManoObra_ActividadManoObraId`;");
            migrationBuilder.Sql("ALTER TABLE `CotizacionDetalles` DROP FOREIGN KEY IF EXISTS `FK_CotizacionDetalles_Posiciones_PosicionId`;");
            migrationBuilder.Sql("ALTER TABLE `Empleados` DROP FOREIGN KEY IF EXISTS `FK_Empleados_ActividadesManoObra_ActividadManoObraId`;");
            migrationBuilder.Sql("ALTER TABLE `Empleados` DROP FOREIGN KEY IF EXISTS `FK_Empleados_Posiciones_PosicionId`;");
            migrationBuilder.Sql("ALTER TABLE `TiposProceso` DROP FOREIGN KEY IF EXISTS `FK_TiposProceso_ActividadesManoObra_ActividadManoObraId`;");
            migrationBuilder.Sql("ALTER TABLE `TiposProceso` DROP FOREIGN KEY IF EXISTS `FK_TiposProceso_Posiciones_PosicionId`;");

            migrationBuilder.Sql("ALTER TABLE `CotizacionDetalles` CHANGE COLUMN IF EXISTS `ActividadManoObraId` `PosicionId` char(36) NULL;");
            migrationBuilder.Sql("ALTER TABLE `Empleados` CHANGE COLUMN IF EXISTS `ActividadManoObraId` `PosicionId` char(36) NULL;");
            migrationBuilder.Sql("ALTER TABLE `TiposProceso` CHANGE COLUMN IF EXISTS `ActividadManoObraId` `PosicionId` char(36) NULL;");

            migrationBuilder.Sql("ALTER TABLE `CotizacionDetalles` DROP INDEX IF EXISTS `IX_CotizacionDetalles_ActividadManoObraId`;");
            migrationBuilder.Sql("ALTER TABLE `CotizacionDetalles` ADD INDEX IF NOT EXISTS `IX_CotizacionDetalles_PosicionId` (`PosicionId`);");
            migrationBuilder.Sql("ALTER TABLE `Empleados` DROP INDEX IF EXISTS `IX_Empleados_ActividadManoObraId`;");
            migrationBuilder.Sql("ALTER TABLE `Empleados` ADD INDEX IF NOT EXISTS `IX_Empleados_PosicionId` (`PosicionId`);");
            migrationBuilder.Sql("ALTER TABLE `TiposProceso` DROP INDEX IF EXISTS `IX_TiposProceso_ActividadManoObraId`;");
            migrationBuilder.Sql("ALTER TABLE `TiposProceso` ADD INDEX IF NOT EXISTS `IX_TiposProceso_PosicionId` (`PosicionId`);");

            migrationBuilder.Sql("RENAME TABLE IF EXISTS `ActividadesManoObra` TO `Posiciones`;");
            migrationBuilder.Sql("ALTER TABLE `Posiciones` CHANGE COLUMN IF EXISTS `SueldoSugeridoSemanal` `TarifaPorMinuto` decimal(18,4) NOT NULL;");
            migrationBuilder.Sql("ALTER TABLE `Posiciones` MODIFY COLUMN IF EXISTS `TarifaPorMinuto` decimal(18,4) NOT NULL;");
            migrationBuilder.Sql("ALTER TABLE `Posiciones` ADD INDEX IF NOT EXISTS `IX_Posiciones_EmpresaId` (`EmpresaId`);");
            migrationBuilder.Sql("ALTER TABLE `Posiciones` ADD INDEX IF NOT EXISTS `IX_Posiciones_Nombre` (`Nombre`);");

            migrationBuilder.Sql("-- Se omite recrear FKs aquí para tolerar estados parciales heredados en MariaDB.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CotizacionDetalles_Posiciones_PosicionId",
                table: "CotizacionDetalles");

            migrationBuilder.DropForeignKey(
                name: "FK_Empleados_Posiciones_PosicionId",
                table: "Empleados");

            migrationBuilder.DropForeignKey(
                name: "FK_TiposProceso_Posiciones_PosicionId",
                table: "TiposProceso");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Posiciones",
                table: "Posiciones");

            migrationBuilder.RenameColumn(
                name: "PosicionId",
                table: "TiposProceso",
                newName: "ActividadManoObraId");

            migrationBuilder.RenameIndex(
                name: "IX_TiposProceso_PosicionId",
                table: "TiposProceso",
                newName: "IX_TiposProceso_ActividadManoObraId");

            migrationBuilder.RenameColumn(
                name: "PosicionId",
                table: "Empleados",
                newName: "ActividadManoObraId");

            migrationBuilder.RenameIndex(
                name: "IX_Empleados_PosicionId",
                table: "Empleados",
                newName: "IX_Empleados_ActividadManoObraId");

            migrationBuilder.RenameColumn(
                name: "PosicionId",
                table: "CotizacionDetalles",
                newName: "ActividadManoObraId");

            migrationBuilder.RenameIndex(
                name: "IX_CotizacionDetalles_PosicionId",
                table: "CotizacionDetalles",
                newName: "IX_CotizacionDetalles_ActividadManoObraId");

            migrationBuilder.AlterColumn<decimal>(
                name: "TarifaPorMinuto",
                table: "Posiciones",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.RenameColumn(
                name: "TarifaPorMinuto",
                table: "Posiciones",
                newName: "SueldoSugeridoSemanal");

            migrationBuilder.RenameTable(
                name: "Posiciones",
                newName: "ActividadesManoObra");

            migrationBuilder.RenameIndex(
                name: "IX_Posiciones_EmpresaId",
                table: "ActividadesManoObra",
                newName: "IX_ActividadesManoObra_EmpresaId");

            migrationBuilder.RenameIndex(
                name: "IX_Posiciones_Nombre",
                table: "ActividadesManoObra",
                newName: "IX_ActividadesManoObra_Nombre");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ActividadesManoObra",
                table: "ActividadesManoObra",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CotizacionDetalles_ActividadesManoObra_ActividadManoObraId",
                table: "CotizacionDetalles",
                column: "ActividadManoObraId",
                principalTable: "ActividadesManoObra",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Empleados_ActividadesManoObra_ActividadManoObraId",
                table: "Empleados",
                column: "ActividadManoObraId",
                principalTable: "ActividadesManoObra",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_TiposProceso_ActividadesManoObra_ActividadManoObraId",
                table: "TiposProceso",
                column: "ActividadManoObraId",
                principalTable: "ActividadesManoObra",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
