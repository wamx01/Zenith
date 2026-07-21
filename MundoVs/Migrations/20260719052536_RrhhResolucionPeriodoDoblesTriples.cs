using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class RrhhResolucionPeriodoDoblesTriples : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MinutosExtraDobles",
                table: "rrhh_resolucion_tiempo_extra_periodo",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MinutosExtraTriples",
                table: "rrhh_resolucion_tiempo_extra_periodo",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MinutosExtraDobles",
                table: "rrhh_resolucion_tiempo_extra_periodo");

            migrationBuilder.DropColumn(
                name: "MinutosExtraTriples",
                table: "rrhh_resolucion_tiempo_extra_periodo");
        }
    }
}
