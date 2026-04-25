using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class RenombrarTintasAMateriasPrimas_AgregarCatalogos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Renombrar tabla Tintas ? MateriasPrimas (preserva datos)
            migrationBuilder.RenameTable(name: "Tintas", newName: "MateriasPrimas");

            // 2. Renombrar columna Tipo ? TipoMateriaPrima
            migrationBuilder.RenameColumn(name: "Tipo", table: "MateriasPrimas", newName: "TipoMateriaPrima");

            // 3. Renombrar PK e índices
            migrationBuilder.RenameIndex(name: "IX_Tintas_Codigo", table: "MateriasPrimas", newName: "IX_MateriasPrimas_Codigo");

            // 4. Agregar columnas nuevas a MateriasPrimas
            migrationBuilder.AddColumn<decimal>(
                name: "PrecioUnitario",
                table: "MateriasPrimas",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            // 5. Agregar índice por TipoMateriaPrima
            migrationBuilder.CreateIndex(
                name: "IX_MateriasPrimas_TipoMateriaPrima",
                table: "MateriasPrimas",
                column: "TipoMateriaPrima");

            // 6. Crear tabla ActividadesManoObra
            migrationBuilder.CreateTable(
                name: "ActividadesManoObra",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Nombre = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SueldoSugeridoSemanal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_ActividadesManoObra", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ActividadesManoObra_Nombre",
                table: "ActividadesManoObra",
                column: "Nombre");

            // 7. Crear tabla GastosFijos
            migrationBuilder.CreateTable(
                name: "GastosFijos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Concepto = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CostoMensual = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_GastosFijos", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_GastosFijos_Concepto",
                table: "GastosFijos",
                column: "Concepto");

            // 8. Crear tabla Insumos
            migrationBuilder.CreateTable(
                name: "Insumos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Codigo = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Nombre = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TipoInsumo = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_Insumos", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Insumos_Codigo",
                table: "Insumos",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Insumos_TipoInsumo",
                table: "Insumos",
                column: "TipoInsumo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "ActividadesManoObra");
            migrationBuilder.DropTable(name: "GastosFijos");
            migrationBuilder.DropTable(name: "Insumos");

            migrationBuilder.DropColumn(name: "PrecioUnitario", table: "MateriasPrimas");
            migrationBuilder.DropIndex(name: "IX_MateriasPrimas_TipoMateriaPrima", table: "MateriasPrimas");

            migrationBuilder.RenameColumn(name: "TipoMateriaPrima", table: "MateriasPrimas", newName: "Tipo");
            migrationBuilder.RenameIndex(name: "IX_MateriasPrimas_Codigo", table: "MateriasPrimas", newName: "IX_Tintas_Codigo");
            migrationBuilder.RenameTable(name: "MateriasPrimas", newName: "Tintas");
        }
    }
}
