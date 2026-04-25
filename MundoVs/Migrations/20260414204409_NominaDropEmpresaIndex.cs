using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class NominaDropEmpresaIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DROP INDEX IF EXISTS `IX_Nominas_EmpresaId` ON `Nominas`;
");

            migrationBuilder.AlterColumn<string>(
                name: "Folio",
                table: "Nominas",
                type: "varchar(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.Sql(@"
SET @idx_exists := (
    SELECT COUNT(1)
    FROM information_schema.statistics
    WHERE table_schema = DATABASE()
      AND table_name = 'Nominas'
      AND index_name = 'IX_Nominas_EmpresaId_Folio');
SET @sql := IF(@idx_exists = 0,
    'CREATE UNIQUE INDEX `IX_Nominas_EmpresaId_Folio` ON `Nominas` (`EmpresaId`, `Folio`);',
    'SELECT 1;');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Nominas_EmpresaId_Folio",
                table: "Nominas");

            migrationBuilder.AlterColumn<string>(
                name: "Folio",
                table: "Nominas",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(30)",
                oldMaxLength: 30,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Nominas_EmpresaId",
                table: "Nominas",
                column: "EmpresaId");
        }
    }
}
