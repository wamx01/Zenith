-- ==========================================
-- Script de Datos de Prueba para MundoVs CRM
-- ==========================================

-- NOTA: Ejecuta primero las migraciones de Entity Framework:
-- dotnet ef database update

-- Luego puedes ejecutar este script para tener datos de ejemplo

USE CrmMundoVs;

-- ==========================================
-- CLIENTES
-- ==========================================

-- Cliente de Calzado
INSERT INTO Clientes (Id, Codigo, RazonSocial, NombreComercial, RfcCif, Email, Telefono, Direccion, Ciudad, Estado, CodigoPostal, Pais, Industria, CreatedAt, IsActive)
VALUES 
(UUID(), 'CLI001', 'Zapaterías del Norte S.A. de C.V.', 'Zapatos Norte', 'ZDN980123ABC', 'contacto@zapatosnorte.com', '8181234567', 'Av. Constitución 123', 'Monterrey', 'Nuevo León', '64000', 'México', 1, NOW(), 1);

-- Cliente de Serigrafía
INSERT INTO Clientes (Id, Codigo, RazonSocial, NombreComercial, RfcCif, Email, Telefono, Direccion, Ciudad, Estado, CodigoPostal, Pais, Industria, CreatedAt, IsActive)
VALUES 
(UUID(), 'CLI002', 'Impresiones Gráficas del Bajío', 'ImpreGrafic', 'IGB850615DEF', 'ventas@impregrafic.com', '4421234567', 'Calle Independencia 456', 'Querétaro', 'Querétaro', '76000', 'México', 2, NOW(), 1);

-- Cliente con ambas industrias
INSERT INTO Clientes (Id, Codigo, RazonSocial, NombreComercial, RfcCif, Email, Telefono, Direccion, Ciudad, Estado, CodigoPostal, Pais, Industria, CreatedAt, IsActive)
VALUES 
(UUID(), 'CLI003', 'Textiles y Calzado del Sur', 'TexCal Sur', 'TCS920830GHI', 'info@texcalsur.com', '9611234567', 'Boulevard Belisario Domínguez 789', 'Tuxtla Gutiérrez', 'Chiapas', '29000', 'México', 3, NOW(), 1);

-- ==========================================
-- CONTACTOS
-- ==========================================

-- Obtener IDs de clientes (ajustar según sea necesario)
SET @cliente1 = (SELECT Id FROM Clientes WHERE Codigo = 'CLI001' LIMIT 1);
SET @cliente2 = (SELECT Id FROM Clientes WHERE Codigo = 'CLI002' LIMIT 1);
SET @cliente3 = (SELECT Id FROM Clientes WHERE Codigo = 'CLI003' LIMIT 1);

INSERT INTO Contactos (Id, ClienteId, Nombre, Cargo, Email, Telefono, Movil, EsPrincipal, CreatedAt, IsActive)
VALUES 
(UUID(), @cliente1, 'Juan Pérez García', 'Gerente de Compras', 'jperez@zapatosnorte.com', '8181234567', '8112345678', 1, NOW(), 1),
(UUID(), @cliente2, 'María González López', 'Directora de Operaciones', 'mgonzalez@impregrafic.com', '4421234567', '4421234568', 1, NOW(), 1),
(UUID(), @cliente3, 'Carlos Ramírez Sánchez', 'Jefe de Producción', 'cramirez@texcalsur.com', '9611234567', '9611234569', 1, NOW(), 1);

-- ==========================================
-- PRODUCTOS - CALZADO
-- ==========================================

INSERT INTO Productos (Id, Codigo, Nombre, Descripcion, Industria, PrecioBase, UnidadMedida, Categoria, CreatedAt, IsActive)
VALUES 
(UUID(), 'CAL001', 'Zapato Formal Hombre Negro', 'Zapato formal de piel genuina color negro', 1, 850.00, 'par', 'Formal', NOW(), 1),
(UUID(), 'CAL002', 'Botín Casual Mujer Café', 'Botín casual de gamuza color café', 1, 950.00, 'par', 'Casual', NOW(), 1),
(UUID(), 'CAL003', 'Tenis Deportivo Unisex Blanco', 'Tenis deportivo con suela de goma', 1, 650.00, 'par', 'Deportivo', NOW(), 1),
(UUID(), 'CAL004', 'Sandalia Niña Rosa', 'Sandalia infantil color rosa con broche', 1, 350.00, 'par', 'Infantil', NOW(), 1);

