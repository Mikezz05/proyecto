# Export mappings and boletas to CSV files
# Usage: run in project folder; requires mysql client in PATH or adjust $mysqlBin

$mysqlBin = 'C:\\xampp\\mysql\\bin\\mysql.exe'
$mysqlArgs = '-u root escuela_pinto_salinas -B -N'

# files
$mapFile = 'calificaciones_mapping.csv'
$boletasFile = 'boletas_calificaciones.csv'

# Query: mapping from calificaciones_old to new inscripcion/calificacion
$q1 = @"
SELECT co.id AS cal_old_id, eo.id AS estudiante_old_id, s.id AS estudiante_new_id, eo.curso_id, i.id AS inscripcion_id, cal.id AS calificacion_new_id
FROM calificaciones_old co
LEFT JOIN estudiantes_old eo ON eo.id = co.estudiante_id
LEFT JOIN estudiantes s ON s.ci = eo.ci
LEFT JOIN inscripciones i ON i.estudiante_id = s.id AND i.curso_id = co.curso_id
LEFT JOIN calificaciones cal ON cal.inscripcion_id = i.id;
"@

# Query: boleta (new calificaciones with student and course)
$q2 = @"
SELECT cal.id AS calificacion_id, e.ci, CONCAT(e.nombres,' ',e.apellidos) AS estudiante, c.nombre_curso, cal.nota1, cal.nota2, cal.nota3, cal.nota_final, CASE WHEN cal.aprobado=1 THEN 'APROBADO' ELSE 'REPROBADO' END AS estado
FROM calificaciones cal
JOIN inscripciones i ON cal.inscripcion_id = i.id
JOIN estudiantes e ON i.estudiante_id = e.id
JOIN cursos c ON i.curso_id = c.id;
"@

# Run and export
& $mysqlBin -u root escuela_pinto_salinas -B -N -e $q1 | Out-File -Encoding utf8 $mapFile
Write-Host "Exported mapping to $mapFile"
& $mysqlBin -u root escuela_pinto_salinas -B -N -e $q2 | Out-File -Encoding utf8 $boletasFile
Write-Host "Exported boletas to $boletasFile"
