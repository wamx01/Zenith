using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class AgregarModuloSerigrafiaCompleto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EscalasSerigrafia",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Nombre = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Descripcion = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Talla15 = table.Column<int>(type: "int", nullable: false),
                    Talla16 = table.Column<int>(type: "int", nullable: false),
                    Talla17 = table.Column<int>(type: "int", nullable: false),
                    Talla18 = table.Column<int>(type: "int", nullable: false),
                    Talla19 = table.Column<int>(type: "int", nullable: false),
                    Talla20 = table.Column<int>(type: "int", nullable: false),
                    Talla21 = table.Column<int>(type: "int", nullable: false),
                    Talla22 = table.Column<int>(type: "int", nullable: false),
                    Talla23 = table.Column<int>(type: "int", nullable: false),
                    Talla24 = table.Column<int>(type: "int", nullable: false),
                    Talla25 = table.Column<int>(type: "int", nullable: false),
                    Talla26 = table.Column<int>(type: "int", nullable: false),
                    Talla27 = table.Column<int>(type: "int", nullable: false),
                    Talla28 = table.Column<int>(type: "int", nullable: false),
                    Talla29 = table.Column<int>(type: "int", nullable: false),
                    Talla30 = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_EscalasSerigrafia", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PedidosSerigrafia",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PedidoDetalleId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Estilo = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Combinacion = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Referencia = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LoteCliente = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Corrida = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProcesoMesa = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ProcesoPulpo = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ProcesoTransfer = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ProcesoSublimacion = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ProcesoPlancha = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ProcesoFrecuencia = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ProcesoPoteo = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ProcesoUV = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ProcesoPlastisol = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Talla15 = table.Column<int>(type: "int", nullable: true),
                    Talla16 = table.Column<int>(type: "int", nullable: true),
                    Talla17 = table.Column<int>(type: "int", nullable: true),
                    Talla18 = table.Column<int>(type: "int", nullable: true),
                    Talla19 = table.Column<int>(type: "int", nullable: true),
                    Talla20 = table.Column<int>(type: "int", nullable: true),
                    Talla21 = table.Column<int>(type: "int", nullable: true),
                    Talla22 = table.Column<int>(type: "int", nullable: true),
                    Talla23 = table.Column<int>(type: "int", nullable: true),
                    Talla24 = table.Column<int>(type: "int", nullable: true),
                    Talla25 = table.Column<int>(type: "int", nullable: true),
                    Talla26 = table.Column<int>(type: "int", nullable: true),
                    Talla27 = table.Column<int>(type: "int", nullable: true),
                    Talla28 = table.Column<int>(type: "int", nullable: true),
                    Talla29 = table.Column<int>(type: "int", nullable: true),
                    Talla30 = table.Column<int>(type: "int", nullable: true),
                    FechaRecibido = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    FechaEstimada = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    FechaEntregaReal = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Hecho = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Nota = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
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
                    table.PrimaryKey("PK_PedidosSerigrafia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PedidosSerigrafia_PedidoDetalles_PedidoDetalleId",
                        column: x => x.PedidoDetalleId,
                        principalTable: "PedidoDetalles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TiposProceso",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Nombre = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Descripcion = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CostoBase = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Activo = table.Column<bool>(type: "tinyint(1)", nullable: false),
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
                    table.PrimaryKey("PK_TiposProceso", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CotizacionesSerigrafia",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PedidoSerigrafiaId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TiempoTiradoMaterial = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TiempoCentradoMaterial = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TiempoSerigrafista1 = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TiempoSerigrafista2 = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TiempoRecogidoAmarrado = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TiempoLavadoPantalla = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PrecioTiradoMaterial = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PrecioCentradoMaterial = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PrecioSerigrafista1 = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PrecioSerigrafista2 = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PrecioRecogidoAmarrado = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PrecioLavadoPantalla = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CostoTiradoMaterial = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CostoCentradoMaterial = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CostoSerigrafista1 = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CostoSerigrafista2 = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CostoRecogidoAmarrado = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CostoLavadoPantalla = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    GastoElectricidadPulpo = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    GastoElectricidadPlancha = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    GastosFijos = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    GastoAdministracion = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    GastoLuzGeneral = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Tinta1PesoInicial = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    Tinta1PesoFinal = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    Tinta1Consumo = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    Tinta1PrecioUnitario = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    Tinta1Costo = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    Tinta1Nombre = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Tinta2PesoInicial = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    Tinta2PesoFinal = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    Tinta2Consumo = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    Tinta2PrecioUnitario = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    Tinta2Costo = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    Tinta2Nombre = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Tinta3PesoInicial = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    Tinta3PesoFinal = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    Tinta3Consumo = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    Tinta3PrecioUnitario = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    Tinta3Costo = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    Tinta3Nombre = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Tinta4PesoInicial = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    Tinta4PesoFinal = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    Tinta4Consumo = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    Tinta4PrecioUnitario = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    Tinta4Costo = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    Tinta4Nombre = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Tinta5PesoInicial = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    Tinta5PesoFinal = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    Tinta5Consumo = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    Tinta5PrecioUnitario = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    Tinta5Costo = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    Tinta5Nombre = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Tinta6PesoInicial = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    Tinta6PesoFinal = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    Tinta6Consumo = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    Tinta6PrecioUnitario = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    Tinta6Costo = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    Tinta6Nombre = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Tinta7PesoInicial = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    Tinta7PesoFinal = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    Tinta7Consumo = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    Tinta7PrecioUnitario = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    Tinta7Costo = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    Tinta7Nombre = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Tinta8PesoInicial = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    Tinta8PesoFinal = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    Tinta8Consumo = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    Tinta8PrecioUnitario = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    Tinta8Costo = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    Tinta8Nombre = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    InsumoTrapoBlancoCantidad = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    InsumoTrapoBlancoPrecio = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    InsumoTrapoBlancoCosto = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    InsumoCicloEcoCantidad = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    InsumoCicloEcoPrecio = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    InsumoCicloEcoCosto = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    InsumoDrySiliconCantidad = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    InsumoDrySiliconPrecio = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    InsumoDrySiliconCosto = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    InsumoPTWaxCantidad = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    InsumoPTWaxPrecio = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    InsumoPTWaxCosto = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    InsumoX6Cantidad = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    InsumoX6Precio = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    InsumoX6Costo = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    InsumoCiclo1RACantidad = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    InsumoCiclo1RAPrecio = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    InsumoCiclo1RACosto = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    CostoTotalPorPar = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CostoTotalPorTarea = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Utilidad = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PrecioSugerido = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PrecioFinalContado = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PrecioCredito = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ParesPorLamina = table.Column<int>(type: "int", nullable: false),
                    Ganancia = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("PK_CotizacionesSerigrafia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CotizacionesSerigrafia_PedidosSerigrafia_PedidoSerigrafiaId",
                        column: x => x.PedidoSerigrafiaId,
                        principalTable: "PedidosSerigrafia",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_CotizacionesSerigrafia_PedidoSerigrafiaId",
                table: "CotizacionesSerigrafia",
                column: "PedidoSerigrafiaId");

            migrationBuilder.CreateIndex(
                name: "IX_PedidosSerigrafia_FechaEstimada",
                table: "PedidosSerigrafia",
                column: "FechaEstimada");

            migrationBuilder.CreateIndex(
                name: "IX_PedidosSerigrafia_Hecho",
                table: "PedidosSerigrafia",
                column: "Hecho");

            migrationBuilder.CreateIndex(
                name: "IX_PedidosSerigrafia_PedidoDetalleId",
                table: "PedidosSerigrafia",
                column: "PedidoDetalleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CotizacionesSerigrafia");

            migrationBuilder.DropTable(
                name: "EscalasSerigrafia");

            migrationBuilder.DropTable(
                name: "TiposProceso");

            migrationBuilder.DropTable(
                name: "PedidosSerigrafia");
        }
    }
}
