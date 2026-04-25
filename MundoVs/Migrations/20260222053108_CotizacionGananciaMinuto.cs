using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class CotizacionGananciaMinuto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
SET @col_gan := (
    SELECT COUNT(*)
    FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'CotizacionDetalles'
      AND COLUMN_NAME = 'GananciaMinuto'
);
SET @sql := IF(@col_gan = 0,
    'ALTER TABLE `CotizacionDetalles` ADD `GananciaMinuto` decimal(18,4) NULL',
    'SELECT 1'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
ALTER TABLE `CotizacionDetalles`
    DROP COLUMN IF EXISTS `GananciaMinuto`;
");
        }
    }
}
