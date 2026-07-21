using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class F4bPrenominaDetallePorHoras : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DiasFestivoTrabajadoFija",
                table: "prenominadetalles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DiasPorHorasTrabajados",
                table: "prenominadetalles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MinutosPorHorasFestivoNetos",
                table: "prenominadetalles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MinutosPorHorasNetos",
                table: "prenominadetalles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Backfill: los detalles existentes son pre-PorHoras, así que los festivos trabajados
            // son todos Fija. Los agregados PorHoras quedan en 0 (no había esquema PorHoras antes).
            migrationBuilder.Sql("UPDATE prenominadetalles SET DiasFestivoTrabajadoFija = DiasFestivoTrabajado;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiasFestivoTrabajadoFija",
                table: "prenominadetalles");

            migrationBuilder.DropColumn(
                name: "DiasPorHorasTrabajados",
                table: "prenominadetalles");

            migrationBuilder.DropColumn(
                name: "MinutosPorHorasFestivoNetos",
                table: "prenominadetalles");

            migrationBuilder.DropColumn(
                name: "MinutosPorHorasNetos",
                table: "prenominadetalles");
        }
    }
}
