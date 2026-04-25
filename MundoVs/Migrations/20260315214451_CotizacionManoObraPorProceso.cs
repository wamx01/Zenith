using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class CotizacionManoObraPorProceso : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TipoProcesoId",
                table: "CotizacionDetalles",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_CotizacionDetalles_TipoProcesoId",
                table: "CotizacionDetalles",
                column: "TipoProcesoId");

            migrationBuilder.AddForeignKey(
                name: "FK_CotizacionDetalles_TiposProceso_TipoProcesoId",
                table: "CotizacionDetalles",
                column: "TipoProcesoId",
                principalTable: "TiposProceso",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CotizacionDetalles_TiposProceso_TipoProcesoId",
                table: "CotizacionDetalles");

            migrationBuilder.DropIndex(
                name: "IX_CotizacionDetalles_TipoProcesoId",
                table: "CotizacionDetalles");

            migrationBuilder.DropColumn(
                name: "TipoProcesoId",
                table: "CotizacionDetalles");
        }
    }
}
