using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class CotizacionProcesosAutoPedido : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CotizacionSerigrafiaProcesos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CotizacionSerigrafiaId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TipoProcesoId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
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
                    table.PrimaryKey("PK_CotizacionSerigrafiaProcesos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CotizacionSerigrafiaProcesos_CotizacionesSerigrafia_Cotizaci~",
                        column: x => x.CotizacionSerigrafiaId,
                        principalTable: "CotizacionesSerigrafia",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CotizacionSerigrafiaProcesos_TiposProceso_TipoProcesoId",
                        column: x => x.TipoProcesoId,
                        principalTable: "TiposProceso",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_CotizacionSerigrafiaProcesos_CotizacionSerigrafiaId",
                table: "CotizacionSerigrafiaProcesos",
                column: "CotizacionSerigrafiaId");

            migrationBuilder.CreateIndex(
                name: "IX_CotizacionSerigrafiaProcesos_CotizacionSerigrafiaId_TipoProc~",
                table: "CotizacionSerigrafiaProcesos",
                columns: new[] { "CotizacionSerigrafiaId", "TipoProcesoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CotizacionSerigrafiaProcesos_TipoProcesoId",
                table: "CotizacionSerigrafiaProcesos",
                column: "TipoProcesoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CotizacionSerigrafiaProcesos");
        }
    }
}
