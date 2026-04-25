using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class PrenominaDetalleSnapshotOperativo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "FactorPagoTiempoExtra",
                table: "PrenominaDetalles",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "HorasBancoAcumuladas",
                table: "PrenominaDetalles",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "HorasBancoConsumidas",
                table: "PrenominaDetalles",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "HorasDescansoNoPagado",
                table: "PrenominaDetalles",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "HorasDescansoPagado",
                table: "PrenominaDetalles",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "HorasDescansoTomado",
                table: "PrenominaDetalles",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "HorasExtraBase",
                table: "PrenominaDetalles",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FactorPagoTiempoExtra",
                table: "PrenominaDetalles");

            migrationBuilder.DropColumn(
                name: "HorasBancoAcumuladas",
                table: "PrenominaDetalles");

            migrationBuilder.DropColumn(
                name: "HorasBancoConsumidas",
                table: "PrenominaDetalles");

            migrationBuilder.DropColumn(
                name: "HorasDescansoNoPagado",
                table: "PrenominaDetalles");

            migrationBuilder.DropColumn(
                name: "HorasDescansoPagado",
                table: "PrenominaDetalles");

            migrationBuilder.DropColumn(
                name: "HorasDescansoTomado",
                table: "PrenominaDetalles");

            migrationBuilder.DropColumn(
                name: "HorasExtraBase",
                table: "PrenominaDetalles");
        }
    }
}
