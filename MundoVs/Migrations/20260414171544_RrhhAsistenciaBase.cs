using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class RrhhAsistenciaBase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CodigoChecador",
                table: "Empleados",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "rrhh_asistencia",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EmpresaId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EmpleadoId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TurnoBaseId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    Fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    HoraEntradaProgramada = table.Column<TimeSpan>(type: "time(6)", nullable: true),
                    HoraSalidaProgramada = table.Column<TimeSpan>(type: "time(6)", nullable: true),
                    HoraEntradaReal = table.Column<TimeSpan>(type: "time(6)", nullable: true),
                    HoraSalidaReal = table.Column<TimeSpan>(type: "time(6)", nullable: true),
                    MinutosRetardo = table.Column<int>(type: "int", nullable: false),
                    MinutosSalidaAnticipada = table.Column<int>(type: "int", nullable: false),
                    MinutosExtra = table.Column<int>(type: "int", nullable: false),
                    Estatus = table.Column<int>(type: "int", nullable: false),
                    RequiereRevision = table.Column<bool>(type: "tinyint(1)", nullable: false),
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
                    table.PrimaryKey("PK_rrhh_asistencia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_rrhh_asistencia_Empleados_EmpleadoId",
                        column: x => x.EmpleadoId,
                        principalTable: "Empleados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_rrhh_asistencia_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_rrhh_asistencia_rrhh_turno_TurnoBaseId",
                        column: x => x.TurnoBaseId,
                        principalTable: "rrhh_turno",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "rrhh_checador",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EmpresaId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Nombre = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NumeroSerie = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Marca = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Modelo = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Ip = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Puerto = table.Column<int>(type: "int", nullable: false),
                    NumeroMaquina = table.Column<int>(type: "int", nullable: false),
                    Ubicacion = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ZonaHoraria = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UltimaSincronizacionUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UltimoEventoLeido = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
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
                    table.PrimaryKey("PK_rrhh_checador", x => x.Id);
                    table.ForeignKey(
                        name: "FK_rrhh_checador_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "rrhh_logchecador",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EmpresaId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ChecadorId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    FechaUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Nivel = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Mensaje = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Detalle = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
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
                    table.PrimaryKey("PK_rrhh_logchecador", x => x.Id);
                    table.ForeignKey(
                        name: "FK_rrhh_logchecador_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_rrhh_logchecador_rrhh_checador_ChecadorId",
                        column: x => x.ChecadorId,
                        principalTable: "rrhh_checador",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "rrhh_marcacion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EmpresaId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ChecadorId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EmpleadoId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    CodigoChecador = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FechaHoraMarcacionUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    TipoMarcacionRaw = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Origen = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EventoIdExterno = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HashUnico = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Procesada = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ResultadoProcesamiento = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PayloadRaw = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: true)
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
                    table.PrimaryKey("PK_rrhh_marcacion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_rrhh_marcacion_Empleados_EmpleadoId",
                        column: x => x.EmpleadoId,
                        principalTable: "Empleados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_rrhh_marcacion_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_rrhh_marcacion_rrhh_checador_ChecadorId",
                        column: x => x.ChecadorId,
                        principalTable: "rrhh_checador",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Empleados_EmpresaId_CodigoChecador",
                table: "Empleados",
                columns: new[] { "EmpresaId", "CodigoChecador" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_rrhh_asistencia_EmpleadoId",
                table: "rrhh_asistencia",
                column: "EmpleadoId");

            migrationBuilder.CreateIndex(
                name: "IX_rrhh_asistencia_EmpresaId_EmpleadoId_Fecha",
                table: "rrhh_asistencia",
                columns: new[] { "EmpresaId", "EmpleadoId", "Fecha" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_rrhh_asistencia_EmpresaId_Fecha_Estatus",
                table: "rrhh_asistencia",
                columns: new[] { "EmpresaId", "Fecha", "Estatus" });

            migrationBuilder.CreateIndex(
                name: "IX_rrhh_asistencia_TurnoBaseId",
                table: "rrhh_asistencia",
                column: "TurnoBaseId");

            migrationBuilder.CreateIndex(
                name: "IX_rrhh_checador_EmpresaId_Nombre",
                table: "rrhh_checador",
                columns: new[] { "EmpresaId", "Nombre" });

            migrationBuilder.CreateIndex(
                name: "IX_rrhh_checador_EmpresaId_NumeroSerie",
                table: "rrhh_checador",
                columns: new[] { "EmpresaId", "NumeroSerie" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_rrhh_logchecador_ChecadorId",
                table: "rrhh_logchecador",
                column: "ChecadorId");

            migrationBuilder.CreateIndex(
                name: "IX_rrhh_logchecador_EmpresaId_FechaUtc",
                table: "rrhh_logchecador",
                columns: new[] { "EmpresaId", "FechaUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_rrhh_marcacion_ChecadorId_FechaHoraMarcacionUtc",
                table: "rrhh_marcacion",
                columns: new[] { "ChecadorId", "FechaHoraMarcacionUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_rrhh_marcacion_EmpleadoId",
                table: "rrhh_marcacion",
                column: "EmpleadoId");

            migrationBuilder.CreateIndex(
                name: "IX_rrhh_marcacion_EmpresaId_CodigoChecador_FechaHoraMarcacionUtc",
                table: "rrhh_marcacion",
                columns: new[] { "EmpresaId", "CodigoChecador", "FechaHoraMarcacionUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_rrhh_marcacion_HashUnico",
                table: "rrhh_marcacion",
                column: "HashUnico",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "rrhh_asistencia");

            migrationBuilder.DropTable(
                name: "rrhh_logchecador");

            migrationBuilder.DropTable(
                name: "rrhh_marcacion");

            migrationBuilder.DropTable(
                name: "rrhh_checador");

            migrationBuilder.DropIndex(
                name: "IX_Empleados_EmpresaId_CodigoChecador",
                table: "Empleados");

            migrationBuilder.DropColumn(
                name: "CodigoChecador",
                table: "Empleados");
        }
    }
}
