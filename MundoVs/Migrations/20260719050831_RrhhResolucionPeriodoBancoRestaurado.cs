using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class RrhhResolucionPeriodoBancoRestaurado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MinutosBancoConsumidoDetectado",
                table: "rrhh_resolucion_tiempo_extra_periodo",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MinutosBancoRestauradoExtra",
                table: "rrhh_resolucion_tiempo_extra_periodo",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MinutosBancoConsumidoDetectado",
                table: "rrhh_resolucion_tiempo_extra_periodo");

            migrationBuilder.DropColumn(
                name: "MinutosBancoRestauradoExtra",
                table: "rrhh_resolucion_tiempo_extra_periodo");
        }
    }
}
