using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class Fase4_InsumoTipoInventario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TipoInventarioId",
                table: "Insumos",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_Insumos_TipoInventarioId",
                table: "Insumos",
                column: "TipoInventarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Insumos_TiposInventario_TipoInventarioId",
                table: "Insumos",
                column: "TipoInventarioId",
                principalTable: "TiposInventario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql(@"
UPDATE Insumos i
JOIN CategoriasInventario ci
    ON ci.EmpresaId = i.EmpresaId
   AND ci.Codigo = 'INSUMO'
JOIN TiposInventario ti
    ON ti.EmpresaId = i.EmpresaId
   AND ti.CategoriaInventarioId = ci.Id
   AND (
        (i.TipoInsumo = 1 AND ti.Codigo = 'BASICO') OR
        (i.TipoInsumo = 2 AND ti.Codigo = 'DIVERSO')
   )
SET i.TipoInventarioId = ti.Id
WHERE i.TipoInventarioId IS NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Insumos_TiposInventario_TipoInventarioId",
                table: "Insumos");

            migrationBuilder.DropIndex(
                name: "IX_Insumos_TipoInventarioId",
                table: "Insumos");

            migrationBuilder.DropColumn(
                name: "TipoInventarioId",
                table: "Insumos");
        }
    }
}
