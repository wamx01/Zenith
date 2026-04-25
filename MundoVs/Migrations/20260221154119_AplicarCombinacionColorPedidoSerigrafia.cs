using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class AplicarCombinacionColorPedidoSerigrafia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Referencia",
                table: "PedidosSerigrafia");

            migrationBuilder.RenameColumn(
                name: "Combinacion",
                table: "PedidosSerigrafia",
                newName: "CombinacionColor");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CombinacionColor",
                table: "PedidosSerigrafia",
                newName: "Combinacion");

            migrationBuilder.AddColumn<string>(
                name: "Referencia",
                table: "PedidosSerigrafia",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
