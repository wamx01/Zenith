using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class RrhhLegalFestivosPrimaDominicalInfonavit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DiasDomingoTrabajado",
                table: "PrenominaDetalles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DiasDomingoTrabajado",
                table: "NominaDetalles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "MontoFestivoTrabajado",
                table: "NominaDetalles",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MontoInfonavit",
                table: "NominaDetalles",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MontoPrimaDominical",
                table: "NominaDetalles",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "AplicaInfonavit",
                table: "Empleados",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "FactorDescuentoInfonavit",
                table: "Empleados",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "NumeroCreditoInfonavit",
                table: "Empleados",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiasDomingoTrabajado",
                table: "PrenominaDetalles");

            migrationBuilder.DropColumn(
                name: "DiasDomingoTrabajado",
                table: "NominaDetalles");

            migrationBuilder.DropColumn(
                name: "MontoFestivoTrabajado",
                table: "NominaDetalles");

            migrationBuilder.DropColumn(
                name: "MontoInfonavit",
                table: "NominaDetalles");

            migrationBuilder.DropColumn(
                name: "MontoPrimaDominical",
                table: "NominaDetalles");

            migrationBuilder.DropColumn(
                name: "AplicaInfonavit",
                table: "Empleados");

            migrationBuilder.DropColumn(
                name: "FactorDescuentoInfonavit",
                table: "Empleados");

            migrationBuilder.DropColumn(
                name: "NumeroCreditoInfonavit",
                table: "Empleados");
        }
    }
}
