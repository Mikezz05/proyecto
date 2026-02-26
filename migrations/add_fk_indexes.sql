-- Idempotent migration: add useful indexes and FK constraints if missing
-- Run this against the target database. It checks INFORMATION_SCHEMA before applying DDL.

-- Ensure index on usuarios.role_id
SELECT COUNT(*) INTO @col FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='usuarios' AND COLUMN_NAME='role_id';
SET @s = IF(@col=0,'SELECT "column role_id missing";',
  (SELECT IF(COUNT(*)=0,
    'ALTER TABLE usuarios ADD INDEX idx_usuarios_role_id (role_id);',
    'SELECT "idx exists";')
  FROM INFORMATION_SCHEMA.STATISTICS
  WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='usuarios' AND INDEX_NAME='idx_usuarios_role_id'));
PREPARE stmt FROM @s; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- Ensure FK usuarios.role_id -> roles(id) (only if column exists)
SET @s = IF(@col=0,'SELECT "column role_id missing";',
  (SELECT IF(COUNT(*)=0,'ALTER TABLE usuarios ADD CONSTRAINT fk_usuarios_role_id FOREIGN KEY (role_id) REFERENCES roles(id) ON DELETE RESTRICT ON UPDATE CASCADE;','SELECT "fk exists";')
   FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE CONSTRAINT_SCHEMA=DATABASE() AND TABLE_NAME='usuarios' AND CONSTRAINT_TYPE='FOREIGN KEY' AND CONSTRAINT_NAME='fk_usuarios_role_id'));
