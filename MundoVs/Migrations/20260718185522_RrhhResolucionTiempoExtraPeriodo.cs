using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class RrhhResolucionTiempoExtraPeriodo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "rrhh_resolucion_tiempo_extra_periodo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EmpresaId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EmpleadoId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PeriodicidadPago = table.Column<int>(type: "int", nullable: false),
                    AnioPeriodo = table.Column<int>(type: "int", nullable: false),
                    NumeroPeriodo = table.Column<int>(type: "int", nullable: false),
                    PeriodoKey = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PeriodoEtiqueta = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FechaInicio = table.Column<DateOnly>(type: "date", nullable: false),
                    FechaFin = table.Column<DateOnly>(type: "date", nullable: false),
                    MinutosExtraDetectado = table.Column<int>(type: "int", nullable: false),
                    MinutosFaltanteDetectado = table.Column<int>(type: "int", nullable: false),
                    MinutosRetardoDetectado = table.Column<int>(type: "int", nullable: false),
                    MinutosTrabajadosNetosDetectado = table.Column<int>(type: "int", nullable: false),
                    MinutosExtraPago = table.Column<int>(type: "int", nullable: false),
                    MinutosExtraBanco = table.Column<int>(type: "int", nullable: false),
                    FactorTiempoExtraAplicado = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    FactorAcumulacionBancoHorasAplicado = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    Resolucion = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Estatus = table.Column<int>(type: "int", nullable: false),
                    AutorizadoPor = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FechaAutorizacion = table.Column<DateTime>(type: "datetime(6)", nullable: true),
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
                    table.PrimaryKey("PK_rrhh_resolucion_tiempo_extra_periodo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_rrhh_resolucion_tiempo_extra_periodo_empleados_EmpleadoId",
                        column: x => x.EmpleadoId,
                        principalTable: "empleados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_rrhh_resolucion_tiempo_extra_periodo_empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_rrhh_resolucion_tiempo_extra_periodo_EmpleadoId",
                table: "rrhh_resolucion_tiempo_extra_periodo",
                column: "EmpleadoId");

            migrationBuilder.CreateIndex(
                name: "IX_rrhh_resolucion_tiempo_extra_periodo_EmpresaId_EmpleadoId_Pe~",
                table: "rrhh_resolucion_tiempo_extra_periodo",
                columns: new[] { "EmpresaId", "EmpleadoId", "PeriodicidadPago", "AnioPeriodo", "NumeroPeriodo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_rrhh_resolucion_tiempo_extra_periodo_EmpresaId_Estatus",
                table: "rrhh_resolucion_tiempo_extra_periodo",
                columns: new[] { "EmpresaId", "Estatus" });

            migrationBuilder.CreateIndex(
                name: "IX_rrhh_resolucion_tiempo_extra_periodo_EmpresaId_FechaInicio_F~",
                table: "rrhh_resolucion_tiempo_extra_periodo",
                columns: new[] { "EmpresaId", "FechaInicio", "FechaFin" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "rrhh_resolucion_tiempo_extra_periodo");
        }
    }
}
