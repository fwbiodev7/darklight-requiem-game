from pathlib import Path
from compile_focused import compile_assembly,OUT
runtime=compile_assembly('Darklight.Runtime',Path('Assets/Darklight/Runtime'))
editor=compile_assembly('Darklight.Editor',Path('Assets/Darklight/Editor'),[OUT/'Darklight.Runtime.dll']) if runtime==0 else 1
tests=compile_assembly('Darklight.EditModeTests',Path('Assets/Darklight/Tests/EditMode'),[OUT/'Darklight.Runtime.dll']) if runtime==0 else 1
raise SystemExit(0 if runtime==editor==tests==0 else 2)