PREPARE stmt FROM @s; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- inscripciones: indexes and FKs
SET @s = (SELECT IF(COUNT(*)=0,'ALTER TABLE inscripciones ADD INDEX idx_inscripciones_estudiante_id (estudiante_id);','SELECT "idx exists";') FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='inscripciones' AND INDEX_NAME='idx_inscripciones_estudiante_id');
PREPARE stmt FROM @s; EXECUTE stmt; DEALLOCATE PREPARE stmt;
SET @s = (SELECT IF(COUNT(*)=0,'ALTER TABLE inscripciones ADD INDEX idx_inscripciones_curso_id (curso_id);','SELECT "idx exists";') FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='inscripciones' AND INDEX_NAME='idx_inscripciones_curso_id');
PREPARE stmt FROM @s; EXECUTE stmt; DEALLOCATE PREPARE stmt;
SELECT COUNT(*) INTO @c FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE CONSTRAINT_SCHEMA=DATABASE() AND TABLE_NAME='inscripciones' AND CONSTRAINT_TYPE='FOREIGN KEY' AND CONSTRAINT_NAME='fk_inscripciones_estudiante';
SET @s = IF(@c=0,'ALTER TABLE inscripciones ADD CONSTRAINT fk_inscripciones_estudiante FOREIGN KEY (estudiante_id) REFERENCES estudiantes(id) ON DELETE CASCADE ON UPDATE CASCADE;','SELECT "fk exists";'); PREPARE stmt FROM @s; EXECUTE stmt; DEALLOCATE PREPARE stmt;
SELECT COUNT(*) INTO @c FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE CONSTRAINT_SCHEMA=DATABASE() AND TABLE_NAME='inscripciones' AND CONSTRAINT_TYPE='FOREIGN KEY' AND CONSTRAINT_NAME='fk_inscripciones_curso';
SET @s = IF(@c=0,'ALTER TABLE inscripciones ADD CONSTRAINT fk_inscripciones_curso FOREIGN KEY (curso_id) REFERENCES cursos(id) ON DELETE RESTRICT ON UPDATE CASCADE;','SELECT "fk exists";'); PREPARE stmt FROM @s; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- calificaciones: index and FK to inscripciones
SET @s = (SELECT IF(COUNT(*)=0,'ALTER TABLE calificaciones ADD INDEX idx_calificaciones_inscripcion_id (inscripcion_id);','SELECT "idx exists";') FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='calificaciones' AND INDEX_NAME='idx_calificaciones_inscripcion_id');
PREPARE stmt FROM @s; EXECUTE stmt; DEALLOCATE PREPARE stmt;
SELECT COUNT(*) INTO @c FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE CONSTRAINT_SCHEMA=DATABASE() AND TABLE_NAME='calificaciones' AND CONSTRAINT_TYPE='FOREIGN KEY' AND CONSTRAINT_NAME='fk_calificaciones_inscripcion';
SET @s = IF(@c=0,'ALTER TABLE calificaciones ADD CONSTRAINT fk_calificaciones_inscripcion FOREIGN KEY (inscripcion_id) REFERENCES inscripciones(id) ON DELETE CASCADE ON UPDATE CASCADE;','SELECT "fk exists";'); PREPARE stmt FROM @s; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- cursos.docente_id -> usuarios(id)
SET @s = (SELECT IF(COUNT(*)=0,'ALTER TABLE cursos ADD INDEX idx_cursos_docente_id (docente_id);','SELECT "idx exists";') FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='cursos' AND INDEX_NAME='idx_cursos_docente_id');
PREPARE stmt FROM @s; EXECUTE stmt; DEALLOCATE PREPARE stmt;
SELECT COUNT(*) INTO @c FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE CONSTRAINT_SCHEMA=DATABASE() AND TABLE_NAME='cursos' AND CONSTRAINT_TYPE='FOREIGN KEY' AND CONSTRAINT_NAME='fk_cursos_docente';
SET @s = IF(@c=0,'ALTER TABLE cursos ADD CONSTRAINT fk_cursos_docente FOREIGN KEY (docente_id) REFERENCES usuarios(id) ON DELETE SET NULL ON UPDATE CASCADE;','SELECT "fk exists";'); PREPARE stmt FROM @s; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- auditoria: link to acciones and modulos
SELECT COUNT(*) INTO @col FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='auditoria' AND COLUMN_NAME='accion_id';
SET @s = IF(@col=0,'SELECT "column accion_id missing";', (SELECT IF(COUNT(*)=0,'ALTER TABLE auditoria ADD INDEX idx_auditoria_accion_id (accion_id);','SELECT "idx exists";') FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='auditoria' AND INDEX_NAME='idx_auditoria_accion_id'));
PREPARE stmt FROM @s; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SELECT COUNT(*) INTO @col2 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='auditoria' AND COLUMN_NAME='modulo_id';
SET @s = IF(@col2=0,'SELECT "column modulo_id missing";', (SELECT IF(COUNT(*)=0,'ALTER TABLE auditoria ADD INDEX idx_auditoria_modulo_id (modulo_id);','SELECT "idx exists";') FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='auditoria' AND INDEX_NAME='idx_auditoria_modulo_id'));
PREPARE stmt FROM @s; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- FK constraints only if normalized columns exist
SET @s = IF(@col=0,'SELECT "column accion_id missing";', (SELECT IF(COUNT(*)=0,'ALTER TABLE auditoria ADD CONSTRAINT fk_auditoria_accion FOREIGN KEY (accion_id) REFERENCES acciones_auditoria(id) ON DELETE SET NULL ON UPDATE CASCADE;','SELECT "fk exists";') FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE CONSTRAINT_SCHEMA=DATABASE() AND TABLE_NAME='auditoria' AND CONSTRAINT_TYPE='FOREIGN KEY' AND CONSTRAINT_NAME='fk_auditoria_accion'));
PREPARE stmt FROM @s; EXECUTE stmt; DEALLOCATE PREPARE stmt;
SET @s = IF(@col2=0,'SELECT "column modulo_id missing";', (SELECT IF(COUNT(*)=0,'ALTER TABLE auditoria ADD CONSTRAINT fk_auditoria_modulo FOREIGN KEY (modulo_id) REFERENCES modulos_auditoria(id) ON DELETE SET NULL ON UPDATE CASCADE;','SELECT "fk exists";') FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE CONSTRAINT_SCHEMA=DATABASE() AND TABLE_NAME='auditoria' AND CONSTRAINT_TYPE='FOREIGN KEY' AND CONSTRAINT_NAME='fk_auditoria_modulo'));
PREPARE stmt FROM @s; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- End of migration
SELECT 'OK' AS result;
