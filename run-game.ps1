$GameDir = Join-Path $PSScriptRoot "outputs\build-windows-estabilidade-20261110\Windows"
$GameExe = Join-Path $GameDir "DarklightRequiem.exe"

if (Test-Path $GameExe) {
    Write-Host "Iniciando DARKLIGHT: REQUIEM [Build Estabilidade 10/11/2026]..." -ForegroundColor Cyan
    if ($args.Count -gt 0) {
        Start-Process -FilePath $GameExe -WorkingDirectory $GameDir -ArgumentList $args
    } else {
        Start-Process -FilePath $GameExe -WorkingDirectory $GameDir
    }
} else {
    Write-Error "Executavel nao encontrado em: $GameExe"
}
