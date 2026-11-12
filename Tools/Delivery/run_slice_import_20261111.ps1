$unity = 'C:\Program Files\Unity\Hub\Editor\6000.6.0f1\Editor\Unity.exe'
$proj = 'c:\Users\Usuario\Documents\Codex\2026-10-10\f\darklight-requiem-main'
$log = "$proj\work\slice_and_import_20261111.log"

Write-Host "Iniciando importacao e slicing em batchmode..."
$process = Start-Process -FilePath $unity -ArgumentList "-projectPath `"$proj`" -batchmode -nographics -quit -executeMethod Darklight.Editor.DarklightBatchAssetImporter.ImportAllFormsAndArconte -logFile `"$log`"" -PassThru -Wait

Write-Host "ExitCode: $($process.ExitCode)"
Get-Content $log -Tail 30

