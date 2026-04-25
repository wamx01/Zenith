using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class RrhhEmpleadoTurnoVigencia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "rrhh_empleado_turno",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EmpresaId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EmpleadoId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TurnoBaseId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    VigenteDesde = table.Column<DateOnly>(type: "date", nullable: false),
                    VigenteHasta = table.Column<DateOnly>(type: "date", nullable: true),
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
                    table.PrimaryKey("PK_rrhh_empleado_turno", x => x.Id);
                    table.ForeignKey(
                        name: "FK_rrhh_empleado_turno_Empleados_EmpleadoId",
                        column: x => x.EmpleadoId,
                        principalTable: "Empleados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_rrhh_empleado_turno_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_rrhh_empleado_turno_rrhh_turno_TurnoBaseId",
                        column: x => x.TurnoBaseId,
                        principalTable: "rrhh_turno",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_rrhh_empleado_turno_EmpleadoId",
                table: "rrhh_empleado_turno",
                column: "EmpleadoId");

            migrationBuilder.CreateIndex(
                name: "IX_rrhh_empleado_turno_EmpresaId_EmpleadoId_VigenteDesde",
                table: "rrhh_empleado_turno",
                columns: new[] { "EmpresaId", "EmpleadoId", "VigenteDesde" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_rrhh_empleado_turno_EmpresaId_EmpleadoId_VigenteDesde_Vigent~",
                table: "rrhh_empleado_turno",
                columns: new[] { "EmpresaId", "EmpleadoId", "VigenteDesde", "VigenteHasta" });

            migrationBuilder.CreateIndex(
                name: "IX_rrhh_empleado_turno_TurnoBaseId",
                table: "rrhh_empleado_turno",
                column: "TurnoBaseId");

            migrationBuilder.Sql(@"
INSERT INTO `rrhh_empleado_turno`
    (`Id`, `EmpresaId`, `EmpleadoId`, `TurnoBaseId`, `VigenteDesde`, `VigenteHasta`, `Observaciones`, `CreatedAt`, `UpdatedAt`, `CreatedBy`, `UpdatedBy`, `IsActive`)
SELECT
    UUID(),
    `EmpresaId`,
    `Id`,
    `TurnoBaseId`,
    COALESCE(DATE(`FechaContratacion`), DATE(`CreatedAt`), UTC_DATE()),
    NULL,
    'Migración inicial desde Empleado.TurnoBaseId',
    UTC_TIMESTAMP(6),
    NULL,
    'migration',
    NULL,
    1
FROM `Empleados`
WHERE `TurnoBaseId` IS NOT NULL;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "rrhh_empleado_turno");
        }
    }
}
