using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class PrenominaSnapshotConfiguracionYCierreInmutable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CerradaPor",
                table: "Prenominas",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaCierre",
                table: "Prenominas",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SnapshotConfiguracionJson",
                table: "Prenominas",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CerradaPor",
                table: "Prenominas");

            migrationBuilder.DropColumn(
                name: "FechaCierre",
                table: "Prenominas");

            migrationBuilder.DropColumn(
                name: "SnapshotConfiguracionJson",
                table: "Prenominas");
        }
    }
}
