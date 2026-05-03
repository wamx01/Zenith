using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class CxcCargosManuales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "contabilidad_cargo_manual_cxc",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EmpresaId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ClienteId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PedidoId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    NotaEntregaId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    FacturaId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    FechaCargo = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Referencia = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Concepto = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Notas = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
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
                    table.PrimaryKey("PK_contabilidad_cargo_manual_cxc", x => x.Id);
                    table.ForeignKey(
                        name: "FK_contabilidad_cargo_manual_cxc_clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_contabilidad_cargo_manual_cxc_empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_contabilidad_cargo_manual_cxc_facturas_FacturaId",
                        column: x => x.FacturaId,
                        principalTable: "facturas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_contabilidad_cargo_manual_cxc_notasentrega_NotaEntregaId",
                        column: x => x.NotaEntregaId,
                        principalTable: "notasentrega",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_contabilidad_cargo_manual_cxc_pedidos_PedidoId",
                        column: x => x.PedidoId,
                        principalTable: "pedidos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_contabilidad_cargo_manual_cxc_ClienteId",
                table: "contabilidad_cargo_manual_cxc",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_contabilidad_cargo_manual_cxc_EmpresaId_ClienteId_FechaCargo",
                table: "contabilidad_cargo_manual_cxc",
                columns: new[] { "EmpresaId", "ClienteId", "FechaCargo" });

            migrationBuilder.CreateIndex(
                name: "IX_contabilidad_cargo_manual_cxc_FacturaId",
                table: "contabilidad_cargo_manual_cxc",
                column: "FacturaId");

            migrationBuilder.CreateIndex(
                name: "IX_contabilidad_cargo_manual_cxc_NotaEntregaId",
                table: "contabilidad_cargo_manual_cxc",
                column: "NotaEntregaId");

            migrationBuilder.CreateIndex(
                name: "IX_contabilidad_cargo_manual_cxc_PedidoId",
                table: "contabilidad_cargo_manual_cxc",
                column: "PedidoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "contabilidad_cargo_manual_cxc");
        }
    }
}
