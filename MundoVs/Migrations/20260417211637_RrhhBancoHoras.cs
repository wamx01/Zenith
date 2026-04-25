using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class RrhhBancoHoras : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "rrhh_banco_horas_movimiento",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EmpresaId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EmpleadoId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    TipoMovimiento = table.Column<int>(type: "int", nullable: false),
                    Horas = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    NominaDetalleId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    ReferenciaTipo = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Observaciones = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EsAutomatico = table.Column<bool>(type: "tinyint(1)", nullable: false),
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
                    table.PrimaryKey("PK_rrhh_banco_horas_movimiento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_rrhh_banco_horas_movimiento_Empleados_EmpleadoId",
                        column: x => x.EmpleadoId,
                        principalTable: "Empleados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_rrhh_banco_horas_movimiento_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_rrhh_banco_horas_movimiento_NominaDetalles_NominaDetalleId",
                        column: x => x.NominaDetalleId,
                        principalTable: "NominaDetalles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_rrhh_banco_horas_movimiento_EmpleadoId",
                table: "rrhh_banco_horas_movimiento",
                column: "EmpleadoId");

            migrationBuilder.CreateIndex(
                name: "IX_rrhh_banco_horas_movimiento_EmpresaId_EmpleadoId_Fecha",
                table: "rrhh_banco_horas_movimiento",
                columns: new[] { "EmpresaId", "EmpleadoId", "Fecha" });

            migrationBuilder.CreateIndex(
                name: "IX_rrhh_banco_horas_movimiento_EmpresaId_NominaDetalleId_TipoMo~",
                table: "rrhh_banco_horas_movimiento",
                columns: new[] { "EmpresaId", "NominaDetalleId", "TipoMovimiento" });

            migrationBuilder.CreateIndex(
                name: "IX_rrhh_banco_horas_movimiento_NominaDetalleId",
                table: "rrhh_banco_horas_movimiento",
                column: "NominaDetalleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "rrhh_banco_horas_movimiento");
        }
    }
}
