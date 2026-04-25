using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class SimplificarTipoProcesoCostos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE `TiposProceso` DROP COLUMN IF EXISTS `CostoBase`;");
            migrationBuilder.Sql("ALTER TABLE `TiposProceso` DROP COLUMN IF EXISTS `TarifaDestajo`;");
            migrationBuilder.Sql("ALTER TABLE `TiposProceso` DROP COLUMN IF EXISTS `TarifaPorMinuto`;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CostoBase",
                table: "TiposProceso",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TarifaDestajo",
                table: "TiposProceso",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TarifaPorMinuto",
                table: "TiposProceso",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);
        }
    }
}
