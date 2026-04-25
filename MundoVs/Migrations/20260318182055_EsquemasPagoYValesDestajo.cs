using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class EsquemasPagoYValesDestajo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "EsquemaPagoId",
                table: "NominaDetalles",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<decimal>(
                name: "MontoBono",
                table: "NominaDetalles",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MontoDestajo",
                table: "NominaDetalles",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "TotalPiezas",
                table: "NominaDetalles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "EsquemasPago",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EmpresaId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Nombre = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Descripcion = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    IncluyeSueldoBase = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    SueldoBaseSugerido = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    UsaMetaPorPedidos = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    BonoCumplimientoMonto = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    BonoAdelantoMonto = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    BonoRepartirPorSueldo = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    BonoRepartirPorAsistencia = table.Column<bool>(type: "tinyint(1)", nullable: false),
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
                    table.PrimaryKey("PK_EsquemasPago", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EsquemasPago_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "EmpleadosEsquemaPago",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EmpleadoId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EsquemaPagoId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    SueldoBaseOverride = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    VigenteDesde = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    VigenteHasta = table.Column<DateTime>(type: "datetime(6)", nullable: true),
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
                    table.PrimaryKey("PK_EmpleadosEsquemaPago", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmpleadosEsquemaPago_Empleados_EmpleadoId",
                        column: x => x.EmpleadoId,
                        principalTable: "Empleados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmpleadosEsquemaPago_EsquemasPago_EsquemaPagoId",
                        column: x => x.EsquemaPagoId,
                        principalTable: "EsquemasPago",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "EsquemasPagoTarifa",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EsquemaPagoId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TipoProcesoId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    PosicionId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    Tarifa = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Descripcion = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
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
                    table.PrimaryKey("PK_EsquemasPagoTarifa", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EsquemasPagoTarifa_EsquemasPago_EsquemaPagoId",
                        column: x => x.EsquemaPagoId,
                        principalTable: "EsquemasPago",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EsquemasPagoTarifa_Posiciones_PosicionId",
                        column: x => x.PosicionId,
                        principalTable: "Posiciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EsquemasPagoTarifa_TiposProceso_TipoProcesoId",
                        column: x => x.TipoProcesoId,
                        principalTable: "TiposProceso",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ValesDestajo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EmpresaId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Folio = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EmpleadoId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Fecha = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Estatus = table.Column<int>(type: "int", nullable: false),
                    EsquemaPagoId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    NominaDetalleId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
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
                    table.PrimaryKey("PK_ValesDestajo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ValesDestajo_Empleados_EmpleadoId",
                        column: x => x.EmpleadoId,
                        principalTable: "Empleados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ValesDestajo_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ValesDestajo_EsquemasPago_EsquemaPagoId",
                        column: x => x.EsquemaPagoId,
                        principalTable: "EsquemasPago",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ValesDestajo_NominaDetalles_NominaDetalleId",
                        column: x => x.NominaDetalleId,
                        principalTable: "NominaDetalles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ValesDestajoDetalle",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ValeDestajoId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TipoProcesoId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PedidoId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    TarifaAplicada = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Importe = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    EsquemaPagoTarifaId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
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
                    table.PrimaryKey("PK_ValesDestajoDetalle", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ValesDestajoDetalle_EsquemasPagoTarifa_EsquemaPagoTarifaId",
                        column: x => x.EsquemaPagoTarifaId,
                        principalTable: "EsquemasPagoTarifa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ValesDestajoDetalle_Pedidos_PedidoId",
                        column: x => x.PedidoId,
                        principalTable: "Pedidos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ValesDestajoDetalle_TiposProceso_TipoProcesoId",
                        column: x => x.TipoProcesoId,
                        principalTable: "TiposProceso",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ValesDestajoDetalle_ValesDestajo_ValeDestajoId",
                        column: x => x.ValeDestajoId,
                        principalTable: "ValesDestajo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_NominaDetalles_EsquemaPagoId",
                table: "NominaDetalles",
                column: "EsquemaPagoId");

            migrationBuilder.CreateIndex(
                name: "IX_EmpleadosEsquemaPago_EmpleadoId",
                table: "EmpleadosEsquemaPago",
                column: "EmpleadoId");

            migrationBuilder.CreateIndex(
                name: "IX_EmpleadosEsquemaPago_EmpleadoId_VigenteDesde",
                table: "EmpleadosEsquemaPago",
                columns: new[] { "EmpleadoId", "VigenteDesde" });

            migrationBuilder.CreateIndex(
                name: "IX_EmpleadosEsquemaPago_EsquemaPagoId",
                table: "EmpleadosEsquemaPago",
                column: "EsquemaPagoId");

            migrationBuilder.CreateIndex(
                name: "IX_EsquemasPago_EmpresaId_Nombre",
                table: "EsquemasPago",
                columns: new[] { "EmpresaId", "Nombre" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EsquemasPago_Tipo",
                table: "EsquemasPago",
                column: "Tipo");

            migrationBuilder.CreateIndex(
                name: "IX_EsquemasPagoTarifa_EsquemaPagoId",
                table: "EsquemasPagoTarifa",
                column: "EsquemaPagoId");

            migrationBuilder.CreateIndex(
                name: "IX_EsquemasPagoTarifa_EsquemaPagoId_TipoProcesoId_PosicionId",
                table: "EsquemasPagoTarifa",
                columns: new[] { "EsquemaPagoId", "TipoProcesoId", "PosicionId" });

            migrationBuilder.CreateIndex(
                name: "IX_EsquemasPagoTarifa_PosicionId",
                table: "EsquemasPagoTarifa",
                column: "PosicionId");

            migrationBuilder.CreateIndex(
                name: "IX_EsquemasPagoTarifa_TipoProcesoId",
                table: "EsquemasPagoTarifa",
                column: "TipoProcesoId");

            migrationBuilder.CreateIndex(
                name: "IX_ValesDestajo_EmpleadoId",
                table: "ValesDestajo",
                column: "EmpleadoId");

            migrationBuilder.CreateIndex(
                name: "IX_ValesDestajo_EmpresaId_Folio",
                table: "ValesDestajo",
                columns: new[] { "EmpresaId", "Folio" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ValesDestajo_EsquemaPagoId",
                table: "ValesDestajo",
                column: "EsquemaPagoId");

            migrationBuilder.CreateIndex(
                name: "IX_ValesDestajo_Estatus",
                table: "ValesDestajo",
                column: "Estatus");

            migrationBuilder.CreateIndex(
                name: "IX_ValesDestajo_Fecha",
                table: "ValesDestajo",
                column: "Fecha");

            migrationBuilder.CreateIndex(
                name: "IX_ValesDestajo_NominaDetalleId",
                table: "ValesDestajo",
                column: "NominaDetalleId");

            migrationBuilder.CreateIndex(
                name: "IX_ValesDestajoDetalle_EsquemaPagoTarifaId",
                table: "ValesDestajoDetalle",
                column: "EsquemaPagoTarifaId");

            migrationBuilder.CreateIndex(
                name: "IX_ValesDestajoDetalle_PedidoId",
                table: "ValesDestajoDetalle",
                column: "PedidoId");

            migrationBuilder.CreateIndex(
                name: "IX_ValesDestajoDetalle_TipoProcesoId",
                table: "ValesDestajoDetalle",
                column: "TipoProcesoId");

            migrationBuilder.CreateIndex(
                name: "IX_ValesDestajoDetalle_ValeDestajoId",
                table: "ValesDestajoDetalle",
                column: "ValeDestajoId");

            migrationBuilder.AddForeignKey(
                name: "FK_NominaDetalles_EsquemasPago_EsquemaPagoId",
                table: "NominaDetalles",
                column: "EsquemaPagoId",
                principalTable: "EsquemasPago",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NominaDetalles_EsquemasPago_EsquemaPagoId",
                table: "NominaDetalles");

            migrationBuilder.DropTable(
                name: "EmpleadosEsquemaPago");

            migrationBuilder.DropTable(
                name: "ValesDestajoDetalle");

            migrationBuilder.DropTable(
                name: "EsquemasPagoTarifa");

            migrationBuilder.DropTable(
                name: "ValesDestajo");

            migrationBuilder.DropTable(
                name: "EsquemasPago");

            migrationBuilder.DropIndex(
                name: "IX_NominaDetalles_EsquemaPagoId",
                table: "NominaDetalles");

            migrationBuilder.DropColumn(
                name: "EsquemaPagoId",
                table: "NominaDetalles");

            migrationBuilder.DropColumn(
                name: "MontoBono",
                table: "NominaDetalles");

            migrationBuilder.DropColumn(
                name: "MontoDestajo",
                table: "NominaDetalles");

            migrationBuilder.DropColumn(
                name: "TotalPiezas",
                table: "NominaDetalles");
        }
    }
}
