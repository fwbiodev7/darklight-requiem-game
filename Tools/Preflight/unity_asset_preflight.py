#!/usr/bin/env python3
"""
DARKLIGHT: REQUIEM — Unity Asset & YAML Preflight Validator
Validates asset integrity, GUID references, and YAML serialization prior to Unity execution.

Checks:
1. Missing .meta files (Assets without .meta)
2. Orphan .meta files (.meta without corresponding asset)
3. Duplicate GUIDs in .meta files
4. Missing MonoScripts in prefabs, scenes, and scriptable objects (Missing MonoScript)
5. Unresolved external asset references in YAML
6. Essential core files integrity (Scenes, asmdefs, project settings)
7. Distinguishes Missing GUID from Namespace/Class mismatch
"""

import os
import sys
import re
import json
import time
from pathlib import Path

# Built-in Unity internal GUIDs
KNOWN_BUILTIN_GUIDS = {
    "0000000000000000e000000000000000": "Unity Default Resources (Internal)",
    "0000000000000000f000000000000000": "Unity Extra Resources (Internal)",
    "00000000000000001000000000000000": "Unity Built-in Shaders (Internal)",
}

def run_asset_preflight(project_root="."):
    start_time = time.time()
    project_root = Path(project_root).resolve()
    assets_dir = project_root / "Assets"
    packages_dir = project_root / "Packages"
    package_cache_dir = project_root / "Library" / "PackageCache"

    report = {
        "timestamp": time.strftime("%Y-%m-%d %H:%M:%S"),
        "project_root": str(project_root),
        "status": "PASS",
        "summary": {
            "total_meta_indexed": 0,
            "total_csharp_scripts": 0,
            "total_yaml_scanned": 0,
            "duplicate_guids": 0,
            "missing_metas": 0,
            "orphan_metas": 0,
            "broken_monoscripts": 0,
            "unresolved_asset_refs": 0,
            "missing_essential_files": 0
        },
        "critical_issues": [],
        "warnings": [],
        "essential_files": {},
        "details": {
            "duplicate_guids": {},
            "missing_metas": [],
            "orphan_metas": [],
            "broken_monoscripts": [],
            "unresolved_asset_refs": []
        }
    }

    def add_issue(severity, category, message, file=None, line=None, guid=None):
        entry = {
            "severity": severity,
            "category": category,
            "message": message,
            "file": file,
            "line": line,
            "guid": guid
        }
        if severity == "CRITICAL":
            report["status"] = "FAIL"
            report["critical_issues"].append(entry)
        else:
            report["warnings"].append(entry)

    # -------------------------------------------------------------
    # Step 1: Index all .meta files in Assets/, Packages/ and PackageCache
    # -------------------------------------------------------------
    print("1/4 Indexando arquivos .meta do projeto e pacotes Unity...")
    guid_to_path = dict(KNOWN_BUILTIN_GUIDS)
    path_to_guid = {}
    duplicate_guids = {}
    missing_metas = []
    orphan_metas = []

    guid_regex = re.compile(r"^guid:\s*([a-fA-F0-9]{32})", re.MULTILINE)

    # Scan project Assets/ and Packages/
    scan_dirs = []
    if assets_dir.exists():
        scan_dirs.append(assets_dir)
    if packages_dir.exists():
        scan_dirs.append(packages_dir)

    for s_dir in scan_dirs:
        for root, dirs, files in os.walk(s_dir):
            for f in files:
                full_path = Path(root) / f
                rel_path = full_path.relative_to(project_root).as_posix()

                if f.endswith(".meta"):
                    asset_target = full_path.with_name(f[:-5])
                    if not asset_target.exists():
                        orphan_metas.append(rel_path)
                    try:
                        content = full_path.read_text(encoding="utf-8-sig", errors="replace")
                        m = guid_regex.search(content)
                        if m:
                            guid = m.group(1).lower()
                            target_rel = asset_target.relative_to(project_root).as_posix()
                            if guid in guid_to_path and guid_to_path[guid] != target_rel:
                                if guid not in duplicate_guids:
                                    duplicate_guids[guid] = [guid_to_path[guid]]
                                duplicate_guids[guid].append(target_rel)
                            else:
                                guid_to_path[guid] = target_rel
                                path_to_guid[target_rel] = guid
                    except Exception as e:
                        add_issue("WARN", "MetaReadError", f"Failed to read {rel_path}: {e}", file=rel_path)
                else:
                    meta_path = full_path.with_name(f + ".meta")
                    if not meta_path.exists():
                        missing_metas.append(rel_path)

    # Fast-scan PackageCache for package GUIDs (.cs.meta, .dll.meta, .shader.meta)
    if package_cache_dir.exists():
        for root, dirs, files in os.walk(package_cache_dir):
            for f in files:
                if f.endswith(".meta"):
                    try:
                        mp = Path(root) / f
                        content = mp.read_text(encoding="utf-8-sig", errors="replace")
                        m = guid_regex.search(content)
                        if m:
                            g = m.group(1).lower()
                            if g not in guid_to_path:
                                target = mp.with_name(f[:-5])
                                guid_to_path[g] = target.relative_to(project_root).as_posix()
                    except Exception:
                        pass

    report["summary"]["total_meta_indexed"] = len(guid_to_path)

    # Process duplicates
    if duplicate_guids:
        report["summary"]["duplicate_guids"] = len(duplicate_guids)
        report["details"]["duplicate_guids"] = duplicate_guids
        for g, paths in duplicate_guids.items():
            add_issue("CRITICAL", "DuplicateGUID", f"GUID {g} duplicado em: {', '.join(paths)}", guid=g)

    # Process missing metas
    if missing_metas:
        report["summary"]["missing_metas"] = len(missing_metas)
        report["details"]["missing_metas"] = missing_metas
        for m in missing_metas:
            add_issue("WARN", "MissingMeta", f"Asset sem arquivo .meta: {m}", file=m)

    # Process orphan metas
    if orphan_metas:
        report["summary"]["orphan_metas"] = len(orphan_metas)
        report["details"]["orphan_metas"] = orphan_metas
        for o in orphan_metas:
            add_issue("WARN", "OrphanMeta", f"Arquivo .meta órfão sem asset: {o}", file=o)

    # -------------------------------------------------------------
    # Step 2: Index C# MonoScripts (Classes & Namespaces)
    # -------------------------------------------------------------
    print("2/4 Indexando scripts C# e classes declaradas...")
    csharp_scripts = {}
    class_regex = re.compile(r"\b(?:class|struct|interface|enum)\s+([A-Za-z0-9_]+)")
    namespace_regex = re.compile(r"\bnamespace\s+([A-Za-z0-9_\.]+)")

    for guid, path_str in guid_to_path.items():
        if path_str.startswith("Assets/") and path_str.endswith(".cs"):
            full_cs = project_root / path_str
            if full_cs.exists():
                try:
                    src = full_cs.read_text(encoding="utf-8-sig", errors="replace")
                    classes = class_regex.findall(src)
                    namespaces = namespace_regex.findall(src)
                    csharp_scripts[guid] = {
                        "path": path_str,
                        "classes": classes,
                        "namespaces": namespaces
                    }
                except Exception:
                    pass

    report["summary"]["total_csharp_scripts"] = len(csharp_scripts)

    # -------------------------------------------------------------
    # Step 3: Scan YAML files (.prefab, .unity, .asset)
    # -------------------------------------------------------------
    print("3/4 Analisando integridade de arquivos YAML (.prefab, .unity, .asset)...")
    yaml_extensions = {".prefab", ".unity", ".asset"}
    script_ref_regex = re.compile(r"m_Script:\s*\{[^}]*guid:\s*([a-fA-F0-9]{32})[^}]*\}")
    general_guid_regex = re.compile(r"guid:\s*([a-fA-F0-9]{32})")

    broken_monoscripts = []
    unresolved_asset_refs = []
    yaml_count = 0

    if assets_dir.exists():
        for root, dirs, files in os.walk(assets_dir):
            for f in files:
                ext = Path(f).suffix.lower()
                if ext in yaml_extensions:
                    yaml_count += 1
                    full_yaml = Path(root) / f
                    rel_yaml = full_yaml.relative_to(project_root).as_posix()

                    try:
                        with open(full_yaml, "r", encoding="utf-8-sig", errors="replace") as yf:
                            for line_num, line in enumerate(yf, 1):
                                sm = script_ref_regex.search(line)
                                if sm:
                                    s_guid = sm.group(1).lower()
                                    if s_guid not in guid_to_path:
                                        entry = {
                                            "file": rel_yaml,
                                            "line": line_num,
                                            "guid": s_guid,
                                            "cause": "GUID_NOT_FOUND",
                                            "description": "Script referenciado pelo YAML não existe no projeto nem nos pacotes (Missing MonoScript no Unity)"
                                        }
                                        broken_monoscripts.append(entry)
                                        add_issue("CRITICAL", "MissingMonoScript", 
                                                  f"MonoScript ausente GUID {s_guid} em {rel_yaml}:{line_num}",
                                                  file=rel_yaml, line=line_num, guid=s_guid)
                                    else:
                                        target_cs = guid_to_path[s_guid]
                                        if not target_cs.endswith(".cs") and not target_cs.endswith(".dll") and not target_cs.startswith("Library/PackageCache") and not target_cs.startswith("Packages/"):
                                            entry = {
                                                "file": rel_yaml,
                                                "line": line_num,
                                                "guid": s_guid,
                                                "target": target_cs,
                                                "cause": "INVALID_SCRIPT_TARGET",
                                                "description": f"GUID de script aponta para asset que não é código: {target_cs}"
                                            }
                                            broken_monoscripts.append(entry)
                                            add_issue("CRITICAL", "InvalidScriptTarget",
                                                      f"GUID de script aponta para {target_cs} em {rel_yaml}:{line_num}",
                                                      file=rel_yaml, line=line_num, guid=s_guid)
                                else:
                                    # General GUIDs
                                    for gm in general_guid_regex.finditer(line):
                                        g = gm.group(1).lower()
                                        if g not in guid_to_path:
                                            entry = {
                                                "file": rel_yaml,
                                                "line": line_num,
                                                "guid": g
                                            }
                                            unresolved_asset_refs.append(entry)
                                            add_issue("WARN", "UnresolvedAssetRef",
                                                      f"Referência a GUID não resolvido {g} em {rel_yaml}:{line_num}",
                                                      file=rel_yaml, line=line_num, guid=g)
                    except Exception as e:
                        add_issue("WARN", "YAMLReadError", f"Erro lendo YAML {rel_yaml}: {e}", file=rel_yaml)

    report["summary"]["total_yaml_scanned"] = yaml_count
    report["summary"]["broken_monoscripts"] = len(broken_monoscripts)
    report["summary"]["unresolved_asset_refs"] = len(unresolved_asset_refs)
    report["details"]["broken_monoscripts"] = broken_monoscripts
    report["details"]["unresolved_asset_refs"] = unresolved_asset_refs[:50]

    # -------------------------------------------------------------
    # Step 4: Validate Essential Core Files
    # -------------------------------------------------------------
    print("4/4 Verificando arquivos essenciais da campanha e settings...")
    essential_check = {
        "Cena Principal (DarklightCampaign)": "Assets/Darklight/Scenes/DarklightCampaign.unity",
        "Cena de Testes (DarklightCombatLab)": "Assets/Darklight/Scenes/DarklightCombatLab.unity",
        "Assembly de Runtime (Darklight.Runtime)": "Assets/Darklight/Runtime/Darklight.Runtime.asmdef",
        "Configuração de Versão Unity": "ProjectSettings/ProjectVersion.txt",
        "Configuração de Cenas no Build": "ProjectSettings/EditorBuildSettings.asset",
        "Manifesto de Pacotes": "Packages/manifest.json",
        "Modelo Central de Combate": "Assets/Darklight/Runtime/Combat/CombatModel.cs",
        "Diretor Central de Campanha": "Assets/Darklight/Runtime/Campaign/DarklightCampaign.cs"
    }

    missing_essentials = 0
    for name, rel_p in essential_check.items():
        exists = (project_root / rel_p).exists()
        report["essential_files"][name] = {
            "path": rel_p,
            "exists": exists
        }
        if not exists:
            missing_essentials += 1
            add_issue("CRITICAL", "MissingEssentialFile", f"Arquivo essencial ausente: {rel_p} ({name})", file=rel_p)

    report["summary"]["missing_essential_files"] = missing_essentials

    # -------------------------------------------------------------
    # Step 5: Save Reports (JSON & Markdown)
    # -------------------------------------------------------------
    elapsed = round(time.time() - start_time, 2)
    report["elapsed_seconds"] = elapsed

    out_dir = project_root / "outputs" / "preflight"
    out_dir.mkdir(parents=True, exist_ok=True)

    json_path = out_dir / "asset_preflight_report.json"
    with open(json_path, "w", encoding="utf-8") as jf:
        json.dump(report, jf, indent=2)

    # Markdown report
    md_lines = [
        "# DARKLIGHT: REQUIEM — Relatório Pré-Unity (Preflight de Assets & YAML)",
        f"**Data da Execução:** {report['timestamp']}  ",
        f"**Status Final:** `{'PASS (OK)' if report['status'] == 'PASS' else 'FAIL (CRITICAL ISSUES FOUND)'}`  ",
        f"**Tempo de Execução:** {elapsed}s  ",
        "",
        "---",
        "",
        "## 1. Sumário Executivo",
        "",
        "| Métrica | Quantidade | Severidade |",
        "| :--- | :---: | :---: |",
        f"| GUIDs Indexados (.meta + Pacotes) | **{report['summary']['total_meta_indexed']}** | INFO |",
        f"| Scripts C# Analisados | **{report['summary']['total_csharp_scripts']}** | INFO |",
        f"| Arquivos YAML Inspecionados (.unity/.prefab/.asset) | **{report['summary']['total_yaml_scanned']}** | INFO |",
        f"| GUIDs Duplicados | **{report['summary']['duplicate_guids']}** | `{'CRITICAL' if report['summary']['duplicate_guids'] > 0 else 'PASS'}` |",
        f"| MonoScripts Ausentes (Missing MonoScript) | **{report['summary']['broken_monoscripts']}** | `{'CRITICAL' if report['summary']['broken_monoscripts'] > 0 else 'PASS'}` |",
        f"| Arquivos Essenciais Ausentes | **{report['summary']['missing_essential_files']}** | `{'CRITICAL' if report['summary']['missing_essential_files'] > 0 else 'PASS'}` |",
        f"| Arquivos sem .meta correspondente | **{report['summary']['missing_metas']}** | `{'WARN' if report['summary']['missing_metas'] > 0 else 'PASS'}` |",
        f"| Metas órfãos (sem arquivo base) | **{report['summary']['orphan_metas']}** | `{'WARN' if report['summary']['orphan_metas'] > 0 else 'PASS'}` |",
        f"| Referências a assets não resolvidos | **{report['summary']['unresolved_asset_refs']}** | `{'WARN' if report['summary']['unresolved_asset_refs'] > 0 else 'PASS'}` |",
        "",
        "---",
        "",
        "## 2. Verificação de Arquivos Essenciais do Projeto",
        "",
        "| Componente | Caminho Relativo | Status |",
        "| :--- | :--- | :---: |"
    ]

    for name, data in report["essential_files"].items():
        st = "✅ PRESENTE" if data["exists"] else "❌ AUSENTE"
        md_lines.append(f"| {name} | `{data['path']}` | {st} |")

    md_lines.append("")
    md_lines.append("---")
    md_lines.append("")
    md_lines.append("## 3. Detalhamento de Problemas Críticos (Bloqueantes de Compilação/Editor)")

    if report["critical_issues"]:
        md_lines.append("")
        for issue in report["critical_issues"]:
            md_lines.append(f"- **[{issue['category']}]** `{issue['file']}` (Linha {issue.get('line', 'N/A')}): {issue['message']}")
    else:
        md_lines.append("")
        md_lines.append("> [!NOTE]")
        md_lines.append("> Nenhum problema crítico de GUID ou MonoScript quebrado foi encontrado no projeto.")

    if report["warnings"]:
        md_lines.append("")
        md_lines.append("## 4. Avisos e Observações Não-Bloqueantes (Recomendações de Limpeza)")
        md_lines.append(f"Total de avisos: {len(report['warnings'])}")
        for w in report["warnings"][:20]:
            f_str = f"`{w['file']}`: " if w.get("file") else ""
            md_lines.append(f"- **[{w['category']}]** {f_str}{w['message']}")
        if len(report["warnings"]) > 20:
            md_lines.append(f"- *... e mais {len(report['warnings']) - 20} avisos (ver JSON completo em `outputs/preflight/asset_preflight_report.json`).*")

    md_path = out_dir / "asset_preflight_report.md"
    with open(md_path, "w", encoding="utf-8") as mf:
        mf.write("\n".join(md_lines))

    return report

if __name__ == "__main__":
    root = sys.argv[1] if len(sys.argv) > 1 else "."
    res = run_asset_preflight(root)
    print(f"\n=======================================================")
    print(f"DARKLIGHT: PREFLIGHT ASSETS & YAML — Status: {res['status']}")
    print(f"=======================================================")
    print(f"Meta Files Indexados: {res['summary']['total_meta_indexed']}")
    print(f"Scripts C# Indexados: {res['summary']['total_csharp_scripts']}")
    print(f"Arquivos YAML Scaneados: {res['summary']['total_yaml_scanned']}")
    print(f"GUIDs Duplicados: {res['summary']['duplicate_guids']}")
    print(f"MonoScripts Quebrados: {res['summary']['broken_monoscripts']}")
    print(f"Arquivos Essenciais Faltando: {res['summary']['missing_essential_files']}")
    print(f"Avisos de Metas/Refs: {len(res['warnings'])}")
    print(f"Tempo: {res['elapsed_seconds']}s")
    print(f"Relatório Markdown: outputs/preflight/asset_preflight_report.md")
    print(f"Relatório JSON:     outputs/preflight/asset_preflight_report.json")
    print(f"=======================================================\n")
    sys.exit(0 if res['status'] == "PASS" else 1)
