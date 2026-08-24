using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class FusionPrenominaNomina_Drop : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_nominas_prenominas_PrenominaId",
                table: "nominas");

            migrationBuilder.DropTable(
                name: "rrhh_prenomina_bono");

            migrationBuilder.DropTable(
                name: "rrhh_prenomina_percepcion");

            migrationBuilder.DropTable(
                name: "prenominadetalles");

            migrationBuilder.DropTable(
                name: "prenominas");

            migrationBuilder.DropIndex(
                name: "IX_nominas_PrenominaId",
                table: "nominas");

            migrationBuilder.DropColumn(
                name: "PrenominaId",
                table: "nominas");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PrenominaId",
                table: "nominas",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateTable(
                name: "prenominas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EmpresaId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    AnioPeriodo = table.Column<int>(type: "int", nullable: false),
                    CerradaPor = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Estatus = table.Column<int>(type: "int", nullable: false),
                    FechaCierre = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    FechaFin = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Folio = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Notas = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NumeroPeriodo = table.Column<int>(type: "int", nullable: false),
                    PeriodicidadPago = table.Column<int>(type: "int", nullable: false),
                    Periodo = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SnapshotConfiguracionJson = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prenominas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_prenominas_empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "prenominadetalles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EmpleadoId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PrenominaId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    AplicaImss = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ComplementoSalarioMinimoSugerido = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DiasConMarcacion = table.Column<int>(type: "int", nullable: false),
                    DiasDescansoTrabajado = table.Column<int>(type: "int", nullable: false),
                    DiasDomingoTrabajado = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    DiasFaltaInjustificada = table.Column<int>(type: "int", nullable: false),
                    DiasFaltaJustificada = table.Column<int>(type: "int", nullable: false),
                    DiasFestivoTrabajado = table.Column<int>(type: "int", nullable: false),
                    DiasFestivoTrabajadoFija = table.Column<int>(type: "int", nullable: false),
                    DiasIncapacidad = table.Column<int>(type: "int", nullable: false),
                    DiasPagados = table.Column<int>(type: "int", nullable: false),
                    DiasPorHorasTrabajados = table.Column<int>(type: "int", nullable: false),
                    DiasTrabajados = table.Column<int>(type: "int", nullable: false),
                    DiasVacaciones = table.Column<int>(type: "int", nullable: false),
                    DiasVacacionesDisponibles = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DiasVacacionesRestantes = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    FactorPagoTiempoExtra = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    HorasBancoAcumuladas = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    HorasBancoConsumidas = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    HorasDescansoNoPagado = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    HorasDescansoPagado = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    HorasDescansoTomado = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    HorasExtra = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    HorasExtraBase = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    HorasTrabajadasNetas = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    MinutosDescuentoManual = table.Column<int>(type: "int", nullable: false),
                    MinutosFaltanteDescontable = table.Column<int>(type: "int", nullable: false),
                    MinutosPerdonadosManual = table.Column<int>(type: "int", nullable: false),
                    MinutosPorHorasFestivoNetos = table.Column<int>(type: "int", nullable: false),
                    MinutosPorHorasNetos = table.Column<int>(type: "int", nullable: false),
                    MinutosRetardo = table.Column<int>(type: "int", nullable: false),
                    MinutosSalidaAnticipada = table.Column<int>(type: "int", nullable: false),
                    MontoDestajoInformativo = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Notas = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prenominadetalles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_prenominadetalles_empleados_EmpleadoId",
                        column: x => x.EmpleadoId,
                        principalTable: "empleados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_prenominadetalles_prenominas_PrenominaId",
                        column: x => x.PrenominaId,
                        principalTable: "prenominas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "rrhh_prenomina_bono",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    BonoRubroRrhhId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PrenominaDetalleId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Importe = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Observaciones = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rrhh_prenomina_bono", x => x.Id);
                    table.ForeignKey(
                        name: "FK_rrhh_prenomina_bono_prenominadetalles_PrenominaDetalleId",
                        column: x => x.PrenominaDetalleId,
                        principalTable: "prenominadetalles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_rrhh_prenomina_bono_rrhh_bono_rubro_BonoRubroRrhhId",
                        column: x => x.BonoRubroRrhhId,
                        principalTable: "rrhh_bono_rubro",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "rrhh_prenomina_percepcion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PrenominaDetalleId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TipoPercepcionId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Importe = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Observaciones = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Referencia = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rrhh_prenomina_percepcion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_rrhh_prenomina_percepcion_prenominadetalles_PrenominaDetalle~",
                        column: x => x.PrenominaDetalleId,
                        principalTable: "prenominadetalles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_rrhh_prenomina_percepcion_rrhh_nomina_percepcion_tipo_TipoPe~",
                        column: x => x.TipoPercepcionId,
                        principalTable: "rrhh_nomina_percepcion_tipo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_nominas_PrenominaId",
                table: "nominas",
                column: "PrenominaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_prenominadetalles_EmpleadoId",
                table: "prenominadetalles",
                column: "EmpleadoId");

            migrationBuilder.CreateIndex(
                name: "IX_prenominadetalles_PrenominaId_EmpleadoId",
                table: "prenominadetalles",
                columns: new[] { "PrenominaId", "EmpleadoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_prenominas_EmpresaId_FechaInicio_FechaFin",
                table: "prenominas",
                columns: new[] { "EmpresaId", "FechaInicio", "FechaFin" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_prenominas_EmpresaId_Folio",
                table: "prenominas",
                columns: new[] { "EmpresaId", "Folio" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_prenominas_EmpresaId_PeriodicidadPago_AnioPeriodo_NumeroPeri~",
                table: "prenominas",
                columns: new[] { "EmpresaId", "PeriodicidadPago", "AnioPeriodo", "NumeroPeriodo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_prenominas_Estatus",
                table: "prenominas",
                column: "Estatus");

            migrationBuilder.CreateIndex(
                name: "IX_rrhh_prenomina_bono_BonoRubroRrhhId",
                table: "rrhh_prenomina_bono",
                column: "BonoRubroRrhhId");

            migrationBuilder.CreateIndex(
                name: "IX_rrhh_prenomina_bono_PrenominaDetalleId",
                table: "rrhh_prenomina_bono",
                column: "PrenominaDetalleId");

            migrationBuilder.CreateIndex(
                name: "IX_rrhh_prenomina_bono_PrenominaDetalleId_BonoRubroRrhhId",
                table: "rrhh_prenomina_bono",
                columns: new[] { "PrenominaDetalleId", "BonoRubroRrhhId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_rrhh_prenomina_percepcion_PrenominaDetalleId",
                table: "rrhh_prenomina_percepcion",
                column: "PrenominaDetalleId");

            migrationBuilder.CreateIndex(
                name: "IX_rrhh_prenomina_percepcion_PrenominaDetalleId_TipoPercepcionId",
                table: "rrhh_prenomina_percepcion",
                columns: new[] { "PrenominaDetalleId", "TipoPercepcionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_rrhh_prenomina_percepcion_TipoPercepcionId",
                table: "rrhh_prenomina_percepcion",
                column: "TipoPercepcionId");

            migrationBuilder.AddForeignKey(
                name: "FK_nominas_prenominas_PrenominaId",
                table: "nominas",
                column: "PrenominaId",
                principalTable: "prenominas",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
