# Migración: `is_role_approved` (Aprobación de roles docentes/administrativos)

Resumen:
- Se añade la columna `is_role_approved` a `usuarios` para distinguir cuentas `Docente`/`Admin` pendientes de aprobación.
- Las cuentas existentes no se ven afectadas (valor por defecto = 1).
- Nuevo flujo: las cuentas `Docente`/`Admin` creadas por registro quedan con `is_role_approved = 0` y no pueden iniciar sesión hasta que un administrador las apruebe mediante `FormSerialManagement`.

Archivos incluidos:
- `migrations/add_role_approval.sql` (idempotente) — despliegue.
- `migrations/rollback_role_approval.sql` — rollback (ELIMINA la columna si existe). Hacer backup antes.
- `FormSerialManagement.cs` — formulario administrativo para aprobar cuentas y generar seriales.
- `FormRegistro.cs` / `FormLogin.cs` — cambios aplicados para flujo de aprobación.

Pasos recomendados para despliegue (producción):
1. Hacer backup completo de la base de datos:

```bash
mysqldump -u <user> -p --single-transaction --quick <dbname> > backup_full_$(date +%F).sql
```

2. Probar la migración en un entorno staging con copia de la BD de producción.
3. Ejecutar el script idempotente en producción:

```sql
-- desde cliente mysql
SOURCE /ruta/a/migrations/add_role_approval.sql;
```

4. Verificar que la columna existe:

```sql
SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='usuarios' AND COLUMN_NAME='is_role_approved';
```

5. Comunicar a los administradores: ahora deben usar el formulario `Aprobación de Serials` en el menú para aprobar cuentas pendientes y generar seriales.

Rollback:
- Si necesita revertir, primero exporte la tabla `usuarios` y `seriales`.

```bash
mysqldump -u <user> -p --single-transaction --quick <dbname> usuarios seriales > partial_backup_usuarios_seriales.sql
```

- Ejecutar:

```sql
SOURCE /ruta/a/migrations/rollback_role_approval.sql;
```

Notas de seguridad:
- Nunca ejecute las migraciones sin backup. El rollback elimina columna y puede causar pérdida de datos.
- Después de alinear esquemas en todos los entornos, considerar agregar FK entre `seriales.usuario_id` y `usuarios.id` para reforzar integridad referencial.

Evidencia para el auditor:
- Incluya en el paquete de auditoría la migración `add_role_approval.sql`, `rollback_role_approval.sql`, capturas de pantalla del `FormSerialManagement`, y registro de auditoría (`auditoria`) donde se vea la acción de aprobación.
