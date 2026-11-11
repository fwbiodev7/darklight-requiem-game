$Build20261111 = Join-Path $PSScriptRoot "outputs\lucius-test-build-20261111\Windows"
$Build20261110 = Join-Path $PSScriptRoot "outputs\lucius-test-build-20261110\Windows"

$GameDir = if (Test-Path (Join-Path $Build20261111 "DarklightRequiem.exe")) { $Build20261111 } else { $Build20261110 }
$GameExe = Join-Path $GameDir "DarklightRequiem.exe"

if (Test-Path $GameExe) {
    Write-Host "Iniciando DARKLIGHT: REQUIEM [Build Lucius & VFX 2.0 - 11/11/2026]..." -ForegroundColor Cyan
    Write-Host "Caminho: $GameExe" -ForegroundColor Gray
    if ($args.Count -gt 0) {
        Start-Process -FilePath $GameExe -WorkingDirectory $GameDir -ArgumentList $args
    } else {
        Start-Process -FilePath $GameExe -WorkingDirectory $GameDir
    }
} else {
    Write-Error "Executavel nao encontrado em: $GameExe"
}
