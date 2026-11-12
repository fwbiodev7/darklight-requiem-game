$unity = 'C:\Program Files\Unity\Hub\Editor\6000.6.0f1\Editor\Unity.exe'
$proj = 'c:\Users\Usuario\Documents\Codex\2026-10-10\f\darklight-requiem-main'
$log = "$proj\work\run_tests_20261111.log"
$xml = "$proj\work\test_results_20261111.xml"

Write-Host "Executando suite de testes EditMode..."
$process = Start-Process -FilePath $unity -ArgumentList "-projectPath `"$proj`" -batchmode -nographics -runTests -testPlatform editmode -testResults `"$xml`" -logFile `"$log`"" -PassThru -Wait

Write-Host "ExitCode: $($process.ExitCode)"
if (Test-Path $xml) {
    [xml]$res = Get-Content $xml
    Write-Host "Total: $($res.'test-run'.total), Passed: $($res.'test-run'.passed), Failed: $($res.'test-run'.failed)"
}

