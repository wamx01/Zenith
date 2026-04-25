using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class CalzadoFraccionesPorCliente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AplicaFraccionCalzado",
                table: "ProductosClientes",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "ClienteFraccionCalzadoId",
                table: "ProductosClientes",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<string>(
                name: "TallaBaseCalzado",
                table: "ProductosClientes",
                type: "varchar(10)",
                maxLength: 10,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "AplicaFraccionCalzado",
                table: "PedidoDetalles",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "ClienteFraccionCalzadoId",
                table: "PedidoDetalles",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<string>(
                name: "TallaBaseCalzado",
                table: "PedidoDetalles",
                type: "varchar(10)",
                maxLength: 10,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ClientesFraccionesCalzado",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EmpresaId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ClienteId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Codigo = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Nombre = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UnidadesPorFraccion = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Activa = table.Column<bool>(type: "tinyint(1)", nullable: false),
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
                    table.PrimaryKey("PK_ClientesFraccionesCalzado", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientesFraccionesCalzado_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClientesFraccionesCalzado_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ClientesTallasCalzado",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EmpresaId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ClienteId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Talla = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    EsTallaBaseDefault = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    PorcentajeVariacionDefault = table.Column<decimal>(type: "decimal(9,4)", precision: 9, scale: 4, nullable: false),
                    Activa = table.Column<bool>(type: "tinyint(1)", nullable: false),
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
                    table.PrimaryKey("PK_ClientesTallasCalzado", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientesTallasCalzado_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClientesTallasCalzado_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ClientesFraccionesCalzadoDetalle",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EmpresaId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ClienteFraccionCalzadoId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ClienteTallaCalzadoId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    Talla = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    Unidades = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PorcentajeVariacion = table.Column<decimal>(type: "decimal(9,4)", precision: 9, scale: 4, nullable: false),
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
                    table.PrimaryKey("PK_ClientesFraccionesCalzadoDetalle", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientesFraccionesCalzadoDetalle_ClientesFraccionesCalzado_C~",
                        column: x => x.ClienteFraccionCalzadoId,
                        principalTable: "ClientesFraccionesCalzado",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClientesFraccionesCalzadoDetalle_ClientesTallasCalzado_Clien~",
                        column: x => x.ClienteTallaCalzadoId,
                        principalTable: "ClientesTallasCalzado",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ClientesFraccionesCalzadoDetalle_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ProductosClientes_ClienteFraccionCalzadoId",
                table: "ProductosClientes",
                column: "ClienteFraccionCalzadoId");

            migrationBuilder.CreateIndex(
                name: "IX_PedidoDetalles_ClienteFraccionCalzadoId",
                table: "PedidoDetalles",
                column: "ClienteFraccionCalzadoId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientesFraccionesCalzado_ClienteId_Codigo",
                table: "ClientesFraccionesCalzado",
                columns: new[] { "ClienteId", "Codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClientesFraccionesCalzado_ClienteId_Nombre",
                table: "ClientesFraccionesCalzado",
                columns: new[] { "ClienteId", "Nombre" });

            migrationBuilder.CreateIndex(
                name: "IX_ClientesFraccionesCalzado_EmpresaId",
                table: "ClientesFraccionesCalzado",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientesFraccionesCalzadoDetalle_ClienteFraccionCalzadoId",
                table: "ClientesFraccionesCalzadoDetalle",
                column: "ClienteFraccionCalzadoId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientesFraccionesCalzadoDetalle_ClienteFraccionCalzadoId_Or~",
                table: "ClientesFraccionesCalzadoDetalle",
                columns: new[] { "ClienteFraccionCalzadoId", "Orden" });

            migrationBuilder.CreateIndex(
                name: "IX_ClientesFraccionesCalzadoDetalle_ClienteFraccionCalzadoId_Ta~",
                table: "ClientesFraccionesCalzadoDetalle",
                columns: new[] { "ClienteFraccionCalzadoId", "Talla" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClientesFraccionesCalzadoDetalle_ClienteTallaCalzadoId",
                table: "ClientesFraccionesCalzadoDetalle",
                column: "ClienteTallaCalzadoId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientesFraccionesCalzadoDetalle_EmpresaId",
                table: "ClientesFraccionesCalzadoDetalle",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientesTallasCalzado_ClienteId_Orden",
                table: "ClientesTallasCalzado",
                columns: new[] { "ClienteId", "Orden" });

            migrationBuilder.CreateIndex(
                name: "IX_ClientesTallasCalzado_ClienteId_Talla",
                table: "ClientesTallasCalzado",
                columns: new[] { "ClienteId", "Talla" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClientesTallasCalzado_EmpresaId",
                table: "ClientesTallasCalzado",
                column: "EmpresaId");

            migrationBuilder.AddForeignKey(
                name: "FK_PedidoDetalles_ClientesFraccionesCalzado_ClienteFraccionCalz~",
                table: "PedidoDetalles",
                column: "ClienteFraccionCalzadoId",
                principalTable: "ClientesFraccionesCalzado",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductosClientes_ClientesFraccionesCalzado_ClienteFraccionC~",
                table: "ProductosClientes",
                column: "ClienteFraccionCalzadoId",
                principalTable: "ClientesFraccionesCalzado",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PedidoDetalles_ClientesFraccionesCalzado_ClienteFraccionCalz~",
                table: "PedidoDetalles");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductosClientes_ClientesFraccionesCalzado_ClienteFraccionC~",
                table: "ProductosClientes");

            migrationBuilder.DropTable(
                name: "ClientesFraccionesCalzadoDetalle");

            migrationBuilder.DropTable(
                name: "ClientesFraccionesCalzado");

            migrationBuilder.DropTable(
                name: "ClientesTallasCalzado");

            migrationBuilder.DropIndex(
                name: "IX_ProductosClientes_ClienteFraccionCalzadoId",
                table: "ProductosClientes");

            migrationBuilder.DropIndex(
                name: "IX_PedidoDetalles_ClienteFraccionCalzadoId",
                table: "PedidoDetalles");

            migrationBuilder.DropColumn(
                name: "AplicaFraccionCalzado",
                table: "ProductosClientes");

            migrationBuilder.DropColumn(
                name: "ClienteFraccionCalzadoId",
                table: "ProductosClientes");

            migrationBuilder.DropColumn(
                name: "TallaBaseCalzado",
                table: "ProductosClientes");

            migrationBuilder.DropColumn(
                name: "AplicaFraccionCalzado",
                table: "PedidoDetalles");

            migrationBuilder.DropColumn(
                name: "ClienteFraccionCalzadoId",
                table: "PedidoDetalles");

            migrationBuilder.DropColumn(
                name: "TallaBaseCalzado",
                table: "PedidoDetalles");
        }
    }
}
