param([switch]$Apply,[switch]$RemainingOldBuilds)
$ErrorActionPreference='Stop'
$taskWorkspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
$taskCodexRoot='C:\Users\Usuario\Documents\Codex'
$taskReportRoot=Join-Path $taskWorkspace 'work\retomada-20261026\cleanup'
$taskTargets=@(
    'C:\Users\Usuario\Documents\Codex\2026-10-10\f\darklight-requiem-main - Copia\Library',
    'C:\Users\Usuario\Documents\Codex\2026-10-10\f\outputs\cabecas-corrigidas-20261012\Windows',
    'C:\Users\Usuario\Documents\Codex\2026-10-10\f\outputs\sprites-inteiros-corrigidos-20261012\Windows',
    'C:\Users\Usuario\Documents\Codex\2026-10-10\f\outputs\folhas-usuario-20261012\Windows'
)
if($RemainingOldBuilds){
    $taskTargets=@(
        'C:\Users\Usuario\Documents\Codex\2026-10-10\f\outputs\transformacoes-vampiras\Windows',
        'C:\Users\Usuario\Documents\Codex\2026-10-10\f\outputs\menu-cinematografico\Windows',
        'C:\Users\Usuario\Documents\Codex\2026-10-10\f\outputs\gameplay-melhorada\Windows',
        'C:\Users\Usuario\Documents\Codex\2026-10-10\f\outputs\campanha\Windows',
        'C:\Users\Usuario\Documents\Codex\2026-10-10\f\outputs\alpha-0.1\Windows'
    )
}
$taskPrevious=Join-Path $taskWorkspace 'outputs\redesign-armas-20261023\Windows\DarklightRequiem.exe'
if(!(Test-Path -LiteralPath $taskPrevious -PathType Leaf)){throw 'Keep the most recent previous build before cleaning obsolete builds'}
if(Get-Process -Name Unity -ErrorAction SilentlyContinue){throw 'Close Unity before removing its old copy cache'}
$taskCutoff=[datetime]'2026-10-12T23:02:55'
$taskRows=@()
foreach($taskTarget in $taskTargets){
    if(!(Test-Path -LiteralPath $taskTarget -PathType Container)){throw "Missing exact cleanup target: $taskTarget"}
    $taskResolved=(Resolve-Path -LiteralPath $taskTarget).ProviderPath
    if($taskResolved -ne [IO.Path]::GetFullPath($taskTarget) -or !$taskResolved.StartsWith($taskCodexRoot+'\',[StringComparison]::OrdinalIgnoreCase)){
        throw "Cleanup path escaped the user-named Codex directory: $taskResolved"
    }
    $taskAncestor=Get-Item -LiteralPath $taskResolved -Force
    while($taskAncestor -and $taskAncestor.FullName.StartsWith($taskCodexRoot,[StringComparison]::OrdinalIgnoreCase)){
        if($taskAncestor.Attributes -band [IO.FileAttributes]::ReparsePoint){throw "Linked cleanup ancestor: $($taskAncestor.FullName)"}
        $taskAncestor=$taskAncestor.Parent
    }
    $taskItems=@(Get-ChildItem -LiteralPath $taskResolved -Recurse -Force)
    if($taskItems | Where-Object {$_.Attributes -band [IO.FileAttributes]::ReparsePoint}){throw "Linked child in cleanup target: $taskResolved"}
    $taskFiles=@($taskItems | Where-Object {!$_.PSIsContainer})
    if($taskResolved.EndsWith('\Library')){
        $taskCopy=[IO.Directory]::GetParent($taskResolved).FullName
        foreach($taskMarker in @('Assets','Packages','ProjectSettings','.git')){
            if(!(Test-Path -LiteralPath (Join-Path $taskCopy $taskMarker))){throw "Old copy source marker missing: $taskMarker"}
        }
        if(Test-Path -LiteralPath (Join-Path $taskCopy 'Temp\UnityLockfile')){throw 'Old copy has a Unity lockfile'}
        $taskTracked=@(git -C $taskCopy ls-files Library)
        if($LASTEXITCODE -ne 0){throw 'Could not verify that the old Library is untracked'}
        if($taskTracked.Count){throw 'Library contains tracked source files; do not remove'}
        $taskKind='regenerable Unity cache from inactive project copy'
    }else{
        if(!(Test-Path -LiteralPath (Join-Path $taskResolved 'DarklightRequiem.exe'))){throw 'Not the expected obsolete DARKLIGHT build'}
        if($taskFiles | Where-Object {$_.LastWriteTime -gt $taskCutoff}){throw 'Old build contains files newer than the cleanup cutoff'}
        $taskKind='obsolete 12/10 Windows build; 25/10 build preserved'
    }
    $taskBytes=($taskFiles | Measure-Object Length -Sum).Sum
    $taskRows+= [ordered]@{path=$taskResolved;kind=$taskKind;files=$taskFiles.Count;bytes=$taskBytes;removed=$false}
}
New-Item -ItemType Directory -Path $taskReportRoot -Force | Out-Null
$taskBefore=(Get-PSDrive C).Free
$taskReceipt=[ordered]@{timestamp=(Get-Date -Format o);apply=[bool]$Apply;freeBefore=$taskBefore;freeAfter=$taskBefore;logicalBytesRemoved=0;rows=$taskRows;exception=$null;latestPreviousBuildPreserved=$true;sourceFoldersRemoved=$false;chatHistoryRemoved=$false}
try{
    foreach($taskRow in $taskRows){
        if($Apply){
            # Each absolute target and every link check above must pass before any deletion.
            Remove-Item -LiteralPath $taskRow.path -Recurse -Force
            if(Test-Path -LiteralPath $taskRow.path){throw "Cleanup target still exists: $($taskRow.path)"}
            $taskRow.removed=$true
            $taskReceipt.logicalBytesRemoved+=$taskRow.bytes
        }
    }
}catch{$taskReceipt.exception=$_.Exception.Message;throw}
finally{
    $taskReceipt.freeAfter=(Get-PSDrive C).Free
    $taskReceipt | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $taskReportRoot ('cleanup-'+$(if($Apply){'applied'}else{'dry-run'})+$(if($RemainingOldBuilds){'-old-builds'}else{''})+'.json')) -Encoding utf8
    [pscustomobject]@{applied=[bool]$Apply;targets=$taskRows.Count;removedGiB=[math]::Round($taskReceipt.logicalBytesRemoved/1GB,2);freeGiB=[math]::Round($taskReceipt.freeAfter/1GB,2)}
}
