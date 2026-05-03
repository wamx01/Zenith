using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class NotaEntregaMultiPedido : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "contabilidad_nota_entrega_pedido",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EmpresaId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    NotaEntregaId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PedidoId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Orden = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    EsPrincipal = table.Column<bool>(type: "tinyint(1)", nullable: false),
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
                    table.PrimaryKey("PK_contabilidad_nota_entrega_pedido", x => x.Id);
                    table.ForeignKey(
                        name: "FK_contabilidad_nota_entrega_pedido_empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_contabilidad_nota_entrega_pedido_notasentrega_NotaEntregaId",
                        column: x => x.NotaEntregaId,
                        principalTable: "notasentrega",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_contabilidad_nota_entrega_pedido_pedidos_PedidoId",
                        column: x => x.PedidoId,
                        principalTable: "pedidos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_contabilidad_nota_entrega_pedido_EmpresaId",
                table: "contabilidad_nota_entrega_pedido",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_contabilidad_nota_entrega_pedido_NotaEntregaId_PedidoId",
                table: "contabilidad_nota_entrega_pedido",
                columns: new[] { "NotaEntregaId", "PedidoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_contabilidad_nota_entrega_pedido_PedidoId_EsPrincipal",
                table: "contabilidad_nota_entrega_pedido",
                columns: new[] { "PedidoId", "EsPrincipal" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "contabilidad_nota_entrega_pedido");
        }
    }
}
