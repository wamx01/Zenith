using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class RrhhBonosEstructurados : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "rrhh_bono_rubro",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EmpresaId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Clave = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Nombre = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Descripcion = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Orden = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_rrhh_bono_rubro", x => x.Id);
                    table.ForeignKey(
                        name: "FK_rrhh_bono_rubro_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "rrhh_nomina_bono",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EmpresaId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    NominaDetalleId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TipoCaptura = table.Column<int>(type: "int", nullable: false),
                    MontoTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("PK_rrhh_nomina_bono", x => x.Id);
                    table.ForeignKey(
                        name: "FK_rrhh_nomina_bono_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_rrhh_nomina_bono_NominaDetalles_NominaDetalleId",
                        column: x => x.NominaDetalleId,
                        principalTable: "NominaDetalles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "rrhh_nomina_bono_detalle",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    NominaBonoId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    BonoRubroRrhhId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Porcentaje = table.Column<decimal>(type: "decimal(9,4)", precision: 9, scale: 4, nullable: false),
                    Importe = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_rrhh_nomina_bono_detalle", x => x.Id);
                    table.ForeignKey(
                        name: "FK_rrhh_nomina_bono_detalle_rrhh_bono_rubro_BonoRubroRrhhId",
                        column: x => x.BonoRubroRrhhId,
                        principalTable: "rrhh_bono_rubro",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_rrhh_nomina_bono_detalle_rrhh_nomina_bono_NominaBonoId",
                        column: x => x.NominaBonoId,
                        principalTable: "rrhh_nomina_bono",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_rrhh_bono_rubro_EmpresaId_Clave",
                table: "rrhh_bono_rubro",
                columns: new[] { "EmpresaId", "Clave" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_rrhh_bono_rubro_EmpresaId_Nombre",
                table: "rrhh_bono_rubro",
                columns: new[] { "EmpresaId", "Nombre" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_rrhh_nomina_bono_EmpresaId_NominaDetalleId",
                table: "rrhh_nomina_bono",
                columns: new[] { "EmpresaId", "NominaDetalleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_rrhh_nomina_bono_NominaDetalleId",
                table: "rrhh_nomina_bono",
                column: "NominaDetalleId");

            migrationBuilder.CreateIndex(
                name: "IX_rrhh_nomina_bono_detalle_BonoRubroRrhhId",
                table: "rrhh_nomina_bono_detalle",
                column: "BonoRubroRrhhId");

            migrationBuilder.CreateIndex(
                name: "IX_rrhh_nomina_bono_detalle_NominaBonoId",
                table: "rrhh_nomina_bono_detalle",
                column: "NominaBonoId");

            migrationBuilder.CreateIndex(
                name: "IX_rrhh_nomina_bono_detalle_NominaBonoId_BonoRubroRrhhId",
                table: "rrhh_nomina_bono_detalle",
                columns: new[] { "NominaBonoId", "BonoRubroRrhhId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "rrhh_nomina_bono_detalle");

            migrationBuilder.DropTable(
                name: "rrhh_bono_rubro");

            migrationBuilder.DropTable(
                name: "rrhh_nomina_bono");
        }
    }
}
