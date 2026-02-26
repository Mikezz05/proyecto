# PowerShell script: respaldo + aplicar esquema y migración
param(
    [string]$mysqlUser = "root",
    [string]$schemaFile = "database_schema.sql",
    [string]$migrationFile = "migration_convert_old_to_new.sql",
    [string]$database = "escuela_pinto_salinas",
    [string]$mysqlBin = "C:\\xampp\\mysql\\bin"
)

Write-Host "Se hará un respaldo de la base de datos '$database' y luego se aplicarán: $schemaFile y $migrationFile"
    $pwd = Read-Host "Introduce la contraseña MySQL (se ocultará) - deja vacío si no tiene" -AsSecureString
    $BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($pwd)
    $plain = [System.Runtime.InteropServices.Marshal]::PtrToStringBSTR($BSTR)

    # Rutas a ejecutables
    $mysqlExe = Join-Path $mysqlBin "mysql.exe"
    $mysqldumpExe = Join-Path $mysqlBin "mysqldump.exe"
    if (-not (Test-Path $mysqlExe)) { throw "No se encontró mysql.exe en '$mysqlExe'. Ajusta el parámetro -mysqlBin." }
    if (-not (Test-Path $mysqldumpExe)) { throw "No se encontró mysqldump.exe en '$mysqldumpExe'. Ajusta el parámetro -mysqlBin." }

    try {
    $backupFile = "backup_$(Get-Date -Format 'yyyyMMdd_HHmmss').sql"
    Write-Host "Creando respaldo -> $backupFile"
    if ([string]::IsNullOrEmpty($plain)) {
        & $mysqldumpExe -u $mysqlUser $database > $backupFile
    } else {
        & $mysqldumpExe -u $mysqlUser -p$plain $database > $backupFile
    }
    if ($LASTEXITCODE -ne 0) { throw "Error creando respaldo" }

    # Renombrar tablas antiguas si existen para evitar colisiones con el nuevo esquema
    Write-Host "Renombrando tablas antiguas (si existen) para preservar datos..."
    $renameSql = @"
SET @s = NULL;
SELECT CASE WHEN (SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'estudiantes') > 0 AND (SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'estudiantes_old') = 0 THEN 'RENAME TABLE estudiantes TO estudiantes_old;' ELSE 'SELECT 1;' END INTO @s;
PREPARE stmt FROM @s; EXECUTE stmt; DEALLOCATE PREPARE stmt;
SELECT CASE WHEN (SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'calificaciones') > 0 AND (SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'calificaciones_old') = 0 THEN 'RENAME TABLE calificaciones TO calificaciones_old;' ELSE 'SELECT 1;' END INTO @s;
PREPARE stmt FROM @s; EXECUTE stmt; DEALLOCATE PREPARE stmt;
"@
    if ([string]::IsNullOrEmpty($plain)) {
        & $mysqlExe -u $mysqlUser -e $renameSql $database
    } else {
        & $mysqlExe -u $mysqlUser -p$plain -e $renameSql $database
    }

    Write-Host "Aplicando esquema: $schemaFile"
    if ([string]::IsNullOrEmpty($plain)) {
        Get-Content $schemaFile -Raw | & $mysqlExe -u $mysqlUser $database
    } else {
        Get-Content $schemaFile -Raw | & $mysqlExe -u $mysqlUser -p$plain $database
    }
    if ($LASTEXITCODE -ne 0) { throw "Error aplicando esquema" }

    Write-Host "Aplicando migración: $migrationFile"
    if ([string]::IsNullOrEmpty($plain)) {
        Get-Content $migrationFile -Raw | & $mysqlExe -u $mysqlUser $database
    } else {
        Get-Content $migrationFile -Raw | & $mysqlExe -u $mysqlUser -p$plain $database
    }
    if ($LASTEXITCODE -ne 0) { throw "Error aplicando migración" }

    Write-Host "Operación completada. Respaldo: $backupFile"
} finally {
    if ($BSTR) { [System.Runtime.InteropServices.Marshal]::ZeroFreeBSTR($BSTR) }
}
