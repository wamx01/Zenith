using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class RrhhBonoEstructuraPorPosicion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Posiciones_EmpresaId",
                table: "Posiciones");

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Posiciones",
                type: "varchar(120)",
                maxLength: 120,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(200)",
                oldMaxLength: 200)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<Guid>(
                name: "BonoEstructuraRrhhId",
                table: "Posiciones",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateTable(
                name: "rrhh_bono_estructura",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EmpresaId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Nombre = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Descripcion = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
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
                    table.PrimaryKey("PK_rrhh_bono_estructura", x => x.Id);
                    table.ForeignKey(
                        name: "FK_rrhh_bono_estructura_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "rrhh_bono_estructura_detalle",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    BonoEstructuraRrhhId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    BonoRubroRrhhId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Orden = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_rrhh_bono_estructura_detalle", x => x.Id);
                    table.ForeignKey(
                        name: "FK_rrhh_bono_estructura_detalle_rrhh_bono_estructura_BonoEstruc~",
                        column: x => x.BonoEstructuraRrhhId,
                        principalTable: "rrhh_bono_estructura",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_rrhh_bono_estructura_detalle_rrhh_bono_rubro_BonoRubroRrhhId",
                        column: x => x.BonoRubroRrhhId,
                        principalTable: "rrhh_bono_rubro",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Posiciones_BonoEstructuraRrhhId",
                table: "Posiciones",
                column: "BonoEstructuraRrhhId");

            migrationBuilder.CreateIndex(
                name: "IX_Posiciones_EmpresaId_Nombre",
                table: "Posiciones",
                columns: new[] { "EmpresaId", "Nombre" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_rrhh_bono_estructura_EmpresaId_Nombre",
                table: "rrhh_bono_estructura",
                columns: new[] { "EmpresaId", "Nombre" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_rrhh_bono_estructura_detalle_BonoEstructuraRrhhId",
                table: "rrhh_bono_estructura_detalle",
                column: "BonoEstructuraRrhhId");

            migrationBuilder.CreateIndex(
                name: "IX_rrhh_bono_estructura_detalle_BonoEstructuraRrhhId_BonoRubroR~",
                table: "rrhh_bono_estructura_detalle",
                columns: new[] { "BonoEstructuraRrhhId", "BonoRubroRrhhId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_rrhh_bono_estructura_detalle_BonoRubroRrhhId",
                table: "rrhh_bono_estructura_detalle",
                column: "BonoRubroRrhhId");

            migrationBuilder.AddForeignKey(
                name: "FK_Posiciones_rrhh_bono_estructura_BonoEstructuraRrhhId",
                table: "Posiciones",
                column: "BonoEstructuraRrhhId",
                principalTable: "rrhh_bono_estructura",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Posiciones_rrhh_bono_estructura_BonoEstructuraRrhhId",
                table: "Posiciones");

            migrationBuilder.DropTable(
                name: "rrhh_bono_estructura_detalle");

            migrationBuilder.DropTable(
                name: "rrhh_bono_estructura");

            migrationBuilder.DropIndex(
                name: "IX_Posiciones_BonoEstructuraRrhhId",
                table: "Posiciones");

            migrationBuilder.DropIndex(
                name: "IX_Posiciones_EmpresaId_Nombre",
                table: "Posiciones");

            migrationBuilder.DropColumn(
                name: "BonoEstructuraRrhhId",
                table: "Posiciones");

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Posiciones",
                type: "varchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(120)",
                oldMaxLength: 120)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Posiciones_EmpresaId",
                table: "Posiciones",
                column: "EmpresaId");
        }
    }
}
