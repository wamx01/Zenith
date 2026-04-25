using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class Fase5_MovimientosInventarioYCapacidades : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MovimientosInventario",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EmpresaId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Origen = table.Column<int>(type: "int", nullable: false),
                    TipoMovimiento = table.Column<int>(type: "int", nullable: false),
                    MateriaPrimaId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    InsumoId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    Cantidad = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ExistenciaAnterior = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ExistenciaNueva = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CostoUnitario = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    FechaMovimiento = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Referencia = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Observaciones = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
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
                    table.PrimaryKey("PK_MovimientosInventario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovimientosInventario_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MovimientosInventario_Insumos_InsumoId",
                        column: x => x.InsumoId,
                        principalTable: "Insumos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovimientosInventario_MateriasPrimas_MateriaPrimaId",
                        column: x => x.MateriaPrimaId,
                        principalTable: "MateriasPrimas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_EmpresaId",
                table: "MovimientosInventario",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_FechaMovimiento",
                table: "MovimientosInventario",
                column: "FechaMovimiento");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_InsumoId",
                table: "MovimientosInventario",
                column: "InsumoId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_MateriaPrimaId",
                table: "MovimientosInventario",
                column: "MateriaPrimaId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_Origen",
                table: "MovimientosInventario",
                column: "Origen");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_TipoMovimiento",
                table: "MovimientosInventario",
                column: "TipoMovimiento");

            migrationBuilder.Sql(@"
INSERT INTO Capacidades (Id, Clave, Nombre, Modulo, Descripcion)
SELECT UUID(), 'inventario.ver', 'Ver inventario', 'Inventario', 'Acceso al módulo unificado de inventario'
WHERE NOT EXISTS (SELECT 1 FROM Capacidades WHERE Clave = 'inventario.ver');

INSERT INTO Capacidades (Id, Clave, Nombre, Modulo, Descripcion)
SELECT UUID(), 'inventario.editar', 'Editar inventario', 'Inventario', 'Alta rápida de materias primas e insumos desde inventario'
WHERE NOT EXISTS (SELECT 1 FROM Capacidades WHERE Clave = 'inventario.editar');

INSERT INTO Capacidades (Id, Clave, Nombre, Modulo, Descripcion)
SELECT UUID(), 'inventario.movimientos', 'Registrar movimientos', 'Inventario', 'Entradas, salidas y ajustes de inventario'
WHERE NOT EXISTS (SELECT 1 FROM Capacidades WHERE Clave = 'inventario.movimientos');

INSERT INTO Capacidades (Id, Clave, Nombre, Modulo, Descripcion)
SELECT UUID(), 'inventario.reportes', 'Ver reportes de inventario', 'Inventario', 'Consulta de movimientos y costo de inventario'
WHERE NOT EXISTS (SELECT 1 FROM Capacidades WHERE Clave = 'inventario.reportes');

INSERT INTO TipoUsuarioCapacidades (TipoUsuarioId, CapacidadId)
SELECT tu.Id, c.Id
FROM TiposUsuario tu
JOIN Capacidades c ON c.Clave IN ('inventario.ver', 'inventario.editar', 'inventario.movimientos', 'inventario.reportes')
WHERE tu.Nombre = 'Administrador'
AND NOT EXISTS (
    SELECT 1 FROM TipoUsuarioCapacidades tuc
    WHERE tuc.TipoUsuarioId = tu.Id AND tuc.CapacidadId = c.Id
);

INSERT INTO TipoUsuarioCapacidades (TipoUsuarioId, CapacidadId)
SELECT tu.Id, c.Id
FROM TiposUsuario tu
JOIN Capacidades c ON c.Clave IN ('inventario.ver', 'inventario.reportes')
WHERE tu.Nombre = 'Gerente'
AND NOT EXISTS (
    SELECT 1 FROM TipoUsuarioCapacidades tuc
    WHERE tuc.TipoUsuarioId = tu.Id AND tuc.CapacidadId = c.Id
);

INSERT INTO TipoUsuarioCapacidades (TipoUsuarioId, CapacidadId)
SELECT tu.Id, c.Id
FROM TiposUsuario tu
JOIN Capacidades c ON c.Clave IN ('inventario.ver', 'inventario.editar', 'inventario.movimientos', 'inventario.reportes')
WHERE tu.Nombre = 'Producción'
AND NOT EXISTS (
    SELECT 1 FROM TipoUsuarioCapacidades tuc
    WHERE tuc.TipoUsuarioId = tu.Id AND tuc.CapacidadId = c.Id
);

INSERT INTO TipoUsuarioCapacidades (TipoUsuarioId, CapacidadId)
SELECT tu.Id, c.Id
FROM TiposUsuario tu
JOIN Capacidades c ON c.Clave IN ('inventario.ver', 'inventario.reportes')
WHERE tu.Nombre = 'Contador'
AND NOT EXISTS (
    SELECT 1 FROM TipoUsuarioCapacidades tuc
    WHERE tuc.TipoUsuarioId = tu.Id AND tuc.CapacidadId = c.Id
);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MovimientosInventario");
        }
    }
}
