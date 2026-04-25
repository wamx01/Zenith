using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    public partial class RrhhMarcacionHoraLocalPersistida : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE rrhh_marcacion ADD COLUMN IF NOT EXISTS FechaHoraMarcacionLocal datetime(6) NULL;");
            migrationBuilder.Sql("ALTER TABLE rrhh_marcacion ADD COLUMN IF NOT EXISTS ZonaHorariaAplicada varchar(100) NULL;");

            migrationBuilder.Sql("""
                UPDATE rrhh_marcacion m
                LEFT JOIN rrhh_checador c ON c.Id = m.ChecadorId
                SET m.ZonaHorariaAplicada = COALESCE(NULLIF(TRIM(m.ZonaHorariaAplicada), ''), NULLIF(TRIM(c.ZonaHoraria), ''), 'Central Standard Time (Mexico)')
                WHERE m.ZonaHorariaAplicada IS NULL OR TRIM(m.ZonaHorariaAplicada) = '';
                """);

            migrationBuilder.Sql("""
                UPDATE rrhh_marcacion m
                SET m.FechaHoraMarcacionLocal = CASE
                    WHEN m.ZonaHorariaAplicada IS NULL OR TRIM(m.ZonaHorariaAplicada) = '' THEN DATE_ADD(m.FechaHoraMarcacionUtc, INTERVAL -360 MINUTE)
                    WHEN UPPER(TRIM(m.ZonaHorariaAplicada)) IN ('UTC', 'ETC/UTC') THEN m.FechaHoraMarcacionUtc
                    WHEN TRIM(m.ZonaHorariaAplicada) IN ('Central Standard Time (Mexico)', 'America/Mexico_City') THEN DATE_ADD(m.FechaHoraMarcacionUtc, INTERVAL -360 MINUTE)
                    WHEN UPPER(TRIM(m.ZonaHorariaAplicada)) REGEXP '^UTC[+-][0-9]{2}(:[0-9]{2})?$' THEN DATE_ADD(
                        m.FechaHoraMarcacionUtc,
                        INTERVAL (
                            (CASE WHEN SUBSTRING(UPPER(TRIM(m.ZonaHorariaAplicada)), 4, 1) = '-' THEN -1 ELSE 1 END)
                            *
                            (
                                (CAST(SUBSTRING(UPPER(TRIM(m.ZonaHorariaAplicada)), 5, 2) AS SIGNED) * 60)
                                +
                                (CASE
                                    WHEN LENGTH(UPPER(TRIM(m.ZonaHorariaAplicada))) >= 9 THEN CAST(SUBSTRING(UPPER(TRIM(m.ZonaHorariaAplicada)), 8, 2) AS SIGNED)
                                    ELSE 0
                                END)
                            )
                        ) MINUTE)
                    ELSE m.FechaHoraMarcacionUtc
                END
                WHERE m.FechaHoraMarcacionLocal IS NULL;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE rrhh_marcacion DROP COLUMN IF EXISTS FechaHoraMarcacionLocal;");
            migrationBuilder.Sql("ALTER TABLE rrhh_marcacion DROP COLUMN IF EXISTS ZonaHorariaAplicada;");
        }
    }
}
