using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class EmpleadoActividadManoObra : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ActividadManoObraId",
                table: "Empleados",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_Empleados_ActividadManoObraId",
                table: "Empleados",
                column: "ActividadManoObraId");

            migrationBuilder.AddForeignKey(
                name: "FK_Empleados_ActividadesManoObra_ActividadManoObraId",
                table: "Empleados",
                column: "ActividadManoObraId",
                principalTable: "ActividadesManoObra",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Empleados_ActividadesManoObra_ActividadManoObraId",
                table: "Empleados");

            migrationBuilder.DropIndex(
                name: "IX_Empleados_ActividadManoObraId",
                table: "Empleados");

            migrationBuilder.DropColumn(
                name: "ActividadManoObraId",
                table: "Empleados");
        }
    }
}
