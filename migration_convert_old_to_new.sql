-- =======================================================================================
-- MIGRACIÓN: Estructura antigua -> Estructura normalizada (3FN)
-- INSTRUCCIONES PREVIAS (LEA ANTES DE EJECUTAR):
-- 1) Haga un respaldo completo de la base de datos: `mysqldump -u root -p escuela_pinto_salinas > backup.sql`
-- 2) Aplique primero el script `database_schema.sql` (crea las tablas normalizadas).
-- 3) Revise manualmente que NO existan tablas con sufijo `_old` que puedan chocar.
-- 4) Ejecute este script en la misma base de datos: `mysql -u root -p < migration_convert_old_to_new.sql`
-- =======================================================================================

START TRANSACTION;

-- 0) Hash de contraseñas en la tabla `usuarios` (solo si no están ya en SHA256 hex)
UPDATE usuarios
SET password = SHA2(password, 256)
WHERE password IS NOT NULL
  AND (CHAR_LENGTH(password) != 64 OR password NOT REGEXP '^[0-9a-f]{64}$');

-- 1) Renombrar tablas antiguas para preservar datos originales
-- (Si estas tablas no existen, las siguientes RENAME fallarán; en ese caso elimine o comente las líneas y continúe manualmente)

-- Nota: Si su base de datos ya contiene la nueva tabla `estudiantes` creada por database_schema.sql,
-- renombrar la tabla antigua `estudiantes` evitará colisiones.

-- Renombrar `estudiantes` existente -> `estudiantes_old` (si existe la columna `curso_id` en ella)
SET @has_old_estudiantes = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'estudiantes' AND COLUMN_NAME = 'curso_id');

-- Si @has_old_estudiantes = 1 entonces existe la versión antigua con curso_id
-- Usamos PREPARE para condicionalmente renombrar
SET @s = NULL;

SELECT CASE WHEN @has_old_estudiantes > 0 THEN 'RENAME TABLE estudiantes TO estudiantes_old;' ELSE 'SELECT 1;' END INTO @s;
PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Renombrar `calificaciones` antigua a `calificaciones_old` (si tiene columnas estudiante_id + curso_id)
SET @has_old_calif = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'calificaciones' AND COLUMN_NAME = 'estudiante_id');
SELECT CASE WHEN @has_old_calif > 0 THEN 'RENAME TABLE calificaciones TO calificaciones_old;' ELSE 'SELECT 1;' END INTO @s;
PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- 2) POBLAR `estudiantes` (nueva tabla creada por database_schema.sql)
-- Insertar estudiantes únicos por CI (con datos representativos)
INSERT INTO estudiantes (ci, nombres, apellidos, telefono, correo, fecha_registro)
SELECT eo.ci, eo.nombres, eo.apellidos, eo.telefono, eo.correo, MIN(COALESCE(eo.fecha_inscripcion, NOW())) AS fecha_registro
FROM estudiantes_old eo
GROUP BY eo.ci
ON DUPLICATE KEY UPDATE nombres = VALUES(nombres);

-- 3) CREAR INSCRIPCIONES a partir de los antiguos registros por fila
INSERT IGNORE INTO inscripciones (estudiante_id, curso_id, fecha_inscripcion, requisitos_verificados)
SELECT s.id AS estudiante_id, eo.curso_id, eo.fecha_inscripcion, eo.requisitos_verificados
FROM estudiantes_old eo
JOIN estudiantes s ON s.ci = eo.ci;

-- 4) MIGRAR CALIFICACIONES a la nueva tabla `calificaciones`
-- Se asume que `calificaciones_old` tiene columnas: id, estudiante_id, curso_id, nota1, desc1, nota2, desc2, nota3, desc3, nota_final, aprobado
INSERT INTO calificaciones (inscripcion_id, nota1, desc1, nota2, desc2, nota3, desc3, nota_final, aprobado)
SELECT i.id AS inscripcion_id, co.nota1, co.desc1, co.nota2, co.desc2, co.nota3, co.desc3, co.nota_final, co.aprobado
FROM calificaciones_old co
JOIN estudiantes_old eo ON eo.id = co.estudiante_id
JOIN estudiantes s ON s.ci = eo.ci
JOIN inscripciones i ON i.estudiante_id = s.id AND i.curso_id = co.curso_id;

-- 5) Si desea, puede regenerar la nota_final según la nueva `configuracion_sistema` con el siguiente bloque:
-- (Descomente para ejecutar)
--
-- SET @p1 = (SELECT porcentaje_evaluacion_1/100 FROM configuracion_sistema WHERE id=1 LIMIT 1);
-- SET @p2 = (SELECT porcentaje_evaluacion_2/100 FROM configuracion_sistema WHERE id=1 LIMIT 1);
-- SET @p3 = (SELECT porcentaje_evaluacion_3/100 FROM configuracion_sistema WHERE id=1 LIMIT 1);
-- SET @notaMin = (SELECT nota_minima_aprobatoria FROM configuracion_sistema WHERE id=1 LIMIT 1);
--
-- UPDATE calificaciones cal JOIN inscripciones i ON cal.inscripcion_id = i.id
-- SET cal.nota_final = (cal.nota1*@p1 + cal.nota2*@p2 + cal.nota3*@p3),
--     cal.aprobado = CASE WHEN (cal.nota1*@p1 + cal.nota2*@p2 + cal.nota3*@p3) >= @notaMin THEN 1 ELSE 0 END;

-- 6) REPORTES DE VERIFICACIÓN
SELECT 'Resumen migración' as paso;
SELECT COUNT(*) AS estudiantes_old_count FROM estudiantes_old;
SELECT COUNT(*) AS estudiantes_new_count FROM estudiantes;
SELECT COUNT(*) AS inscripciones_count FROM inscripciones;
SELECT COUNT(*) AS calificaciones_count FROM calificaciones;

COMMIT;

-- FIN MIGRACIÓN
-- Revise los resultados, valide integridad y, si todo está correcto, puede eliminar las tablas *_old tras mantener un respaldo seguro.
