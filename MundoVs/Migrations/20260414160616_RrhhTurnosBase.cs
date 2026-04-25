using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class RrhhTurnosBase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TurnoBaseId",
                table: "Empleados",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateTable(
                name: "rrhh_turno",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EmpresaId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Nombre = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Descripcion = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true)
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
                    table.PrimaryKey("PK_rrhh_turno", x => x.Id);
                    table.ForeignKey(
                        name: "FK_rrhh_turno_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "rrhh_turno_detalle",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TurnoBaseId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    DiaSemana = table.Column<int>(type: "int", nullable: false),
                    Labora = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    HoraEntrada = table.Column<TimeSpan>(type: "time(6)", nullable: true),
                    HoraSalida = table.Column<TimeSpan>(type: "time(6)", nullable: true),
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
                    table.PrimaryKey("PK_rrhh_turno_detalle", x => x.Id);
                    table.ForeignKey(
                        name: "FK_rrhh_turno_detalle_rrhh_turno_TurnoBaseId",
                        column: x => x.TurnoBaseId,
                        principalTable: "rrhh_turno",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Empleados_TurnoBaseId",
                table: "Empleados",
                column: "TurnoBaseId");

            migrationBuilder.CreateIndex(
                name: "IX_rrhh_turno_EmpresaId_Nombre",
                table: "rrhh_turno",
                columns: new[] { "EmpresaId", "Nombre" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_rrhh_turno_detalle_TurnoBaseId_DiaSemana",
                table: "rrhh_turno_detalle",
                columns: new[] { "TurnoBaseId", "DiaSemana" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Empleados_rrhh_turno_TurnoBaseId",
                table: "Empleados",
                column: "TurnoBaseId",
                principalTable: "rrhh_turno",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Empleados_rrhh_turno_TurnoBaseId",
                table: "Empleados");

            migrationBuilder.DropTable(
                name: "rrhh_turno_detalle");

            migrationBuilder.DropTable(
                name: "rrhh_turno");

            migrationBuilder.DropIndex(
                name: "IX_Empleados_TurnoBaseId",
                table: "Empleados");

            migrationBuilder.DropColumn(
                name: "TurnoBaseId",
                table: "Empleados");
        }
    }
}
