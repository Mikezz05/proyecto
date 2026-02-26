-- Idempotent migration: normalize usuarios -> role_id and auditoria -> accion_id/modulo_id
-- BACKUP your DB before running. This script is safe to run multiple times.

-- 1) Ensure roles catalog exists
CREATE TABLE IF NOT EXISTS roles (
  id INT AUTO_INCREMENT PRIMARY KEY,
  nombre VARCHAR(64) NOT NULL UNIQUE,
  descripcion VARCHAR(255)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

INSERT INTO roles (nombre, descripcion)
SELECT 'Admin','Administrador del sistema' WHERE NOT EXISTS (SELECT 1 FROM roles WHERE nombre='Admin');
INSERT INTO roles (nombre, descripcion)
SELECT 'Docente','Profesor / docente' WHERE NOT EXISTS (SELECT 1 FROM roles WHERE nombre='Docente');
INSERT INTO roles (nombre, descripcion)
SELECT 'Estudiante','Estudiante' WHERE NOT EXISTS (SELECT 1 FROM roles WHERE nombre='Estudiante');

-- 2) Add role_id column to usuarios (if missing) and backfill from legacy `rol` column
SET @has_col = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='usuarios' AND COLUMN_NAME='role_id');
SET @s = IF(@has_col=0, 'ALTER TABLE usuarios ADD COLUMN role_id INT NULL;', 'SELECT "col exists";');
PREPARE stmt FROM @s; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- Backfill role_id using existing rol enum/text
UPDATE usuarios u
JOIN roles r ON r.nombre = u.rol
SET u.role_id = r.id
WHERE (u.role_id IS NULL OR u.role_id = 0) AND u.rol IS NOT NULL AND u.rol != '';

-- Add index and FK if possible
SET @has_idx = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='usuarios' AND INDEX_NAME='idx_usuarios_role_id');
SET @s = IF(@has_idx=0, 'ALTER TABLE usuarios ADD INDEX idx_usuarios_role_id (role_id);', 'SELECT "idx exists";'); PREPARE stmt FROM @s; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @has_fk = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE CONSTRAINT_SCHEMA=DATABASE() AND TABLE_NAME='usuarios' AND CONSTRAINT_TYPE='FOREIGN KEY' AND CONSTRAINT_NAME='fk_usuarios_role_id');
SET @s = IF(@has_fk=0, 'ALTER TABLE usuarios ADD CONSTRAINT fk_usuarios_role_id FOREIGN KEY (role_id) REFERENCES roles(id) ON DELETE RESTRICT ON UPDATE CASCADE;', 'SELECT "fk exists";'); PREPARE stmt FROM @s; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- 3) Ensure auditoria catalogs exist
CREATE TABLE IF NOT EXISTS modulos_auditoria (
  id INT AUTO_INCREMENT PRIMARY KEY,
  nombre VARCHAR(128) NOT NULL UNIQUE,
  descripcion VARCHAR(255)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS acciones_auditoria (
  id INT AUTO_INCREMENT PRIMARY KEY,
  nombre VARCHAR(128) NOT NULL UNIQUE,
  descripcion VARCHAR(255)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 4) Add modulo_id and accion_id columns to auditoria if missing
SET @has_mod_col = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='auditoria' AND COLUMN_NAME='modulo_id');
SET @s = IF(@has_mod_col=0, 'ALTER TABLE auditoria ADD COLUMN modulo_id INT NULL;', 'SELECT "col exists";'); PREPARE stmt FROM @s; EXECUTE stmt; DEALLOCATE PREPARE stmt;
SET @has_act_col = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='auditoria' AND COLUMN_NAME='accion_id');
SET @s = IF(@has_act_col=0, 'ALTER TABLE auditoria ADD COLUMN accion_id INT NULL;', 'SELECT "col exists";'); PREPARE stmt FROM @s; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- 5) Populate catalogs from existing legacy text columns and backfill ids
-- Insert distinct modulo names
INSERT INTO modulos_auditoria (nombre)
SELECT DISTINCT TRIM(modulo) FROM auditoria WHERE modulo IS NOT NULL AND TRIM(modulo)<>'' AND NOT EXISTS (SELECT 1 FROM modulos_auditoria ma WHERE ma.nombre = TRIM(auditoria.modulo));

-- Insert distinct accion names
INSERT INTO acciones_auditoria (nombre)
SELECT DISTINCT TRIM(accion) FROM auditoria WHERE accion IS NOT NULL AND TRIM(accion)<>'' AND NOT EXISTS (SELECT 1 FROM acciones_auditoria aa WHERE aa.nombre = TRIM(auditoria.accion));

-- Backfill IDs
UPDATE auditoria a JOIN modulos_auditoria m ON TRIM(a.modulo) = m.nombre SET a.modulo_id = m.id WHERE (a.modulo_id IS NULL OR a.modulo_id = 0) AND a.modulo IS NOT NULL;
UPDATE auditoria a JOIN acciones_auditoria ac ON TRIM(a.accion) = ac.nombre SET a.accion_id = ac.id WHERE (a.accion_id IS NULL OR a.accion_id = 0) AND a.accion IS NOT NULL;

-- 6) Add indexes and FKs for new columns if not present
SET @has_idx1 = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='auditoria' AND INDEX_NAME='idx_auditoria_modulo_id');
SET @s = IF(@has_idx1=0, 'ALTER TABLE auditoria ADD INDEX idx_auditoria_modulo_id (modulo_id);', 'SELECT "idx exists";'); PREPARE stmt FROM @s; EXECUTE stmt; DEALLOCATE PREPARE stmt;
SET @has_idx2 = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='auditoria' AND INDEX_NAME='idx_auditoria_accion_id');
SET @s = IF(@has_idx2=0, 'ALTER TABLE auditoria ADD INDEX idx_auditoria_accion_id (accion_id);', 'SELECT "idx exists";'); PREPARE stmt FROM @s; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @has_fk1 = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE CONSTRAINT_SCHEMA=DATABASE() AND TABLE_NAME='auditoria' AND CONSTRAINT_TYPE='FOREIGN KEY' AND CONSTRAINT_NAME='fk_auditoria_modulo');
SET @s = IF(@has_fk1=0, 'ALTER TABLE auditoria ADD CONSTRAINT fk_auditoria_modulo FOREIGN KEY (modulo_id) REFERENCES modulos_auditoria(id) ON DELETE SET NULL ON UPDATE CASCADE;', 'SELECT "fk exists";'); PREPARE stmt FROM @s; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @has_fk2 = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE CONSTRAINT_SCHEMA=DATABASE() AND TABLE_NAME='auditoria' AND CONSTRAINT_TYPE='FOREIGN KEY' AND CONSTRAINT_NAME='fk_auditoria_accion');
SET @s = IF(@has_fk2=0, 'ALTER TABLE auditoria ADD CONSTRAINT fk_auditoria_accion FOREIGN KEY (accion_id) REFERENCES acciones_auditoria(id) ON DELETE SET NULL ON UPDATE CASCADE;', 'SELECT "fk exists";'); PREPARE stmt FROM @s; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SELECT 'OK' AS result;
