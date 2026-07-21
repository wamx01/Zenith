using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class RrhhResolucionPeriodoRetardoAbsorbido : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MinutosRetardoAbsorbidoExtra",
                table: "rrhh_resolucion_tiempo_extra_periodo",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MinutosRetardoAbsorbidoExtra",
                table: "rrhh_resolucion_tiempo_extra_periodo");
        }
    }
}
