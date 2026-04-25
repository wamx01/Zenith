using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class PedidoConceptosYEmpleadoSeguimiento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "EmpleadoId",
                table: "PedidoSerigrafiaTallaProcesos",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateTable(
                name: "PedidoConceptos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PedidoId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    Descripcion = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("PK_PedidoConceptos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PedidoConceptos_Pedidos_PedidoId",
                        column: x => x.PedidoId,
                        principalTable: "Pedidos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_PedidoSerigrafiaTallaProcesos_EmpleadoId",
                table: "PedidoSerigrafiaTallaProcesos",
                column: "EmpleadoId");

            migrationBuilder.CreateIndex(
                name: "IX_PedidoConceptos_PedidoId",
                table: "PedidoConceptos",
                column: "PedidoId");

            migrationBuilder.CreateIndex(
                name: "IX_PedidoConceptos_Tipo",
                table: "PedidoConceptos",
                column: "Tipo");

            migrationBuilder.AddForeignKey(
                name: "FK_PedidoSerigrafiaTallaProcesos_Empleados_EmpleadoId",
                table: "PedidoSerigrafiaTallaProcesos",
                column: "EmpleadoId",
                principalTable: "Empleados",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PedidoSerigrafiaTallaProcesos_Empleados_EmpleadoId",
                table: "PedidoSerigrafiaTallaProcesos");

            migrationBuilder.DropTable(
                name: "PedidoConceptos");

            migrationBuilder.DropIndex(
                name: "IX_PedidoSerigrafiaTallaProcesos_EmpleadoId",
                table: "PedidoSerigrafiaTallaProcesos");

            migrationBuilder.DropColumn(
                name: "EmpleadoId",
                table: "PedidoSerigrafiaTallaProcesos");
        }
    }
}
