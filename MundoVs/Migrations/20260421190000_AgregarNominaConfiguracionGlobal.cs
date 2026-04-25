using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    public partial class AgregarNominaConfiguracionGlobal : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "rrhh_nomina_configuracion_global",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UmaDiaria = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    SalarioMinimoGeneral = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    SalarioMinimoFrontera = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    TablaIsrJson = table.Column<string>(type: "longtext", nullable: false),
                    TablaSubsidioJson = table.Column<string>(type: "longtext", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: true),
                    UpdatedBy = table.Column<string>(type: "longtext", nullable: true),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rrhh_nomina_configuracion_global", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "rrhh_nomina_configuracion_global");
        }
    }
}
