using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class PagoFijoPorLaborJornada : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EsPagoFijoPorLabor",
                table: "rrhh_asistencia",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PagoFijoPorLabor",
                table: "empleadosesquemajornada",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EsPagoFijoPorLabor",
                table: "rrhh_asistencia");

            migrationBuilder.DropColumn(
                name: "PagoFijoPorLabor",
                table: "empleadosesquemajornada");
        }
    }
}
