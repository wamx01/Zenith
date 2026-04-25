using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class TipoProcesoActividadMo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ActividadManoObraId",
                table: "TiposProceso",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<decimal>(
                name: "MinutosEstandar",
                table: "TiposProceso",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TarifaPorMinuto",
                table: "TiposProceso",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_TiposProceso_ActividadManoObraId",
                table: "TiposProceso",
                column: "ActividadManoObraId");

            migrationBuilder.AddForeignKey(
                name: "FK_TiposProceso_ActividadesManoObra_ActividadManoObraId",
                table: "TiposProceso",
                column: "ActividadManoObraId",
                principalTable: "ActividadesManoObra",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TiposProceso_ActividadesManoObra_ActividadManoObraId",
                table: "TiposProceso");

            migrationBuilder.DropIndex(
                name: "IX_TiposProceso_ActividadManoObraId",
                table: "TiposProceso");

            migrationBuilder.DropColumn(
                name: "ActividadManoObraId",
                table: "TiposProceso");

            migrationBuilder.DropColumn(
                name: "MinutosEstandar",
                table: "TiposProceso");

            migrationBuilder.DropColumn(
                name: "TarifaPorMinuto",
                table: "TiposProceso");
        }
    }
}
