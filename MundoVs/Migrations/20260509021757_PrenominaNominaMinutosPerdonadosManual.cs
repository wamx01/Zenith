using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class PrenominaNominaMinutosPerdonadosManual : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MinutosPerdonadosManual",
                table: "prenominadetalles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MinutosPerdonadosManual",
                table: "nominadetalles",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MinutosPerdonadosManual",
                table: "prenominadetalles");

            migrationBuilder.DropColumn(
                name: "MinutosPerdonadosManual",
                table: "nominadetalles");
        }
    }
}
