using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class NominaPrenominaImssVacaciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PrenominaId",
                table: "Nominas",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
                name: "AplicaImss",
                table: "NominaDetalles",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "ComplementoSalarioMinimo",
                table: "NominaDetalles",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CuotaImssObrera",
                table: "NominaDetalles",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CuotaImssPatronal",
                table: "NominaDetalles",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "DiasDescansoTrabajado",
                table: "NominaDetalles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DiasFaltaInjustificada",
                table: "NominaDetalles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DiasFaltaJustificada",
                table: "NominaDetalles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DiasFestivoTrabajado",
                table: "NominaDetalles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DiasIncapacidad",
                table: "NominaDetalles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DiasPagados",
                table: "NominaDetalles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DiasTrabajados",
                table: "NominaDetalles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DiasVacaciones",
                table: "NominaDetalles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "MontoPrimaVacacional",
                table: "NominaDetalles",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "AplicaImss",
                table: "Empleados",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Prenominas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EmpresaId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Periodo = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FechaInicio = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Estatus = table.Column<int>(type: "int", nullable: false),
                    Notas = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
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
                    table.PrimaryKey("PK_Prenominas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Prenominas_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PrenominaDetalles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PrenominaId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EmpleadoId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    DiasTrabajados = table.Column<int>(type: "int", nullable: false),
                    DiasPagados = table.Column<int>(type: "int", nullable: false),
                    DiasVacaciones = table.Column<int>(type: "int", nullable: false),
                    DiasFaltaJustificada = table.Column<int>(type: "int", nullable: false),
                    DiasFaltaInjustificada = table.Column<int>(type: "int", nullable: false),
                    DiasIncapacidad = table.Column<int>(type: "int", nullable: false),
                    DiasDescansoTrabajado = table.Column<int>(type: "int", nullable: false),
                    DiasFestivoTrabajado = table.Column<int>(type: "int", nullable: false),
                    AplicaImss = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    HorasExtra = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MontoDestajoInformativo = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DiasVacacionesDisponibles = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DiasVacacionesRestantes = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ComplementoSalarioMinimoSugerido = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Notas = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
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
                    table.PrimaryKey("PK_PrenominaDetalles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrenominaDetalles_Empleados_EmpleadoId",
                        column: x => x.EmpleadoId,
                        principalTable: "Empleados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrenominaDetalles_Prenominas_PrenominaId",
                        column: x => x.PrenominaId,
                        principalTable: "Prenominas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Nominas_PrenominaId",
                table: "Nominas",
                column: "PrenominaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Empleados_AplicaImss",
                table: "Empleados",
                column: "AplicaImss");

            migrationBuilder.CreateIndex(
                name: "IX_PrenominaDetalles_EmpleadoId",
                table: "PrenominaDetalles",
                column: "EmpleadoId");

            migrationBuilder.CreateIndex(
                name: "IX_PrenominaDetalles_PrenominaId_EmpleadoId",
                table: "PrenominaDetalles",
                columns: new[] { "PrenominaId", "EmpleadoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Prenominas_EmpresaId_FechaInicio_FechaFin",
                table: "Prenominas",
                columns: new[] { "EmpresaId", "FechaInicio", "FechaFin" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Prenominas_Estatus",
                table: "Prenominas",
                column: "Estatus");

            migrationBuilder.AddForeignKey(
                name: "FK_Nominas_Prenominas_PrenominaId",
                table: "Nominas",
                column: "PrenominaId",
                principalTable: "Prenominas",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Nominas_Prenominas_PrenominaId",
                table: "Nominas");

            migrationBuilder.DropTable(
                name: "PrenominaDetalles");

            migrationBuilder.DropTable(
                name: "Prenominas");

            migrationBuilder.DropIndex(
                name: "IX_Nominas_PrenominaId",
                table: "Nominas");

            migrationBuilder.DropIndex(
                name: "IX_Empleados_AplicaImss",
                table: "Empleados");

            migrationBuilder.DropColumn(
                name: "PrenominaId",
                table: "Nominas");

            migrationBuilder.DropColumn(
                name: "AplicaImss",
                table: "NominaDetalles");

            migrationBuilder.DropColumn(
                name: "ComplementoSalarioMinimo",
                table: "NominaDetalles");

            migrationBuilder.DropColumn(
                name: "CuotaImssObrera",
                table: "NominaDetalles");

            migrationBuilder.DropColumn(
                name: "CuotaImssPatronal",
                table: "NominaDetalles");

            migrationBuilder.DropColumn(
                name: "DiasDescansoTrabajado",
                table: "NominaDetalles");

            migrationBuilder.DropColumn(
                name: "DiasFaltaInjustificada",
                table: "NominaDetalles");

            migrationBuilder.DropColumn(
                name: "DiasFaltaJustificada",
                table: "NominaDetalles");

            migrationBuilder.DropColumn(
                name: "DiasFestivoTrabajado",
                table: "NominaDetalles");

            migrationBuilder.DropColumn(
                name: "DiasIncapacidad",
                table: "NominaDetalles");

            migrationBuilder.DropColumn(
                name: "DiasPagados",
                table: "NominaDetalles");

            migrationBuilder.DropColumn(
                name: "DiasTrabajados",
                table: "NominaDetalles");

            migrationBuilder.DropColumn(
                name: "DiasVacaciones",
                table: "NominaDetalles");

            migrationBuilder.DropColumn(
                name: "MontoPrimaVacacional",
                table: "NominaDetalles");

            migrationBuilder.DropColumn(
                name: "AplicaImss",
                table: "Empleados");
        }
    }
}
