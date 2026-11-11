$unity = 'C:\Program Files\Unity\Hub\Editor\6000.6.0f1\Editor\Unity.exe'
$proj = 'c:\Users\Usuario\Documents\Codex\2026-10-10\f\darklight-requiem-main'
$log = "$proj\work\build_windows_20261111.log"
$out = 'outputs/build-windows-20261111/Windows'

Write-Host "Iniciando build do Unity..."
$process = Start-Process -FilePath $unity -ArgumentList "-projectPath `"$proj`" -batchmode -nographics -quit -executeMethod Darklight.Editor.DarklightLuciusTestBuild.Build -darklightLuciusOutput `"$out`" -logFile `"$log`"" -PassThru -Wait

Write-Host "ExitCode: $($process.ExitCode)"
if (Test-Path "$proj\$out\build-result.txt") {
    Get-Content "$proj\$out\build-result.txt"
}

