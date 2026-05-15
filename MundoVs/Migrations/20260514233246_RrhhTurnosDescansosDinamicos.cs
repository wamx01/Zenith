using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class RrhhTurnosDescansosDinamicos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "rrhh_turno_detalle_descanso",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TurnoBaseDetalleId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Orden = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    HoraInicio = table.Column<TimeSpan>(type: "time(6)", nullable: true),
                    HoraFin = table.Column<TimeSpan>(type: "time(6)", nullable: true),
                    EsPagado = table.Column<bool>(type: "tinyint(1)", nullable: false),
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
                    table.PrimaryKey("PK_rrhh_turno_detalle_descanso", x => x.Id);
                    table.ForeignKey(
                        name: "FK_rrhh_turno_detalle_descanso_rrhh_turno_detalle_TurnoBaseDeta~",
                        column: x => x.TurnoBaseDetalleId,
                        principalTable: "rrhh_turno_detalle",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_rrhh_turno_detalle_descanso_TurnoBaseDetalleId_Orden",
                table: "rrhh_turno_detalle_descanso",
                columns: new[] { "TurnoBaseDetalleId", "Orden" },
                unique: true);

            migrationBuilder.Sql(@"
INSERT INTO rrhh_turno_detalle_descanso (Id, TurnoBaseDetalleId, Orden, HoraInicio, HoraFin, EsPagado, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, IsActive)
SELECT UUID(), Id, 1, Descanso1Inicio, Descanso1Fin, Descanso1EsPagado, UTC_TIMESTAMP(), NULL, NULL, NULL, 1
FROM rrhh_turno_detalle
WHERE Descanso1Inicio IS NOT NULL AND Descanso1Fin IS NOT NULL;

INSERT INTO rrhh_turno_detalle_descanso (Id, TurnoBaseDetalleId, Orden, HoraInicio, HoraFin, EsPagado, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, IsActive)
SELECT UUID(), Id, 2, Descanso2Inicio, Descanso2Fin, Descanso2EsPagado, UTC_TIMESTAMP(), NULL, NULL, NULL, 1
FROM rrhh_turno_detalle
WHERE Descanso2Inicio IS NOT NULL AND Descanso2Fin IS NOT NULL;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "rrhh_turno_detalle_descanso");
        }
    }
}
