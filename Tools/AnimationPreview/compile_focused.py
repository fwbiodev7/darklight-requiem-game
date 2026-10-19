"""Compile the actual Unity sources with Unity's existing response files.

No editor launch, audio or save access. Outputs stay under project work/.
"""
from pathlib import Path
import subprocess
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'work'/'game-over-validation'
OUT.mkdir(parents=True,exist_ok=True)
DOTNET=Path('C:/Program Files/Unity/Hub/Editor/6000.6.0f1/Editor/Data/DotNetSdk/dotnet.exe')
CSC=DOTNET.parent/'sdk/8.0.318/Roslyn/bincore/csc.dll'

def compile_assembly(name,source_directory,extra_references=()):
    original=ROOT/'Library/Bee/artifacts/1900b0aE.dag'/f'{name}.rsp'
    lines=original.read_text(encoding='utf-8-sig').splitlines()
    lines=[s for s in lines if not s.startswith(('-out:','-refout:','-analyzer:'))
           and not (s.strip('"').endswith('.cs'))]
    # Reference the newly compiled runtime for tests, never the old assembly.
    lines=[s for s in lines if not (name!='Darklight.Runtime' and
           s.startswith('-r:') and 'Darklight.Runtime' in s)]
    lines += [f'-out:"{OUT/name}.dll"']
    lines += [f'-r:"{p}"' for p in extra_references]
    lines += ['"'+str(p.relative_to(ROOT)).replace('\\','/')+'"'
              for p in (ROOT/source_directory).rglob('*.cs')]
    rsp=OUT/f'{name}.rsp';rsp.write_text('\n'.join(lines),encoding='utf-8')
    result=subprocess.run([str(DOTNET),str(CSC),'@'+str(rsp)],cwd=ROOT,
                          capture_output=True,text=True,encoding='utf-8',errors='replace')
    log=OUT/f'{name}.log';log.write_text(result.stdout+result.stderr,encoding='utf-8')
    errors=[s for s in (result.stdout+result.stderr).splitlines() if 'error ' in s]
    print(name,'exit='+str(result.returncode),'errors='+str(len(errors)))
    print('\n'.join(errors[:8]))
    return result.returncode

if __name__=='__main__':
    raise SystemExit(compile_assembly('Darklight.Runtime',Path('Assets/Darklight/Runtime')))
