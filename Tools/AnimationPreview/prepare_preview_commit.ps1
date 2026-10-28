# Copies only the reviewed preview package. Does not stage, commit, push or delete.
# Default is a read-only plan. Use -Apply only after promoting previa.html.
[CmdletBinding()]
param([switch]$Apply)

$ErrorActionPreference = 'Stop'
$projectRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$cloneRoot = [IO.Path]::GetFullPath((Join-Path $projectRoot 'work/publish-review-20261017'))
$previewRelative = 'outputs/fidelidade-original-20261018'
$previewRoot = Join-Path $projectRoot $previewRelative

if (-not $cloneRoot.StartsWith($projectRoot + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) {
    throw 'Publish clone must remain inside this project workspace.'
}
if (-not (Test-Path -LiteralPath (Join-Path $cloneRoot '.git'))) {
    throw 'Expected isolated publish clone is missing.'
}

# Fixed selected PNG resources, reviewed against source hashes on 19/10/2026.
# Any selection change requires reviewing this list again.
$resourceAllowlist = @(
    'pacote-v6/Requiem-adagas-combo-branca-coerencia-v4-olhos-ownership.png'
    'pacote-v6/Requiem-adagas-combo-branca-coerencia-v4-olhos.png'
    'pacote-v6/Requiem-adagas-combo-normal-coerencia-v3-ownership.png'
    'pacote-v6/Requiem-adagas-combo-normal-coerencia-v3.png'
    'pacote-v6/Requiem-adagas-combo-roxa-coerencia-v4-olhos-ownership.png'
    'pacote-v6/Requiem-adagas-combo-roxa-coerencia-v4-olhos.png'
    'pacote-v6/Requiem-adagas-combo-vermelha-coerencia-v4-olhos-ownership.png'
    'pacote-v6/Requiem-adagas-combo-vermelha-coerencia-v4-olhos.png'
    'pacote-v6/Requiem-corrente-combo-branca-coerencia-v4-olhos-ownership.png'
    'pacote-v6/Requiem-corrente-combo-branca-coerencia-v4-olhos.png'
    'pacote-v6/Requiem-corrente-combo-normal-coerencia-v3-ownership.png'
    'pacote-v6/Requiem-corrente-combo-normal-coerencia-v3.png'
    'pacote-v6/Requiem-corrente-combo-roxa-coerencia-v4-olhos-ownership.png'
    'pacote-v6/Requiem-corrente-combo-roxa-coerencia-v4-olhos.png'
    'pacote-v6/Requiem-corrente-combo-vermelha-coerencia-v4-olhos-ownership.png'
    'pacote-v6/Requiem-corrente-combo-vermelha-coerencia-v4-olhos.png'
    'pacote-v6/Requiem-frontal-grande-branca-cristais-v2-ownership.png'
    'pacote-v6/Requiem-frontal-grande-branca-cristais-v2.png'
    'pacote-v6/Requiem-frontal-grande-normal-ownership.png'
    'pacote-v6/Requiem-frontal-grande-normal.png'
    'pacote-v6/Requiem-frontal-grande-roxa-ownership.png'
    'pacote-v6/Requiem-frontal-grande-roxa.png'
    'pacote-v6/Requiem-frontal-grande-vermelha-ownership.png'
    'pacote-v6/Requiem-frontal-grande-vermelha.png'
    'pacote-v6/Requiem-katana-combo-branca-coerencia-v4-olhos-ownership.png'
    'pacote-v6/Requiem-katana-combo-branca-coerencia-v4-olhos.png'
    'pacote-v6/Requiem-katana-combo-normal-coerencia-v3-ownership.png'
    'pacote-v6/Requiem-katana-combo-normal-coerencia-v3.png'
    'pacote-v6/Requiem-katana-combo-roxa-coerencia-v4-olhos-ownership.png'
    'pacote-v6/Requiem-katana-combo-roxa-coerencia-v4-olhos.png'
    'pacote-v6/Requiem-katana-combo-vermelha-coerencia-v4-olhos-ownership.png'
    'pacote-v6/Requiem-katana-combo-vermelha-coerencia-v4-olhos.png'
    'pacote-v6/Rock-adagas-combo-branca-coerencia-v4-olhos-ownership.png'
    'pacote-v6/Rock-adagas-combo-branca-coerencia-v4-olhos.png'
    'pacote-v6/Rock-adagas-combo-normal-coerencia-v3-ownership.png'
    'pacote-v6/Rock-adagas-combo-normal-coerencia-v3.png'
    'pacote-v6/Rock-adagas-combo-roxa-coerencia-v4-olhos-ownership.png'
    'pacote-v6/Rock-adagas-combo-roxa-coerencia-v4-olhos.png'
    'pacote-v6/Rock-adagas-combo-vermelha-coerencia-v4-olhos-ownership.png'
    'pacote-v6/Rock-adagas-combo-vermelha-coerencia-v4-olhos.png'
    'pacote-v6/Rock-cavaleiro-combo-branca-coerencia-v4-olhos-ownership.png'
    'pacote-v6/Rock-cavaleiro-combo-branca-coerencia-v4-olhos.png'
    'pacote-v6/Rock-cavaleiro-combo-normal-ownership.png'
    'pacote-v6/Rock-cavaleiro-combo-normal.png'
    'pacote-v6/Rock-cavaleiro-combo-roxa-coerencia-v4-olhos-ownership.png'
    'pacote-v6/Rock-cavaleiro-combo-roxa-coerencia-v4-olhos.png'
    'pacote-v6/Rock-cavaleiro-combo-vermelha-coerencia-v4-olhos-ownership.png'
    'pacote-v6/Rock-cavaleiro-combo-vermelha-coerencia-v4-olhos.png'
    'pacote-v6/Rock-corrente-combo-branca-coerencia-v4-olhos-ownership.png'
    'pacote-v6/Rock-corrente-combo-branca-coerencia-v4-olhos.png'
    'pacote-v6/Rock-corrente-combo-normal-coerencia-v3-ownership.png'
    'pacote-v6/Rock-corrente-combo-normal-coerencia-v3.png'
    'pacote-v6/Rock-corrente-combo-roxa-coerencia-v4-olhos-ownership.png'
    'pacote-v6/Rock-corrente-combo-roxa-coerencia-v4-olhos.png'
    'pacote-v6/Rock-corrente-combo-vermelha-coerencia-v4-olhos-ownership.png'
    'pacote-v6/Rock-corrente-combo-vermelha-coerencia-v4-olhos.png'
    'pacote-v6/Rock-frontal-grande-branca-cristais-v2-ownership.png'
    'pacote-v6/Rock-frontal-grande-branca-cristais-v2.png'
    'pacote-v6/Rock-frontal-grande-normal-ownership.png'
    'pacote-v6/Rock-frontal-grande-normal.png'
    'pacote-v6/Rock-frontal-grande-roxa-ownership.png'
    'pacote-v6/Rock-frontal-grande-roxa.png'
    'pacote-v6/Rock-frontal-grande-vermelha-ownership.png'
    'pacote-v6/Rock-frontal-grande-vermelha.png'
    'pacote-v6/Rock-katana-combo-branca-coerencia-v4-olhos-ownership.png'
    'pacote-v6/Rock-katana-combo-branca-coerencia-v4-olhos.png'
    'pacote-v6/Rock-katana-combo-normal-coerencia-v3-ownership.png'
    'pacote-v6/Rock-katana-combo-normal-coerencia-v3.png'
    'pacote-v6/Rock-katana-combo-roxa-coerencia-v4-olhos-ownership.png'
    'pacote-v6/Rock-katana-combo-roxa-coerencia-v4-olhos.png'
    'pacote-v6/Rock-katana-combo-vermelha-coerencia-v4-olhos-ownership.png'
    'pacote-v6/Rock-katana-combo-vermelha-coerencia-v4-olhos.png'
)
$actualResources = @(
    (Get-Content -LiteralPath (Join-Path $previewRoot 'pacote-v6/manifesto.json') -Raw | ConvertFrom-Json).sheets |
        ForEach-Object { $_.file; $_.ownerFile } |
        Sort-Object -Unique
)
$selectionDifference = @(Compare-Object $resourceAllowlist $actualResources)
if ($selectionDifference.Count) {
    throw 'Manifest resource selection changed. Review the explicit PNG allowlist before copying.'
}

$previewAllowlist = @(
    'previa.html'
    'previa-v6.html'
    'combat-preview.js'
    'referencia-original.png'
    'auditoria-frontais-grandes-20261018.json'
    'pacote-v6/fontes.json'
    'pacote-v6/manifesto.json'
    'pacote-v6/FONTES-COMBOS.json'
    'pacote-v6/FONTES-CAMINHADA-GRANDE.json'
    'pacote-v6/FONTES-CRISTAIS-BRANCOS-V2.json'
    'pacote-v6/QA-CRISTAIS-BRANCOS-V2.json'
    'pacote-v6/QA-CRISTAIS-BRANCOS-V2.md'
    'pacote-v6/verificacao.json'
    'pacote-v6/tempos-combos.json'
    'pacote-v6/REGISTRO.md'
    'pacote-v6/PENDENCIAS-20261018.md'
    'pacote-v6/QA-CONTROLES.json'
    'pacote-v6/QA-CONTROLES.md'
    'pacote-v6/AUDITORIA-JOGO-20261018.md'
    'pacote-v6/QA-ADAGAS-EDITMODE.xml'
    'pacote-v6/QA-ADAGAS-EDITMODE.json'
    'pacote-v6/FONTES-COMBOS-CRISTAIS-BRANCOS-V2.json'
    'pacote-v6/QA-FONTES-20261019.json'
    'pacote-v6/QA-CONTROLES-20261019.json'
    'pacote-v6/QA-NAVEGADOR-20261019.json'
    'pacote-v6/QA-RUNTIME.json'
    'pacote-v6/QA-PUBLISH-RUNTIME.json'
    'pacote-v6/paleta-pele.json'
    'pacote-v6/FONTES-COERENCIA-ATAQUES-20261019.json'
    'pacote-v6/QA-COERENCIA-ATAQUES-20261019.json'
    'pacote-v6/QA-PELE-DUAS-SKINS.png'
    'pacote-v6/QA-ATAQUES-DUAS-SKINS.png'
    'pacote-v6/QA-UNITY-EDITMODE-20261019.xml'
    'pacote-v6/QA-UNITY-PALETTE-20261019.xml'
    'pacote-v6/QA-UNITY-GAMEPLAY-20261019.xml'
    'pacote-v6/QA-UNITY-HUD-20261019.xml'
    'pacote-v6/PENDENCIAS-20261019.md'
    'STATUS-REVISAO.md'
    'COMPARACAO.md'
)
# These are included only once the current browser verification is recorded.
$optionalCurrentDocs = @(
    'pacote-v6/QA-NAVEGADOR.json'
    'pacote-v6/QA-NAVEGADOR.md'
)
foreach ($relative in $optionalCurrentDocs) {
    if (Test-Path -LiteralPath (Join-Path $previewRoot $relative)) {
        $previewAllowlist += $relative
    }
}
$toolAllowlist = @(
    'Tools/AnimationPreview/build_combat_preview.py'
    'Tools/AnimationPreview/whole_figure_components.py'
    'Tools/AnimationPreview/audit_visual_selection.py'
    'Tools/AnimationPreview/inventory_animation_coverage.py'
    'Tools/AnimationPreview/select_combat_sources.py'
    'Tools/AnimationPreview/prepare_preview_commit.ps1'
    'Tools/AnimationPreview/check_preview_sources.py'
    'Tools/AnimationPreview/check_preview_controls.cjs'
    'Tools/AnimationPreview/compile_focused.py'
    'Tools/AnimationPreview/prepare_complexion.py'
    'Tools/AnimationPreview/prepare_preview_complexion.py'
    'Tools/AnimationPreview/summarize_runtime_review.py'
    'Tools/AnimationPreview/publish_runtime_release.py'
)
# Only the reviewed runtime/resource files and focused tests may enter Assets/.
# Keep this exact list separate from the general preview resources.
$runtimeAssetAllowlist = @(
    'Assets/Darklight/Runtime/Combat/DarklightCombatProfiles.cs'
    'Assets/Darklight/Runtime/Character/DarklightCharacterView.cs'
    'Assets/Darklight/Runtime/Character/DarklightChainView.cs'
    'Assets/Darklight/Runtime/Character/DarklightSmallTransformationAura.cs'
    'Assets/Darklight/Runtime/Character/DarklightTransformationBurst.cs'
    'Assets/Darklight/Runtime/World/DarklightCombatTrail.cs'
    'Assets/Darklight/Runtime/Character/DarklightAttackFrameTiming.cs'
    'Assets/Darklight/Runtime/Character/TodayCharacterCatalog.cs'
    'Assets/Darklight/Runtime/World/DarklightAtlas.cs'
    'Assets/Darklight/Resources/Darklight/CanonicalComplexion.json'
)
$assetAllowlist = @($runtimeAssetAllowlist)
foreach ($relative in $runtimeAssetAllowlist) {
    if (Test-Path -LiteralPath (Join-Path $projectRoot ($relative + '.meta'))) {
        $assetAllowlist += $relative + '.meta'
    }
}
$assetAllowlist += @(
    'Assets/Darklight/Tests/EditMode/DarklightDaggerComboProfileTests.cs'
    'Assets/Darklight/Tests/EditMode/DarklightDaggerComboProfileTests.cs.meta'
    'Assets/Darklight/Tests/EditMode/DarklightContactPresentationTests.cs'
    'Assets/Darklight/Tests/EditMode/DarklightContactPresentationTests.cs.meta'
    'Assets/Darklight/Tests/PlayMode/DarklightBridgeDepthTests.cs'
    'Assets/Darklight/Tests/PlayMode/DarklightBridgeDepthTests.cs.meta'
    'Assets/Darklight/Tests/PlayMode/DarklightSustainedCombatTests.cs'
    'Assets/Darklight/Tests/PlayMode/DarklightSustainedCombatTests.cs.meta'
    'Assets/Darklight/Tests/PlayMode/DarklightMapIconTests.cs'
    'Assets/Darklight/Tests/PlayMode/DarklightMapIconTests.cs.meta'
)
$deliveryAllowlist = @(
    'outputs/revisao-runtime-20261019/LEIA-ME.md'
    'outputs/revisao-runtime-20261019/build-result.json'
    'outputs/revisao-runtime-20261019/validation-summary.json'
    'outputs/revisao-runtime-20261019/native-check.json'
    'outputs/revisao-runtime-20261019/build-files.json'
    'outputs/revisao-runtime-20261019/package-result.json'
    'outputs/revisao-runtime-20261019/SHA256.txt'
    'outputs/revisao-runtime-20261019/github-release-result.json'
    'outputs/revisao-runtime-20261019/RELEASE.md'
)
$protectedAssetPaths = @(
    'Assets/Darklight/Runtime/Character/DarklightDeathReversion.cs'
    'Assets/Darklight/Runtime/Character/DarklightOriginalVampireView.cs'
)
$allowlist = @(
    $toolAllowlist
    $assetAllowlist
    $deliveryAllowlist
    $previewAllowlist | ForEach-Object { $previewRelative + '/' + $_ }
    $resourceAllowlist | ForEach-Object { $previewRelative + '/' + $_ }
) | Sort-Object -Unique

# Check everything before writing a single destination file.
$records = @(
    foreach ($relative in $allowlist) {
        if (($relative.StartsWith('Assets/', [StringComparison]::OrdinalIgnoreCase) -and $relative -notin $assetAllowlist) -or
            $relative -in $protectedAssetPaths -or
            $relative -match '(^|/)(ArtSources|Builds?|Library|UserSettings|\.git)(/|$)' -or
            $relative -match '(?i)(morte-|save|rejeitad|medidas\.json$|\.log$|\.jsonl$)') {
            throw "Forbidden delivery path: $relative"
        }
        $source = [IO.Path]::GetFullPath((Join-Path $projectRoot $relative))
        $destination = [IO.Path]::GetFullPath((Join-Path $cloneRoot $relative))
        if (-not $source.StartsWith($projectRoot + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase) -or
            -not $destination.StartsWith($cloneRoot + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) {
            throw "Path escaped its intended root: $relative"
        }
        $item = Get-Item -LiteralPath $source -Force
        if ($item.PSIsContainer -or $item.Length -ge 95MB) {
            throw "Unexpected source type or oversized Git file: $relative"
        }
        [pscustomobject]@{
            Relative = $relative
            Source = $source
            Destination = $destination
            Length = $item.Length
            Sha256 = (Get-FileHash -LiteralPath $source -Algorithm SHA256).Hash
        }
    }
)
if ($Apply) {
    $entryHtml = Get-Content -LiteralPath (Join-Path $previewRoot 'previa.html') -Raw
    if ($entryHtml -notmatch 'combat-preview\.js') {
        throw 'previa.html still uses the previous preview. Promote and verify it before copying.'
    }
    foreach ($record in $records) {
        $destinationParent = Split-Path -Parent $record.Destination
        if (-not (Test-Path -LiteralPath $destinationParent)) {
            New-Item -ItemType Directory -Path $destinationParent -Force | Out-Null
        }
        Copy-Item -LiteralPath $record.Source -Destination $record.Destination -Force
        $copiedHash = (Get-FileHash -LiteralPath $record.Destination -Algorithm SHA256).Hash
        if ($copiedHash -ne $record.Sha256) {
            throw "Destination hash mismatch: $($record.Relative)"
        }
    }
}
[pscustomobject]@{
    applied = [bool]$Apply
    count = $records.Count
    totalMiB = [math]::Round(($records | Measure-Object Length -Sum).Sum / 1MB, 2)
    resources = $resourceAllowlist.Count
    files = @($records.Relative)
} | ConvertTo-Json -Depth 4
