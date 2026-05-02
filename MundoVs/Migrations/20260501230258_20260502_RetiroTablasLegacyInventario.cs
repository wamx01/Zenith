using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class _20260502_RetiroTablasLegacyInventario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
SET @fk_name := (
    SELECT kcu.CONSTRAINT_NAME
    FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu
    WHERE kcu.TABLE_SCHEMA = DATABASE() AND kcu.TABLE_NAME = 'cotizaciondetalles' AND kcu.COLUMN_NAME = 'InsumoId' AND kcu.REFERENCED_TABLE_NAME IS NOT NULL
    LIMIT 1
);
SET @drop_fk_sql := IF(@fk_name IS NULL, 'SELECT 1', CONCAT('ALTER TABLE `cotizaciondetalles` DROP FOREIGN KEY `', @fk_name, '`'));
PREPARE drop_fk_stmt FROM @drop_fk_sql;
EXECUTE drop_fk_stmt;
DEALLOCATE PREPARE drop_fk_stmt;

SET @fk_name := (
    SELECT kcu.CONSTRAINT_NAME
    FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu
    WHERE kcu.TABLE_SCHEMA = DATABASE() AND kcu.TABLE_NAME = 'cotizaciondetalles' AND kcu.COLUMN_NAME = 'MateriaPrimaId' AND kcu.REFERENCED_TABLE_NAME IS NOT NULL
    LIMIT 1
);
SET @drop_fk_sql := IF(@fk_name IS NULL, 'SELECT 1', CONCAT('ALTER TABLE `cotizaciondetalles` DROP FOREIGN KEY `', @fk_name, '`'));
PREPARE drop_fk_stmt FROM @drop_fk_sql;
EXECUTE drop_fk_stmt;
DEALLOCATE PREPARE drop_fk_stmt;

SET @fk_name := (
    SELECT kcu.CONSTRAINT_NAME
    FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu
    WHERE kcu.TABLE_SCHEMA = DATABASE() AND kcu.TABLE_NAME = 'movimientosinventario' AND kcu.COLUMN_NAME = 'InsumoId' AND kcu.REFERENCED_TABLE_NAME IS NOT NULL
    LIMIT 1
);
SET @drop_fk_sql := IF(@fk_name IS NULL, 'SELECT 1', CONCAT('ALTER TABLE `movimientosinventario` DROP FOREIGN KEY `', @fk_name, '`'));
PREPARE drop_fk_stmt FROM @drop_fk_sql;
EXECUTE drop_fk_stmt;
DEALLOCATE PREPARE drop_fk_stmt;

SET @fk_name := (
    SELECT kcu.CONSTRAINT_NAME
    FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu
    WHERE kcu.TABLE_SCHEMA = DATABASE() AND kcu.TABLE_NAME = 'movimientosinventario' AND kcu.COLUMN_NAME = 'MateriaPrimaId' AND kcu.REFERENCED_TABLE_NAME IS NOT NULL
    LIMIT 1
);
SET @drop_fk_sql := IF(@fk_name IS NULL, 'SELECT 1', CONCAT('ALTER TABLE `movimientosinventario` DROP FOREIGN KEY `', @fk_name, '`'));
PREPARE drop_fk_stmt FROM @drop_fk_sql;
EXECUTE drop_fk_stmt;
DEALLOCATE PREPARE drop_fk_stmt;

SET @fk_name := (
    SELECT kcu.CONSTRAINT_NAME
    FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu
    WHERE kcu.TABLE_SCHEMA = DATABASE() AND kcu.TABLE_NAME = 'tiposprocesoconsumos' AND kcu.COLUMN_NAME = 'InsumoId' AND kcu.REFERENCED_TABLE_NAME IS NOT NULL
    LIMIT 1
);
SET @drop_fk_sql := IF(@fk_name IS NULL, 'SELECT 1', CONCAT('ALTER TABLE `tiposprocesoconsumos` DROP FOREIGN KEY `', @fk_name, '`'));
PREPARE drop_fk_stmt FROM @drop_fk_sql;
EXECUTE drop_fk_stmt;
DEALLOCATE PREPARE drop_fk_stmt;

