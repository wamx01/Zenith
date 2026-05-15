CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
) CHARACTER SET=utf8mb4;

START TRANSACTION;
DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221063813_InitialCreate') THEN

    ALTER DATABASE CHARACTER SET utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221063813_InitialCreate') THEN

    CREATE TABLE `Clientes` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `Codigo` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `RazonSocial` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `NombreComercial` varchar(200) CHARACTER SET utf8mb4 NULL,
        `RfcCif` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `Email` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `Telefono` varchar(20) CHARACTER SET utf8mb4 NULL,
        `Direccion` varchar(250) CHARACTER SET utf8mb4 NULL,
        `Ciudad` varchar(100) CHARACTER SET utf8mb4 NULL,
        `Estado` varchar(100) CHARACTER SET utf8mb4 NULL,
        `CodigoPostal` varchar(10) CHARACTER SET utf8mb4 NULL,
        `Pais` varchar(50) CHARACTER SET utf8mb4 NULL,
        `Industria` int NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_Clientes` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221063813_InitialCreate') THEN

    CREATE TABLE `Hormas` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `Codigo` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `Nombre` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `Descripcion` varchar(500) CHARACTER SET utf8mb4 NULL,
        `Talla` varchar(10) CHARACTER SET utf8mb4 NOT NULL,
        `Medidas` varchar(100) CHARACTER SET utf8mb4 NULL,
        `StockDisponible` int NOT NULL,
        `Estado` int NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_Hormas` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221063813_InitialCreate') THEN

    CREATE TABLE `Pantallas` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `Codigo` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `Descripcion` varchar(500) CHARACTER SET utf8mb4 NULL,
        `MallaNumero` int NOT NULL,
        `Dimensiones` varchar(50) CHARACTER SET utf8mb4 NULL,
        `Estado` int NOT NULL,
        `UsosTotales` int NOT NULL,
        `FechaCreacion` datetime(6) NULL,
        `DisenoPara` varchar(200) CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_Pantallas` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221063813_InitialCreate') THEN

    CREATE TABLE `Productos` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `Codigo` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `Nombre` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `Descripcion` varchar(500) CHARACTER SET utf8mb4 NULL,
        `Industria` int NOT NULL,
        `PrecioBase` decimal(18,2) NOT NULL,
        `UnidadMedida` varchar(20) CHARACTER SET utf8mb4 NULL,
        `StockMinimo` int NOT NULL,
        `StockActual` int NOT NULL,
        `Categoria` varchar(100) CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_Productos` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221063813_InitialCreate') THEN

    CREATE TABLE `Tintas` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `Codigo` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `Nombre` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `CodigoPantone` varchar(20) CHARACTER SET utf8mb4 NULL,
        `CodigoHex` varchar(7) CHARACTER SET utf8mb4 NULL,
        `Tipo` int NOT NULL,
        `Cantidad` decimal(18,2) NOT NULL,
        `UnidadMedida` varchar(10) CHARACTER SET utf8mb4 NOT NULL,
        `StockMinimo` decimal(18,2) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_Tintas` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221063813_InitialCreate') THEN

    CREATE TABLE `Contactos` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `ClienteId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Nombre` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `Cargo` varchar(100) CHARACTER SET utf8mb4 NULL,
        `Email` varchar(100) CHARACTER SET utf8mb4 NULL,
        `Telefono` varchar(20) CHARACTER SET utf8mb4 NULL,
        `Movil` varchar(20) CHARACTER SET utf8mb4 NULL,
        `EsPrincipal` tinyint(1) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_Contactos` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_Contactos_Clientes_ClienteId` FOREIGN KEY (`ClienteId`) REFERENCES `Clientes` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221063813_InitialCreate') THEN

    CREATE TABLE `Disenos` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `Codigo` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `Nombre` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `Descripcion` varchar(500) CHARACTER SET utf8mb4 NULL,
        `ClienteId` char(36) COLLATE ascii_general_ci NULL,
        `RutaArchivo` varchar(500) CHARACTER SET utf8mb4 NULL,
        `NumeroColores` int NOT NULL,
        `Dimensiones` varchar(50) CHARACTER SET utf8mb4 NULL,
        `Aprobado` tinyint(1) NOT NULL,
        `FechaAprobacion` datetime(6) NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_Disenos` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_Disenos_Clientes_ClienteId` FOREIGN KEY (`ClienteId`) REFERENCES `Clientes` (`Id`) ON DELETE SET NULL
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221063813_InitialCreate') THEN

    CREATE TABLE `Pedidos` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `NumeroPedido` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `ClienteId` char(36) COLLATE ascii_general_ci NOT NULL,
        `FechaPedido` datetime(6) NOT NULL,
        `FechaEntregaEstimada` datetime(6) NULL,
        `Estado` int NOT NULL,
        `Subtotal` decimal(18,2) NOT NULL,
        `Impuestos` decimal(18,2) NOT NULL,
        `Total` decimal(18,2) NOT NULL,
        `Observaciones` varchar(500) CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_Pedidos` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_Pedidos_Clientes_ClienteId` FOREIGN KEY (`ClienteId`) REFERENCES `Clientes` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221063813_InitialCreate') THEN

    CREATE TABLE `ProductosCalzado` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `ProductoId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Modelo` varchar(100) CHARACTER SET utf8mb4 NULL,
        `Material` varchar(100) CHARACTER SET utf8mb4 NULL,
        `TipoSuela` varchar(100) CHARACTER SET utf8mb4 NULL,
        `Color` varchar(50) CHARACTER SET utf8mb4 NULL,
        `Temporada` varchar(50) CHARACTER SET utf8mb4 NULL,
        `Genero` int NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_ProductosCalzado` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_ProductosCalzado_Productos_ProductoId` FOREIGN KEY (`ProductoId`) REFERENCES `Productos` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221063813_InitialCreate') THEN

    CREATE TABLE `ProductosSerigrafia` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `ProductoId` char(36) COLLATE ascii_general_ci NOT NULL,
        `TipoImpresion` int NOT NULL,
        `MaterialBase` varchar(100) CHARACTER SET utf8mb4 NULL,
        `NumeroColores` int NOT NULL,
        `TamanoImpresion` varchar(50) CHARACTER SET utf8mb4 NULL,
        `TipoTinta` varchar(50) CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_ProductosSerigrafia` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_ProductosSerigrafia_Productos_ProductoId` FOREIGN KEY (`ProductoId`) REFERENCES `Productos` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221063813_InitialCreate') THEN

    CREATE TABLE `PedidoDetalles` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `PedidoId` char(36) COLLATE ascii_general_ci NOT NULL,
        `ProductoId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Cantidad` int NOT NULL,
        `PrecioUnitario` decimal(18,2) NOT NULL,
        `Descuento` decimal(18,2) NOT NULL,
        `Total` decimal(18,2) NOT NULL,
        `Especificaciones` varchar(500) CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_PedidoDetalles` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_PedidoDetalles_Pedidos_PedidoId` FOREIGN KEY (`PedidoId`) REFERENCES `Pedidos` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_PedidoDetalles_Productos_ProductoId` FOREIGN KEY (`ProductoId`) REFERENCES `Productos` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221063813_InitialCreate') THEN

    CREATE TABLE `TallasCalzado` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `ProductoCalzadoId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Talla` varchar(10) CHARACTER SET utf8mb4 NOT NULL,
        `TallaUS` varchar(10) CHARACTER SET utf8mb4 NULL,
        `TallaEU` varchar(10) CHARACTER SET utf8mb4 NULL,
        `TallaUK` varchar(10) CHARACTER SET utf8mb4 NULL,
        `StockDisponible` int NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_TallasCalzado` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_TallasCalzado_ProductosCalzado_ProductoCalzadoId` FOREIGN KEY (`ProductoCalzadoId`) REFERENCES `ProductosCalzado` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221063813_InitialCreate') THEN

    CREATE TABLE `ColoresSerigrafia` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `ProductoSerigrafiaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `NombreColor` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `CodigoPantone` varchar(20) CHARACTER SET utf8mb4 NULL,
        `CodigoHex` varchar(7) CHARACTER SET utf8mb4 NULL,
        `Orden` int NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_ColoresSerigrafia` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_ColoresSerigrafia_ProductosSerigrafia_ProductoSerigrafiaId` FOREIGN KEY (`ProductoSerigrafiaId`) REFERENCES `ProductosSerigrafia` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221063813_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_Clientes_Codigo` ON `Clientes` (`Codigo`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221063813_InitialCreate') THEN

    CREATE INDEX `IX_Clientes_Email` ON `Clientes` (`Email`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221063813_InitialCreate') THEN

    CREATE INDEX `IX_Clientes_RfcCif` ON `Clientes` (`RfcCif`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221063813_InitialCreate') THEN

    CREATE INDEX `IX_ColoresSerigrafia_ProductoSerigrafiaId` ON `ColoresSerigrafia` (`ProductoSerigrafiaId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221063813_InitialCreate') THEN

    CREATE INDEX `IX_Contactos_ClienteId` ON `Contactos` (`ClienteId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221063813_InitialCreate') THEN

    CREATE INDEX `IX_Disenos_ClienteId` ON `Disenos` (`ClienteId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221063813_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_Disenos_Codigo` ON `Disenos` (`Codigo`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221063813_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_Hormas_Codigo` ON `Hormas` (`Codigo`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221063813_InitialCreate') THEN

    CREATE INDEX `IX_Hormas_Talla` ON `Hormas` (`Talla`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221063813_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_Pantallas_Codigo` ON `Pantallas` (`Codigo`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221063813_InitialCreate') THEN

    CREATE INDEX `IX_PedidoDetalles_PedidoId` ON `PedidoDetalles` (`PedidoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221063813_InitialCreate') THEN

    CREATE INDEX `IX_PedidoDetalles_ProductoId` ON `PedidoDetalles` (`ProductoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221063813_InitialCreate') THEN

    CREATE INDEX `IX_Pedidos_ClienteId` ON `Pedidos` (`ClienteId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221063813_InitialCreate') THEN

    CREATE INDEX `IX_Pedidos_Estado` ON `Pedidos` (`Estado`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221063813_InitialCreate') THEN

    CREATE INDEX `IX_Pedidos_FechaPedido` ON `Pedidos` (`FechaPedido`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221063813_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_Pedidos_NumeroPedido` ON `Pedidos` (`NumeroPedido`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221063813_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_Productos_Codigo` ON `Productos` (`Codigo`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221063813_InitialCreate') THEN

    CREATE INDEX `IX_Productos_Industria` ON `Productos` (`Industria`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221063813_InitialCreate') THEN

    CREATE INDEX `IX_ProductosCalzado_ProductoId` ON `ProductosCalzado` (`ProductoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221063813_InitialCreate') THEN

    CREATE INDEX `IX_ProductosSerigrafia_ProductoId` ON `ProductosSerigrafia` (`ProductoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221063813_InitialCreate') THEN

    CREATE INDEX `IX_TallasCalzado_ProductoCalzadoId` ON `TallasCalzado` (`ProductoCalzadoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221063813_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_Tintas_Codigo` ON `Tintas` (`Codigo`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221063813_InitialCreate') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260221063813_InitialCreate', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221070945_AgregarModuloSerigrafiaCompleto') THEN

    CREATE TABLE `EscalasSerigrafia` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `Nombre` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `Descripcion` varchar(500) CHARACTER SET utf8mb4 NULL,
        `Talla15` int NOT NULL,
        `Talla16` int NOT NULL,
        `Talla17` int NOT NULL,
        `Talla18` int NOT NULL,
        `Talla19` int NOT NULL,
        `Talla20` int NOT NULL,
        `Talla21` int NOT NULL,
        `Talla22` int NOT NULL,
        `Talla23` int NOT NULL,
        `Talla24` int NOT NULL,
        `Talla25` int NOT NULL,
        `Talla26` int NOT NULL,
        `Talla27` int NOT NULL,
        `Talla28` int NOT NULL,
        `Talla29` int NOT NULL,
        `Talla30` int NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_EscalasSerigrafia` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221070945_AgregarModuloSerigrafiaCompleto') THEN

    CREATE TABLE `PedidosSerigrafia` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `PedidoDetalleId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Estilo` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `Combinacion` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `Referencia` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `LoteCliente` varchar(50) CHARACTER SET utf8mb4 NULL,
        `Corrida` varchar(50) CHARACTER SET utf8mb4 NULL,
        `ProcesoMesa` tinyint(1) NOT NULL,
        `ProcesoPulpo` tinyint(1) NOT NULL,
        `ProcesoTransfer` tinyint(1) NOT NULL,
        `ProcesoSublimacion` tinyint(1) NOT NULL,
        `ProcesoPlancha` tinyint(1) NOT NULL,
        `ProcesoFrecuencia` tinyint(1) NOT NULL,
        `ProcesoPoteo` tinyint(1) NOT NULL,
        `ProcesoUV` tinyint(1) NOT NULL,
        `ProcesoPlastisol` tinyint(1) NOT NULL,
        `Talla15` int NULL,
        `Talla16` int NULL,
        `Talla17` int NULL,
        `Talla18` int NULL,
        `Talla19` int NULL,
        `Talla20` int NULL,
        `Talla21` int NULL,
        `Talla22` int NULL,
        `Talla23` int NULL,
        `Talla24` int NULL,
        `Talla25` int NULL,
        `Talla26` int NULL,
        `Talla27` int NULL,
        `Talla28` int NULL,
        `Talla29` int NULL,
        `Talla30` int NULL,
        `FechaRecibido` datetime(6) NULL,
        `FechaEstimada` datetime(6) NULL,
        `FechaEntregaReal` datetime(6) NULL,
        `Hecho` tinyint(1) NOT NULL,
        `Nota` varchar(500) CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_PedidosSerigrafia` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_PedidosSerigrafia_PedidoDetalles_PedidoDetalleId` FOREIGN KEY (`PedidoDetalleId`) REFERENCES `PedidoDetalles` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221070945_AgregarModuloSerigrafiaCompleto') THEN

    CREATE TABLE `TiposProceso` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `Nombre` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `Descripcion` varchar(500) CHARACTER SET utf8mb4 NULL,
        `CostoBase` decimal(18,2) NOT NULL,
        `Activo` tinyint(1) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_TiposProceso` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221070945_AgregarModuloSerigrafiaCompleto') THEN

    CREATE TABLE `CotizacionesSerigrafia` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `PedidoSerigrafiaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `TiempoTiradoMaterial` decimal(18,2) NOT NULL,
        `TiempoCentradoMaterial` decimal(18,2) NOT NULL,
        `TiempoSerigrafista1` decimal(18,2) NOT NULL,
        `TiempoSerigrafista2` decimal(18,2) NOT NULL,
        `TiempoRecogidoAmarrado` decimal(18,2) NOT NULL,
        `TiempoLavadoPantalla` decimal(18,2) NOT NULL,
        `PrecioTiradoMaterial` decimal(18,2) NOT NULL,
        `PrecioCentradoMaterial` decimal(18,2) NOT NULL,
        `PrecioSerigrafista1` decimal(18,2) NOT NULL,
        `PrecioSerigrafista2` decimal(18,2) NOT NULL,
        `PrecioRecogidoAmarrado` decimal(18,2) NOT NULL,
        `PrecioLavadoPantalla` decimal(18,2) NOT NULL,
        `CostoTiradoMaterial` decimal(18,2) NOT NULL,
        `CostoCentradoMaterial` decimal(18,2) NOT NULL,
        `CostoSerigrafista1` decimal(18,2) NOT NULL,
        `CostoSerigrafista2` decimal(18,2) NOT NULL,
        `CostoRecogidoAmarrado` decimal(18,2) NOT NULL,
        `CostoLavadoPantalla` decimal(18,2) NOT NULL,
        `GastoElectricidadPulpo` decimal(18,2) NOT NULL,
        `GastoElectricidadPlancha` decimal(18,2) NOT NULL,
        `GastosFijos` decimal(18,2) NOT NULL,
        `GastoAdministracion` decimal(18,2) NOT NULL,
        `GastoLuzGeneral` decimal(18,2) NOT NULL,
        `Tinta1PesoInicial` decimal(65,30) NULL,
        `Tinta1PesoFinal` decimal(65,30) NULL,
        `Tinta1Consumo` decimal(65,30) NULL,
        `Tinta1PrecioUnitario` decimal(65,30) NULL,
        `Tinta1Costo` decimal(65,30) NULL,
        `Tinta1Nombre` varchar(100) CHARACTER SET utf8mb4 NULL,
        `Tinta2PesoInicial` decimal(65,30) NULL,
        `Tinta2PesoFinal` decimal(65,30) NULL,
        `Tinta2Consumo` decimal(65,30) NULL,
        `Tinta2PrecioUnitario` decimal(65,30) NULL,
        `Tinta2Costo` decimal(65,30) NULL,
        `Tinta2Nombre` varchar(100) CHARACTER SET utf8mb4 NULL,
        `Tinta3PesoInicial` decimal(65,30) NULL,
        `Tinta3PesoFinal` decimal(65,30) NULL,
        `Tinta3Consumo` decimal(65,30) NULL,
        `Tinta3PrecioUnitario` decimal(65,30) NULL,
        `Tinta3Costo` decimal(65,30) NULL,
        `Tinta3Nombre` varchar(100) CHARACTER SET utf8mb4 NULL,
        `Tinta4PesoInicial` decimal(65,30) NULL,
        `Tinta4PesoFinal` decimal(65,30) NULL,
        `Tinta4Consumo` decimal(65,30) NULL,
        `Tinta4PrecioUnitario` decimal(65,30) NULL,
        `Tinta4Costo` decimal(65,30) NULL,
        `Tinta4Nombre` varchar(100) CHARACTER SET utf8mb4 NULL,
        `Tinta5PesoInicial` decimal(65,30) NULL,
        `Tinta5PesoFinal` decimal(65,30) NULL,
        `Tinta5Consumo` decimal(65,30) NULL,
        `Tinta5PrecioUnitario` decimal(65,30) NULL,
        `Tinta5Costo` decimal(65,30) NULL,
        `Tinta5Nombre` varchar(100) CHARACTER SET utf8mb4 NULL,
        `Tinta6PesoInicial` decimal(65,30) NULL,
        `Tinta6PesoFinal` decimal(65,30) NULL,
        `Tinta6Consumo` decimal(65,30) NULL,
        `Tinta6PrecioUnitario` decimal(65,30) NULL,
        `Tinta6Costo` decimal(65,30) NULL,
        `Tinta6Nombre` varchar(100) CHARACTER SET utf8mb4 NULL,
        `Tinta7PesoInicial` decimal(65,30) NULL,
        `Tinta7PesoFinal` decimal(65,30) NULL,
        `Tinta7Consumo` decimal(65,30) NULL,
        `Tinta7PrecioUnitario` decimal(65,30) NULL,
        `Tinta7Costo` decimal(65,30) NULL,
        `Tinta7Nombre` varchar(100) CHARACTER SET utf8mb4 NULL,
        `Tinta8PesoInicial` decimal(65,30) NULL,
        `Tinta8PesoFinal` decimal(65,30) NULL,
        `Tinta8Consumo` decimal(65,30) NULL,
        `Tinta8PrecioUnitario` decimal(65,30) NULL,
        `Tinta8Costo` decimal(65,30) NULL,
        `Tinta8Nombre` varchar(100) CHARACTER SET utf8mb4 NULL,
        `InsumoTrapoBlancoCantidad` decimal(65,30) NULL,
        `InsumoTrapoBlancoPrecio` decimal(65,30) NULL,
        `InsumoTrapoBlancoCosto` decimal(65,30) NULL,
        `InsumoCicloEcoCantidad` decimal(65,30) NULL,
        `InsumoCicloEcoPrecio` decimal(65,30) NULL,
        `InsumoCicloEcoCosto` decimal(65,30) NULL,
        `InsumoDrySiliconCantidad` decimal(65,30) NULL,
        `InsumoDrySiliconPrecio` decimal(65,30) NULL,
        `InsumoDrySiliconCosto` decimal(65,30) NULL,
        `InsumoPTWaxCantidad` decimal(65,30) NULL,
        `InsumoPTWaxPrecio` decimal(65,30) NULL,
        `InsumoPTWaxCosto` decimal(65,30) NULL,
        `InsumoX6Cantidad` decimal(65,30) NULL,
        `InsumoX6Precio` decimal(65,30) NULL,
        `InsumoX6Costo` decimal(65,30) NULL,
        `InsumoCiclo1RACantidad` decimal(65,30) NULL,
        `InsumoCiclo1RAPrecio` decimal(65,30) NULL,
        `InsumoCiclo1RACosto` decimal(65,30) NULL,
        `CostoTotalPorPar` decimal(18,2) NOT NULL,
        `CostoTotalPorTarea` decimal(18,2) NOT NULL,
        `Utilidad` decimal(18,2) NOT NULL,
        `PrecioSugerido` decimal(18,2) NOT NULL,
        `PrecioFinalContado` decimal(18,2) NOT NULL,
        `PrecioCredito` decimal(18,2) NOT NULL,
        `ParesPorLamina` int NOT NULL,
        `Ganancia` decimal(18,2) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_CotizacionesSerigrafia` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_CotizacionesSerigrafia_PedidosSerigrafia_PedidoSerigrafiaId` FOREIGN KEY (`PedidoSerigrafiaId`) REFERENCES `PedidosSerigrafia` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221070945_AgregarModuloSerigrafiaCompleto') THEN

    CREATE INDEX `IX_CotizacionesSerigrafia_PedidoSerigrafiaId` ON `CotizacionesSerigrafia` (`PedidoSerigrafiaId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221070945_AgregarModuloSerigrafiaCompleto') THEN

    CREATE INDEX `IX_PedidosSerigrafia_FechaEstimada` ON `PedidosSerigrafia` (`FechaEstimada`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221070945_AgregarModuloSerigrafiaCompleto') THEN

    CREATE INDEX `IX_PedidosSerigrafia_Hecho` ON `PedidosSerigrafia` (`Hecho`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221070945_AgregarModuloSerigrafiaCompleto') THEN

    CREATE INDEX `IX_PedidosSerigrafia_PedidoDetalleId` ON `PedidosSerigrafia` (`PedidoDetalleId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221070945_AgregarModuloSerigrafiaCompleto') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260221070945_AgregarModuloSerigrafiaCompleto', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221071743_AgregarRelacionTiposProceso') THEN

    ALTER TABLE `TiposProceso` ADD `Orden` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221071743_AgregarRelacionTiposProceso') THEN

    CREATE TABLE `PedidoSerigrafiaProcesoDetalles` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `PedidoSerigrafiaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `TipoProcesoId` char(36) COLLATE ascii_general_ci NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_PedidoSerigrafiaProcesoDetalles` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_PedidoSerigrafiaProcesoDetalles_PedidosSerigrafia_PedidoSeri~` FOREIGN KEY (`PedidoSerigrafiaId`) REFERENCES `PedidosSerigrafia` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_PedidoSerigrafiaProcesoDetalles_TiposProceso_TipoProcesoId` FOREIGN KEY (`TipoProcesoId`) REFERENCES `TiposProceso` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221071743_AgregarRelacionTiposProceso') THEN

    CREATE INDEX `IX_TiposProceso_Activo` ON `TiposProceso` (`Activo`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221071743_AgregarRelacionTiposProceso') THEN

    CREATE INDEX `IX_TiposProceso_Nombre` ON `TiposProceso` (`Nombre`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221071743_AgregarRelacionTiposProceso') THEN

    CREATE INDEX `IX_PedidoSerigrafiaProcesoDetalles_PedidoSerigrafiaId` ON `PedidoSerigrafiaProcesoDetalles` (`PedidoSerigrafiaId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221071743_AgregarRelacionTiposProceso') THEN

    CREATE INDEX `IX_PedidoSerigrafiaProcesoDetalles_TipoProcesoId` ON `PedidoSerigrafiaProcesoDetalles` (`TipoProcesoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221071743_AgregarRelacionTiposProceso') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260221071743_AgregarRelacionTiposProceso', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221073343_AgregarProductoCliente') THEN

    CREATE TABLE `ProductosClientes` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `ClienteId` char(36) COLLATE ascii_general_ci NOT NULL,
        `ProductoId` char(36) COLLATE ascii_general_ci NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_ProductosClientes` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_ProductosClientes_Clientes_ClienteId` FOREIGN KEY (`ClienteId`) REFERENCES `Clientes` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_ProductosClientes_Productos_ProductoId` FOREIGN KEY (`ProductoId`) REFERENCES `Productos` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221073343_AgregarProductoCliente') THEN

    CREATE INDEX `IX_ProductosClientes_ClienteId` ON `ProductosClientes` (`ClienteId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221073343_AgregarProductoCliente') THEN

    CREATE UNIQUE INDEX `IX_ProductosClientes_ClienteId_ProductoId` ON `ProductosClientes` (`ClienteId`, `ProductoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221073343_AgregarProductoCliente') THEN

    CREATE INDEX `IX_ProductosClientes_ProductoId` ON `ProductosClientes` (`ProductoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221073343_AgregarProductoCliente') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260221073343_AgregarProductoCliente', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221073843_QuitarStockProducto') THEN

    ALTER TABLE `Productos` DROP COLUMN `StockActual`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221073843_QuitarStockProducto') THEN

    ALTER TABLE `Productos` DROP COLUMN `StockMinimo`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221073843_QuitarStockProducto') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260221073843_QuitarStockProducto', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221080421_AgregarPedidoSeguimiento') THEN

    CREATE TABLE `PedidosSeguimiento` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `PedidoId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Tipo` int NOT NULL,
        `Titulo` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `Descripcion` varchar(2000) CHARACTER SET utf8mb4 NULL,
        `Fecha` datetime(6) NOT NULL,
        `Completado` tinyint(1) NOT NULL,
        `FechaCompletado` datetime(6) NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_PedidosSeguimiento` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_PedidosSeguimiento_Pedidos_PedidoId` FOREIGN KEY (`PedidoId`) REFERENCES `Pedidos` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221080421_AgregarPedidoSeguimiento') THEN

    CREATE INDEX `IX_PedidosSeguimiento_Completado` ON `PedidosSeguimiento` (`Completado`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221080421_AgregarPedidoSeguimiento') THEN

    CREATE INDEX `IX_PedidosSeguimiento_Fecha` ON `PedidosSeguimiento` (`Fecha`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221080421_AgregarPedidoSeguimiento') THEN

    CREATE INDEX `IX_PedidosSeguimiento_PedidoId` ON `PedidosSeguimiento` (`PedidoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221080421_AgregarPedidoSeguimiento') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260221080421_AgregarPedidoSeguimiento', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221151955_AgregarReferenciaProducto') THEN

    ALTER TABLE `Productos` ADD `Referencia` varchar(100) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221151955_AgregarReferenciaProducto') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260221151955_AgregarReferenciaProducto', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221152332_AgregarOrdenCompraPedidoSerigrafia') THEN

    ALTER TABLE `PedidosSerigrafia` ADD `OrdenCompra` varchar(50) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221152332_AgregarOrdenCompraPedidoSerigrafia') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260221152332_AgregarOrdenCompraPedidoSerigrafia', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221152643_FixProductoReferenciaMapping') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260221152643_FixProductoReferenciaMapping', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221152836_AplicarColumnasReferenciaOC') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260221152836_AplicarColumnasReferenciaOC', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221153725_QuitarReferenciaPedidoSerigrafia_RenombrarCombinacionColor') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260221153725_QuitarReferenciaPedidoSerigrafia_RenombrarCombinacionColor', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221154119_AplicarCombinacionColorPedidoSerigrafia') THEN

    ALTER TABLE `PedidosSerigrafia` DROP COLUMN `Referencia`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221154119_AplicarCombinacionColorPedidoSerigrafia') THEN

    ALTER TABLE `PedidosSerigrafia` RENAME COLUMN `Combinacion` TO `CombinacionColor`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221154119_AplicarCombinacionColorPedidoSerigrafia') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260221154119_AplicarCombinacionColorPedidoSerigrafia', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221162835_AgregarPresupuestoProducto') THEN

    CREATE TABLE `PresupuestosProducto` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `ProductoId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Nombre` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `Descripcion` varchar(500) CHARACTER SET utf8mb4 NULL,
        `SueldoSemanalBase` decimal(18,2) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_PresupuestosProducto` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_PresupuestosProducto_Productos_ProductoId` FOREIGN KEY (`ProductoId`) REFERENCES `Productos` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221162835_AgregarPresupuestoProducto') THEN

    CREATE TABLE `PresupuestosDetalle` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `PresupuestoProductoId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Categoria` int NOT NULL,
        `Concepto` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `Consumo` decimal(18,4) NOT NULL,
        `Costo` decimal(18,4) NOT NULL,
        `SueldoSugerido` decimal(18,2) NULL,
        `TiempoCompleto` decimal(18,2) NULL,
        `TiempoBasico` decimal(18,2) NULL,
        `Orden` int NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_PresupuestosDetalle` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_PresupuestosDetalle_PresupuestosProducto_PresupuestoProducto~` FOREIGN KEY (`PresupuestoProductoId`) REFERENCES `PresupuestosProducto` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221162835_AgregarPresupuestoProducto') THEN

    CREATE INDEX `IX_PresupuestosDetalle_Categoria` ON `PresupuestosDetalle` (`Categoria`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221162835_AgregarPresupuestoProducto') THEN

    CREATE INDEX `IX_PresupuestosDetalle_PresupuestoProductoId` ON `PresupuestosDetalle` (`PresupuestoProductoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221162835_AgregarPresupuestoProducto') THEN

    CREATE INDEX `IX_PresupuestosProducto_ProductoId` ON `PresupuestosProducto` (`ProductoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221162835_AgregarPresupuestoProducto') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260221162835_AgregarPresupuestoProducto', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221164258_AgregarAppConfig') THEN

    CREATE TABLE `AppConfigs` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `Clave` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `Valor` varchar(2000) CHARACTER SET utf8mb4 NOT NULL,
        `Descripcion` varchar(500) CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        CONSTRAINT `PK_AppConfigs` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221164258_AgregarAppConfig') THEN

    INSERT INTO `AppConfigs` (`Id`, `Clave`, `CreatedAt`, `Descripcion`, `UpdatedAt`, `Valor`)
    VALUES ('00000000-0000-0000-0000-000000000001', 'CompanyName', TIMESTAMP '2026-02-21 16:42:57', 'Nombre de la empresa', NULL, 'MundoVs'),
    ('00000000-0000-0000-0000-000000000002', 'CompanySlogan', TIMESTAMP '2026-02-21 16:42:57', 'Slogan o subtítulo', NULL, 'CRM & Producción');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221164258_AgregarAppConfig') THEN

    CREATE UNIQUE INDEX `IX_AppConfigs_Clave` ON `AppConfigs` (`Clave`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221164258_AgregarAppConfig') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260221164258_AgregarAppConfig', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221171117_RenombrarTintasAMateriasPrimas_AgregarCatalogos') THEN

    ALTER TABLE `Tintas` RENAME `MateriasPrimas`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221171117_RenombrarTintasAMateriasPrimas_AgregarCatalogos') THEN

    ALTER TABLE `MateriasPrimas` RENAME COLUMN `Tipo` TO `TipoMateriaPrima`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221171117_RenombrarTintasAMateriasPrimas_AgregarCatalogos') THEN

    ALTER TABLE `MateriasPrimas` RENAME INDEX `IX_Tintas_Codigo` TO `IX_MateriasPrimas_Codigo`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221171117_RenombrarTintasAMateriasPrimas_AgregarCatalogos') THEN

    ALTER TABLE `MateriasPrimas` ADD `PrecioUnitario` decimal(18,4) NOT NULL DEFAULT 0.0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221171117_RenombrarTintasAMateriasPrimas_AgregarCatalogos') THEN

    CREATE INDEX `IX_MateriasPrimas_TipoMateriaPrima` ON `MateriasPrimas` (`TipoMateriaPrima`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221171117_RenombrarTintasAMateriasPrimas_AgregarCatalogos') THEN

    CREATE TABLE `ActividadesManoObra` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `Nombre` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `SueldoSugeridoSemanal` decimal(18,2) NOT NULL,
        `Orden` int NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_ActividadesManoObra` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221171117_RenombrarTintasAMateriasPrimas_AgregarCatalogos') THEN

    CREATE INDEX `IX_ActividadesManoObra_Nombre` ON `ActividadesManoObra` (`Nombre`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221171117_RenombrarTintasAMateriasPrimas_AgregarCatalogos') THEN

    CREATE TABLE `GastosFijos` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `Concepto` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `CostoMensual` decimal(18,2) NOT NULL,
        `Orden` int NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_GastosFijos` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221171117_RenombrarTintasAMateriasPrimas_AgregarCatalogos') THEN

    CREATE INDEX `IX_GastosFijos_Concepto` ON `GastosFijos` (`Concepto`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221171117_RenombrarTintasAMateriasPrimas_AgregarCatalogos') THEN

    CREATE TABLE `Insumos` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `Codigo` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `Nombre` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `TipoInsumo` int NOT NULL,
        `PrecioUnitario` decimal(18,4) NOT NULL,
        `Cantidad` decimal(18,2) NOT NULL,
        `UnidadMedida` varchar(10) CHARACTER SET utf8mb4 NOT NULL,
        `StockMinimo` decimal(18,2) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_Insumos` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221171117_RenombrarTintasAMateriasPrimas_AgregarCatalogos') THEN

    CREATE UNIQUE INDEX `IX_Insumos_Codigo` ON `Insumos` (`Codigo`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221171117_RenombrarTintasAMateriasPrimas_AgregarCatalogos') THEN

    CREATE INDEX `IX_Insumos_TipoInsumo` ON `Insumos` (`TipoInsumo`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260221171117_RenombrarTintasAMateriasPrimas_AgregarCatalogos') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260221171117_RenombrarTintasAMateriasPrimas_AgregarCatalogos', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260222015448_NormalizarPedidoSerigrafiaYCotizacion') THEN


    ALTER TABLE `PedidosSerigrafia`
        DROP COLUMN IF EXISTS `ProcesoFrecuencia`,
        DROP COLUMN IF EXISTS `ProcesoMesa`,
        DROP COLUMN IF EXISTS `ProcesoPlancha`,
        DROP COLUMN IF EXISTS `ProcesoPlastisol`,
        DROP COLUMN IF EXISTS `ProcesoPoteo`,
        DROP COLUMN IF EXISTS `ProcesoPulpo`,
        DROP COLUMN IF EXISTS `ProcesoSublimacion`,
        DROP COLUMN IF EXISTS `ProcesoTransfer`,
        DROP COLUMN IF EXISTS `ProcesoUV`,
        DROP COLUMN IF EXISTS `Talla15`,
        DROP COLUMN IF EXISTS `Talla16`,
        DROP COLUMN IF EXISTS `Talla17`,
        DROP COLUMN IF EXISTS `Talla18`,
        DROP COLUMN IF EXISTS `Talla19`,
        DROP COLUMN IF EXISTS `Talla20`,
        DROP COLUMN IF EXISTS `Talla21`,
        DROP COLUMN IF EXISTS `Talla22`,
        DROP COLUMN IF EXISTS `Talla23`,
        DROP COLUMN IF EXISTS `Talla24`,
        DROP COLUMN IF EXISTS `Talla25`,
        DROP COLUMN IF EXISTS `Talla26`,
        DROP COLUMN IF EXISTS `Talla27`,
        DROP COLUMN IF EXISTS `Talla28`,
        DROP COLUMN IF EXISTS `Talla29`,
        DROP COLUMN IF EXISTS `Talla30`;


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260222015448_NormalizarPedidoSerigrafiaYCotizacion') THEN


    ALTER TABLE `EscalasSerigrafia`
        DROP COLUMN IF EXISTS `Talla15`,
        DROP COLUMN IF EXISTS `Talla16`,
        DROP COLUMN IF EXISTS `Talla17`,
        DROP COLUMN IF EXISTS `Talla18`,
        DROP COLUMN IF EXISTS `Talla19`,
        DROP COLUMN IF EXISTS `Talla20`,
        DROP COLUMN IF EXISTS `Talla21`,
        DROP COLUMN IF EXISTS `Talla22`,
        DROP COLUMN IF EXISTS `Talla23`,
        DROP COLUMN IF EXISTS `Talla24`,
        DROP COLUMN IF EXISTS `Talla25`,
        DROP COLUMN IF EXISTS `Talla26`,
        DROP COLUMN IF EXISTS `Talla27`,
        DROP COLUMN IF EXISTS `Talla28`,
        DROP COLUMN IF EXISTS `Talla29`,
        DROP COLUMN IF EXISTS `Talla30`;


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260222015448_NormalizarPedidoSerigrafiaYCotizacion') THEN


    ALTER TABLE `CotizacionesSerigrafia`
        DROP COLUMN IF EXISTS `CostoCentradoMaterial`,
        DROP COLUMN IF EXISTS `CostoLavadoPantalla`,
        DROP COLUMN IF EXISTS `CostoRecogidoAmarrado`,
        DROP COLUMN IF EXISTS `CostoSerigrafista1`,
        DROP COLUMN IF EXISTS `CostoSerigrafista2`,
        DROP COLUMN IF EXISTS `CostoTiradoMaterial`,
        DROP COLUMN IF EXISTS `GastoAdministracion`,
        DROP COLUMN IF EXISTS `GastoElectricidadPlancha`,
        DROP COLUMN IF EXISTS `GastoElectricidadPulpo`,
        DROP COLUMN IF EXISTS `GastoLuzGeneral`,
        DROP COLUMN IF EXISTS `GastosFijos`,
        DROP COLUMN IF EXISTS `InsumoCiclo1RACantidad`,
        DROP COLUMN IF EXISTS `InsumoCiclo1RACosto`,
        DROP COLUMN IF EXISTS `InsumoCiclo1RAPrecio`,
        DROP COLUMN IF EXISTS `InsumoCicloEcoCantidad`,
        DROP COLUMN IF EXISTS `InsumoCicloEcoCosto`,
        DROP COLUMN IF EXISTS `InsumoCicloEcoPrecio`,
        DROP COLUMN IF EXISTS `InsumoDrySiliconCantidad`,
        DROP COLUMN IF EXISTS `InsumoDrySiliconCosto`,
        DROP COLUMN IF EXISTS `InsumoDrySiliconPrecio`,
        DROP COLUMN IF EXISTS `InsumoPTWaxCantidad`,
        DROP COLUMN IF EXISTS `InsumoPTWaxCosto`,
        DROP COLUMN IF EXISTS `InsumoPTWaxPrecio`,
        DROP COLUMN IF EXISTS `InsumoTrapoBlancoCantidad`,
        DROP COLUMN IF EXISTS `InsumoTrapoBlancoCosto`,
        DROP COLUMN IF EXISTS `InsumoTrapoBlancoPrecio`,
        DROP COLUMN IF EXISTS `InsumoX6Cantidad`,
        DROP COLUMN IF EXISTS `InsumoX6Costo`,
        DROP COLUMN IF EXISTS `InsumoX6Precio`,
        DROP COLUMN IF EXISTS `PrecioCentradoMaterial`,
        DROP COLUMN IF EXISTS `PrecioLavadoPantalla`,
        DROP COLUMN IF EXISTS `PrecioRecogidoAmarrado`,
        DROP COLUMN IF EXISTS `PrecioSerigrafista1`,
        DROP COLUMN IF EXISTS `PrecioSerigrafista2`,
        DROP COLUMN IF EXISTS `PrecioTiradoMaterial`,
        DROP COLUMN IF EXISTS `TiempoCentradoMaterial`,
        DROP COLUMN IF EXISTS `TiempoLavadoPantalla`,
        DROP COLUMN IF EXISTS `TiempoRecogidoAmarrado`,
        DROP COLUMN IF EXISTS `TiempoSerigrafista1`,
        DROP COLUMN IF EXISTS `TiempoSerigrafista2`,
        DROP COLUMN IF EXISTS `TiempoTiradoMaterial`,
        DROP COLUMN IF EXISTS `Tinta1Consumo`,
        DROP COLUMN IF EXISTS `Tinta1Costo`,
        DROP COLUMN IF EXISTS `Tinta1Nombre`,
        DROP COLUMN IF EXISTS `Tinta1PesoFinal`,
        DROP COLUMN IF EXISTS `Tinta1PesoInicial`,
        DROP COLUMN IF EXISTS `Tinta1PrecioUnitario`,
        DROP COLUMN IF EXISTS `Tinta2Consumo`,
        DROP COLUMN IF EXISTS `Tinta2Costo`,
        DROP COLUMN IF EXISTS `Tinta2Nombre`,
        DROP COLUMN IF EXISTS `Tinta2PesoFinal`,
        DROP COLUMN IF EXISTS `Tinta2PesoInicial`,
        DROP COLUMN IF EXISTS `Tinta2PrecioUnitario`,
        DROP COLUMN IF EXISTS `Tinta3Consumo`,
        DROP COLUMN IF EXISTS `Tinta3Costo`,
        DROP COLUMN IF EXISTS `Tinta3Nombre`,
        DROP COLUMN IF EXISTS `Tinta3PesoFinal`,
        DROP COLUMN IF EXISTS `Tinta3PesoInicial`,
        DROP COLUMN IF EXISTS `Tinta3PrecioUnitario`,
        DROP COLUMN IF EXISTS `Tinta4Consumo`,
        DROP COLUMN IF EXISTS `Tinta4Costo`,
        DROP COLUMN IF EXISTS `Tinta4Nombre`,
        DROP COLUMN IF EXISTS `Tinta4PesoFinal`,
        DROP COLUMN IF EXISTS `Tinta4PesoInicial`,
        DROP COLUMN IF EXISTS `Tinta4PrecioUnitario`,
        DROP COLUMN IF EXISTS `Tinta5Consumo`,
        DROP COLUMN IF EXISTS `Tinta5Costo`,
        DROP COLUMN IF EXISTS `Tinta5Nombre`,
        DROP COLUMN IF EXISTS `Tinta5PesoFinal`,
        DROP COLUMN IF EXISTS `Tinta5PesoInicial`,
        DROP COLUMN IF EXISTS `Tinta5PrecioUnitario`,
        DROP COLUMN IF EXISTS `Tinta6Consumo`,
        DROP COLUMN IF EXISTS `Tinta6Costo`,
        DROP COLUMN IF EXISTS `Tinta6Nombre`,
        DROP COLUMN IF EXISTS `Tinta6PesoFinal`,
        DROP COLUMN IF EXISTS `Tinta6PesoInicial`,
        DROP COLUMN IF EXISTS `Tinta6PrecioUnitario`,
        DROP COLUMN IF EXISTS `Tinta7Consumo`,
        DROP COLUMN IF EXISTS `Tinta7Costo`,
        DROP COLUMN IF EXISTS `Tinta7Nombre`,
        DROP COLUMN IF EXISTS `Tinta7PesoFinal`,
        DROP COLUMN IF EXISTS `Tinta7PesoInicial`,
        DROP COLUMN IF EXISTS `Tinta7PrecioUnitario`,
        DROP COLUMN IF EXISTS `Tinta8Consumo`,
        DROP COLUMN IF EXISTS `Tinta8Costo`,
        DROP COLUMN IF EXISTS `Tinta8Nombre`,
        DROP COLUMN IF EXISTS `Tinta8PesoFinal`,
        DROP COLUMN IF EXISTS `Tinta8PesoInicial`,
        DROP COLUMN IF EXISTS `Tinta8PrecioUnitario`;


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260222015448_NormalizarPedidoSerigrafiaYCotizacion') THEN


    CREATE TABLE IF NOT EXISTS `CotizacionDetalles` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `CotizacionSerigrafiaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Categoria` int NOT NULL,
        `Concepto` varchar(200) NOT NULL,
        `Orden` int NOT NULL,
        `Consumo` decimal(18,4) NOT NULL,
        `PrecioUnitario` decimal(18,4) NOT NULL,
        `Costo` decimal(18,4) NOT NULL,
        `PesoInicial` decimal(18,4) NULL,
        `PesoFinal` decimal(18,4) NULL,
        `Tiempo` decimal(18,2) NULL,
        `Precio` decimal(18,2) NULL,
        `MateriaPrimaId` char(36) COLLATE ascii_general_ci NULL,
        `InsumoId` char(36) COLLATE ascii_general_ci NULL,
        `ActividadManoObraId` char(36) COLLATE ascii_general_ci NULL,
        `GastoFijoId` char(36) COLLATE ascii_general_ci NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext NULL,
        `UpdatedBy` longtext NULL,
        `IsActive` tinyint(1) NOT NULL,
        PRIMARY KEY (`Id`),
        KEY `IX_CotizacionDetalles_CotizacionSerigrafiaId` (`CotizacionSerigrafiaId`),
        KEY `IX_CotizacionDetalles_Categoria` (`Categoria`),
        KEY `IX_CotizacionDetalles_ActividadManoObraId` (`ActividadManoObraId`),
        KEY `IX_CotizacionDetalles_GastoFijoId` (`GastoFijoId`),
        KEY `IX_CotizacionDetalles_InsumoId` (`InsumoId`),
        KEY `IX_CotizacionDetalles_MateriaPrimaId` (`MateriaPrimaId`),
        CONSTRAINT `FK_CotDet_CotSer` FOREIGN KEY (`CotizacionSerigrafiaId`) REFERENCES `CotizacionesSerigrafia` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_CotDet_ActMO` FOREIGN KEY (`ActividadManoObraId`) REFERENCES `ActividadesManoObra` (`Id`) ON DELETE SET NULL,
        CONSTRAINT `FK_CotDet_GasFix` FOREIGN KEY (`GastoFijoId`) REFERENCES `GastosFijos` (`Id`) ON DELETE SET NULL,
        CONSTRAINT `FK_CotDet_Insumo` FOREIGN KEY (`InsumoId`) REFERENCES `Insumos` (`Id`) ON DELETE SET NULL,
        CONSTRAINT `FK_CotDet_MatPri` FOREIGN KEY (`MateriaPrimaId`) REFERENCES `MateriasPrimas` (`Id`) ON DELETE SET NULL
    ) CHARACTER SET=utf8mb4;


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260222015448_NormalizarPedidoSerigrafiaYCotizacion') THEN


    CREATE TABLE IF NOT EXISTS `EscalaSerigrafiaTallas` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `EscalaSerigrafiaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Talla` varchar(10) NOT NULL,
        `Cantidad` int NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext NULL,
        `UpdatedBy` longtext NULL,
        `IsActive` tinyint(1) NOT NULL,
        PRIMARY KEY (`Id`),
        KEY `IX_EscalaSerigrafiaTallas_EscalaSerigrafiaId` (`EscalaSerigrafiaId`),
        CONSTRAINT `FK_EscalaTall_Escala` FOREIGN KEY (`EscalaSerigrafiaId`) REFERENCES `EscalasSerigrafia` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260222015448_NormalizarPedidoSerigrafiaYCotizacion') THEN


    CREATE TABLE IF NOT EXISTS `PedidoSerigrafiaTallas` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `PedidoSerigrafiaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Talla` varchar(10) NOT NULL,
        `Cantidad` int NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext NULL,
        `UpdatedBy` longtext NULL,
        `IsActive` tinyint(1) NOT NULL,
        PRIMARY KEY (`Id`),
        KEY `IX_PedidoSerigrafiaTallas_PedidoSerigrafiaId` (`PedidoSerigrafiaId`),
        CONSTRAINT `FK_PedTall_Pedido` FOREIGN KEY (`PedidoSerigrafiaId`) REFERENCES `PedidosSerigrafia` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260222015448_NormalizarPedidoSerigrafiaYCotizacion') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260222015448_NormalizarPedidoSerigrafiaYCotizacion', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260222050940_CotizacionPorProducto') THEN


    SET @fk := (
        SELECT CONSTRAINT_NAME
        FROM information_schema.KEY_COLUMN_USAGE
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = 'CotizacionesSerigrafia'
          AND CONSTRAINT_NAME = 'FK_CotizacionesSerigrafia_PedidosSerigrafia_PedidoSerigrafiaId'
        LIMIT 1
    );
    SET @sql := IF(@fk IS NULL, 'SELECT 1', 'ALTER TABLE `CotizacionesSerigrafia` DROP FOREIGN KEY `FK_CotizacionesSerigrafia_PedidosSerigrafia_PedidoSerigrafiaId`');
    PREPARE stmt FROM @sql;
    EXECUTE stmt;
    DEALLOCATE PREPARE stmt;


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260222050940_CotizacionPorProducto') THEN


    SET @col_pedido := (
        SELECT COUNT(*)
        FROM information_schema.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = 'CotizacionesSerigrafia'
          AND COLUMN_NAME = 'PedidoSerigrafiaId'
    );
    SET @col_producto := (
        SELECT COUNT(*)
        FROM information_schema.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = 'CotizacionesSerigrafia'
          AND COLUMN_NAME = 'ProductoId'
    );
    SET @sql := IF(@col_pedido = 1 AND @col_producto = 0,
        'ALTER TABLE `CotizacionesSerigrafia` CHANGE COLUMN `PedidoSerigrafiaId` `ProductoId` char(36) COLLATE ascii_general_ci NOT NULL',
        'SELECT 1'
    );
    PREPARE stmt FROM @sql;
    EXECUTE stmt;
    DEALLOCATE PREPARE stmt;

    SET @idx_pedido := (
        SELECT COUNT(*)
        FROM information_schema.STATISTICS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = 'CotizacionesSerigrafia'
          AND INDEX_NAME = 'IX_CotizacionesSerigrafia_PedidoSerigrafiaId'
    );
    SET @idx_producto := (
        SELECT COUNT(*)
        FROM information_schema.STATISTICS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = 'CotizacionesSerigrafia'
          AND INDEX_NAME = 'IX_CotizacionesSerigrafia_ProductoId'
    );
    SET @sql := IF(@idx_pedido = 1,
        'DROP INDEX `IX_CotizacionesSerigrafia_PedidoSerigrafiaId` ON `CotizacionesSerigrafia`',
        'SELECT 1'
    );
    PREPARE stmt FROM @sql;
    EXECUTE stmt;
    DEALLOCATE PREPARE stmt;

    SET @sql := IF(@idx_producto = 0,
        'CREATE INDEX `IX_CotizacionesSerigrafia_ProductoId` ON `CotizacionesSerigrafia` (`ProductoId`)',
        'SELECT 1'
    );
    PREPARE stmt FROM @sql;
    EXECUTE stmt;
    DEALLOCATE PREPARE stmt;


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260222050940_CotizacionPorProducto') THEN


    SET @col_desc := (
        SELECT COUNT(*)
        FROM information_schema.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = 'CotizacionesSerigrafia'
          AND COLUMN_NAME = 'Descripcion'
    );
    SET @sql := IF(@col_desc = 0,
        'ALTER TABLE `CotizacionesSerigrafia` ADD `Descripcion` varchar(500) CHARACTER SET utf8mb4 NULL',
        'SELECT 1'
    );
    PREPARE stmt FROM @sql;
    EXECUTE stmt;
    DEALLOCATE PREPARE stmt;

    SET @col_np := (
        SELECT COUNT(*)
        FROM information_schema.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = 'CotizacionesSerigrafia'
          AND COLUMN_NAME = 'NumeroPersonas'
    );
    SET @sql := IF(@col_np = 0,
        'ALTER TABLE `CotizacionesSerigrafia` ADD `NumeroPersonas` int NOT NULL DEFAULT 0',
        'SELECT 1'
    );
    PREPARE stmt FROM @sql;
    EXECUTE stmt;
    DEALLOCATE PREPARE stmt;


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260222050940_CotizacionPorProducto') THEN

    ALTER TABLE `CotizacionesSerigrafia` ADD CONSTRAINT `FK_CotizacionesSerigrafia_Productos_ProductoId` FOREIGN KEY (`ProductoId`) REFERENCES `Productos` (`Id`) ON DELETE RESTRICT;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260222050940_CotizacionPorProducto') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260222050940_CotizacionPorProducto', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260222053108_CotizacionGananciaMinuto') THEN


    SET @col_gan := (
        SELECT COUNT(*)
        FROM information_schema.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = 'CotizacionDetalles'
          AND COLUMN_NAME = 'GananciaMinuto'
    );
    SET @sql := IF(@col_gan = 0,
        'ALTER TABLE `CotizacionDetalles` ADD `GananciaMinuto` decimal(18,4) NULL',
        'SELECT 1'
    );
    PREPARE stmt FROM @sql;
    EXECUTE stmt;
    DEALLOCATE PREPARE stmt;


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260222053108_CotizacionGananciaMinuto') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260222053108_CotizacionGananciaMinuto', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260222054019_CotizacionGananciaPorcentaje') THEN


    SET @col_min := (
        SELECT COUNT(*)
        FROM information_schema.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = 'CotizacionDetalles'
          AND COLUMN_NAME = 'GananciaMinuto'
    );
    SET @col_pct := (
        SELECT COUNT(*)
        FROM information_schema.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = 'CotizacionDetalles'
          AND COLUMN_NAME = 'GananciaPorcentaje'
    );
    SET @sql := IF(@col_min = 1 AND @col_pct = 0,
        'ALTER TABLE `CotizacionDetalles` CHANGE COLUMN `GananciaMinuto` `GananciaPorcentaje` decimal(18,4) NULL',
        'SELECT 1'
    );
    PREPARE stmt FROM @sql;
    EXECUTE stmt;
    DEALLOCATE PREPARE stmt;


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260222054019_CotizacionGananciaPorcentaje') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260222054019_CotizacionGananciaPorcentaje', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260222180153_PedidoSerigrafiaTallaProcesos') THEN

    CREATE TABLE `PedidoSerigrafiaTallaProcesos` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `PedidoSerigrafiaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `PedidoSerigrafiaTallaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `TipoProcesoId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Completado` tinyint(1) NOT NULL DEFAULT FALSE,
        `FechaPaso` datetime(6) NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_PedidoSerigrafiaTallaProcesos` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_PedidoSerigrafiaTallaProcesos_PedidoSerigrafiaTallas_PedidoS~` FOREIGN KEY (`PedidoSerigrafiaTallaId`) REFERENCES `PedidoSerigrafiaTallas` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_PedidoSerigrafiaTallaProcesos_PedidosSerigrafia_PedidoSerigr~` FOREIGN KEY (`PedidoSerigrafiaId`) REFERENCES `PedidosSerigrafia` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_PedidoSerigrafiaTallaProcesos_TiposProceso_TipoProcesoId` FOREIGN KEY (`TipoProcesoId`) REFERENCES `TiposProceso` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260222180153_PedidoSerigrafiaTallaProcesos') THEN

    UPDATE `AppConfigs` SET `CreatedAt` = TIMESTAMP '2026-02-22 18:01:52'
    WHERE `Id` = '00000000-0000-0000-0000-000000000001';
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260222180153_PedidoSerigrafiaTallaProcesos') THEN

    UPDATE `AppConfigs` SET `CreatedAt` = TIMESTAMP '2026-02-22 18:01:52'
    WHERE `Id` = '00000000-0000-0000-0000-000000000002';
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260222180153_PedidoSerigrafiaTallaProcesos') THEN

    UPDATE `AppConfigs` SET `CreatedAt` = TIMESTAMP '2026-02-22 18:01:52'
    WHERE `Id` = '00000000-0000-0000-0000-000000000010';
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260222180153_PedidoSerigrafiaTallaProcesos') THEN

    UPDATE `AppConfigs` SET `CreatedAt` = TIMESTAMP '2026-02-22 18:01:52'
    WHERE `Id` = '00000000-0000-0000-0000-000000000011';
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260222180153_PedidoSerigrafiaTallaProcesos') THEN

    UPDATE `AppConfigs` SET `CreatedAt` = TIMESTAMP '2026-02-22 18:01:52'
    WHERE `Id` = '00000000-0000-0000-0000-000000000020';
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260222180153_PedidoSerigrafiaTallaProcesos') THEN

    UPDATE `AppConfigs` SET `CreatedAt` = TIMESTAMP '2026-02-22 18:01:52'
    WHERE `Id` = '00000000-0000-0000-0000-000000000021';
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260222180153_PedidoSerigrafiaTallaProcesos') THEN

    UPDATE `AppConfigs` SET `CreatedAt` = TIMESTAMP '2026-02-22 18:01:52'
    WHERE `Id` = '00000000-0000-0000-0000-000000000022';
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260222180153_PedidoSerigrafiaTallaProcesos') THEN

    UPDATE `AppConfigs` SET `CreatedAt` = TIMESTAMP '2026-02-22 18:01:52'
    WHERE `Id` = '00000000-0000-0000-0000-000000000023';
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260222180153_PedidoSerigrafiaTallaProcesos') THEN

    CREATE UNIQUE INDEX `IX_PedidoSerigrafiaTallaProcesos_PedidoSerigrafiaId_PedidoSerig~` ON `PedidoSerigrafiaTallaProcesos` (`PedidoSerigrafiaId`, `PedidoSerigrafiaTallaId`, `TipoProcesoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260222180153_PedidoSerigrafiaTallaProcesos') THEN

    CREATE INDEX `IX_PedidoSerigrafiaTallaProcesos_PedidoSerigrafiaTallaId` ON `PedidoSerigrafiaTallaProcesos` (`PedidoSerigrafiaTallaId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260222180153_PedidoSerigrafiaTallaProcesos') THEN

    CREATE INDEX `IX_PedidoSerigrafiaTallaProcesos_TipoProcesoId` ON `PedidoSerigrafiaTallaProcesos` (`TipoProcesoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260222180153_PedidoSerigrafiaTallaProcesos') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260222180153_PedidoSerigrafiaTallaProcesos', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260225024415_RenombrarNotaAFactura') THEN

    ALTER TABLE `PedidosSerigrafia` RENAME COLUMN `Nota` TO `Factura`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260225024415_RenombrarNotaAFactura') THEN

    UPDATE `AppConfigs` SET `CreatedAt` = TIMESTAMP '2026-02-25 02:44:13'
    WHERE `Id` = '00000000-0000-0000-0000-000000000001';
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260225024415_RenombrarNotaAFactura') THEN

    UPDATE `AppConfigs` SET `CreatedAt` = TIMESTAMP '2026-02-25 02:44:13'
    WHERE `Id` = '00000000-0000-0000-0000-000000000002';
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260225024415_RenombrarNotaAFactura') THEN

    UPDATE `AppConfigs` SET `CreatedAt` = TIMESTAMP '2026-02-25 02:44:13'
    WHERE `Id` = '00000000-0000-0000-0000-000000000010';
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260225024415_RenombrarNotaAFactura') THEN

    UPDATE `AppConfigs` SET `CreatedAt` = TIMESTAMP '2026-02-25 02:44:13'
    WHERE `Id` = '00000000-0000-0000-0000-000000000011';
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260225024415_RenombrarNotaAFactura') THEN

    UPDATE `AppConfigs` SET `CreatedAt` = TIMESTAMP '2026-02-25 02:44:13'
    WHERE `Id` = '00000000-0000-0000-0000-000000000020';
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260225024415_RenombrarNotaAFactura') THEN

    UPDATE `AppConfigs` SET `CreatedAt` = TIMESTAMP '2026-02-25 02:44:13'
    WHERE `Id` = '00000000-0000-0000-0000-000000000021';
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260225024415_RenombrarNotaAFactura') THEN

    UPDATE `AppConfigs` SET `CreatedAt` = TIMESTAMP '2026-02-25 02:44:13'
    WHERE `Id` = '00000000-0000-0000-0000-000000000022';
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260225024415_RenombrarNotaAFactura') THEN

    UPDATE `AppConfigs` SET `CreatedAt` = TIMESTAMP '2026-02-25 02:44:13'
    WHERE `Id` = '00000000-0000-0000-0000-000000000023';
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260225024415_RenombrarNotaAFactura') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260225024415_RenombrarNotaAFactura', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260225041610_AgregarPagosPedido') THEN

    CREATE TABLE `PagosPedido` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `PedidoId` char(36) COLLATE ascii_general_ci NOT NULL,
        `FechaPago` datetime(6) NOT NULL,
        `Monto` decimal(18,2) NOT NULL,
        `FormaPago` int NOT NULL,
        `Referencia` varchar(100) CHARACTER SET utf8mb4 NULL,
        `Notas` varchar(500) CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_PagosPedido` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_PagosPedido_Pedidos_PedidoId` FOREIGN KEY (`PedidoId`) REFERENCES `Pedidos` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260225041610_AgregarPagosPedido') THEN

    UPDATE `AppConfigs` SET `CreatedAt` = TIMESTAMP '2026-02-25 04:16:09'
    WHERE `Id` = '00000000-0000-0000-0000-000000000001';
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260225041610_AgregarPagosPedido') THEN

    UPDATE `AppConfigs` SET `CreatedAt` = TIMESTAMP '2026-02-25 04:16:09'
    WHERE `Id` = '00000000-0000-0000-0000-000000000002';
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260225041610_AgregarPagosPedido') THEN

    UPDATE `AppConfigs` SET `CreatedAt` = TIMESTAMP '2026-02-25 04:16:09'
    WHERE `Id` = '00000000-0000-0000-0000-000000000010';
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260225041610_AgregarPagosPedido') THEN

    UPDATE `AppConfigs` SET `CreatedAt` = TIMESTAMP '2026-02-25 04:16:09'
    WHERE `Id` = '00000000-0000-0000-0000-000000000011';
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260225041610_AgregarPagosPedido') THEN

    UPDATE `AppConfigs` SET `CreatedAt` = TIMESTAMP '2026-02-25 04:16:09'
    WHERE `Id` = '00000000-0000-0000-0000-000000000020';
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260225041610_AgregarPagosPedido') THEN

    UPDATE `AppConfigs` SET `CreatedAt` = TIMESTAMP '2026-02-25 04:16:09'
    WHERE `Id` = '00000000-0000-0000-0000-000000000021';
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260225041610_AgregarPagosPedido') THEN

    UPDATE `AppConfigs` SET `CreatedAt` = TIMESTAMP '2026-02-25 04:16:09'
    WHERE `Id` = '00000000-0000-0000-0000-000000000022';
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260225041610_AgregarPagosPedido') THEN

    UPDATE `AppConfigs` SET `CreatedAt` = TIMESTAMP '2026-02-25 04:16:09'
    WHERE `Id` = '00000000-0000-0000-0000-000000000023';
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260225041610_AgregarPagosPedido') THEN

    CREATE INDEX `IX_PagosPedido_FechaPago` ON `PagosPedido` (`FechaPago`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260225041610_AgregarPagosPedido') THEN

    CREATE INDEX `IX_PagosPedido_PedidoId` ON `PagosPedido` (`PedidoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260225041610_AgregarPagosPedido') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260225041610_AgregarPagosPedido', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260225050126_AgregarTipoPrecioPedido') THEN

    ALTER TABLE `Pedidos` ADD `TipoPrecio` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260225050126_AgregarTipoPrecioPedido') THEN

    UPDATE `AppConfigs` SET `CreatedAt` = TIMESTAMP '2026-02-25 05:01:25'
    WHERE `Id` = '00000000-0000-0000-0000-000000000001';
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260225050126_AgregarTipoPrecioPedido') THEN

    UPDATE `AppConfigs` SET `CreatedAt` = TIMESTAMP '2026-02-25 05:01:25'
    WHERE `Id` = '00000000-0000-0000-0000-000000000002';
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260225050126_AgregarTipoPrecioPedido') THEN

    UPDATE `AppConfigs` SET `CreatedAt` = TIMESTAMP '2026-02-25 05:01:25'
    WHERE `Id` = '00000000-0000-0000-0000-000000000010';
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260225050126_AgregarTipoPrecioPedido') THEN

    UPDATE `AppConfigs` SET `CreatedAt` = TIMESTAMP '2026-02-25 05:01:25'
    WHERE `Id` = '00000000-0000-0000-0000-000000000011';
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260225050126_AgregarTipoPrecioPedido') THEN

    UPDATE `AppConfigs` SET `CreatedAt` = TIMESTAMP '2026-02-25 05:01:25'
    WHERE `Id` = '00000000-0000-0000-0000-000000000020';
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260225050126_AgregarTipoPrecioPedido') THEN

    UPDATE `AppConfigs` SET `CreatedAt` = TIMESTAMP '2026-02-25 05:01:25'
    WHERE `Id` = '00000000-0000-0000-0000-000000000021';
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260225050126_AgregarTipoPrecioPedido') THEN

    UPDATE `AppConfigs` SET `CreatedAt` = TIMESTAMP '2026-02-25 05:01:25'
    WHERE `Id` = '00000000-0000-0000-0000-000000000022';
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260225050126_AgregarTipoPrecioPedido') THEN

    UPDATE `AppConfigs` SET `CreatedAt` = TIMESTAMP '2026-02-25 05:01:25'
    WHERE `Id` = '00000000-0000-0000-0000-000000000023';
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260225050126_AgregarTipoPrecioPedido') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260225050126_AgregarTipoPrecioPedido', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260225050452_SyncSnapshot') THEN

    UPDATE `AppConfigs` SET `CreatedAt` = TIMESTAMP '2026-02-25 05:04:51'
    WHERE `Id` = '00000000-0000-0000-0000-000000000001';
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260225050452_SyncSnapshot') THEN

    UPDATE `AppConfigs` SET `CreatedAt` = TIMESTAMP '2026-02-25 05:04:51'
    WHERE `Id` = '00000000-0000-0000-0000-000000000002';
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260225050452_SyncSnapshot') THEN

    UPDATE `AppConfigs` SET `CreatedAt` = TIMESTAMP '2026-02-25 05:04:51'
    WHERE `Id` = '00000000-0000-0000-0000-000000000010';
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260225050452_SyncSnapshot') THEN

    UPDATE `AppConfigs` SET `CreatedAt` = TIMESTAMP '2026-02-25 05:04:51'
    WHERE `Id` = '00000000-0000-0000-0000-000000000011';
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260225050452_SyncSnapshot') THEN

    UPDATE `AppConfigs` SET `CreatedAt` = TIMESTAMP '2026-02-25 05:04:51'
    WHERE `Id` = '00000000-0000-0000-0000-000000000020';
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260225050452_SyncSnapshot') THEN

    UPDATE `AppConfigs` SET `CreatedAt` = TIMESTAMP '2026-02-25 05:04:51'
    WHERE `Id` = '00000000-0000-0000-0000-000000000021';
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260225050452_SyncSnapshot') THEN

    UPDATE `AppConfigs` SET `CreatedAt` = TIMESTAMP '2026-02-25 05:04:51'
    WHERE `Id` = '00000000-0000-0000-0000-000000000022';
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260225050452_SyncSnapshot') THEN

    UPDATE `AppConfigs` SET `CreatedAt` = TIMESTAMP '2026-02-25 05:04:51'
    WHERE `Id` = '00000000-0000-0000-0000-000000000023';
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260225050452_SyncSnapshot') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260225050452_SyncSnapshot', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260225061237_Auth') THEN

    CREATE TABLE `Capacidades` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `Clave` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `Nombre` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `Modulo` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `Descripcion` varchar(200) CHARACTER SET utf8mb4 NULL,
        CONSTRAINT `PK_Capacidades` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260225061237_Auth') THEN

    CREATE TABLE `TiposUsuario` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `Nombre` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `Descripcion` varchar(200) CHARACTER SET utf8mb4 NULL,
        `IsSystem` tinyint(1) NOT NULL,
        `IsActive` tinyint(1) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        CONSTRAINT `PK_TiposUsuario` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260225061237_Auth') THEN

    CREATE TABLE `TipoUsuarioCapacidades` (
        `TipoUsuarioId` char(36) COLLATE ascii_general_ci NOT NULL,
        `CapacidadId` char(36) COLLATE ascii_general_ci NOT NULL,
        CONSTRAINT `PK_TipoUsuarioCapacidades` PRIMARY KEY (`TipoUsuarioId`, `CapacidadId`),
        CONSTRAINT `FK_TipoUsuarioCapacidades_Capacidades_CapacidadId` FOREIGN KEY (`CapacidadId`) REFERENCES `Capacidades` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_TipoUsuarioCapacidades_TiposUsuario_TipoUsuarioId` FOREIGN KEY (`TipoUsuarioId`) REFERENCES `TiposUsuario` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260225061237_Auth') THEN

    CREATE TABLE `Usuarios` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `Email` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `NombreCompleto` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `PasswordHash` varchar(500) CHARACTER SET utf8mb4 NOT NULL,
        `TipoUsuarioId` char(36) COLLATE ascii_general_ci NOT NULL,
        `IsActive` tinyint(1) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `UltimoAcceso` datetime(6) NULL,
        CONSTRAINT `PK_Usuarios` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_Usuarios_TiposUsuario_TipoUsuarioId` FOREIGN KEY (`TipoUsuarioId`) REFERENCES `TiposUsuario` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260225061237_Auth') THEN

    UPDATE `AppConfigs` SET `CreatedAt` = TIMESTAMP '2026-02-25 06:12:36'
    WHERE `Id` = '00000000-0000-0000-0000-000000000001';
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260225061237_Auth') THEN

    UPDATE `AppConfigs` SET `CreatedAt` = TIMESTAMP '2026-02-25 06:12:36'
    WHERE `Id` = '00000000-0000-0000-0000-000000000002';
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260225061237_Auth') THEN

    UPDATE `AppConfigs` SET `CreatedAt` = TIMESTAMP '2026-02-25 06:12:36'
    WHERE `Id` = '00000000-0000-0000-0000-000000000010';
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260225061237_Auth') THEN

    UPDATE `AppConfigs` SET `CreatedAt` = TIMESTAMP '2026-02-25 06:12:36'
    WHERE `Id` = '00000000-0000-0000-0000-000000000011';
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260225061237_Auth') THEN

    UPDATE `AppConfigs` SET `CreatedAt` = TIMESTAMP '2026-02-25 06:12:36'
    WHERE `Id` = '00000000-0000-0000-0000-000000000020';
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260225061237_Auth') THEN

    UPDATE `AppConfigs` SET `CreatedAt` = TIMESTAMP '2026-02-25 06:12:36'
    WHERE `Id` = '00000000-0000-0000-0000-000000000021';
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260225061237_Auth') THEN

    UPDATE `AppConfigs` SET `CreatedAt` = TIMESTAMP '2026-02-25 06:12:36'
    WHERE `Id` = '00000000-0000-0000-0000-000000000022';
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260225061237_Auth') THEN

    UPDATE `AppConfigs` SET `CreatedAt` = TIMESTAMP '2026-02-25 06:12:36'
    WHERE `Id` = '00000000-0000-0000-0000-000000000023';
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260225061237_Auth') THEN

    CREATE UNIQUE INDEX `IX_Capacidades_Clave` ON `Capacidades` (`Clave`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260225061237_Auth') THEN

    CREATE UNIQUE INDEX `IX_TiposUsuario_Nombre` ON `TiposUsuario` (`Nombre`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260225061237_Auth') THEN

    CREATE INDEX `IX_TipoUsuarioCapacidades_CapacidadId` ON `TipoUsuarioCapacidades` (`CapacidadId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260225061237_Auth') THEN

    CREATE UNIQUE INDEX `IX_Usuarios_Email` ON `Usuarios` (`Email`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260225061237_Auth') THEN

    CREATE INDEX `IX_Usuarios_TipoUsuarioId` ON `Usuarios` (`TipoUsuarioId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260225061237_Auth') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260225061237_Auth', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304230228_MultiEmpresa') THEN

    DROP INDEX IF EXISTS `IX_TiposUsuario_Nombre` ON `TiposUsuario`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304230228_MultiEmpresa') THEN

    DROP INDEX IF EXISTS `IX_Productos_Codigo` ON `Productos`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304230228_MultiEmpresa') THEN

    DROP INDEX IF EXISTS `IX_Pantallas_Codigo` ON `Pantallas`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304230228_MultiEmpresa') THEN

    DROP INDEX IF EXISTS `IX_MateriasPrimas_Codigo` ON `MateriasPrimas`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304230228_MultiEmpresa') THEN

    DROP INDEX IF EXISTS `IX_Insumos_Codigo` ON `Insumos`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304230228_MultiEmpresa') THEN

    DROP INDEX IF EXISTS `IX_Hormas_Codigo` ON `Hormas`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304230228_MultiEmpresa') THEN

    DROP INDEX IF EXISTS `IX_Disenos_Codigo` ON `Disenos`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304230228_MultiEmpresa') THEN

    DROP INDEX IF EXISTS `IX_Clientes_Codigo` ON `Clientes`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304230228_MultiEmpresa') THEN

    DROP INDEX IF EXISTS `IX_AppConfigs_Clave` ON `AppConfigs`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304230228_MultiEmpresa') THEN

    DELETE FROM AppConfigs WHERE Id IN ('00000000-0000-0000-0000-000000000001','00000000-0000-0000-0000-000000000002','00000000-0000-0000-0000-000000000010','00000000-0000-0000-0000-000000000011','00000000-0000-0000-0000-000000000020','00000000-0000-0000-0000-000000000021','00000000-0000-0000-0000-000000000022','00000000-0000-0000-0000-000000000023');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304230228_MultiEmpresa') THEN


                        SET @exist := (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Usuarios' AND COLUMN_NAME = 'EmpresaId');
                        SET @sql := IF(@exist = 0, 'ALTER TABLE `Usuarios` ADD COLUMN `EmpresaId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL DEFAULT ''00000000-0000-0000-0000-000000000000''', 'SELECT 1');
                        PREPARE stmt FROM @sql;
                        EXECUTE stmt;
                        DEALLOCATE PREPARE stmt;
                    

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304230228_MultiEmpresa') THEN


                        SET @exist := (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'TiposUsuario' AND COLUMN_NAME = 'EmpresaId');
                        SET @sql := IF(@exist = 0, 'ALTER TABLE `TiposUsuario` ADD COLUMN `EmpresaId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL DEFAULT ''00000000-0000-0000-0000-000000000000''', 'SELECT 1');
                        PREPARE stmt FROM @sql;
                        EXECUTE stmt;
                        DEALLOCATE PREPARE stmt;
                    

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304230228_MultiEmpresa') THEN


                        SET @exist := (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'TiposProceso' AND COLUMN_NAME = 'EmpresaId');
                        SET @sql := IF(@exist = 0, 'ALTER TABLE `TiposProceso` ADD COLUMN `EmpresaId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL DEFAULT ''00000000-0000-0000-0000-000000000000''', 'SELECT 1');
                        PREPARE stmt FROM @sql;
                        EXECUTE stmt;
                        DEALLOCATE PREPARE stmt;
                    

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304230228_MultiEmpresa') THEN


                        SET @exist := (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Productos' AND COLUMN_NAME = 'EmpresaId');
                        SET @sql := IF(@exist = 0, 'ALTER TABLE `Productos` ADD COLUMN `EmpresaId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL DEFAULT ''00000000-0000-0000-0000-000000000000''', 'SELECT 1');
                        PREPARE stmt FROM @sql;
                        EXECUTE stmt;
                        DEALLOCATE PREPARE stmt;
                    

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304230228_MultiEmpresa') THEN


                        SET @exist := (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Pantallas' AND COLUMN_NAME = 'EmpresaId');
                        SET @sql := IF(@exist = 0, 'ALTER TABLE `Pantallas` ADD COLUMN `EmpresaId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL DEFAULT ''00000000-0000-0000-0000-000000000000''', 'SELECT 1');
                        PREPARE stmt FROM @sql;
                        EXECUTE stmt;
                        DEALLOCATE PREPARE stmt;
                    

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304230228_MultiEmpresa') THEN


                        SET @exist := (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'MateriasPrimas' AND COLUMN_NAME = 'EmpresaId');
                        SET @sql := IF(@exist = 0, 'ALTER TABLE `MateriasPrimas` ADD COLUMN `EmpresaId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL DEFAULT ''00000000-0000-0000-0000-000000000000''', 'SELECT 1');
                        PREPARE stmt FROM @sql;
                        EXECUTE stmt;
                        DEALLOCATE PREPARE stmt;
                    

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304230228_MultiEmpresa') THEN


                        SET @exist := (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Insumos' AND COLUMN_NAME = 'EmpresaId');
                        SET @sql := IF(@exist = 0, 'ALTER TABLE `Insumos` ADD COLUMN `EmpresaId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL DEFAULT ''00000000-0000-0000-0000-000000000000''', 'SELECT 1');
                        PREPARE stmt FROM @sql;
                        EXECUTE stmt;
                        DEALLOCATE PREPARE stmt;
                    

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304230228_MultiEmpresa') THEN


                        SET @exist := (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Hormas' AND COLUMN_NAME = 'EmpresaId');
                        SET @sql := IF(@exist = 0, 'ALTER TABLE `Hormas` ADD COLUMN `EmpresaId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL DEFAULT ''00000000-0000-0000-0000-000000000000''', 'SELECT 1');
                        PREPARE stmt FROM @sql;
                        EXECUTE stmt;
                        DEALLOCATE PREPARE stmt;
                    

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304230228_MultiEmpresa') THEN


                        SET @exist := (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'GastosFijos' AND COLUMN_NAME = 'EmpresaId');
                        SET @sql := IF(@exist = 0, 'ALTER TABLE `GastosFijos` ADD COLUMN `EmpresaId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL DEFAULT ''00000000-0000-0000-0000-000000000000''', 'SELECT 1');
                        PREPARE stmt FROM @sql;
                        EXECUTE stmt;
                        DEALLOCATE PREPARE stmt;
                    

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304230228_MultiEmpresa') THEN


                        SET @exist := (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'EscalasSerigrafia' AND COLUMN_NAME = 'EmpresaId');
                        SET @sql := IF(@exist = 0, 'ALTER TABLE `EscalasSerigrafia` ADD COLUMN `EmpresaId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL DEFAULT ''00000000-0000-0000-0000-000000000000''', 'SELECT 1');
                        PREPARE stmt FROM @sql;
                        EXECUTE stmt;
                        DEALLOCATE PREPARE stmt;
                    

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304230228_MultiEmpresa') THEN


                        SET @exist := (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Disenos' AND COLUMN_NAME = 'EmpresaId');
                        SET @sql := IF(@exist = 0, 'ALTER TABLE `Disenos` ADD COLUMN `EmpresaId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL DEFAULT ''00000000-0000-0000-0000-000000000000''', 'SELECT 1');
                        PREPARE stmt FROM @sql;
                        EXECUTE stmt;
                        DEALLOCATE PREPARE stmt;
                    

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304230228_MultiEmpresa') THEN


                        SET @exist := (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Clientes' AND COLUMN_NAME = 'EmpresaId');
                        SET @sql := IF(@exist = 0, 'ALTER TABLE `Clientes` ADD COLUMN `EmpresaId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL DEFAULT ''00000000-0000-0000-0000-000000000000''', 'SELECT 1');
                        PREPARE stmt FROM @sql;
                        EXECUTE stmt;
                        DEALLOCATE PREPARE stmt;
                    

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304230228_MultiEmpresa') THEN


                        SET @exist := (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'AppConfigs' AND COLUMN_NAME = 'EmpresaId');
                        SET @sql := IF(@exist = 0, 'ALTER TABLE `AppConfigs` ADD COLUMN `EmpresaId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL DEFAULT ''00000000-0000-0000-0000-000000000000''', 'SELECT 1');
                        PREPARE stmt FROM @sql;
                        EXECUTE stmt;
                        DEALLOCATE PREPARE stmt;
                    

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304230228_MultiEmpresa') THEN


                        SET @exist := (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'ActividadesManoObra' AND COLUMN_NAME = 'EmpresaId');
                        SET @sql := IF(@exist = 0, 'ALTER TABLE `ActividadesManoObra` ADD COLUMN `EmpresaId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL DEFAULT ''00000000-0000-0000-0000-000000000000''', 'SELECT 1');
                        PREPARE stmt FROM @sql;
                        EXECUTE stmt;
                        DEALLOCATE PREPARE stmt;
                    

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304230228_MultiEmpresa') THEN


                    CREATE TABLE IF NOT EXISTS `Empresas` (
                        `Id` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
                        `Codigo` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
                        `RazonSocial` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
                        `NombreComercial` varchar(200) CHARACTER SET utf8mb4 NULL,
                        `Rfc` varchar(20) CHARACTER SET utf8mb4 NULL,
                        `Slogan` varchar(200) CHARACTER SET utf8mb4 NULL,
                        `IsActive` tinyint(1) NOT NULL,
                        `CreatedAt` datetime(6) NOT NULL,
                        `UpdatedAt` datetime(6) NULL,
                        CONSTRAINT `PK_Empresas` PRIMARY KEY (`Id`)
                    ) CHARACTER SET=utf8mb4;
                

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304230228_MultiEmpresa') THEN

    CREATE INDEX IF NOT EXISTS `IX_Usuarios_EmpresaId` ON `Usuarios` (`EmpresaId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304230228_MultiEmpresa') THEN

    CREATE UNIQUE INDEX IF NOT EXISTS `IX_TiposUsuario_EmpresaId_Nombre` ON `TiposUsuario` (`EmpresaId`, `Nombre`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304230228_MultiEmpresa') THEN

    CREATE INDEX IF NOT EXISTS `IX_TiposProceso_EmpresaId` ON `TiposProceso` (`EmpresaId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304230228_MultiEmpresa') THEN

    CREATE UNIQUE INDEX IF NOT EXISTS `IX_Productos_EmpresaId_Codigo` ON `Productos` (`EmpresaId`, `Codigo`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304230228_MultiEmpresa') THEN

    CREATE UNIQUE INDEX IF NOT EXISTS `IX_Pantallas_EmpresaId_Codigo` ON `Pantallas` (`EmpresaId`, `Codigo`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304230228_MultiEmpresa') THEN

    CREATE UNIQUE INDEX IF NOT EXISTS `IX_MateriasPrimas_EmpresaId_Codigo` ON `MateriasPrimas` (`EmpresaId`, `Codigo`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304230228_MultiEmpresa') THEN

    CREATE UNIQUE INDEX IF NOT EXISTS `IX_Insumos_EmpresaId_Codigo` ON `Insumos` (`EmpresaId`, `Codigo`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304230228_MultiEmpresa') THEN

    CREATE UNIQUE INDEX IF NOT EXISTS `IX_Hormas_EmpresaId_Codigo` ON `Hormas` (`EmpresaId`, `Codigo`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304230228_MultiEmpresa') THEN

    CREATE INDEX IF NOT EXISTS `IX_GastosFijos_EmpresaId` ON `GastosFijos` (`EmpresaId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304230228_MultiEmpresa') THEN

    CREATE INDEX IF NOT EXISTS `IX_EscalasSerigrafia_EmpresaId` ON `EscalasSerigrafia` (`EmpresaId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304230228_MultiEmpresa') THEN

    CREATE UNIQUE INDEX IF NOT EXISTS `IX_Disenos_EmpresaId_Codigo` ON `Disenos` (`EmpresaId`, `Codigo`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304230228_MultiEmpresa') THEN

    CREATE UNIQUE INDEX IF NOT EXISTS `IX_Clientes_EmpresaId_Codigo` ON `Clientes` (`EmpresaId`, `Codigo`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304230228_MultiEmpresa') THEN

    CREATE UNIQUE INDEX IF NOT EXISTS `IX_AppConfigs_EmpresaId_Clave` ON `AppConfigs` (`EmpresaId`, `Clave`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304230228_MultiEmpresa') THEN

    CREATE INDEX IF NOT EXISTS `IX_ActividadesManoObra_EmpresaId` ON `ActividadesManoObra` (`EmpresaId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304230228_MultiEmpresa') THEN

    CREATE UNIQUE INDEX IF NOT EXISTS `IX_Empresas_Codigo` ON `Empresas` (`Codigo`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304230228_MultiEmpresa') THEN


                    INSERT IGNORE INTO Empresas (Id, Codigo, RazonSocial, NombreComercial, Slogan, IsActive, CreatedAt)
                    VALUES ('11111111-1111-1111-1111-111111111111', 'DEFAULT', 'MundoVs', 'MundoVs', 'CRM & Producción', 1, UTC_TIMESTAMP());
                

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304230228_MultiEmpresa') THEN

    UPDATE `ActividadesManoObra` SET EmpresaId = '11111111-1111-1111-1111-111111111111' WHERE EmpresaId = '00000000-0000-0000-0000-000000000000';

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304230228_MultiEmpresa') THEN

    UPDATE `AppConfigs` SET EmpresaId = '11111111-1111-1111-1111-111111111111' WHERE EmpresaId = '00000000-0000-0000-0000-000000000000';

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304230228_MultiEmpresa') THEN

    UPDATE `Clientes` SET EmpresaId = '11111111-1111-1111-1111-111111111111' WHERE EmpresaId = '00000000-0000-0000-0000-000000000000';

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304230228_MultiEmpresa') THEN

    UPDATE `Disenos` SET EmpresaId = '11111111-1111-1111-1111-111111111111' WHERE EmpresaId = '00000000-0000-0000-0000-000000000000';

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304230228_MultiEmpresa') THEN

    UPDATE `EscalasSerigrafia` SET EmpresaId = '11111111-1111-1111-1111-111111111111' WHERE EmpresaId = '00000000-0000-0000-0000-000000000000';

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304230228_MultiEmpresa') THEN

    UPDATE `GastosFijos` SET EmpresaId = '11111111-1111-1111-1111-111111111111' WHERE EmpresaId = '00000000-0000-0000-0000-000000000000';

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304230228_MultiEmpresa') THEN

    UPDATE `Hormas` SET EmpresaId = '11111111-1111-1111-1111-111111111111' WHERE EmpresaId = '00000000-0000-0000-0000-000000000000';

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304230228_MultiEmpresa') THEN

    UPDATE `Insumos` SET EmpresaId = '11111111-1111-1111-1111-111111111111' WHERE EmpresaId = '00000000-0000-0000-0000-000000000000';

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304230228_MultiEmpresa') THEN

    UPDATE `MateriasPrimas` SET EmpresaId = '11111111-1111-1111-1111-111111111111' WHERE EmpresaId = '00000000-0000-0000-0000-000000000000';

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304230228_MultiEmpresa') THEN

    UPDATE `Pantallas` SET EmpresaId = '11111111-1111-1111-1111-111111111111' WHERE EmpresaId = '00000000-0000-0000-0000-000000000000';

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304230228_MultiEmpresa') THEN

    UPDATE `Productos` SET EmpresaId = '11111111-1111-1111-1111-111111111111' WHERE EmpresaId = '00000000-0000-0000-0000-000000000000';

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304230228_MultiEmpresa') THEN

    UPDATE `TiposProceso` SET EmpresaId = '11111111-1111-1111-1111-111111111111' WHERE EmpresaId = '00000000-0000-0000-0000-000000000000';

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304230228_MultiEmpresa') THEN

    UPDATE `TiposUsuario` SET EmpresaId = '11111111-1111-1111-1111-111111111111' WHERE EmpresaId = '00000000-0000-0000-0000-000000000000';

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304230228_MultiEmpresa') THEN

    UPDATE `Usuarios` SET EmpresaId = '11111111-1111-1111-1111-111111111111' WHERE EmpresaId = '00000000-0000-0000-0000-000000000000';

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304230228_MultiEmpresa') THEN

    ALTER TABLE `ActividadesManoObra` ADD CONSTRAINT `FK_ActividadesManoObra_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `Empresas` (`Id`) ON DELETE CASCADE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304230228_MultiEmpresa') THEN

    ALTER TABLE `AppConfigs` ADD CONSTRAINT `FK_AppConfigs_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `Empresas` (`Id`) ON DELETE CASCADE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304230228_MultiEmpresa') THEN

    ALTER TABLE `Clientes` ADD CONSTRAINT `FK_Clientes_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `Empresas` (`Id`) ON DELETE CASCADE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304230228_MultiEmpresa') THEN

    ALTER TABLE `Disenos` ADD CONSTRAINT `FK_Disenos_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `Empresas` (`Id`) ON DELETE CASCADE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304230228_MultiEmpresa') THEN

    ALTER TABLE `EscalasSerigrafia` ADD CONSTRAINT `FK_EscalasSerigrafia_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `Empresas` (`Id`) ON DELETE CASCADE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304230228_MultiEmpresa') THEN

    ALTER TABLE `GastosFijos` ADD CONSTRAINT `FK_GastosFijos_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `Empresas` (`Id`) ON DELETE CASCADE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304230228_MultiEmpresa') THEN

    ALTER TABLE `Hormas` ADD CONSTRAINT `FK_Hormas_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `Empresas` (`Id`) ON DELETE CASCADE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304230228_MultiEmpresa') THEN

    ALTER TABLE `Insumos` ADD CONSTRAINT `FK_Insumos_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `Empresas` (`Id`) ON DELETE CASCADE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304230228_MultiEmpresa') THEN

    ALTER TABLE `MateriasPrimas` ADD CONSTRAINT `FK_MateriasPrimas_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `Empresas` (`Id`) ON DELETE CASCADE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304230228_MultiEmpresa') THEN

    ALTER TABLE `Pantallas` ADD CONSTRAINT `FK_Pantallas_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `Empresas` (`Id`) ON DELETE CASCADE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304230228_MultiEmpresa') THEN

    ALTER TABLE `Productos` ADD CONSTRAINT `FK_Productos_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `Empresas` (`Id`) ON DELETE CASCADE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304230228_MultiEmpresa') THEN

    ALTER TABLE `TiposProceso` ADD CONSTRAINT `FK_TiposProceso_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `Empresas` (`Id`) ON DELETE CASCADE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304230228_MultiEmpresa') THEN

    ALTER TABLE `TiposUsuario` ADD CONSTRAINT `FK_TiposUsuario_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `Empresas` (`Id`) ON DELETE CASCADE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304230228_MultiEmpresa') THEN

    ALTER TABLE `Usuarios` ADD CONSTRAINT `FK_Usuarios_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `Empresas` (`Id`) ON DELETE CASCADE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304230228_MultiEmpresa') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260304230228_MultiEmpresa', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260305060231_CxP_Empleados_Nominas') THEN

    CREATE TABLE `Empleados` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `EmpresaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Codigo` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `Nombre` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `ApellidoPaterno` varchar(100) CHARACTER SET utf8mb4 NULL,
        `ApellidoMaterno` varchar(100) CHARACTER SET utf8mb4 NULL,
        `Curp` varchar(18) CHARACTER SET utf8mb4 NULL,
        `Nss` varchar(15) CHARACTER SET utf8mb4 NULL,
        `Telefono` varchar(20) CHARACTER SET utf8mb4 NULL,
        `Email` varchar(100) CHARACTER SET utf8mb4 NULL,
        `Direccion` varchar(250) CHARACTER SET utf8mb4 NULL,
        `FechaNacimiento` datetime(6) NULL,
        `FechaContratacion` datetime(6) NULL,
        `Puesto` varchar(100) CHARACTER SET utf8mb4 NULL,
        `Departamento` varchar(100) CHARACTER SET utf8mb4 NULL,
        `SueldoSemanal` decimal(18,2) NOT NULL,
        `TipoNomina` int NOT NULL,
        `IsActive` tinyint(1) NOT NULL,
        `Notas` varchar(500) CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        CONSTRAINT `PK_Empleados` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_Empleados_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `Empresas` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260305060231_CxP_Empleados_Nominas') THEN

    CREATE TABLE `Nominas` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `EmpresaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Periodo` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `FechaInicio` datetime(6) NOT NULL,
        `FechaFin` datetime(6) NOT NULL,
        `FechaPago` datetime(6) NULL,
        `Estatus` int NOT NULL,
        `Notas` varchar(500) CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        CONSTRAINT `PK_Nominas` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_Nominas_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `Empresas` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260305060231_CxP_Empleados_Nominas') THEN

    CREATE TABLE `Proveedores` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `EmpresaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Codigo` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `Nombre` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `RazonSocial` varchar(200) CHARACTER SET utf8mb4 NULL,
        `Rfc` varchar(20) CHARACTER SET utf8mb4 NULL,
        `Telefono` varchar(20) CHARACTER SET utf8mb4 NULL,
        `Email` varchar(100) CHARACTER SET utf8mb4 NULL,
        `Direccion` varchar(250) CHARACTER SET utf8mb4 NULL,
        `Contacto` varchar(200) CHARACTER SET utf8mb4 NULL,
        `Notas` varchar(500) CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        CONSTRAINT `PK_Proveedores` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_Proveedores_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `Empresas` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260305060231_CxP_Empleados_Nominas') THEN

    CREATE TABLE `NominaDetalles` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `NominaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `EmpleadoId` char(36) COLLATE ascii_general_ci NOT NULL,
        `TipoPago` int NOT NULL,
        `SueldoBase` decimal(18,2) NOT NULL,
        `PiezasProducidas` int NOT NULL,
        `TarifaPorPieza` decimal(18,4) NOT NULL,
        `HorasExtra` decimal(18,2) NOT NULL,
        `MontoHorasExtra` decimal(18,2) NOT NULL,
        `Bonos` decimal(18,2) NOT NULL,
        `Deducciones` decimal(18,2) NOT NULL,
        `ConceptoDeducciones` varchar(500) CHARACTER SET utf8mb4 NULL,
        `Notas` varchar(500) CHARACTER SET utf8mb4 NULL,
        CONSTRAINT `PK_NominaDetalles` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_NominaDetalles_Empleados_EmpleadoId` FOREIGN KEY (`EmpleadoId`) REFERENCES `Empleados` (`Id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_NominaDetalles_Nominas_NominaId` FOREIGN KEY (`NominaId`) REFERENCES `Nominas` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260305060231_CxP_Empleados_Nominas') THEN

    CREATE TABLE `CuentasPorPagar` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `EmpresaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `ProveedorId` char(36) COLLATE ascii_general_ci NOT NULL,
        `TipoDocumento` int NOT NULL,
        `NumeroDocumento` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `Concepto` varchar(500) CHARACTER SET utf8mb4 NULL,
        `FechaEmision` datetime(6) NOT NULL,
        `FechaVencimiento` datetime(6) NULL,
        `Subtotal` decimal(18,2) NOT NULL,
        `Impuestos` decimal(18,2) NOT NULL,
        `Total` decimal(18,2) NOT NULL,
        `Estatus` int NOT NULL,
        `Notas` varchar(500) CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        CONSTRAINT `PK_CuentasPorPagar` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_CuentasPorPagar_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `Empresas` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_CuentasPorPagar_Proveedores_ProveedorId` FOREIGN KEY (`ProveedorId`) REFERENCES `Proveedores` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260305060231_CxP_Empleados_Nominas') THEN

    CREATE TABLE `PagosCxP` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `CuentaPorPagarId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Monto` decimal(18,2) NOT NULL,
        `FechaPago` datetime(6) NOT NULL,
        `MetodoPago` int NOT NULL,
        `Referencia` varchar(100) CHARACTER SET utf8mb4 NULL,
        `Notas` varchar(500) CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        CONSTRAINT `PK_PagosCxP` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_PagosCxP_CuentasPorPagar_CuentaPorPagarId` FOREIGN KEY (`CuentaPorPagarId`) REFERENCES `CuentasPorPagar` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260305060231_CxP_Empleados_Nominas') THEN

    CREATE INDEX `IX_CuentasPorPagar_EmpresaId` ON `CuentasPorPagar` (`EmpresaId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260305060231_CxP_Empleados_Nominas') THEN

    CREATE INDEX `IX_CuentasPorPagar_Estatus` ON `CuentasPorPagar` (`Estatus`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260305060231_CxP_Empleados_Nominas') THEN

    CREATE INDEX `IX_CuentasPorPagar_FechaEmision` ON `CuentasPorPagar` (`FechaEmision`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260305060231_CxP_Empleados_Nominas') THEN

    CREATE INDEX `IX_CuentasPorPagar_ProveedorId` ON `CuentasPorPagar` (`ProveedorId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260305060231_CxP_Empleados_Nominas') THEN

    CREATE INDEX `IX_Empleados_Departamento` ON `Empleados` (`Departamento`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260305060231_CxP_Empleados_Nominas') THEN

    CREATE UNIQUE INDEX `IX_Empleados_EmpresaId_Codigo` ON `Empleados` (`EmpresaId`, `Codigo`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260305060231_CxP_Empleados_Nominas') THEN

    CREATE INDEX `IX_NominaDetalles_EmpleadoId` ON `NominaDetalles` (`EmpleadoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260305060231_CxP_Empleados_Nominas') THEN

    CREATE INDEX `IX_NominaDetalles_NominaId` ON `NominaDetalles` (`NominaId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260305060231_CxP_Empleados_Nominas') THEN

    CREATE INDEX `IX_Nominas_EmpresaId` ON `Nominas` (`EmpresaId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260305060231_CxP_Empleados_Nominas') THEN

    CREATE INDEX `IX_Nominas_Estatus` ON `Nominas` (`Estatus`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260305060231_CxP_Empleados_Nominas') THEN

    CREATE INDEX `IX_Nominas_FechaInicio` ON `Nominas` (`FechaInicio`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260305060231_CxP_Empleados_Nominas') THEN

    CREATE INDEX `IX_PagosCxP_CuentaPorPagarId` ON `PagosCxP` (`CuentaPorPagarId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260305060231_CxP_Empleados_Nominas') THEN

    CREATE INDEX `IX_PagosCxP_FechaPago` ON `PagosCxP` (`FechaPago`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260305060231_CxP_Empleados_Nominas') THEN

    CREATE UNIQUE INDEX `IX_Proveedores_EmpresaId_Codigo` ON `Proveedores` (`EmpresaId`, `Codigo`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260305060231_CxP_Empleados_Nominas') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260305060231_CxP_Empleados_Nominas', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260314172940_Fase3_TenantLifecycleEmpresa') THEN

    ALTER TABLE `Empresas` ADD `ActivatedAt` datetime(6) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260314172940_Fase3_TenantLifecycleEmpresa') THEN

    ALTER TABLE `Empresas` ADD `Estado` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260314172940_Fase3_TenantLifecycleEmpresa') THEN

    ALTER TABLE `Empresas` ADD `IsSuspended` tinyint(1) NOT NULL DEFAULT FALSE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260314172940_Fase3_TenantLifecycleEmpresa') THEN

    ALTER TABLE `Empresas` ADD `MaxUsuarios` int NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260314172940_Fase3_TenantLifecycleEmpresa') THEN

    ALTER TABLE `Empresas` ADD `PlanActualId` char(36) COLLATE ascii_general_ci NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260314172940_Fase3_TenantLifecycleEmpresa') THEN

    ALTER TABLE `Empresas` ADD `TrialEndsAt` datetime(6) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260314172940_Fase3_TenantLifecycleEmpresa') THEN

    UPDATE `Empresas` SET `Estado` = CASE WHEN `IsActive` = 1 THEN 1 ELSE 2 END, `IsSuspended` = CASE WHEN `IsActive` = 1 THEN 0 ELSE 1 END, `ActivatedAt` = CASE WHEN `IsActive` = 1 THEN COALESCE(`ActivatedAt`, `CreatedAt`, UTC_TIMESTAMP()) ELSE `ActivatedAt` END;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260314172940_Fase3_TenantLifecycleEmpresa') THEN

    CREATE INDEX `IX_Empresas_Estado` ON `Empresas` (`Estado`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260314172940_Fase3_TenantLifecycleEmpresa') THEN

    CREATE INDEX `IX_Empresas_IsSuspended` ON `Empresas` (`IsSuspended`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260314172940_Fase3_TenantLifecycleEmpresa') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260314172940_Fase3_TenantLifecycleEmpresa', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260314175945_Fase4_OnboardingPasswordInicial') THEN

    ALTER TABLE `Usuarios` ADD `RequiereCambioPassword` tinyint(1) NOT NULL DEFAULT FALSE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260314175945_Fase4_OnboardingPasswordInicial') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260314175945_Fase4_OnboardingPasswordInicial', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260314181755_Fase5_PlanesSuscripciones') THEN

    CREATE TABLE `Planes` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `Nombre` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `Descripcion` varchar(500) CHARACTER SET utf8mb4 NULL,
        `PrecioMensual` decimal(18,2) NOT NULL,
        `PrecioAnual` decimal(18,2) NOT NULL,
        `LimiteUsuarios` int NULL,
        `ModulosIncluidos` varchar(500) CHARACTER SET utf8mb4 NULL,
        `TrialDays` int NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_Planes` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260314181755_Fase5_PlanesSuscripciones') THEN

    CREATE TABLE `SuscripcionesEmpresa` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `EmpresaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `PlanId` char(36) COLLATE ascii_general_ci NOT NULL,
        `FechaInicio` datetime(6) NOT NULL,
        `FechaFin` datetime(6) NULL,
        `Estado` int NOT NULL,
        `Periodicidad` int NOT NULL,
        `RenovacionAutomatica` tinyint(1) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_SuscripcionesEmpresa` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_SuscripcionesEmpresa_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `Empresas` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_SuscripcionesEmpresa_Planes_PlanId` FOREIGN KEY (`PlanId`) REFERENCES `Planes` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260314181755_Fase5_PlanesSuscripciones') THEN

    CREATE TABLE `PagosSuscripcion` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `SuscripcionEmpresaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Monto` decimal(18,2) NOT NULL,
        `FechaPago` datetime(6) NOT NULL,
        `MetodoPago` int NOT NULL,
        `Referencia` varchar(100) CHARACTER SET utf8mb4 NULL,
        `Notas` varchar(500) CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_PagosSuscripcion` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_PagosSuscripcion_SuscripcionesEmpresa_SuscripcionEmpresaId` FOREIGN KEY (`SuscripcionEmpresaId`) REFERENCES `SuscripcionesEmpresa` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260314181755_Fase5_PlanesSuscripciones') THEN

    CREATE INDEX `IX_Empresas_PlanActualId` ON `Empresas` (`PlanActualId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260314181755_Fase5_PlanesSuscripciones') THEN

    CREATE INDEX `IX_PagosSuscripcion_FechaPago` ON `PagosSuscripcion` (`FechaPago`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260314181755_Fase5_PlanesSuscripciones') THEN

    CREATE INDEX `IX_PagosSuscripcion_SuscripcionEmpresaId` ON `PagosSuscripcion` (`SuscripcionEmpresaId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260314181755_Fase5_PlanesSuscripciones') THEN

    CREATE UNIQUE INDEX `IX_Planes_Nombre` ON `Planes` (`Nombre`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260314181755_Fase5_PlanesSuscripciones') THEN

    CREATE INDEX `IX_SuscripcionesEmpresa_EmpresaId` ON `SuscripcionesEmpresa` (`EmpresaId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260314181755_Fase5_PlanesSuscripciones') THEN

    CREATE INDEX `IX_SuscripcionesEmpresa_Estado` ON `SuscripcionesEmpresa` (`Estado`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260314181755_Fase5_PlanesSuscripciones') THEN

    CREATE INDEX `IX_SuscripcionesEmpresa_PlanId` ON `SuscripcionesEmpresa` (`PlanId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260314181755_Fase5_PlanesSuscripciones') THEN

    ALTER TABLE `Empresas` ADD CONSTRAINT `FK_Empresas_Planes_PlanActualId` FOREIGN KEY (`PlanActualId`) REFERENCES `Planes` (`Id`) ON DELETE SET NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260314181755_Fase5_PlanesSuscripciones') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260314181755_Fase5_PlanesSuscripciones', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260314191137_Fase6_Auditoria') THEN

    CREATE TABLE `AuditLogs` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `EmpresaId` char(36) COLLATE ascii_general_ci NULL,
        `UsuarioId` char(36) COLLATE ascii_general_ci NULL,
        `Accion` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `Entidad` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `EntidadId` char(36) COLLATE ascii_general_ci NULL,
        `Detalle` varchar(2000) CHARACTER SET utf8mb4 NULL,
        `Fecha` datetime(6) NOT NULL,
        `IpAddress` varchar(100) CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_AuditLogs` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260314191137_Fase6_Auditoria') THEN

    CREATE INDEX `IX_AuditLogs_EmpresaId` ON `AuditLogs` (`EmpresaId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260314191137_Fase6_Auditoria') THEN

    CREATE INDEX `IX_AuditLogs_Entidad` ON `AuditLogs` (`Entidad`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260314191137_Fase6_Auditoria') THEN

    CREATE INDEX `IX_AuditLogs_Fecha` ON `AuditLogs` (`Fecha`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260314191137_Fase6_Auditoria') THEN

    CREATE INDEX `IX_AuditLogs_UsuarioId` ON `AuditLogs` (`UsuarioId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260314191137_Fase6_Auditoria') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260314191137_Fase6_Auditoria', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260314192147_Fase7_SeguridadAcceso') THEN

    ALTER TABLE `Usuarios` ADD `BloqueadoHastaUtc` datetime(6) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260314192147_Fase7_SeguridadAcceso') THEN

    ALTER TABLE `Usuarios` ADD `IntentosFallidos` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260314192147_Fase7_SeguridadAcceso') THEN

    ALTER TABLE `Usuarios` ADD `PasswordChangedAt` datetime(6) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260314192147_Fase7_SeguridadAcceso') THEN

    ALTER TABLE `Usuarios` ADD `PasswordResetTokenExpiresAt` datetime(6) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260314192147_Fase7_SeguridadAcceso') THEN

    ALTER TABLE `Usuarios` ADD `PasswordResetTokenHash` varchar(200) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260314192147_Fase7_SeguridadAcceso') THEN

    CREATE INDEX `IX_Usuarios_BloqueadoHastaUtc` ON `Usuarios` (`BloqueadoHastaUtc`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260314192147_Fase7_SeguridadAcceso') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260314192147_Fase7_SeguridadAcceso', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260315162538_Fase1_CategoriasYTiposInventario') THEN

    CREATE TABLE `CategoriasInventario` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `EmpresaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Codigo` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `Nombre` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `Orden` int NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_CategoriasInventario` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_CategoriasInventario_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `Empresas` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260315162538_Fase1_CategoriasYTiposInventario') THEN

    CREATE TABLE `TiposInventario` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `EmpresaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `CategoriaInventarioId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Codigo` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `Nombre` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `Color` varchar(20) CHARACTER SET utf8mb4 NULL,
        `Orden` int NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_TiposInventario` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_TiposInventario_CategoriasInventario_CategoriaInventarioId` FOREIGN KEY (`CategoriaInventarioId`) REFERENCES `CategoriasInventario` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_TiposInventario_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `Empresas` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260315162538_Fase1_CategoriasYTiposInventario') THEN

    CREATE UNIQUE INDEX `IX_CategoriasInventario_EmpresaId_Codigo` ON `CategoriasInventario` (`EmpresaId`, `Codigo`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260315162538_Fase1_CategoriasYTiposInventario') THEN

    CREATE UNIQUE INDEX `IX_CategoriasInventario_EmpresaId_Nombre` ON `CategoriasInventario` (`EmpresaId`, `Nombre`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260315162538_Fase1_CategoriasYTiposInventario') THEN

    CREATE INDEX `IX_CategoriasInventario_Orden` ON `CategoriasInventario` (`Orden`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260315162538_Fase1_CategoriasYTiposInventario') THEN

    CREATE INDEX `IX_TiposInventario_CategoriaInventarioId` ON `TiposInventario` (`CategoriaInventarioId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260315162538_Fase1_CategoriasYTiposInventario') THEN

    CREATE UNIQUE INDEX `IX_TiposInventario_EmpresaId_CategoriaInventarioId_Codigo` ON `TiposInventario` (`EmpresaId`, `CategoriaInventarioId`, `Codigo`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260315162538_Fase1_CategoriasYTiposInventario') THEN

    CREATE UNIQUE INDEX `IX_TiposInventario_EmpresaId_CategoriaInventarioId_Nombre` ON `TiposInventario` (`EmpresaId`, `CategoriaInventarioId`, `Nombre`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260315162538_Fase1_CategoriasYTiposInventario') THEN

    CREATE INDEX `IX_TiposInventario_Orden` ON `TiposInventario` (`Orden`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260315162538_Fase1_CategoriasYTiposInventario') THEN


    INSERT INTO CategoriasInventario (Id, EmpresaId, Codigo, Nombre, Orden, CreatedAt, IsActive)
    SELECT UUID(), e.Id, 'MATERIA_PRIMA', 'Materia Prima', 1, UTC_TIMESTAMP(6), 1
    FROM Empresas e
    WHERE NOT EXISTS (
        SELECT 1
        FROM CategoriasInventario c
        WHERE c.EmpresaId = e.Id AND c.Codigo = 'MATERIA_PRIMA'
    );

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260315162538_Fase1_CategoriasYTiposInventario') THEN


    INSERT INTO CategoriasInventario (Id, EmpresaId, Codigo, Nombre, Orden, CreatedAt, IsActive)
    SELECT UUID(), e.Id, 'INSUMO', 'Insumo', 2, UTC_TIMESTAMP(6), 1
    FROM Empresas e
    WHERE NOT EXISTS (
        SELECT 1
        FROM CategoriasInventario c
        WHERE c.EmpresaId = e.Id AND c.Codigo = 'INSUMO'
    );

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260315162538_Fase1_CategoriasYTiposInventario') THEN


    INSERT INTO TiposInventario (Id, EmpresaId, CategoriaInventarioId, Codigo, Nombre, Color, Orden, CreatedAt, IsActive)
    SELECT UUID(), c.EmpresaId, c.Id, 'TINTA', 'Tinta', '#0dcaf0', 1, UTC_TIMESTAMP(6), 1
    FROM CategoriasInventario c
    WHERE c.Codigo = 'MATERIA_PRIMA'
    AND NOT EXISTS (
        SELECT 1
        FROM TiposInventario t
        WHERE t.EmpresaId = c.EmpresaId AND t.CategoriaInventarioId = c.Id AND t.Codigo = 'TINTA'
    );

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260315162538_Fase1_CategoriasYTiposInventario') THEN


    INSERT INTO TiposInventario (Id, EmpresaId, CategoriaInventarioId, Codigo, Nombre, Color, Orden, CreatedAt, IsActive)
    SELECT UUID(), c.EmpresaId, c.Id, 'BASE', 'Base', '#ffc107', 2, UTC_TIMESTAMP(6), 1
    FROM CategoriasInventario c
    WHERE c.Codigo = 'MATERIA_PRIMA'
    AND NOT EXISTS (
        SELECT 1
        FROM TiposInventario t
        WHERE t.EmpresaId = c.EmpresaId AND t.CategoriaInventarioId = c.Id AND t.Codigo = 'BASE'
    );

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260315162538_Fase1_CategoriasYTiposInventario') THEN


    INSERT INTO TiposInventario (Id, EmpresaId, CategoriaInventarioId, Codigo, Nombre, Color, Orden, CreatedAt, IsActive)
    SELECT UUID(), c.EmpresaId, c.Id, 'SOLVENTE', 'Solvente', '#6c757d', 3, UTC_TIMESTAMP(6), 1
    FROM CategoriasInventario c
    WHERE c.Codigo = 'MATERIA_PRIMA'
    AND NOT EXISTS (
        SELECT 1
        FROM TiposInventario t
        WHERE t.EmpresaId = c.EmpresaId AND t.CategoriaInventarioId = c.Id AND t.Codigo = 'SOLVENTE'
    );

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260315162538_Fase1_CategoriasYTiposInventario') THEN


    INSERT INTO TiposInventario (Id, EmpresaId, CategoriaInventarioId, Codigo, Nombre, Color, Orden, CreatedAt, IsActive)
    SELECT UUID(), c.EmpresaId, c.Id, 'ADITIVO', 'Aditivo', '#0d6efd', 4, UTC_TIMESTAMP(6), 1
    FROM CategoriasInventario c
    WHERE c.Codigo = 'MATERIA_PRIMA'
    AND NOT EXISTS (
        SELECT 1
        FROM TiposInventario t
        WHERE t.EmpresaId = c.EmpresaId AND t.CategoriaInventarioId = c.Id AND t.Codigo = 'ADITIVO'
    );

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260315162538_Fase1_CategoriasYTiposInventario') THEN


    INSERT INTO TiposInventario (Id, EmpresaId, CategoriaInventarioId, Codigo, Nombre, Color, Orden, CreatedAt, IsActive)
    SELECT UUID(), c.EmpresaId, c.Id, 'OTRO', 'Otro', '#212529', 5, UTC_TIMESTAMP(6), 1
    FROM CategoriasInventario c
    WHERE c.Codigo = 'MATERIA_PRIMA'
    AND NOT EXISTS (
        SELECT 1
        FROM TiposInventario t
        WHERE t.EmpresaId = c.EmpresaId AND t.CategoriaInventarioId = c.Id AND t.Codigo = 'OTRO'
    );

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260315162538_Fase1_CategoriasYTiposInventario') THEN


    INSERT INTO TiposInventario (Id, EmpresaId, CategoriaInventarioId, Codigo, Nombre, Color, Orden, CreatedAt, IsActive)
    SELECT UUID(), c.EmpresaId, c.Id, 'BASICO', 'Básico', '#198754', 1, UTC_TIMESTAMP(6), 1
    FROM CategoriasInventario c
    WHERE c.Codigo = 'INSUMO'
    AND NOT EXISTS (
        SELECT 1
        FROM TiposInventario t
        WHERE t.EmpresaId = c.EmpresaId AND t.CategoriaInventarioId = c.Id AND t.Codigo = 'BASICO'
    );

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260315162538_Fase1_CategoriasYTiposInventario') THEN


    INSERT INTO TiposInventario (Id, EmpresaId, CategoriaInventarioId, Codigo, Nombre, Color, Orden, CreatedAt, IsActive)
    SELECT UUID(), c.EmpresaId, c.Id, 'DIVERSO', 'Diverso', '#6610f2', 2, UTC_TIMESTAMP(6), 1
    FROM CategoriasInventario c
    WHERE c.Codigo = 'INSUMO'
    AND NOT EXISTS (
        SELECT 1
        FROM TiposInventario t
        WHERE t.EmpresaId = c.EmpresaId AND t.CategoriaInventarioId = c.Id AND t.Codigo = 'DIVERSO'
    );

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260315162538_Fase1_CategoriasYTiposInventario') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260315162538_Fase1_CategoriasYTiposInventario', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260315163116_Fase2_MateriaPrimaTipoInventario') THEN

    ALTER TABLE `MateriasPrimas` ADD `TipoInventarioId` char(36) COLLATE ascii_general_ci NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260315163116_Fase2_MateriaPrimaTipoInventario') THEN

    CREATE INDEX `IX_MateriasPrimas_TipoInventarioId` ON `MateriasPrimas` (`TipoInventarioId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260315163116_Fase2_MateriaPrimaTipoInventario') THEN

    ALTER TABLE `MateriasPrimas` ADD CONSTRAINT `FK_MateriasPrimas_TiposInventario_TipoInventarioId` FOREIGN KEY (`TipoInventarioId`) REFERENCES `TiposInventario` (`Id`) ON DELETE RESTRICT;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260315163116_Fase2_MateriaPrimaTipoInventario') THEN


    UPDATE MateriasPrimas mp
    JOIN CategoriasInventario ci
        ON ci.EmpresaId = mp.EmpresaId
       AND ci.Codigo = 'MATERIA_PRIMA'
    JOIN TiposInventario ti
        ON ti.EmpresaId = mp.EmpresaId
       AND ti.CategoriaInventarioId = ci.Id
       AND (
            (mp.TipoMateriaPrima = 1 AND ti.Codigo = 'TINTA') OR
            (mp.TipoMateriaPrima = 2 AND ti.Codigo = 'BASE') OR
            (mp.TipoMateriaPrima = 3 AND ti.Codigo = 'SOLVENTE') OR
            (mp.TipoMateriaPrima = 4 AND ti.Codigo = 'ADITIVO') OR
            (mp.TipoMateriaPrima = 5 AND ti.Codigo = 'OTRO')
       )
    SET mp.TipoInventarioId = ti.Id
    WHERE mp.TipoInventarioId IS NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260315163116_Fase2_MateriaPrimaTipoInventario') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260315163116_Fase2_MateriaPrimaTipoInventario', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260315164407_Fase3_QuitarColorTipoInventario') THEN

    ALTER TABLE `TiposInventario` DROP COLUMN `Color`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260315164407_Fase3_QuitarColorTipoInventario') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260315164407_Fase3_QuitarColorTipoInventario', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260315164922_Fase4_InsumoTipoInventario') THEN

    ALTER TABLE `Insumos` ADD `TipoInventarioId` char(36) COLLATE ascii_general_ci NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260315164922_Fase4_InsumoTipoInventario') THEN

    CREATE INDEX `IX_Insumos_TipoInventarioId` ON `Insumos` (`TipoInventarioId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260315164922_Fase4_InsumoTipoInventario') THEN

    ALTER TABLE `Insumos` ADD CONSTRAINT `FK_Insumos_TiposInventario_TipoInventarioId` FOREIGN KEY (`TipoInventarioId`) REFERENCES `TiposInventario` (`Id`) ON DELETE RESTRICT;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260315164922_Fase4_InsumoTipoInventario') THEN


    UPDATE Insumos i
    JOIN CategoriasInventario ci
        ON ci.EmpresaId = i.EmpresaId
       AND ci.Codigo = 'INSUMO'
    JOIN TiposInventario ti
        ON ti.EmpresaId = i.EmpresaId
       AND ti.CategoriaInventarioId = ci.Id
       AND (
            (i.TipoInsumo = 1 AND ti.Codigo = 'BASICO') OR
            (i.TipoInsumo = 2 AND ti.Codigo = 'DIVERSO')
       )
    SET i.TipoInventarioId = ti.Id
    WHERE i.TipoInventarioId IS NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260315164922_Fase4_InsumoTipoInventario') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260315164922_Fase4_InsumoTipoInventario', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260315173146_Fase5_MovimientosInventarioYCapacidades') THEN

    CREATE TABLE `MovimientosInventario` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `EmpresaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Origen` int NOT NULL,
        `TipoMovimiento` int NOT NULL,
        `MateriaPrimaId` char(36) COLLATE ascii_general_ci NULL,
        `InsumoId` char(36) COLLATE ascii_general_ci NULL,
        `Cantidad` decimal(18,2) NOT NULL,
        `ExistenciaAnterior` decimal(18,2) NOT NULL,
        `ExistenciaNueva` decimal(18,2) NOT NULL,
        `CostoUnitario` decimal(18,4) NOT NULL,
        `FechaMovimiento` datetime(6) NOT NULL,
        `Referencia` varchar(100) CHARACTER SET utf8mb4 NULL,
        `Observaciones` varchar(500) CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_MovimientosInventario` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_MovimientosInventario_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `Empresas` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_MovimientosInventario_Insumos_InsumoId` FOREIGN KEY (`InsumoId`) REFERENCES `Insumos` (`Id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_MovimientosInventario_MateriasPrimas_MateriaPrimaId` FOREIGN KEY (`MateriaPrimaId`) REFERENCES `MateriasPrimas` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260315173146_Fase5_MovimientosInventarioYCapacidades') THEN

    CREATE INDEX `IX_MovimientosInventario_EmpresaId` ON `MovimientosInventario` (`EmpresaId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260315173146_Fase5_MovimientosInventarioYCapacidades') THEN

    CREATE INDEX `IX_MovimientosInventario_FechaMovimiento` ON `MovimientosInventario` (`FechaMovimiento`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260315173146_Fase5_MovimientosInventarioYCapacidades') THEN

    CREATE INDEX `IX_MovimientosInventario_InsumoId` ON `MovimientosInventario` (`InsumoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260315173146_Fase5_MovimientosInventarioYCapacidades') THEN

    CREATE INDEX `IX_MovimientosInventario_MateriaPrimaId` ON `MovimientosInventario` (`MateriaPrimaId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260315173146_Fase5_MovimientosInventarioYCapacidades') THEN

    CREATE INDEX `IX_MovimientosInventario_Origen` ON `MovimientosInventario` (`Origen`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260315173146_Fase5_MovimientosInventarioYCapacidades') THEN

    CREATE INDEX `IX_MovimientosInventario_TipoMovimiento` ON `MovimientosInventario` (`TipoMovimiento`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260315173146_Fase5_MovimientosInventarioYCapacidades') THEN


    INSERT INTO Capacidades (Id, Clave, Nombre, Modulo, Descripcion)
    SELECT UUID(), 'inventario.ver', 'Ver inventario', 'Inventario', 'Acceso al módulo unificado de inventario'
    WHERE NOT EXISTS (SELECT 1 FROM Capacidades WHERE Clave = 'inventario.ver');

    INSERT INTO Capacidades (Id, Clave, Nombre, Modulo, Descripcion)
    SELECT UUID(), 'inventario.editar', 'Editar inventario', 'Inventario', 'Alta rápida de materias primas e insumos desde inventario'
    WHERE NOT EXISTS (SELECT 1 FROM Capacidades WHERE Clave = 'inventario.editar');

    INSERT INTO Capacidades (Id, Clave, Nombre, Modulo, Descripcion)
    SELECT UUID(), 'inventario.movimientos', 'Registrar movimientos', 'Inventario', 'Entradas, salidas y ajustes de inventario'
    WHERE NOT EXISTS (SELECT 1 FROM Capacidades WHERE Clave = 'inventario.movimientos');

    INSERT INTO Capacidades (Id, Clave, Nombre, Modulo, Descripcion)
    SELECT UUID(), 'inventario.reportes', 'Ver reportes de inventario', 'Inventario', 'Consulta de movimientos y costo de inventario'
    WHERE NOT EXISTS (SELECT 1 FROM Capacidades WHERE Clave = 'inventario.reportes');

    INSERT INTO TipoUsuarioCapacidades (TipoUsuarioId, CapacidadId)
    SELECT tu.Id, c.Id
    FROM TiposUsuario tu
    JOIN Capacidades c ON c.Clave IN ('inventario.ver', 'inventario.editar', 'inventario.movimientos', 'inventario.reportes')
    WHERE tu.Nombre = 'Administrador'
    AND NOT EXISTS (
        SELECT 1 FROM TipoUsuarioCapacidades tuc
        WHERE tuc.TipoUsuarioId = tu.Id AND tuc.CapacidadId = c.Id
    );

    INSERT INTO TipoUsuarioCapacidades (TipoUsuarioId, CapacidadId)
    SELECT tu.Id, c.Id
    FROM TiposUsuario tu
    JOIN Capacidades c ON c.Clave IN ('inventario.ver', 'inventario.reportes')
    WHERE tu.Nombre = 'Gerente'
    AND NOT EXISTS (
        SELECT 1 FROM TipoUsuarioCapacidades tuc
        WHERE tuc.TipoUsuarioId = tu.Id AND tuc.CapacidadId = c.Id
    );

    INSERT INTO TipoUsuarioCapacidades (TipoUsuarioId, CapacidadId)
    SELECT tu.Id, c.Id
    FROM TiposUsuario tu
    JOIN Capacidades c ON c.Clave IN ('inventario.ver', 'inventario.editar', 'inventario.movimientos', 'inventario.reportes')
    WHERE tu.Nombre = 'Producción'
    AND NOT EXISTS (
        SELECT 1 FROM TipoUsuarioCapacidades tuc
        WHERE tuc.TipoUsuarioId = tu.Id AND tuc.CapacidadId = c.Id
    );

    INSERT INTO TipoUsuarioCapacidades (TipoUsuarioId, CapacidadId)
    SELECT tu.Id, c.Id
    FROM TiposUsuario tu
    JOIN Capacidades c ON c.Clave IN ('inventario.ver', 'inventario.reportes')
    WHERE tu.Nombre = 'Contador'
    AND NOT EXISTS (
        SELECT 1 FROM TipoUsuarioCapacidades tuc
        WHERE tuc.TipoUsuarioId = tu.Id AND tuc.CapacidadId = c.Id
    );

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260315173146_Fase5_MovimientosInventarioYCapacidades') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260315173146_Fase5_MovimientosInventarioYCapacidades', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260315194300_PedidoConceptosYEmpleadoSeguimiento') THEN

    ALTER TABLE `PedidoSerigrafiaTallaProcesos` ADD `EmpleadoId` char(36) COLLATE ascii_general_ci NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260315194300_PedidoConceptosYEmpleadoSeguimiento') THEN

    CREATE TABLE `PedidoConceptos` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `PedidoId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Tipo` int NOT NULL,
        `Descripcion` varchar(500) CHARACTER SET utf8mb4 NOT NULL,
        `Cantidad` int NOT NULL,
        `PrecioUnitario` decimal(18,2) NOT NULL,
        `Total` decimal(18,2) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_PedidoConceptos` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_PedidoConceptos_Pedidos_PedidoId` FOREIGN KEY (`PedidoId`) REFERENCES `Pedidos` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260315194300_PedidoConceptosYEmpleadoSeguimiento') THEN

    CREATE INDEX `IX_PedidoSerigrafiaTallaProcesos_EmpleadoId` ON `PedidoSerigrafiaTallaProcesos` (`EmpleadoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260315194300_PedidoConceptosYEmpleadoSeguimiento') THEN

    CREATE INDEX `IX_PedidoConceptos_PedidoId` ON `PedidoConceptos` (`PedidoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260315194300_PedidoConceptosYEmpleadoSeguimiento') THEN

    CREATE INDEX `IX_PedidoConceptos_Tipo` ON `PedidoConceptos` (`Tipo`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260315194300_PedidoConceptosYEmpleadoSeguimiento') THEN

    ALTER TABLE `PedidoSerigrafiaTallaProcesos` ADD CONSTRAINT `FK_PedidoSerigrafiaTallaProcesos_Empleados_EmpleadoId` FOREIGN KEY (`EmpleadoId`) REFERENCES `Empleados` (`Id`) ON DELETE RESTRICT;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260315194300_PedidoConceptosYEmpleadoSeguimiento') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260315194300_PedidoConceptosYEmpleadoSeguimiento', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260315195550_PedidoConceptoCompletado') THEN

    ALTER TABLE `PedidoConceptos` ADD `Completado` tinyint(1) NOT NULL DEFAULT FALSE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260315195550_PedidoConceptoCompletado') THEN

    ALTER TABLE `PedidoConceptos` ADD `FechaCompletado` datetime(6) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260315195550_PedidoConceptoCompletado') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260315195550_PedidoConceptoCompletado', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260315210254_RegistroDestajoProceso') THEN

    ALTER TABLE `TiposProceso` ADD `TarifaDestajo` decimal(18,4) NOT NULL DEFAULT 0.0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260315210254_RegistroDestajoProceso') THEN

    CREATE TABLE `RegistrosDestajoProceso` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `PedidoSerigrafiaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `PedidoSerigrafiaTallaProcesoId` char(36) COLLATE ascii_general_ci NOT NULL,
        `PedidoSerigrafiaTallaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `TipoProcesoId` char(36) COLLATE ascii_general_ci NOT NULL,
        `EmpleadoId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Fecha` datetime(6) NOT NULL,
        `CantidadProcesada` int NOT NULL,
        `TarifaUnitario` decimal(18,4) NOT NULL,
        `Importe` decimal(18,2) NOT NULL,
        `TiempoMinutos` int NULL,
        `Observaciones` varchar(500) CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_RegistrosDestajoProceso` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_RegistrosDestajoProceso_Empleados_EmpleadoId` FOREIGN KEY (`EmpleadoId`) REFERENCES `Empleados` (`Id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_RegistrosDestajoProceso_PedidoSerigrafiaTallaProcesos_Pedido~` FOREIGN KEY (`PedidoSerigrafiaTallaProcesoId`) REFERENCES `PedidoSerigrafiaTallaProcesos` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_RegistrosDestajoProceso_PedidoSerigrafiaTallas_PedidoSerigra~` FOREIGN KEY (`PedidoSerigrafiaTallaId`) REFERENCES `PedidoSerigrafiaTallas` (`Id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_RegistrosDestajoProceso_PedidosSerigrafia_PedidoSerigrafiaId` FOREIGN KEY (`PedidoSerigrafiaId`) REFERENCES `PedidosSerigrafia` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_RegistrosDestajoProceso_TiposProceso_TipoProcesoId` FOREIGN KEY (`TipoProcesoId`) REFERENCES `TiposProceso` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260315210254_RegistroDestajoProceso') THEN

    CREATE INDEX `IX_RegistrosDestajoProceso_EmpleadoId` ON `RegistrosDestajoProceso` (`EmpleadoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260315210254_RegistroDestajoProceso') THEN

    CREATE INDEX `IX_RegistrosDestajoProceso_Fecha` ON `RegistrosDestajoProceso` (`Fecha`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260315210254_RegistroDestajoProceso') THEN

    CREATE INDEX `IX_RegistrosDestajoProceso_PedidoSerigrafiaId` ON `RegistrosDestajoProceso` (`PedidoSerigrafiaId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260315210254_RegistroDestajoProceso') THEN

    CREATE INDEX `IX_RegistrosDestajoProceso_PedidoSerigrafiaTallaId` ON `RegistrosDestajoProceso` (`PedidoSerigrafiaTallaId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260315210254_RegistroDestajoProceso') THEN

    CREATE UNIQUE INDEX `IX_RegistrosDestajoProceso_PedidoSerigrafiaTallaProcesoId` ON `RegistrosDestajoProceso` (`PedidoSerigrafiaTallaProcesoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260315210254_RegistroDestajoProceso') THEN

    CREATE INDEX `IX_RegistrosDestajoProceso_TipoProcesoId` ON `RegistrosDestajoProceso` (`TipoProcesoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260315210254_RegistroDestajoProceso') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260315210254_RegistroDestajoProceso', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260315213238_TipoProcesoActividadMo') THEN

    ALTER TABLE `TiposProceso` ADD `ActividadManoObraId` char(36) COLLATE ascii_general_ci NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260315213238_TipoProcesoActividadMo') THEN

    ALTER TABLE `TiposProceso` ADD `MinutosEstandar` decimal(18,2) NOT NULL DEFAULT 0.0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260315213238_TipoProcesoActividadMo') THEN

    ALTER TABLE `TiposProceso` ADD `TarifaPorMinuto` decimal(18,4) NOT NULL DEFAULT 0.0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260315213238_TipoProcesoActividadMo') THEN

    CREATE INDEX `IX_TiposProceso_ActividadManoObraId` ON `TiposProceso` (`ActividadManoObraId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260315213238_TipoProcesoActividadMo') THEN

    ALTER TABLE `TiposProceso` ADD CONSTRAINT `FK_TiposProceso_ActividadesManoObra_ActividadManoObraId` FOREIGN KEY (`ActividadManoObraId`) REFERENCES `ActividadesManoObra` (`Id`) ON DELETE SET NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260315213238_TipoProcesoActividadMo') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260315213238_TipoProcesoActividadMo', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260315213943_CotizacionProcesosAutoPedido') THEN

    CREATE TABLE `CotizacionSerigrafiaProcesos` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `CotizacionSerigrafiaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `TipoProcesoId` char(36) COLLATE ascii_general_ci NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_CotizacionSerigrafiaProcesos` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_CotizacionSerigrafiaProcesos_CotizacionesSerigrafia_Cotizaci~` FOREIGN KEY (`CotizacionSerigrafiaId`) REFERENCES `CotizacionesSerigrafia` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_CotizacionSerigrafiaProcesos_TiposProceso_TipoProcesoId` FOREIGN KEY (`TipoProcesoId`) REFERENCES `TiposProceso` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260315213943_CotizacionProcesosAutoPedido') THEN

    CREATE INDEX `IX_CotizacionSerigrafiaProcesos_CotizacionSerigrafiaId` ON `CotizacionSerigrafiaProcesos` (`CotizacionSerigrafiaId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260315213943_CotizacionProcesosAutoPedido') THEN

    CREATE UNIQUE INDEX `IX_CotizacionSerigrafiaProcesos_CotizacionSerigrafiaId_TipoProc~` ON `CotizacionSerigrafiaProcesos` (`CotizacionSerigrafiaId`, `TipoProcesoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260315213943_CotizacionProcesosAutoPedido') THEN

    CREATE INDEX `IX_CotizacionSerigrafiaProcesos_TipoProcesoId` ON `CotizacionSerigrafiaProcesos` (`TipoProcesoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260315213943_CotizacionProcesosAutoPedido') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260315213943_CotizacionProcesosAutoPedido', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260315214451_CotizacionManoObraPorProceso') THEN

    ALTER TABLE `CotizacionDetalles` ADD `TipoProcesoId` char(36) COLLATE ascii_general_ci NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260315214451_CotizacionManoObraPorProceso') THEN

    CREATE INDEX `IX_CotizacionDetalles_TipoProcesoId` ON `CotizacionDetalles` (`TipoProcesoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260315214451_CotizacionManoObraPorProceso') THEN

    ALTER TABLE `CotizacionDetalles` ADD CONSTRAINT `FK_CotizacionDetalles_TiposProceso_TipoProcesoId` FOREIGN KEY (`TipoProcesoId`) REFERENCES `TiposProceso` (`Id`) ON DELETE SET NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260315214451_CotizacionManoObraPorProceso') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260315214451_CotizacionManoObraPorProceso', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260315220820_RemoveMinutosEstandarTipoProceso') THEN

    ALTER TABLE `TiposProceso` DROP COLUMN `MinutosEstandar`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260315220820_RemoveMinutosEstandarTipoProceso') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260315220820_RemoveMinutosEstandarTipoProceso', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260316003744_EmpleadoActividadManoObra') THEN

    ALTER TABLE `Empleados` ADD `ActividadManoObraId` char(36) COLLATE ascii_general_ci NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260316003744_EmpleadoActividadManoObra') THEN

    CREATE INDEX `IX_Empleados_ActividadManoObraId` ON `Empleados` (`ActividadManoObraId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260316003744_EmpleadoActividadManoObra') THEN

    ALTER TABLE `Empleados` ADD CONSTRAINT `FK_Empleados_ActividadesManoObra_ActividadManoObraId` FOREIGN KEY (`ActividadManoObraId`) REFERENCES `ActividadesManoObra` (`Id`) ON DELETE SET NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260316003744_EmpleadoActividadManoObra') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260316003744_EmpleadoActividadManoObra', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260316004542_RenombrarActividadManoObraAPosicion') THEN

    ALTER TABLE `CotizacionDetalles` DROP FOREIGN KEY IF EXISTS `FK_CotizacionDetalles_ActividadesManoObra_ActividadManoObraId`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260316004542_RenombrarActividadManoObraAPosicion') THEN

    ALTER TABLE `CotizacionDetalles` DROP FOREIGN KEY IF EXISTS `FK_CotDet_ActMO`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260316004542_RenombrarActividadManoObraAPosicion') THEN

    ALTER TABLE `CotizacionDetalles` DROP FOREIGN KEY IF EXISTS `FK_CotizacionDetalles_Posiciones_PosicionId`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260316004542_RenombrarActividadManoObraAPosicion') THEN

    ALTER TABLE `Empleados` DROP FOREIGN KEY IF EXISTS `FK_Empleados_ActividadesManoObra_ActividadManoObraId`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260316004542_RenombrarActividadManoObraAPosicion') THEN

    ALTER TABLE `Empleados` DROP FOREIGN KEY IF EXISTS `FK_Empleados_Posiciones_PosicionId`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260316004542_RenombrarActividadManoObraAPosicion') THEN

    ALTER TABLE `TiposProceso` DROP FOREIGN KEY IF EXISTS `FK_TiposProceso_ActividadesManoObra_ActividadManoObraId`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260316004542_RenombrarActividadManoObraAPosicion') THEN

    ALTER TABLE `TiposProceso` DROP FOREIGN KEY IF EXISTS `FK_TiposProceso_Posiciones_PosicionId`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260316004542_RenombrarActividadManoObraAPosicion') THEN

    ALTER TABLE `CotizacionDetalles` CHANGE COLUMN IF EXISTS `ActividadManoObraId` `PosicionId` char(36) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260316004542_RenombrarActividadManoObraAPosicion') THEN

    ALTER TABLE `Empleados` CHANGE COLUMN IF EXISTS `ActividadManoObraId` `PosicionId` char(36) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260316004542_RenombrarActividadManoObraAPosicion') THEN

    ALTER TABLE `TiposProceso` CHANGE COLUMN IF EXISTS `ActividadManoObraId` `PosicionId` char(36) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260316004542_RenombrarActividadManoObraAPosicion') THEN

    ALTER TABLE `CotizacionDetalles` DROP INDEX IF EXISTS `IX_CotizacionDetalles_ActividadManoObraId`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260316004542_RenombrarActividadManoObraAPosicion') THEN

    ALTER TABLE `CotizacionDetalles` ADD INDEX IF NOT EXISTS `IX_CotizacionDetalles_PosicionId` (`PosicionId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260316004542_RenombrarActividadManoObraAPosicion') THEN

    ALTER TABLE `Empleados` DROP INDEX IF EXISTS `IX_Empleados_ActividadManoObraId`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260316004542_RenombrarActividadManoObraAPosicion') THEN

    ALTER TABLE `Empleados` ADD INDEX IF NOT EXISTS `IX_Empleados_PosicionId` (`PosicionId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260316004542_RenombrarActividadManoObraAPosicion') THEN

    ALTER TABLE `TiposProceso` DROP INDEX IF EXISTS `IX_TiposProceso_ActividadManoObraId`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260316004542_RenombrarActividadManoObraAPosicion') THEN

    ALTER TABLE `TiposProceso` ADD INDEX IF NOT EXISTS `IX_TiposProceso_PosicionId` (`PosicionId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260316004542_RenombrarActividadManoObraAPosicion') THEN

    RENAME TABLE IF EXISTS `ActividadesManoObra` TO `Posiciones`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260316004542_RenombrarActividadManoObraAPosicion') THEN

    ALTER TABLE `Posiciones` CHANGE COLUMN IF EXISTS `SueldoSugeridoSemanal` `TarifaPorMinuto` decimal(18,4) NOT NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260316004542_RenombrarActividadManoObraAPosicion') THEN

    ALTER TABLE `Posiciones` MODIFY COLUMN IF EXISTS `TarifaPorMinuto` decimal(18,4) NOT NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260316004542_RenombrarActividadManoObraAPosicion') THEN

    ALTER TABLE `Posiciones` ADD INDEX IF NOT EXISTS `IX_Posiciones_EmpresaId` (`EmpresaId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260316004542_RenombrarActividadManoObraAPosicion') THEN

    ALTER TABLE `Posiciones` ADD INDEX IF NOT EXISTS `IX_Posiciones_Nombre` (`Nombre`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260316004542_RenombrarActividadManoObraAPosicion') THEN

    -- Se omite recrear FKs aquí para tolerar estados parciales heredados en MariaDB.

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260316004542_RenombrarActividadManoObraAPosicion') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260316004542_RenombrarActividadManoObraAPosicion', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260316011412_SimplificarTipoProcesoCostos') THEN

    ALTER TABLE `TiposProceso` DROP COLUMN IF EXISTS `CostoBase`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260316011412_SimplificarTipoProcesoCostos') THEN

    ALTER TABLE `TiposProceso` DROP COLUMN IF EXISTS `TarifaDestajo`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260316011412_SimplificarTipoProcesoCostos') THEN

    ALTER TABLE `TiposProceso` DROP COLUMN IF EXISTS `TarifaPorMinuto`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260316011412_SimplificarTipoProcesoCostos') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260316011412_SimplificarTipoProcesoCostos', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260316012919_PedidoDetalleCotizacionSeleccionada') THEN

    ALTER TABLE `PedidoDetalles` ADD COLUMN IF NOT EXISTS `CotizacionSerigrafiaId` char(36) NULL COLLATE ascii_general_ci;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260316012919_PedidoDetalleCotizacionSeleccionada') THEN

    ALTER TABLE `PedidoDetalles` ADD INDEX IF NOT EXISTS `IX_PedidoDetalles_CotizacionSerigrafiaId` (`CotizacionSerigrafiaId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260316012919_PedidoDetalleCotizacionSeleccionada') THEN

    ALTER TABLE `PedidoDetalles` DROP FOREIGN KEY IF EXISTS `FK_PedidoDetalles_CotizacionesSerigrafia_CotizacionSerigrafiaId`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260316012919_PedidoDetalleCotizacionSeleccionada') THEN

    ALTER TABLE `PedidoDetalles` ADD CONSTRAINT `FK_PedidoDetalles_CotizacionesSerigrafia_CotizacionSerigrafiaId` FOREIGN KEY (`CotizacionSerigrafiaId`) REFERENCES `CotizacionesSerigrafia` (`Id`) ON DELETE SET NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260316012919_PedidoDetalleCotizacionSeleccionada') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260316012919_PedidoDetalleCotizacionSeleccionada', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260316025644_PermitirMultiplesRegistrosDestajoPorProceso') THEN

    ALTER TABLE `RegistrosDestajoProceso` DROP FOREIGN KEY `FK_RegistrosDestajoProceso_PedidoSerigrafiaTallaProcesos_Pedido~`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260316025644_PermitirMultiplesRegistrosDestajoPorProceso') THEN

    ALTER TABLE `RegistrosDestajoProceso` DROP INDEX `IX_RegistrosDestajoProceso_PedidoSerigrafiaTallaProcesoId`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260316025644_PermitirMultiplesRegistrosDestajoPorProceso') THEN

    CREATE INDEX `IX_RegistrosDestajoProceso_PedidoSerigrafiaTallaProcesoId` ON `RegistrosDestajoProceso` (`PedidoSerigrafiaTallaProcesoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260316025644_PermitirMultiplesRegistrosDestajoPorProceso') THEN

    ALTER TABLE `RegistrosDestajoProceso` ADD CONSTRAINT `FK_RegistrosDestajoProceso_PedidoSerigrafiaTallaProcesos_Pedido~` FOREIGN KEY (`PedidoSerigrafiaTallaProcesoId`) REFERENCES `PedidoSerigrafiaTallaProcesos` (`Id`) ON DELETE CASCADE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260316025644_PermitirMultiplesRegistrosDestajoPorProceso') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260316025644_PermitirMultiplesRegistrosDestajoPorProceso', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260316040201_NominaPeriodicidadPagoYConfiguracion') THEN

    ALTER TABLE `Empleados` ADD `PeriodicidadPago` int NOT NULL DEFAULT 1;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260316040201_NominaPeriodicidadPagoYConfiguracion') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260316040201_NominaPeriodicidadPagoYConfiguracion', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260318182055_EsquemasPagoYValesDestajo') THEN

    ALTER TABLE `NominaDetalles` ADD `EsquemaPagoId` char(36) COLLATE ascii_general_ci NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260318182055_EsquemasPagoYValesDestajo') THEN

    ALTER TABLE `NominaDetalles` ADD `MontoBono` decimal(18,2) NOT NULL DEFAULT 0.0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260318182055_EsquemasPagoYValesDestajo') THEN

    ALTER TABLE `NominaDetalles` ADD `MontoDestajo` decimal(18,2) NOT NULL DEFAULT 0.0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260318182055_EsquemasPagoYValesDestajo') THEN

    ALTER TABLE `NominaDetalles` ADD `TotalPiezas` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260318182055_EsquemasPagoYValesDestajo') THEN

    CREATE TABLE `EsquemasPago` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `EmpresaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Nombre` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `Descripcion` varchar(500) CHARACTER SET utf8mb4 NULL,
        `Tipo` int NOT NULL,
        `IncluyeSueldoBase` tinyint(1) NOT NULL,
        `SueldoBaseSugerido` decimal(18,2) NULL,
        `UsaMetaPorPedidos` tinyint(1) NOT NULL,
        `BonoCumplimientoMonto` decimal(18,2) NULL,
        `BonoAdelantoMonto` decimal(18,2) NULL,
        `BonoRepartirPorSueldo` tinyint(1) NOT NULL,
        `BonoRepartirPorAsistencia` tinyint(1) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_EsquemasPago` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_EsquemasPago_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `Empresas` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260318182055_EsquemasPagoYValesDestajo') THEN

    CREATE TABLE `EmpleadosEsquemaPago` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `EmpleadoId` char(36) COLLATE ascii_general_ci NOT NULL,
        `EsquemaPagoId` char(36) COLLATE ascii_general_ci NOT NULL,
        `SueldoBaseOverride` decimal(18,2) NULL,
        `VigenteDesde` datetime(6) NOT NULL,
        `VigenteHasta` datetime(6) NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_EmpleadosEsquemaPago` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_EmpleadosEsquemaPago_Empleados_EmpleadoId` FOREIGN KEY (`EmpleadoId`) REFERENCES `Empleados` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_EmpleadosEsquemaPago_EsquemasPago_EsquemaPagoId` FOREIGN KEY (`EsquemaPagoId`) REFERENCES `EsquemasPago` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260318182055_EsquemasPagoYValesDestajo') THEN

    CREATE TABLE `EsquemasPagoTarifa` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `EsquemaPagoId` char(36) COLLATE ascii_general_ci NOT NULL,
        `TipoProcesoId` char(36) COLLATE ascii_general_ci NULL,
        `PosicionId` char(36) COLLATE ascii_general_ci NULL,
        `Tarifa` decimal(18,4) NOT NULL,
        `Descripcion` varchar(200) CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_EsquemasPagoTarifa` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_EsquemasPagoTarifa_EsquemasPago_EsquemaPagoId` FOREIGN KEY (`EsquemaPagoId`) REFERENCES `EsquemasPago` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_EsquemasPagoTarifa_Posiciones_PosicionId` FOREIGN KEY (`PosicionId`) REFERENCES `Posiciones` (`Id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_EsquemasPagoTarifa_TiposProceso_TipoProcesoId` FOREIGN KEY (`TipoProcesoId`) REFERENCES `TiposProceso` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260318182055_EsquemasPagoYValesDestajo') THEN

    CREATE TABLE `ValesDestajo` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `EmpresaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Folio` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `EmpleadoId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Fecha` datetime(6) NOT NULL,
        `Estatus` int NOT NULL,
        `EsquemaPagoId` char(36) COLLATE ascii_general_ci NULL,
        `NominaDetalleId` char(36) COLLATE ascii_general_ci NULL,
        `Observaciones` varchar(500) CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_ValesDestajo` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_ValesDestajo_Empleados_EmpleadoId` FOREIGN KEY (`EmpleadoId`) REFERENCES `Empleados` (`Id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_ValesDestajo_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `Empresas` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_ValesDestajo_EsquemasPago_EsquemaPagoId` FOREIGN KEY (`EsquemaPagoId`) REFERENCES `EsquemasPago` (`Id`) ON DELETE SET NULL,
        CONSTRAINT `FK_ValesDestajo_NominaDetalles_NominaDetalleId` FOREIGN KEY (`NominaDetalleId`) REFERENCES `NominaDetalles` (`Id`) ON DELETE SET NULL
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260318182055_EsquemasPagoYValesDestajo') THEN

    CREATE TABLE `ValesDestajoDetalle` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `ValeDestajoId` char(36) COLLATE ascii_general_ci NOT NULL,
        `TipoProcesoId` char(36) COLLATE ascii_general_ci NOT NULL,
        `PedidoId` char(36) COLLATE ascii_general_ci NULL,
        `Cantidad` int NOT NULL,
        `TarifaAplicada` decimal(18,4) NOT NULL,
        `Importe` decimal(18,2) NOT NULL,
        `EsquemaPagoTarifaId` char(36) COLLATE ascii_general_ci NULL,
        `TiempoMinutos` int NULL,
        `Observaciones` varchar(500) CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_ValesDestajoDetalle` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_ValesDestajoDetalle_EsquemasPagoTarifa_EsquemaPagoTarifaId` FOREIGN KEY (`EsquemaPagoTarifaId`) REFERENCES `EsquemasPagoTarifa` (`Id`) ON DELETE SET NULL,
        CONSTRAINT `FK_ValesDestajoDetalle_Pedidos_PedidoId` FOREIGN KEY (`PedidoId`) REFERENCES `Pedidos` (`Id`) ON DELETE SET NULL,
        CONSTRAINT `FK_ValesDestajoDetalle_TiposProceso_TipoProcesoId` FOREIGN KEY (`TipoProcesoId`) REFERENCES `TiposProceso` (`Id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_ValesDestajoDetalle_ValesDestajo_ValeDestajoId` FOREIGN KEY (`ValeDestajoId`) REFERENCES `ValesDestajo` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260318182055_EsquemasPagoYValesDestajo') THEN

    CREATE INDEX `IX_NominaDetalles_EsquemaPagoId` ON `NominaDetalles` (`EsquemaPagoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260318182055_EsquemasPagoYValesDestajo') THEN

    CREATE INDEX `IX_EmpleadosEsquemaPago_EmpleadoId` ON `EmpleadosEsquemaPago` (`EmpleadoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260318182055_EsquemasPagoYValesDestajo') THEN

    CREATE INDEX `IX_EmpleadosEsquemaPago_EmpleadoId_VigenteDesde` ON `EmpleadosEsquemaPago` (`EmpleadoId`, `VigenteDesde`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260318182055_EsquemasPagoYValesDestajo') THEN

    CREATE INDEX `IX_EmpleadosEsquemaPago_EsquemaPagoId` ON `EmpleadosEsquemaPago` (`EsquemaPagoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260318182055_EsquemasPagoYValesDestajo') THEN

    CREATE UNIQUE INDEX `IX_EsquemasPago_EmpresaId_Nombre` ON `EsquemasPago` (`EmpresaId`, `Nombre`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260318182055_EsquemasPagoYValesDestajo') THEN

    CREATE INDEX `IX_EsquemasPago_Tipo` ON `EsquemasPago` (`Tipo`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260318182055_EsquemasPagoYValesDestajo') THEN

    CREATE INDEX `IX_EsquemasPagoTarifa_EsquemaPagoId` ON `EsquemasPagoTarifa` (`EsquemaPagoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260318182055_EsquemasPagoYValesDestajo') THEN

    CREATE INDEX `IX_EsquemasPagoTarifa_EsquemaPagoId_TipoProcesoId_PosicionId` ON `EsquemasPagoTarifa` (`EsquemaPagoId`, `TipoProcesoId`, `PosicionId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260318182055_EsquemasPagoYValesDestajo') THEN

    CREATE INDEX `IX_EsquemasPagoTarifa_PosicionId` ON `EsquemasPagoTarifa` (`PosicionId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260318182055_EsquemasPagoYValesDestajo') THEN

    CREATE INDEX `IX_EsquemasPagoTarifa_TipoProcesoId` ON `EsquemasPagoTarifa` (`TipoProcesoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260318182055_EsquemasPagoYValesDestajo') THEN

    CREATE INDEX `IX_ValesDestajo_EmpleadoId` ON `ValesDestajo` (`EmpleadoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260318182055_EsquemasPagoYValesDestajo') THEN

    CREATE UNIQUE INDEX `IX_ValesDestajo_EmpresaId_Folio` ON `ValesDestajo` (`EmpresaId`, `Folio`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260318182055_EsquemasPagoYValesDestajo') THEN

    CREATE INDEX `IX_ValesDestajo_EsquemaPagoId` ON `ValesDestajo` (`EsquemaPagoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260318182055_EsquemasPagoYValesDestajo') THEN

    CREATE INDEX `IX_ValesDestajo_Estatus` ON `ValesDestajo` (`Estatus`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260318182055_EsquemasPagoYValesDestajo') THEN

    CREATE INDEX `IX_ValesDestajo_Fecha` ON `ValesDestajo` (`Fecha`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260318182055_EsquemasPagoYValesDestajo') THEN

    CREATE INDEX `IX_ValesDestajo_NominaDetalleId` ON `ValesDestajo` (`NominaDetalleId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260318182055_EsquemasPagoYValesDestajo') THEN

    CREATE INDEX `IX_ValesDestajoDetalle_EsquemaPagoTarifaId` ON `ValesDestajoDetalle` (`EsquemaPagoTarifaId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260318182055_EsquemasPagoYValesDestajo') THEN

    CREATE INDEX `IX_ValesDestajoDetalle_PedidoId` ON `ValesDestajoDetalle` (`PedidoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260318182055_EsquemasPagoYValesDestajo') THEN

    CREATE INDEX `IX_ValesDestajoDetalle_TipoProcesoId` ON `ValesDestajoDetalle` (`TipoProcesoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260318182055_EsquemasPagoYValesDestajo') THEN

    CREATE INDEX `IX_ValesDestajoDetalle_ValeDestajoId` ON `ValesDestajoDetalle` (`ValeDestajoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260318182055_EsquemasPagoYValesDestajo') THEN

    ALTER TABLE `NominaDetalles` ADD CONSTRAINT `FK_NominaDetalles_EsquemasPago_EsquemaPagoId` FOREIGN KEY (`EsquemaPagoId`) REFERENCES `EsquemasPago` (`Id`) ON DELETE SET NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260318182055_EsquemasPagoYValesDestajo') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260318182055_EsquemasPagoYValesDestajo', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260408213105_VincularRegistrosDestajoAVales') THEN

    ALTER TABLE `RegistrosDestajoProceso` ADD `ValeDestajoDetalleId` char(36) COLLATE ascii_general_ci NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260408213105_VincularRegistrosDestajoAVales') THEN

    CREATE INDEX `IX_RegistrosDestajoProceso_ValeDestajoDetalleId` ON `RegistrosDestajoProceso` (`ValeDestajoDetalleId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260408213105_VincularRegistrosDestajoAVales') THEN

    ALTER TABLE `RegistrosDestajoProceso` ADD CONSTRAINT `FK_RegistrosDestajoProceso_ValesDestajoDetalle_ValeDestajoDetal~` FOREIGN KEY (`ValeDestajoDetalleId`) REFERENCES `ValesDestajoDetalle` (`Id`) ON DELETE SET NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260408213105_VincularRegistrosDestajoAVales') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260408213105_VincularRegistrosDestajoAVales', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260408224547_AddFacturacionBase') THEN

    ALTER TABLE `Proveedores` ADD `Banco` varchar(100) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260408224547_AddFacturacionBase') THEN

    ALTER TABLE `Proveedores` ADD `Ciudad` varchar(100) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260408224547_AddFacturacionBase') THEN

    ALTER TABLE `Proveedores` ADD `Clabe` varchar(25) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260408224547_AddFacturacionBase') THEN

    ALTER TABLE `Proveedores` ADD `CodigoPostal` varchar(10) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260408224547_AddFacturacionBase') THEN

    ALTER TABLE `Proveedores` ADD `CreatedBy` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260408224547_AddFacturacionBase') THEN

    ALTER TABLE `Proveedores` ADD `CuentaBancaria` varchar(50) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260408224547_AddFacturacionBase') THEN

    ALTER TABLE `Proveedores` ADD `DiasPago` int NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260408224547_AddFacturacionBase') THEN

    ALTER TABLE `Proveedores` ADD `EmailFiscal` varchar(100) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260408224547_AddFacturacionBase') THEN

    ALTER TABLE `Proveedores` ADD `EmailOrdenesCompra` varchar(100) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260408224547_AddFacturacionBase') THEN

    ALTER TABLE `Proveedores` ADD `EsCliente` tinyint(1) NOT NULL DEFAULT FALSE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260408224547_AddFacturacionBase') THEN

    ALTER TABLE `Proveedores` ADD `Estado` varchar(100) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260408224547_AddFacturacionBase') THEN

    ALTER TABLE `Proveedores` ADD `Pais` varchar(50) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260408224547_AddFacturacionBase') THEN

    ALTER TABLE `Proveedores` ADD `RegimenFiscal` varchar(10) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260408224547_AddFacturacionBase') THEN

    ALTER TABLE `Proveedores` ADD `TitularCuenta` varchar(200) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260408224547_AddFacturacionBase') THEN

    ALTER TABLE `Proveedores` ADD `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260408224547_AddFacturacionBase') THEN

    ALTER TABLE `PagosPedido` ADD `MedioCobroInterno` varchar(50) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260408224547_AddFacturacionBase') THEN

    ALTER TABLE `PagosPedido` ADD `MetodoPagoSat` varchar(10) CHARACTER SET utf8mb4 NOT NULL DEFAULT '';

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260408224547_AddFacturacionBase') THEN

    ALTER TABLE `Clientes` ADD `DiasCredito` int NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260408224547_AddFacturacionBase') THEN

    ALTER TABLE `Clientes` ADD `DomicilioFiscalCp` varchar(10) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260408224547_AddFacturacionBase') THEN

    ALTER TABLE `Clientes` ADD `EmailCobranza` varchar(100) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260408224547_AddFacturacionBase') THEN

    ALTER TABLE `Clientes` ADD `EmailFacturacion` varchar(100) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260408224547_AddFacturacionBase') THEN

    ALTER TABLE `Clientes` ADD `FechaValidacionRfc` datetime(6) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260408224547_AddFacturacionBase') THEN

    ALTER TABLE `Clientes` ADD `LimiteCredito` decimal(18,2) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260408224547_AddFacturacionBase') THEN

    ALTER TABLE `Clientes` ADD `RegimenFiscalReceptor` varchar(10) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260408224547_AddFacturacionBase') THEN

    ALTER TABLE `Clientes` ADD `RfcValidado` tinyint(1) NOT NULL DEFAULT FALSE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260408224547_AddFacturacionBase') THEN

    ALTER TABLE `Clientes` ADD `UsoCfdi` varchar(10) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260408224547_AddFacturacionBase') THEN

    CREATE TABLE `Facturas` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `EmpresaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `ClienteId` char(36) COLLATE ascii_general_ci NOT NULL,
        `PedidoId` char(36) COLLATE ascii_general_ci NULL,
        `TipoComprobante` int NOT NULL,
        `Estatus` int NOT NULL,
        `Serie` varchar(20) CHARACTER SET utf8mb4 NULL,
        `FolioInterno` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `SerieFiscal` varchar(20) CHARACTER SET utf8mb4 NULL,
        `FolioFiscal` varchar(50) CHARACTER SET utf8mb4 NULL,
        `ExternalDocumentId` varchar(100) CHARACTER SET utf8mb4 NULL,
        `UuidFiscal` varchar(50) CHARACTER SET utf8mb4 NULL,
        `FechaEmision` datetime(6) NOT NULL,
        `FechaTimbrado` datetime(6) NULL,
        `Moneda` varchar(10) CHARACTER SET utf8mb4 NOT NULL,
        `TipoCambio` decimal(18,6) NOT NULL,
        `LugarExpedicionCp` varchar(10) CHARACTER SET utf8mb4 NULL,
        `MetodoPagoSat` varchar(10) CHARACTER SET utf8mb4 NOT NULL,
        `FormaPagoSat` varchar(10) CHARACTER SET utf8mb4 NOT NULL,
        `UsoCfdi` varchar(10) CHARACTER SET utf8mb4 NULL,
        `Exportacion` varchar(10) CHARACTER SET utf8mb4 NULL,
        `CondicionesPago` varchar(250) CHARACTER SET utf8mb4 NULL,
        `Observaciones` varchar(500) CHARACTER SET utf8mb4 NULL,
        `Subtotal` decimal(18,2) NOT NULL,
        `DescuentoTotal` decimal(18,2) NOT NULL,
        `TotalImpuestosTrasladados` decimal(18,2) NOT NULL,
        `TotalImpuestosRetenidos` decimal(18,2) NOT NULL,
        `Total` decimal(18,2) NOT NULL,
        `Cancelable` tinyint(1) NOT NULL,
        `CancellationStatus` varchar(50) CHARACTER SET utf8mb4 NULL,
        `XmlUrl` varchar(500) CHARACTER SET utf8mb4 NULL,
        `PdfUrl` varchar(500) CHARACTER SET utf8mb4 NULL,
        `AcuseCancelacionUrl` varchar(500) CHARACTER SET utf8mb4 NULL,
        `ErrorCode` varchar(50) CHARACTER SET utf8mb4 NULL,
        `ErrorMessage` varchar(1000) CHARACTER SET utf8mb4 NULL,
        `JsonSnapshotComercial` longtext CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_Facturas` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_Facturas_Clientes_ClienteId` FOREIGN KEY (`ClienteId`) REFERENCES `Clientes` (`Id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_Facturas_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `Empresas` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_Facturas_Pedidos_PedidoId` FOREIGN KEY (`PedidoId`) REFERENCES `Pedidos` (`Id`) ON DELETE SET NULL
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260408224547_AddFacturacionBase') THEN

    CREATE TABLE `PagosRecibidos` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `EmpresaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `ClienteId` char(36) COLLATE ascii_general_ci NOT NULL,
        `PedidoId` char(36) COLLATE ascii_general_ci NULL,
        `FechaPago` datetime(6) NOT NULL,
        `Monto` decimal(18,2) NOT NULL,
        `FormaPagoSat` varchar(10) CHARACTER SET utf8mb4 NOT NULL,
        `MedioCobroInterno` varchar(50) CHARACTER SET utf8mb4 NULL,
        `Referencia` varchar(100) CHARACTER SET utf8mb4 NULL,
        `Notas` varchar(500) CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_PagosRecibidos` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_PagosRecibidos_Clientes_ClienteId` FOREIGN KEY (`ClienteId`) REFERENCES `Clientes` (`Id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_PagosRecibidos_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `Empresas` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_PagosRecibidos_Pedidos_PedidoId` FOREIGN KEY (`PedidoId`) REFERENCES `Pedidos` (`Id`) ON DELETE SET NULL
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260408224547_AddFacturacionBase') THEN

    CREATE TABLE `ClientesDatosFiscalesSnapshot` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `EmpresaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `ClienteId` char(36) COLLATE ascii_general_ci NOT NULL,
        `FacturaId` char(36) COLLATE ascii_general_ci NULL,
        `RazonSocial` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `Rfc` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `RegimenFiscalReceptor` varchar(10) CHARACTER SET utf8mb4 NULL,
        `DomicilioFiscalCp` varchar(10) CHARACTER SET utf8mb4 NULL,
        `UsoCfdi` varchar(10) CHARACTER SET utf8mb4 NULL,
        `EmailFactura` varchar(100) CHARACTER SET utf8mb4 NULL,
        `JsonDatos` longtext CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_ClientesDatosFiscalesSnapshot` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_ClientesDatosFiscalesSnapshot_Clientes_ClienteId` FOREIGN KEY (`ClienteId`) REFERENCES `Clientes` (`Id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_ClientesDatosFiscalesSnapshot_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `Empresas` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_ClientesDatosFiscalesSnapshot_Facturas_FacturaId` FOREIGN KEY (`FacturaId`) REFERENCES `Facturas` (`Id`) ON DELETE SET NULL
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260408224547_AddFacturacionBase') THEN

    CREATE TABLE `FacturaDetalles` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `FacturaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `ReferenciaOrigen` varchar(100) CHARACTER SET utf8mb4 NULL,
        `Cantidad` decimal(18,4) NOT NULL,
        `ClaveUnidadSat` varchar(10) CHARACTER SET utf8mb4 NULL,
        `Unidad` varchar(50) CHARACTER SET utf8mb4 NULL,
        `ClaveProductoServicioSat` varchar(20) CHARACTER SET utf8mb4 NULL,
        `Descripcion` varchar(500) CHARACTER SET utf8mb4 NOT NULL,
        `ValorUnitario` decimal(18,4) NOT NULL,
        `Descuento` decimal(18,2) NOT NULL,
        `ObjetoImpuesto` varchar(10) CHARACTER SET utf8mb4 NULL,
        `Importe` decimal(18,2) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_FacturaDetalles` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_FacturaDetalles_Facturas_FacturaId` FOREIGN KEY (`FacturaId`) REFERENCES `Facturas` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260408224547_AddFacturacionBase') THEN

    CREATE TABLE `FacturaEventos` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `EmpresaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `FacturaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Tipo` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `Fecha` datetime(6) NOT NULL,
        `Usuario` varchar(100) CHARACTER SET utf8mb4 NULL,
        `Descripcion` varchar(1000) CHARACTER SET utf8mb4 NULL,
        `DatosJson` longtext CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_FacturaEventos` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_FacturaEventos_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `Empresas` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_FacturaEventos_Facturas_FacturaId` FOREIGN KEY (`FacturaId`) REFERENCES `Facturas` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260408224547_AddFacturacionBase') THEN

    CREATE TABLE `FacturasRelacionadas` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `EmpresaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `FacturaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `TipoRelacionCfdi` varchar(10) CHARACTER SET utf8mb4 NOT NULL,
        `UuidRelacionado` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_FacturasRelacionadas` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_FacturasRelacionadas_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `Empresas` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_FacturasRelacionadas_Facturas_FacturaId` FOREIGN KEY (`FacturaId`) REFERENCES `Facturas` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260408224547_AddFacturacionBase') THEN

    CREATE TABLE `ComplementosPago` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `EmpresaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `ClienteId` char(36) COLLATE ascii_general_ci NOT NULL,
        `PagoRecibidoId` char(36) COLLATE ascii_general_ci NULL,
        `Estatus` int NOT NULL,
        `Serie` varchar(20) CHARACTER SET utf8mb4 NULL,
        `FolioInterno` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `SerieFiscal` varchar(20) CHARACTER SET utf8mb4 NULL,
        `FolioFiscal` varchar(50) CHARACTER SET utf8mb4 NULL,
        `ExternalDocumentId` varchar(100) CHARACTER SET utf8mb4 NULL,
        `UuidFiscal` varchar(50) CHARACTER SET utf8mb4 NULL,
        `FechaEmision` datetime(6) NOT NULL,
        `FechaTimbrado` datetime(6) NULL,
        `Moneda` varchar(10) CHARACTER SET utf8mb4 NOT NULL,
        `TipoCambio` decimal(18,6) NOT NULL,
        `MontoTotalPagos` decimal(18,2) NOT NULL,
        `XmlUrl` varchar(500) CHARACTER SET utf8mb4 NULL,
        `PdfUrl` varchar(500) CHARACTER SET utf8mb4 NULL,
        `ErrorCode` varchar(50) CHARACTER SET utf8mb4 NULL,
        `ErrorMessage` varchar(1000) CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_ComplementosPago` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_ComplementosPago_Clientes_ClienteId` FOREIGN KEY (`ClienteId`) REFERENCES `Clientes` (`Id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_ComplementosPago_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `Empresas` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_ComplementosPago_PagosRecibidos_PagoRecibidoId` FOREIGN KEY (`PagoRecibidoId`) REFERENCES `PagosRecibidos` (`Id`) ON DELETE SET NULL
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260408224547_AddFacturacionBase') THEN

    CREATE TABLE `PagosAplicacionDocumento` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `EmpresaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `PagoRecibidoId` char(36) COLLATE ascii_general_ci NOT NULL,
        `FacturaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `ImporteAplicado` decimal(18,2) NOT NULL,
        `SaldoAnterior` decimal(18,2) NOT NULL,
        `SaldoInsoluto` decimal(18,2) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_PagosAplicacionDocumento` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_PagosAplicacionDocumento_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `Empresas` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_PagosAplicacionDocumento_Facturas_FacturaId` FOREIGN KEY (`FacturaId`) REFERENCES `Facturas` (`Id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_PagosAplicacionDocumento_PagosRecibidos_PagoRecibidoId` FOREIGN KEY (`PagoRecibidoId`) REFERENCES `PagosRecibidos` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260408224547_AddFacturacionBase') THEN

    CREATE TABLE `FacturaImpuestos` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `EmpresaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `FacturaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `FacturaDetalleId` char(36) COLLATE ascii_general_ci NULL,
        `Impuesto` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `TipoFactor` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `TasaOCuota` decimal(18,6) NOT NULL,
        `Base` decimal(18,2) NOT NULL,
        `Importe` decimal(18,2) NOT NULL,
        `EsRetencion` tinyint(1) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_FacturaImpuestos` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_FacturaImpuestos_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `Empresas` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_FacturaImpuestos_FacturaDetalles_FacturaDetalleId` FOREIGN KEY (`FacturaDetalleId`) REFERENCES `FacturaDetalles` (`Id`) ON DELETE SET NULL,
        CONSTRAINT `FK_FacturaImpuestos_Facturas_FacturaId` FOREIGN KEY (`FacturaId`) REFERENCES `Facturas` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260408224547_AddFacturacionBase') THEN

    CREATE TABLE `ComplementosPagoDocumentosRelacionados` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `EmpresaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `ComplementoPagoId` char(36) COLLATE ascii_general_ci NOT NULL,
        `FacturaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `UuidDocumentoRelacionado` varchar(50) CHARACTER SET utf8mb4 NULL,
        `MonedaDocumento` varchar(10) CHARACTER SET utf8mb4 NOT NULL,
        `TipoCambioDocumento` decimal(18,6) NOT NULL,
        `SaldoAnterior` decimal(18,2) NOT NULL,
        `ImportePagado` decimal(18,2) NOT NULL,
        `SaldoInsoluto` decimal(18,2) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_ComplementosPagoDocumentosRelacionados` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_ComplementosPagoDocumentosRelacionados_ComplementosPago_Comp~` FOREIGN KEY (`ComplementoPagoId`) REFERENCES `ComplementosPago` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_ComplementosPagoDocumentosRelacionados_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `Empresas` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_ComplementosPagoDocumentosRelacionados_Facturas_FacturaId` FOREIGN KEY (`FacturaId`) REFERENCES `Facturas` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260408224547_AddFacturacionBase') THEN

    CREATE INDEX `IX_ClientesDatosFiscalesSnapshot_ClienteId` ON `ClientesDatosFiscalesSnapshot` (`ClienteId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260408224547_AddFacturacionBase') THEN

    CREATE INDEX `IX_ClientesDatosFiscalesSnapshot_EmpresaId` ON `ClientesDatosFiscalesSnapshot` (`EmpresaId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260408224547_AddFacturacionBase') THEN

    CREATE INDEX `IX_ClientesDatosFiscalesSnapshot_FacturaId` ON `ClientesDatosFiscalesSnapshot` (`FacturaId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260408224547_AddFacturacionBase') THEN

    CREATE INDEX `IX_ComplementosPago_ClienteId` ON `ComplementosPago` (`ClienteId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260408224547_AddFacturacionBase') THEN

    CREATE UNIQUE INDEX `IX_ComplementosPago_EmpresaId_FolioInterno` ON `ComplementosPago` (`EmpresaId`, `FolioInterno`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260408224547_AddFacturacionBase') THEN

    CREATE INDEX `IX_ComplementosPago_PagoRecibidoId` ON `ComplementosPago` (`PagoRecibidoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260408224547_AddFacturacionBase') THEN

    CREATE INDEX `IX_ComplementosPago_UuidFiscal` ON `ComplementosPago` (`UuidFiscal`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260408224547_AddFacturacionBase') THEN

    CREATE INDEX `IX_ComplementosPagoDocumentosRelacionados_ComplementoPagoId` ON `ComplementosPagoDocumentosRelacionados` (`ComplementoPagoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260408224547_AddFacturacionBase') THEN

    CREATE INDEX `IX_ComplementosPagoDocumentosRelacionados_EmpresaId` ON `ComplementosPagoDocumentosRelacionados` (`EmpresaId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260408224547_AddFacturacionBase') THEN

    CREATE INDEX `IX_ComplementosPagoDocumentosRelacionados_FacturaId` ON `ComplementosPagoDocumentosRelacionados` (`FacturaId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260408224547_AddFacturacionBase') THEN

    CREATE INDEX `IX_FacturaDetalles_FacturaId` ON `FacturaDetalles` (`FacturaId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260408224547_AddFacturacionBase') THEN

    CREATE INDEX `IX_FacturaEventos_EmpresaId` ON `FacturaEventos` (`EmpresaId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260408224547_AddFacturacionBase') THEN

    CREATE INDEX `IX_FacturaEventos_FacturaId` ON `FacturaEventos` (`FacturaId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260408224547_AddFacturacionBase') THEN

    CREATE INDEX `IX_FacturaEventos_Fecha` ON `FacturaEventos` (`Fecha`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260408224547_AddFacturacionBase') THEN

    CREATE INDEX `IX_FacturaImpuestos_EmpresaId` ON `FacturaImpuestos` (`EmpresaId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260408224547_AddFacturacionBase') THEN

    CREATE INDEX `IX_FacturaImpuestos_FacturaDetalleId` ON `FacturaImpuestos` (`FacturaDetalleId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260408224547_AddFacturacionBase') THEN

    CREATE INDEX `IX_FacturaImpuestos_FacturaId` ON `FacturaImpuestos` (`FacturaId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260408224547_AddFacturacionBase') THEN

    CREATE INDEX `IX_Facturas_ClienteId` ON `Facturas` (`ClienteId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260408224547_AddFacturacionBase') THEN

    CREATE UNIQUE INDEX `IX_Facturas_EmpresaId_FolioInterno` ON `Facturas` (`EmpresaId`, `FolioInterno`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260408224547_AddFacturacionBase') THEN

    CREATE INDEX `IX_Facturas_Estatus` ON `Facturas` (`Estatus`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260408224547_AddFacturacionBase') THEN

    CREATE INDEX `IX_Facturas_FechaEmision` ON `Facturas` (`FechaEmision`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260408224547_AddFacturacionBase') THEN

    CREATE INDEX `IX_Facturas_PedidoId` ON `Facturas` (`PedidoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260408224547_AddFacturacionBase') THEN

    CREATE INDEX `IX_Facturas_UuidFiscal` ON `Facturas` (`UuidFiscal`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260408224547_AddFacturacionBase') THEN

    CREATE INDEX `IX_FacturasRelacionadas_EmpresaId` ON `FacturasRelacionadas` (`EmpresaId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260408224547_AddFacturacionBase') THEN

    CREATE INDEX `IX_FacturasRelacionadas_FacturaId` ON `FacturasRelacionadas` (`FacturaId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260408224547_AddFacturacionBase') THEN

    CREATE INDEX `IX_PagosAplicacionDocumento_EmpresaId` ON `PagosAplicacionDocumento` (`EmpresaId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260408224547_AddFacturacionBase') THEN

    CREATE INDEX `IX_PagosAplicacionDocumento_FacturaId` ON `PagosAplicacionDocumento` (`FacturaId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260408224547_AddFacturacionBase') THEN

    CREATE INDEX `IX_PagosAplicacionDocumento_PagoRecibidoId` ON `PagosAplicacionDocumento` (`PagoRecibidoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260408224547_AddFacturacionBase') THEN

    CREATE INDEX `IX_PagosRecibidos_ClienteId` ON `PagosRecibidos` (`ClienteId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260408224547_AddFacturacionBase') THEN

    CREATE INDEX `IX_PagosRecibidos_EmpresaId` ON `PagosRecibidos` (`EmpresaId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260408224547_AddFacturacionBase') THEN

    CREATE INDEX `IX_PagosRecibidos_FechaPago` ON `PagosRecibidos` (`FechaPago`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260408224547_AddFacturacionBase') THEN

    CREATE INDEX `IX_PagosRecibidos_PedidoId` ON `PagosRecibidos` (`PedidoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260408224547_AddFacturacionBase') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260408224547_AddFacturacionBase', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260409144029_AddNotasEntregaParciales') THEN

    ALTER TABLE `PagosRecibidos` ADD `NotaEntregaId` char(36) COLLATE ascii_general_ci NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260409144029_AddNotasEntregaParciales') THEN

    ALTER TABLE `Facturas` ADD `NotaEntregaId` char(36) COLLATE ascii_general_ci NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260409144029_AddNotasEntregaParciales') THEN

    CREATE TABLE `NotasEntrega` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `EmpresaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `ClienteId` char(36) COLLATE ascii_general_ci NOT NULL,
        `PedidoId` char(36) COLLATE ascii_general_ci NOT NULL,
        `NumeroNota` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `FechaNota` datetime(6) NOT NULL,
        `Estatus` int NOT NULL,
        `Observaciones` varchar(500) CHARACTER SET utf8mb4 NULL,
        `Subtotal` decimal(18,2) NOT NULL,
        `Impuestos` decimal(18,2) NOT NULL,
        `Total` decimal(18,2) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_NotasEntrega` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_NotasEntrega_Clientes_ClienteId` FOREIGN KEY (`ClienteId`) REFERENCES `Clientes` (`Id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_NotasEntrega_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `Empresas` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_NotasEntrega_Pedidos_PedidoId` FOREIGN KEY (`PedidoId`) REFERENCES `Pedidos` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260409144029_AddNotasEntregaParciales') THEN

    CREATE TABLE `NotasEntregaDetalle` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `EmpresaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `NotaEntregaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `PedidoDetalleId` char(36) COLLATE ascii_general_ci NULL,
        `Descripcion` varchar(500) CHARACTER SET utf8mb4 NOT NULL,
        `Cantidad` decimal(18,4) NOT NULL,
        `PrecioUnitario` decimal(18,4) NOT NULL,
        `Importe` decimal(18,2) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_NotasEntregaDetalle` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_NotasEntregaDetalle_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `Empresas` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_NotasEntregaDetalle_NotasEntrega_NotaEntregaId` FOREIGN KEY (`NotaEntregaId`) REFERENCES `NotasEntrega` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_NotasEntregaDetalle_PedidoDetalles_PedidoDetalleId` FOREIGN KEY (`PedidoDetalleId`) REFERENCES `PedidoDetalles` (`Id`) ON DELETE SET NULL
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260409144029_AddNotasEntregaParciales') THEN

    CREATE INDEX `IX_PagosRecibidos_NotaEntregaId` ON `PagosRecibidos` (`NotaEntregaId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260409144029_AddNotasEntregaParciales') THEN

    CREATE INDEX `IX_Facturas_NotaEntregaId` ON `Facturas` (`NotaEntregaId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260409144029_AddNotasEntregaParciales') THEN

    CREATE INDEX `IX_NotasEntrega_ClienteId` ON `NotasEntrega` (`ClienteId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260409144029_AddNotasEntregaParciales') THEN

    CREATE UNIQUE INDEX `IX_NotasEntrega_EmpresaId_NumeroNota` ON `NotasEntrega` (`EmpresaId`, `NumeroNota`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260409144029_AddNotasEntregaParciales') THEN

    CREATE INDEX `IX_NotasEntrega_FechaNota` ON `NotasEntrega` (`FechaNota`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260409144029_AddNotasEntregaParciales') THEN

    CREATE INDEX `IX_NotasEntrega_PedidoId` ON `NotasEntrega` (`PedidoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260409144029_AddNotasEntregaParciales') THEN

    CREATE INDEX `IX_NotasEntregaDetalle_EmpresaId` ON `NotasEntregaDetalle` (`EmpresaId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260409144029_AddNotasEntregaParciales') THEN

    CREATE INDEX `IX_NotasEntregaDetalle_NotaEntregaId` ON `NotasEntregaDetalle` (`NotaEntregaId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260409144029_AddNotasEntregaParciales') THEN

    CREATE INDEX `IX_NotasEntregaDetalle_PedidoDetalleId` ON `NotasEntregaDetalle` (`PedidoDetalleId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260409144029_AddNotasEntregaParciales') THEN

    ALTER TABLE `Facturas` ADD CONSTRAINT `FK_Facturas_NotasEntrega_NotaEntregaId` FOREIGN KEY (`NotaEntregaId`) REFERENCES `NotasEntrega` (`Id`) ON DELETE SET NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260409144029_AddNotasEntregaParciales') THEN

    ALTER TABLE `PagosRecibidos` ADD CONSTRAINT `FK_PagosRecibidos_NotasEntrega_NotaEntregaId` FOREIGN KEY (`NotaEntregaId`) REFERENCES `NotasEntrega` (`Id`) ON DELETE SET NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260409144029_AddNotasEntregaParciales') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260409144029_AddNotasEntregaParciales', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260409150834_FacturaAgrupaMultiplesNotas') THEN

    CREATE TABLE `FacturasNotasEntrega` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `EmpresaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `FacturaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `NotaEntregaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Subtotal` decimal(18,2) NOT NULL,
        `Impuestos` decimal(18,2) NOT NULL,
        `Total` decimal(18,2) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_FacturasNotasEntrega` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_FacturasNotasEntrega_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `Empresas` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_FacturasNotasEntrega_Facturas_FacturaId` FOREIGN KEY (`FacturaId`) REFERENCES `Facturas` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_FacturasNotasEntrega_NotasEntrega_NotaEntregaId` FOREIGN KEY (`NotaEntregaId`) REFERENCES `NotasEntrega` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260409150834_FacturaAgrupaMultiplesNotas') THEN

    CREATE INDEX `IX_FacturasNotasEntrega_EmpresaId` ON `FacturasNotasEntrega` (`EmpresaId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260409150834_FacturaAgrupaMultiplesNotas') THEN

    CREATE INDEX `IX_FacturasNotasEntrega_FacturaId` ON `FacturasNotasEntrega` (`FacturaId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260409150834_FacturaAgrupaMultiplesNotas') THEN

    CREATE INDEX `IX_FacturasNotasEntrega_NotaEntregaId` ON `FacturasNotasEntrega` (`NotaEntregaId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260409150834_FacturaAgrupaMultiplesNotas') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260409150834_FacturaAgrupaMultiplesNotas', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260409151956_NotaEntregaNoRequiereFactura') THEN

    ALTER TABLE `NotasEntrega` ADD `NoRequiereFactura` tinyint(1) NOT NULL DEFAULT FALSE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260409151956_NotaEntregaNoRequiereFactura') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260409151956_NotaEntregaNoRequiereFactura', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260409155251_CalzadoFraccionesPorCliente') THEN

    ALTER TABLE `ProductosClientes` ADD `AplicaFraccionCalzado` tinyint(1) NOT NULL DEFAULT FALSE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260409155251_CalzadoFraccionesPorCliente') THEN

    ALTER TABLE `ProductosClientes` ADD `ClienteFraccionCalzadoId` char(36) COLLATE ascii_general_ci NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260409155251_CalzadoFraccionesPorCliente') THEN

    ALTER TABLE `ProductosClientes` ADD `TallaBaseCalzado` varchar(10) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260409155251_CalzadoFraccionesPorCliente') THEN

    ALTER TABLE `PedidoDetalles` ADD `AplicaFraccionCalzado` tinyint(1) NOT NULL DEFAULT FALSE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260409155251_CalzadoFraccionesPorCliente') THEN

    ALTER TABLE `PedidoDetalles` ADD `ClienteFraccionCalzadoId` char(36) COLLATE ascii_general_ci NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260409155251_CalzadoFraccionesPorCliente') THEN

    ALTER TABLE `PedidoDetalles` ADD `TallaBaseCalzado` varchar(10) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260409155251_CalzadoFraccionesPorCliente') THEN

    CREATE TABLE `ClientesFraccionesCalzado` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `EmpresaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `ClienteId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Codigo` varchar(30) CHARACTER SET utf8mb4 NOT NULL,
        `Nombre` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `UnidadesPorFraccion` decimal(18,4) NOT NULL,
        `Activa` tinyint(1) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_ClientesFraccionesCalzado` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_ClientesFraccionesCalzado_Clientes_ClienteId` FOREIGN KEY (`ClienteId`) REFERENCES `Clientes` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_ClientesFraccionesCalzado_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `Empresas` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260409155251_CalzadoFraccionesPorCliente') THEN

    CREATE TABLE `ClientesTallasCalzado` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `EmpresaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `ClienteId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Talla` varchar(10) CHARACTER SET utf8mb4 NOT NULL,
        `Orden` int NOT NULL,
        `EsTallaBaseDefault` tinyint(1) NOT NULL,
        `PorcentajeVariacionDefault` decimal(9,4) NOT NULL,
        `Activa` tinyint(1) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_ClientesTallasCalzado` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_ClientesTallasCalzado_Clientes_ClienteId` FOREIGN KEY (`ClienteId`) REFERENCES `Clientes` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_ClientesTallasCalzado_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `Empresas` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260409155251_CalzadoFraccionesPorCliente') THEN

    CREATE TABLE `ClientesFraccionesCalzadoDetalle` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `EmpresaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `ClienteFraccionCalzadoId` char(36) COLLATE ascii_general_ci NOT NULL,
        `ClienteTallaCalzadoId` char(36) COLLATE ascii_general_ci NULL,
        `Talla` varchar(10) CHARACTER SET utf8mb4 NOT NULL,
        `Orden` int NOT NULL,
        `Unidades` decimal(18,4) NOT NULL,
        `PorcentajeVariacion` decimal(9,4) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_ClientesFraccionesCalzadoDetalle` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_ClientesFraccionesCalzadoDetalle_ClientesFraccionesCalzado_C~` FOREIGN KEY (`ClienteFraccionCalzadoId`) REFERENCES `ClientesFraccionesCalzado` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_ClientesFraccionesCalzadoDetalle_ClientesTallasCalzado_Clien~` FOREIGN KEY (`ClienteTallaCalzadoId`) REFERENCES `ClientesTallasCalzado` (`Id`) ON DELETE SET NULL,
        CONSTRAINT `FK_ClientesFraccionesCalzadoDetalle_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `Empresas` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260409155251_CalzadoFraccionesPorCliente') THEN

    CREATE INDEX `IX_ProductosClientes_ClienteFraccionCalzadoId` ON `ProductosClientes` (`ClienteFraccionCalzadoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260409155251_CalzadoFraccionesPorCliente') THEN

    CREATE INDEX `IX_PedidoDetalles_ClienteFraccionCalzadoId` ON `PedidoDetalles` (`ClienteFraccionCalzadoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260409155251_CalzadoFraccionesPorCliente') THEN

    CREATE UNIQUE INDEX `IX_ClientesFraccionesCalzado_ClienteId_Codigo` ON `ClientesFraccionesCalzado` (`ClienteId`, `Codigo`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260409155251_CalzadoFraccionesPorCliente') THEN

    CREATE INDEX `IX_ClientesFraccionesCalzado_ClienteId_Nombre` ON `ClientesFraccionesCalzado` (`ClienteId`, `Nombre`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260409155251_CalzadoFraccionesPorCliente') THEN

    CREATE INDEX `IX_ClientesFraccionesCalzado_EmpresaId` ON `ClientesFraccionesCalzado` (`EmpresaId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260409155251_CalzadoFraccionesPorCliente') THEN

    CREATE INDEX `IX_ClientesFraccionesCalzadoDetalle_ClienteFraccionCalzadoId` ON `ClientesFraccionesCalzadoDetalle` (`ClienteFraccionCalzadoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260409155251_CalzadoFraccionesPorCliente') THEN

    CREATE INDEX `IX_ClientesFraccionesCalzadoDetalle_ClienteFraccionCalzadoId_Or~` ON `ClientesFraccionesCalzadoDetalle` (`ClienteFraccionCalzadoId`, `Orden`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260409155251_CalzadoFraccionesPorCliente') THEN

    CREATE UNIQUE INDEX `IX_ClientesFraccionesCalzadoDetalle_ClienteFraccionCalzadoId_Ta~` ON `ClientesFraccionesCalzadoDetalle` (`ClienteFraccionCalzadoId`, `Talla`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260409155251_CalzadoFraccionesPorCliente') THEN

    CREATE INDEX `IX_ClientesFraccionesCalzadoDetalle_ClienteTallaCalzadoId` ON `ClientesFraccionesCalzadoDetalle` (`ClienteTallaCalzadoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260409155251_CalzadoFraccionesPorCliente') THEN

    CREATE INDEX `IX_ClientesFraccionesCalzadoDetalle_EmpresaId` ON `ClientesFraccionesCalzadoDetalle` (`EmpresaId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260409155251_CalzadoFraccionesPorCliente') THEN

    CREATE INDEX `IX_ClientesTallasCalzado_ClienteId_Orden` ON `ClientesTallasCalzado` (`ClienteId`, `Orden`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260409155251_CalzadoFraccionesPorCliente') THEN

    CREATE UNIQUE INDEX `IX_ClientesTallasCalzado_ClienteId_Talla` ON `ClientesTallasCalzado` (`ClienteId`, `Talla`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260409155251_CalzadoFraccionesPorCliente') THEN

    CREATE INDEX `IX_ClientesTallasCalzado_EmpresaId` ON `ClientesTallasCalzado` (`EmpresaId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260409155251_CalzadoFraccionesPorCliente') THEN

    ALTER TABLE `PedidoDetalles` ADD CONSTRAINT `FK_PedidoDetalles_ClientesFraccionesCalzado_ClienteFraccionCalz~` FOREIGN KEY (`ClienteFraccionCalzadoId`) REFERENCES `ClientesFraccionesCalzado` (`Id`) ON DELETE SET NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260409155251_CalzadoFraccionesPorCliente') THEN

    ALTER TABLE `ProductosClientes` ADD CONSTRAINT `FK_ProductosClientes_ClientesFraccionesCalzado_ClienteFraccionC~` FOREIGN KEY (`ClienteFraccionCalzadoId`) REFERENCES `ClientesFraccionesCalzado` (`Id`) ON DELETE SET NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260409155251_CalzadoFraccionesPorCliente') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260409155251_CalzadoFraccionesPorCliente', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260409164056_PedidoDetalleTallaCalzado') THEN

    CREATE TABLE `PedidosDetalleTalla` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `PedidoDetalleId` char(36) COLLATE ascii_general_ci NOT NULL,
        `ClienteFraccionCalzadoDetalleId` char(36) COLLATE ascii_general_ci NULL,
        `Talla` varchar(10) CHARACTER SET utf8mb4 NOT NULL,
        `Orden` int NOT NULL,
        `Cantidad` decimal(18,4) NOT NULL,
        `PorcentajeVariacion` decimal(9,4) NOT NULL,
        `GeneradaDesdeFraccion` tinyint(1) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_PedidosDetalleTalla` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_PedidosDetalleTalla_PedidoDetalles_PedidoDetalleId` FOREIGN KEY (`PedidoDetalleId`) REFERENCES `PedidoDetalles` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260409164056_PedidoDetalleTallaCalzado') THEN

    CREATE INDEX `IX_PedidosDetalleTalla_PedidoDetalleId` ON `PedidosDetalleTalla` (`PedidoDetalleId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260409164056_PedidoDetalleTallaCalzado') THEN

    CREATE INDEX `IX_PedidosDetalleTalla_PedidoDetalleId_Talla` ON `PedidosDetalleTalla` (`PedidoDetalleId`, `Talla`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260409164056_PedidoDetalleTallaCalzado') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260409164056_PedidoDetalleTallaCalzado', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260409164901_NotaEntregaDetalleTalla') THEN

    CREATE TABLE `NotasEntregaDetalleTalla` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `EmpresaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `NotaEntregaDetalleId` char(36) COLLATE ascii_general_ci NOT NULL,
        `PedidoDetalleTallaId` char(36) COLLATE ascii_general_ci NULL,
        `Talla` varchar(10) CHARACTER SET utf8mb4 NOT NULL,
        `Orden` int NOT NULL,
        `Cantidad` decimal(18,4) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_NotasEntregaDetalleTalla` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_NotasEntregaDetalleTalla_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `Empresas` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_NotasEntregaDetalleTalla_NotasEntregaDetalle_NotaEntregaDeta~` FOREIGN KEY (`NotaEntregaDetalleId`) REFERENCES `NotasEntregaDetalle` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260409164901_NotaEntregaDetalleTalla') THEN

    CREATE INDEX `IX_NotasEntregaDetalleTalla_EmpresaId` ON `NotasEntregaDetalleTalla` (`EmpresaId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260409164901_NotaEntregaDetalleTalla') THEN

    CREATE INDEX `IX_NotasEntregaDetalleTalla_NotaEntregaDetalleId` ON `NotasEntregaDetalleTalla` (`NotaEntregaDetalleId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260409164901_NotaEntregaDetalleTalla') THEN

    CREATE INDEX `IX_NotasEntregaDetalleTalla_NotaEntregaDetalleId_Talla` ON `NotasEntregaDetalleTalla` (`NotaEntregaDetalleId`, `Talla`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260409164901_NotaEntregaDetalleTalla') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260409164901_NotaEntregaDetalleTalla', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260409170745_NotaEntregaPdfUrl') THEN

    ALTER TABLE `NotasEntrega` ADD `PdfUrl` varchar(500) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260409170745_NotaEntregaPdfUrl') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260409170745_NotaEntregaPdfUrl', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260409180132_ConfiguracionStorageAdjuntos') THEN

    CREATE TABLE `EmpresasStorageConfiguracion` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `EmpresaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `UsaConfiguracionGlobal` tinyint(1) NOT NULL,
        `StorageProvider` int NOT NULL,
        `BasePath` varchar(1000) CHARACTER SET utf8mb4 NULL,
        `PathTemplate` varchar(500) CHARACTER SET utf8mb4 NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_EmpresasStorageConfiguracion` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_EmpresasStorageConfiguracion_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `Empresas` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260409180132_ConfiguracionStorageAdjuntos') THEN

    CREATE TABLE `StorageConfiguracionesGlobales` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `StorageProvider` int NOT NULL,
        `BasePath` varchar(1000) CHARACTER SET utf8mb4 NULL,
        `PathTemplate` varchar(500) CHARACTER SET utf8mb4 NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_StorageConfiguracionesGlobales` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260409180132_ConfiguracionStorageAdjuntos') THEN

    CREATE UNIQUE INDEX `IX_EmpresasStorageConfiguracion_EmpresaId` ON `EmpresasStorageConfiguracion` (`EmpresaId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260409180132_ConfiguracionStorageAdjuntos') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260409180132_ConfiguracionStorageAdjuntos', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260409232121_CatalogoTallasEmpresaCalzado') THEN

    CREATE TABLE `CatalogoTallasCalzado` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `EmpresaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Talla` varchar(10) CHARACTER SET utf8mb4 NOT NULL,
        `SistemaNumeracion` int NOT NULL,
        `Orden` int NOT NULL,
        `Activa` tinyint(1) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_CatalogoTallasCalzado` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_CatalogoTallasCalzado_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `Empresas` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260409232121_CatalogoTallasEmpresaCalzado') THEN

    CREATE INDEX `IX_CatalogoTallasCalzado_EmpresaId_Orden` ON `CatalogoTallasCalzado` (`EmpresaId`, `Orden`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260409232121_CatalogoTallasEmpresaCalzado') THEN

    CREATE UNIQUE INDEX `IX_CatalogoTallasCalzado_EmpresaId_SistemaNumeracion_Talla` ON `CatalogoTallasCalzado` (`EmpresaId`, `SistemaNumeracion`, `Talla`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260409232121_CatalogoTallasEmpresaCalzado') THEN


    INSERT INTO CatalogoTallasCalzado (Id, EmpresaId, Talla, SistemaNumeracion, Orden, Activa, CreatedAt, IsActive)
    SELECT UUID(), src.EmpresaId, src.Talla, 1, MIN(src.Orden), 1, UTC_TIMESTAMP(), 1
    FROM (
        SELECT EmpresaId, Talla, Orden
        FROM ClientesTallasCalzado
        WHERE IsActive = 1 AND Talla IS NOT NULL AND Talla <> ''
        UNION ALL
        SELECT EmpresaId, Talla, Orden
        FROM ClientesFraccionesCalzadoDetalle
        WHERE IsActive = 1 AND Talla IS NOT NULL AND Talla <> ''
    ) AS src
    GROUP BY src.EmpresaId, src.Talla;


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260409232121_CatalogoTallasEmpresaCalzado') THEN

    ALTER TABLE `ClientesTallasCalzado` DROP INDEX `IX_ClientesTallasCalzado_ClienteId_Talla`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260409232121_CatalogoTallasEmpresaCalzado') THEN

    ALTER TABLE `ClientesTallasCalzado` ADD `CatalogoTallaCalzadoId` char(36) COLLATE ascii_general_ci NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260409232121_CatalogoTallasEmpresaCalzado') THEN

    ALTER TABLE `ClientesFraccionesCalzadoDetalle` ADD `CatalogoTallaCalzadoId` char(36) COLLATE ascii_general_ci NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260409232121_CatalogoTallasEmpresaCalzado') THEN


    UPDATE ClientesTallasCalzado ct
    INNER JOIN CatalogoTallasCalzado cat
        ON cat.EmpresaId = ct.EmpresaId
       AND cat.Talla = ct.Talla
       AND cat.SistemaNumeracion = 1
    SET ct.CatalogoTallaCalzadoId = cat.Id
    WHERE ct.IsActive = 1;


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260409232121_CatalogoTallasEmpresaCalzado') THEN


    UPDATE ClientesFraccionesCalzadoDetalle det
    INNER JOIN CatalogoTallasCalzado cat
        ON cat.EmpresaId = det.EmpresaId
       AND cat.Talla = det.Talla
       AND cat.SistemaNumeracion = 1
    SET det.CatalogoTallaCalzadoId = cat.Id
    WHERE det.IsActive = 1;


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260409232121_CatalogoTallasEmpresaCalzado') THEN

    ALTER TABLE `ClientesTallasCalzado` MODIFY COLUMN `CatalogoTallaCalzadoId` char(36) COLLATE ascii_general_ci NOT NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260409232121_CatalogoTallasEmpresaCalzado') THEN

    CREATE INDEX `IX_ClientesTallasCalzado_CatalogoTallaCalzadoId` ON `ClientesTallasCalzado` (`CatalogoTallaCalzadoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260409232121_CatalogoTallasEmpresaCalzado') THEN

    CREATE UNIQUE INDEX `IX_ClientesTallasCalzado_ClienteId_CatalogoTallaCalzadoId` ON `ClientesTallasCalzado` (`ClienteId`, `CatalogoTallaCalzadoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260409232121_CatalogoTallasEmpresaCalzado') THEN

    CREATE INDEX `IX_ClientesFraccionesCalzadoDetalle_CatalogoTallaCalzadoId` ON `ClientesFraccionesCalzadoDetalle` (`CatalogoTallaCalzadoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260409232121_CatalogoTallasEmpresaCalzado') THEN

    ALTER TABLE `ClientesFraccionesCalzadoDetalle` ADD CONSTRAINT `FK_ClientesFraccionesCalzadoDetalle_CatalogoTallasCalzado_Catalo` FOREIGN KEY (`CatalogoTallaCalzadoId`) REFERENCES `CatalogoTallasCalzado` (`Id`) ON DELETE SET NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260409232121_CatalogoTallasEmpresaCalzado') THEN

    ALTER TABLE `ClientesTallasCalzado` ADD CONSTRAINT `FK_ClientesTallasCalzado_CatalogoTallasCalzado_CatalogoTallaCalz` FOREIGN KEY (`CatalogoTallaCalzadoId`) REFERENCES `CatalogoTallasCalzado` (`Id`) ON DELETE CASCADE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260409232121_CatalogoTallasEmpresaCalzado') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260409232121_CatalogoTallasEmpresaCalzado', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260410170259_NominaPrenominaImssVacaciones') THEN

    ALTER TABLE `Nominas` ADD `PrenominaId` char(36) COLLATE ascii_general_ci NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260410170259_NominaPrenominaImssVacaciones') THEN

    ALTER TABLE `NominaDetalles` ADD `AplicaImss` tinyint(1) NOT NULL DEFAULT FALSE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260410170259_NominaPrenominaImssVacaciones') THEN

    ALTER TABLE `NominaDetalles` ADD `ComplementoSalarioMinimo` decimal(18,2) NOT NULL DEFAULT 0.0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260410170259_NominaPrenominaImssVacaciones') THEN

    ALTER TABLE `NominaDetalles` ADD `CuotaImssObrera` decimal(18,2) NOT NULL DEFAULT 0.0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260410170259_NominaPrenominaImssVacaciones') THEN

    ALTER TABLE `NominaDetalles` ADD `CuotaImssPatronal` decimal(18,2) NOT NULL DEFAULT 0.0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260410170259_NominaPrenominaImssVacaciones') THEN

    ALTER TABLE `NominaDetalles` ADD `DiasDescansoTrabajado` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260410170259_NominaPrenominaImssVacaciones') THEN

    ALTER TABLE `NominaDetalles` ADD `DiasFaltaInjustificada` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260410170259_NominaPrenominaImssVacaciones') THEN

    ALTER TABLE `NominaDetalles` ADD `DiasFaltaJustificada` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260410170259_NominaPrenominaImssVacaciones') THEN

    ALTER TABLE `NominaDetalles` ADD `DiasFestivoTrabajado` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260410170259_NominaPrenominaImssVacaciones') THEN

    ALTER TABLE `NominaDetalles` ADD `DiasIncapacidad` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260410170259_NominaPrenominaImssVacaciones') THEN

    ALTER TABLE `NominaDetalles` ADD `DiasPagados` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260410170259_NominaPrenominaImssVacaciones') THEN

    ALTER TABLE `NominaDetalles` ADD `DiasTrabajados` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260410170259_NominaPrenominaImssVacaciones') THEN

    ALTER TABLE `NominaDetalles` ADD `DiasVacaciones` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260410170259_NominaPrenominaImssVacaciones') THEN

    ALTER TABLE `NominaDetalles` ADD `MontoPrimaVacacional` decimal(18,2) NOT NULL DEFAULT 0.0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260410170259_NominaPrenominaImssVacaciones') THEN

    ALTER TABLE `Empleados` ADD `AplicaImss` tinyint(1) NOT NULL DEFAULT FALSE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260410170259_NominaPrenominaImssVacaciones') THEN

    CREATE TABLE `Prenominas` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `EmpresaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Periodo` varchar(80) CHARACTER SET utf8mb4 NOT NULL,
        `FechaInicio` datetime(6) NOT NULL,
        `FechaFin` datetime(6) NOT NULL,
        `Estatus` int NOT NULL,
        `Notas` varchar(500) CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_Prenominas` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_Prenominas_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `Empresas` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260410170259_NominaPrenominaImssVacaciones') THEN

    CREATE TABLE `PrenominaDetalles` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `PrenominaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `EmpleadoId` char(36) COLLATE ascii_general_ci NOT NULL,
        `DiasTrabajados` int NOT NULL,
        `DiasPagados` int NOT NULL,
        `DiasVacaciones` int NOT NULL,
        `DiasFaltaJustificada` int NOT NULL,
        `DiasFaltaInjustificada` int NOT NULL,
        `DiasIncapacidad` int NOT NULL,
        `DiasDescansoTrabajado` int NOT NULL,
        `DiasFestivoTrabajado` int NOT NULL,
        `AplicaImss` tinyint(1) NOT NULL,
        `HorasExtra` decimal(18,2) NOT NULL,
        `MontoDestajoInformativo` decimal(18,2) NOT NULL,
        `DiasVacacionesDisponibles` decimal(18,2) NOT NULL,
        `DiasVacacionesRestantes` decimal(18,2) NOT NULL,
        `ComplementoSalarioMinimoSugerido` decimal(18,2) NOT NULL,
        `Notas` varchar(500) CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_PrenominaDetalles` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_PrenominaDetalles_Empleados_EmpleadoId` FOREIGN KEY (`EmpleadoId`) REFERENCES `Empleados` (`Id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_PrenominaDetalles_Prenominas_PrenominaId` FOREIGN KEY (`PrenominaId`) REFERENCES `Prenominas` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260410170259_NominaPrenominaImssVacaciones') THEN

    CREATE UNIQUE INDEX `IX_Nominas_PrenominaId` ON `Nominas` (`PrenominaId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260410170259_NominaPrenominaImssVacaciones') THEN

    CREATE INDEX `IX_Empleados_AplicaImss` ON `Empleados` (`AplicaImss`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260410170259_NominaPrenominaImssVacaciones') THEN

    CREATE INDEX `IX_PrenominaDetalles_EmpleadoId` ON `PrenominaDetalles` (`EmpleadoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260410170259_NominaPrenominaImssVacaciones') THEN

    CREATE UNIQUE INDEX `IX_PrenominaDetalles_PrenominaId_EmpleadoId` ON `PrenominaDetalles` (`PrenominaId`, `EmpleadoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260410170259_NominaPrenominaImssVacaciones') THEN

    CREATE UNIQUE INDEX `IX_Prenominas_EmpresaId_FechaInicio_FechaFin` ON `Prenominas` (`EmpresaId`, `FechaInicio`, `FechaFin`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260410170259_NominaPrenominaImssVacaciones') THEN

    CREATE INDEX `IX_Prenominas_Estatus` ON `Prenominas` (`Estatus`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260410170259_NominaPrenominaImssVacaciones') THEN

    ALTER TABLE `Nominas` ADD CONSTRAINT `FK_Nominas_Prenominas_PrenominaId` FOREIGN KEY (`PrenominaId`) REFERENCES `Prenominas` (`Id`) ON DELETE SET NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260410170259_NominaPrenominaImssVacaciones') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260410170259_NominaPrenominaImssVacaciones', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260410183652_ProductoVariantesSku') THEN

    ALTER TABLE `Productos` ADD `TieneVariantes` tinyint(1) NOT NULL DEFAULT FALSE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260410183652_ProductoVariantesSku') THEN

    ALTER TABLE `Productos` ADD `UsaVariantesColor` tinyint(1) NOT NULL DEFAULT FALSE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260410183652_ProductoVariantesSku') THEN

    ALTER TABLE `Productos` ADD `UsaVariantesTalla` tinyint(1) NOT NULL DEFAULT FALSE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260410183652_ProductoVariantesSku') THEN

    CREATE TABLE `ProductosVariantes` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `EmpresaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `ProductoId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Sku` varchar(80) CHARACTER SET utf8mb4 NOT NULL,
        `Talla` varchar(20) CHARACTER SET utf8mb4 NULL,
        `Color` varchar(50) CHARACTER SET utf8mb4 NULL,
        `PrecioOverride` decimal(18,2) NULL,
        `Activa` tinyint(1) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_ProductosVariantes` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_ProductosVariantes_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `Empresas` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_ProductosVariantes_Productos_ProductoId` FOREIGN KEY (`ProductoId`) REFERENCES `Productos` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260410183652_ProductoVariantesSku') THEN

    CREATE UNIQUE INDEX `IX_ProductosVariantes_EmpresaId_Sku` ON `ProductosVariantes` (`EmpresaId`, `Sku`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260410183652_ProductoVariantesSku') THEN

    CREATE INDEX `IX_ProductosVariantes_ProductoId_Talla_Color` ON `ProductosVariantes` (`ProductoId`, `Talla`, `Color`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260410183652_ProductoVariantesSku') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260410183652_ProductoVariantesSku', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260410190712_PedidoVariantesSku') THEN

    ALTER TABLE `PedidosDetalleTalla` ADD `ProductoVarianteId` char(36) COLLATE ascii_general_ci NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260410190712_PedidoVariantesSku') THEN

    ALTER TABLE `PedidoDetalles` ADD `ProductoVarianteId` char(36) COLLATE ascii_general_ci NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260410190712_PedidoVariantesSku') THEN

    ALTER TABLE `PedidoDetalles` ADD `VariacionValor` varchar(50) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260410190712_PedidoVariantesSku') THEN

    CREATE INDEX `IX_PedidosDetalleTalla_ProductoVarianteId` ON `PedidosDetalleTalla` (`ProductoVarianteId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260410190712_PedidoVariantesSku') THEN

    CREATE INDEX `IX_PedidoDetalles_ProductoVarianteId` ON `PedidoDetalles` (`ProductoVarianteId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260410190712_PedidoVariantesSku') THEN

    ALTER TABLE `PedidoDetalles` ADD CONSTRAINT `FK_PedidoDetalles_ProductosVariantes_ProductoVarianteId` FOREIGN KEY (`ProductoVarianteId`) REFERENCES `ProductosVariantes` (`Id`) ON DELETE SET NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260410190712_PedidoVariantesSku') THEN

    ALTER TABLE `PedidosDetalleTalla` ADD CONSTRAINT `FK_PedidosDetalleTalla_ProductosVariantes_ProductoVarianteId` FOREIGN KEY (`ProductoVarianteId`) REFERENCES `ProductosVariantes` (`Id`) ON DELETE SET NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260410190712_PedidoVariantesSku') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260410190712_PedidoVariantesSku', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260410200111_ReglasVariacionClienteYCotizacionVariantePrecio') THEN

    ALTER TABLE `CotizacionesSerigrafia` ADD `ClienteId` char(36) COLLATE ascii_general_ci NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260410200111_ReglasVariacionClienteYCotizacionVariantePrecio') THEN

    CREATE TABLE `ClientesReglasVariacionPrecio` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `EmpresaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `ClienteId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Dimension` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `Valor` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `Orden` int NOT NULL,
        `PermiteVariacionPrecio` tinyint(1) NOT NULL,
        `PorcentajeVariacionSugerido` decimal(9,4) NOT NULL,
        `Activa` tinyint(1) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_ClientesReglasVariacionPrecio` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_ClientesReglasVariacionPrecio_Clientes_ClienteId` FOREIGN KEY (`ClienteId`) REFERENCES `Clientes` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_ClientesReglasVariacionPrecio_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `Empresas` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260410200111_ReglasVariacionClienteYCotizacionVariantePrecio') THEN

    CREATE TABLE `CotizacionVariantePrecios` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `CotizacionSerigrafiaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `ProductoVarianteId` char(36) COLLATE ascii_general_ci NULL,
        `Etiqueta` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
        `Talla` varchar(50) CHARACTER SET utf8mb4 NULL,
        `Color` varchar(50) CHARACTER SET utf8mb4 NULL,
        `EsPrecioBase` tinyint(1) NOT NULL,
        `PorcentajeVariacionAplicado` decimal(9,4) NOT NULL,
        `PrecioContado` decimal(18,2) NOT NULL,
        `PrecioCredito` decimal(18,2) NOT NULL,
        `Aceptada` tinyint(1) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_CotizacionVariantePrecios` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_CotizacionVariantePrecios_CotizacionesSerigrafia_CotizacionS~` FOREIGN KEY (`CotizacionSerigrafiaId`) REFERENCES `CotizacionesSerigrafia` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_CotizacionVariantePrecios_ProductosVariantes_ProductoVariant~` FOREIGN KEY (`ProductoVarianteId`) REFERENCES `ProductosVariantes` (`Id`) ON DELETE SET NULL
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260410200111_ReglasVariacionClienteYCotizacionVariantePrecio') THEN

    CREATE INDEX `IX_CotizacionesSerigrafia_ClienteId` ON `CotizacionesSerigrafia` (`ClienteId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260410200111_ReglasVariacionClienteYCotizacionVariantePrecio') THEN

    CREATE INDEX `IX_ClientesReglasVariacionPrecio_ClienteId` ON `ClientesReglasVariacionPrecio` (`ClienteId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260410200111_ReglasVariacionClienteYCotizacionVariantePrecio') THEN

    CREATE INDEX `IX_ClientesReglasVariacionPrecio_ClienteId_Dimension_Valor_Orden` ON `ClientesReglasVariacionPrecio` (`ClienteId`, `Dimension`, `Valor`, `Orden`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260410200111_ReglasVariacionClienteYCotizacionVariantePrecio') THEN

    CREATE INDEX `IX_ClientesReglasVariacionPrecio_EmpresaId` ON `ClientesReglasVariacionPrecio` (`EmpresaId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260410200111_ReglasVariacionClienteYCotizacionVariantePrecio') THEN

    CREATE INDEX `IX_CotizacionVariantePrecios_CotizacionSerigrafiaId` ON `CotizacionVariantePrecios` (`CotizacionSerigrafiaId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260410200111_ReglasVariacionClienteYCotizacionVariantePrecio') THEN

    CREATE INDEX `IX_CotizacionVariantePrecios_ProductoVarianteId` ON `CotizacionVariantePrecios` (`ProductoVarianteId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260410200111_ReglasVariacionClienteYCotizacionVariantePrecio') THEN

    ALTER TABLE `CotizacionesSerigrafia` ADD CONSTRAINT `FK_CotizacionesSerigrafia_Clientes_ClienteId` FOREIGN KEY (`ClienteId`) REFERENCES `Clientes` (`Id`) ON DELETE SET NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260410200111_ReglasVariacionClienteYCotizacionVariantePrecio') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260410200111_ReglasVariacionClienteYCotizacionVariantePrecio', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260410201552_SnapshotPrecioPedidoDetalleTalla') THEN

    ALTER TABLE `PedidosDetalleTalla` ADD `PrecioUnitario` decimal(18,4) NOT NULL DEFAULT 0.0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260410201552_SnapshotPrecioPedidoDetalleTalla') THEN

    ALTER TABLE `PedidosDetalleTalla` ADD `Total` decimal(18,2) NOT NULL DEFAULT 0.0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260410201552_SnapshotPrecioPedidoDetalleTalla') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260410201552_SnapshotPrecioPedidoDetalleTalla', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260411170540_InventarioFinishedGoodsInicial') THEN

    CREATE TABLE `InventariosFinishedGoods` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `EmpresaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `ClienteId` char(36) COLLATE ascii_general_ci NOT NULL,
        `ProductoId` char(36) COLLATE ascii_general_ci NOT NULL,
        `ProductoVarianteId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Sku` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `Talla` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `Color` varchar(50) CHARACTER SET utf8mb4 NULL,
        `CantidadDisponible` decimal(18,2) NOT NULL,
        `CantidadReservada` decimal(18,2) NOT NULL,
        `Ubicacion` varchar(100) CHARACTER SET utf8mb4 NULL,
        `Observaciones` varchar(500) CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_InventariosFinishedGoods` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_InventariosFinishedGoods_Clientes_ClienteId` FOREIGN KEY (`ClienteId`) REFERENCES `Clientes` (`Id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_InventariosFinishedGoods_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `Empresas` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_InventariosFinishedGoods_ProductosVariantes_ProductoVariante~` FOREIGN KEY (`ProductoVarianteId`) REFERENCES `ProductosVariantes` (`Id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_InventariosFinishedGoods_Productos_ProductoId` FOREIGN KEY (`ProductoId`) REFERENCES `Productos` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260411170540_InventarioFinishedGoodsInicial') THEN

    CREATE TABLE `MovimientosFinishedGoods` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `EmpresaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `InventarioFinishedGoodId` char(36) COLLATE ascii_general_ci NOT NULL,
        `TipoMovimiento` int NOT NULL,
        `Cantidad` decimal(18,2) NOT NULL,
        `ExistenciaAnterior` decimal(18,2) NOT NULL,
        `ExistenciaNueva` decimal(18,2) NOT NULL,
        `FechaMovimiento` datetime(6) NOT NULL,
        `Referencia` varchar(100) CHARACTER SET utf8mb4 NULL,
        `Observaciones` varchar(500) CHARACTER SET utf8mb4 NULL,
        `PedidoId` char(36) COLLATE ascii_general_ci NULL,
        `PedidoSerigrafiaId` char(36) COLLATE ascii_general_ci NULL,
        `PedidoDetalleTallaId` char(36) COLLATE ascii_general_ci NULL,
        `NotaEntregaId` char(36) COLLATE ascii_general_ci NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_MovimientosFinishedGoods` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_MovimientosFinishedGoods_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `Empresas` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_MovimientosFinishedGoods_InventariosFinishedGoods_Inventario~` FOREIGN KEY (`InventarioFinishedGoodId`) REFERENCES `InventariosFinishedGoods` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_MovimientosFinishedGoods_NotasEntrega_NotaEntregaId` FOREIGN KEY (`NotaEntregaId`) REFERENCES `NotasEntrega` (`Id`) ON DELETE SET NULL,
        CONSTRAINT `FK_MovimientosFinishedGoods_PedidosDetalleTalla_PedidoDetalleTa~` FOREIGN KEY (`PedidoDetalleTallaId`) REFERENCES `PedidosDetalleTalla` (`Id`) ON DELETE SET NULL,
        CONSTRAINT `FK_MovimientosFinishedGoods_PedidosSerigrafia_PedidoSerigrafiaId` FOREIGN KEY (`PedidoSerigrafiaId`) REFERENCES `PedidosSerigrafia` (`Id`) ON DELETE SET NULL,
        CONSTRAINT `FK_MovimientosFinishedGoods_Pedidos_PedidoId` FOREIGN KEY (`PedidoId`) REFERENCES `Pedidos` (`Id`) ON DELETE SET NULL
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260411170540_InventarioFinishedGoodsInicial') THEN

    CREATE INDEX `IX_InventariosFinishedGoods_ClienteId` ON `InventariosFinishedGoods` (`ClienteId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260411170540_InventarioFinishedGoodsInicial') THEN

    CREATE UNIQUE INDEX `IX_InventariosFinishedGoods_EmpresaId_ClienteId_ProductoVariant~` ON `InventariosFinishedGoods` (`EmpresaId`, `ClienteId`, `ProductoVarianteId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260411170540_InventarioFinishedGoodsInicial') THEN

    CREATE INDEX `IX_InventariosFinishedGoods_EmpresaId_Sku` ON `InventariosFinishedGoods` (`EmpresaId`, `Sku`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260411170540_InventarioFinishedGoodsInicial') THEN

    CREATE INDEX `IX_InventariosFinishedGoods_ProductoId` ON `InventariosFinishedGoods` (`ProductoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260411170540_InventarioFinishedGoodsInicial') THEN

    CREATE INDEX `IX_InventariosFinishedGoods_ProductoVarianteId` ON `InventariosFinishedGoods` (`ProductoVarianteId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260411170540_InventarioFinishedGoodsInicial') THEN

    CREATE INDEX `IX_MovimientosFinishedGoods_EmpresaId` ON `MovimientosFinishedGoods` (`EmpresaId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260411170540_InventarioFinishedGoodsInicial') THEN

    CREATE INDEX `IX_MovimientosFinishedGoods_FechaMovimiento` ON `MovimientosFinishedGoods` (`FechaMovimiento`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260411170540_InventarioFinishedGoodsInicial') THEN

    CREATE INDEX `IX_MovimientosFinishedGoods_InventarioFinishedGoodId` ON `MovimientosFinishedGoods` (`InventarioFinishedGoodId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260411170540_InventarioFinishedGoodsInicial') THEN

    CREATE INDEX `IX_MovimientosFinishedGoods_NotaEntregaId` ON `MovimientosFinishedGoods` (`NotaEntregaId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260411170540_InventarioFinishedGoodsInicial') THEN

    CREATE INDEX `IX_MovimientosFinishedGoods_PedidoDetalleTallaId` ON `MovimientosFinishedGoods` (`PedidoDetalleTallaId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260411170540_InventarioFinishedGoodsInicial') THEN

    CREATE INDEX `IX_MovimientosFinishedGoods_PedidoId` ON `MovimientosFinishedGoods` (`PedidoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260411170540_InventarioFinishedGoodsInicial') THEN

    CREATE INDEX `IX_MovimientosFinishedGoods_PedidoSerigrafiaId` ON `MovimientosFinishedGoods` (`PedidoSerigrafiaId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260411170540_InventarioFinishedGoodsInicial') THEN

    CREATE INDEX `IX_MovimientosFinishedGoods_TipoMovimiento` ON `MovimientosFinishedGoods` (`TipoMovimiento`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260411170540_InventarioFinishedGoodsInicial') THEN


    INSERT INTO Capacidades (Id, Clave, Nombre, Modulo, Descripcion)
    SELECT UUID(), 'inventario.fg.ver', 'Ver inventario FG', 'Inventario', 'Consulta del inventario de finished goods por cliente y SKU'
    WHERE NOT EXISTS (SELECT 1 FROM Capacidades WHERE Clave = 'inventario.fg.ver');

    INSERT INTO Capacidades (Id, Clave, Nombre, Modulo, Descripcion)
    SELECT UUID(), 'inventario.fg.ajustar', 'Ajustar inventario FG', 'Inventario', 'Ajustes manuales de finished goods sin registrar entregas desde la pantalla'
    WHERE NOT EXISTS (SELECT 1 FROM Capacidades WHERE Clave = 'inventario.fg.ajustar');

    INSERT INTO Capacidades (Id, Clave, Nombre, Modulo, Descripcion)
    SELECT UUID(), 'inventario.fg.ingresar', 'Ingresar a inventario FG', 'Inventario', 'Permite ingresar SKU completos desde producción a finished goods'
    WHERE NOT EXISTS (SELECT 1 FROM Capacidades WHERE Clave = 'inventario.fg.ingresar');

    INSERT INTO Capacidades (Id, Clave, Nombre, Modulo, Descripcion)
    SELECT UUID(), 'pedidos.produccion.reabrir', 'Reabrir producción cerrada', 'Pedidos', 'Permite editar checks y fechas de proceso en pedidos ya producidos o entregados'
    WHERE NOT EXISTS (SELECT 1 FROM Capacidades WHERE Clave = 'pedidos.produccion.reabrir');

    INSERT INTO TipoUsuarioCapacidades (TipoUsuarioId, CapacidadId)
    SELECT tu.Id, c.Id
    FROM TiposUsuario tu
    JOIN Capacidades c ON c.Clave IN ('inventario.fg.ver', 'inventario.fg.ajustar', 'inventario.fg.ingresar')
    WHERE tu.Nombre = 'Administrador'
    AND NOT EXISTS (
        SELECT 1 FROM TipoUsuarioCapacidades tuc
        WHERE tuc.TipoUsuarioId = tu.Id AND tuc.CapacidadId = c.Id
    );

    INSERT INTO TipoUsuarioCapacidades (TipoUsuarioId, CapacidadId)
    SELECT tu.Id, c.Id
    FROM TiposUsuario tu
    JOIN Capacidades c ON c.Clave IN ('inventario.fg.ver', 'inventario.fg.ajustar', 'inventario.fg.ingresar')
    WHERE tu.Nombre = 'Producción'
    AND NOT EXISTS (
        SELECT 1 FROM TipoUsuarioCapacidades tuc
        WHERE tuc.TipoUsuarioId = tu.Id AND tuc.CapacidadId = c.Id
    );

    INSERT INTO TipoUsuarioCapacidades (TipoUsuarioId, CapacidadId)
    SELECT tu.Id, c.Id
    FROM TiposUsuario tu
    JOIN Capacidades c ON c.Clave IN ('inventario.fg.ver', 'pedidos.produccion.reabrir')
    WHERE tu.Nombre = 'Gerente'
    AND NOT EXISTS (
        SELECT 1 FROM TipoUsuarioCapacidades tuc
        WHERE tuc.TipoUsuarioId = tu.Id AND tuc.CapacidadId = c.Id
    );

    INSERT INTO TipoUsuarioCapacidades (TipoUsuarioId, CapacidadId)
    SELECT tu.Id, c.Id
    FROM TiposUsuario tu
    JOIN Capacidades c ON c.Clave IN ('pedidos.produccion.reabrir')
    WHERE tu.Nombre = 'Administrador'
    AND NOT EXISTS (
        SELECT 1 FROM TipoUsuarioCapacidades tuc
        WHERE tuc.TipoUsuarioId = tu.Id AND tuc.CapacidadId = c.Id
    );

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260411170540_InventarioFinishedGoodsInicial') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260411170540_InventarioFinishedGoodsInicial', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260414160616_RrhhTurnosBase') THEN

    ALTER TABLE `Empleados` ADD `TurnoBaseId` char(36) COLLATE ascii_general_ci NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260414160616_RrhhTurnosBase') THEN

    CREATE TABLE `rrhh_turno` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `EmpresaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Nombre` varchar(120) CHARACTER SET utf8mb4 NOT NULL,
        `Descripcion` varchar(300) CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_rrhh_turno` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_rrhh_turno_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `Empresas` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260414160616_RrhhTurnosBase') THEN

    CREATE TABLE `rrhh_turno_detalle` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `TurnoBaseId` char(36) COLLATE ascii_general_ci NOT NULL,
        `DiaSemana` int NOT NULL,
        `Labora` tinyint(1) NOT NULL,
        `HoraEntrada` time(6) NULL,
        `HoraSalida` time(6) NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_rrhh_turno_detalle` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_rrhh_turno_detalle_rrhh_turno_TurnoBaseId` FOREIGN KEY (`TurnoBaseId`) REFERENCES `rrhh_turno` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260414160616_RrhhTurnosBase') THEN

    CREATE INDEX `IX_Empleados_TurnoBaseId` ON `Empleados` (`TurnoBaseId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260414160616_RrhhTurnosBase') THEN

    CREATE UNIQUE INDEX `IX_rrhh_turno_EmpresaId_Nombre` ON `rrhh_turno` (`EmpresaId`, `Nombre`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260414160616_RrhhTurnosBase') THEN

    CREATE UNIQUE INDEX `IX_rrhh_turno_detalle_TurnoBaseId_DiaSemana` ON `rrhh_turno_detalle` (`TurnoBaseId`, `DiaSemana`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260414160616_RrhhTurnosBase') THEN

    ALTER TABLE `Empleados` ADD CONSTRAINT `FK_Empleados_rrhh_turno_TurnoBaseId` FOREIGN KEY (`TurnoBaseId`) REFERENCES `rrhh_turno` (`Id`) ON DELETE SET NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260414160616_RrhhTurnosBase') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260414160616_RrhhTurnosBase', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260414171544_RrhhAsistenciaBase') THEN

    ALTER TABLE `Empleados` ADD `CodigoChecador` varchar(100) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260414171544_RrhhAsistenciaBase') THEN

    CREATE TABLE `rrhh_asistencia` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `EmpresaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `EmpleadoId` char(36) COLLATE ascii_general_ci NOT NULL,
        `TurnoBaseId` char(36) COLLATE ascii_general_ci NULL,
        `Fecha` date NOT NULL,
        `HoraEntradaProgramada` time(6) NULL,
        `HoraSalidaProgramada` time(6) NULL,
        `HoraEntradaReal` time(6) NULL,
        `HoraSalidaReal` time(6) NULL,
        `MinutosRetardo` int NOT NULL,
        `MinutosSalidaAnticipada` int NOT NULL,
        `MinutosExtra` int NOT NULL,
        `Estatus` int NOT NULL,
        `RequiereRevision` tinyint(1) NOT NULL,
        `Observaciones` varchar(500) CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_rrhh_asistencia` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_rrhh_asistencia_Empleados_EmpleadoId` FOREIGN KEY (`EmpleadoId`) REFERENCES `Empleados` (`Id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_rrhh_asistencia_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `Empresas` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_rrhh_asistencia_rrhh_turno_TurnoBaseId` FOREIGN KEY (`TurnoBaseId`) REFERENCES `rrhh_turno` (`Id`) ON DELETE SET NULL
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260414171544_RrhhAsistenciaBase') THEN

    CREATE TABLE `rrhh_checador` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `EmpresaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Nombre` varchar(120) CHARACTER SET utf8mb4 NOT NULL,
        `NumeroSerie` varchar(100) CHARACTER SET utf8mb4 NULL,
        `Marca` varchar(60) CHARACTER SET utf8mb4 NULL,
        `Modelo` varchar(60) CHARACTER SET utf8mb4 NULL,
        `Ip` varchar(50) CHARACTER SET utf8mb4 NULL,
        `Puerto` int NOT NULL,
        `NumeroMaquina` int NOT NULL,
        `Ubicacion` varchar(150) CHARACTER SET utf8mb4 NULL,
        `ZonaHoraria` varchar(60) CHARACTER SET utf8mb4 NULL,
        `UltimaSincronizacionUtc` datetime(6) NULL,
        `UltimoEventoLeido` varchar(200) CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_rrhh_checador` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_rrhh_checador_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `Empresas` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260414171544_RrhhAsistenciaBase') THEN

    CREATE TABLE `rrhh_logchecador` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `EmpresaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `ChecadorId` char(36) COLLATE ascii_general_ci NULL,
        `FechaUtc` datetime(6) NOT NULL,
        `Nivel` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `Mensaje` varchar(500) CHARACTER SET utf8mb4 NOT NULL,
        `Detalle` varchar(2000) CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_rrhh_logchecador` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_rrhh_logchecador_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `Empresas` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_rrhh_logchecador_rrhh_checador_ChecadorId` FOREIGN KEY (`ChecadorId`) REFERENCES `rrhh_checador` (`Id`) ON DELETE SET NULL
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260414171544_RrhhAsistenciaBase') THEN

    CREATE TABLE `rrhh_marcacion` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `EmpresaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `ChecadorId` char(36) COLLATE ascii_general_ci NOT NULL,
        `EmpleadoId` char(36) COLLATE ascii_general_ci NULL,
        `CodigoChecador` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `FechaHoraMarcacionUtc` datetime(6) NOT NULL,
        `TipoMarcacionRaw` varchar(50) CHARACTER SET utf8mb4 NULL,
        `Origen` varchar(50) CHARACTER SET utf8mb4 NULL,
        `EventoIdExterno` varchar(100) CHARACTER SET utf8mb4 NULL,
        `HashUnico` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `Procesada` tinyint(1) NOT NULL,
        `ResultadoProcesamiento` varchar(200) CHARACTER SET utf8mb4 NULL,
        `PayloadRaw` varchar(4000) CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_rrhh_marcacion` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_rrhh_marcacion_Empleados_EmpleadoId` FOREIGN KEY (`EmpleadoId`) REFERENCES `Empleados` (`Id`) ON DELETE SET NULL,
        CONSTRAINT `FK_rrhh_marcacion_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `Empresas` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_rrhh_marcacion_rrhh_checador_ChecadorId` FOREIGN KEY (`ChecadorId`) REFERENCES `rrhh_checador` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260414171544_RrhhAsistenciaBase') THEN

    CREATE UNIQUE INDEX `IX_Empleados_EmpresaId_CodigoChecador` ON `Empleados` (`EmpresaId`, `CodigoChecador`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260414171544_RrhhAsistenciaBase') THEN

    CREATE INDEX `IX_rrhh_asistencia_EmpleadoId` ON `rrhh_asistencia` (`EmpleadoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260414171544_RrhhAsistenciaBase') THEN

    CREATE UNIQUE INDEX `IX_rrhh_asistencia_EmpresaId_EmpleadoId_Fecha` ON `rrhh_asistencia` (`EmpresaId`, `EmpleadoId`, `Fecha`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260414171544_RrhhAsistenciaBase') THEN

    CREATE INDEX `IX_rrhh_asistencia_EmpresaId_Fecha_Estatus` ON `rrhh_asistencia` (`EmpresaId`, `Fecha`, `Estatus`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260414171544_RrhhAsistenciaBase') THEN

    CREATE INDEX `IX_rrhh_asistencia_TurnoBaseId` ON `rrhh_asistencia` (`TurnoBaseId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260414171544_RrhhAsistenciaBase') THEN

    CREATE INDEX `IX_rrhh_checador_EmpresaId_Nombre` ON `rrhh_checador` (`EmpresaId`, `Nombre`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260414171544_RrhhAsistenciaBase') THEN

    CREATE UNIQUE INDEX `IX_rrhh_checador_EmpresaId_NumeroSerie` ON `rrhh_checador` (`EmpresaId`, `NumeroSerie`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260414171544_RrhhAsistenciaBase') THEN

    CREATE INDEX `IX_rrhh_logchecador_ChecadorId` ON `rrhh_logchecador` (`ChecadorId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260414171544_RrhhAsistenciaBase') THEN

    CREATE INDEX `IX_rrhh_logchecador_EmpresaId_FechaUtc` ON `rrhh_logchecador` (`EmpresaId`, `FechaUtc`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260414171544_RrhhAsistenciaBase') THEN

    CREATE INDEX `IX_rrhh_marcacion_ChecadorId_FechaHoraMarcacionUtc` ON `rrhh_marcacion` (`ChecadorId`, `FechaHoraMarcacionUtc`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260414171544_RrhhAsistenciaBase') THEN

    CREATE INDEX `IX_rrhh_marcacion_EmpleadoId` ON `rrhh_marcacion` (`EmpleadoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260414171544_RrhhAsistenciaBase') THEN

    CREATE INDEX `IX_rrhh_marcacion_EmpresaId_CodigoChecador_FechaHoraMarcacionUtc` ON `rrhh_marcacion` (`EmpresaId`, `CodigoChecador`, `FechaHoraMarcacionUtc`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260414171544_RrhhAsistenciaBase') THEN

    CREATE UNIQUE INDEX `IX_rrhh_marcacion_HashUnico` ON `rrhh_marcacion` (`HashUnico`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260414171544_RrhhAsistenciaBase') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260414171544_RrhhAsistenciaBase', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260414190702_AuthModulosEmpresa') THEN

    CREATE TABLE `auth_modulo` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `Clave` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `Nombre` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `Descripcion` varchar(200) CHARACTER SET utf8mb4 NULL,
        `Orden` int NOT NULL,
        `EsGlobal` tinyint(1) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_auth_modulo` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260414190702_AuthModulosEmpresa') THEN

    CREATE TABLE `auth_empresa_modulo` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `EmpresaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `ModuloAccesoId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Habilitado` tinyint(1) NOT NULL,
        `VigenteDesde` date NOT NULL,
        `VigenteHasta` date NULL,
        `Origen` int NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_auth_empresa_modulo` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_auth_empresa_modulo_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `Empresas` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_auth_empresa_modulo_auth_modulo_ModuloAccesoId` FOREIGN KEY (`ModuloAccesoId`) REFERENCES `auth_modulo` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260414190702_AuthModulosEmpresa') THEN

    ALTER TABLE `Capacidades` ADD `ModuloAccesoId` char(36) COLLATE ascii_general_ci NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260414190702_AuthModulosEmpresa') THEN


    INSERT INTO `auth_modulo` (`Id`, `Clave`, `Nombre`, `Descripcion`, `Orden`, `EsGlobal`, `CreatedAt`, `IsActive`)
    VALUES
    ('9D788036-58AC-4B52-99D8-6755C04E7982', 'plataforma', 'Plataforma', 'Administración global y multiempresa', 1, 1, UTC_TIMESTAMP(), 1),
    ('56BCE92E-D7A9-472E-B769-B317CC741F1D', 'administracion', 'Administración', 'Usuarios, roles y permisos por empresa', 2, 0, UTC_TIMESTAMP(), 1),
    ('FB8D5495-78EF-4D34-B37A-B955F909B4D6', 'catalogos', 'Catálogos', 'Clientes, productos y compras base', 10, 0, UTC_TIMESTAMP(), 1),
    ('1B15C33F-01C4-4C7B-9D13-436902995E3F', 'manufactura', 'Manufactura', 'Pedidos, cotizaciones y producción', 20, 0, UTC_TIMESTAMP(), 1),
    ('2E113DFA-8D39-493F-8A4E-09B19AAB9B87', 'logistica', 'Logística', 'Inventario, insumos y finished goods', 30, 0, UTC_TIMESTAMP(), 1),
    ('F9FB95D2-E6A3-4948-9558-C9ACD6E4A443', 'contabilidad', 'Contabilidad', 'Finanzas, facturación y cuentas por pagar', 40, 0, UTC_TIMESTAMP(), 1),
    ('BB759E51-4B79-4ACB-B503-4FCD3A8B8568', 'rrhh', 'Recursos Humanos', 'Empleados, prenómina y nómina', 50, 0, UTC_TIMESTAMP(), 1),
    ('B9AA1C62-4201-4B53-BE95-FDCC0CCF146E', 'configuracion', 'Configuración', 'Parámetros operativos por empresa', 60, 0, UTC_TIMESTAMP(), 1);

    UPDATE `Capacidades`
    SET `ModuloAccesoId` = CASE
        WHEN `Clave` LIKE 'empresas.%' OR LOWER(`Modulo`) = 'plataforma' THEN '9D788036-58AC-4B52-99D8-6755C04E7982'
        WHEN `Clave` LIKE 'usuarios.%' OR LOWER(`Modulo`) = 'usuarios' THEN '56BCE92E-D7A9-472E-B769-B317CC741F1D'
        WHEN LOWER(`Modulo`) IN ('clientes', 'productos', 'proveedores') THEN 'FB8D5495-78EF-4D34-B37A-B955F909B4D6'
        WHEN LOWER(`Modulo`) IN ('pedidos', 'cotizaciones', 'serigrafía', 'serigrafia') THEN '1B15C33F-01C4-4C7B-9D13-436902995E3F'
        WHEN LOWER(`Modulo`) = 'inventario' THEN '2E113DFA-8D39-493F-8A4E-09B19AAB9B87'
        WHEN LOWER(`Modulo`) IN ('contabilidad', 'facturación', 'facturacion', 'cuentas por pagar') THEN 'F9FB95D2-E6A3-4948-9558-C9ACD6E4A443'
        WHEN LOWER(`Modulo`) IN ('empleados', 'nóminas', 'nominas') THEN 'BB759E51-4B79-4ACB-B503-4FCD3A8B8568'
        WHEN LOWER(`Modulo`) = 'configuración' OR LOWER(`Modulo`) = 'configuracion' THEN 'B9AA1C62-4201-4B53-BE95-FDCC0CCF146E'
        ELSE '56BCE92E-D7A9-472E-B769-B317CC741F1D'
    END
    WHERE `ModuloAccesoId` IS NULL;

    INSERT INTO `auth_empresa_modulo` (`Id`, `EmpresaId`, `ModuloAccesoId`, `Habilitado`, `VigenteDesde`, `VigenteHasta`, `Origen`, `CreatedAt`, `IsActive`)
    SELECT UUID(), e.`Id`, m.`Id`, CASE WHEN m.`Clave` = 'plataforma' THEN 0 ELSE 1 END, UTC_DATE(), NULL, CASE WHEN m.`EsGlobal` = 1 THEN 1 ELSE 2 END, UTC_TIMESTAMP(), 1
    FROM `Empresas` e
    CROSS JOIN `auth_modulo` m;



    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260414190702_AuthModulosEmpresa') THEN

    ALTER TABLE `Capacidades` MODIFY COLUMN `ModuloAccesoId` char(36) COLLATE ascii_general_ci NOT NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260414190702_AuthModulosEmpresa') THEN

    CREATE INDEX `IX_Capacidades_ModuloAccesoId_Nombre` ON `Capacidades` (`ModuloAccesoId`, `Nombre`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260414190702_AuthModulosEmpresa') THEN

    CREATE INDEX `IX_auth_empresa_modulo_EmpresaId_Habilitado_VigenteDesde_Vigent~` ON `auth_empresa_modulo` (`EmpresaId`, `Habilitado`, `VigenteDesde`, `VigenteHasta`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260414190702_AuthModulosEmpresa') THEN

    CREATE UNIQUE INDEX `IX_auth_empresa_modulo_EmpresaId_ModuloAccesoId` ON `auth_empresa_modulo` (`EmpresaId`, `ModuloAccesoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260414190702_AuthModulosEmpresa') THEN

    CREATE INDEX `IX_auth_empresa_modulo_ModuloAccesoId` ON `auth_empresa_modulo` (`ModuloAccesoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260414190702_AuthModulosEmpresa') THEN

    CREATE UNIQUE INDEX `IX_auth_modulo_Clave` ON `auth_modulo` (`Clave`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260414190702_AuthModulosEmpresa') THEN

    CREATE INDEX `IX_auth_modulo_Orden_Nombre` ON `auth_modulo` (`Orden`, `Nombre`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260414190702_AuthModulosEmpresa') THEN

    ALTER TABLE `Capacidades` ADD CONSTRAINT `FK_Capacidades_auth_modulo_ModuloAccesoId` FOREIGN KEY (`ModuloAccesoId`) REFERENCES `auth_modulo` (`Id`) ON DELETE RESTRICT;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260414190702_AuthModulosEmpresa') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260414190702_AuthModulosEmpresa', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260414201120_EmpleadoNumeroEmpleado') THEN

    ALTER TABLE `Empleados` ADD `NumeroEmpleado` varchar(30) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260414201120_EmpleadoNumeroEmpleado') THEN


    UPDATE `Empleados`
    SET `NumeroEmpleado` = CASE
        WHEN `Codigo` IS NOT NULL AND TRIM(`Codigo`) <> '' THEN LEFT(TRIM(`Codigo`), 30)
        ELSE CONCAT('EMP-', UPPER(LEFT(REPLACE(`Id`, '-', ''), 10)))
    END
    WHERE `NumeroEmpleado` IS NULL OR TRIM(`NumeroEmpleado`) = '';


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260414201120_EmpleadoNumeroEmpleado') THEN

    ALTER TABLE `Empleados` MODIFY COLUMN `NumeroEmpleado` varchar(30) NOT NULL DEFAULT '';

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260414201120_EmpleadoNumeroEmpleado') THEN

    CREATE UNIQUE INDEX `IX_Empleados_EmpresaId_NumeroEmpleado` ON `Empleados` (`EmpresaId`, `NumeroEmpleado`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260414201120_EmpleadoNumeroEmpleado') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260414201120_EmpleadoNumeroEmpleado', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260414203231_CodigosCortosYFoliosNegocio') THEN

    ALTER TABLE `Prenominas` ADD `Folio` varchar(30) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260414203231_CodigosCortosYFoliosNegocio') THEN

    ALTER TABLE `Nominas` ADD `Folio` varchar(30) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260414203231_CodigosCortosYFoliosNegocio') THEN


    UPDATE `Prenominas`
    SET `Folio` = CONCAT('PRE-', DATE_FORMAT(`FechaInicio`, '%Y%m'), '-', UPPER(LEFT(REPLACE(`Id`, '-', ''), 8)))
    WHERE `Folio` IS NULL OR TRIM(`Folio`) = '';


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260414203231_CodigosCortosYFoliosNegocio') THEN


    UPDATE `Nominas`
    SET `Folio` = CONCAT('NOM-', DATE_FORMAT(`FechaInicio`, '%Y%m'), '-', UPPER(LEFT(REPLACE(`Id`, '-', ''), 8)))
    WHERE `Folio` IS NULL OR TRIM(`Folio`) = '';


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260414203231_CodigosCortosYFoliosNegocio') THEN

    CREATE UNIQUE INDEX `IX_Nominas_EmpresaId_Folio` ON `Nominas` (`EmpresaId`, `Folio`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260414203231_CodigosCortosYFoliosNegocio') THEN

    CREATE UNIQUE INDEX `IX_Prenominas_EmpresaId_Folio` ON `Prenominas` (`EmpresaId`, `Folio`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260414203231_CodigosCortosYFoliosNegocio') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260414203231_CodigosCortosYFoliosNegocio', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260414204409_NominaDropEmpresaIndex') THEN


    DROP INDEX IF EXISTS `IX_Nominas_EmpresaId` ON `Nominas`;


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260414204409_NominaDropEmpresaIndex') THEN

    ALTER TABLE `Nominas` MODIFY COLUMN `Folio` varchar(30) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260414204409_NominaDropEmpresaIndex') THEN


    SET @idx_exists := (
        SELECT COUNT(1)
        FROM information_schema.statistics
        WHERE table_schema = DATABASE()
          AND table_name = 'Nominas'
          AND index_name = 'IX_Nominas_EmpresaId_Folio');
    SET @sql := IF(@idx_exists = 0,
        'CREATE UNIQUE INDEX `IX_Nominas_EmpresaId_Folio` ON `Nominas` (`EmpresaId`, `Folio`);',
        'SELECT 1;');
    PREPARE stmt FROM @sql;
    EXECUTE stmt;
    DEALLOCATE PREPARE stmt;


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260414204409_NominaDropEmpresaIndex') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260414204409_NominaDropEmpresaIndex', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260414213135_RrhhDepartamentosCatalogo') THEN

    CREATE TABLE `rrhh_departamento` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `EmpresaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Nombre` varchar(120) CHARACTER SET utf8mb4 NOT NULL,
        `Descripcion` varchar(300) CHARACTER SET utf8mb4 NULL,
        `Orden` int NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_rrhh_departamento` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_rrhh_departamento_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `Empresas` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260414213135_RrhhDepartamentosCatalogo') THEN

    CREATE UNIQUE INDEX `IX_rrhh_departamento_EmpresaId_Nombre` ON `rrhh_departamento` (`EmpresaId`, `Nombre`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260414213135_RrhhDepartamentosCatalogo') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260414213135_RrhhDepartamentosCatalogo', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260415181437_RrhhNominaCorteFestivos') THEN

    CREATE TABLE `rrhh_festivo` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `EmpresaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Fecha` date NOT NULL,
        `Nombre` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
        `Tipo` int NOT NULL,
        `AplicaPrimaEspecial` tinyint(1) NOT NULL,
        `FactorPago` decimal(8,4) NOT NULL,
        `EsOficial` tinyint(1) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_rrhh_festivo` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_rrhh_festivo_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `Empresas` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260415181437_RrhhNominaCorteFestivos') THEN

    CREATE TABLE `rrhh_nomina_corte` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `EmpresaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `PeriodicidadPago` int NOT NULL,
        `DiaCorteSemana` int NOT NULL,
        `DiaPagoSugerido` int NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_rrhh_nomina_corte` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_rrhh_nomina_corte_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `Empresas` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260415181437_RrhhNominaCorteFestivos') THEN

    CREATE UNIQUE INDEX `IX_rrhh_festivo_EmpresaId_Fecha` ON `rrhh_festivo` (`EmpresaId`, `Fecha`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260415181437_RrhhNominaCorteFestivos') THEN

    CREATE UNIQUE INDEX `IX_rrhh_nomina_corte_EmpresaId_PeriodicidadPago` ON `rrhh_nomina_corte` (`EmpresaId`, `PeriodicidadPago`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260415181437_RrhhNominaCorteFestivos') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260415181437_RrhhNominaCorteFestivos', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260415182450_RrhhBonosEstructurados') THEN

    CREATE TABLE `rrhh_bono_rubro` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `EmpresaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Clave` varchar(40) CHARACTER SET utf8mb4 NOT NULL,
        `Nombre` varchar(120) CHARACTER SET utf8mb4 NOT NULL,
        `Descripcion` varchar(300) CHARACTER SET utf8mb4 NULL,
        `Orden` int NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_rrhh_bono_rubro` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_rrhh_bono_rubro_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `Empresas` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260415182450_RrhhBonosEstructurados') THEN

    CREATE TABLE `rrhh_nomina_bono` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `EmpresaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `NominaDetalleId` char(36) COLLATE ascii_general_ci NOT NULL,
        `TipoCaptura` int NOT NULL,
        `MontoTotal` decimal(18,2) NOT NULL,
        `Observaciones` varchar(500) CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_rrhh_nomina_bono` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_rrhh_nomina_bono_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `Empresas` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_rrhh_nomina_bono_NominaDetalles_NominaDetalleId` FOREIGN KEY (`NominaDetalleId`) REFERENCES `NominaDetalles` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260415182450_RrhhBonosEstructurados') THEN

    CREATE TABLE `rrhh_nomina_bono_detalle` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `NominaBonoId` char(36) COLLATE ascii_general_ci NOT NULL,
        `BonoRubroRrhhId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Porcentaje` decimal(9,4) NOT NULL,
        `Importe` decimal(18,2) NOT NULL,
        `Orden` int NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_rrhh_nomina_bono_detalle` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_rrhh_nomina_bono_detalle_rrhh_bono_rubro_BonoRubroRrhhId` FOREIGN KEY (`BonoRubroRrhhId`) REFERENCES `rrhh_bono_rubro` (`Id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_rrhh_nomina_bono_detalle_rrhh_nomina_bono_NominaBonoId` FOREIGN KEY (`NominaBonoId`) REFERENCES `rrhh_nomina_bono` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260415182450_RrhhBonosEstructurados') THEN

    CREATE UNIQUE INDEX `IX_rrhh_bono_rubro_EmpresaId_Clave` ON `rrhh_bono_rubro` (`EmpresaId`, `Clave`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260415182450_RrhhBonosEstructurados') THEN

    CREATE UNIQUE INDEX `IX_rrhh_bono_rubro_EmpresaId_Nombre` ON `rrhh_bono_rubro` (`EmpresaId`, `Nombre`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260415182450_RrhhBonosEstructurados') THEN

    CREATE UNIQUE INDEX `IX_rrhh_nomina_bono_EmpresaId_NominaDetalleId` ON `rrhh_nomina_bono` (`EmpresaId`, `NominaDetalleId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260415182450_RrhhBonosEstructurados') THEN

    CREATE INDEX `IX_rrhh_nomina_bono_NominaDetalleId` ON `rrhh_nomina_bono` (`NominaDetalleId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260415182450_RrhhBonosEstructurados') THEN

    CREATE INDEX `IX_rrhh_nomina_bono_detalle_BonoRubroRrhhId` ON `rrhh_nomina_bono_detalle` (`BonoRubroRrhhId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260415182450_RrhhBonosEstructurados') THEN

    CREATE INDEX `IX_rrhh_nomina_bono_detalle_NominaBonoId` ON `rrhh_nomina_bono_detalle` (`NominaBonoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260415182450_RrhhBonosEstructurados') THEN

    CREATE UNIQUE INDEX `IX_rrhh_nomina_bono_detalle_NominaBonoId_BonoRubroRrhhId` ON `rrhh_nomina_bono_detalle` (`NominaBonoId`, `BonoRubroRrhhId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260415182450_RrhhBonosEstructurados') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260415182450_RrhhBonosEstructurados', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260415184524_RrhhPercepcionesManuales') THEN

    CREATE TABLE `rrhh_nomina_percepcion_tipo` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `EmpresaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Clave` varchar(40) CHARACTER SET utf8mb4 NOT NULL,
        `Nombre` varchar(120) CHARACTER SET utf8mb4 NOT NULL,
        `Categoria` int NOT NULL,
        `AfectaBaseImss` tinyint(1) NOT NULL,
        `AfectaBaseIsr` tinyint(1) NOT NULL,
        `Orden` int NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_rrhh_nomina_percepcion_tipo` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_rrhh_nomina_percepcion_tipo_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `Empresas` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260415184524_RrhhPercepcionesManuales') THEN

    CREATE TABLE `rrhh_nomina_percepcion` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `EmpresaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `NominaDetalleId` char(36) COLLATE ascii_general_ci NOT NULL,
        `TipoPercepcionId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Descripcion` varchar(250) CHARACTER SET utf8mb4 NULL,
        `Importe` decimal(18,2) NOT NULL,
        `Origen` int NOT NULL,
        `Referencia` varchar(120) CHARACTER SET utf8mb4 NULL,
        `Observaciones` varchar(500) CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_rrhh_nomina_percepcion` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_rrhh_nomina_percepcion_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `Empresas` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_rrhh_nomina_percepcion_NominaDetalles_NominaDetalleId` FOREIGN KEY (`NominaDetalleId`) REFERENCES `NominaDetalles` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_rrhh_nomina_percepcion_rrhh_nomina_percepcion_tipo_TipoPerce~` FOREIGN KEY (`TipoPercepcionId`) REFERENCES `rrhh_nomina_percepcion_tipo` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260415184524_RrhhPercepcionesManuales') THEN

    CREATE INDEX `IX_rrhh_nomina_percepcion_EmpresaId_NominaDetalleId` ON `rrhh_nomina_percepcion` (`EmpresaId`, `NominaDetalleId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260415184524_RrhhPercepcionesManuales') THEN

    CREATE INDEX `IX_rrhh_nomina_percepcion_NominaDetalleId` ON `rrhh_nomina_percepcion` (`NominaDetalleId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260415184524_RrhhPercepcionesManuales') THEN

    CREATE INDEX `IX_rrhh_nomina_percepcion_TipoPercepcionId` ON `rrhh_nomina_percepcion` (`TipoPercepcionId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260415184524_RrhhPercepcionesManuales') THEN

    CREATE UNIQUE INDEX `IX_rrhh_nomina_percepcion_tipo_EmpresaId_Clave` ON `rrhh_nomina_percepcion_tipo` (`EmpresaId`, `Clave`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260415184524_RrhhPercepcionesManuales') THEN

    CREATE UNIQUE INDEX `IX_rrhh_nomina_percepcion_tipo_EmpresaId_Nombre` ON `rrhh_nomina_percepcion_tipo` (`EmpresaId`, `Nombre`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260415184524_RrhhPercepcionesManuales') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260415184524_RrhhPercepcionesManuales', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260416135211_RrhhDeduccionesEstructuradas') THEN

    CREATE TABLE `rrhh_deduccion_tipo` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `EmpresaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Clave` varchar(40) CHARACTER SET utf8mb4 NOT NULL,
        `Nombre` varchar(120) CHARACTER SET utf8mb4 NOT NULL,
        `Descripcion` varchar(300) CHARACTER SET utf8mb4 NULL,
        `EsLegal` tinyint(1) NOT NULL,
        `AfectaRecibo` tinyint(1) NOT NULL,
        `Orden` int NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_rrhh_deduccion_tipo` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_rrhh_deduccion_tipo_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `Empresas` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260416135211_RrhhDeduccionesEstructuradas') THEN

    CREATE TABLE `rrhh_nomina_deduccion` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `EmpresaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `NominaDetalleId` char(36) COLLATE ascii_general_ci NOT NULL,
        `TipoDeduccionId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Descripcion` varchar(250) CHARACTER SET utf8mb4 NULL,
        `Importe` decimal(18,2) NOT NULL,
        `EsRetencionLegal` tinyint(1) NOT NULL,
        `Observaciones` varchar(500) CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_rrhh_nomina_deduccion` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_rrhh_nomina_deduccion_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `Empresas` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_rrhh_nomina_deduccion_NominaDetalles_NominaDetalleId` FOREIGN KEY (`NominaDetalleId`) REFERENCES `NominaDetalles` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_rrhh_nomina_deduccion_rrhh_deduccion_tipo_TipoDeduccionId` FOREIGN KEY (`TipoDeduccionId`) REFERENCES `rrhh_deduccion_tipo` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260416135211_RrhhDeduccionesEstructuradas') THEN

    CREATE UNIQUE INDEX `IX_rrhh_deduccion_tipo_EmpresaId_Clave` ON `rrhh_deduccion_tipo` (`EmpresaId`, `Clave`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260416135211_RrhhDeduccionesEstructuradas') THEN

    CREATE UNIQUE INDEX `IX_rrhh_deduccion_tipo_EmpresaId_Nombre` ON `rrhh_deduccion_tipo` (`EmpresaId`, `Nombre`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260416135211_RrhhDeduccionesEstructuradas') THEN

    CREATE INDEX `IX_rrhh_nomina_deduccion_EmpresaId_NominaDetalleId` ON `rrhh_nomina_deduccion` (`EmpresaId`, `NominaDetalleId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260416135211_RrhhDeduccionesEstructuradas') THEN

    CREATE INDEX `IX_rrhh_nomina_deduccion_NominaDetalleId` ON `rrhh_nomina_deduccion` (`NominaDetalleId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260416135211_RrhhDeduccionesEstructuradas') THEN

    CREATE INDEX `IX_rrhh_nomina_deduccion_TipoDeduccionId` ON `rrhh_nomina_deduccion` (`TipoDeduccionId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260416135211_RrhhDeduccionesEstructuradas') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260416135211_RrhhDeduccionesEstructuradas', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260416141830_RrhhLegalFestivosPrimaDominicalInfonavit') THEN

    ALTER TABLE `PrenominaDetalles` ADD `DiasDomingoTrabajado` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260416141830_RrhhLegalFestivosPrimaDominicalInfonavit') THEN

    ALTER TABLE `NominaDetalles` ADD `DiasDomingoTrabajado` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260416141830_RrhhLegalFestivosPrimaDominicalInfonavit') THEN

    ALTER TABLE `NominaDetalles` ADD `MontoFestivoTrabajado` decimal(18,2) NOT NULL DEFAULT 0.0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260416141830_RrhhLegalFestivosPrimaDominicalInfonavit') THEN

    ALTER TABLE `NominaDetalles` ADD `MontoInfonavit` decimal(18,2) NOT NULL DEFAULT 0.0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260416141830_RrhhLegalFestivosPrimaDominicalInfonavit') THEN

    ALTER TABLE `NominaDetalles` ADD `MontoPrimaDominical` decimal(18,2) NOT NULL DEFAULT 0.0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260416141830_RrhhLegalFestivosPrimaDominicalInfonavit') THEN

    ALTER TABLE `Empleados` ADD `AplicaInfonavit` tinyint(1) NOT NULL DEFAULT FALSE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260416141830_RrhhLegalFestivosPrimaDominicalInfonavit') THEN

    ALTER TABLE `Empleados` ADD `FactorDescuentoInfonavit` decimal(18,4) NOT NULL DEFAULT 0.0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260416141830_RrhhLegalFestivosPrimaDominicalInfonavit') THEN

    ALTER TABLE `Empleados` ADD `NumeroCreditoInfonavit` varchar(50) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260416141830_RrhhLegalFestivosPrimaDominicalInfonavit') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260416141830_RrhhLegalFestivosPrimaDominicalInfonavit', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260416174851_RrhhPrenominaCapturaRapida') THEN

    CREATE TABLE `rrhh_prenomina_bono` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `PrenominaDetalleId` char(36) COLLATE ascii_general_ci NOT NULL,
        `BonoRubroRrhhId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Importe` decimal(18,2) NOT NULL,
        `Observaciones` varchar(300) CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_rrhh_prenomina_bono` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_rrhh_prenomina_bono_PrenominaDetalles_PrenominaDetalleId` FOREIGN KEY (`PrenominaDetalleId`) REFERENCES `PrenominaDetalles` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_rrhh_prenomina_bono_rrhh_bono_rubro_BonoRubroRrhhId` FOREIGN KEY (`BonoRubroRrhhId`) REFERENCES `rrhh_bono_rubro` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260416174851_RrhhPrenominaCapturaRapida') THEN

    CREATE TABLE `rrhh_prenomina_percepcion` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `PrenominaDetalleId` char(36) COLLATE ascii_general_ci NOT NULL,
        `TipoPercepcionId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Importe` decimal(18,2) NOT NULL,
        `Referencia` varchar(120) CHARACTER SET utf8mb4 NULL,
        `Observaciones` varchar(300) CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_rrhh_prenomina_percepcion` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_rrhh_prenomina_percepcion_PrenominaDetalles_PrenominaDetalle~` FOREIGN KEY (`PrenominaDetalleId`) REFERENCES `PrenominaDetalles` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_rrhh_prenomina_percepcion_rrhh_nomina_percepcion_tipo_TipoPe~` FOREIGN KEY (`TipoPercepcionId`) REFERENCES `rrhh_nomina_percepcion_tipo` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260416174851_RrhhPrenominaCapturaRapida') THEN

    CREATE INDEX `IX_rrhh_prenomina_bono_BonoRubroRrhhId` ON `rrhh_prenomina_bono` (`BonoRubroRrhhId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260416174851_RrhhPrenominaCapturaRapida') THEN

    CREATE INDEX `IX_rrhh_prenomina_bono_PrenominaDetalleId` ON `rrhh_prenomina_bono` (`PrenominaDetalleId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260416174851_RrhhPrenominaCapturaRapida') THEN

    CREATE UNIQUE INDEX `IX_rrhh_prenomina_bono_PrenominaDetalleId_BonoRubroRrhhId` ON `rrhh_prenomina_bono` (`PrenominaDetalleId`, `BonoRubroRrhhId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260416174851_RrhhPrenominaCapturaRapida') THEN

    CREATE INDEX `IX_rrhh_prenomina_percepcion_PrenominaDetalleId` ON `rrhh_prenomina_percepcion` (`PrenominaDetalleId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260416174851_RrhhPrenominaCapturaRapida') THEN

    CREATE UNIQUE INDEX `IX_rrhh_prenomina_percepcion_PrenominaDetalleId_TipoPercepcionId` ON `rrhh_prenomina_percepcion` (`PrenominaDetalleId`, `TipoPercepcionId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260416174851_RrhhPrenominaCapturaRapida') THEN

    CREATE INDEX `IX_rrhh_prenomina_percepcion_TipoPercepcionId` ON `rrhh_prenomina_percepcion` (`TipoPercepcionId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260416174851_RrhhPrenominaCapturaRapida') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260416174851_RrhhPrenominaCapturaRapida', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260416183427_RrhhBonoEstructuraPorPosicion') THEN

    ALTER TABLE `Posiciones` DROP INDEX `IX_Posiciones_EmpresaId`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260416183427_RrhhBonoEstructuraPorPosicion') THEN

    ALTER TABLE `Posiciones` MODIFY COLUMN `Nombre` varchar(120) CHARACTER SET utf8mb4 NOT NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260416183427_RrhhBonoEstructuraPorPosicion') THEN

    ALTER TABLE `Posiciones` ADD `BonoEstructuraRrhhId` char(36) COLLATE ascii_general_ci NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260416183427_RrhhBonoEstructuraPorPosicion') THEN

    CREATE TABLE `rrhh_bono_estructura` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `EmpresaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Nombre` varchar(120) CHARACTER SET utf8mb4 NOT NULL,
        `Descripcion` varchar(300) CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_rrhh_bono_estructura` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_rrhh_bono_estructura_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `Empresas` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260416183427_RrhhBonoEstructuraPorPosicion') THEN

    CREATE TABLE `rrhh_bono_estructura_detalle` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `BonoEstructuraRrhhId` char(36) COLLATE ascii_general_ci NOT NULL,
        `BonoRubroRrhhId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Orden` int NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_rrhh_bono_estructura_detalle` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_rrhh_bono_estructura_detalle_rrhh_bono_estructura_BonoEstruc~` FOREIGN KEY (`BonoEstructuraRrhhId`) REFERENCES `rrhh_bono_estructura` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_rrhh_bono_estructura_detalle_rrhh_bono_rubro_BonoRubroRrhhId` FOREIGN KEY (`BonoRubroRrhhId`) REFERENCES `rrhh_bono_rubro` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260416183427_RrhhBonoEstructuraPorPosicion') THEN

    CREATE INDEX `IX_Posiciones_BonoEstructuraRrhhId` ON `Posiciones` (`BonoEstructuraRrhhId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260416183427_RrhhBonoEstructuraPorPosicion') THEN

    CREATE UNIQUE INDEX `IX_Posiciones_EmpresaId_Nombre` ON `Posiciones` (`EmpresaId`, `Nombre`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260416183427_RrhhBonoEstructuraPorPosicion') THEN

    CREATE UNIQUE INDEX `IX_rrhh_bono_estructura_EmpresaId_Nombre` ON `rrhh_bono_estructura` (`EmpresaId`, `Nombre`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260416183427_RrhhBonoEstructuraPorPosicion') THEN

    CREATE INDEX `IX_rrhh_bono_estructura_detalle_BonoEstructuraRrhhId` ON `rrhh_bono_estructura_detalle` (`BonoEstructuraRrhhId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260416183427_RrhhBonoEstructuraPorPosicion') THEN

    CREATE UNIQUE INDEX `IX_rrhh_bono_estructura_detalle_BonoEstructuraRrhhId_BonoRubroR~` ON `rrhh_bono_estructura_detalle` (`BonoEstructuraRrhhId`, `BonoRubroRrhhId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260416183427_RrhhBonoEstructuraPorPosicion') THEN

    CREATE INDEX `IX_rrhh_bono_estructura_detalle_BonoRubroRrhhId` ON `rrhh_bono_estructura_detalle` (`BonoRubroRrhhId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260416183427_RrhhBonoEstructuraPorPosicion') THEN

    ALTER TABLE `Posiciones` ADD CONSTRAINT `FK_Posiciones_rrhh_bono_estructura_BonoEstructuraRrhhId` FOREIGN KEY (`BonoEstructuraRrhhId`) REFERENCES `rrhh_bono_estructura` (`Id`) ON DELETE SET NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260416183427_RrhhBonoEstructuraPorPosicion') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260416183427_RrhhBonoEstructuraPorPosicion', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260416185716_RrhhBonoEstructuraTipoCaptura') THEN

    ALTER TABLE `rrhh_bono_estructura_detalle` ADD `Porcentaje` decimal(9,4) NOT NULL DEFAULT 0.0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260416185716_RrhhBonoEstructuraTipoCaptura') THEN

    ALTER TABLE `rrhh_bono_estructura` ADD `TipoCaptura` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260416185716_RrhhBonoEstructuraTipoCaptura') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260416185716_RrhhBonoEstructuraTipoCaptura', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260416193957_RrhhBonosPantallaMontoPosicion') THEN

    ALTER TABLE `Posiciones` ADD `MontoBonoDistribuido` decimal(18,2) NOT NULL DEFAULT 0.0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260416193957_RrhhBonosPantallaMontoPosicion') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260416193957_RrhhBonosPantallaMontoPosicion', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260416215427_RrhhBonosDistribuidosPeriodoEmpleado') THEN

    CREATE TABLE `rrhh_bono_distribucion_periodo` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `EmpresaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Periodo` varchar(80) CHARACTER SET utf8mb4 NOT NULL,
        `FechaInicio` datetime(6) NOT NULL,
        `FechaFin` datetime(6) NOT NULL,
        `Departamento` varchar(100) CHARACTER SET utf8mb4 NULL,
        `PosicionId` char(36) COLLATE ascii_general_ci NOT NULL,
        `BonoEstructuraRrhhId` char(36) COLLATE ascii_general_ci NOT NULL,
        `MontoTotalDistribuir` decimal(18,2) NOT NULL,
        `Observaciones` varchar(300) CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_rrhh_bono_distribucion_periodo` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_rrhh_bono_distribucion_periodo_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `Empresas` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_rrhh_bono_distribucion_periodo_Posiciones_PosicionId` FOREIGN KEY (`PosicionId`) REFERENCES `Posiciones` (`Id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_rrhh_bono_distribucion_periodo_rrhh_bono_estructura_BonoEstr~` FOREIGN KEY (`BonoEstructuraRrhhId`) REFERENCES `rrhh_bono_estructura` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260416215427_RrhhBonosDistribuidosPeriodoEmpleado') THEN

    CREATE TABLE `rrhh_bono_distribucion_empleado` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `BonoDistribucionPeriodoRrhhId` char(36) COLLATE ascii_general_ci NOT NULL,
        `EmpleadoId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Porcentaje` decimal(9,4) NOT NULL,
        `MontoAsignado` decimal(18,2) NOT NULL,
        `Observaciones` varchar(300) CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_rrhh_bono_distribucion_empleado` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_rrhh_bono_distribucion_empleado_Empleados_EmpleadoId` FOREIGN KEY (`EmpleadoId`) REFERENCES `Empleados` (`Id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_rrhh_bono_distribucion_empleado_rrhh_bono_distribucion_perio~` FOREIGN KEY (`BonoDistribucionPeriodoRrhhId`) REFERENCES `rrhh_bono_distribucion_periodo` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260416215427_RrhhBonosDistribuidosPeriodoEmpleado') THEN

    CREATE UNIQUE INDEX `IX_rrhh_bono_distribucion_empleado_BonoDistribucionPeriodoRrhhI~` ON `rrhh_bono_distribucion_empleado` (`BonoDistribucionPeriodoRrhhId`, `EmpleadoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260416215427_RrhhBonosDistribuidosPeriodoEmpleado') THEN

    CREATE INDEX `IX_rrhh_bono_distribucion_empleado_EmpleadoId` ON `rrhh_bono_distribucion_empleado` (`EmpleadoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260416215427_RrhhBonosDistribuidosPeriodoEmpleado') THEN

    CREATE INDEX `IX_rrhh_bono_distribucion_periodo_BonoEstructuraRrhhId` ON `rrhh_bono_distribucion_periodo` (`BonoEstructuraRrhhId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260416215427_RrhhBonosDistribuidosPeriodoEmpleado') THEN

    CREATE UNIQUE INDEX `IX_rrhh_bono_distribucion_periodo_EmpresaId_FechaInicio_FechaFi~` ON `rrhh_bono_distribucion_periodo` (`EmpresaId`, `FechaInicio`, `FechaFin`, `PosicionId`, `Departamento`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260416215427_RrhhBonosDistribuidosPeriodoEmpleado') THEN

    CREATE INDEX `IX_rrhh_bono_distribucion_periodo_PosicionId` ON `rrhh_bono_distribucion_periodo` (`PosicionId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260416215427_RrhhBonosDistribuidosPeriodoEmpleado') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260416215427_RrhhBonosDistribuidosPeriodoEmpleado', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260416224730_RrhhBonosDistribuidosDetalleRubro') THEN

    CREATE TABLE `rrhh_bono_distribucion_empleado_detalle` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `BonoDistribucionEmpleadoRrhhId` char(36) COLLATE ascii_general_ci NOT NULL,
        `BonoRubroRrhhId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Porcentaje` decimal(9,4) NOT NULL,
        `MontoAsignado` decimal(18,2) NOT NULL,
        `Orden` int NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_rrhh_bono_distribucion_empleado_detalle` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_rrhh_bono_distribucion_empleado_detalle_rrhh_bono_distribuci~` FOREIGN KEY (`BonoDistribucionEmpleadoRrhhId`) REFERENCES `rrhh_bono_distribucion_empleado` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_rrhh_bono_distribucion_empleado_detalle_rrhh_bono_rubro_Bono~` FOREIGN KEY (`BonoRubroRrhhId`) REFERENCES `rrhh_bono_rubro` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260416224730_RrhhBonosDistribuidosDetalleRubro') THEN

    CREATE UNIQUE INDEX `IX_rrhh_bono_distribucion_empleado_detalle_BonoDistribucionEmp~1` ON `rrhh_bono_distribucion_empleado_detalle` (`BonoDistribucionEmpleadoRrhhId`, `BonoRubroRrhhId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260416224730_RrhhBonosDistribuidosDetalleRubro') THEN

    CREATE INDEX `IX_rrhh_bono_distribucion_empleado_detalle_BonoDistribucionEmpl~` ON `rrhh_bono_distribucion_empleado_detalle` (`BonoDistribucionEmpleadoRrhhId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260416224730_RrhhBonosDistribuidosDetalleRubro') THEN

    CREATE INDEX `IX_rrhh_bono_distribucion_empleado_detalle_BonoRubroRrhhId` ON `rrhh_bono_distribucion_empleado_detalle` (`BonoRubroRrhhId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260416224730_RrhhBonosDistribuidosDetalleRubro') THEN


    INSERT INTO rrhh_bono_distribucion_empleado_detalle
        (Id, BonoDistribucionEmpleadoRrhhId, BonoRubroRrhhId, Porcentaje, MontoAsignado, Orden, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, IsActive)
    SELECT
        UUID(),
        e.Id,
        ed.BonoRubroRrhhId,
        ROUND(e.Porcentaje * (ed.Porcentaje / 100), 4),
        ROUND(e.MontoAsignado * (ed.Porcentaje / 100), 2),
        ed.Orden,
        e.CreatedAt,
        e.UpdatedAt,
        e.CreatedBy,
        e.UpdatedBy,
        e.IsActive
    FROM rrhh_bono_distribucion_empleado e
    INNER JOIN rrhh_bono_distribucion_periodo p ON p.Id = e.BonoDistribucionPeriodoRrhhId
    INNER JOIN rrhh_bono_estructura_detalle ed ON ed.BonoEstructuraRrhhId = p.BonoEstructuraRrhhId
    WHERE e.IsActive = 1 AND ed.IsActive = 1 AND e.Porcentaje > 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260416224730_RrhhBonosDistribuidosDetalleRubro') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260416224730_RrhhBonosDistribuidosDetalleRubro', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260417164059_RrhhEmpleadoTurnoVigencia') THEN

    CREATE TABLE `rrhh_empleado_turno` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `EmpresaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `EmpleadoId` char(36) COLLATE ascii_general_ci NOT NULL,
        `TurnoBaseId` char(36) COLLATE ascii_general_ci NOT NULL,
        `VigenteDesde` date NOT NULL,
        `VigenteHasta` date NULL,
        `Observaciones` varchar(500) CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_rrhh_empleado_turno` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_rrhh_empleado_turno_Empleados_EmpleadoId` FOREIGN KEY (`EmpleadoId`) REFERENCES `Empleados` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_rrhh_empleado_turno_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `Empresas` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_rrhh_empleado_turno_rrhh_turno_TurnoBaseId` FOREIGN KEY (`TurnoBaseId`) REFERENCES `rrhh_turno` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260417164059_RrhhEmpleadoTurnoVigencia') THEN

    CREATE INDEX `IX_rrhh_empleado_turno_EmpleadoId` ON `rrhh_empleado_turno` (`EmpleadoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260417164059_RrhhEmpleadoTurnoVigencia') THEN

    CREATE UNIQUE INDEX `IX_rrhh_empleado_turno_EmpresaId_EmpleadoId_VigenteDesde` ON `rrhh_empleado_turno` (`EmpresaId`, `EmpleadoId`, `VigenteDesde`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260417164059_RrhhEmpleadoTurnoVigencia') THEN

    CREATE INDEX `IX_rrhh_empleado_turno_EmpresaId_EmpleadoId_VigenteDesde_Vigent~` ON `rrhh_empleado_turno` (`EmpresaId`, `EmpleadoId`, `VigenteDesde`, `VigenteHasta`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260417164059_RrhhEmpleadoTurnoVigencia') THEN

    CREATE INDEX `IX_rrhh_empleado_turno_TurnoBaseId` ON `rrhh_empleado_turno` (`TurnoBaseId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260417164059_RrhhEmpleadoTurnoVigencia') THEN


    INSERT INTO `rrhh_empleado_turno`
        (`Id`, `EmpresaId`, `EmpleadoId`, `TurnoBaseId`, `VigenteDesde`, `VigenteHasta`, `Observaciones`, `CreatedAt`, `UpdatedAt`, `CreatedBy`, `UpdatedBy`, `IsActive`)
    SELECT
        UUID(),
        `EmpresaId`,
        `Id`,
        `TurnoBaseId`,
        COALESCE(DATE(`FechaContratacion`), DATE(`CreatedAt`), UTC_DATE()),
        NULL,
        'Migración inicial desde Empleado.TurnoBaseId',
        UTC_TIMESTAMP(6),
        NULL,
        'migration',
        NULL,
        1
    FROM `Empleados`
    WHERE `TurnoBaseId` IS NOT NULL;


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260417164059_RrhhEmpleadoTurnoVigencia') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260417164059_RrhhEmpleadoTurnoVigencia', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260417171735_RrhhEstadoAgenteMonitoreo') THEN

    CREATE TABLE `rrhh_estado_agente` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `EmpresaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `NombreAgente` varchar(120) CHARACTER SET utf8mb4 NOT NULL,
        `Hostname` varchar(120) CHARACTER SET utf8mb4 NULL,
        `Version` varchar(40) CHARACTER SET utf8mb4 NULL,
        `UltimoHeartbeatUtc` datetime(6) NULL,
        `UltimaEjecucionUtc` datetime(6) NULL,
        `MarcacionesLeidas` int NOT NULL,
        `MarcacionesEnviadas` int NOT NULL,
        `UltimoError` varchar(500) CHARACTER SET utf8mb4 NULL,
        `UltimoLogNivel` varchar(20) CHARACTER SET utf8mb4 NULL,
        `UltimoLogMensaje` varchar(500) CHARACTER SET utf8mb4 NULL,
        `UltimoLogDetalle` varchar(2000) CHARACTER SET utf8mb4 NULL,
        `UltimoLogUtc` datetime(6) NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_rrhh_estado_agente` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_rrhh_estado_agente_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `Empresas` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260417171735_RrhhEstadoAgenteMonitoreo') THEN

    CREATE UNIQUE INDEX `IX_rrhh_estado_agente_EmpresaId_NombreAgente` ON `rrhh_estado_agente` (`EmpresaId`, `NombreAgente`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260417171735_RrhhEstadoAgenteMonitoreo') THEN

    CREATE INDEX `IX_rrhh_estado_agente_EmpresaId_UltimoHeartbeatUtc` ON `rrhh_estado_agente` (`EmpresaId`, `UltimoHeartbeatUtc`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260417171735_RrhhEstadoAgenteMonitoreo') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260417171735_RrhhEstadoAgenteMonitoreo', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260417204234_RrhhAsistenciaDescansosTurno') THEN

    ALTER TABLE `rrhh_turno_detalle` ADD `CantidadDescansos` tinyint unsigned NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260417204234_RrhhAsistenciaDescansosTurno') THEN

    ALTER TABLE `rrhh_turno_detalle` ADD `Descanso1EsPagado` tinyint(1) NOT NULL DEFAULT FALSE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260417204234_RrhhAsistenciaDescansosTurno') THEN

    ALTER TABLE `rrhh_turno_detalle` ADD `Descanso1Fin` time(6) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260417204234_RrhhAsistenciaDescansosTurno') THEN

    ALTER TABLE `rrhh_turno_detalle` ADD `Descanso1Inicio` time(6) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260417204234_RrhhAsistenciaDescansosTurno') THEN

    ALTER TABLE `rrhh_turno_detalle` ADD `Descanso2EsPagado` tinyint(1) NOT NULL DEFAULT FALSE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260417204234_RrhhAsistenciaDescansosTurno') THEN

    ALTER TABLE `rrhh_turno_detalle` ADD `Descanso2Fin` time(6) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260417204234_RrhhAsistenciaDescansosTurno') THEN

    ALTER TABLE `rrhh_turno_detalle` ADD `Descanso2Inicio` time(6) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260417204234_RrhhAsistenciaDescansosTurno') THEN

    ALTER TABLE `rrhh_asistencia` ADD `MinutosDescansoNoPagado` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260417204234_RrhhAsistenciaDescansosTurno') THEN

    ALTER TABLE `rrhh_asistencia` ADD `MinutosDescansoPagado` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260417204234_RrhhAsistenciaDescansosTurno') THEN

    ALTER TABLE `rrhh_asistencia` ADD `MinutosDescansoProgramado` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260417204234_RrhhAsistenciaDescansosTurno') THEN

    ALTER TABLE `rrhh_asistencia` ADD `MinutosDescansoTomado` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260417204234_RrhhAsistenciaDescansosTurno') THEN

    ALTER TABLE `rrhh_asistencia` ADD `MinutosJornadaNetaProgramada` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260417204234_RrhhAsistenciaDescansosTurno') THEN

    ALTER TABLE `rrhh_asistencia` ADD `MinutosJornadaProgramada` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260417204234_RrhhAsistenciaDescansosTurno') THEN

    ALTER TABLE `rrhh_asistencia` ADD `MinutosTrabajadosBrutos` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260417204234_RrhhAsistenciaDescansosTurno') THEN

    ALTER TABLE `rrhh_asistencia` ADD `MinutosTrabajadosNetos` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260417204234_RrhhAsistenciaDescansosTurno') THEN

    ALTER TABLE `rrhh_asistencia` ADD `ResumenDescansos` varchar(1000) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260417204234_RrhhAsistenciaDescansosTurno') THEN

    ALTER TABLE `rrhh_asistencia` ADD `TotalMarcaciones` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260417204234_RrhhAsistenciaDescansosTurno') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260417204234_RrhhAsistenciaDescansosTurno', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260417211637_RrhhBancoHoras') THEN

    CREATE TABLE `rrhh_banco_horas_movimiento` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `EmpresaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `EmpleadoId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Fecha` date NOT NULL,
        `TipoMovimiento` int NOT NULL,
        `Horas` decimal(18,2) NOT NULL,
        `NominaDetalleId` char(36) COLLATE ascii_general_ci NULL,
        `ReferenciaTipo` varchar(80) CHARACTER SET utf8mb4 NULL,
        `Observaciones` varchar(500) CHARACTER SET utf8mb4 NULL,
        `EsAutomatico` tinyint(1) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_rrhh_banco_horas_movimiento` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_rrhh_banco_horas_movimiento_Empleados_EmpleadoId` FOREIGN KEY (`EmpleadoId`) REFERENCES `Empleados` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_rrhh_banco_horas_movimiento_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `Empresas` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_rrhh_banco_horas_movimiento_NominaDetalles_NominaDetalleId` FOREIGN KEY (`NominaDetalleId`) REFERENCES `NominaDetalles` (`Id`) ON DELETE SET NULL
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260417211637_RrhhBancoHoras') THEN

    CREATE INDEX `IX_rrhh_banco_horas_movimiento_EmpleadoId` ON `rrhh_banco_horas_movimiento` (`EmpleadoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260417211637_RrhhBancoHoras') THEN

    CREATE INDEX `IX_rrhh_banco_horas_movimiento_EmpresaId_EmpleadoId_Fecha` ON `rrhh_banco_horas_movimiento` (`EmpresaId`, `EmpleadoId`, `Fecha`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260417211637_RrhhBancoHoras') THEN

    CREATE INDEX `IX_rrhh_banco_horas_movimiento_EmpresaId_NominaDetalleId_TipoMo~` ON `rrhh_banco_horas_movimiento` (`EmpresaId`, `NominaDetalleId`, `TipoMovimiento`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260417211637_RrhhBancoHoras') THEN

    CREATE INDEX `IX_rrhh_banco_horas_movimiento_NominaDetalleId` ON `rrhh_banco_horas_movimiento` (`NominaDetalleId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260417211637_RrhhBancoHoras') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260417211637_RrhhBancoHoras', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260417212711_RrhhAusencias') THEN

    CREATE TABLE `rrhh_ausencia` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `EmpresaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `EmpleadoId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Tipo` int NOT NULL,
        `Estatus` int NOT NULL,
        `FechaInicio` date NOT NULL,
        `FechaFin` date NOT NULL,
        `Dias` int NOT NULL,
        `Horas` decimal(18,2) NOT NULL,
        `ConGocePago` tinyint(1) NOT NULL,
        `Motivo` varchar(250) CHARACTER SET utf8mb4 NULL,
        `Observaciones` varchar(500) CHARACTER SET utf8mb4 NULL,
        `FechaAprobacion` datetime(6) NULL,
        `AprobadoPor` varchar(120) CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_rrhh_ausencia` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_rrhh_ausencia_Empleados_EmpleadoId` FOREIGN KEY (`EmpleadoId`) REFERENCES `Empleados` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_rrhh_ausencia_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `Empresas` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260417212711_RrhhAusencias') THEN

    CREATE INDEX `IX_rrhh_ausencia_EmpleadoId` ON `rrhh_ausencia` (`EmpleadoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260417212711_RrhhAusencias') THEN

    CREATE INDEX `IX_rrhh_ausencia_EmpresaId_EmpleadoId_FechaInicio_FechaFin` ON `rrhh_ausencia` (`EmpresaId`, `EmpleadoId`, `FechaInicio`, `FechaFin`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260417212711_RrhhAusencias') THEN

    CREATE INDEX `IX_rrhh_ausencia_EmpresaId_Estatus_Tipo` ON `rrhh_ausencia` (`EmpresaId`, `Estatus`, `Tipo`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260417212711_RrhhAusencias') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260417212711_RrhhAusencias', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260418133030_RrhhMarcacionCorreccionOperativa') THEN

    ALTER TABLE `rrhh_marcacion` ADD `ClasificacionOperativa` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260418133030_RrhhMarcacionCorreccionOperativa') THEN

    ALTER TABLE `rrhh_marcacion` ADD `EsAnulada` tinyint(1) NOT NULL DEFAULT FALSE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260418133030_RrhhMarcacionCorreccionOperativa') THEN

    ALTER TABLE `rrhh_marcacion` ADD `EsManual` tinyint(1) NOT NULL DEFAULT FALSE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260418133030_RrhhMarcacionCorreccionOperativa') THEN

    ALTER TABLE `rrhh_marcacion` ADD `ObservacionManual` varchar(500) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260418133030_RrhhMarcacionCorreccionOperativa') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260418133030_RrhhMarcacionCorreccionOperativa', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260418135548_RrhhMarcacionesAuditoriaCapability') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260418135548_RrhhMarcacionesAuditoriaCapability', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260418151704_RrhhAsistenciaResolucionTiempoBanco') THEN

    ALTER TABLE `rrhh_asistencia` ADD `MinutosCubiertosBancoHoras` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260418151704_RrhhAsistenciaResolucionTiempoBanco') THEN

    ALTER TABLE `rrhh_asistencia` ADD `MinutosExtraAutorizadosBanco` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260418151704_RrhhAsistenciaResolucionTiempoBanco') THEN

    ALTER TABLE `rrhh_asistencia` ADD `MinutosExtraAutorizadosPago` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260418151704_RrhhAsistenciaResolucionTiempoBanco') THEN

    ALTER TABLE `rrhh_asistencia` ADD `ResolucionTiempoExtra` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260418151704_RrhhAsistenciaResolucionTiempoBanco') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260418151704_RrhhAsistenciaResolucionTiempoBanco', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260418152937_RrhhMarcacionResultadoProcesamientoLargo') THEN

    ALTER TABLE `rrhh_marcacion` MODIFY COLUMN `ResultadoProcesamiento` varchar(500) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260418152937_RrhhMarcacionResultadoProcesamientoLargo') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260418152937_RrhhMarcacionResultadoProcesamientoLargo', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260419045612_PrenominaDetalleSnapshotOperativo') THEN

    ALTER TABLE `PrenominaDetalles` ADD `FactorPagoTiempoExtra` decimal(18,4) NOT NULL DEFAULT 0.0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260419045612_PrenominaDetalleSnapshotOperativo') THEN

    ALTER TABLE `PrenominaDetalles` ADD `HorasBancoAcumuladas` decimal(18,2) NOT NULL DEFAULT 0.0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260419045612_PrenominaDetalleSnapshotOperativo') THEN

    ALTER TABLE `PrenominaDetalles` ADD `HorasBancoConsumidas` decimal(18,2) NOT NULL DEFAULT 0.0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260419045612_PrenominaDetalleSnapshotOperativo') THEN

    ALTER TABLE `PrenominaDetalles` ADD `HorasDescansoNoPagado` decimal(18,2) NOT NULL DEFAULT 0.0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260419045612_PrenominaDetalleSnapshotOperativo') THEN

    ALTER TABLE `PrenominaDetalles` ADD `HorasDescansoPagado` decimal(18,2) NOT NULL DEFAULT 0.0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260419045612_PrenominaDetalleSnapshotOperativo') THEN

    ALTER TABLE `PrenominaDetalles` ADD `HorasDescansoTomado` decimal(18,2) NOT NULL DEFAULT 0.0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260419045612_PrenominaDetalleSnapshotOperativo') THEN

    ALTER TABLE `PrenominaDetalles` ADD `HorasExtraBase` decimal(18,2) NOT NULL DEFAULT 0.0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260419045612_PrenominaDetalleSnapshotOperativo') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260419045612_PrenominaDetalleSnapshotOperativo', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260419061630_NominaHorasExtraLegalSplit') THEN

    ALTER TABLE `NominaDetalles` ADD `HorasExtraBanco` decimal(18,2) NOT NULL DEFAULT 0.0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260419061630_NominaHorasExtraLegalSplit') THEN

    ALTER TABLE `NominaDetalles` ADD `HorasExtraBase` decimal(18,2) NOT NULL DEFAULT 0.0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260419061630_NominaHorasExtraLegalSplit') THEN

    ALTER TABLE `NominaDetalles` ADD `HorasExtraDobles` decimal(18,2) NOT NULL DEFAULT 0.0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260419061630_NominaHorasExtraLegalSplit') THEN

    ALTER TABLE `NominaDetalles` ADD `HorasExtraTriples` decimal(18,2) NOT NULL DEFAULT 0.0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260419061630_NominaHorasExtraLegalSplit') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260419061630_NominaHorasExtraLegalSplit', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260419160827_NormalizarCatalogosSatNominaGlobal') THEN


    UPDATE rrhh_nomina_percepcion p
    INNER JOIN rrhh_nomina_percepcion_tipo t ON t.Id = p.TipoPercepcionId
    INNER JOIN (
        SELECT Clave, MIN(Id) AS CanonicoId
        FROM rrhh_nomina_percepcion_tipo
        GROUP BY Clave
    ) c ON c.Clave = t.Clave
    SET p.TipoPercepcionId = c.CanonicoId
    WHERE p.TipoPercepcionId <> c.CanonicoId;


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260419160827_NormalizarCatalogosSatNominaGlobal') THEN


    DELETE t1 FROM rrhh_nomina_percepcion_tipo t1
    INNER JOIN rrhh_nomina_percepcion_tipo t2
        ON t1.Clave = t2.Clave
        AND t1.Id > t2.Id;


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260419160827_NormalizarCatalogosSatNominaGlobal') THEN


    UPDATE rrhh_nomina_deduccion d
    INNER JOIN rrhh_deduccion_tipo t ON t.Id = d.TipoDeduccionId
    INNER JOIN (
        SELECT Clave, MIN(Id) AS CanonicoId
        FROM rrhh_deduccion_tipo
        GROUP BY Clave
    ) c ON c.Clave = t.Clave
    SET d.TipoDeduccionId = c.CanonicoId
    WHERE d.TipoDeduccionId <> c.CanonicoId;


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260419160827_NormalizarCatalogosSatNominaGlobal') THEN


    DELETE t1 FROM rrhh_deduccion_tipo t1
    INNER JOIN rrhh_deduccion_tipo t2
        ON t1.Clave = t2.Clave
        AND t1.Id > t2.Id;


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260419160827_NormalizarCatalogosSatNominaGlobal') THEN

    ALTER TABLE `rrhh_deduccion_tipo` DROP FOREIGN KEY `FK_rrhh_deduccion_tipo_Empresas_EmpresaId`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260419160827_NormalizarCatalogosSatNominaGlobal') THEN

    ALTER TABLE `rrhh_nomina_percepcion_tipo` DROP FOREIGN KEY `FK_rrhh_nomina_percepcion_tipo_Empresas_EmpresaId`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260419160827_NormalizarCatalogosSatNominaGlobal') THEN

    ALTER TABLE `rrhh_nomina_percepcion_tipo` DROP INDEX `IX_rrhh_nomina_percepcion_tipo_EmpresaId_Clave`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260419160827_NormalizarCatalogosSatNominaGlobal') THEN

    ALTER TABLE `rrhh_nomina_percepcion_tipo` DROP INDEX `IX_rrhh_nomina_percepcion_tipo_EmpresaId_Nombre`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260419160827_NormalizarCatalogosSatNominaGlobal') THEN

    ALTER TABLE `rrhh_deduccion_tipo` DROP INDEX `IX_rrhh_deduccion_tipo_EmpresaId_Clave`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260419160827_NormalizarCatalogosSatNominaGlobal') THEN

    ALTER TABLE `rrhh_deduccion_tipo` DROP INDEX `IX_rrhh_deduccion_tipo_EmpresaId_Nombre`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260419160827_NormalizarCatalogosSatNominaGlobal') THEN

    ALTER TABLE `rrhh_nomina_percepcion_tipo` DROP COLUMN `EmpresaId`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260419160827_NormalizarCatalogosSatNominaGlobal') THEN

    ALTER TABLE `rrhh_deduccion_tipo` DROP COLUMN `EmpresaId`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260419160827_NormalizarCatalogosSatNominaGlobal') THEN

    CREATE UNIQUE INDEX `IX_rrhh_nomina_percepcion_tipo_Clave` ON `rrhh_nomina_percepcion_tipo` (`Clave`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260419160827_NormalizarCatalogosSatNominaGlobal') THEN

    CREATE UNIQUE INDEX `IX_rrhh_nomina_percepcion_tipo_Nombre` ON `rrhh_nomina_percepcion_tipo` (`Nombre`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260419160827_NormalizarCatalogosSatNominaGlobal') THEN

    CREATE UNIQUE INDEX `IX_rrhh_deduccion_tipo_Clave` ON `rrhh_deduccion_tipo` (`Clave`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260419160827_NormalizarCatalogosSatNominaGlobal') THEN

    CREATE UNIQUE INDEX `IX_rrhh_deduccion_tipo_Nombre` ON `rrhh_deduccion_tipo` (`Nombre`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260419160827_NormalizarCatalogosSatNominaGlobal') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260419160827_NormalizarCatalogosSatNominaGlobal', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260420052146_RrhhPrenominaNominaMinutosYDescansoTrabajado') THEN

    ALTER TABLE `PrenominaDetalles` ADD `DiasConMarcacion` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260420052146_RrhhPrenominaNominaMinutosYDescansoTrabajado') THEN

    ALTER TABLE `PrenominaDetalles` ADD `MinutosDescuentoManual` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260420052146_RrhhPrenominaNominaMinutosYDescansoTrabajado') THEN

    ALTER TABLE `PrenominaDetalles` ADD `MinutosRetardo` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260420052146_RrhhPrenominaNominaMinutosYDescansoTrabajado') THEN

    ALTER TABLE `PrenominaDetalles` ADD `MinutosSalidaAnticipada` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260420052146_RrhhPrenominaNominaMinutosYDescansoTrabajado') THEN

    ALTER TABLE `NominaDetalles` ADD `DiasConMarcacion` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260420052146_RrhhPrenominaNominaMinutosYDescansoTrabajado') THEN

    ALTER TABLE `NominaDetalles` ADD `MinutosDescuentoManual` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260420052146_RrhhPrenominaNominaMinutosYDescansoTrabajado') THEN

    ALTER TABLE `NominaDetalles` ADD `MinutosRetardo` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260420052146_RrhhPrenominaNominaMinutosYDescansoTrabajado') THEN

    ALTER TABLE `NominaDetalles` ADD `MinutosSalidaAnticipada` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260420052146_RrhhPrenominaNominaMinutosYDescansoTrabajado') THEN

    ALTER TABLE `NominaDetalles` ADD `MontoDescansoTrabajado` decimal(65,30) NOT NULL DEFAULT 0.0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260420052146_RrhhPrenominaNominaMinutosYDescansoTrabajado') THEN

    ALTER TABLE `NominaDetalles` ADD `MontoDescuentoMinutos` decimal(65,30) NOT NULL DEFAULT 0.0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260420052146_RrhhPrenominaNominaMinutosYDescansoTrabajado') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260420052146_RrhhPrenominaNominaMinutosYDescansoTrabajado', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260420053429_PrenominaSnapshotConfiguracionYCierreInmutable') THEN

    ALTER TABLE `Prenominas` ADD `CerradaPor` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260420053429_PrenominaSnapshotConfiguracionYCierreInmutable') THEN

    ALTER TABLE `Prenominas` ADD `FechaCierre` datetime(6) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260420053429_PrenominaSnapshotConfiguracionYCierreInmutable') THEN

    ALTER TABLE `Prenominas` ADD `SnapshotConfiguracionJson` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260420053429_PrenominaSnapshotConfiguracionYCierreInmutable') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260420053429_PrenominaSnapshotConfiguracionYCierreInmutable', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260420182951_AgregarIsrSubsidioProvisionesZonaFrontera') THEN

    ALTER TABLE `NominaDetalles` ADD `AguinaldoProvision` decimal(65,30) NOT NULL DEFAULT 0.0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260420182951_AgregarIsrSubsidioProvisionesZonaFrontera') THEN

    ALTER TABLE `NominaDetalles` ADD `PrimaVacacionalProvision` decimal(65,30) NOT NULL DEFAULT 0.0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260420182951_AgregarIsrSubsidioProvisionesZonaFrontera') THEN

    ALTER TABLE `NominaDetalles` ADD `RetencionIsr` decimal(65,30) NOT NULL DEFAULT 0.0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260420182951_AgregarIsrSubsidioProvisionesZonaFrontera') THEN

    ALTER TABLE `NominaDetalles` ADD `SubsidioEmpleo` decimal(65,30) NOT NULL DEFAULT 0.0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260420182951_AgregarIsrSubsidioProvisionesZonaFrontera') THEN

    ALTER TABLE `Empresas` ADD `AplicaSalarioMinimoFrontera` tinyint(1) NOT NULL DEFAULT FALSE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260420182951_AgregarIsrSubsidioProvisionesZonaFrontera') THEN

    ALTER TABLE `Empleados` ADD `AplicaIsr` tinyint(1) NOT NULL DEFAULT FALSE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260420182951_AgregarIsrSubsidioProvisionesZonaFrontera') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260420182951_AgregarIsrSubsidioProvisionesZonaFrontera', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260420200817_AgregarNumeroNomina') THEN

    ALTER TABLE `Nominas` ADD `NumeroNomina` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260420200817_AgregarNumeroNomina') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260420200817_AgregarNumeroNomina', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260420211151_BlindarNominaPrenominaYNumeroNominaUnico') THEN

    ALTER TABLE `Nominas` MODIFY COLUMN `NumeroNomina` varchar(20) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260420211151_BlindarNominaPrenominaYNumeroNominaUnico') THEN

    CREATE UNIQUE INDEX `IX_Nominas_EmpresaId_Periodo_NumeroNomina` ON `Nominas` (`EmpresaId`, `Periodo`, `NumeroNomina`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260420211151_BlindarNominaPrenominaYNumeroNominaUnico') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260420211151_BlindarNominaPrenominaYNumeroNominaUnico', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260421195743_CreateNominaConfiguracionGlobalTable') THEN

    CREATE TABLE `rrhh_nomina_configuracion_global` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `UmaDiaria` decimal(65,30) NOT NULL,
        `SalarioMinimoGeneral` decimal(65,30) NOT NULL,
        `SalarioMinimoFrontera` decimal(65,30) NOT NULL,
        `TablaIsrJson` longtext CHARACTER SET utf8mb4 NOT NULL,
        `TablaSubsidioJson` longtext CHARACTER SET utf8mb4 NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_rrhh_nomina_configuracion_global` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260421195743_CreateNominaConfiguracionGlobalTable') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260421195743_CreateNominaConfiguracionGlobalTable', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260421212358_RrhhPeriodoAutomaticoNomina') THEN

    ALTER TABLE `rrhh_nomina_corte` ADD COLUMN IF NOT EXISTS `DiaCorteMes` int NOT NULL DEFAULT 15;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260421212358_RrhhPeriodoAutomaticoNomina') THEN

    ALTER TABLE `Prenominas` ADD COLUMN IF NOT EXISTS `AnioPeriodo` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260421212358_RrhhPeriodoAutomaticoNomina') THEN

    ALTER TABLE `Prenominas` ADD COLUMN IF NOT EXISTS `NumeroPeriodo` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260421212358_RrhhPeriodoAutomaticoNomina') THEN

    ALTER TABLE `Prenominas` ADD COLUMN IF NOT EXISTS `PeriodicidadPago` int NOT NULL DEFAULT 1;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260421212358_RrhhPeriodoAutomaticoNomina') THEN

    ALTER TABLE `Nominas` ADD COLUMN IF NOT EXISTS `AnioPeriodo` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260421212358_RrhhPeriodoAutomaticoNomina') THEN

    ALTER TABLE `Nominas` ADD COLUMN IF NOT EXISTS `NumeroPeriodo` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260421212358_RrhhPeriodoAutomaticoNomina') THEN

    ALTER TABLE `Nominas` ADD COLUMN IF NOT EXISTS `PeriodicidadPago` int NOT NULL DEFAULT 1;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260421212358_RrhhPeriodoAutomaticoNomina') THEN

    UPDATE Prenominas
    SET PeriodicidadPago = 1,
        AnioPeriodo = YEAR(FechaFin),
        NumeroPeriodo = CASE
            WHEN DAYOFYEAR(FechaFin) <= 7 THEN 1
            ELSE CEILING(DAYOFYEAR(FechaFin) / 7)
        END
    WHERE AnioPeriodo = 0 OR NumeroPeriodo = 0 OR PeriodicidadPago = 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260421212358_RrhhPeriodoAutomaticoNomina') THEN

    UPDATE Nominas
    SET PeriodicidadPago = 1,
        AnioPeriodo = YEAR(FechaFin),
        NumeroPeriodo = CASE
            WHEN DAYOFYEAR(FechaFin) <= 7 THEN 1
            ELSE CEILING(DAYOFYEAR(FechaFin) / 7)
        END
    WHERE AnioPeriodo = 0 OR NumeroPeriodo = 0 OR PeriodicidadPago = 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260421212358_RrhhPeriodoAutomaticoNomina') THEN

    CREATE UNIQUE INDEX `IX_Prenominas_EmpresaId_PeriodicidadPago_AnioPeriodo_NumeroPeri~` ON `Prenominas` (`EmpresaId`, `PeriodicidadPago`, `AnioPeriodo`, `NumeroPeriodo`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260421212358_RrhhPeriodoAutomaticoNomina') THEN

    CREATE UNIQUE INDEX `IX_Nominas_EmpresaId_PeriodicidadPago_AnioPeriodo_NumeroPeriodo` ON `Nominas` (`EmpresaId`, `PeriodicidadPago`, `AnioPeriodo`, `NumeroPeriodo`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260421212358_RrhhPeriodoAutomaticoNomina') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260421212358_RrhhPeriodoAutomaticoNomina', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260422103000_RrhhMarcacionHoraLocalPersistida') THEN

    ALTER TABLE rrhh_marcacion ADD COLUMN IF NOT EXISTS FechaHoraMarcacionLocal datetime(6) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260422103000_RrhhMarcacionHoraLocalPersistida') THEN

    ALTER TABLE rrhh_marcacion ADD COLUMN IF NOT EXISTS ZonaHorariaAplicada varchar(100) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260422103000_RrhhMarcacionHoraLocalPersistida') THEN

    UPDATE rrhh_marcacion m
    LEFT JOIN rrhh_checador c ON c.Id = m.ChecadorId
    SET m.ZonaHorariaAplicada = COALESCE(NULLIF(TRIM(m.ZonaHorariaAplicada), ''), NULLIF(TRIM(c.ZonaHoraria), ''), 'Central Standard Time (Mexico)')
    WHERE m.ZonaHorariaAplicada IS NULL OR TRIM(m.ZonaHorariaAplicada) = '';

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260422103000_RrhhMarcacionHoraLocalPersistida') THEN

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

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260422103000_RrhhMarcacionHoraLocalPersistida') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260422103000_RrhhMarcacionHoraLocalPersistida', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260428153000_RrhhConceptosConfigurablesYProvisionesManual') THEN

    CREATE TABLE `rrhh_nomina_concepto_config` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `EmpresaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Clave` varchar(40) CHARACTER SET utf8mb4 NOT NULL,
        `Nombre` varchar(120) CHARACTER SET utf8mb4 NOT NULL,
        `Naturaleza` int NOT NULL,
        `Destino` int NOT NULL,
        `TipoCalculo` int NOT NULL,
        `MontoFijoDefault` decimal(18,2) NOT NULL,
        `PorcentajeDefault` decimal(9,4) NOT NULL,
        `CantidadDefault` decimal(18,4) NOT NULL,
        `TarifaDefault` decimal(18,4) NOT NULL,
        `Orden` int NOT NULL,
        `EsRecurrente` tinyint(1) NOT NULL,
        `AplicaPorEmpleado` tinyint(1) NOT NULL,
        `AfectaNetoEmpleado` tinyint(1) NOT NULL,
        `AfectaCostoEmpresa` tinyint(1) NOT NULL,
        `AfectaPasivoSat` tinyint(1) NOT NULL,
        `AfectaPasivoImss` tinyint(1) NOT NULL,
        `AfectaProvision` tinyint(1) NOT NULL,
        `AfectaBaseIsr` tinyint(1) NOT NULL,
        `AfectaBaseImss` tinyint(1) NOT NULL,
        `EsLegal` tinyint(1) NOT NULL,
        `Observaciones` varchar(500) CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_rrhh_nomina_concepto_config` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_rrhh_nomina_concepto_config_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `Empresas` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260428153000_RrhhConceptosConfigurablesYProvisionesManual') THEN

    CREATE TABLE `rrhh_empleado_concepto` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `EmpresaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `EmpleadoId` char(36) COLLATE ascii_general_ci NOT NULL,
        `ConceptoConfigId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Monto` decimal(18,2) NOT NULL,
        `Porcentaje` decimal(9,4) NOT NULL,
        `Cantidad` decimal(18,4) NOT NULL,
        `Tarifa` decimal(18,4) NOT NULL,
        `Saldo` decimal(18,2) NOT NULL,
        `Limite` decimal(18,2) NOT NULL,
        `FechaInicio` date NULL,
        `FechaFin` date NULL,
        `EsRecurrente` tinyint(1) NOT NULL,
        `Observaciones` varchar(500) CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_rrhh_empleado_concepto` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_rrhh_empleado_concepto_Empleados_EmpleadoId` FOREIGN KEY (`EmpleadoId`) REFERENCES `Empleados` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_rrhh_empleado_concepto_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `Empresas` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_rrhh_empleado_concepto_rrhh_nomina_concepto_config_ConceptoCo` FOREIGN KEY (`ConceptoConfigId`) REFERENCES `rrhh_nomina_concepto_config` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260428153000_RrhhConceptosConfigurablesYProvisionesManual') THEN

    CREATE TABLE `rrhh_nomina_provision_detalle` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `EmpresaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `NominaDetalleId` char(36) COLLATE ascii_general_ci NOT NULL,
        `EmpleadoId` char(36) COLLATE ascii_general_ci NOT NULL,
        `ConceptoConfigId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Importe` decimal(18,2) NOT NULL,
        `BaseCalculo` decimal(18,2) NOT NULL,
        `Cantidad` decimal(18,4) NOT NULL,
        `Tarifa` decimal(18,4) NOT NULL,
        `PeriodoInicio` date NULL,
        `PeriodoFin` date NULL,
        `EsAjusteManual` tinyint(1) NOT NULL,
        `Observaciones` varchar(500) CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_rrhh_nomina_provision_detalle` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_rrhh_nomina_provision_detalle_Empleados_EmpleadoId` FOREIGN KEY (`EmpleadoId`) REFERENCES `Empleados` (`Id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_rrhh_nomina_provision_detalle_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `Empresas` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_rrhh_nomina_provision_detalle_NominaDetalles_NominaDetalleId` FOREIGN KEY (`NominaDetalleId`) REFERENCES `NominaDetalles` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_rrhh_nomina_provision_detalle_rrhh_nomina_concepto_config_Co~` FOREIGN KEY (`ConceptoConfigId`) REFERENCES `rrhh_nomina_concepto_config` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260428153000_RrhhConceptosConfigurablesYProvisionesManual') THEN

    CREATE INDEX `IX_rrhh_empleado_concepto_ConceptoConfigId` ON `rrhh_empleado_concepto` (`ConceptoConfigId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260428153000_RrhhConceptosConfigurablesYProvisionesManual') THEN

    CREATE INDEX `IX_rrhh_empleado_concepto_EmpleadoId` ON `rrhh_empleado_concepto` (`EmpleadoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260428153000_RrhhConceptosConfigurablesYProvisionesManual') THEN

    CREATE INDEX `IX_rrhh_empleado_concepto_EmpresaId_EmpleadoId_ConceptoConfigId_` ON `rrhh_empleado_concepto` (`EmpresaId`, `EmpleadoId`, `ConceptoConfigId`, `FechaInicio`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260428153000_RrhhConceptosConfigurablesYProvisionesManual') THEN

    CREATE INDEX `IX_rrhh_empleado_concepto_EmpresaId_EmpleadoId_IsActive` ON `rrhh_empleado_concepto` (`EmpresaId`, `EmpleadoId`, `IsActive`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260428153000_RrhhConceptosConfigurablesYProvisionesManual') THEN

    CREATE UNIQUE INDEX `IX_rrhh_nomina_concepto_config_EmpresaId_Clave` ON `rrhh_nomina_concepto_config` (`EmpresaId`, `Clave`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260428153000_RrhhConceptosConfigurablesYProvisionesManual') THEN

    CREATE INDEX `IX_rrhh_nomina_concepto_config_EmpresaId_Naturaleza_Destino` ON `rrhh_nomina_concepto_config` (`EmpresaId`, `Naturaleza`, `Destino`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260428153000_RrhhConceptosConfigurablesYProvisionesManual') THEN

    CREATE INDEX `IX_rrhh_nomina_concepto_config_EmpresaId_Nombre` ON `rrhh_nomina_concepto_config` (`EmpresaId`, `Nombre`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260428153000_RrhhConceptosConfigurablesYProvisionesManual') THEN

    CREATE INDEX `IX_rrhh_nomina_provision_detalle_ConceptoConfigId` ON `rrhh_nomina_provision_detalle` (`ConceptoConfigId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260428153000_RrhhConceptosConfigurablesYProvisionesManual') THEN

    CREATE INDEX `IX_rrhh_nomina_provision_detalle_EmpleadoId` ON `rrhh_nomina_provision_detalle` (`EmpleadoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260428153000_RrhhConceptosConfigurablesYProvisionesManual') THEN

    CREATE INDEX `IX_rrhh_nomina_provision_detalle_EmpresaId_EmpleadoId_PeriodoIni` ON `rrhh_nomina_provision_detalle` (`EmpresaId`, `EmpleadoId`, `PeriodoInicio`, `PeriodoFin`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260428153000_RrhhConceptosConfigurablesYProvisionesManual') THEN

    CREATE INDEX `IX_rrhh_nomina_provision_detalle_EmpresaId_NominaDetalleId_Conce` ON `rrhh_nomina_provision_detalle` (`EmpresaId`, `NominaDetalleId`, `ConceptoConfigId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260428153000_RrhhConceptosConfigurablesYProvisionesManual') THEN

    CREATE INDEX `IX_rrhh_nomina_provision_detalle_NominaDetalleId` ON `rrhh_nomina_provision_detalle` (`NominaDetalleId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260428153000_RrhhConceptosConfigurablesYProvisionesManual') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260428153000_RrhhConceptosConfigurablesYProvisionesManual', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260501173500_TipoProcesoConsumosEstandarYCotizacionAutomation') THEN

    ALTER TABLE `tiposproceso` ADD `GeneraConsumosAutomaticos` tinyint(1) NOT NULL DEFAULT FALSE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260501173500_TipoProcesoConsumosEstandarYCotizacionAutomation') THEN

    ALTER TABLE `tiposproceso` ADD `MinutosEstandar` decimal(18,2) NOT NULL DEFAULT 0.0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260501173500_TipoProcesoConsumosEstandarYCotizacionAutomation') THEN

    ALTER TABLE `tiposproceso` ADD `MultiplicadorDefault` decimal(18,2) NOT NULL DEFAULT 1.0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260501173500_TipoProcesoConsumosEstandarYCotizacionAutomation') THEN

    ALTER TABLE `tiposproceso` ADD `PermiteMultiplicador` tinyint(1) NOT NULL DEFAULT TRUE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260501173500_TipoProcesoConsumosEstandarYCotizacionAutomation') THEN

    ALTER TABLE `cotizacionserigrafiaprocesos` ADD `MinutosEstandarAplicados` decimal(18,2) NOT NULL DEFAULT 0.0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260501173500_TipoProcesoConsumosEstandarYCotizacionAutomation') THEN

    ALTER TABLE `cotizacionserigrafiaprocesos` ADD `Multiplicador` decimal(18,2) NOT NULL DEFAULT 1.0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260501173500_TipoProcesoConsumosEstandarYCotizacionAutomation') THEN

    ALTER TABLE `cotizacionserigrafiaprocesos` ADD `Orden` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260501173500_TipoProcesoConsumosEstandarYCotizacionAutomation') THEN

    ALTER TABLE `cotizacionserigrafiaprocesos` ADD `TiempoTotal` decimal(18,2) NOT NULL DEFAULT 0.0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260501173500_TipoProcesoConsumosEstandarYCotizacionAutomation') THEN

    ALTER TABLE `cotizaciondetalles` ADD `CotizacionSerigrafiaProcesoId` char(36) COLLATE ascii_general_ci NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260501173500_TipoProcesoConsumosEstandarYCotizacionAutomation') THEN

    ALTER TABLE `cotizaciondetalles` ADD `OrigenDetalle` int NOT NULL DEFAULT 1;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260501173500_TipoProcesoConsumosEstandarYCotizacionAutomation') THEN

    ALTER TABLE `cotizaciondetalles` ADD `TipoProcesoConsumoId` char(36) COLLATE ascii_general_ci NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260501173500_TipoProcesoConsumosEstandarYCotizacionAutomation') THEN


    CREATE TABLE `tiposprocesoconsumos` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `TipoProcesoId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Origen` int NOT NULL,
        `CategoriaCotizacion` int NOT NULL,
        `MateriaPrimaId` char(36) COLLATE ascii_general_ci NULL,
        `InsumoId` char(36) COLLATE ascii_general_ci NULL,
        `CantidadBase` decimal(18,4) NOT NULL,
        `Orden` int NOT NULL,
        `Activo` tinyint(1) NOT NULL,
        `Observaciones` varchar(300) NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext NULL,
        `UpdatedBy` longtext NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_tiposprocesoconsumos` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_tiposprocesoconsumos_tiposproceso_TipoProcesoId` FOREIGN KEY (`TipoProcesoId`) REFERENCES `tiposproceso` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_tiposprocesoconsumos_materiasprimas_MateriaPrimaId` FOREIGN KEY (`MateriaPrimaId`) REFERENCES `materiasprimas` (`Id`) ON DELETE SET NULL,
        CONSTRAINT `FK_tiposprocesoconsumos_insumos_InsumoId` FOREIGN KEY (`InsumoId`) REFERENCES `insumos` (`Id`) ON DELETE SET NULL
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260501173500_TipoProcesoConsumosEstandarYCotizacionAutomation') THEN

    CREATE INDEX `IX_cotizaciondetalles_CotizacionSerigrafiaProcesoId` ON `cotizaciondetalles` (`CotizacionSerigrafiaProcesoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260501173500_TipoProcesoConsumosEstandarYCotizacionAutomation') THEN

    CREATE INDEX `IX_cotizaciondetalles_TipoProcesoConsumoId` ON `cotizaciondetalles` (`TipoProcesoConsumoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260501173500_TipoProcesoConsumosEstandarYCotizacionAutomation') THEN

    CREATE INDEX `IX_tiposprocesoconsumos_TipoProcesoId` ON `tiposprocesoconsumos` (`TipoProcesoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260501173500_TipoProcesoConsumosEstandarYCotizacionAutomation') THEN

    CREATE INDEX `IX_tiposprocesoconsumos_TipoProcesoId_Orden` ON `tiposprocesoconsumos` (`TipoProcesoId`, `Orden`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260501173500_TipoProcesoConsumosEstandarYCotizacionAutomation') THEN

    CREATE INDEX `IX_tiposprocesoconsumos_MateriaPrimaId` ON `tiposprocesoconsumos` (`MateriaPrimaId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260501173500_TipoProcesoConsumosEstandarYCotizacionAutomation') THEN

    CREATE INDEX `IX_tiposprocesoconsumos_InsumoId` ON `tiposprocesoconsumos` (`InsumoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260501173500_TipoProcesoConsumosEstandarYCotizacionAutomation') THEN

    ALTER TABLE `cotizaciondetalles` ADD CONSTRAINT `FK_cotizaciondetalles_cotizacionserigrafiaprocesos_CotizacionSer` FOREIGN KEY (`CotizacionSerigrafiaProcesoId`) REFERENCES `cotizacionserigrafiaprocesos` (`Id`) ON DELETE SET NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260501173500_TipoProcesoConsumosEstandarYCotizacionAutomation') THEN

    ALTER TABLE `cotizaciondetalles` ADD CONSTRAINT `FK_cotizaciondetalles_tiposprocesoconsumos_TipoProcesoConsumoId` FOREIGN KEY (`TipoProcesoConsumoId`) REFERENCES `tiposprocesoconsumos` (`Id`) ON DELETE SET NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260501173500_TipoProcesoConsumosEstandarYCotizacionAutomation') THEN


    UPDATE `tiposproceso`
    SET `PermiteMultiplicador` = 1,
        `MultiplicadorDefault` = 1
    WHERE `MultiplicadorDefault` = 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260501173500_TipoProcesoConsumosEstandarYCotizacionAutomation') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260501173500_TipoProcesoConsumosEstandarYCotizacionAutomation', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260501194500_Fase1_InventarioItemUnificado') THEN

    DROP TABLE IF EXISTS logistica_inventario_item;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260501194500_Fase1_InventarioItemUnificado') THEN

    CREATE TABLE `logistica_inventario_item` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `EmpresaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Codigo` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `Nombre` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `CategoriaInventarioId` char(36) COLLATE ascii_general_ci NULL,
        `TipoInventarioId` char(36) COLLATE ascii_general_ci NULL,
        `OrigenLegacy` int NOT NULL,
        `MateriaPrimaOrigenId` char(36) COLLATE ascii_general_ci NULL,
        `InsumoOrigenId` char(36) COLLATE ascii_general_ci NULL,
        `CodigoPantone` varchar(20) CHARACTER SET utf8mb4 NULL,
        `CodigoHex` varchar(7) CHARACTER SET utf8mb4 NULL,
        `PrecioUnitario` decimal(18,4) NOT NULL,
        `Cantidad` decimal(18,2) NOT NULL,
        `UnidadMedida` varchar(10) CHARACTER SET utf8mb4 NOT NULL,
        `StockMinimo` decimal(18,2) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_logistica_inventario_item` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260501194500_Fase1_InventarioItemUnificado') THEN

    CREATE INDEX `IX_logistica_inventario_item_CategoriaInventarioId` ON `logistica_inventario_item` (`CategoriaInventarioId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260501194500_Fase1_InventarioItemUnificado') THEN

    CREATE UNIQUE INDEX `IX_logistica_inventario_item_EmpresaId_Codigo` ON `logistica_inventario_item` (`EmpresaId`, `Codigo`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260501194500_Fase1_InventarioItemUnificado') THEN

    CREATE UNIQUE INDEX `IX_logistica_inventario_item_InsumoOrigenId` ON `logistica_inventario_item` (`InsumoOrigenId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260501194500_Fase1_InventarioItemUnificado') THEN

    CREATE UNIQUE INDEX `IX_logistica_inventario_item_MateriaPrimaOrigenId` ON `logistica_inventario_item` (`MateriaPrimaOrigenId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260501194500_Fase1_InventarioItemUnificado') THEN

    CREATE INDEX `IX_logistica_inventario_item_TipoInventarioId` ON `logistica_inventario_item` (`TipoInventarioId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260501194500_Fase1_InventarioItemUnificado') THEN


    INSERT INTO logistica_inventario_item
        (Id, EmpresaId, Codigo, Nombre, CategoriaInventarioId, TipoInventarioId, OrigenLegacy, MateriaPrimaOrigenId, CodigoPantone, CodigoHex, PrecioUnitario, Cantidad, UnidadMedida, StockMinimo, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, IsActive)
    SELECT
        UUID(),
        mp.EmpresaId,
        mp.Codigo,
        mp.Nombre,
        ti.CategoriaInventarioId,
        mp.TipoInventarioId,
        1,
        mp.Id,
        mp.CodigoPantone,
        mp.CodigoHex,
        mp.PrecioUnitario,
        mp.Cantidad,
        mp.UnidadMedida,
        mp.StockMinimo,
        mp.CreatedAt,
        mp.UpdatedAt,
        mp.CreatedBy,
        mp.UpdatedBy,
        mp.IsActive
    FROM materiasprimas mp
    LEFT JOIN tiposinventario ti ON ti.Id = mp.TipoInventarioId
    WHERE NOT EXISTS (
        SELECT 1
        FROM logistica_inventario_item ii
        WHERE ii.MateriaPrimaOrigenId = mp.Id
    );

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260501194500_Fase1_InventarioItemUnificado') THEN


    INSERT INTO logistica_inventario_item
        (Id, EmpresaId, Codigo, Nombre, CategoriaInventarioId, TipoInventarioId, OrigenLegacy, InsumoOrigenId, PrecioUnitario, Cantidad, UnidadMedida, StockMinimo, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, IsActive)
    SELECT
        UUID(),
        i.EmpresaId,
        i.Codigo,
        i.Nombre,
        ti.CategoriaInventarioId,
        i.TipoInventarioId,
        2,
        i.Id,
        i.PrecioUnitario,
        i.Cantidad,
        i.UnidadMedida,
        i.StockMinimo,
        i.CreatedAt,
        i.UpdatedAt,
        i.CreatedBy,
        i.UpdatedBy,
        i.IsActive
    FROM insumos i
    LEFT JOIN tiposinventario ti ON ti.Id = i.TipoInventarioId
    WHERE NOT EXISTS (
        SELECT 1
        FROM logistica_inventario_item ii
        WHERE ii.InsumoOrigenId = i.Id
    );

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260501194500_Fase1_InventarioItemUnificado') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260501194500_Fase1_InventarioItemUnificado', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260501200500_Fase2_MovimientosInventarioItemCompat') THEN

    ALTER TABLE `movimientosinventario` ADD `InventarioItemId` char(36) COLLATE ascii_general_ci NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260501200500_Fase2_MovimientosInventarioItemCompat') THEN

    CREATE INDEX `IX_MovimientosInventario_InventarioItemId` ON `movimientosinventario` (`InventarioItemId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260501200500_Fase2_MovimientosInventarioItemCompat') THEN

    ALTER TABLE `movimientosinventario` ADD CONSTRAINT `FK_MovimientosInventario_logistica_inventario_item_InventarioIte` FOREIGN KEY (`InventarioItemId`) REFERENCES `logistica_inventario_item` (`Id`) ON DELETE RESTRICT;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260501200500_Fase2_MovimientosInventarioItemCompat') THEN


    UPDATE movimientosinventario m
    INNER JOIN logistica_inventario_item ii ON ii.MateriaPrimaOrigenId = m.MateriaPrimaId
    SET m.InventarioItemId = ii.Id
    WHERE m.MateriaPrimaId IS NOT NULL
      AND m.InventarioItemId IS NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260501200500_Fase2_MovimientosInventarioItemCompat') THEN


    UPDATE movimientosinventario m
    INNER JOIN logistica_inventario_item ii ON ii.InsumoOrigenId = m.InsumoId
    SET m.InventarioItemId = ii.Id
    WHERE m.InsumoId IS NOT NULL
      AND m.InventarioItemId IS NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260501200500_Fase2_MovimientosInventarioItemCompat') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260501200500_Fase2_MovimientosInventarioItemCompat', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260501212000_Fase3_ReferenciasFuncionalesInventarioItem') THEN

    ALTER TABLE `cotizaciondetalles` ADD `InventarioItemId` char(36) COLLATE ascii_general_ci NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260501212000_Fase3_ReferenciasFuncionalesInventarioItem') THEN

    ALTER TABLE `tiposprocesoconsumos` ADD `InventarioItemId` char(36) COLLATE ascii_general_ci NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260501212000_Fase3_ReferenciasFuncionalesInventarioItem') THEN

    CREATE INDEX `IX_cotizaciondetalles_InventarioItemId` ON `cotizaciondetalles` (`InventarioItemId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260501212000_Fase3_ReferenciasFuncionalesInventarioItem') THEN

    CREATE INDEX `IX_tiposprocesoconsumos_InventarioItemId` ON `tiposprocesoconsumos` (`InventarioItemId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260501212000_Fase3_ReferenciasFuncionalesInventarioItem') THEN

    ALTER TABLE `cotizaciondetalles` ADD CONSTRAINT `FK_cotizaciondetalles_logistica_inventario_item_InventarioItemId` FOREIGN KEY (`InventarioItemId`) REFERENCES `logistica_inventario_item` (`Id`) ON DELETE SET NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260501212000_Fase3_ReferenciasFuncionalesInventarioItem') THEN

    ALTER TABLE `tiposprocesoconsumos` ADD CONSTRAINT `FK_tiposprocesoconsumos_logistica_inventario_item_InventarioItem` FOREIGN KEY (`InventarioItemId`) REFERENCES `logistica_inventario_item` (`Id`) ON DELETE SET NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260501212000_Fase3_ReferenciasFuncionalesInventarioItem') THEN


    UPDATE cotizaciondetalles d
    INNER JOIN logistica_inventario_item ii ON ii.MateriaPrimaOrigenId = d.MateriaPrimaId
    SET d.InventarioItemId = ii.Id
    WHERE d.MateriaPrimaId IS NOT NULL
      AND d.InventarioItemId IS NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260501212000_Fase3_ReferenciasFuncionalesInventarioItem') THEN


    UPDATE cotizaciondetalles d
    INNER JOIN logistica_inventario_item ii ON ii.InsumoOrigenId = d.InsumoId
    SET d.InventarioItemId = ii.Id
    WHERE d.InsumoId IS NOT NULL
      AND d.InventarioItemId IS NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260501212000_Fase3_ReferenciasFuncionalesInventarioItem') THEN


    UPDATE tiposprocesoconsumos c
    INNER JOIN logistica_inventario_item ii ON ii.MateriaPrimaOrigenId = c.MateriaPrimaId
    SET c.InventarioItemId = ii.Id
    WHERE c.MateriaPrimaId IS NOT NULL
      AND c.InventarioItemId IS NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260501212000_Fase3_ReferenciasFuncionalesInventarioItem') THEN


    UPDATE tiposprocesoconsumos c
    INNER JOIN logistica_inventario_item ii ON ii.InsumoOrigenId = c.InsumoId
    SET c.InventarioItemId = ii.Id
    WHERE c.InsumoId IS NOT NULL
      AND c.InventarioItemId IS NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260501212000_Fase3_ReferenciasFuncionalesInventarioItem') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260501212000_Fase3_ReferenciasFuncionalesInventarioItem', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260501230258_20260502_RetiroTablasLegacyInventario') THEN


    SET @fk_name := (
        SELECT kcu.CONSTRAINT_NAME
        FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu
        WHERE kcu.TABLE_SCHEMA = DATABASE() AND kcu.TABLE_NAME = 'cotizaciondetalles' AND kcu.COLUMN_NAME = 'InsumoId' AND kcu.REFERENCED_TABLE_NAME IS NOT NULL
        LIMIT 1
    );
    SET @drop_fk_sql := IF(@fk_name IS NULL, 'SELECT 1', CONCAT('ALTER TABLE `cotizaciondetalles` DROP FOREIGN KEY `', @fk_name, '`'));
    PREPARE drop_fk_stmt FROM @drop_fk_sql;
    EXECUTE drop_fk_stmt;
    DEALLOCATE PREPARE drop_fk_stmt;

    SET @fk_name := (
        SELECT kcu.CONSTRAINT_NAME
        FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu
        WHERE kcu.TABLE_SCHEMA = DATABASE() AND kcu.TABLE_NAME = 'cotizaciondetalles' AND kcu.COLUMN_NAME = 'MateriaPrimaId' AND kcu.REFERENCED_TABLE_NAME IS NOT NULL
        LIMIT 1
    );
    SET @drop_fk_sql := IF(@fk_name IS NULL, 'SELECT 1', CONCAT('ALTER TABLE `cotizaciondetalles` DROP FOREIGN KEY `', @fk_name, '`'));
    PREPARE drop_fk_stmt FROM @drop_fk_sql;
    EXECUTE drop_fk_stmt;
    DEALLOCATE PREPARE drop_fk_stmt;

    SET @fk_name := (
        SELECT kcu.CONSTRAINT_NAME
        FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu
        WHERE kcu.TABLE_SCHEMA = DATABASE() AND kcu.TABLE_NAME = 'movimientosinventario' AND kcu.COLUMN_NAME = 'InsumoId' AND kcu.REFERENCED_TABLE_NAME IS NOT NULL
        LIMIT 1
    );
    SET @drop_fk_sql := IF(@fk_name IS NULL, 'SELECT 1', CONCAT('ALTER TABLE `movimientosinventario` DROP FOREIGN KEY `', @fk_name, '`'));
    PREPARE drop_fk_stmt FROM @drop_fk_sql;
    EXECUTE drop_fk_stmt;
    DEALLOCATE PREPARE drop_fk_stmt;

    SET @fk_name := (
        SELECT kcu.CONSTRAINT_NAME
        FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu
        WHERE kcu.TABLE_SCHEMA = DATABASE() AND kcu.TABLE_NAME = 'movimientosinventario' AND kcu.COLUMN_NAME = 'MateriaPrimaId' AND kcu.REFERENCED_TABLE_NAME IS NOT NULL
        LIMIT 1
    );
    SET @drop_fk_sql := IF(@fk_name IS NULL, 'SELECT 1', CONCAT('ALTER TABLE `movimientosinventario` DROP FOREIGN KEY `', @fk_name, '`'));
    PREPARE drop_fk_stmt FROM @drop_fk_sql;
    EXECUTE drop_fk_stmt;
    DEALLOCATE PREPARE drop_fk_stmt;

    SET @fk_name := (
        SELECT kcu.CONSTRAINT_NAME
        FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu
        WHERE kcu.TABLE_SCHEMA = DATABASE() AND kcu.TABLE_NAME = 'tiposprocesoconsumos' AND kcu.COLUMN_NAME = 'InsumoId' AND kcu.REFERENCED_TABLE_NAME IS NOT NULL
        LIMIT 1
    );
    SET @drop_fk_sql := IF(@fk_name IS NULL, 'SELECT 1', CONCAT('ALTER TABLE `tiposprocesoconsumos` DROP FOREIGN KEY `', @fk_name, '`'));
    PREPARE drop_fk_stmt FROM @drop_fk_sql;
    EXECUTE drop_fk_stmt;
    DEALLOCATE PREPARE drop_fk_stmt;

    SET @fk_name := (
        SELECT kcu.CONSTRAINT_NAME
        FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu
        WHERE kcu.TABLE_SCHEMA = DATABASE() AND kcu.TABLE_NAME = 'tiposprocesoconsumos' AND kcu.COLUMN_NAME = 'MateriaPrimaId' AND kcu.REFERENCED_TABLE_NAME IS NOT NULL
        LIMIT 1
    );
    SET @drop_fk_sql := IF(@fk_name IS NULL, 'SELECT 1', CONCAT('ALTER TABLE `tiposprocesoconsumos` DROP FOREIGN KEY `', @fk_name, '`'));
    PREPARE drop_fk_stmt FROM @drop_fk_sql;
    EXECUTE drop_fk_stmt;
    DEALLOCATE PREPARE drop_fk_stmt;

    SET @fk_name := (
        SELECT kcu.CONSTRAINT_NAME
        FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu
        WHERE kcu.TABLE_SCHEMA = DATABASE() AND kcu.TABLE_NAME = 'insumos' AND kcu.COLUMN_NAME = 'EmpresaId' AND kcu.REFERENCED_TABLE_NAME IS NOT NULL
        LIMIT 1
    );
    SET @drop_fk_sql := IF(@fk_name IS NULL, 'SELECT 1', CONCAT('ALTER TABLE `insumos` DROP FOREIGN KEY `', @fk_name, '`'));
    PREPARE drop_fk_stmt FROM @drop_fk_sql;
    EXECUTE drop_fk_stmt;
    DEALLOCATE PREPARE drop_fk_stmt;

    SET @fk_name := (
        SELECT kcu.CONSTRAINT_NAME
        FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu
        WHERE kcu.TABLE_SCHEMA = DATABASE() AND kcu.TABLE_NAME = 'insumos' AND kcu.COLUMN_NAME = 'TipoInventarioId' AND kcu.REFERENCED_TABLE_NAME IS NOT NULL
        LIMIT 1
    );
    SET @drop_fk_sql := IF(@fk_name IS NULL, 'SELECT 1', CONCAT('ALTER TABLE `insumos` DROP FOREIGN KEY `', @fk_name, '`'));
    PREPARE drop_fk_stmt FROM @drop_fk_sql;
    EXECUTE drop_fk_stmt;
    DEALLOCATE PREPARE drop_fk_stmt;


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260501230258_20260502_RetiroTablasLegacyInventario') THEN

    DROP TABLE IF EXISTS `materiasprimas`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260501230258_20260502_RetiroTablasLegacyInventario') THEN

    DROP TABLE IF EXISTS `insumos`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260501230258_20260502_RetiroTablasLegacyInventario') THEN


    SET @index_exists := (
        SELECT COUNT(*)
        FROM INFORMATION_SCHEMA.STATISTICS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = 'tiposprocesoconsumos'
          AND INDEX_NAME = 'IX_tiposprocesoconsumos_InsumoId'
    );
    SET @drop_index_sql := IF(
        @index_exists = 0,
        'SELECT 1',
        'ALTER TABLE `tiposprocesoconsumos` DROP INDEX `IX_tiposprocesoconsumos_InsumoId`'
    );
    PREPARE drop_index_stmt FROM @drop_index_sql;
    EXECUTE drop_index_stmt;
    DEALLOCATE PREPARE drop_index_stmt;


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260501230258_20260502_RetiroTablasLegacyInventario') THEN


    SET @index_exists := (
        SELECT COUNT(*)
        FROM INFORMATION_SCHEMA.STATISTICS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = 'tiposprocesoconsumos'
          AND INDEX_NAME = 'IX_tiposprocesoconsumos_MateriaPrimaId'
    );
    SET @drop_index_sql := IF(
        @index_exists = 0,
        'SELECT 1',
        'ALTER TABLE `tiposprocesoconsumos` DROP INDEX `IX_tiposprocesoconsumos_MateriaPrimaId`'
    );
    PREPARE drop_index_stmt FROM @drop_index_sql;
    EXECUTE drop_index_stmt;
    DEALLOCATE PREPARE drop_index_stmt;


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260501230258_20260502_RetiroTablasLegacyInventario') THEN


    SET @index_exists := (
        SELECT COUNT(*)
        FROM INFORMATION_SCHEMA.STATISTICS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = 'movimientosinventario'
          AND INDEX_NAME = 'IX_movimientosinventario_InsumoId'
    );
    SET @drop_index_sql := IF(
        @index_exists = 0,
        'SELECT 1',
        'ALTER TABLE `movimientosinventario` DROP INDEX `IX_movimientosinventario_InsumoId`'
    );
    PREPARE drop_index_stmt FROM @drop_index_sql;
    EXECUTE drop_index_stmt;
    DEALLOCATE PREPARE drop_index_stmt;


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260501230258_20260502_RetiroTablasLegacyInventario') THEN


    SET @index_exists := (
        SELECT COUNT(*)
        FROM INFORMATION_SCHEMA.STATISTICS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = 'movimientosinventario'
          AND INDEX_NAME = 'IX_movimientosinventario_MateriaPrimaId'
    );
    SET @drop_index_sql := IF(
        @index_exists = 0,
        'SELECT 1',
        'ALTER TABLE `movimientosinventario` DROP INDEX `IX_movimientosinventario_MateriaPrimaId`'
    );
    PREPARE drop_index_stmt FROM @drop_index_sql;
    EXECUTE drop_index_stmt;
    DEALLOCATE PREPARE drop_index_stmt;


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260501230258_20260502_RetiroTablasLegacyInventario') THEN


    SET @index_exists := (
        SELECT COUNT(*)
        FROM INFORMATION_SCHEMA.STATISTICS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = 'cotizaciondetalles'
          AND INDEX_NAME = 'IX_cotizaciondetalles_InsumoId'
    );
    SET @drop_index_sql := IF(
        @index_exists = 0,
        'SELECT 1',
        'ALTER TABLE `cotizaciondetalles` DROP INDEX `IX_cotizaciondetalles_InsumoId`'
    );
    PREPARE drop_index_stmt FROM @drop_index_sql;
    EXECUTE drop_index_stmt;
    DEALLOCATE PREPARE drop_index_stmt;


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260501230258_20260502_RetiroTablasLegacyInventario') THEN


    SET @index_exists := (
        SELECT COUNT(*)
        FROM INFORMATION_SCHEMA.STATISTICS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = 'cotizaciondetalles'
          AND INDEX_NAME = 'IX_cotizaciondetalles_MateriaPrimaId'
    );
    SET @drop_index_sql := IF(
        @index_exists = 0,
        'SELECT 1',
        'ALTER TABLE `cotizaciondetalles` DROP INDEX `IX_cotizaciondetalles_MateriaPrimaId`'
    );
    PREPARE drop_index_stmt FROM @drop_index_sql;
    EXECUTE drop_index_stmt;
    DEALLOCATE PREPARE drop_index_stmt;


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260501230258_20260502_RetiroTablasLegacyInventario') THEN


    SET @column_exists := (
        SELECT COUNT(*)
        FROM INFORMATION_SCHEMA.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = 'tiposprocesoconsumos'
          AND COLUMN_NAME = 'InsumoId'
    );
    SET @drop_column_sql := IF(
        @column_exists = 0,
        'SELECT 1',
        'ALTER TABLE `tiposprocesoconsumos` DROP COLUMN `InsumoId`'
    );
    PREPARE drop_column_stmt FROM @drop_column_sql;
    EXECUTE drop_column_stmt;
    DEALLOCATE PREPARE drop_column_stmt;


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260501230258_20260502_RetiroTablasLegacyInventario') THEN


    SET @column_exists := (
        SELECT COUNT(*)
        FROM INFORMATION_SCHEMA.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = 'tiposprocesoconsumos'
          AND COLUMN_NAME = 'MateriaPrimaId'
    );
    SET @drop_column_sql := IF(
        @column_exists = 0,
        'SELECT 1',
        'ALTER TABLE `tiposprocesoconsumos` DROP COLUMN `MateriaPrimaId`'
    );
    PREPARE drop_column_stmt FROM @drop_column_sql;
    EXECUTE drop_column_stmt;
    DEALLOCATE PREPARE drop_column_stmt;


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260501230258_20260502_RetiroTablasLegacyInventario') THEN


    SET @column_exists := (
        SELECT COUNT(*)
        FROM INFORMATION_SCHEMA.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = 'movimientosinventario'
          AND COLUMN_NAME = 'InsumoId'
    );
    SET @drop_column_sql := IF(
        @column_exists = 0,
        'SELECT 1',
        'ALTER TABLE `movimientosinventario` DROP COLUMN `InsumoId`'
    );
    PREPARE drop_column_stmt FROM @drop_column_sql;
    EXECUTE drop_column_stmt;
    DEALLOCATE PREPARE drop_column_stmt;


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260501230258_20260502_RetiroTablasLegacyInventario') THEN


    SET @column_exists := (
        SELECT COUNT(*)
        FROM INFORMATION_SCHEMA.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = 'movimientosinventario'
          AND COLUMN_NAME = 'MateriaPrimaId'
    );
    SET @drop_column_sql := IF(
        @column_exists = 0,
        'SELECT 1',
        'ALTER TABLE `movimientosinventario` DROP COLUMN `MateriaPrimaId`'
    );
    PREPARE drop_column_stmt FROM @drop_column_sql;
    EXECUTE drop_column_stmt;
    DEALLOCATE PREPARE drop_column_stmt;


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260501230258_20260502_RetiroTablasLegacyInventario') THEN


    SET @column_exists := (
        SELECT COUNT(*)
        FROM INFORMATION_SCHEMA.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = 'cotizaciondetalles'
          AND COLUMN_NAME = 'InsumoId'
    );
    SET @drop_column_sql := IF(
        @column_exists = 0,
        'SELECT 1',
        'ALTER TABLE `cotizaciondetalles` DROP COLUMN `InsumoId`'
    );
    PREPARE drop_column_stmt FROM @drop_column_sql;
    EXECUTE drop_column_stmt;
    DEALLOCATE PREPARE drop_column_stmt;


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260501230258_20260502_RetiroTablasLegacyInventario') THEN


    SET @column_exists := (
        SELECT COUNT(*)
        FROM INFORMATION_SCHEMA.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = 'cotizaciondetalles'
          AND COLUMN_NAME = 'MateriaPrimaId'
    );
    SET @drop_column_sql := IF(
        @column_exists = 0,
        'SELECT 1',
        'ALTER TABLE `cotizaciondetalles` DROP COLUMN `MateriaPrimaId`'
    );
    PREPARE drop_column_stmt FROM @drop_column_sql;
    EXECUTE drop_column_stmt;
    DEALLOCATE PREPARE drop_column_stmt;


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260501230258_20260502_RetiroTablasLegacyInventario') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260501230258_20260502_RetiroTablasLegacyInventario', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260502070200_NotaEntregaPagareEditable') THEN

    ALTER TABLE `notasentrega` ADD `TextoPagare` varchar(4000) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260502070200_NotaEntregaPagareEditable') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260502070200_NotaEntregaPagareEditable', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260502071757_NotaEntregaDetallePedidoConcepto') THEN

    ALTER TABLE `notasentregadetalle` ADD `PedidoConceptoId` char(36) COLLATE ascii_general_ci NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260502071757_NotaEntregaDetallePedidoConcepto') THEN

    CREATE INDEX `IX_notasentregadetalle_PedidoConceptoId` ON `notasentregadetalle` (`PedidoConceptoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260502071757_NotaEntregaDetallePedidoConcepto') THEN

    ALTER TABLE `notasentregadetalle` ADD CONSTRAINT `FK_notasentregadetalle_pedidoconceptos_PedidoConceptoId` FOREIGN KEY (`PedidoConceptoId`) REFERENCES `pedidoconceptos` (`Id`) ON DELETE SET NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260502071757_NotaEntregaDetallePedidoConcepto') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260502071757_NotaEntregaDetallePedidoConcepto', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260503014629_RrhhAsistenciasCorreccionResumenRapido') THEN

    ALTER TABLE `prenominadetalles` ADD `HorasTrabajadasNetas` decimal(18,2) NOT NULL DEFAULT 0.0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260503014629_RrhhAsistenciasCorreccionResumenRapido') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260503014629_RrhhAsistenciasCorreccionResumenRapido', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260503030000_RrhhHorasTrabajadasNetasSchemaRepair') THEN


    ALTER TABLE `nominadetalles`
    ADD COLUMN IF NOT EXISTS `HorasTrabajadasNetas` decimal(18,2) NOT NULL DEFAULT 0.00;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260503030000_RrhhHorasTrabajadasNetasSchemaRepair') THEN


    ALTER TABLE `prenominadetalles`
    ADD COLUMN IF NOT EXISTS `HorasTrabajadasNetas` decimal(18,2) NOT NULL DEFAULT 0.00;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260503030000_RrhhHorasTrabajadasNetasSchemaRepair') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260503030000_RrhhHorasTrabajadasNetasSchemaRepair', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260503053110_CxcCargosManuales') THEN

    CREATE TABLE `contabilidad_cargo_manual_cxc` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `EmpresaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `ClienteId` char(36) COLLATE ascii_general_ci NOT NULL,
        `PedidoId` char(36) COLLATE ascii_general_ci NULL,
        `NotaEntregaId` char(36) COLLATE ascii_general_ci NULL,
        `FacturaId` char(36) COLLATE ascii_general_ci NULL,
        `Tipo` int NOT NULL,
        `FechaCargo` datetime(6) NOT NULL,
        `Referencia` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `Concepto` varchar(250) CHARACTER SET utf8mb4 NOT NULL,
        `Monto` decimal(18,2) NOT NULL,
        `Notas` varchar(500) CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_contabilidad_cargo_manual_cxc` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_contabilidad_cargo_manual_cxc_clientes_ClienteId` FOREIGN KEY (`ClienteId`) REFERENCES `clientes` (`Id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_contabilidad_cargo_manual_cxc_empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `empresas` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_contabilidad_cargo_manual_cxc_facturas_FacturaId` FOREIGN KEY (`FacturaId`) REFERENCES `facturas` (`Id`) ON DELETE SET NULL,
        CONSTRAINT `FK_contabilidad_cargo_manual_cxc_notasentrega_NotaEntregaId` FOREIGN KEY (`NotaEntregaId`) REFERENCES `notasentrega` (`Id`) ON DELETE SET NULL,
        CONSTRAINT `FK_contabilidad_cargo_manual_cxc_pedidos_PedidoId` FOREIGN KEY (`PedidoId`) REFERENCES `pedidos` (`Id`) ON DELETE SET NULL
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260503053110_CxcCargosManuales') THEN

    CREATE INDEX `IX_contabilidad_cargo_manual_cxc_ClienteId` ON `contabilidad_cargo_manual_cxc` (`ClienteId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260503053110_CxcCargosManuales') THEN

    CREATE INDEX `IX_contabilidad_cargo_manual_cxc_EmpresaId_ClienteId_FechaCargo` ON `contabilidad_cargo_manual_cxc` (`EmpresaId`, `ClienteId`, `FechaCargo`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260503053110_CxcCargosManuales') THEN

    CREATE INDEX `IX_contabilidad_cargo_manual_cxc_FacturaId` ON `contabilidad_cargo_manual_cxc` (`FacturaId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260503053110_CxcCargosManuales') THEN

    CREATE INDEX `IX_contabilidad_cargo_manual_cxc_NotaEntregaId` ON `contabilidad_cargo_manual_cxc` (`NotaEntregaId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260503053110_CxcCargosManuales') THEN

    CREATE INDEX `IX_contabilidad_cargo_manual_cxc_PedidoId` ON `contabilidad_cargo_manual_cxc` (`PedidoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260503053110_CxcCargosManuales') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260503053110_CxcCargosManuales', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260503060406_NotaEntregaMultiPedido') THEN

    CREATE TABLE `contabilidad_nota_entrega_pedido` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `EmpresaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `NotaEntregaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `PedidoId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Orden` int NOT NULL DEFAULT 0,
        `EsPrincipal` tinyint(1) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_contabilidad_nota_entrega_pedido` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_contabilidad_nota_entrega_pedido_empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `empresas` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_contabilidad_nota_entrega_pedido_notasentrega_NotaEntregaId` FOREIGN KEY (`NotaEntregaId`) REFERENCES `notasentrega` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_contabilidad_nota_entrega_pedido_pedidos_PedidoId` FOREIGN KEY (`PedidoId`) REFERENCES `pedidos` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260503060406_NotaEntregaMultiPedido') THEN

    CREATE INDEX `IX_contabilidad_nota_entrega_pedido_EmpresaId` ON `contabilidad_nota_entrega_pedido` (`EmpresaId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260503060406_NotaEntregaMultiPedido') THEN

    CREATE UNIQUE INDEX `IX_contabilidad_nota_entrega_pedido_NotaEntregaId_PedidoId` ON `contabilidad_nota_entrega_pedido` (`NotaEntregaId`, `PedidoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260503060406_NotaEntregaMultiPedido') THEN

    CREATE INDEX `IX_contabilidad_nota_entrega_pedido_PedidoId_EsPrincipal` ON `contabilidad_nota_entrega_pedido` (`PedidoId`, `EsPrincipal`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260503060406_NotaEntregaMultiPedido') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260503060406_NotaEntregaMultiPedido', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260503063735_NotaEntregaAsignacionBase') THEN

    CREATE TABLE `contabilidad_nota_entrega_asignacion` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `EmpresaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `NotaEntregaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `PedidoId` char(36) COLLATE ascii_general_ci NOT NULL,
        `PedidoDetalleId` char(36) COLLATE ascii_general_ci NULL,
        `PedidoDetalleTallaId` char(36) COLLATE ascii_general_ci NULL,
        `PedidoConceptoId` char(36) COLLATE ascii_general_ci NULL,
        `TipoOrigen` int NOT NULL,
        `Cantidad` decimal(18,4) NOT NULL,
        `CantidadFgTomada` decimal(18,4) NOT NULL,
        `PrecioUnitario` decimal(18,4) NOT NULL,
        `Importe` decimal(18,2) NOT NULL,
        `EsParcial` tinyint(1) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_contabilidad_nota_entrega_asignacion` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_contabilidad_nota_entrega_asignacion_empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `empresas` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_contabilidad_nota_entrega_asignacion_notasentrega_NotaEntreg~` FOREIGN KEY (`NotaEntregaId`) REFERENCES `notasentrega` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_contabilidad_nota_entrega_asignacion_pedidoconceptos_PedidoC~` FOREIGN KEY (`PedidoConceptoId`) REFERENCES `pedidoconceptos` (`Id`) ON DELETE SET NULL,
        CONSTRAINT `FK_contabilidad_nota_entrega_asignacion_pedidodetalles_PedidoDe~` FOREIGN KEY (`PedidoDetalleId`) REFERENCES `pedidodetalles` (`Id`) ON DELETE SET NULL,
        CONSTRAINT `FK_contabilidad_nota_entrega_asignacion_pedidos_PedidoId` FOREIGN KEY (`PedidoId`) REFERENCES `pedidos` (`Id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_contabilidad_nota_entrega_asignacion_pedidosdetalletalla_Ped~` FOREIGN KEY (`PedidoDetalleTallaId`) REFERENCES `pedidosdetalletalla` (`Id`) ON DELETE SET NULL
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260503063735_NotaEntregaAsignacionBase') THEN

    CREATE INDEX `IX_contabilidad_nota_entrega_asignacion_EmpresaId` ON `contabilidad_nota_entrega_asignacion` (`EmpresaId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260503063735_NotaEntregaAsignacionBase') THEN

    CREATE INDEX `IX_contabilidad_nota_entrega_asignacion_NotaEntregaId` ON `contabilidad_nota_entrega_asignacion` (`NotaEntregaId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260503063735_NotaEntregaAsignacionBase') THEN

    CREATE INDEX `IX_contabilidad_nota_entrega_asignacion_PedidoConceptoId` ON `contabilidad_nota_entrega_asignacion` (`PedidoConceptoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260503063735_NotaEntregaAsignacionBase') THEN

    CREATE INDEX `IX_contabilidad_nota_entrega_asignacion_PedidoDetalleId` ON `contabilidad_nota_entrega_asignacion` (`PedidoDetalleId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260503063735_NotaEntregaAsignacionBase') THEN

    CREATE INDEX `IX_contabilidad_nota_entrega_asignacion_PedidoDetalleTallaId` ON `contabilidad_nota_entrega_asignacion` (`PedidoDetalleTallaId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260503063735_NotaEntregaAsignacionBase') THEN

    CREATE INDEX `IX_contabilidad_nota_entrega_asignacion_PedidoId` ON `contabilidad_nota_entrega_asignacion` (`PedidoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260503063735_NotaEntregaAsignacionBase') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260503063735_NotaEntregaAsignacionBase', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260503212600_ClienteIndustriaPersonalizada') THEN

    ALTER TABLE `clientes` ADD `IndustriaPersonalizada` varchar(120) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260503212600_ClienteIndustriaPersonalizada') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260503212600_ClienteIndustriaPersonalizada', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260506152252_RrhhSegmentoResolucion') THEN

    CREATE TABLE `rrhh_segmento_resolucion` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `EmpresaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `EmpleadoId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Fecha` date NOT NULL,
        `MarcacionInicioId` char(36) COLLATE ascii_general_ci NOT NULL,
        `MarcacionFinId` char(36) COLLATE ascii_general_ci NOT NULL,
        `TipoSegmento` int NOT NULL,
        `Estado` int NOT NULL,
        `FueInferidoAutomaticamente` tinyint(1) NOT NULL,
        `Observaciones` varchar(500) CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        CONSTRAINT `PK_rrhh_segmento_resolucion` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_rrhh_segmento_resolucion_empleados_EmpleadoId` FOREIGN KEY (`EmpleadoId`) REFERENCES `empleados` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_rrhh_segmento_resolucion_empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `empresas` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_rrhh_segmento_resolucion_rrhh_marcacion_MarcacionFinId` FOREIGN KEY (`MarcacionFinId`) REFERENCES `rrhh_marcacion` (`Id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_rrhh_segmento_resolucion_rrhh_marcacion_MarcacionInicioId` FOREIGN KEY (`MarcacionInicioId`) REFERENCES `rrhh_marcacion` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260506152252_RrhhSegmentoResolucion') THEN

    CREATE INDEX `IX_rrhh_segmento_resolucion_EmpleadoId` ON `rrhh_segmento_resolucion` (`EmpleadoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260506152252_RrhhSegmentoResolucion') THEN

    CREATE INDEX `IX_rrhh_segmento_resolucion_EmpresaId_EmpleadoId_Fecha_Estado` ON `rrhh_segmento_resolucion` (`EmpresaId`, `EmpleadoId`, `Fecha`, `Estado`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260506152252_RrhhSegmentoResolucion') THEN

    CREATE UNIQUE INDEX `IX_rrhh_segmento_resolucion_EmpresaId_EmpleadoId_Fecha_Marcacio~` ON `rrhh_segmento_resolucion` (`EmpresaId`, `EmpleadoId`, `Fecha`, `MarcacionInicioId`, `MarcacionFinId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260506152252_RrhhSegmentoResolucion') THEN

    CREATE INDEX `IX_rrhh_segmento_resolucion_MarcacionFinId` ON `rrhh_segmento_resolucion` (`MarcacionFinId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260506152252_RrhhSegmentoResolucion') THEN

    CREATE INDEX `IX_rrhh_segmento_resolucion_MarcacionInicioId` ON `rrhh_segmento_resolucion` (`MarcacionInicioId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260506152252_RrhhSegmentoResolucion') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260506152252_RrhhSegmentoResolucion', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260506215243_RrhhSegmentoResolucionMinutosAplicadosOverride') THEN

    ALTER TABLE `rrhh_segmento_resolucion` ADD `MinutosAplicadosOverride` int NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260506215243_RrhhSegmentoResolucionMinutosAplicadosOverride') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260506215243_RrhhSegmentoResolucionMinutosAplicadosOverride', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260507222745_RrhhAsistenciaPerdonManualVisible') THEN

    ALTER TABLE `rrhh_asistencia` ADD `MinutosPerdonadosManual` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260507222745_RrhhAsistenciaPerdonManualVisible') THEN

    ALTER TABLE `rrhh_asistencia` ADD `ObservacionPerdonManual` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260507222745_RrhhAsistenciaPerdonManualVisible') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260507222745_RrhhAsistenciaPerdonManualVisible', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260508193302_PrenominaDetalleMinutosFaltanteDescontable') THEN

    ALTER TABLE `prenominadetalles` ADD `MinutosFaltanteDescontable` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260508193302_PrenominaDetalleMinutosFaltanteDescontable') THEN

    ALTER TABLE `nominadetalles` ADD `MinutosFaltanteDescontable` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260508193302_PrenominaDetalleMinutosFaltanteDescontable') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260508193302_PrenominaDetalleMinutosFaltanteDescontable', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260509021757_PrenominaNominaMinutosPerdonadosManual') THEN

    ALTER TABLE `prenominadetalles` ADD `MinutosPerdonadosManual` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260509021757_PrenominaNominaMinutosPerdonadosManual') THEN

    ALTER TABLE `nominadetalles` ADD `MinutosPerdonadosManual` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260509021757_PrenominaNominaMinutosPerdonadosManual') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260509021757_PrenominaNominaMinutosPerdonadosManual', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260514233246_RrhhTurnosDescansosDinamicos') THEN

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

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260514233246_RrhhTurnosDescansosDinamicos') THEN

    CREATE UNIQUE INDEX `IX_rrhh_turno_detalle_descanso_TurnoBaseDetalleId_Orden` ON `rrhh_turno_detalle_descanso` (`TurnoBaseDetalleId`, `Orden`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260514233246_RrhhTurnosDescansosDinamicos') THEN


    INSERT INTO rrhh_turno_detalle_descanso (Id, TurnoBaseDetalleId, Orden, HoraInicio, HoraFin, EsPagado, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, IsActive)
    SELECT UUID(), Id, 1, Descanso1Inicio, Descanso1Fin, Descanso1EsPagado, UTC_TIMESTAMP(), NULL, NULL, NULL, 1
    FROM rrhh_turno_detalle
    WHERE Descanso1Inicio IS NOT NULL AND Descanso1Fin IS NOT NULL;

    INSERT INTO rrhh_turno_detalle_descanso (Id, TurnoBaseDetalleId, Orden, HoraInicio, HoraFin, EsPagado, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, IsActive)
    SELECT UUID(), Id, 2, Descanso2Inicio, Descanso2Fin, Descanso2EsPagado, UTC_TIMESTAMP(), NULL, NULL, NULL, 1
    FROM rrhh_turno_detalle
    WHERE Descanso2Inicio IS NOT NULL AND Descanso2Fin IS NOT NULL;


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260514233246_RrhhTurnosDescansosDinamicos') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260514233246_RrhhTurnosDescansosDinamicos', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

