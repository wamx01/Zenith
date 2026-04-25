using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class ReglasVariacionClienteYCotizacionVariantePrecio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ClienteId",
                table: "CotizacionesSerigrafia",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateTable(
                name: "ClientesReglasVariacionPrecio",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EmpresaId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ClienteId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Dimension = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Valor = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    PermiteVariacionPrecio = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    PorcentajeVariacionSugerido = table.Column<decimal>(type: "decimal(9,4)", precision: 9, scale: 4, nullable: false),
                    Activa = table.Column<bool>(type: "tinyint(1)", nullable: false),
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
                    table.PrimaryKey("PK_ClientesReglasVariacionPrecio", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientesReglasVariacionPrecio_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClientesReglasVariacionPrecio_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CotizacionVariantePrecios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CotizacionSerigrafiaId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ProductoVarianteId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    Etiqueta = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Talla = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Color = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EsPrecioBase = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    PorcentajeVariacionAplicado = table.Column<decimal>(type: "decimal(9,4)", precision: 9, scale: 4, nullable: false),
                    PrecioContado = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PrecioCredito = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Aceptada = table.Column<bool>(type: "tinyint(1)", nullable: false),
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
                    table.PrimaryKey("PK_CotizacionVariantePrecios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CotizacionVariantePrecios_CotizacionesSerigrafia_CotizacionS~",
                        column: x => x.CotizacionSerigrafiaId,
                        principalTable: "CotizacionesSerigrafia",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CotizacionVariantePrecios_ProductosVariantes_ProductoVariant~",
                        column: x => x.ProductoVarianteId,
                        principalTable: "ProductosVariantes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_CotizacionesSerigrafia_ClienteId",
                table: "CotizacionesSerigrafia",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientesReglasVariacionPrecio_ClienteId",
                table: "ClientesReglasVariacionPrecio",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientesReglasVariacionPrecio_ClienteId_Dimension_Valor_Orden",
                table: "ClientesReglasVariacionPrecio",
                columns: new[] { "ClienteId", "Dimension", "Valor", "Orden" });

            migrationBuilder.CreateIndex(
                name: "IX_ClientesReglasVariacionPrecio_EmpresaId",
                table: "ClientesReglasVariacionPrecio",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_CotizacionVariantePrecios_CotizacionSerigrafiaId",
                table: "CotizacionVariantePrecios",
                column: "CotizacionSerigrafiaId");

            migrationBuilder.CreateIndex(
                name: "IX_CotizacionVariantePrecios_ProductoVarianteId",
                table: "CotizacionVariantePrecios",
                column: "ProductoVarianteId");

            migrationBuilder.AddForeignKey(
                name: "FK_CotizacionesSerigrafia_Clientes_ClienteId",
                table: "CotizacionesSerigrafia",
                column: "ClienteId",
                principalTable: "Clientes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CotizacionesSerigrafia_Clientes_ClienteId",
                table: "CotizacionesSerigrafia");

            migrationBuilder.DropTable(
                name: "ClientesReglasVariacionPrecio");

            migrationBuilder.DropTable(
                name: "CotizacionVariantePrecios");

            migrationBuilder.DropIndex(
                name: "IX_CotizacionesSerigrafia_ClienteId",
                table: "CotizacionesSerigrafia");

            migrationBuilder.DropColumn(
                name: "ClienteId",
                table: "CotizacionesSerigrafia");
        }
    }
}
