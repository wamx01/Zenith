using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class InventarioFinishedGoodsInicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InventariosFinishedGoods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EmpresaId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ClienteId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ProductoId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ProductoVarianteId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Sku = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Talla = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Color = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CantidadDisponible = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CantidadReservada = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Ubicacion = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
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
                    table.PrimaryKey("PK_InventariosFinishedGoods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventariosFinishedGoods_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventariosFinishedGoods_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InventariosFinishedGoods_ProductosVariantes_ProductoVariante~",
                        column: x => x.ProductoVarianteId,
                        principalTable: "ProductosVariantes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventariosFinishedGoods_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "MovimientosFinishedGoods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EmpresaId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    InventarioFinishedGoodId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TipoMovimiento = table.Column<int>(type: "int", nullable: false),
                    Cantidad = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ExistenciaAnterior = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ExistenciaNueva = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    FechaMovimiento = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Referencia = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Observaciones = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PedidoId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    PedidoSerigrafiaId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    PedidoDetalleTallaId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    NotaEntregaId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
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
                    table.PrimaryKey("PK_MovimientosFinishedGoods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovimientosFinishedGoods_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MovimientosFinishedGoods_InventariosFinishedGoods_Inventario~",
                        column: x => x.InventarioFinishedGoodId,
                        principalTable: "InventariosFinishedGoods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MovimientosFinishedGoods_NotasEntrega_NotaEntregaId",
                        column: x => x.NotaEntregaId,
                        principalTable: "NotasEntrega",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_MovimientosFinishedGoods_PedidosDetalleTalla_PedidoDetalleTa~",
                        column: x => x.PedidoDetalleTallaId,
                        principalTable: "PedidosDetalleTalla",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_MovimientosFinishedGoods_PedidosSerigrafia_PedidoSerigrafiaId",
                        column: x => x.PedidoSerigrafiaId,
                        principalTable: "PedidosSerigrafia",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_MovimientosFinishedGoods_Pedidos_PedidoId",
                        column: x => x.PedidoId,
                        principalTable: "Pedidos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_InventariosFinishedGoods_ClienteId",
                table: "InventariosFinishedGoods",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_InventariosFinishedGoods_EmpresaId_ClienteId_ProductoVariant~",
                table: "InventariosFinishedGoods",
                columns: new[] { "EmpresaId", "ClienteId", "ProductoVarianteId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventariosFinishedGoods_EmpresaId_Sku",
                table: "InventariosFinishedGoods",
                columns: new[] { "EmpresaId", "Sku" });

            migrationBuilder.CreateIndex(
                name: "IX_InventariosFinishedGoods_ProductoId",
                table: "InventariosFinishedGoods",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_InventariosFinishedGoods_ProductoVarianteId",
                table: "InventariosFinishedGoods",
                column: "ProductoVarianteId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosFinishedGoods_EmpresaId",
                table: "MovimientosFinishedGoods",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosFinishedGoods_FechaMovimiento",
                table: "MovimientosFinishedGoods",
                column: "FechaMovimiento");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosFinishedGoods_InventarioFinishedGoodId",
                table: "MovimientosFinishedGoods",
                column: "InventarioFinishedGoodId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosFinishedGoods_NotaEntregaId",
                table: "MovimientosFinishedGoods",
                column: "NotaEntregaId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosFinishedGoods_PedidoDetalleTallaId",
                table: "MovimientosFinishedGoods",
                column: "PedidoDetalleTallaId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosFinishedGoods_PedidoId",
                table: "MovimientosFinishedGoods",
                column: "PedidoId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosFinishedGoods_PedidoSerigrafiaId",
                table: "MovimientosFinishedGoods",
                column: "PedidoSerigrafiaId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosFinishedGoods_TipoMovimiento",
                table: "MovimientosFinishedGoods",
                column: "TipoMovimiento");

            migrationBuilder.Sql(@"
INSERT INTO Capacidades (Id, Clave, Nombre, Modulo, Descripcion)
SELECT UUID(), 'inventario.fg.ver', 'Ver inventario FG', 'Inventario', 'Consulta del inventario de finished goods por cliente y SKU'
WHERE NOT EXISTS (SELECT 1 FROM Capacidades WHERE Clave = 'inventario.fg.ver');

INSERT INTO Capacidades (Id, Clave, Nombre, Modulo, Descripcion)
SELECT UUID(), 'inventario.fg.ajustar', 'Ajustar inventario FG', 'Inventario', 'Ajustes manuales de finished goods sin registrar entregas desde la pantalla'
WHERE NOT EXISTS (SELECT 1 FROM Capacidades WHERE Clave = 'inventario.fg.ajustar');

INSERT INTO Capacidades (Id, Clave, Nombre, Modulo, Descripcion)
SELECT UUID(), 'inventario.fg.ingresar', 'Ingresar a inventario FG', 'Inventario', 'Permite ingresar SKU completos desde producción a finished goods'
WHERE NOT EXISTS (SELECT 1 FROM Capacidades WHERE Clave = 'inventario.fg.ingresar');

INSERT INTO Capacidades (Id, Clave, Nombre, Modulo, Descripcion)
SELECT UUID(), 'pedidos.produccion.reabrir', 'Reabrir producción cerrada', 'Pedidos', 'Permite editar checks y fechas de proceso en pedidos ya producidos o entregados'
WHERE NOT EXISTS (SELECT 1 FROM Capacidades WHERE Clave = 'pedidos.produccion.reabrir');

INSERT INTO TipoUsuarioCapacidades (TipoUsuarioId, CapacidadId)
SELECT tu.Id, c.Id
FROM TiposUsuario tu
JOIN Capacidades c ON c.Clave IN ('inventario.fg.ver', 'inventario.fg.ajustar', 'inventario.fg.ingresar')
WHERE tu.Nombre = 'Administrador'
AND NOT EXISTS (
    SELECT 1 FROM TipoUsuarioCapacidades tuc
    WHERE tuc.TipoUsuarioId = tu.Id AND tuc.CapacidadId = c.Id
);

INSERT INTO TipoUsuarioCapacidades (TipoUsuarioId, CapacidadId)
SELECT tu.Id, c.Id
FROM TiposUsuario tu
JOIN Capacidades c ON c.Clave IN ('inventario.fg.ver', 'inventario.fg.ajustar', 'inventario.fg.ingresar')
WHERE tu.Nombre = 'Producción'
AND NOT EXISTS (
    SELECT 1 FROM TipoUsuarioCapacidades tuc
    WHERE tuc.TipoUsuarioId = tu.Id AND tuc.CapacidadId = c.Id
);

INSERT INTO TipoUsuarioCapacidades (TipoUsuarioId, CapacidadId)
SELECT tu.Id, c.Id
FROM TiposUsuario tu
JOIN Capacidades c ON c.Clave IN ('inventario.fg.ver', 'pedidos.produccion.reabrir')
WHERE tu.Nombre = 'Gerente'
AND NOT EXISTS (
    SELECT 1 FROM TipoUsuarioCapacidades tuc
    WHERE tuc.TipoUsuarioId = tu.Id AND tuc.CapacidadId = c.Id
);

INSERT INTO TipoUsuarioCapacidades (TipoUsuarioId, CapacidadId)
SELECT tu.Id, c.Id
FROM TiposUsuario tu
JOIN Capacidades c ON c.Clave IN ('pedidos.produccion.reabrir')
WHERE tu.Nombre = 'Administrador'
AND NOT EXISTS (
    SELECT 1 FROM TipoUsuarioCapacidades tuc
    WHERE tuc.TipoUsuarioId = tu.Id AND tuc.CapacidadId = c.Id
);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MovimientosFinishedGoods");

            migrationBuilder.DropTable(
                name: "InventariosFinishedGoods");
        }
    }
}
