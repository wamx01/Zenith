using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using MundoVs.Infrastructure.Data;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(CrmDbContext))]
    [Migration("20260428153000_RrhhConceptosConfigurablesYProvisionesManual")]
    public partial class RrhhConceptosConfigurablesYProvisionesManual : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "rrhh_nomina_concepto_config",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EmpresaId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Clave = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Nombre = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Naturaleza = table.Column<int>(type: "int", nullable: false),
                    Destino = table.Column<int>(type: "int", nullable: false),
                    TipoCalculo = table.Column<int>(type: "int", nullable: false),
                    MontoFijoDefault = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PorcentajeDefault = table.Column<decimal>(type: "decimal(9,4)", precision: 9, scale: 4, nullable: false),
                    CantidadDefault = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TarifaDefault = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    EsRecurrente = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AplicaPorEmpleado = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AfectaNetoEmpleado = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AfectaCostoEmpresa = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AfectaPasivoSat = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AfectaPasivoImss = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AfectaProvision = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AfectaBaseIsr = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AfectaBaseImss = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    EsLegal = table.Column<bool>(type: "tinyint(1)", nullable: false),
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
                    table.PrimaryKey("PK_rrhh_nomina_concepto_config", x => x.Id);
                    table.ForeignKey(
                        name: "FK_rrhh_nomina_concepto_config_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "rrhh_empleado_concepto",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EmpresaId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EmpleadoId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ConceptoConfigId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Porcentaje = table.Column<decimal>(type: "decimal(9,4)", precision: 9, scale: 4, nullable: false),
                    Cantidad = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Tarifa = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Saldo = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Limite = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "date", nullable: true),
                    FechaFin = table.Column<DateTime>(type: "date", nullable: true),
                    EsRecurrente = table.Column<bool>(type: "tinyint(1)", nullable: false),
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
                    table.PrimaryKey("PK_rrhh_empleado_concepto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_rrhh_empleado_concepto_Empleados_EmpleadoId",
                        column: x => x.EmpleadoId,
                        principalTable: "Empleados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_rrhh_empleado_concepto_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_rrhh_empleado_concepto_rrhh_nomina_concepto_config_ConceptoCo~",
                        column: x => x.ConceptoConfigId,
                        principalTable: "rrhh_nomina_concepto_config",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "rrhh_nomina_provision_detalle",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EmpresaId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    NominaDetalleId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EmpleadoId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ConceptoConfigId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Importe = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    BaseCalculo = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Cantidad = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Tarifa = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PeriodoInicio = table.Column<DateTime>(type: "date", nullable: true),
                    PeriodoFin = table.Column<DateTime>(type: "date", nullable: true),
                    EsAjusteManual = table.Column<bool>(type: "tinyint(1)", nullable: false),
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
                    table.PrimaryKey("PK_rrhh_nomina_provision_detalle", x => x.Id);
                    table.ForeignKey(
                        name: "FK_rrhh_nomina_provision_detalle_Empleados_EmpleadoId",
                        column: x => x.EmpleadoId,
                        principalTable: "Empleados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_rrhh_nomina_provision_detalle_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_rrhh_nomina_provision_detalle_NominaDetalles_NominaDetalleId",
                        column: x => x.NominaDetalleId,
                        principalTable: "NominaDetalles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_rrhh_nomina_provision_detalle_rrhh_nomina_concepto_config_Co~",
                        column: x => x.ConceptoConfigId,
                        principalTable: "rrhh_nomina_concepto_config",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_rrhh_empleado_concepto_ConceptoConfigId",
                table: "rrhh_empleado_concepto",
                column: "ConceptoConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_rrhh_empleado_concepto_EmpleadoId",
                table: "rrhh_empleado_concepto",
                column: "EmpleadoId");

            migrationBuilder.CreateIndex(
                name: "IX_rrhh_empleado_concepto_EmpresaId_EmpleadoId_ConceptoConfigId_F~",
                table: "rrhh_empleado_concepto",
                columns: new[] { "EmpresaId", "EmpleadoId", "ConceptoConfigId", "FechaInicio" });

            migrationBuilder.CreateIndex(
                name: "IX_rrhh_empleado_concepto_EmpresaId_EmpleadoId_IsActive",
                table: "rrhh_empleado_concepto",
                columns: new[] { "EmpresaId", "EmpleadoId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_rrhh_nomina_concepto_config_EmpresaId_Clave",
                table: "rrhh_nomina_concepto_config",
                columns: new[] { "EmpresaId", "Clave" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_rrhh_nomina_concepto_config_EmpresaId_Naturaleza_Destino",
                table: "rrhh_nomina_concepto_config",
                columns: new[] { "EmpresaId", "Naturaleza", "Destino" });

            migrationBuilder.CreateIndex(
                name: "IX_rrhh_nomina_concepto_config_EmpresaId_Nombre",
                table: "rrhh_nomina_concepto_config",
                columns: new[] { "EmpresaId", "Nombre" });

            migrationBuilder.CreateIndex(
                name: "IX_rrhh_nomina_provision_detalle_ConceptoConfigId",
                table: "rrhh_nomina_provision_detalle",
                column: "ConceptoConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_rrhh_nomina_provision_detalle_EmpleadoId",
                table: "rrhh_nomina_provision_detalle",
                column: "EmpleadoId");

            migrationBuilder.CreateIndex(
                name: "IX_rrhh_nomina_provision_detalle_EmpresaId_EmpleadoId_PeriodoIni~",
                table: "rrhh_nomina_provision_detalle",
                columns: new[] { "EmpresaId", "EmpleadoId", "PeriodoInicio", "PeriodoFin" });

            migrationBuilder.CreateIndex(
                name: "IX_rrhh_nomina_provision_detalle_EmpresaId_NominaDetalleId_Concep~",
                table: "rrhh_nomina_provision_detalle",
                columns: new[] { "EmpresaId", "NominaDetalleId", "ConceptoConfigId" });

            migrationBuilder.CreateIndex(
                name: "IX_rrhh_nomina_provision_detalle_NominaDetalleId",
                table: "rrhh_nomina_provision_detalle",
                column: "NominaDetalleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "rrhh_empleado_concepto");

            migrationBuilder.DropTable(
                name: "rrhh_nomina_provision_detalle");

            migrationBuilder.DropTable(
                name: "rrhh_nomina_concepto_config");
        }
    }
}