SET @fk_name := (
    SELECT kcu.CONSTRAINT_NAME
    FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu
    WHERE kcu.TABLE_SCHEMA = DATABASE() AND kcu.TABLE_NAME = 'tiposprocesoconsumos' AND kcu.COLUMN_NAME = 'MateriaPrimaId' AND kcu.REFERENCED_TABLE_NAME IS NOT NULL
    LIMIT 1
);
SET @drop_fk_sql := IF(@fk_name IS NULL, 'SELECT 1', CONCAT('ALTER TABLE `tiposprocesoconsumos` DROP FOREIGN KEY `', @fk_name, '`'));
PREPARE drop_fk_stmt FROM @drop_fk_sql;
EXECUTE drop_fk_stmt;
DEALLOCATE PREPARE drop_fk_stmt;

SET @fk_name := (
    SELECT kcu.CONSTRAINT_NAME
    FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu
    WHERE kcu.TABLE_SCHEMA = DATABASE() AND kcu.TABLE_NAME = 'insumos' AND kcu.COLUMN_NAME = 'EmpresaId' AND kcu.REFERENCED_TABLE_NAME IS NOT NULL
    LIMIT 1
);
SET @drop_fk_sql := IF(@fk_name IS NULL, 'SELECT 1', CONCAT('ALTER TABLE `insumos` DROP FOREIGN KEY `', @fk_name, '`'));
PREPARE drop_fk_stmt FROM @drop_fk_sql;
EXECUTE drop_fk_stmt;
DEALLOCATE PREPARE drop_fk_stmt;

