using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class EmpleadoNumeroEmpleado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NumeroEmpleado",
                table: "Empleados",
                type: "varchar(30)",
                maxLength: 30,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.Sql(@"
UPDATE `Empleados`
SET `NumeroEmpleado` = CASE
    WHEN `Codigo` IS NOT NULL AND TRIM(`Codigo`) <> '' THEN LEFT(TRIM(`Codigo`), 30)
    ELSE CONCAT('EMP-', UPPER(LEFT(REPLACE(`Id`, '-', ''), 10)))
END
WHERE `NumeroEmpleado` IS NULL OR TRIM(`NumeroEmpleado`) = '';
");

            migrationBuilder.AlterColumn<string>(
                name: "NumeroEmpleado",
                table: "Empleados",
                type: "varchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(30)",
                oldMaxLength: 30,
                oldNullable: true)
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Empleados_EmpresaId_NumeroEmpleado",
                table: "Empleados",
                columns: new[] { "EmpresaId", "NumeroEmpleado" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Empleados_EmpresaId_NumeroEmpleado",
                table: "Empleados");

            migrationBuilder.DropColumn(
                name: "NumeroEmpleado",
                table: "Empleados");
        }
    }
}
