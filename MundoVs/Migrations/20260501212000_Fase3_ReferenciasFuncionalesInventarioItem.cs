using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using MundoVs.Infrastructure.Data;

#nullable disable

namespace MundoVs.Migrations
{
    [DbContext(typeof(CrmDbContext))]
    [Migration("20260501212000_Fase3_ReferenciasFuncionalesInventarioItem")]
    public partial class Fase3_ReferenciasFuncionalesInventarioItem : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "InventarioItemId",
                table: "cotizaciondetalles",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "InventarioItemId",
                table: "tiposprocesoconsumos",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_cotizaciondetalles_InventarioItemId",
                table: "cotizaciondetalles",
                column: "InventarioItemId");

            migrationBuilder.CreateIndex(
                name: "IX_tiposprocesoconsumos_InventarioItemId",
                table: "tiposprocesoconsumos",
                column: "InventarioItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_cotizaciondetalles_logistica_inventario_item_InventarioItemId",
                table: "cotizaciondetalles",
                column: "InventarioItemId",
                principalTable: "logistica_inventario_item",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_tiposprocesoconsumos_logistica_inventario_item_InventarioItemId",
                table: "tiposprocesoconsumos",
                column: "InventarioItemId",
                principalTable: "logistica_inventario_item",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.Sql(@"
UPDATE cotizaciondetalles d
INNER JOIN logistica_inventario_item ii ON ii.MateriaPrimaOrigenId = d.MateriaPrimaId
SET d.InventarioItemId = ii.Id
WHERE d.MateriaPrimaId IS NOT NULL
  AND d.InventarioItemId IS NULL;");

            migrationBuilder.Sql(@"
UPDATE cotizaciondetalles d
INNER JOIN logistica_inventario_item ii ON ii.InsumoOrigenId = d.InsumoId
SET d.InventarioItemId = ii.Id
WHERE d.InsumoId IS NOT NULL
  AND d.InventarioItemId IS NULL;");

            migrationBuilder.Sql(@"
UPDATE tiposprocesoconsumos c
INNER JOIN logistica_inventario_item ii ON ii.MateriaPrimaOrigenId = c.MateriaPrimaId
SET c.InventarioItemId = ii.Id
WHERE c.MateriaPrimaId IS NOT NULL
  AND c.InventarioItemId IS NULL;");

            migrationBuilder.Sql(@"
UPDATE tiposprocesoconsumos c
INNER JOIN logistica_inventario_item ii ON ii.InsumoOrigenId = c.InsumoId
SET c.InventarioItemId = ii.Id
WHERE c.InsumoId IS NOT NULL
  AND c.InventarioItemId IS NULL;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_cotizaciondetalles_logistica_inventario_item_InventarioItemId",
                table: "cotizaciondetalles");

            migrationBuilder.DropForeignKey(
                name: "FK_tiposprocesoconsumos_logistica_inventario_item_InventarioItemId",
                table: "tiposprocesoconsumos");

            migrationBuilder.DropIndex(
                name: "IX_cotizaciondetalles_InventarioItemId",
                table: "cotizaciondetalles");

            migrationBuilder.DropIndex(
                name: "IX_tiposprocesoconsumos_InventarioItemId",
                table: "tiposprocesoconsumos");

            migrationBuilder.DropColumn(
                name: "InventarioItemId",
                table: "cotizaciondetalles");

            migrationBuilder.DropColumn(
                name: "InventarioItemId",
                table: "tiposprocesoconsumos");
        }
    }
}
