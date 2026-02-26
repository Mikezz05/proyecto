-- Rollback: rollback_role_approval.sql
-- Elimina la columna is_role_approved de la tabla usuarios si existe.
-- ADVERTENCIA: esto eliminará la información de aprobación y puede permitir logins que antes estaban bloqueados.
-- HAGA BACKUP antes de ejecutar: mysqldump -u user -p --single-transaction --quick <dbname> usuarios > usuarios_backup.sql

SELECT COUNT(*) INTO @exists FROM INFORMATION_SCHEMA.COLUMNS
 WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'usuarios' AND COLUMN_NAME = 'is_role_approved';

SET @stmt = IF(@exists = 1,
  'ALTER TABLE usuarios DROP COLUMN is_role_approved',
  'SELECT "column_not_found"');

PREPARE s FROM @stmt;
EXECUTE s;
DEALLOCATE PREPARE s;
