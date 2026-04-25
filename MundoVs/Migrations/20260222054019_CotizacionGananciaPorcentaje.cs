using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class CotizacionGananciaPorcentaje : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
SET @col_min := (
    SELECT COUNT(*)
    FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'CotizacionDetalles'
      AND COLUMN_NAME = 'GananciaMinuto'
);
SET @col_pct := (
    SELECT COUNT(*)
    FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'CotizacionDetalles'
      AND COLUMN_NAME = 'GananciaPorcentaje'
);
SET @sql := IF(@col_min = 1 AND @col_pct = 0,
    'ALTER TABLE `CotizacionDetalles` CHANGE COLUMN `GananciaMinuto` `GananciaPorcentaje` decimal(18,4) NULL',
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
SET @col_min := (
    SELECT COUNT(*)
    FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'CotizacionDetalles'
      AND COLUMN_NAME = 'GananciaMinuto'
);
SET @col_pct := (
    SELECT COUNT(*)
    FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'CotizacionDetalles'
      AND COLUMN_NAME = 'GananciaPorcentaje'
);
SET @sql := IF(@col_pct = 1 AND @col_min = 0,
    'ALTER TABLE `CotizacionDetalles` CHANGE COLUMN `GananciaPorcentaje` `GananciaMinuto` decimal(18,4) NULL',
    'SELECT 1'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");
        }
    }
}
