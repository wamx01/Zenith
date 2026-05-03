using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class NotaEntregaAsignacionBase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "contabilidad_nota_entrega_asignacion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EmpresaId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    NotaEntregaId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PedidoId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PedidoDetalleId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    PedidoDetalleTallaId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    PedidoConceptoId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    TipoOrigen = table.Column<int>(type: "int", nullable: false),
                    Cantidad = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CantidadFgTomada = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Importe = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    EsParcial = table.Column<bool>(type: "tinyint(1)", nullable: false),
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
                    table.PrimaryKey("PK_contabilidad_nota_entrega_asignacion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_contabilidad_nota_entrega_asignacion_empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_contabilidad_nota_entrega_asignacion_notasentrega_NotaEntreg~",
                        column: x => x.NotaEntregaId,
                        principalTable: "notasentrega",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_contabilidad_nota_entrega_asignacion_pedidoconceptos_PedidoC~",
                        column: x => x.PedidoConceptoId,
                        principalTable: "pedidoconceptos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_contabilidad_nota_entrega_asignacion_pedidodetalles_PedidoDe~",
                        column: x => x.PedidoDetalleId,
                        principalTable: "pedidodetalles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_contabilidad_nota_entrega_asignacion_pedidos_PedidoId",
                        column: x => x.PedidoId,
                        principalTable: "pedidos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_contabilidad_nota_entrega_asignacion_pedidosdetalletalla_Ped~",
                        column: x => x.PedidoDetalleTallaId,
                        principalTable: "pedidosdetalletalla",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_contabilidad_nota_entrega_asignacion_EmpresaId",
                table: "contabilidad_nota_entrega_asignacion",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_contabilidad_nota_entrega_asignacion_NotaEntregaId",
                table: "contabilidad_nota_entrega_asignacion",
                column: "NotaEntregaId");

            migrationBuilder.CreateIndex(
                name: "IX_contabilidad_nota_entrega_asignacion_PedidoConceptoId",
                table: "contabilidad_nota_entrega_asignacion",
                column: "PedidoConceptoId");

            migrationBuilder.CreateIndex(
                name: "IX_contabilidad_nota_entrega_asignacion_PedidoDetalleId",
                table: "contabilidad_nota_entrega_asignacion",
                column: "PedidoDetalleId");

            migrationBuilder.CreateIndex(
                name: "IX_contabilidad_nota_entrega_asignacion_PedidoDetalleTallaId",
                table: "contabilidad_nota_entrega_asignacion",
                column: "PedidoDetalleTallaId");

            migrationBuilder.CreateIndex(
                name: "IX_contabilidad_nota_entrega_asignacion_PedidoId",
                table: "contabilidad_nota_entrega_asignacion",
                column: "PedidoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "contabilidad_nota_entrega_asignacion");
        }
    }
}
