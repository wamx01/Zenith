using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class AuthModulosEmpresa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "auth_modulo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Clave = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Nombre = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Descripcion = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    EsGlobal = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_auth_modulo", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "auth_empresa_modulo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EmpresaId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ModuloAccesoId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Habilitado = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    VigenteDesde = table.Column<DateTime>(type: "date", nullable: false),
                    VigenteHasta = table.Column<DateTime>(type: "date", nullable: true),
                    Origen = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_auth_empresa_modulo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_auth_empresa_modulo_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_auth_empresa_modulo_auth_modulo_ModuloAccesoId",
                        column: x => x.ModuloAccesoId,
                        principalTable: "auth_modulo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<Guid>(
                name: "ModuloAccesoId",
                table: "Capacidades",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.Sql(@"
INSERT INTO `auth_modulo` (`Id`, `Clave`, `Nombre`, `Descripcion`, `Orden`, `EsGlobal`, `CreatedAt`, `IsActive`)
VALUES
('9D788036-58AC-4B52-99D8-6755C04E7982', 'plataforma', 'Plataforma', 'Administración global y multiempresa', 1, 1, UTC_TIMESTAMP(), 1),
('56BCE92E-D7A9-472E-B769-B317CC741F1D', 'administracion', 'Administración', 'Usuarios, roles y permisos por empresa', 2, 0, UTC_TIMESTAMP(), 1),
('FB8D5495-78EF-4D34-B37A-B955F909B4D6', 'catalogos', 'Catálogos', 'Clientes, productos y compras base', 10, 0, UTC_TIMESTAMP(), 1),
('1B15C33F-01C4-4C7B-9D13-436902995E3F', 'manufactura', 'Manufactura', 'Pedidos, cotizaciones y producción', 20, 0, UTC_TIMESTAMP(), 1),
('2E113DFA-8D39-493F-8A4E-09B19AAB9B87', 'logistica', 'Logística', 'Inventario, insumos y finished goods', 30, 0, UTC_TIMESTAMP(), 1),
('F9FB95D2-E6A3-4948-9558-C9ACD6E4A443', 'contabilidad', 'Contabilidad', 'Finanzas, facturación y cuentas por pagar', 40, 0, UTC_TIMESTAMP(), 1),
('BB759E51-4B79-4ACB-B503-4FCD3A8B8568', 'rrhh', 'Recursos Humanos', 'Empleados, prenómina y nómina', 50, 0, UTC_TIMESTAMP(), 1),
('B9AA1C62-4201-4B53-BE95-FDCC0CCF146E', 'configuracion', 'Configuración', 'Parámetros operativos por empresa', 60, 0, UTC_TIMESTAMP(), 1);

UPDATE `Capacidades`
SET `ModuloAccesoId` = CASE
    WHEN `Clave` LIKE 'empresas.%' OR LOWER(`Modulo`) = 'plataforma' THEN '9D788036-58AC-4B52-99D8-6755C04E7982'
    WHEN `Clave` LIKE 'usuarios.%' OR LOWER(`Modulo`) = 'usuarios' THEN '56BCE92E-D7A9-472E-B769-B317CC741F1D'
    WHEN LOWER(`Modulo`) IN ('clientes', 'productos', 'proveedores') THEN 'FB8D5495-78EF-4D34-B37A-B955F909B4D6'
    WHEN LOWER(`Modulo`) IN ('pedidos', 'cotizaciones', 'serigrafía', 'serigrafia') THEN '1B15C33F-01C4-4C7B-9D13-436902995E3F'
    WHEN LOWER(`Modulo`) = 'inventario' THEN '2E113DFA-8D39-493F-8A4E-09B19AAB9B87'
    WHEN LOWER(`Modulo`) IN ('contabilidad', 'facturación', 'facturacion', 'cuentas por pagar') THEN 'F9FB95D2-E6A3-4948-9558-C9ACD6E4A443'
    WHEN LOWER(`Modulo`) IN ('empleados', 'nóminas', 'nominas') THEN 'BB759E51-4B79-4ACB-B503-4FCD3A8B8568'
    WHEN LOWER(`Modulo`) = 'configuración' OR LOWER(`Modulo`) = 'configuracion' THEN 'B9AA1C62-4201-4B53-BE95-FDCC0CCF146E'
    ELSE '56BCE92E-D7A9-472E-B769-B317CC741F1D'
END
WHERE `ModuloAccesoId` IS NULL;

INSERT INTO `auth_empresa_modulo` (`Id`, `EmpresaId`, `ModuloAccesoId`, `Habilitado`, `VigenteDesde`, `VigenteHasta`, `Origen`, `CreatedAt`, `IsActive`)
SELECT UUID(), e.`Id`, m.`Id`, CASE WHEN m.`Clave` = 'plataforma' THEN 0 ELSE 1 END, UTC_DATE(), NULL, CASE WHEN m.`EsGlobal` = 1 THEN 1 ELSE 2 END, UTC_TIMESTAMP(), 1
FROM `Empresas` e
CROSS JOIN `auth_modulo` m;

");

            migrationBuilder.AlterColumn<Guid>(
                name: "ModuloAccesoId",
                table: "Capacidades",
                type: "char(36)",
                nullable: false,
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)",
                oldNullable: true)
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_Capacidades_ModuloAccesoId_Nombre",
                table: "Capacidades",
                columns: new[] { "ModuloAccesoId", "Nombre" });

            migrationBuilder.CreateIndex(
                name: "IX_auth_empresa_modulo_EmpresaId_Habilitado_VigenteDesde_Vigent~",
                table: "auth_empresa_modulo",
                columns: new[] { "EmpresaId", "Habilitado", "VigenteDesde", "VigenteHasta" });

            migrationBuilder.CreateIndex(
                name: "IX_auth_empresa_modulo_EmpresaId_ModuloAccesoId",
                table: "auth_empresa_modulo",
                columns: new[] { "EmpresaId", "ModuloAccesoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_auth_empresa_modulo_ModuloAccesoId",
                table: "auth_empresa_modulo",
                column: "ModuloAccesoId");

            migrationBuilder.CreateIndex(
                name: "IX_auth_modulo_Clave",
                table: "auth_modulo",
                column: "Clave",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_auth_modulo_Orden_Nombre",
                table: "auth_modulo",
                columns: new[] { "Orden", "Nombre" });

            migrationBuilder.AddForeignKey(
                name: "FK_Capacidades_auth_modulo_ModuloAccesoId",
                table: "Capacidades",
                column: "ModuloAccesoId",
                principalTable: "auth_modulo",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Capacidades_auth_modulo_ModuloAccesoId",
                table: "Capacidades");

            migrationBuilder.DropTable(
                name: "auth_empresa_modulo");

            migrationBuilder.DropTable(
                name: "auth_modulo");

            migrationBuilder.DropIndex(
                name: "IX_Capacidades_ModuloAccesoId_Nombre",
                table: "Capacidades");

            migrationBuilder.DropColumn(
                name: "ModuloAccesoId",
                table: "Capacidades");
        }
    }
}
