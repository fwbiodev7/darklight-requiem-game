"""Audit committed technical changes between explicit Git references; fail closed."""
import argparse
import json
from pathlib import Path
import subprocess

DEFAULT_BASE="b719c527c736009e3ff560fd124ad07f67ae30d0"

def get_git_output(cmd,cwd="."):
    return subprocess.check_output(cmd,cwd=cwd,text=True,encoding="utf-8",stderr=subprocess.PIPE).strip()

def verify_integration(project_root=".",base_commit=DEFAULT_BASE,ref="HEAD",output_root=None):
    git=lambda *args:get_git_output(["git",*args],project_root)
    base=git("rev-parse",base_commit+"^{commit}");head=git("rev-parse",ref+"^{commit}")
    categorized={key:[] for key in ("docs","tests","tools","runtime","editor","art","other")}
    for line in git("diff","--no-renames","--name-status",base,head).splitlines():
        status,path=line.split("\t",1);lower=path.lower()
        if lower.endswith((".png",".jpg",".jpeg",".psd",".aseprite",".anim",".controller",".prefab",".unity")) or path.startswith(("ArtSources/","Assets/Darklight/Art/")):category="art"
        elif path.startswith("Docs/"):category="docs"
        elif "/Tests/" in path:category="tests"
        elif path.startswith("Tools/"):category="tools"
        elif "/Runtime/" in path:category="runtime"
        elif "/Editor/" in path:category="editor"
        else:category="other"
        categorized[category].append(dict(status=status,path=path))
    commits=[]
    for row in git("log","--format=%H %s",base+".."+head).splitlines():
        sha,subject=row.split(" ",1);commits.append(dict(hash=sha,subject=subject))
    manifest=dict(repository="fwbiodev7/darklight-requiem-game",base_commit=base,head_commit=head,
        branch=git("branch","--show-current"),comparison_ref=ref,commits_ahead_count=len(commits),commits=commits,
        file_summary={key+"_count":len(value) for key,value in categorized.items()},categorized_files=categorized,
        technical_boundary_passed=not categorized["runtime"] and not categorized["art"],
        runtime_code_modified=bool(categorized["runtime"]),art_modified_or_generated=bool(categorized["art"]),
        limits=["Committed reference comparison only; working-tree changes are not included", "No Unity compilation, save compatibility or gameplay claim follows from this inventory"])
    target=Path(output_root or Path(project_root)/"outputs/validation");target.mkdir(parents=True,exist_ok=True)
    (target/"integration_manifest.json").write_text(json.dumps(manifest,ensure_ascii=False,indent=2)+"\n",encoding="utf-8")
    return manifest

if __name__=="__main__":
    parser=argparse.ArgumentParser(description=__doc__);parser.add_argument("project",nargs="?",default=".")
    parser.add_argument("--base",default=DEFAULT_BASE);parser.add_argument("--ref",default="HEAD");parser.add_argument("--output")
    args=parser.parse_args()
    try:result=verify_integration(args.project,args.base,args.ref,args.output)
    except (subprocess.CalledProcessError,ValueError,OSError) as error:
        print(json.dumps(dict(status="ERROR",error=str(error))));raise SystemExit(2)
    print(json.dumps(dict(status="PASS" if result["technical_boundary_passed"] else "FAIL",files=result["file_summary"],head=result["head_commit"])))
    raise SystemExit(0 if result["technical_boundary_passed"] else 1)
