using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using MundoVs.Infrastructure.Data;

#nullable disable

namespace MundoVs.Migrations
{
    [DbContext(typeof(CrmDbContext))]
    [Migration("20260501200500_Fase2_MovimientosInventarioItemCompat")]
    public partial class Fase2_MovimientosInventarioItemCompat : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "InventarioItemId",
                table: "movimientosinventario",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_InventarioItemId",
                table: "movimientosinventario",
                column: "InventarioItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_MovimientosInventario_logistica_inventario_item_InventarioItemId",
                table: "movimientosinventario",
                column: "InventarioItemId",
                principalTable: "logistica_inventario_item",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql(@"
UPDATE movimientosinventario m
INNER JOIN logistica_inventario_item ii ON ii.MateriaPrimaOrigenId = m.MateriaPrimaId
SET m.InventarioItemId = ii.Id
WHERE m.MateriaPrimaId IS NOT NULL
  AND m.InventarioItemId IS NULL;");

            migrationBuilder.Sql(@"
UPDATE movimientosinventario m
INNER JOIN logistica_inventario_item ii ON ii.InsumoOrigenId = m.InsumoId
SET m.InventarioItemId = ii.Id
WHERE m.InsumoId IS NOT NULL
  AND m.InventarioItemId IS NULL;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MovimientosInventario_logistica_inventario_item_InventarioItemId",
                table: "movimientosinventario");

            migrationBuilder.DropIndex(
                name: "IX_MovimientosInventario_InventarioItemId",
                table: "movimientosinventario");

            migrationBuilder.DropColumn(
                name: "InventarioItemId",
                table: "movimientosinventario");
        }
    }
}
