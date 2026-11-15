$GameDir = Join-Path $PSScriptRoot "outputs\build-windows-gothic-20261114-feedback\Windows"
$GameExe = Join-Path $GameDir "DarklightRequiem.exe"

if (Test-Path $GameExe) {
    Write-Host "Iniciando DARKLIGHT: REQUIEM [Artes, colisoes e nevoa - 14/11/2026]..." -ForegroundColor Cyan
    if ($args.Count -gt 0) {
        Start-Process -FilePath $GameExe -WorkingDirectory $GameDir -ArgumentList $args
    } else {
        Start-Process -FilePath $GameExe -WorkingDirectory $GameDir
    }
} else {
    Write-Error "Executavel nao encontrado em: $GameExe"
}
