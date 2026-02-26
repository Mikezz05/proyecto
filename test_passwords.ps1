$users = & "C:\xampp\mysql\bin\mysql.exe" -u root escuela_pinto_salinas -N -e "SELECT id,usuario,password FROM usuarios;"
$lines = $users -split "`n" | Where-Object { $_ -ne "" }
$candidates = @('1234','admin','password','root','secret','123456','admin123')

Add-Type -AssemblyName System.Security

function HashHex([string]$alg, [string]$text) {
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($text)
    switch ($alg) {
        'MD5' { $h = [System.Security.Cryptography.MD5]::Create().ComputeHash($bytes) }
        'SHA1' { $h = [System.Security.Cryptography.SHA1]::Create().ComputeHash($bytes) }
        'SHA256' { $h = [System.Security.Cryptography.SHA256]::Create().ComputeHash($bytes) }
        'SHA384' { $h = [System.Security.Cryptography.SHA384]::Create().ComputeHash($bytes) }
        'SHA512' { $h = [System.Security.Cryptography.SHA512]::Create().ComputeHash($bytes) }
        default { return '' }
    }
    return ([BitConverter]::ToString($h)).Replace('-','').ToLower()
}

foreach ($ln in $lines) {
    $parts = $ln -split "\t"
    $id = $parts[0]; $user = $parts[1]; $stored = $parts[2]
    Write-Host "User: $user (id=$id) stored: $stored"
    foreach ($pwd in $candidates) {
        $md5 = HashHex 'MD5' $pwd
        $sha1 = HashHex 'SHA1' $pwd
        $sha256 = HashHex 'SHA256' $pwd
        if ($md5 -eq $stored) { Write-Host "  match MD5 -> $pwd" }
        if ($sha1 -eq $stored) { Write-Host "  match SHA1 -> $pwd" }
        if ($sha256 -eq $stored) { Write-Host "  match SHA256 -> $pwd" }
    }
}
