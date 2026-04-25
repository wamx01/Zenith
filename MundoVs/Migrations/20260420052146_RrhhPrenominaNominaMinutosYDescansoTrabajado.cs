using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class RrhhPrenominaNominaMinutosYDescansoTrabajado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DiasConMarcacion",
                table: "PrenominaDetalles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MinutosDescuentoManual",
                table: "PrenominaDetalles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MinutosRetardo",
                table: "PrenominaDetalles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MinutosSalidaAnticipada",
                table: "PrenominaDetalles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DiasConMarcacion",
                table: "NominaDetalles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MinutosDescuentoManual",
                table: "NominaDetalles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MinutosRetardo",
                table: "NominaDetalles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MinutosSalidaAnticipada",
                table: "NominaDetalles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "MontoDescansoTrabajado",
                table: "NominaDetalles",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MontoDescuentoMinutos",
                table: "NominaDetalles",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiasConMarcacion",
                table: "PrenominaDetalles");

            migrationBuilder.DropColumn(
                name: "MinutosDescuentoManual",
                table: "PrenominaDetalles");

            migrationBuilder.DropColumn(
                name: "MinutosRetardo",
                table: "PrenominaDetalles");

            migrationBuilder.DropColumn(
                name: "MinutosSalidaAnticipada",
                table: "PrenominaDetalles");

            migrationBuilder.DropColumn(
                name: "DiasConMarcacion",
                table: "NominaDetalles");

            migrationBuilder.DropColumn(
                name: "MinutosDescuentoManual",
                table: "NominaDetalles");

            migrationBuilder.DropColumn(
                name: "MinutosRetardo",
                table: "NominaDetalles");

            migrationBuilder.DropColumn(
                name: "MinutosSalidaAnticipada",
                table: "NominaDetalles");

            migrationBuilder.DropColumn(
                name: "MontoDescansoTrabajado",
                table: "NominaDetalles");

            migrationBuilder.DropColumn(
                name: "MontoDescuentoMinutos",
                table: "NominaDetalles");
        }
    }
}
