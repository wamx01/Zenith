using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class RrhhBonosDistribuidosPeriodoEmpleado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "rrhh_bono_distribucion_periodo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EmpresaId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Periodo = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FechaInicio = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Departamento = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PosicionId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    BonoEstructuraRrhhId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    MontoTotalDistribuir = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Observaciones = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true)
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
                    table.PrimaryKey("PK_rrhh_bono_distribucion_periodo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_rrhh_bono_distribucion_periodo_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_rrhh_bono_distribucion_periodo_Posiciones_PosicionId",
                        column: x => x.PosicionId,
                        principalTable: "Posiciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_rrhh_bono_distribucion_periodo_rrhh_bono_estructura_BonoEstr~",
                        column: x => x.BonoEstructuraRrhhId,
                        principalTable: "rrhh_bono_estructura",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "rrhh_bono_distribucion_empleado",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    BonoDistribucionPeriodoRrhhId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EmpleadoId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Porcentaje = table.Column<decimal>(type: "decimal(9,4)", precision: 9, scale: 4, nullable: false),
                    MontoAsignado = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Observaciones = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true)
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
                    table.PrimaryKey("PK_rrhh_bono_distribucion_empleado", x => x.Id);
                    table.ForeignKey(
                        name: "FK_rrhh_bono_distribucion_empleado_Empleados_EmpleadoId",
                        column: x => x.EmpleadoId,
                        principalTable: "Empleados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_rrhh_bono_distribucion_empleado_rrhh_bono_distribucion_perio~",
                        column: x => x.BonoDistribucionPeriodoRrhhId,
                        principalTable: "rrhh_bono_distribucion_periodo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_rrhh_bono_distribucion_empleado_BonoDistribucionPeriodoRrhhI~",
                table: "rrhh_bono_distribucion_empleado",
                columns: new[] { "BonoDistribucionPeriodoRrhhId", "EmpleadoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_rrhh_bono_distribucion_empleado_EmpleadoId",
                table: "rrhh_bono_distribucion_empleado",
                column: "EmpleadoId");

            migrationBuilder.CreateIndex(
                name: "IX_rrhh_bono_distribucion_periodo_BonoEstructuraRrhhId",
                table: "rrhh_bono_distribucion_periodo",
                column: "BonoEstructuraRrhhId");

            migrationBuilder.CreateIndex(
                name: "IX_rrhh_bono_distribucion_periodo_EmpresaId_FechaInicio_FechaFi~",
                table: "rrhh_bono_distribucion_periodo",
                columns: new[] { "EmpresaId", "FechaInicio", "FechaFin", "PosicionId", "Departamento" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_rrhh_bono_distribucion_periodo_PosicionId",
                table: "rrhh_bono_distribucion_periodo",
                column: "PosicionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "rrhh_bono_distribucion_empleado");

            migrationBuilder.DropTable(
                name: "rrhh_bono_distribucion_periodo");
        }
    }
}
