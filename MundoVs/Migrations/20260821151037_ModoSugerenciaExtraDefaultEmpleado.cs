using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class ModoSugerenciaExtraDefaultEmpleado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ModoSugerenciaExtraDefault",
                table: "empleados",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ModoSugerenciaExtraDefault",
                table: "empleados");
        }
    }
}
