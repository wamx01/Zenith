using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class CotizacionPorProducto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
SET @fk := (
    SELECT CONSTRAINT_NAME
    FROM information_schema.KEY_COLUMN_USAGE
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'CotizacionesSerigrafia'
      AND CONSTRAINT_NAME = 'FK_CotizacionesSerigrafia_PedidosSerigrafia_PedidoSerigrafiaId'
    LIMIT 1
);
SET @sql := IF(@fk IS NULL, 'SELECT 1', 'ALTER TABLE `CotizacionesSerigrafia` DROP FOREIGN KEY `FK_CotizacionesSerigrafia_PedidosSerigrafia_PedidoSerigrafiaId`');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");

            migrationBuilder.Sql(@"
SET @col_pedido := (
    SELECT COUNT(*)
    FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'CotizacionesSerigrafia'
      AND COLUMN_NAME = 'PedidoSerigrafiaId'
);
SET @col_producto := (
    SELECT COUNT(*)
    FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'CotizacionesSerigrafia'
      AND COLUMN_NAME = 'ProductoId'
);
SET @sql := IF(@col_pedido = 1 AND @col_producto = 0,
    'ALTER TABLE `CotizacionesSerigrafia` CHANGE COLUMN `PedidoSerigrafiaId` `ProductoId` char(36) COLLATE ascii_general_ci NOT NULL',
    'SELECT 1'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @idx_pedido := (
    SELECT COUNT(*)
    FROM information_schema.STATISTICS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'CotizacionesSerigrafia'
      AND INDEX_NAME = 'IX_CotizacionesSerigrafia_PedidoSerigrafiaId'
);
SET @idx_producto := (
    SELECT COUNT(*)
    FROM information_schema.STATISTICS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'CotizacionesSerigrafia'
      AND INDEX_NAME = 'IX_CotizacionesSerigrafia_ProductoId'
);
SET @sql := IF(@idx_pedido = 1,
    'DROP INDEX `IX_CotizacionesSerigrafia_PedidoSerigrafiaId` ON `CotizacionesSerigrafia`',
    'SELECT 1'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @sql := IF(@idx_producto = 0,
    'CREATE INDEX `IX_CotizacionesSerigrafia_ProductoId` ON `CotizacionesSerigrafia` (`ProductoId`)',
    'SELECT 1'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");

            migrationBuilder.Sql(@"
SET @col_desc := (
    SELECT COUNT(*)
    FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'CotizacionesSerigrafia'
      AND COLUMN_NAME = 'Descripcion'
);
SET @sql := IF(@col_desc = 0,
    'ALTER TABLE `CotizacionesSerigrafia` ADD `Descripcion` varchar(500) CHARACTER SET utf8mb4 NULL',
    'SELECT 1'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @col_np := (
    SELECT COUNT(*)
    FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'CotizacionesSerigrafia'
      AND COLUMN_NAME = 'NumeroPersonas'
);
SET @sql := IF(@col_np = 0,
    'ALTER TABLE `CotizacionesSerigrafia` ADD `NumeroPersonas` int NOT NULL DEFAULT 0',
    'SELECT 1'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");


            migrationBuilder.AddForeignKey(
                name: "FK_CotizacionesSerigrafia_Productos_ProductoId",
                table: "CotizacionesSerigrafia",
                column: "ProductoId",
                principalTable: "Productos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
SET @fk := (
    SELECT CONSTRAINT_NAME
    FROM information_schema.KEY_COLUMN_USAGE
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'CotizacionesSerigrafia'
      AND CONSTRAINT_NAME = 'FK_CotizacionesSerigrafia_Productos_ProductoId'
    LIMIT 1
);
SET @sql := IF(@fk IS NULL, 'SELECT 1', 'ALTER TABLE `CotizacionesSerigrafia` DROP FOREIGN KEY `FK_CotizacionesSerigrafia_Productos_ProductoId`');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");


            migrationBuilder.Sql(@"
ALTER TABLE `CotizacionesSerigrafia`
    DROP COLUMN IF EXISTS `Descripcion`,
    DROP COLUMN IF EXISTS `NumeroPersonas`;
");

            migrationBuilder.RenameColumn(
                name: "ProductoId",
                table: "CotizacionesSerigrafia",
                newName: "PedidoSerigrafiaId");

            migrationBuilder.RenameIndex(
                name: "IX_CotizacionesSerigrafia_ProductoId",
                table: "CotizacionesSerigrafia",
                newName: "IX_CotizacionesSerigrafia_PedidoSerigrafiaId");


            migrationBuilder.AddForeignKey(
                name: "FK_CotizacionesSerigrafia_PedidosSerigrafia_PedidoSerigrafiaId",
                table: "CotizacionesSerigrafia",
                column: "PedidoSerigrafiaId",
                principalTable: "PedidosSerigrafia",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
