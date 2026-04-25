-- ==========================================
-- INSERTAR TIPOS DE PROCESO INICIALES
-- ==========================================

USE CrmMundoVs_Dev;

INSERT INTO TiposProceso (Id, Nombre, Descripcion, CostoBase, Activo, Orden, CreatedAt, IsActive, CreatedBy, UpdatedBy, UpdatedAt)
VALUES 
(UUID(), 'Mesa', 'Serigrafía en mesa manual', 15.00, 1, 1, NOW(), 1, NULL, NULL, NULL),
(UUID(), 'Pulpo', 'Serigrafía con máquina pulpo', 25.00, 1, 2, NOW(), 1, NULL, NULL, NULL),
(UUID(), 'Transfer', 'Impresión por transferencia térmica', 20.00, 1, 3, NOW(), 1, NULL, NULL, NULL),
(UUID(), 'Sublimación', 'Sublimación digital', 30.00, 1, 4, NOW(), 1, NULL, NULL, NULL),
(UUID(), 'Plancha', 'Aplicación con plancha', 12.00, 1, 5, NOW(), 1, NULL, NULL, NULL),
(UUID(), 'Frecuencia', 'Proceso de frecuencia', 18.00, 1, 6, NOW(), 1, NULL, NULL, NULL),
(UUID(), 'Ploteo', 'Corte y aplicación de vinil', 22.00, 1, 7, NOW(), 1, NULL, NULL, NULL),
(UUID(), 'UV', 'Impresión UV', 35.00, 1, 8, NOW(), 1, NULL, NULL, NULL),
(UUID(), 'Plastisol', 'Serigrafía con tinta plastisol', 28.00, 1, 9, NOW(), 1, NULL, NULL, NULL);

-- Verificar
SELECT * FROM TiposProceso ORDER BY Orden;
