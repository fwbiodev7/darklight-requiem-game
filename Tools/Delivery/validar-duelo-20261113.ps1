$ErrorActionPreference = "Stop"
$project = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
$editor = Join-Path $env:ProgramFiles "Unity/Hub/Editor/6000.6.0f1/Editor/Unity.exe"
$out = Join-Path $project "outputs/build-windows-20261113/Windows"
$logs = Join-Path $project "work/validation-20261113"
New-Item -ItemType Directory -Force $logs | Out-Null
$stamp = Get-Date -Format yyyyMMdd-HHmmss
function Run-Editor([string[]] $editorArgs) {
    $process = Start-Process -FilePath $editor -ArgumentList $editorArgs -WorkingDirectory $project -WindowStyle Hidden -PassThru -Wait
    if ($process.ExitCode -ne 0) { throw "Unity encerrou com código $($process.ExitCode). Logs em $logs." }
}
$common = @("-batchmode","-nographics","-darklightSilent","-projectPath",("`"$project`""))
$passed = 0
foreach ($suite in @(
    @{mode="EditMode"; filter="DarklightLivingPresentationTests;DarklightLuciusCombatEncounterTests;DarklightLuciusAnimationIntegrationTests;DarklightLuciusHoodedTests;DarklightFabiusFormsTests;DarklightFabiusAnimationStabilityTests"},
    @{mode="PlayMode"; filter="DarklightNarrativePlayTests.FinalPortalDuelRespectsBothRecordedStances"}
)) {
    $mode = $suite.mode
    $xml = Join-Path $logs "$stamp-$mode.xml"
    Run-Editor ($common + @("-runTests","-testPlatform",$mode,"-testFilter",$suite.filter,"-testResults",("`"$xml`""),"-logFile",("`"$logs/$stamp-$mode.log`"")))
    $result = ([xml](Get-Content $xml -Raw))."test-run"
    if ($result.result -ne "Passed" -or [int]$result.passed -lt 1) { throw "Testes incompletos. Consulte $xml." }
    $passed += [int]$result.passed
}
Run-Editor ($common + @("-quit","-executeMethod","Darklight.Editor.DarklightLuciusTestBuild.Build","-darklightLuciusOutput",("`"$out`""),"-logFile",("`"$logs/$stamp-build.log`"")))
$receipt = Get-Content (Join-Path $out "build-result.json") -Raw | ConvertFrom-Json
if ($receipt.result -ne "Succeeded" -or !(Test-Path $receipt.path)) { throw "Build sem executável confirmado." }
Write-Output "$passed testes aprovados. Executável: $($receipt.path)"
