using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class RrhhAsistenciaDescansosTurno : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte>(
                name: "CantidadDescansos",
                table: "rrhh_turno_detalle",
                type: "tinyint unsigned",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<bool>(
                name: "Descanso1EsPagado",
                table: "rrhh_turno_detalle",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "Descanso1Fin",
                table: "rrhh_turno_detalle",
                type: "time(6)",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "Descanso1Inicio",
                table: "rrhh_turno_detalle",
                type: "time(6)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Descanso2EsPagado",
                table: "rrhh_turno_detalle",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "Descanso2Fin",
                table: "rrhh_turno_detalle",
                type: "time(6)",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "Descanso2Inicio",
                table: "rrhh_turno_detalle",
                type: "time(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MinutosDescansoNoPagado",
                table: "rrhh_asistencia",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MinutosDescansoPagado",
                table: "rrhh_asistencia",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MinutosDescansoProgramado",
                table: "rrhh_asistencia",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MinutosDescansoTomado",
                table: "rrhh_asistencia",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MinutosJornadaNetaProgramada",
                table: "rrhh_asistencia",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MinutosJornadaProgramada",
                table: "rrhh_asistencia",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MinutosTrabajadosBrutos",
                table: "rrhh_asistencia",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MinutosTrabajadosNetos",
                table: "rrhh_asistencia",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ResumenDescansos",
                table: "rrhh_asistencia",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "TotalMarcaciones",
                table: "rrhh_asistencia",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CantidadDescansos",
                table: "rrhh_turno_detalle");

            migrationBuilder.DropColumn(
                name: "Descanso1EsPagado",
                table: "rrhh_turno_detalle");

            migrationBuilder.DropColumn(
                name: "Descanso1Fin",
                table: "rrhh_turno_detalle");

            migrationBuilder.DropColumn(
                name: "Descanso1Inicio",
                table: "rrhh_turno_detalle");

            migrationBuilder.DropColumn(
                name: "Descanso2EsPagado",
                table: "rrhh_turno_detalle");

            migrationBuilder.DropColumn(
                name: "Descanso2Fin",
                table: "rrhh_turno_detalle");

            migrationBuilder.DropColumn(
                name: "Descanso2Inicio",
                table: "rrhh_turno_detalle");

            migrationBuilder.DropColumn(
                name: "MinutosDescansoNoPagado",
                table: "rrhh_asistencia");

            migrationBuilder.DropColumn(
                name: "MinutosDescansoPagado",
                table: "rrhh_asistencia");

            migrationBuilder.DropColumn(
                name: "MinutosDescansoProgramado",
                table: "rrhh_asistencia");

            migrationBuilder.DropColumn(
                name: "MinutosDescansoTomado",
                table: "rrhh_asistencia");

            migrationBuilder.DropColumn(
                name: "MinutosJornadaNetaProgramada",
                table: "rrhh_asistencia");

            migrationBuilder.DropColumn(
                name: "MinutosJornadaProgramada",
                table: "rrhh_asistencia");

            migrationBuilder.DropColumn(
                name: "MinutosTrabajadosBrutos",
                table: "rrhh_asistencia");

            migrationBuilder.DropColumn(
                name: "MinutosTrabajadosNetos",
                table: "rrhh_asistencia");

            migrationBuilder.DropColumn(
                name: "ResumenDescansos",
                table: "rrhh_asistencia");

            migrationBuilder.DropColumn(
                name: "TotalMarcaciones",
                table: "rrhh_asistencia");
        }
    }
}
