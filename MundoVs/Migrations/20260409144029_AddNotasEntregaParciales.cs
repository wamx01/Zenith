using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class AddNotasEntregaParciales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "NotaEntregaId",
                table: "PagosRecibidos",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "NotaEntregaId",
                table: "Facturas",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateTable(
                name: "NotasEntrega",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EmpresaId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ClienteId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PedidoId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    NumeroNota = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FechaNota = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Estatus = table.Column<int>(type: "int", nullable: false),
                    Observaciones = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Impuestos = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("PK_NotasEntrega", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotasEntrega_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NotasEntrega_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NotasEntrega_Pedidos_PedidoId",
                        column: x => x.PedidoId,
                        principalTable: "Pedidos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "NotasEntregaDetalle",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EmpresaId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    NotaEntregaId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PedidoDetalleId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    Descripcion = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Cantidad = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Importe = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("PK_NotasEntregaDetalle", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotasEntregaDetalle_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NotasEntregaDetalle_NotasEntrega_NotaEntregaId",
                        column: x => x.NotaEntregaId,
                        principalTable: "NotasEntrega",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NotasEntregaDetalle_PedidoDetalles_PedidoDetalleId",
                        column: x => x.PedidoDetalleId,
                        principalTable: "PedidoDetalles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_PagosRecibidos_NotaEntregaId",
                table: "PagosRecibidos",
                column: "NotaEntregaId");

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_NotaEntregaId",
                table: "Facturas",
                column: "NotaEntregaId");

            migrationBuilder.CreateIndex(
                name: "IX_NotasEntrega_ClienteId",
                table: "NotasEntrega",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_NotasEntrega_EmpresaId_NumeroNota",
                table: "NotasEntrega",
                columns: new[] { "EmpresaId", "NumeroNota" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NotasEntrega_FechaNota",
                table: "NotasEntrega",
                column: "FechaNota");

            migrationBuilder.CreateIndex(
                name: "IX_NotasEntrega_PedidoId",
                table: "NotasEntrega",
                column: "PedidoId");

            migrationBuilder.CreateIndex(
                name: "IX_NotasEntregaDetalle_EmpresaId",
                table: "NotasEntregaDetalle",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_NotasEntregaDetalle_NotaEntregaId",
                table: "NotasEntregaDetalle",
                column: "NotaEntregaId");

            migrationBuilder.CreateIndex(
                name: "IX_NotasEntregaDetalle_PedidoDetalleId",
                table: "NotasEntregaDetalle",
                column: "PedidoDetalleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Facturas_NotasEntrega_NotaEntregaId",
                table: "Facturas",
                column: "NotaEntregaId",
                principalTable: "NotasEntrega",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_PagosRecibidos_NotasEntrega_NotaEntregaId",
                table: "PagosRecibidos",
                column: "NotaEntregaId",
                principalTable: "NotasEntrega",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Facturas_NotasEntrega_NotaEntregaId",
                table: "Facturas");

            migrationBuilder.DropForeignKey(
                name: "FK_PagosRecibidos_NotasEntrega_NotaEntregaId",
                table: "PagosRecibidos");

            migrationBuilder.DropTable(
                name: "NotasEntregaDetalle");

            migrationBuilder.DropTable(
                name: "NotasEntrega");

            migrationBuilder.DropIndex(
                name: "IX_PagosRecibidos_NotaEntregaId",
                table: "PagosRecibidos");

            migrationBuilder.DropIndex(
                name: "IX_Facturas_NotaEntregaId",
                table: "Facturas");

            migrationBuilder.DropColumn(
                name: "NotaEntregaId",
                table: "PagosRecibidos");

            migrationBuilder.DropColumn(
                name: "NotaEntregaId",
                table: "Facturas");
        }
    }
}
