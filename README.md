Instrucciones rápidas para aplicar el esquema de base de datos (auditoría Antequera)

1) Conecta a tu servidor MySQL y ejecuta el script `database_schema.sql` ubicado en la raíz del proyecto:

Linux/WSL / Mac:

```bash
mysql -u root -p < database_schema.sql
```

Windows (PowerShell):

```powershell
mysql -u root -p < .\database_schema.sql
```

2) Verifica que las tablas `usuarios`, `estudiantes`, `docentes`, `administrativos`, `inscripciones`, `calificaciones`, `cursos`, `configuracion_sistema`, `auditoria`, `seriales_validos` y `seriales_admin` existan.

3) No permita que la aplicación ejecute ALTER TABLE en tiempo de ejecución; las migraciones deben hacerse con este script o su sistema de migraciones preferido.

Notas:
- Las contraseñas ahora se almacenan como SHA256. Al importar usuarios legacy, convierta las contraseñas a SHA256.
- Las inscripciones y calificaciones están normalizadas: un estudiante tiene filas únicas en `estudiantes` y múltiples `inscripciones`.
- El cálculo de promedios usa los valores de `configuracion_sistema`.

Si quieres, aplico el cambio final para adaptar cualquier otro formulario o preparo un script de migración que convierta la estructura antigua a la nueva automáticamente.
