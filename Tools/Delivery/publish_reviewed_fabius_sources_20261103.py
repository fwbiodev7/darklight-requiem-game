"""Back up reviewed source art only; never publish game Assets or untested runtime."""
from pathlib import Path
from datetime import datetime,timezone
import hashlib,json,shutil,subprocess
ROOT=Path(__file__).resolve().parents[2];PUB=ROOT/'work/publicacao-20261021'
DELIVERY=ROOT/'outputs/entrega-artes-fabius-20261103';OUT=ROOT/'outputs/retomada-20261102'
PREFIX=Path('ArtSources/ReviewedDeliveries/FabiusNormal20261103')
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def git(*args):
    p=subprocess.run(['git','-C',str(PUB),*args],capture_output=True,text=True,encoding='utf-8',errors='replace')
    if p.returncode:raise RuntimeError('git '+args[0]+': '+p.stderr[-1000:])
    return p.stdout.strip()
def main():
    if git('branch','--show-current')!='integracao-completa-20261026' or git('remote','get-url','origin')!='https://github.com/fwbiodev7/darklight-requiem-game.git':raise RuntimeError('Wrong publication destination')
    if git('diff','--cached','--name-only'):raise RuntimeError('Preserve existing index')
    manifest=json.loads((DELIVERY/'manifest.json').read_text(encoding='utf-8-sig'))
    before=git('rev-parse','HEAD');files={};published=[]
    for sheet in manifest['sheets']:
        for item in sheet['files']:
            if not item['path'].startswith('staging/'):continue
            source=DELIVERY/item['path']
            if sha(source)!=item['sha256']:raise RuntimeError('Changed reviewed delivery')
            target=PREFIX/item['path'];files[target]=source
            published.append({'path':target.as_posix(),'sha256':item['sha256']})
    files[PREFIX/'PAGES.json']=DELIVERY/'PAGES.json'
    public={'purpose':'Reviewed native source art backup, not a gameplay/runtime release',
        'totals':manifest['totals'],'files':published,'runtimeApproved':False,'gameAssetsPublished':False,
        'localIntegrationRequiresOriginalSourcesOrCompleteLocalHandoff':True,'localManifestSha256':sha(DELIVERY/'manifest.json')}
    (DELIVERY/'source-archive-manifest.json').write_text(json.dumps(public,indent=2)+'\n',encoding='utf-8')
    files[PREFIX/'manifest.json']=DELIVERY/'source-archive-manifest.json'
    (DELIVERY/'SOURCE-ARCHIVE-README.md').write_text('# Reviewed Fabius source art\n\nStatic-reviewed PNG atlases and metadata only. Not an updated playable build or approved Unity runtime. Gameplay Assets and code are deliberately excluded until their gates pass.\n\nUse the complete local handoff for integration: these descriptors may refer to original sources retained in the original workspace. Do not batch-import this archive into Assets or regenerate protected GUIDs. Excluded candidates remain excluded.\n',encoding='utf-8')
    files[PREFIX/'README.md']=DELIVERY/'SOURCE-ARCHIVE-README.md'
    for name in ['PROMPT-ANTIGRAVITY-INTEGRAR-ARTES-FABIUS-20261103.md','CORRECAO-CANONICA-FORMAS-20261102.md','HANDOFF-ANTIGRAVITY-RECONCILIACAO-20261103.md']:
        files[Path('Docs/Darklight')/name]=ROOT/'Docs/Darklight'/name
    for relative,source in files.items():
        target=PUB/relative
        if target.exists() and git('diff','--name-only','HEAD','--',relative.as_posix()):raise RuntimeError('Preserve unrelated change: '+str(relative))
        target.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(source,target)
        if sha(target)!=sha(source):raise RuntimeError('Copy mismatch')
    selection=OUT/'reviewed-art-source-publication.nul'
    selection.write_bytes(b''.join(p.as_posix().encode()+b'\0' for p in sorted(files)))
    git('add','--force','--pathspec-from-file='+str(selection),'--pathspec-file-nul')
    staged=git('diff','--cached','--name-only').splitlines()
    if not set(staged)<=set(p.as_posix() for p in files):raise RuntimeError('Unexpected index')
    git('diff','--cached','--check')
    if staged:
        body=OUT/'reviewed-art-source-commit.txt'
        body.write_text('art: preserve reviewed Fabius atlases and integration handoff\n\nSource art only; no game Assets, runtime code or build included.\nUnity integration and functional validation remain pending.\n',encoding='utf-8')
        git('commit','--file',str(body))
    head=git('rev-parse','HEAD')
    receipt={'updatedUtc':datetime.now(timezone.utc).isoformat(),'commit':head,'previousHead':before,
        'branch':'integracao-completa-20261026','remote':'https://github.com/fwbiodev7/darklight-requiem-game.git',
        'scope':'reviewed-source-art-and-handoff-only','reviewedArtSourcesPublished':True,'gameAssetsPublished':False,
        'runtimeCodePublished':False,'runtimePublicationGateBypassed':False,'pushed':False,'files':staged}
    final=OUT/'final-git-receipt.json'
    if final.exists():shutil.copy2(final,OUT/'final-git-receipt-before-source-art.json')
    try:
        git('push','origin','HEAD:refs/heads/integracao-completa-20261026')
        if git('ls-remote','origin','refs/heads/integracao-completa-20261026').split()[0]!=head:raise RuntimeError('Remote mismatch')
        receipt['pushed']=True
    except Exception as error:receipt['error']=str(error);raise
    finally:final.write_text(json.dumps(receipt,indent=2)+'\n',encoding='utf-8')
    print(json.dumps({'commit':head,'pushed':True,'files':len(staged),'sourceSheets':manifest['totals']['sheets'],'gameAssetsPublished':False}))
if __name__=='__main__':main()
