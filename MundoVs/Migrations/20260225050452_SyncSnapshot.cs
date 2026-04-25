using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class SyncSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 25, 5, 4, 51, 930, DateTimeKind.Utc).AddTicks(7832));

            migrationBuilder.UpdateData(
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000002"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 25, 5, 4, 51, 930, DateTimeKind.Utc).AddTicks(9786));

            migrationBuilder.UpdateData(
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000010"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 25, 5, 4, 51, 930, DateTimeKind.Utc).AddTicks(9799));

            migrationBuilder.UpdateData(
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000011"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 25, 5, 4, 51, 930, DateTimeKind.Utc).AddTicks(9802));

            migrationBuilder.UpdateData(
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000020"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 25, 5, 4, 51, 930, DateTimeKind.Utc).AddTicks(9805));

            migrationBuilder.UpdateData(
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000021"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 25, 5, 4, 51, 930, DateTimeKind.Utc).AddTicks(9808));

            migrationBuilder.UpdateData(
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000022"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 25, 5, 4, 51, 930, DateTimeKind.Utc).AddTicks(9811));

            migrationBuilder.UpdateData(
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000023"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 25, 5, 4, 51, 930, DateTimeKind.Utc).AddTicks(9813));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
    }
}
