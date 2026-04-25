using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class RegistroDestajoProceso : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "TarifaDestajo",
                table: "TiposProceso",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "RegistrosDestajoProceso",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PedidoSerigrafiaId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PedidoSerigrafiaTallaProcesoId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PedidoSerigrafiaTallaId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TipoProcesoId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EmpleadoId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Fecha = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CantidadProcesada = table.Column<int>(type: "int", nullable: false),
                    TarifaUnitario = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Importe = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TiempoMinutos = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_RegistrosDestajoProceso", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RegistrosDestajoProceso_Empleados_EmpleadoId",
                        column: x => x.EmpleadoId,
                        principalTable: "Empleados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RegistrosDestajoProceso_PedidoSerigrafiaTallaProcesos_Pedido~",
                        column: x => x.PedidoSerigrafiaTallaProcesoId,
                        principalTable: "PedidoSerigrafiaTallaProcesos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RegistrosDestajoProceso_PedidoSerigrafiaTallas_PedidoSerigra~",
                        column: x => x.PedidoSerigrafiaTallaId,
                        principalTable: "PedidoSerigrafiaTallas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RegistrosDestajoProceso_PedidosSerigrafia_PedidoSerigrafiaId",
                        column: x => x.PedidoSerigrafiaId,
                        principalTable: "PedidosSerigrafia",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RegistrosDestajoProceso_TiposProceso_TipoProcesoId",
                        column: x => x.TipoProcesoId,
                        principalTable: "TiposProceso",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosDestajoProceso_EmpleadoId",
                table: "RegistrosDestajoProceso",
                column: "EmpleadoId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosDestajoProceso_Fecha",
                table: "RegistrosDestajoProceso",
                column: "Fecha");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosDestajoProceso_PedidoSerigrafiaId",
                table: "RegistrosDestajoProceso",
                column: "PedidoSerigrafiaId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosDestajoProceso_PedidoSerigrafiaTallaId",
                table: "RegistrosDestajoProceso",
                column: "PedidoSerigrafiaTallaId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosDestajoProceso_PedidoSerigrafiaTallaProcesoId",
                table: "RegistrosDestajoProceso",
                column: "PedidoSerigrafiaTallaProcesoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosDestajoProceso_TipoProcesoId",
                table: "RegistrosDestajoProceso",
                column: "TipoProcesoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RegistrosDestajoProceso");

            migrationBuilder.DropColumn(
                name: "TarifaDestajo",
                table: "TiposProceso");
        }
    }
}
