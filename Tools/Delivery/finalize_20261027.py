"""Run the reviewed import, fresh test gates and Windows build sequentially."""
from datetime import datetime,timezone
import hashlib
import json
from pathlib import Path
import subprocess
import sys
import xml.etree.ElementTree as ET
import zipfile

ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/"outputs/retomada-20261027"
WORK=ROOT/"work/retomada-20261027/release"
UNITY=Path(r"C:\Program Files\Unity\Hub\Editor\6000.6.0f1\Editor\Unity.exe")
state=dict(stages=[],buildSucceeded=False,standaloneExecuted=False,manualGameplayCompleted=False,zip=None)
def save():
    OUT.mkdir(parents=True,exist_ok=True)
    (OUT/"release-progress.json").write_text(json.dumps(state,indent=2)+"\n",encoding="utf-8")
def now():return datetime.now(timezone.utc).isoformat()
def execute(name,args,timeout=600,xml=None):
    row=dict(stage=name,started=now(),completed=False);state["stages"].append(row);save()
    log=WORK/(name+".log");stdout=WORK/(name+"-stdout.log")
    command=[str(UNITY),"-batchmode","-darklightSilent","-projectPath",str(ROOT),"-logFile",str(log),*args]
    with stdout.open("wb") as stream:
        process=subprocess.Popen(command,cwd=ROOT,stdout=stream,stderr=subprocess.STDOUT)
        try:code=process.wait(timeout=timeout)
        except subprocess.TimeoutExpired:
            subprocess.run(["taskkill","/PID",str(process.pid),"/T","/F"],stdout=stream,stderr=stream)
            raise RuntimeError(name+" timed out")
    row.update(exitCode=code,ended=now(),log=str(log))
    if code!=0:save();raise RuntimeError(name+" failed; inspect "+str(log))
    if xml:
        if not xml.is_file():raise RuntimeError("No new test XML: "+name)
        result=ET.parse(xml).getroot();cases=list(result.iter("test-case"))
        row.update(cases=len(cases),passed=sum(c.get("result")=="Passed" for c in cases))
        if result.get("result")!="Passed" or not cases or any(c.get("result")!="Passed" for c in cases):
            row["failedCases"]=[c.get("fullname") for c in cases if c.get("result")!="Passed"];save()
            raise RuntimeError("Test gate failed: "+name)
    row["completed"]=True;save();print(json.dumps(row),flush=True)

def main():
    WORK.mkdir(parents=True,exist_ok=True);OUT.mkdir(parents=True,exist_ok=True)
    if (ROOT/"Temp/UnityLockfile").exists():raise RuntimeError("Unity project is already locked")
    selection=json.loads((ROOT/"work/retomada-20261026/final-gate-selection.json").read_text())
    edits=selection["editClasses"]+["Darklight.Tests.DarklightSaveContractTests","Darklight.Tests.DarklightTestOutputTests"]
    play=selection["playFilters"]+[
        "Darklight.Tests.DarklightWeaponRedesignGameplayTests.EveryActiveWeaponStateExcludesRetiredCharacterFrames",
        "Darklight.Tests.DarklightWeaponRedesignGameplayTests.ReviewedKatanaWalkLoopsReachAllDeclaredFramesAndReturnToIdle",
        "Darklight.Tests.DarklightWeaponRedesignGameplayTests.ConfirmedContactEmphasizesWholePoseButWhiffsDoNot"]
    stamp=datetime.now().strftime("%H%M%S")
    edit=OUT/("final-edit-"+stamp+".xml");played=OUT/("final-play-"+stamp+".xml")
    if "--skip-import" in sys.argv:
        previous=json.loads((OUT/"release-progress.json").read_text())
        imported=next((s for s in previous["stages"] if s["stage"]=="import" and s["completed"]),None)
        if imported is None:raise RuntimeError("No successful import to reuse")
        state["stages"].append(imported);save()
    else:execute("import",["-quit","-executeMethod","Darklight.Editor.DarklightScope26Delivery.Import"])
    execute("edit",["-runTests","-testPlatform","EditMode","-testFilter",";".join(edits),"-testResults",str(edit)],xml=edit)
    execute("play",["-runTests","-testPlatform","PlayMode","-testFilter",";".join(play),"-testResults",str(played)],xml=played)
    execute("build",["-quit","-executeMethod","Darklight.Editor.DarklightWeaponRedesignBuild.Build",
        "-darklightRedesignEditResults",str(edit),"-darklightRedesignPlayResults",str(played)],timeout=900)
    receipt=json.loads((OUT/"build-result.json").read_text());exe=OUT/"Windows/DarklightRequiem.exe"
    if receipt.get("result")!="Succeeded" or not exe.is_file():raise RuntimeError("Missing successful build receipt/executable")
    state["buildSucceeded"]=True;save()
    archive=OUT/("DarklightRequiem-Windows-20261027-"+stamp+".zip")
    with zipfile.ZipFile(archive,"x",zipfile.ZIP_DEFLATED,compresslevel=3) as bundle:
        for path in sorted((OUT/"Windows").rglob("*")):
            if path.is_file() and path.suffix.lower()!=".pdb" and not any("DoNotShip" in part for part in path.parts):
                bundle.write(path,Path("DarklightRequiem")/path.relative_to(OUT/"Windows"))
    state["zip"]=str(archive);state["zipBytes"]=archive.stat().st_size
    with archive.open("rb") as stream:state["zipSha256"]=hashlib.file_digest(stream,"sha256").hexdigest()
    state["finished"]=now();save();print(json.dumps(state),flush=True)

if __name__=="__main__":
    try:main()
    except Exception as error:
        state["error"]=str(error);save();print(str(error),file=sys.stderr);raise SystemExit(1)
