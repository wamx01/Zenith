using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class CodigosCortosYFoliosNegocio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Folio",
                table: "Prenominas",
                type: "varchar(30)",
                maxLength: 30,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Folio",
                table: "Nominas",
                type: "varchar(30)",
                maxLength: 30,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.Sql(@"
UPDATE `Prenominas`
SET `Folio` = CONCAT('PRE-', DATE_FORMAT(`FechaInicio`, '%Y%m'), '-', UPPER(LEFT(REPLACE(`Id`, '-', ''), 8)))
WHERE `Folio` IS NULL OR TRIM(`Folio`) = '';
");

            migrationBuilder.Sql(@"
UPDATE `Nominas`
SET `Folio` = CONCAT('NOM-', DATE_FORMAT(`FechaInicio`, '%Y%m'), '-', UPPER(LEFT(REPLACE(`Id`, '-', ''), 8)))
WHERE `Folio` IS NULL OR TRIM(`Folio`) = '';
");

            migrationBuilder.CreateIndex(
                name: "IX_Nominas_EmpresaId_Folio",
                table: "Nominas",
                columns: new[] { "EmpresaId", "Folio" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Prenominas_EmpresaId_Folio",
                table: "Prenominas",
                columns: new[] { "EmpresaId", "Folio" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Nominas_EmpresaId_Folio",
                table: "Nominas");

            migrationBuilder.DropIndex(
                name: "IX_Prenominas_EmpresaId_Folio",
                table: "Prenominas");

            migrationBuilder.DropColumn(
                name: "Folio",
                table: "Prenominas");

            migrationBuilder.DropColumn(
                name: "Folio",
                table: "Nominas");
        }
    }
}
