using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class RrhhBonosDistribuidosDetalleRubro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "rrhh_bono_distribucion_empleado_detalle",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    BonoDistribucionEmpleadoRrhhId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    BonoRubroRrhhId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Porcentaje = table.Column<decimal>(type: "decimal(9,4)", precision: 9, scale: 4, nullable: false),
                    MontoAsignado = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("PK_rrhh_bono_distribucion_empleado_detalle", x => x.Id);
                    table.ForeignKey(
                        name: "FK_rrhh_bono_distribucion_empleado_detalle_rrhh_bono_distribuci~",
                        column: x => x.BonoDistribucionEmpleadoRrhhId,
                        principalTable: "rrhh_bono_distribucion_empleado",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_rrhh_bono_distribucion_empleado_detalle_rrhh_bono_rubro_Bono~",
                        column: x => x.BonoRubroRrhhId,
                        principalTable: "rrhh_bono_rubro",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_rrhh_bono_distribucion_empleado_detalle_BonoDistribucionEmp~1",
                table: "rrhh_bono_distribucion_empleado_detalle",
                columns: new[] { "BonoDistribucionEmpleadoRrhhId", "BonoRubroRrhhId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_rrhh_bono_distribucion_empleado_detalle_BonoDistribucionEmpl~",
                table: "rrhh_bono_distribucion_empleado_detalle",
                column: "BonoDistribucionEmpleadoRrhhId");

            migrationBuilder.CreateIndex(
                name: "IX_rrhh_bono_distribucion_empleado_detalle_BonoRubroRrhhId",
                table: "rrhh_bono_distribucion_empleado_detalle",
                column: "BonoRubroRrhhId");

            migrationBuilder.Sql(@"
INSERT INTO rrhh_bono_distribucion_empleado_detalle
    (Id, BonoDistribucionEmpleadoRrhhId, BonoRubroRrhhId, Porcentaje, MontoAsignado, Orden, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, IsActive)
SELECT
    UUID(),
    e.Id,
    ed.BonoRubroRrhhId,
    ROUND(e.Porcentaje * (ed.Porcentaje / 100), 4),
    ROUND(e.MontoAsignado * (ed.Porcentaje / 100), 2),
    ed.Orden,
    e.CreatedAt,
    e.UpdatedAt,
    e.CreatedBy,
    e.UpdatedBy,
    e.IsActive
FROM rrhh_bono_distribucion_empleado e
INNER JOIN rrhh_bono_distribucion_periodo p ON p.Id = e.BonoDistribucionPeriodoRrhhId
INNER JOIN rrhh_bono_estructura_detalle ed ON ed.BonoEstructuraRrhhId = p.BonoEstructuraRrhhId
WHERE e.IsActive = 1 AND ed.IsActive = 1 AND e.Porcentaje > 0;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "rrhh_bono_distribucion_empleado_detalle");
        }
    }
}
