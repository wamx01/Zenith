using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class RrhhAsistenciaResolucionTiempoBanco : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MinutosCubiertosBancoHoras",
                table: "rrhh_asistencia",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MinutosExtraAutorizadosBanco",
                table: "rrhh_asistencia",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MinutosExtraAutorizadosPago",
                table: "rrhh_asistencia",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ResolucionTiempoExtra",
                table: "rrhh_asistencia",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MinutosCubiertosBancoHoras",
                table: "rrhh_asistencia");

            migrationBuilder.DropColumn(
                name: "MinutosExtraAutorizadosBanco",
                table: "rrhh_asistencia");

            migrationBuilder.DropColumn(
                name: "MinutosExtraAutorizadosPago",
                table: "rrhh_asistencia");

            migrationBuilder.DropColumn(
                name: "ResolucionTiempoExtra",
                table: "rrhh_asistencia");
        }
    }
}
