using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class F0EsquemaJornadaPorHoras : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EsPorHoras",
                table: "rrhh_asistencia",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "empleadosesquemajornada",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EmpleadoId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TipoJornada = table.Column<int>(type: "int", nullable: false),
                    VigenteDesde = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    VigenteHasta = table.Column<DateTime>(type: "datetime(6)", nullable: true),
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
                    table.PrimaryKey("PK_empleadosesquemajornada", x => x.Id);
                    table.ForeignKey(
                        name: "FK_empleadosesquemajornada_empleados_EmpleadoId",
                        column: x => x.EmpleadoId,
                        principalTable: "empleados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_empleadosesquemajornada_EmpleadoId",
                table: "empleadosesquemajornada",
                column: "EmpleadoId");

            migrationBuilder.CreateIndex(
                name: "IX_empleadosesquemajornada_EmpleadoId_VigenteDesde",
                table: "empleadosesquemajornada",
                columns: new[] { "EmpleadoId", "VigenteDesde" });

            // Backfill: esquema Fija por empleado con vigencia desde su fecha de contratación
            // (o 2000-01-01 si no tiene), para que las asistencias históricas resuelvan a Fija.
            // Los empleados creados después no necesitan backfill (sin esquema → Fija default).
            migrationBuilder.Sql(@"
INSERT INTO empleadosesquemajornada (Id, EmpleadoId, TipoJornada, VigenteDesde, VigenteHasta, CreatedAt, IsActive)
SELECT UUID(), e.Id, 1, COALESCE(e.FechaContratacion, '2000-01-01 00:00:00'), NULL, UTC_TIMESTAMP(6), 1
FROM empleados e;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "empleadosesquemajornada");

            migrationBuilder.DropColumn(
                name: "EsPorHoras",
                table: "rrhh_asistencia");
        }
    }
}
