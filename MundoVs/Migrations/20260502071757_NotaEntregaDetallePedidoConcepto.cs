using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class NotaEntregaDetallePedidoConcepto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PedidoConceptoId",
                table: "notasentregadetalle",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_notasentregadetalle_PedidoConceptoId",
                table: "notasentregadetalle",
                column: "PedidoConceptoId");

            migrationBuilder.AddForeignKey(
                name: "FK_notasentregadetalle_pedidoconceptos_PedidoConceptoId",
                table: "notasentregadetalle",
                column: "PedidoConceptoId",
                principalTable: "pedidoconceptos",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_notasentregadetalle_pedidoconceptos_PedidoConceptoId",
                table: "notasentregadetalle");

            migrationBuilder.DropIndex(
                name: "IX_notasentregadetalle_PedidoConceptoId",
                table: "notasentregadetalle");

            migrationBuilder.DropColumn(
                name: "PedidoConceptoId",
                table: "notasentregadetalle");
        }
    }
}
