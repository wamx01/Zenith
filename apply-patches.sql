-- Hotfix: apply missing columns that were marked as migrated but not actually created
-- Target DB: CrmMundoVs_Dev

ALTER TABLE `Productos` ADD COLUMN IF NOT EXISTS `Referencia` varchar(100) NULL;
ALTER TABLE `PedidosSerigrafia` ADD COLUMN IF NOT EXISTS `OrdenCompra` varchar(50) NULL;
