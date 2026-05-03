using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using MundoVs.Infrastructure.Data;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(CrmDbContext))]
    [Migration("20260503030000_RrhhHorasTrabajadasNetasSchemaRepair")]
    public partial class RrhhHorasTrabajadasNetasSchemaRepair : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
ALTER TABLE `nominadetalles`
ADD COLUMN IF NOT EXISTS `HorasTrabajadasNetas` decimal(18,2) NOT NULL DEFAULT 0.00;");

            migrationBuilder.Sql(@"
ALTER TABLE `prenominadetalles`
ADD COLUMN IF NOT EXISTS `HorasTrabajadasNetas` decimal(18,2) NOT NULL DEFAULT 0.00;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
ALTER TABLE `nominadetalles`
DROP COLUMN IF EXISTS `HorasTrabajadasNetas`;");

            migrationBuilder.Sql(@"
ALTER TABLE `prenominadetalles`
DROP COLUMN IF EXISTS `HorasTrabajadasNetas`;");
        }
    }
}
