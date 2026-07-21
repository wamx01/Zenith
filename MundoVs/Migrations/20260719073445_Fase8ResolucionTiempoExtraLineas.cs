using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class Fase8ResolucionTiempoExtraLineas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "HorasExtraFactoradas",
                table: "rrhh_resolucion_tiempo_extra_periodo",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "MinutosExtraSimples",
                table: "rrhh_resolucion_tiempo_extra_periodo",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "HorasExtraFactoradas",
                table: "nominadetalles",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "rrhh_resolucion_tiempo_extra_linea",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EmpresaId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EmpleadoId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ResolucionPeriodoId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    Destino = table.Column<int>(type: "int", nullable: false),
                    Minutos = table.Column<int>(type: "int", nullable: false),
                    Factor = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
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
                    table.PrimaryKey("PK_rrhh_resolucion_tiempo_extra_linea", x => x.Id);
                    table.ForeignKey(
                        name: "FK_rrhh_resolucion_tiempo_extra_linea_rrhh_resolucion_tiempo_ex~",
                        column: x => x.ResolucionPeriodoId,
                        principalTable: "rrhh_resolucion_tiempo_extra_periodo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_rrhh_resolucion_tiempo_extra_linea_EmpresaId_EmpleadoId",
                table: "rrhh_resolucion_tiempo_extra_linea",
                columns: new[] { "EmpresaId", "EmpleadoId" });

            migrationBuilder.CreateIndex(
                name: "IX_rrhh_resolucion_tiempo_extra_linea_ResolucionPeriodoId",
                table: "rrhh_resolucion_tiempo_extra_linea",
                column: "ResolucionPeriodoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "rrhh_resolucion_tiempo_extra_linea");

            migrationBuilder.DropColumn(
                name: "HorasExtraFactoradas",
                table: "rrhh_resolucion_tiempo_extra_periodo");

            migrationBuilder.DropColumn(
                name: "MinutosExtraSimples",
                table: "rrhh_resolucion_tiempo_extra_periodo");

            migrationBuilder.DropColumn(
                name: "HorasExtraFactoradas",
                table: "nominadetalles");
        }
    }
}
