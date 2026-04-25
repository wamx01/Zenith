using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class RrhhPrenominaCapturaRapida : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "rrhh_prenomina_bono",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PrenominaDetalleId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    BonoRubroRrhhId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Importe = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("PK_rrhh_prenomina_bono", x => x.Id);
                    table.ForeignKey(
                        name: "FK_rrhh_prenomina_bono_PrenominaDetalles_PrenominaDetalleId",
                        column: x => x.PrenominaDetalleId,
                        principalTable: "PrenominaDetalles",
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
                    Importe = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Referencia = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
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
                    table.PrimaryKey("PK_rrhh_prenomina_percepcion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_rrhh_prenomina_percepcion_PrenominaDetalles_PrenominaDetalle~",
                        column: x => x.PrenominaDetalleId,
                        principalTable: "PrenominaDetalles",
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "rrhh_prenomina_bono");

            migrationBuilder.DropTable(
                name: "rrhh_prenomina_percepcion");
        }
    }
}
