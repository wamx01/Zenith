using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using MundoVs.Infrastructure.Data;

#nullable disable

namespace MundoVs.Migrations
{
    [DbContext(typeof(CrmDbContext))]
    [Migration("20260501194500_Fase1_InventarioItemUnificado")]
    public partial class Fase1_InventarioItemUnificado : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TABLE IF EXISTS logistica_inventario_item;");

            migrationBuilder.CreateTable(
                name: "logistica_inventario_item",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EmpresaId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Codigo = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Nombre = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CategoriaInventarioId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    TipoInventarioId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    OrigenLegacy = table.Column<int>(type: "int", nullable: false),
                    MateriaPrimaOrigenId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    InsumoOrigenId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    CodigoPantone = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CodigoHex = table.Column<string>(type: "varchar(7)", maxLength: 7, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Cantidad = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    UnidadMedida = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StockMinimo = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_logistica_inventario_item", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_logistica_inventario_item_CategoriaInventarioId",
                table: "logistica_inventario_item",
                column: "CategoriaInventarioId");

            migrationBuilder.CreateIndex(
                name: "IX_logistica_inventario_item_EmpresaId_Codigo",
                table: "logistica_inventario_item",
                columns: new[] { "EmpresaId", "Codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_logistica_inventario_item_InsumoOrigenId",
                table: "logistica_inventario_item",
                column: "InsumoOrigenId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_logistica_inventario_item_MateriaPrimaOrigenId",
                table: "logistica_inventario_item",
                column: "MateriaPrimaOrigenId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_logistica_inventario_item_TipoInventarioId",
                table: "logistica_inventario_item",
                column: "TipoInventarioId");

            migrationBuilder.Sql(@"
INSERT INTO logistica_inventario_item
    (Id, EmpresaId, Codigo, Nombre, CategoriaInventarioId, TipoInventarioId, OrigenLegacy, MateriaPrimaOrigenId, CodigoPantone, CodigoHex, PrecioUnitario, Cantidad, UnidadMedida, StockMinimo, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, IsActive)
SELECT
    UUID(),
    mp.EmpresaId,
    mp.Codigo,
    mp.Nombre,
    ti.CategoriaInventarioId,
    mp.TipoInventarioId,
    1,
    mp.Id,
    mp.CodigoPantone,
    mp.CodigoHex,
    mp.PrecioUnitario,
    mp.Cantidad,
    mp.UnidadMedida,
    mp.StockMinimo,
    mp.CreatedAt,
    mp.UpdatedAt,
    mp.CreatedBy,
    mp.UpdatedBy,
    mp.IsActive
FROM materiasprimas mp
LEFT JOIN tiposinventario ti ON ti.Id = mp.TipoInventarioId
WHERE NOT EXISTS (
    SELECT 1
    FROM logistica_inventario_item ii
    WHERE ii.MateriaPrimaOrigenId = mp.Id
);");

            migrationBuilder.Sql(@"
INSERT INTO logistica_inventario_item
    (Id, EmpresaId, Codigo, Nombre, CategoriaInventarioId, TipoInventarioId, OrigenLegacy, InsumoOrigenId, PrecioUnitario, Cantidad, UnidadMedida, StockMinimo, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, IsActive)
SELECT
    UUID(),
    i.EmpresaId,
    i.Codigo,
    i.Nombre,
    ti.CategoriaInventarioId,
    i.TipoInventarioId,
    2,
    i.Id,
    i.PrecioUnitario,
    i.Cantidad,
    i.UnidadMedida,
    i.StockMinimo,
    i.CreatedAt,
    i.UpdatedAt,
    i.CreatedBy,
    i.UpdatedBy,
    i.IsActive
FROM insumos i
LEFT JOIN tiposinventario ti ON ti.Id = i.TipoInventarioId
WHERE NOT EXISTS (
    SELECT 1
    FROM logistica_inventario_item ii
    WHERE ii.InsumoOrigenId = i.Id
);");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "logistica_inventario_item");
        }
    }
}
