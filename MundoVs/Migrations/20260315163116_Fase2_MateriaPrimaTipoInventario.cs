using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class Fase2_MateriaPrimaTipoInventario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TipoInventarioId",
                table: "MateriasPrimas",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_MateriasPrimas_TipoInventarioId",
                table: "MateriasPrimas",
                column: "TipoInventarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_MateriasPrimas_TiposInventario_TipoInventarioId",
                table: "MateriasPrimas",
                column: "TipoInventarioId",
                principalTable: "TiposInventario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql(@"
UPDATE MateriasPrimas mp
JOIN CategoriasInventario ci
    ON ci.EmpresaId = mp.EmpresaId
   AND ci.Codigo = 'MATERIA_PRIMA'
JOIN TiposInventario ti
    ON ti.EmpresaId = mp.EmpresaId
   AND ti.CategoriaInventarioId = ci.Id
   AND (
        (mp.TipoMateriaPrima = 1 AND ti.Codigo = 'TINTA') OR
        (mp.TipoMateriaPrima = 2 AND ti.Codigo = 'BASE') OR
        (mp.TipoMateriaPrima = 3 AND ti.Codigo = 'SOLVENTE') OR
        (mp.TipoMateriaPrima = 4 AND ti.Codigo = 'ADITIVO') OR
        (mp.TipoMateriaPrima = 5 AND ti.Codigo = 'OTRO')
   )
SET mp.TipoInventarioId = ti.Id
WHERE mp.TipoInventarioId IS NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MateriasPrimas_TiposInventario_TipoInventarioId",
                table: "MateriasPrimas");

            migrationBuilder.DropIndex(
                name: "IX_MateriasPrimas_TipoInventarioId",
                table: "MateriasPrimas");

            migrationBuilder.DropColumn(
                name: "TipoInventarioId",
                table: "MateriasPrimas");
        }
    }
}
