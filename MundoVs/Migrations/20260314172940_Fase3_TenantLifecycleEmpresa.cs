using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class Fase3_TenantLifecycleEmpresa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ActivatedAt",
                table: "Empresas",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Estado",
                table: "Empresas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsSuspended",
                table: "Empresas",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MaxUsuarios",
                table: "Empresas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PlanActualId",
                table: "Empresas",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<DateTime>(
                name: "TrialEndsAt",
                table: "Empresas",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.Sql("UPDATE `Empresas` SET `Estado` = CASE WHEN `IsActive` = 1 THEN 1 ELSE 2 END, `IsSuspended` = CASE WHEN `IsActive` = 1 THEN 0 ELSE 1 END, `ActivatedAt` = CASE WHEN `IsActive` = 1 THEN COALESCE(`ActivatedAt`, `CreatedAt`, UTC_TIMESTAMP()) ELSE `ActivatedAt` END;");

            migrationBuilder.CreateIndex(
                name: "IX_Empresas_Estado",
                table: "Empresas",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_Empresas_IsSuspended",
                table: "Empresas",
                column: "IsSuspended");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Empresas_Estado",
                table: "Empresas");

            migrationBuilder.DropIndex(
                name: "IX_Empresas_IsSuspended",
                table: "Empresas");

            migrationBuilder.DropColumn(
                name: "ActivatedAt",
                table: "Empresas");

            migrationBuilder.DropColumn(
                name: "Estado",
                table: "Empresas");

            migrationBuilder.DropColumn(
                name: "IsSuspended",
                table: "Empresas");

            migrationBuilder.DropColumn(
                name: "MaxUsuarios",
                table: "Empresas");

            migrationBuilder.DropColumn(
                name: "PlanActualId",
                table: "Empresas");

            migrationBuilder.DropColumn(
                name: "TrialEndsAt",
                table: "Empresas");
        }
    }
}
