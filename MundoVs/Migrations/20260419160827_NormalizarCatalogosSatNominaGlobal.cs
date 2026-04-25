using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class NormalizarCatalogosSatNominaGlobal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
UPDATE rrhh_nomina_percepcion p
INNER JOIN rrhh_nomina_percepcion_tipo t ON t.Id = p.TipoPercepcionId
INNER JOIN (
    SELECT Clave, MIN(Id) AS CanonicoId
    FROM rrhh_nomina_percepcion_tipo
    GROUP BY Clave
) c ON c.Clave = t.Clave
SET p.TipoPercepcionId = c.CanonicoId
WHERE p.TipoPercepcionId <> c.CanonicoId;
");

            migrationBuilder.Sql(@"
DELETE t1 FROM rrhh_nomina_percepcion_tipo t1
INNER JOIN rrhh_nomina_percepcion_tipo t2
    ON t1.Clave = t2.Clave
    AND t1.Id > t2.Id;
");

            migrationBuilder.Sql(@"
UPDATE rrhh_nomina_deduccion d
INNER JOIN rrhh_deduccion_tipo t ON t.Id = d.TipoDeduccionId
INNER JOIN (
    SELECT Clave, MIN(Id) AS CanonicoId
    FROM rrhh_deduccion_tipo
    GROUP BY Clave
) c ON c.Clave = t.Clave
SET d.TipoDeduccionId = c.CanonicoId
WHERE d.TipoDeduccionId <> c.CanonicoId;
");

            migrationBuilder.Sql(@"
DELETE t1 FROM rrhh_deduccion_tipo t1
INNER JOIN rrhh_deduccion_tipo t2
    ON t1.Clave = t2.Clave
    AND t1.Id > t2.Id;
");

            migrationBuilder.DropForeignKey(
                name: "FK_rrhh_deduccion_tipo_Empresas_EmpresaId",
                table: "rrhh_deduccion_tipo");

            migrationBuilder.DropForeignKey(
                name: "FK_rrhh_nomina_percepcion_tipo_Empresas_EmpresaId",
                table: "rrhh_nomina_percepcion_tipo");

            migrationBuilder.DropIndex(
                name: "IX_rrhh_nomina_percepcion_tipo_EmpresaId_Clave",
                table: "rrhh_nomina_percepcion_tipo");

            migrationBuilder.DropIndex(
                name: "IX_rrhh_nomina_percepcion_tipo_EmpresaId_Nombre",
                table: "rrhh_nomina_percepcion_tipo");

            migrationBuilder.DropIndex(
                name: "IX_rrhh_deduccion_tipo_EmpresaId_Clave",
                table: "rrhh_deduccion_tipo");

            migrationBuilder.DropIndex(
                name: "IX_rrhh_deduccion_tipo_EmpresaId_Nombre",
                table: "rrhh_deduccion_tipo");

            migrationBuilder.DropColumn(
                name: "EmpresaId",
                table: "rrhh_nomina_percepcion_tipo");

            migrationBuilder.DropColumn(
                name: "EmpresaId",
                table: "rrhh_deduccion_tipo");

            migrationBuilder.CreateIndex(
                name: "IX_rrhh_nomina_percepcion_tipo_Clave",
                table: "rrhh_nomina_percepcion_tipo",
                column: "Clave",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_rrhh_nomina_percepcion_tipo_Nombre",
                table: "rrhh_nomina_percepcion_tipo",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_rrhh_deduccion_tipo_Clave",
                table: "rrhh_deduccion_tipo",
                column: "Clave",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_rrhh_deduccion_tipo_Nombre",
                table: "rrhh_deduccion_tipo",
                column: "Nombre",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_rrhh_nomina_percepcion_tipo_Clave",
                table: "rrhh_nomina_percepcion_tipo");

            migrationBuilder.DropIndex(
                name: "IX_rrhh_nomina_percepcion_tipo_Nombre",
                table: "rrhh_nomina_percepcion_tipo");

            migrationBuilder.DropIndex(
                name: "IX_rrhh_deduccion_tipo_Clave",
                table: "rrhh_deduccion_tipo");

            migrationBuilder.DropIndex(
                name: "IX_rrhh_deduccion_tipo_Nombre",
                table: "rrhh_deduccion_tipo");

            migrationBuilder.AddColumn<Guid>(
                name: "EmpresaId",
                table: "rrhh_nomina_percepcion_tipo",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "EmpresaId",
                table: "rrhh_deduccion_tipo",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_rrhh_nomina_percepcion_tipo_EmpresaId_Clave",
                table: "rrhh_nomina_percepcion_tipo",
                columns: new[] { "EmpresaId", "Clave" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_rrhh_nomina_percepcion_tipo_EmpresaId_Nombre",
                table: "rrhh_nomina_percepcion_tipo",
                columns: new[] { "EmpresaId", "Nombre" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_rrhh_deduccion_tipo_EmpresaId_Clave",
                table: "rrhh_deduccion_tipo",
                columns: new[] { "EmpresaId", "Clave" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_rrhh_deduccion_tipo_EmpresaId_Nombre",
                table: "rrhh_deduccion_tipo",
                columns: new[] { "EmpresaId", "Nombre" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_rrhh_deduccion_tipo_Empresas_EmpresaId",
                table: "rrhh_deduccion_tipo",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_rrhh_nomina_percepcion_tipo_Empresas_EmpresaId",
                table: "rrhh_nomina_percepcion_tipo",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
