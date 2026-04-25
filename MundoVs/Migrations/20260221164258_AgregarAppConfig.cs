using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class AgregarAppConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppConfigs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Clave = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Valor = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Descripcion = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppConfigs", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "AppConfigs",
                columns: new[] { "Id", "Clave", "CreatedAt", "Descripcion", "UpdatedAt", "Valor" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000001"), "CompanyName", new DateTime(2026, 2, 21, 16, 42, 57, 286, DateTimeKind.Utc).AddTicks(1083), "Nombre de la empresa", null, "MundoVs" },
                    { new Guid("00000000-0000-0000-0000-000000000002"), "CompanySlogan", new DateTime(2026, 2, 21, 16, 42, 57, 286, DateTimeKind.Utc).AddTicks(2391), "Slogan o subtítulo", null, "CRM & Producción" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppConfigs_Clave",
                table: "AppConfigs",
                column: "Clave",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppConfigs");
        }
    }
}
