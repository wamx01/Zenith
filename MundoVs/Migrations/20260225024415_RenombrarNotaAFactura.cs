using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class RenombrarNotaAFactura : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Nota",
                table: "PedidosSerigrafia",
                newName: "Factura");

            migrationBuilder.UpdateData(
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 25, 2, 44, 13, 821, DateTimeKind.Utc).AddTicks(4293));

            migrationBuilder.UpdateData(
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000002"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 25, 2, 44, 13, 821, DateTimeKind.Utc).AddTicks(5599));

            migrationBuilder.UpdateData(
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000010"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 25, 2, 44, 13, 821, DateTimeKind.Utc).AddTicks(5609));

            migrationBuilder.UpdateData(
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000011"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 25, 2, 44, 13, 821, DateTimeKind.Utc).AddTicks(5623));

            migrationBuilder.UpdateData(
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000020"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 25, 2, 44, 13, 821, DateTimeKind.Utc).AddTicks(5625));

            migrationBuilder.UpdateData(
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000021"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 25, 2, 44, 13, 821, DateTimeKind.Utc).AddTicks(5628));

            migrationBuilder.UpdateData(
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000022"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 25, 2, 44, 13, 821, DateTimeKind.Utc).AddTicks(5630));

            migrationBuilder.UpdateData(
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000023"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 25, 2, 44, 13, 821, DateTimeKind.Utc).AddTicks(5632));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Factura",
                table: "PedidosSerigrafia",
                newName: "Nota");

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
        }
    }
}
