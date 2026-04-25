using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class AgregarRelacionTiposProceso : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Orden",
                table: "TiposProceso",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "PedidoSerigrafiaProcesoDetalles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PedidoSerigrafiaId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
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
                    table.PrimaryKey("PK_PedidoSerigrafiaProcesoDetalles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PedidoSerigrafiaProcesoDetalles_PedidosSerigrafia_PedidoSeri~",
                        column: x => x.PedidoSerigrafiaId,
                        principalTable: "PedidosSerigrafia",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PedidoSerigrafiaProcesoDetalles_TiposProceso_TipoProcesoId",
                        column: x => x.TipoProcesoId,
                        principalTable: "TiposProceso",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_TiposProceso_Activo",
                table: "TiposProceso",
                column: "Activo");

            migrationBuilder.CreateIndex(
                name: "IX_TiposProceso_Nombre",
                table: "TiposProceso",
                column: "Nombre");

            migrationBuilder.CreateIndex(
                name: "IX_PedidoSerigrafiaProcesoDetalles_PedidoSerigrafiaId",
                table: "PedidoSerigrafiaProcesoDetalles",
                column: "PedidoSerigrafiaId");

            migrationBuilder.CreateIndex(
                name: "IX_PedidoSerigrafiaProcesoDetalles_TipoProcesoId",
                table: "PedidoSerigrafiaProcesoDetalles",
                column: "TipoProcesoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PedidoSerigrafiaProcesoDetalles");

            migrationBuilder.DropIndex(
                name: "IX_TiposProceso_Activo",
                table: "TiposProceso");

            migrationBuilder.DropIndex(
                name: "IX_TiposProceso_Nombre",
                table: "TiposProceso");

            migrationBuilder.DropColumn(
                name: "Orden",
                table: "TiposProceso");
        }
    }
}
