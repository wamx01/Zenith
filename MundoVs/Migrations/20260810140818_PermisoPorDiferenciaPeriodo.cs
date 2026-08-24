using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class PermisoPorDiferenciaPeriodo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrigenAusencia",
                table: "rrhh_ausencia",
                type: "int",
                nullable: false,
                defaultValue: 1); // 1 = OrigenAusenciaRrhh.Manual

            migrationBuilder.AddColumn<string>(
                name: "PeriodoKey",
                table: "rrhh_ausencia",
                type: "varchar(60)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_rrhh_ausencia_PeriodoKey",
                table: "rrhh_ausencia",
                columns: new[] { "EmpresaId", "EmpleadoId", "PeriodoKey" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_rrhh_ausencia_PeriodoKey",
                table: "rrhh_ausencia");

            migrationBuilder.DropColumn(
                name: "OrigenAusencia",
                table: "rrhh_ausencia");

            migrationBuilder.DropColumn(
                name: "PeriodoKey",
                table: "rrhh_ausencia");
        }
    }
}
