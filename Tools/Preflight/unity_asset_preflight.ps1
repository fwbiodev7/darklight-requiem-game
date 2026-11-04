<#
.SYNOPSIS
    DARKLIGHT: REQUIEM â€” Wrapper PowerShell para o Preflight de Assets & YAML.
.DESCRIPTION
    Executa a auditoria estÃ¡tica prÃ©-Unity sem abrir o Unity Editor.
    Retorna 0 se estiver limpo, ou 1 se houver problemas crÃ­ticos de MonoScript/GUID.
#>
param (
    [string]$ProjectRoot = "."
)

$pyScript = Join-Path $PSScriptRoot "unity_asset_preflight.py"
python -u $pyScript $ProjectRoot
exit $LASTEXITCODE