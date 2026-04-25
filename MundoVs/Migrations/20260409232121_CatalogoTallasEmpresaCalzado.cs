using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class CatalogoTallasEmpresaCalzado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CatalogoTallasCalzado",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EmpresaId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Talla = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SistemaNumeracion = table.Column<int>(type: "int", nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    Activa = table.Column<bool>(type: "tinyint(1)", nullable: false),
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
                    table.PrimaryKey("PK_CatalogoTallasCalzado", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CatalogoTallasCalzado_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogoTallasCalzado_EmpresaId_Orden",
                table: "CatalogoTallasCalzado",
                columns: new[] { "EmpresaId", "Orden" });

            migrationBuilder.CreateIndex(
                name: "IX_CatalogoTallasCalzado_EmpresaId_SistemaNumeracion_Talla",
                table: "CatalogoTallasCalzado",
                columns: new[] { "EmpresaId", "SistemaNumeracion", "Talla" },
                unique: true);

            migrationBuilder.Sql(@"
INSERT INTO CatalogoTallasCalzado (Id, EmpresaId, Talla, SistemaNumeracion, Orden, Activa, CreatedAt, IsActive)
SELECT UUID(), src.EmpresaId, src.Talla, 1, MIN(src.Orden), 1, UTC_TIMESTAMP(), 1
FROM (
    SELECT EmpresaId, Talla, Orden
    FROM ClientesTallasCalzado
    WHERE IsActive = 1 AND Talla IS NOT NULL AND Talla <> ''
    UNION ALL
    SELECT EmpresaId, Talla, Orden
    FROM ClientesFraccionesCalzadoDetalle
    WHERE IsActive = 1 AND Talla IS NOT NULL AND Talla <> ''
) AS src
GROUP BY src.EmpresaId, src.Talla;
");

            migrationBuilder.DropIndex(
                name: "IX_ClientesTallasCalzado_ClienteId_Talla",
                table: "ClientesTallasCalzado");

            migrationBuilder.AddColumn<Guid>(
                name: "CatalogoTallaCalzadoId",
                table: "ClientesTallasCalzado",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "CatalogoTallaCalzadoId",
                table: "ClientesFraccionesCalzadoDetalle",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.Sql(@"
UPDATE ClientesTallasCalzado ct
INNER JOIN CatalogoTallasCalzado cat
    ON cat.EmpresaId = ct.EmpresaId
   AND cat.Talla = ct.Talla
   AND cat.SistemaNumeracion = 1
SET ct.CatalogoTallaCalzadoId = cat.Id
WHERE ct.IsActive = 1;
");

            migrationBuilder.Sql(@"
UPDATE ClientesFraccionesCalzadoDetalle det
INNER JOIN CatalogoTallasCalzado cat
    ON cat.EmpresaId = det.EmpresaId
   AND cat.Talla = det.Talla
   AND cat.SistemaNumeracion = 1
SET det.CatalogoTallaCalzadoId = cat.Id
WHERE det.IsActive = 1;
");

            migrationBuilder.AlterColumn<Guid>(
                name: "CatalogoTallaCalzadoId",
                table: "ClientesTallasCalzado",
                type: "char(36)",
                nullable: false,
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)",
                oldNullable: true,
                oldCollation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_ClientesTallasCalzado_CatalogoTallaCalzadoId",
                table: "ClientesTallasCalzado",
                column: "CatalogoTallaCalzadoId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientesTallasCalzado_ClienteId_CatalogoTallaCalzadoId",
                table: "ClientesTallasCalzado",
                columns: new[] { "ClienteId", "CatalogoTallaCalzadoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClientesFraccionesCalzadoDetalle_CatalogoTallaCalzadoId",
                table: "ClientesFraccionesCalzadoDetalle",
                column: "CatalogoTallaCalzadoId");

            migrationBuilder.AddForeignKey(
                name: "FK_ClientesFraccionesCalzadoDetalle_CatalogoTallasCalzado_CatalogoTallaCalzadoId",
                table: "ClientesFraccionesCalzadoDetalle",
                column: "CatalogoTallaCalzadoId",
                principalTable: "CatalogoTallasCalzado",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ClientesTallasCalzado_CatalogoTallasCalzado_CatalogoTallaCalzadoId",
                table: "ClientesTallasCalzado",
                column: "CatalogoTallaCalzadoId",
                principalTable: "CatalogoTallasCalzado",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClientesFraccionesCalzadoDetalle_CatalogoTallasCalzado_CatalogoTallaCalzadoId",
                table: "ClientesFraccionesCalzadoDetalle");

            migrationBuilder.DropForeignKey(
                name: "FK_ClientesTallasCalzado_CatalogoTallasCalzado_CatalogoTallaCalzadoId",
                table: "ClientesTallasCalzado");

            migrationBuilder.DropIndex(
                name: "IX_ClientesTallasCalzado_CatalogoTallaCalzadoId",
                table: "ClientesTallasCalzado");

            migrationBuilder.DropIndex(
                name: "IX_ClientesTallasCalzado_ClienteId_CatalogoTallaCalzadoId",
                table: "ClientesTallasCalzado");

            migrationBuilder.DropIndex(
                name: "IX_ClientesFraccionesCalzadoDetalle_CatalogoTallaCalzadoId",
                table: "ClientesFraccionesCalzadoDetalle");

            migrationBuilder.DropColumn(
                name: "CatalogoTallaCalzadoId",
                table: "ClientesTallasCalzado");

            migrationBuilder.DropColumn(
                name: "CatalogoTallaCalzadoId",
                table: "ClientesFraccionesCalzadoDetalle");

            migrationBuilder.DropTable(
                name: "CatalogoTallasCalzado");

            migrationBuilder.CreateIndex(
                name: "IX_ClientesTallasCalzado_ClienteId_Talla",
                table: "ClientesTallasCalzado",
                columns: new[] { "ClienteId", "Talla" },
                unique: true);
        }
    }
}
