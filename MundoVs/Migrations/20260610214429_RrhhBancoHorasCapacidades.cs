using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class RrhhBancoHorasCapacidades : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
INSERT INTO capacidades (Id, Clave, Nombre, Modulo, ModuloAccesoId, Descripcion)
SELECT UUID(), 'rrhh.bancohoras.ver', 'Ver banco de horas', 'Banco de Horas', am.Id, 'Acceso al módulo de banco de horas'
FROM auth_modulo am
WHERE am.Clave = 'rrhh'
AND NOT EXISTS (SELECT 1 FROM capacidades WHERE Clave = 'rrhh.bancohoras.ver');

INSERT INTO capacidades (Id, Clave, Nombre, Modulo, ModuloAccesoId, Descripcion)
SELECT UUID(), 'rrhh.bancohoras.editar', 'Editar banco de horas', 'Banco de Horas', am.Id, 'Registrar ajustes y consumos de banco de horas'
FROM auth_modulo am
WHERE am.Clave = 'rrhh'
AND NOT EXISTS (SELECT 1 FROM capacidades WHERE Clave = 'rrhh.bancohoras.editar');

INSERT INTO tipousuariocapacidades (TipoUsuarioId, CapacidadId)
SELECT tu.Id, c.Id
FROM tiposusuario tu
JOIN capacidades c ON c.Clave IN ('rrhh.bancohoras.ver', 'rrhh.bancohoras.editar')
WHERE tu.Nombre = 'Administrador'
AND NOT EXISTS (
    SELECT 1 FROM tipousuariocapacidades tuc
    WHERE tuc.TipoUsuarioId = tu.Id AND tuc.CapacidadId = c.Id
);

INSERT INTO tipousuariocapacidades (TipoUsuarioId, CapacidadId)
SELECT tu.Id, c.Id
FROM tiposusuario tu
JOIN capacidades c ON c.Clave = 'rrhh.bancohoras.ver'
WHERE tu.Nombre = 'Gerente'
AND NOT EXISTS (
    SELECT 1 FROM tipousuariocapacidades tuc
    WHERE tuc.TipoUsuarioId = tu.Id AND tuc.CapacidadId = c.Id
);
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DELETE tuc FROM tipousuariocapacidades tuc
JOIN capacidades c ON c.Id = tuc.CapacidadId
WHERE c.Clave IN ('rrhh.bancohoras.ver', 'rrhh.bancohoras.editar');

DELETE FROM capacidades WHERE Clave IN ('rrhh.bancohoras.ver', 'rrhh.bancohoras.editar');
");
        }
    }
}
