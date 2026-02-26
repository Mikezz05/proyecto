-- Script to create inscripciones from estudiantes_old and migrate calificaciones_old
START TRANSACTION;

-- 1) Insert inscripciones for each estudiantes_old row that has a matching student by CI
INSERT IGNORE INTO inscripciones (estudiante_id, curso_id, fecha_inscripcion, requisitos_verificados)
SELECT s.id, eo.curso_id, COALESCE(eo.fecha_inscripcion, NOW()), eo.requisitos_verificados
FROM estudiantes_old eo
JOIN estudiantes s ON s.ci = eo.ci;

-- 2) Insert calificaciones linking to created inscripciones
INSERT IGNORE INTO calificaciones (inscripcion_id, nota1, desc1, nota2, desc2, nota3, desc3, nota_final, aprobado)
SELECT i.id, co.nota1, co.desc1, co.nota2, co.desc2, co.nota3, co.desc3, co.nota_final, co.aprobado
FROM calificaciones_old co
JOIN estudiantes_old eo ON eo.id = co.estudiante_id
JOIN estudiantes s ON s.ci = eo.ci
JOIN inscripciones i ON i.estudiante_id = s.id AND i.curso_id = co.curso_id;

-- Verification
SELECT 'inscripciones_after' as tipo, COUNT(*) FROM inscripciones;
SELECT 'calificaciones_after' as tipo, COUNT(*) FROM calificaciones;

COMMIT;
