using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddModoSugerenciaExtra : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ModoSugerenciaExtra",
                table: "rrhh_asistencia",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ModoSugerenciaExtra",
                table: "rrhh_asistencia");
        }
    }
}
