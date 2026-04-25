using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class VincularRegistrosDestajoAVales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ValeDestajoDetalleId",
                table: "RegistrosDestajoProceso",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosDestajoProceso_ValeDestajoDetalleId",
                table: "RegistrosDestajoProceso",
                column: "ValeDestajoDetalleId");

            migrationBuilder.AddForeignKey(
                name: "FK_RegistrosDestajoProceso_ValesDestajoDetalle_ValeDestajoDetal~",
                table: "RegistrosDestajoProceso",
                column: "ValeDestajoDetalleId",
                principalTable: "ValesDestajoDetalle",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RegistrosDestajoProceso_ValesDestajoDetalle_ValeDestajoDetal~",
                table: "RegistrosDestajoProceso");

            migrationBuilder.DropIndex(
                name: "IX_RegistrosDestajoProceso_ValeDestajoDetalleId",
                table: "RegistrosDestajoProceso");

            migrationBuilder.DropColumn(
                name: "ValeDestajoDetalleId",
                table: "RegistrosDestajoProceso");
        }
    }
}
