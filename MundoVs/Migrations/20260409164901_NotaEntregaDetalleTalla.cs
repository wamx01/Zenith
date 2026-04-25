using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class NotaEntregaDetalleTalla : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NotasEntregaDetalleTalla",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EmpresaId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    NotaEntregaDetalleId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PedidoDetalleTallaId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    Talla = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    Cantidad = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
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
                    table.PrimaryKey("PK_NotasEntregaDetalleTalla", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotasEntregaDetalleTalla_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NotasEntregaDetalleTalla_NotasEntregaDetalle_NotaEntregaDeta~",
                        column: x => x.NotaEntregaDetalleId,
                        principalTable: "NotasEntregaDetalle",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_NotasEntregaDetalleTalla_EmpresaId",
                table: "NotasEntregaDetalleTalla",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_NotasEntregaDetalleTalla_NotaEntregaDetalleId",
                table: "NotasEntregaDetalleTalla",
                column: "NotaEntregaDetalleId");

            migrationBuilder.CreateIndex(
                name: "IX_NotasEntregaDetalleTalla_NotaEntregaDetalleId_Talla",
                table: "NotasEntregaDetalleTalla",
                columns: new[] { "NotaEntregaDetalleId", "Talla" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NotasEntregaDetalleTalla");
        }
    }
}
