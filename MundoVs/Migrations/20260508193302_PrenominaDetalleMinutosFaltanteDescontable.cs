using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class PrenominaDetalleMinutosFaltanteDescontable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MinutosFaltanteDescontable",
                table: "prenominadetalles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MinutosFaltanteDescontable",
                table: "nominadetalles",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MinutosFaltanteDescontable",
                table: "prenominadetalles");

            migrationBuilder.DropColumn(
                name: "MinutosFaltanteDescontable",
                table: "nominadetalles");
        }
    }
}
