using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class PedidoSerigrafiaTallaProcesos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PedidoSerigrafiaTallaProcesos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PedidoSerigrafiaId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PedidoSerigrafiaTallaId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TipoProcesoId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Completado = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    FechaPaso = table.Column<DateTime>(type: "datetime(6)", nullable: true),
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
                    table.PrimaryKey("PK_PedidoSerigrafiaTallaProcesos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PedidoSerigrafiaTallaProcesos_PedidoSerigrafiaTallas_PedidoS~",
                        column: x => x.PedidoSerigrafiaTallaId,
                        principalTable: "PedidoSerigrafiaTallas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PedidoSerigrafiaTallaProcesos_PedidosSerigrafia_PedidoSerigr~",
                        column: x => x.PedidoSerigrafiaId,
                        principalTable: "PedidosSerigrafia",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PedidoSerigrafiaTallaProcesos_TiposProceso_TipoProcesoId",
                        column: x => x.TipoProcesoId,
                        principalTable: "TiposProceso",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 22, 18, 1, 52, 443, DateTimeKind.Utc).AddTicks(6110));

            migrationBuilder.UpdateData(
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000002"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 22, 18, 1, 52, 443, DateTimeKind.Utc).AddTicks(7346));

            migrationBuilder.UpdateData(
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000010"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 22, 18, 1, 52, 443, DateTimeKind.Utc).AddTicks(7355));

            migrationBuilder.UpdateData(
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000011"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 22, 18, 1, 52, 443, DateTimeKind.Utc).AddTicks(7358));

            migrationBuilder.UpdateData(
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000020"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 22, 18, 1, 52, 443, DateTimeKind.Utc).AddTicks(7362));

            migrationBuilder.UpdateData(
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000021"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 22, 18, 1, 52, 443, DateTimeKind.Utc).AddTicks(7364));

            migrationBuilder.UpdateData(
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000022"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 22, 18, 1, 52, 443, DateTimeKind.Utc).AddTicks(7366));

            migrationBuilder.UpdateData(
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000023"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 22, 18, 1, 52, 443, DateTimeKind.Utc).AddTicks(7378));

            migrationBuilder.CreateIndex(
                name: "IX_PedidoSerigrafiaTallaProcesos_PedidoSerigrafiaId_PedidoSerig~",
                table: "PedidoSerigrafiaTallaProcesos",
                columns: new[] { "PedidoSerigrafiaId", "PedidoSerigrafiaTallaId", "TipoProcesoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PedidoSerigrafiaTallaProcesos_PedidoSerigrafiaTallaId",
                table: "PedidoSerigrafiaTallaProcesos",
                column: "PedidoSerigrafiaTallaId");

            migrationBuilder.CreateIndex(
                name: "IX_PedidoSerigrafiaTallaProcesos_TipoProcesoId",
                table: "PedidoSerigrafiaTallaProcesos",
                column: "TipoProcesoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PedidoSerigrafiaTallaProcesos");

            migrationBuilder.UpdateData(
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 22, 5, 40, 17, 956, DateTimeKind.Utc).AddTicks(7441));

            migrationBuilder.UpdateData(
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000002"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 22, 5, 40, 17, 957, DateTimeKind.Utc).AddTicks(4619));

            migrationBuilder.UpdateData(
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000010"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 22, 5, 40, 17, 957, DateTimeKind.Utc).AddTicks(4645));

            migrationBuilder.UpdateData(
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000011"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 22, 5, 40, 17, 957, DateTimeKind.Utc).AddTicks(4649));

            migrationBuilder.UpdateData(
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000020"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 22, 5, 40, 17, 957, DateTimeKind.Utc).AddTicks(4651));

            migrationBuilder.UpdateData(
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000021"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 22, 5, 40, 17, 957, DateTimeKind.Utc).AddTicks(4663));

            migrationBuilder.UpdateData(
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000022"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 22, 5, 40, 17, 957, DateTimeKind.Utc).AddTicks(4665));

            migrationBuilder.UpdateData(
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000023"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 22, 5, 40, 17, 957, DateTimeKind.Utc).AddTicks(4668));
        }
    }
}
