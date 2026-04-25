using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class AgregarIsrSubsidioProvisionesZonaFrontera : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AguinaldoProvision",
                table: "NominaDetalles",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PrimaVacacionalProvision",
                table: "NominaDetalles",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "RetencionIsr",
                table: "NominaDetalles",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SubsidioEmpleo",
                table: "NominaDetalles",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "AplicaSalarioMinimoFrontera",
                table: "Empresas",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AplicaIsr",
                table: "Empleados",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AguinaldoProvision",
                table: "NominaDetalles");

            migrationBuilder.DropColumn(
                name: "PrimaVacacionalProvision",
                table: "NominaDetalles");

            migrationBuilder.DropColumn(
                name: "RetencionIsr",
                table: "NominaDetalles");

            migrationBuilder.DropColumn(
                name: "SubsidioEmpleo",
                table: "NominaDetalles");

            migrationBuilder.DropColumn(
                name: "AplicaSalarioMinimoFrontera",
                table: "Empresas");

            migrationBuilder.DropColumn(
                name: "AplicaIsr",
                table: "Empleados");
        }
    }
}
