using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class PedidoVariantesSku : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ProductoVarianteId",
                table: "PedidosDetalleTalla",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "ProductoVarianteId",
                table: "PedidoDetalles",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<string>(
                name: "VariacionValor",
                table: "PedidoDetalles",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_PedidosDetalleTalla_ProductoVarianteId",
                table: "PedidosDetalleTalla",
                column: "ProductoVarianteId");

            migrationBuilder.CreateIndex(
                name: "IX_PedidoDetalles_ProductoVarianteId",
                table: "PedidoDetalles",
                column: "ProductoVarianteId");

            migrationBuilder.AddForeignKey(
                name: "FK_PedidoDetalles_ProductosVariantes_ProductoVarianteId",
                table: "PedidoDetalles",
                column: "ProductoVarianteId",
                principalTable: "ProductosVariantes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_PedidosDetalleTalla_ProductosVariantes_ProductoVarianteId",
                table: "PedidosDetalleTalla",
                column: "ProductoVarianteId",
                principalTable: "ProductosVariantes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PedidoDetalles_ProductosVariantes_ProductoVarianteId",
                table: "PedidoDetalles");

            migrationBuilder.DropForeignKey(
                name: "FK_PedidosDetalleTalla_ProductosVariantes_ProductoVarianteId",
                table: "PedidosDetalleTalla");

            migrationBuilder.DropIndex(
                name: "IX_PedidosDetalleTalla_ProductoVarianteId",
                table: "PedidosDetalleTalla");

            migrationBuilder.DropIndex(
                name: "IX_PedidoDetalles_ProductoVarianteId",
                table: "PedidoDetalles");

            migrationBuilder.DropColumn(
                name: "ProductoVarianteId",
                table: "PedidosDetalleTalla");

            migrationBuilder.DropColumn(
                name: "ProductoVarianteId",
                table: "PedidoDetalles");

            migrationBuilder.DropColumn(
                name: "VariacionValor",
                table: "PedidoDetalles");
        }
    }
}
