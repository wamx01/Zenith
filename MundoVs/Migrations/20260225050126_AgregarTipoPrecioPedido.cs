using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class AgregarTipoPrecioPedido : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TipoPrecio",
                table: "Pedidos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 25, 5, 1, 25, 234, DateTimeKind.Utc).AddTicks(6461));

            migrationBuilder.UpdateData(
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000002"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 25, 5, 1, 25, 234, DateTimeKind.Utc).AddTicks(7826));

            migrationBuilder.UpdateData(
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000010"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 25, 5, 1, 25, 234, DateTimeKind.Utc).AddTicks(7836));

            migrationBuilder.UpdateData(
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000011"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 25, 5, 1, 25, 234, DateTimeKind.Utc).AddTicks(7838));

            migrationBuilder.UpdateData(
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000020"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 25, 5, 1, 25, 234, DateTimeKind.Utc).AddTicks(7841));

            migrationBuilder.UpdateData(
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000021"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 25, 5, 1, 25, 234, DateTimeKind.Utc).AddTicks(7843));

            migrationBuilder.UpdateData(
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000022"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 25, 5, 1, 25, 234, DateTimeKind.Utc).AddTicks(7845));

            migrationBuilder.UpdateData(
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000023"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 25, 5, 1, 25, 234, DateTimeKind.Utc).AddTicks(7848));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TipoPrecio",
                table: "Pedidos");

            migrationBuilder.UpdateData(
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 25, 4, 16, 9, 741, DateTimeKind.Utc).AddTicks(2537));

            migrationBuilder.UpdateData(
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000002"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 25, 4, 16, 9, 741, DateTimeKind.Utc).AddTicks(3490));

            migrationBuilder.UpdateData(
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000010"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 25, 4, 16, 9, 741, DateTimeKind.Utc).AddTicks(3498));

            migrationBuilder.UpdateData(
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000011"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 25, 4, 16, 9, 741, DateTimeKind.Utc).AddTicks(3501));

            migrationBuilder.UpdateData(
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000020"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 25, 4, 16, 9, 741, DateTimeKind.Utc).AddTicks(3503));

            migrationBuilder.UpdateData(
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000021"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 25, 4, 16, 9, 741, DateTimeKind.Utc).AddTicks(3506));

            migrationBuilder.UpdateData(
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000022"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 25, 4, 16, 9, 741, DateTimeKind.Utc).AddTicks(3507));

            migrationBuilder.UpdateData(
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000023"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 25, 4, 16, 9, 741, DateTimeKind.Utc).AddTicks(3517));
        }
    }
}