SET @fk_name := (
    SELECT kcu.CONSTRAINT_NAME
    FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu
    WHERE kcu.TABLE_SCHEMA = DATABASE() AND kcu.TABLE_NAME = 'insumos' AND kcu.COLUMN_NAME = 'TipoInventarioId' AND kcu.REFERENCED_TABLE_NAME IS NOT NULL
    LIMIT 1
);
SET @drop_fk_sql := IF(@fk_name IS NULL, 'SELECT 1', CONCAT('ALTER TABLE `insumos` DROP FOREIGN KEY `', @fk_name, '`'));
PREPARE drop_fk_stmt FROM @drop_fk_sql;
EXECUTE drop_fk_stmt;
DEALLOCATE PREPARE drop_fk_stmt;
");

            DropTableIfExists(migrationBuilder, "materiasprimas");
            DropTableIfExists(migrationBuilder, "insumos");

            DropIndexIfExists(migrationBuilder, "tiposprocesoconsumos", "IX_tiposprocesoconsumos_InsumoId");
            DropIndexIfExists(migrationBuilder, "tiposprocesoconsumos", "IX_tiposprocesoconsumos_MateriaPrimaId");
            DropIndexIfExists(migrationBuilder, "movimientosinventario", "IX_movimientosinventario_InsumoId");
            DropIndexIfExists(migrationBuilder, "movimientosinventario", "IX_movimientosinventario_MateriaPrimaId");
            DropIndexIfExists(migrationBuilder, "cotizaciondetalles", "IX_cotizaciondetalles_InsumoId");
            DropIndexIfExists(migrationBuilder, "cotizaciondetalles", "IX_cotizaciondetalles_MateriaPrimaId");

            DropColumnIfExists(migrationBuilder, "tiposprocesoconsumos", "InsumoId");
            DropColumnIfExists(migrationBuilder, "tiposprocesoconsumos", "MateriaPrimaId");
            DropColumnIfExists(migrationBuilder, "movimientosinventario", "InsumoId");
            DropColumnIfExists(migrationBuilder, "movimientosinventario", "MateriaPrimaId");
            DropColumnIfExists(migrationBuilder, "cotizaciondetalles", "InsumoId");
            DropColumnIfExists(migrationBuilder, "cotizaciondetalles", "MateriaPrimaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "InsumoId",
                table: "tiposprocesoconsumos",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "MateriaPrimaId",
                table: "tiposprocesoconsumos",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "InsumoId",
                table: "movimientosinventario",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "MateriaPrimaId",
                table: "movimientosinventario",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "InsumoId",
                table: "cotizaciondetalles",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "MateriaPrimaId",
                table: "cotizaciondetalles",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateTable(
                name: "materiasprimas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EmpresaId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TipoInventarioId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    Cantidad = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Codigo = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CodigoHex = table.Column<string>(type: "varchar(7)", maxLength: 7, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CodigoPantone = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Nombre = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    StockMinimo = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TipoMateriaPrima = table.Column<int>(type: "int", nullable: false),
                    UnidadMedida = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_materiasprimas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_materiasprimas_empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_materiasprimas_tiposinventario_TipoInventarioId",
                        column: x => x.TipoInventarioId,
                        principalTable: "tiposinventario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_tiposprocesoconsumos_InsumoId",
                table: "tiposprocesoconsumos",
                column: "InsumoId");

            migrationBuilder.CreateIndex(
                name: "IX_tiposprocesoconsumos_MateriaPrimaId",
                table: "tiposprocesoconsumos",
                column: "MateriaPrimaId");

            migrationBuilder.CreateIndex(
                name: "IX_movimientosinventario_InsumoId",
                table: "movimientosinventario",
                column: "InsumoId");

            migrationBuilder.CreateIndex(
                name: "IX_movimientosinventario_MateriaPrimaId",
                table: "movimientosinventario",
                column: "MateriaPrimaId");

            migrationBuilder.CreateIndex(
                name: "IX_cotizaciondetalles_InsumoId",
                table: "cotizaciondetalles",
                column: "InsumoId");

            migrationBuilder.CreateIndex(
                name: "IX_cotizaciondetalles_MateriaPrimaId",
                table: "cotizaciondetalles",
                column: "MateriaPrimaId");

            migrationBuilder.CreateIndex(
                name: "IX_materiasprimas_EmpresaId_Codigo",
                table: "materiasprimas",
                columns: new[] { "EmpresaId", "Codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_materiasprimas_TipoInventarioId",
                table: "materiasprimas",
                column: "TipoInventarioId");

            migrationBuilder.CreateIndex(
                name: "IX_materiasprimas_TipoMateriaPrima",
                table: "materiasprimas",
                column: "TipoMateriaPrima");

            migrationBuilder.AddForeignKey(
                name: "FK_cotizaciondetalles_insumos_InsumoId",
                table: "cotizaciondetalles",
                column: "InsumoId",
                principalTable: "insumos",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_cotizaciondetalles_materiasprimas_MateriaPrimaId",
                table: "cotizaciondetalles",
                column: "MateriaPrimaId",
                principalTable: "materiasprimas",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_insumos_empresas_EmpresaId",
                table: "insumos",
                column: "EmpresaId",
                principalTable: "empresas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_insumos_tiposinventario_TipoInventarioId",
                table: "insumos",
                column: "TipoInventarioId",
                principalTable: "tiposinventario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_movimientosinventario_insumos_InsumoId",
                table: "movimientosinventario",
                column: "InsumoId",
                principalTable: "insumos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_movimientosinventario_materiasprimas_MateriaPrimaId",
                table: "movimientosinventario",
                column: "MateriaPrimaId",
                principalTable: "materiasprimas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tiposprocesoconsumos_insumos_InsumoId",
                table: "tiposprocesoconsumos",
                column: "InsumoId",
                principalTable: "insumos",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_tiposprocesoconsumos_materiasprimas_MateriaPrimaId",
                table: "tiposprocesoconsumos",
                column: "MateriaPrimaId",
                principalTable: "materiasprimas",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        private static void DropTableIfExists(MigrationBuilder migrationBuilder, string tableName)
        {
            migrationBuilder.Sql($"DROP TABLE IF EXISTS `{tableName}`;");
        }

        private static void DropIndexIfExists(MigrationBuilder migrationBuilder, string tableName, string indexName)
        {
            migrationBuilder.Sql($@"
SET @index_exists := (
    SELECT COUNT(*)
    FROM INFORMATION_SCHEMA.STATISTICS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = '{tableName}'
      AND INDEX_NAME = '{indexName}'
);
SET @drop_index_sql := IF(
    @index_exists = 0,
    'SELECT 1',
    'ALTER TABLE `{tableName}` DROP INDEX `{indexName}`'
);
PREPARE drop_index_stmt FROM @drop_index_sql;
EXECUTE drop_index_stmt;
DEALLOCATE PREPARE drop_index_stmt;
");
        }

        private static void DropColumnIfExists(MigrationBuilder migrationBuilder, string tableName, string columnName)
        {
            migrationBuilder.Sql($@"
SET @column_exists := (
    SELECT COUNT(*)
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = '{tableName}'
      AND COLUMN_NAME = '{columnName}'
);
SET @drop_column_sql := IF(
    @column_exists = 0,
    'SELECT 1',
    'ALTER TABLE `{tableName}` DROP COLUMN `{columnName}`'
);
PREPARE drop_column_stmt FROM @drop_column_sql;
EXECUTE drop_column_stmt;
DEALLOCATE PREPARE drop_column_stmt;
");
        }
    }
}
