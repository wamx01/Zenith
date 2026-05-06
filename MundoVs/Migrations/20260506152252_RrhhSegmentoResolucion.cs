using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class RrhhSegmentoResolucion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "rrhh_segmento_resolucion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EmpresaId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EmpleadoId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    MarcacionInicioId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    MarcacionFinId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TipoSegmento = table.Column<int>(type: "int", nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    FueInferidoAutomaticamente = table.Column<bool>(type: "tinyint(1)", nullable: false),
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
                    table.PrimaryKey("PK_rrhh_segmento_resolucion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_rrhh_segmento_resolucion_empleados_EmpleadoId",
                        column: x => x.EmpleadoId,
                        principalTable: "empleados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_rrhh_segmento_resolucion_empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_rrhh_segmento_resolucion_rrhh_marcacion_MarcacionFinId",
                        column: x => x.MarcacionFinId,
                        principalTable: "rrhh_marcacion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_rrhh_segmento_resolucion_rrhh_marcacion_MarcacionInicioId",
                        column: x => x.MarcacionInicioId,
                        principalTable: "rrhh_marcacion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_rrhh_segmento_resolucion_EmpleadoId",
                table: "rrhh_segmento_resolucion",
                column: "EmpleadoId");

            migrationBuilder.CreateIndex(
                name: "IX_rrhh_segmento_resolucion_EmpresaId_EmpleadoId_Fecha_Estado",
                table: "rrhh_segmento_resolucion",
                columns: new[] { "EmpresaId", "EmpleadoId", "Fecha", "Estado" });

            migrationBuilder.CreateIndex(
                name: "IX_rrhh_segmento_resolucion_EmpresaId_EmpleadoId_Fecha_Marcacio~",
                table: "rrhh_segmento_resolucion",
                columns: new[] { "EmpresaId", "EmpleadoId", "Fecha", "MarcacionInicioId", "MarcacionFinId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_rrhh_segmento_resolucion_MarcacionFinId",
                table: "rrhh_segmento_resolucion",
                column: "MarcacionFinId");

            migrationBuilder.CreateIndex(
                name: "IX_rrhh_segmento_resolucion_MarcacionInicioId",
                table: "rrhh_segmento_resolucion",
                column: "MarcacionInicioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "rrhh_segmento_resolucion");
        }
    }
}
