using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class RrhhMarcacionCorreccionOperativa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClasificacionOperativa",
                table: "rrhh_marcacion",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "EsAnulada",
                table: "rrhh_marcacion",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EsManual",
                table: "rrhh_marcacion",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ObservacionManual",
                table: "rrhh_marcacion",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClasificacionOperativa",
                table: "rrhh_marcacion");

            migrationBuilder.DropColumn(
                name: "EsAnulada",
                table: "rrhh_marcacion");

            migrationBuilder.DropColumn(
                name: "EsManual",
                table: "rrhh_marcacion");

            migrationBuilder.DropColumn(
                name: "ObservacionManual",
                table: "rrhh_marcacion");
        }
    }
}
