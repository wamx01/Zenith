using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class NeteoSalidaAnticipadaBasePagada : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MinutosSalidaAnticipadaAbsorbidoExtra",
                table: "rrhh_resolucion_tiempo_extra_periodo",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MinutosSalidaAnticipadaDetectado",
                table: "rrhh_resolucion_tiempo_extra_periodo",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MinutosSalidaAnticipadaAbsorbidoExtra",
                table: "rrhh_resolucion_tiempo_extra_periodo");

            migrationBuilder.DropColumn(
                name: "MinutosSalidaAnticipadaDetectado",
                table: "rrhh_resolucion_tiempo_extra_periodo");
        }
    }
}
