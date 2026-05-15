START TRANSACTION;
CREATE TABLE `rrhh_turno_detalle_descanso` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `TurnoBaseDetalleId` char(36) COLLATE ascii_general_ci NOT NULL,
    `Orden` tinyint unsigned NOT NULL,
    `HoraInicio` time(6) NULL,
    `HoraFin` time(6) NULL,
    `EsPagado` tinyint(1) NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NULL,
    `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
    `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
    `IsActive` tinyint(1) NOT NULL,
    CONSTRAINT `PK_rrhh_turno_detalle_descanso` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_rrhh_turno_detalle_descanso_rrhh_turno_detalle_TurnoBaseDeta~` FOREIGN KEY (`TurnoBaseDetalleId`) REFERENCES `rrhh_turno_detalle` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE UNIQUE INDEX `IX_rrhh_turno_detalle_descanso_TurnoBaseDetalleId_Orden` ON `rrhh_turno_detalle_descanso` (`TurnoBaseDetalleId`, `Orden`);


INSERT INTO rrhh_turno_detalle_descanso (Id, TurnoBaseDetalleId, Orden, HoraInicio, HoraFin, EsPagado, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, IsActive)
SELECT UUID(), Id, 1, Descanso1Inicio, Descanso1Fin, Descanso1EsPagado, UTC_TIMESTAMP(), NULL, NULL, NULL, 1
FROM rrhh_turno_detalle
WHERE Descanso1Inicio IS NOT NULL AND Descanso1Fin IS NOT NULL;

INSERT INTO rrhh_turno_detalle_descanso (Id, TurnoBaseDetalleId, Orden, HoraInicio, HoraFin, EsPagado, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, IsActive)
SELECT UUID(), Id, 2, Descanso2Inicio, Descanso2Fin, Descanso2EsPagado, UTC_TIMESTAMP(), NULL, NULL, NULL, 1
FROM rrhh_turno_detalle
WHERE Descanso2Inicio IS NOT NULL AND Descanso2Fin IS NOT NULL;


INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260514233246_RrhhTurnosDescansosDinamicos', '9.0.0');

COMMIT;

