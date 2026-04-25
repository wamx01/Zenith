using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class NominaHorasExtraLegalSplit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "HorasExtraBanco",
                table: "NominaDetalles",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "HorasExtraBase",
                table: "NominaDetalles",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "HorasExtraDobles",
                table: "NominaDetalles",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "HorasExtraTriples",
                table: "NominaDetalles",
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
                name: "HorasExtraBanco",
                table: "NominaDetalles");

            migrationBuilder.DropColumn(
                name: "HorasExtraBase",
                table: "NominaDetalles");

            migrationBuilder.DropColumn(
                name: "HorasExtraDobles",
                table: "NominaDetalles");

            migrationBuilder.DropColumn(
                name: "HorasExtraTriples",
                table: "NominaDetalles");
        }
    }
}
