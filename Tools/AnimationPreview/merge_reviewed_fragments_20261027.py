"""Integrate reviewed complete-figure fragments without replacing other contexts."""
import argparse
import hashlib
import json
from pathlib import Path
import shutil

ROOT=Path(__file__).resolve().parents[2]
BANK=ROOT/"Assets/Darklight/Resources/Darklight/WeaponRedesign20261023"
OUT=ROOT/"outputs/retomada-20261027"
def read(path):return json.loads(path.read_text(encoding="utf-8-sig"))
def sha(path):return hashlib.sha256(path.read_bytes()).hexdigest()
def context(page):return tuple(page.get(key,"") for key in ("skin","weapon","form","direction","action","attackKey"))

def main():
    parser=argparse.ArgumentParser(description=__doc__);parser.add_argument("fragments",nargs="+",type=Path);args=parser.parse_args()
    catalog_path=BANK/"catalog.json";before=catalog_path.read_bytes();catalog=read(catalog_path)
    known={context(page):page for page in catalog["pages"]};incoming=[];copied=[]
    for fragment_path in args.fragments:
        fragment_path=fragment_path.resolve()
        if not fragment_path.is_relative_to(ROOT):raise ValueError("Fragment escaped workspace")
        for page in read(fragment_path)["pages"]:
            key=context(page)
            if key in known:
                if known[key]["imageSha256"]!=page["imageSha256"]:raise ValueError("Existing context would be replaced: "+page["id"])
                continue
            name=Path(page["resourcePath"]).name
            for suffix in (".json",".png"):
                target=BANK/(name+suffix)
                if target.is_file():continue
                candidates=[fragment_path.parent/folder/(name+suffix) for folder in ("payload","staged")]
                source=next((path for path in candidates if path.is_file()),None)
                if source is None:raise FileNotFoundError(name+suffix)
                shutil.copy2(source,target);copied.append(target.relative_to(ROOT).as_posix())
            sheet=read(BANK/(name+".json"))
            if not sheet.get("wholeFigures") or sheet.get("status")!="visual-reviewed":raise ValueError("Unreviewed whole sheet")
            for field in ("imageSha256","sourceSha256","sourcePath","pixelsPerUnit"):
                if sheet[field]!=page[field]:raise ValueError("Page/sheet mismatch: "+field)
            source=(ROOT/page["sourcePath"]).resolve()
            if not source.is_relative_to(ROOT/"ArtSources"):raise ValueError("Source escaped ArtSources")
            if sha(source)!=page["sourceSha256"] or sha(BANK/(name+".png"))!=page["imageSha256"]:raise ValueError("Source/atlas hash mismatch")
            known[key]=page;incoming.append(page)
    OUT.mkdir(parents=True,exist_ok=True)
    backup=OUT/("catalog-before-"+hashlib.sha256(before).hexdigest()[:12]+".json")
    if not backup.exists():backup.write_bytes(before)
    catalog["pages"].extend(incoming);catalog["complete"]=False
    catalog_path.write_text(json.dumps(catalog,ensure_ascii=False,indent=2)+"\n",encoding="utf-8")
    result=dict(addedClips=len(incoming),catalogClips=len(catalog["pages"]),copied=copied,
        pageIds=[page["id"] for page in incoming],catalogSha256=sha(catalog_path),unityImported=False,runtimeVerified=False)
    (OUT/"fragment-integration.json").write_text(json.dumps(result,indent=2)+"\n",encoding="utf-8")
    print(json.dumps({key:result[key] for key in ("addedClips","catalogClips","catalogSha256")}))

if __name__=="__main__":main()