-- ==========================================
-- PRODUCTOS - SERIGRAFÍA
-- ==========================================

INSERT INTO Productos (Id, Codigo, Nombre, Descripcion, Industria, PrecioBase, UnidadMedida, Categoria, CreatedAt, IsActive)
VALUES 
(UUID(), 'SER001', 'Playera Blanca Cuello Redondo', 'Playera 100% algodón blanca para impresión', 2, 45.00, 'pza', 'Textil', NOW(), 1),
(UUID(), 'SER002', 'Playera Negra Cuello V', 'Playera 100% algodón negra para impresión', 2, 50.00, 'pza', 'Textil', NOW(), 1),
(UUID(), 'SER003', 'Sudadera Con Capucha', 'Sudadera 80% algodón 20% poliéster', 2, 180.00, 'pza', 'Textil', NOW(), 1),
(UUID(), 'SER004', 'Taza Blanca Sublimable', 'Taza cerámica blanca 11oz', 2, 25.00, 'pza', 'Sublimación', NOW(), 1);

-- ==========================================
-- HORMAS (Calzado)
-- ==========================================

INSERT INTO Hormas (Id, Codigo, Nombre, Descripcion, Talla, Medidas, StockDisponible, Estado, CreatedAt, IsActive)
VALUES 
(UUID(), 'HOR001', 'Horma Formal Hombre 26', 'Horma para zapato formal masculino', '26', '26cm largo x 10cm ancho', 10, 1, NOW(), 1),
(UUID(), 'HOR002', 'Horma Formal Hombre 27', 'Horma para zapato formal masculino', '27', '27cm largo x 10.5cm ancho', 8, 1, NOW(), 1),
(UUID(), 'HOR003', 'Horma Casual Mujer 24', 'Horma para calzado casual femenino', '24', '24cm largo x 8.5cm ancho', 12, 1, NOW(), 1),
(UUID(), 'HOR004', 'Horma Deportivo 25', 'Horma para tenis deportivos', '25', '25cm largo x 9.5cm ancho', 15, 1, NOW(), 1);

-- ==========================================
-- TINTAS (Serigrafía)
-- ==========================================

INSERT INTO Tintas (Id, Codigo, Nombre, CodigoPantone, CodigoHex, Tipo, Cantidad, UnidadMedida, StockMinimo, CreatedAt, IsActive)
VALUES 
(UUID(), 'TIN001', 'Tinta Plastisol Blanco', '', '#FFFFFF', 2, 5000.00, 'ml', 1000.00, NOW(), 1),
(UUID(), 'TIN002', 'Tinta Plastisol Negro', '', '#000000', 2, 4500.00, 'ml', 1000.00, NOW(), 1),
(UUID(), 'TIN003', 'Tinta Base Agua Rojo', '186 C', '#ED1C24', 1, 2000.00, 'ml', 500.00, NOW(), 1),
(UUID(), 'TIN004', 'Tinta Base Agua Azul', '286 C', '#0033A0', 1, 1800.00, 'ml', 500.00, NOW(), 1),
(UUID(), 'TIN005', 'Tinta Sublimación Amarillo', '109 C', '#FFD700', 4, 1500.00, 'ml', 500.00, NOW(), 1);

-- ==========================================
-- PANTALLAS (Serigrafía)
-- ==========================================

INSERT INTO Pantallas (Id, Codigo, Descripcion, MallaNumero, Dimensiones, Estado, UsosTotales, FechaCreacion, DisenoPara, CreatedAt, IsActive)
VALUES 
(UUID(), 'PAN001', 'Pantalla para logo empresarial', 110, '50x60 cm', 2, 25, NOW(), 'Logo corporativo - playeras', NOW(), 1),
(UUID(), 'PAN002', 'Pantalla diseño flores', 150, '40x50 cm', 2, 15, NOW(), 'Diseño floral - textil', NOW(), 1),
(UUID(), 'PAN003', 'Pantalla texto promocional', 86, '60x70 cm', 1, 5, NOW(), 'Texto gran formato', NOW(), 1);

-- ==========================================
-- DISEÑOS (Serigrafía)
-- ==========================================

