using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class RrhhPeriodoAutomaticoNomina : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE `rrhh_nomina_corte` ADD COLUMN IF NOT EXISTS `DiaCorteMes` int NOT NULL DEFAULT 15;");
            migrationBuilder.Sql("ALTER TABLE `Prenominas` ADD COLUMN IF NOT EXISTS `AnioPeriodo` int NOT NULL DEFAULT 0;");
            migrationBuilder.Sql("ALTER TABLE `Prenominas` ADD COLUMN IF NOT EXISTS `NumeroPeriodo` int NOT NULL DEFAULT 0;");
            migrationBuilder.Sql("ALTER TABLE `Prenominas` ADD COLUMN IF NOT EXISTS `PeriodicidadPago` int NOT NULL DEFAULT 1;");
            migrationBuilder.Sql("ALTER TABLE `Nominas` ADD COLUMN IF NOT EXISTS `AnioPeriodo` int NOT NULL DEFAULT 0;");
            migrationBuilder.Sql("ALTER TABLE `Nominas` ADD COLUMN IF NOT EXISTS `NumeroPeriodo` int NOT NULL DEFAULT 0;");
            migrationBuilder.Sql("ALTER TABLE `Nominas` ADD COLUMN IF NOT EXISTS `PeriodicidadPago` int NOT NULL DEFAULT 1;");

            migrationBuilder.Sql("""
                UPDATE Prenominas
                SET PeriodicidadPago = 1,
                    AnioPeriodo = YEAR(FechaFin),
                    NumeroPeriodo = CASE
                        WHEN DAYOFYEAR(FechaFin) <= 7 THEN 1
                        ELSE CEILING(DAYOFYEAR(FechaFin) / 7)
                    END
                WHERE AnioPeriodo = 0 OR NumeroPeriodo = 0 OR PeriodicidadPago = 0;
                """);

            migrationBuilder.Sql("""
                UPDATE Nominas
                SET PeriodicidadPago = 1,
                    AnioPeriodo = YEAR(FechaFin),
                    NumeroPeriodo = CASE
                        WHEN DAYOFYEAR(FechaFin) <= 7 THEN 1
                        ELSE CEILING(DAYOFYEAR(FechaFin) / 7)
                    END
                WHERE AnioPeriodo = 0 OR NumeroPeriodo = 0 OR PeriodicidadPago = 0;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Prenominas_EmpresaId_PeriodicidadPago_AnioPeriodo_NumeroPeri~",
                table: "Prenominas",
                columns: new[] { "EmpresaId", "PeriodicidadPago", "AnioPeriodo", "NumeroPeriodo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Nominas_EmpresaId_PeriodicidadPago_AnioPeriodo_NumeroPeriodo",
                table: "Nominas",
                columns: new[] { "EmpresaId", "PeriodicidadPago", "AnioPeriodo", "NumeroPeriodo" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Prenominas_EmpresaId_PeriodicidadPago_AnioPeriodo_NumeroPeri~",
                table: "Prenominas");

            migrationBuilder.DropIndex(
                name: "IX_Nominas_EmpresaId_PeriodicidadPago_AnioPeriodo_NumeroPeriodo",
                table: "Nominas");

            migrationBuilder.Sql("ALTER TABLE `rrhh_nomina_corte` DROP COLUMN IF EXISTS `DiaCorteMes`;");
            migrationBuilder.Sql("ALTER TABLE `Prenominas` DROP COLUMN IF EXISTS `AnioPeriodo`;");
            migrationBuilder.Sql("ALTER TABLE `Prenominas` DROP COLUMN IF EXISTS `NumeroPeriodo`;");
            migrationBuilder.Sql("ALTER TABLE `Prenominas` DROP COLUMN IF EXISTS `PeriodicidadPago`;");
            migrationBuilder.Sql("ALTER TABLE `Nominas` DROP COLUMN IF EXISTS `AnioPeriodo`;");
            migrationBuilder.Sql("ALTER TABLE `Nominas` DROP COLUMN IF EXISTS `NumeroPeriodo`;");
            migrationBuilder.Sql("ALTER TABLE `Nominas` DROP COLUMN IF EXISTS `PeriodicidadPago`;");
        }
    }
}
