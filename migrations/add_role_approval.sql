-- Migration: add_role_approval.sql
-- Añade la columna is_role_approved a la tabla usuarios (idempotente)

SELECT COUNT(*) INTO @exists FROM INFORMATION_SCHEMA.COLUMNS
 WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'usuarios' AND COLUMN_NAME = 'is_role_approved';

SET @stmt = IF(@exists = 0,
  'ALTER TABLE usuarios ADD COLUMN is_role_approved TINYINT(1) NOT NULL DEFAULT 1',
  'SELECT "already_exists"');

PREPARE s FROM @stmt;
EXECUTE s;
DEALLOCATE PREPARE s;

-- Nota: las cuentas nuevas de tipo Docente/Admin serán creadas con is_role_approved=0 por la aplicación;
-- este script solo añade la columna y la configura por defecto en 1 para compatibilidad con cuentas existentes.