INSERT INTO Disenos (Id, Codigo, Nombre, Descripcion, ClienteId, RutaArchivo, NumeroColores, Dimensiones, Aprobado, FechaAprobacion, CreatedAt, IsActive)
VALUES 
(UUID(), 'DIS001', 'Logo Empresa ABC', 'Logotipo corporativo 3 colores', @cliente2, '/disenos/logo-abc.ai', 3, '20x15 cm', 1, NOW(), NOW(), 1),
(UUID(), 'DIS002', 'Diseño Floral Primavera', 'Diseño de flores para temporada', @cliente3, '/disenos/flores-primavera.psd', 4, '25x30 cm', 1, NOW(), NOW(), 1),
(UUID(), 'DIS003', 'Promoción Verano 2024', 'Diseño promocional de verano', @cliente2, '/disenos/promo-verano.pdf', 2, '30x20 cm', 0, NULL, NOW(), 1);

-- ==========================================
-- PEDIDOS
-- ==========================================

INSERT INTO Pedidos (Id, NumeroPedido, ClienteId, FechaPedido, FechaEntregaEstimada, Estado, Subtotal, Impuestos, Total, Observaciones, CreatedAt, IsActive)
VALUES 
(UUID(), 'PED-2024-001', @cliente1, NOW(), DATE_ADD(NOW(), INTERVAL 15 DAY), 2, 17000.00, 2720.00, 19720.00, 'Pedido urgente - entrega en 15 días', NOW(), 1),
(UUID(), 'PED-2024-002', @cliente2, NOW(), DATE_ADD(NOW(), INTERVAL 7 DAY), 3, 4500.00, 720.00, 5220.00, 'Producción en curso', NOW(), 1),
(UUID(), 'PED-2024-003', @cliente3, NOW(), DATE_ADD(NOW(), INTERVAL 30 DAY), 1, 8500.00, 1360.00, 9860.00, 'Cotización - pendiente de confirmación', NOW(), 1);

-- ==========================================
-- Verificar datos insertados
-- ==========================================

SELECT 'Clientes insertados:' AS Resumen, COUNT(*) AS Total FROM Clientes WHERE IsActive = 1
UNION ALL
SELECT 'Contactos insertados:', COUNT(*) FROM Contactos WHERE IsActive = 1
UNION ALL
SELECT 'Productos insertados:', COUNT(*) FROM Productos WHERE IsActive = 1
UNION ALL
SELECT 'Hormas insertadas:', COUNT(*) FROM Hormas WHERE IsActive = 1
UNION ALL
SELECT 'Tintas insertadas:', COUNT(*) FROM Tintas WHERE IsActive = 1
UNION ALL
SELECT 'Pantallas insertadas:', COUNT(*) FROM Pantallas WHERE IsActive = 1
UNION ALL
SELECT 'Diseños insertados:', COUNT(*) FROM Disenos WHERE IsActive = 1
UNION ALL
SELECT 'Pedidos insertados:', COUNT(*) FROM Pedidos WHERE IsActive = 1;

-- ==========================================
-- CONFIGURACIÓN DE MÓDULOS (AppConfigs)
-- ==========================================

INSERT IGNORE INTO AppConfigs (Id, Clave, Valor, Descripcion, CreatedAt)
VALUES 
('00000000-0000-0000-0000-000000000010', 'Modulo:Calzado', 'false', 'Habilitar módulo de Calzado', NOW()),
('00000000-0000-0000-0000-000000000011', 'Modulo:Serigrafia', 'true', 'Habilitar módulo de Serigrafía', NOW());

-- ==========================================
-- CONSECUTIVOS PARA CÓDIGOS AUTOMÁTICOS
-- ==========================================

INSERT IGNORE INTO AppConfigs (Id, Clave, Valor, Descripcion, CreatedAt)
VALUES 
('00000000-0000-0000-0000-000000000020', 'Consecutivo:Producto', '0', 'Consecutivo para código de Producto', NOW()),
('00000000-0000-0000-0000-000000000021', 'Consecutivo:MateriaPrima', '0', 'Consecutivo para código de Materia Prima', NOW()),
('00000000-0000-0000-0000-000000000022', 'Consecutivo:Insumo', '0', 'Consecutivo para código de Insumo', NOW()),
('00000000-0000-0000-0000-000000000023', 'Consecutivo:Cliente', '0', 'Consecutivo para código de Cliente', NOW());

-- ==========================================
-- FIN DEL SCRIPT
-- ==========================================
