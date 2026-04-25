using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class Fase1_CategoriasYTiposInventario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CategoriasInventario",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EmpresaId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Codigo = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Nombre = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Orden = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_CategoriasInventario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CategoriasInventario_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TiposInventario",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EmpresaId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CategoriaInventarioId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Codigo = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Nombre = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Color = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Orden = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_TiposInventario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TiposInventario_CategoriasInventario_CategoriaInventarioId",
                        column: x => x.CategoriaInventarioId,
                        principalTable: "CategoriasInventario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TiposInventario_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_CategoriasInventario_EmpresaId_Codigo",
                table: "CategoriasInventario",
                columns: new[] { "EmpresaId", "Codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CategoriasInventario_EmpresaId_Nombre",
                table: "CategoriasInventario",
                columns: new[] { "EmpresaId", "Nombre" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CategoriasInventario_Orden",
                table: "CategoriasInventario",
                column: "Orden");

            migrationBuilder.CreateIndex(
                name: "IX_TiposInventario_CategoriaInventarioId",
                table: "TiposInventario",
                column: "CategoriaInventarioId");

            migrationBuilder.CreateIndex(
                name: "IX_TiposInventario_EmpresaId_CategoriaInventarioId_Codigo",
                table: "TiposInventario",
                columns: new[] { "EmpresaId", "CategoriaInventarioId", "Codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TiposInventario_EmpresaId_CategoriaInventarioId_Nombre",
                table: "TiposInventario",
                columns: new[] { "EmpresaId", "CategoriaInventarioId", "Nombre" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TiposInventario_Orden",
                table: "TiposInventario",
                column: "Orden");

            migrationBuilder.Sql(@"
INSERT INTO CategoriasInventario (Id, EmpresaId, Codigo, Nombre, Orden, CreatedAt, IsActive)
SELECT UUID(), e.Id, 'MATERIA_PRIMA', 'Materia Prima', 1, UTC_TIMESTAMP(6), 1
FROM Empresas e
WHERE NOT EXISTS (
    SELECT 1
    FROM CategoriasInventario c
    WHERE c.EmpresaId = e.Id AND c.Codigo = 'MATERIA_PRIMA'
);");

            migrationBuilder.Sql(@"
INSERT INTO CategoriasInventario (Id, EmpresaId, Codigo, Nombre, Orden, CreatedAt, IsActive)
SELECT UUID(), e.Id, 'INSUMO', 'Insumo', 2, UTC_TIMESTAMP(6), 1
FROM Empresas e
WHERE NOT EXISTS (
    SELECT 1
    FROM CategoriasInventario c
    WHERE c.EmpresaId = e.Id AND c.Codigo = 'INSUMO'
);");

            migrationBuilder.Sql(@"
INSERT INTO TiposInventario (Id, EmpresaId, CategoriaInventarioId, Codigo, Nombre, Color, Orden, CreatedAt, IsActive)
SELECT UUID(), c.EmpresaId, c.Id, 'TINTA', 'Tinta', '#0dcaf0', 1, UTC_TIMESTAMP(6), 1
FROM CategoriasInventario c
WHERE c.Codigo = 'MATERIA_PRIMA'
AND NOT EXISTS (
    SELECT 1
    FROM TiposInventario t
    WHERE t.EmpresaId = c.EmpresaId AND t.CategoriaInventarioId = c.Id AND t.Codigo = 'TINTA'
);");

            migrationBuilder.Sql(@"
INSERT INTO TiposInventario (Id, EmpresaId, CategoriaInventarioId, Codigo, Nombre, Color, Orden, CreatedAt, IsActive)
SELECT UUID(), c.EmpresaId, c.Id, 'BASE', 'Base', '#ffc107', 2, UTC_TIMESTAMP(6), 1
FROM CategoriasInventario c
WHERE c.Codigo = 'MATERIA_PRIMA'
AND NOT EXISTS (
    SELECT 1
    FROM TiposInventario t
    WHERE t.EmpresaId = c.EmpresaId AND t.CategoriaInventarioId = c.Id AND t.Codigo = 'BASE'
);");

            migrationBuilder.Sql(@"
INSERT INTO TiposInventario (Id, EmpresaId, CategoriaInventarioId, Codigo, Nombre, Color, Orden, CreatedAt, IsActive)
SELECT UUID(), c.EmpresaId, c.Id, 'SOLVENTE', 'Solvente', '#6c757d', 3, UTC_TIMESTAMP(6), 1
FROM CategoriasInventario c
WHERE c.Codigo = 'MATERIA_PRIMA'
AND NOT EXISTS (
    SELECT 1
    FROM TiposInventario t
    WHERE t.EmpresaId = c.EmpresaId AND t.CategoriaInventarioId = c.Id AND t.Codigo = 'SOLVENTE'
);");

            migrationBuilder.Sql(@"
INSERT INTO TiposInventario (Id, EmpresaId, CategoriaInventarioId, Codigo, Nombre, Color, Orden, CreatedAt, IsActive)
SELECT UUID(), c.EmpresaId, c.Id, 'ADITIVO', 'Aditivo', '#0d6efd', 4, UTC_TIMESTAMP(6), 1
FROM CategoriasInventario c
WHERE c.Codigo = 'MATERIA_PRIMA'
AND NOT EXISTS (
    SELECT 1
    FROM TiposInventario t
    WHERE t.EmpresaId = c.EmpresaId AND t.CategoriaInventarioId = c.Id AND t.Codigo = 'ADITIVO'
);");

            migrationBuilder.Sql(@"
INSERT INTO TiposInventario (Id, EmpresaId, CategoriaInventarioId, Codigo, Nombre, Color, Orden, CreatedAt, IsActive)
SELECT UUID(), c.EmpresaId, c.Id, 'OTRO', 'Otro', '#212529', 5, UTC_TIMESTAMP(6), 1
FROM CategoriasInventario c
WHERE c.Codigo = 'MATERIA_PRIMA'
AND NOT EXISTS (
    SELECT 1
    FROM TiposInventario t
    WHERE t.EmpresaId = c.EmpresaId AND t.CategoriaInventarioId = c.Id AND t.Codigo = 'OTRO'
);");

            migrationBuilder.Sql(@"
INSERT INTO TiposInventario (Id, EmpresaId, CategoriaInventarioId, Codigo, Nombre, Color, Orden, CreatedAt, IsActive)
SELECT UUID(), c.EmpresaId, c.Id, 'BASICO', 'Básico', '#198754', 1, UTC_TIMESTAMP(6), 1
FROM CategoriasInventario c
WHERE c.Codigo = 'INSUMO'
AND NOT EXISTS (
    SELECT 1
    FROM TiposInventario t
    WHERE t.EmpresaId = c.EmpresaId AND t.CategoriaInventarioId = c.Id AND t.Codigo = 'BASICO'
);");

            migrationBuilder.Sql(@"
INSERT INTO TiposInventario (Id, EmpresaId, CategoriaInventarioId, Codigo, Nombre, Color, Orden, CreatedAt, IsActive)
SELECT UUID(), c.EmpresaId, c.Id, 'DIVERSO', 'Diverso', '#6610f2', 2, UTC_TIMESTAMP(6), 1
FROM CategoriasInventario c
WHERE c.Codigo = 'INSUMO'
AND NOT EXISTS (
    SELECT 1
    FROM TiposInventario t
    WHERE t.EmpresaId = c.EmpresaId AND t.CategoriaInventarioId = c.Id AND t.Codigo = 'DIVERSO'
);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TiposInventario");

            migrationBuilder.DropTable(
                name: "CategoriasInventario");
        }
    }
}
