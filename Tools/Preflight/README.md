# Ferramentas de Preflight Pré-Unity — DARKLIGHT: REQUIEM

Este diretório contém validadores automatizados que realizam inspeção estática profunda do projeto antes da execução do Unity Editor ou builds.

---

## 1. Ferramentas Disponíveis

### `unity_asset_preflight.py` / `unity_asset_preflight.ps1`
Validador completo de integridade de assets, GUIDs e serialização YAML.
- **O que faz:**
  - Indexa todos os arquivos `.meta` em `Assets/`, `Packages/` e `Library/PackageCache`.
  - Detecta GUIDs duplicados.
  - Detecta arquivos sem `.meta` e arquivos `.meta` órfãos.
  - Inspeciona prefabs, cenas e ScriptableObjects buscando componentes quebrados (`Missing (MonoScript)`).
  - Distingue scripts ausentes de bibliotecas internas.
  - Valida presença dos arquivos essenciais (cenas principais, settings, asmdefs).
- **Como executar:**
  ```powershell
  # Via PowerShell:
  powershell -ExecutionPolicy Bypass -File Tools/Preflight/unity_asset_preflight.ps1

  # Ou via Python:
  python Tools/Preflight/unity_asset_preflight.py .
  ```
- **Saídas geradas:**
  - `outputs/preflight/asset_preflight_report.md` (relatório legível em markdown com tabelas)
  - `outputs/preflight/asset_preflight_report.json` (saída estruturada para automação)
- **Código de Saída:**
  - `0`: PASS (Sem falhas críticas de MonoScript ou GUIDs duplicados)
  - `1`: FAIL (Problemas críticos encontrados)

### `build_preflight.py`
Validador de dependências de build, asmdefs e versão do Unity.

---

## 2. Instruções para o Codex

Execute o script de preflight antes de abrir a Unity ou após mesclar alterações grandes:
```powershell
powershell -ExecutionPolicy Bypass -File Tools/Preflight/unity_asset_preflight.ps1
```
Se o status for `PASS`, o projeto está seguro para inicializar na Unity sem riscos de scripts corrompidos.
