using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class FactorTiempoExtraAplicado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "FactorTiempoExtraAplicado",
                table: "rrhh_asistencia",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FactorPagoTiempoExtra",
                table: "nominadetalles",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FactorTiempoExtraAplicado",
                table: "rrhh_asistencia");

            migrationBuilder.DropColumn(
                name: "FactorPagoTiempoExtra",
                table: "nominadetalles");
        }
    }
}
