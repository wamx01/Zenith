using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class MultiEmpresa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Use IF EXISTS to handle partial previous runs
            migrationBuilder.Sql("DROP INDEX IF EXISTS `IX_TiposUsuario_Nombre` ON `TiposUsuario`;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS `IX_Productos_Codigo` ON `Productos`;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS `IX_Pantallas_Codigo` ON `Pantallas`;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS `IX_MateriasPrimas_Codigo` ON `MateriasPrimas`;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS `IX_Insumos_Codigo` ON `Insumos`;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS `IX_Hormas_Codigo` ON `Hormas`;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS `IX_Disenos_Codigo` ON `Disenos`;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS `IX_Clientes_Codigo` ON `Clientes`;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS `IX_AppConfigs_Clave` ON `AppConfigs`;");

            migrationBuilder.Sql("DELETE FROM AppConfigs WHERE Id IN ('00000000-0000-0000-0000-000000000001','00000000-0000-0000-0000-000000000002','00000000-0000-0000-0000-000000000010','00000000-0000-0000-0000-000000000011','00000000-0000-0000-0000-000000000020','00000000-0000-0000-0000-000000000021','00000000-0000-0000-0000-000000000022','00000000-0000-0000-0000-000000000023');");

            // Add EmpresaId columns (IF NOT EXISTS for idempotency)
            var tablesForEmpresaId = new[] {
                "Usuarios", "TiposUsuario", "TiposProceso", "Productos", "Pantallas",
                "MateriasPrimas", "Insumos", "Hormas", "GastosFijos", "EscalasSerigrafia",
                "Disenos", "Clientes", "AppConfigs", "ActividadesManoObra"
            };
            foreach (var table in tablesForEmpresaId)
            {
                migrationBuilder.Sql($@"
                    SET @exist := (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = '{table}' AND COLUMN_NAME = 'EmpresaId');
                    SET @sql := IF(@exist = 0, 'ALTER TABLE `{table}` ADD COLUMN `EmpresaId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL DEFAULT ''00000000-0000-0000-0000-000000000000''', 'SELECT 1');
                    PREPARE stmt FROM @sql;
                    EXECUTE stmt;
                    DEALLOCATE PREPARE stmt;
                ");
            }

            // Create Empresas table only if it doesn't exist
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS `Empresas` (
                    `Id` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
                    `Codigo` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
                    `RazonSocial` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
                    `NombreComercial` varchar(200) CHARACTER SET utf8mb4 NULL,
                    `Rfc` varchar(20) CHARACTER SET utf8mb4 NULL,
                    `Slogan` varchar(200) CHARACTER SET utf8mb4 NULL,
                    `IsActive` tinyint(1) NOT NULL,
                    `CreatedAt` datetime(6) NOT NULL,
                    `UpdatedAt` datetime(6) NULL,
                    CONSTRAINT `PK_Empresas` PRIMARY KEY (`Id`)
                ) CHARACTER SET=utf8mb4;
            ");

            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS `IX_Usuarios_EmpresaId` ON `Usuarios` (`EmpresaId`);");
            migrationBuilder.Sql("CREATE UNIQUE INDEX IF NOT EXISTS `IX_TiposUsuario_EmpresaId_Nombre` ON `TiposUsuario` (`EmpresaId`, `Nombre`);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS `IX_TiposProceso_EmpresaId` ON `TiposProceso` (`EmpresaId`);");
            migrationBuilder.Sql("CREATE UNIQUE INDEX IF NOT EXISTS `IX_Productos_EmpresaId_Codigo` ON `Productos` (`EmpresaId`, `Codigo`);");
            migrationBuilder.Sql("CREATE UNIQUE INDEX IF NOT EXISTS `IX_Pantallas_EmpresaId_Codigo` ON `Pantallas` (`EmpresaId`, `Codigo`);");
            migrationBuilder.Sql("CREATE UNIQUE INDEX IF NOT EXISTS `IX_MateriasPrimas_EmpresaId_Codigo` ON `MateriasPrimas` (`EmpresaId`, `Codigo`);");
            migrationBuilder.Sql("CREATE UNIQUE INDEX IF NOT EXISTS `IX_Insumos_EmpresaId_Codigo` ON `Insumos` (`EmpresaId`, `Codigo`);");
            migrationBuilder.Sql("CREATE UNIQUE INDEX IF NOT EXISTS `IX_Hormas_EmpresaId_Codigo` ON `Hormas` (`EmpresaId`, `Codigo`);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS `IX_GastosFijos_EmpresaId` ON `GastosFijos` (`EmpresaId`);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS `IX_EscalasSerigrafia_EmpresaId` ON `EscalasSerigrafia` (`EmpresaId`);");
            migrationBuilder.Sql("CREATE UNIQUE INDEX IF NOT EXISTS `IX_Disenos_EmpresaId_Codigo` ON `Disenos` (`EmpresaId`, `Codigo`);");
            migrationBuilder.Sql("CREATE UNIQUE INDEX IF NOT EXISTS `IX_Clientes_EmpresaId_Codigo` ON `Clientes` (`EmpresaId`, `Codigo`);");
            migrationBuilder.Sql("CREATE UNIQUE INDEX IF NOT EXISTS `IX_AppConfigs_EmpresaId_Clave` ON `AppConfigs` (`EmpresaId`, `Clave`);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS `IX_ActividadesManoObra_EmpresaId` ON `ActividadesManoObra` (`EmpresaId`);");
            migrationBuilder.Sql("CREATE UNIQUE INDEX IF NOT EXISTS `IX_Empresas_Codigo` ON `Empresas` (`Codigo`);");

            // Insertar empresa default y asignar a todos los registros existentes
            var defaultEmpresaId = "11111111-1111-1111-1111-111111111111";
            migrationBuilder.Sql($@"
                INSERT IGNORE INTO Empresas (Id, Codigo, RazonSocial, NombreComercial, Slogan, IsActive, CreatedAt)
                VALUES ('{defaultEmpresaId}', 'DEFAULT', 'MundoVs', 'MundoVs', 'CRM & Producción', 1, UTC_TIMESTAMP());
            ");

            var tables = new[] {
                "ActividadesManoObra", "AppConfigs", "Clientes", "Disenos",
                "EscalasSerigrafia", "GastosFijos", "Hormas", "Insumos",
                "MateriasPrimas", "Pantallas", "Productos", "TiposProceso",
                "TiposUsuario", "Usuarios"
            };
            foreach (var table in tables)
            {
                migrationBuilder.Sql($"UPDATE `{table}` SET EmpresaId = '{defaultEmpresaId}' WHERE EmpresaId = '00000000-0000-0000-0000-000000000000';");
            }

            migrationBuilder.AddForeignKey(
                name: "FK_ActividadesManoObra_Empresas_EmpresaId",
                table: "ActividadesManoObra",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AppConfigs_Empresas_EmpresaId",
                table: "AppConfigs",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Clientes_Empresas_EmpresaId",
                table: "Clientes",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Disenos_Empresas_EmpresaId",
                table: "Disenos",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EscalasSerigrafia_Empresas_EmpresaId",
                table: "EscalasSerigrafia",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GastosFijos_Empresas_EmpresaId",
                table: "GastosFijos",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Hormas_Empresas_EmpresaId",
                table: "Hormas",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Insumos_Empresas_EmpresaId",
                table: "Insumos",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MateriasPrimas_Empresas_EmpresaId",
                table: "MateriasPrimas",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Pantallas_Empresas_EmpresaId",
                table: "Pantallas",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Productos_Empresas_EmpresaId",
                table: "Productos",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TiposProceso_Empresas_EmpresaId",
                table: "TiposProceso",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TiposUsuario_Empresas_EmpresaId",
                table: "TiposUsuario",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_Empresas_EmpresaId",
                table: "Usuarios",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActividadesManoObra_Empresas_EmpresaId",
                table: "ActividadesManoObra");

            migrationBuilder.DropForeignKey(
                name: "FK_AppConfigs_Empresas_EmpresaId",
                table: "AppConfigs");

            migrationBuilder.DropForeignKey(
                name: "FK_Clientes_Empresas_EmpresaId",
                table: "Clientes");

            migrationBuilder.DropForeignKey(
                name: "FK_Disenos_Empresas_EmpresaId",
                table: "Disenos");

            migrationBuilder.DropForeignKey(
                name: "FK_EscalasSerigrafia_Empresas_EmpresaId",
                table: "EscalasSerigrafia");

            migrationBuilder.DropForeignKey(
                name: "FK_GastosFijos_Empresas_EmpresaId",
                table: "GastosFijos");

            migrationBuilder.DropForeignKey(
                name: "FK_Hormas_Empresas_EmpresaId",
                table: "Hormas");

            migrationBuilder.DropForeignKey(
                name: "FK_Insumos_Empresas_EmpresaId",
                table: "Insumos");

            migrationBuilder.DropForeignKey(
                name: "FK_MateriasPrimas_Empresas_EmpresaId",
                table: "MateriasPrimas");

            migrationBuilder.DropForeignKey(
                name: "FK_Pantallas_Empresas_EmpresaId",
                table: "Pantallas");

            migrationBuilder.DropForeignKey(
                name: "FK_Productos_Empresas_EmpresaId",
                table: "Productos");

            migrationBuilder.DropForeignKey(
                name: "FK_TiposProceso_Empresas_EmpresaId",
                table: "TiposProceso");

            migrationBuilder.DropForeignKey(
                name: "FK_TiposUsuario_Empresas_EmpresaId",
                table: "TiposUsuario");

            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_Empresas_EmpresaId",
                table: "Usuarios");

            migrationBuilder.DropTable(
                name: "Empresas");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_EmpresaId",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_TiposUsuario_EmpresaId_Nombre",
                table: "TiposUsuario");

            migrationBuilder.DropIndex(
                name: "IX_TiposProceso_EmpresaId",
                table: "TiposProceso");

            migrationBuilder.DropIndex(
                name: "IX_Productos_EmpresaId_Codigo",
                table: "Productos");

            migrationBuilder.DropIndex(
                name: "IX_Pantallas_EmpresaId_Codigo",
                table: "Pantallas");

            migrationBuilder.DropIndex(
                name: "IX_MateriasPrimas_EmpresaId_Codigo",
                table: "MateriasPrimas");

            migrationBuilder.DropIndex(
                name: "IX_Insumos_EmpresaId_Codigo",
                table: "Insumos");

            migrationBuilder.DropIndex(
                name: "IX_Hormas_EmpresaId_Codigo",
                table: "Hormas");

            migrationBuilder.DropIndex(
                name: "IX_GastosFijos_EmpresaId",
                table: "GastosFijos");

            migrationBuilder.DropIndex(
                name: "IX_EscalasSerigrafia_EmpresaId",
                table: "EscalasSerigrafia");

            migrationBuilder.DropIndex(
                name: "IX_Disenos_EmpresaId_Codigo",
                table: "Disenos");

            migrationBuilder.DropIndex(
                name: "IX_Clientes_EmpresaId_Codigo",
                table: "Clientes");

            migrationBuilder.DropIndex(
                name: "IX_AppConfigs_EmpresaId_Clave",
                table: "AppConfigs");

            migrationBuilder.DropIndex(
                name: "IX_ActividadesManoObra_EmpresaId",
                table: "ActividadesManoObra");

            migrationBuilder.DropColumn(
                name: "EmpresaId",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "EmpresaId",
                table: "TiposUsuario");

            migrationBuilder.DropColumn(
                name: "EmpresaId",
                table: "TiposProceso");

            migrationBuilder.DropColumn(
                name: "EmpresaId",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "EmpresaId",
                table: "Pantallas");

            migrationBuilder.DropColumn(
                name: "EmpresaId",
                table: "MateriasPrimas");

            migrationBuilder.DropColumn(
                name: "EmpresaId",
                table: "Insumos");

            migrationBuilder.DropColumn(
                name: "EmpresaId",
                table: "Hormas");

            migrationBuilder.DropColumn(
                name: "EmpresaId",
                table: "GastosFijos");

            migrationBuilder.DropColumn(
                name: "EmpresaId",
                table: "EscalasSerigrafia");

            migrationBuilder.DropColumn(
                name: "EmpresaId",
                table: "Disenos");

            migrationBuilder.DropColumn(
                name: "EmpresaId",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "EmpresaId",
                table: "AppConfigs");

            migrationBuilder.DropColumn(
                name: "EmpresaId",
                table: "ActividadesManoObra");

            migrationBuilder.InsertData(
                table: "AppConfigs",
                columns: new[] { "Id", "Clave", "CreatedAt", "Descripcion", "UpdatedAt", "Valor" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000001"), "CompanyName", new DateTime(2026, 2, 25, 6, 12, 36, 967, DateTimeKind.Utc).AddTicks(3817), "Nombre de la empresa", null, "MundoVs" },
                    { new Guid("00000000-0000-0000-0000-000000000002"), "CompanySlogan", new DateTime(2026, 2, 25, 6, 12, 36, 967, DateTimeKind.Utc).AddTicks(4758), "Slogan o subtítulo", null, "CRM & Producción" },
                    { new Guid("00000000-0000-0000-0000-000000000010"), "Modulo:Calzado", new DateTime(2026, 2, 25, 6, 12, 36, 967, DateTimeKind.Utc).AddTicks(4765), "Habilitar módulo de Calzado", null, "false" },
                    { new Guid("00000000-0000-0000-0000-000000000011"), "Modulo:Serigrafia", new DateTime(2026, 2, 25, 6, 12, 36, 967, DateTimeKind.Utc).AddTicks(4767), "Habilitar módulo de Serigrafía", null, "true" },
                    { new Guid("00000000-0000-0000-0000-000000000020"), "Consecutivo:Producto", new DateTime(2026, 2, 25, 6, 12, 36, 967, DateTimeKind.Utc).AddTicks(4770), "Consecutivo para código de Producto", null, "0" },
                    { new Guid("00000000-0000-0000-0000-000000000021"), "Consecutivo:MateriaPrima", new DateTime(2026, 2, 25, 6, 12, 36, 967, DateTimeKind.Utc).AddTicks(4772), "Consecutivo para código de Materia Prima", null, "0" },
                    { new Guid("00000000-0000-0000-0000-000000000022"), "Consecutivo:Insumo", new DateTime(2026, 2, 25, 6, 12, 36, 967, DateTimeKind.Utc).AddTicks(4774), "Consecutivo para código de Insumo", null, "0" },
                    { new Guid("00000000-0000-0000-0000-000000000023"), "Consecutivo:Cliente", new DateTime(2026, 2, 25, 6, 12, 36, 967, DateTimeKind.Utc).AddTicks(4775), "Consecutivo para código de Cliente", null, "0" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_TiposUsuario_Nombre",
                table: "TiposUsuario",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Productos_Codigo",
                table: "Productos",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pantallas_Codigo",
                table: "Pantallas",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MateriasPrimas_Codigo",
                table: "MateriasPrimas",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Insumos_Codigo",
                table: "Insumos",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Hormas_Codigo",
                table: "Hormas",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Disenos_Codigo",
                table: "Disenos",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_Codigo",
                table: "Clientes",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppConfigs_Clave",
                table: "AppConfigs",
                column: "Clave",
                unique: true);
        }
    }
}
