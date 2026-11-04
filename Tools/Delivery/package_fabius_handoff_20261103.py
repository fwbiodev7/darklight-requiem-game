"""Snapshot reviewed whole-figure deliveries, without applying art or running Unity."""
from datetime import datetime,timezone
from pathlib import Path
import hashlib,json,shutil
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'outputs/retomada-20261102'
DELIVERY=ROOT/'outputs/entrega-artes-fabius-20261103'
BANK=ROOT/'Assets/Darklight/Resources/Darklight/WeaponRedesign20261023'
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def write(p,d):p.write_text(json.dumps(d,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
def copy(source,target):
    before=sha(source);target.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(source,target)
    if sha(source)!=before or sha(target)!=before:raise RuntimeError('File changed during snapshot: '+str(source))
    return {'path':target.relative_to(DELIVERY).as_posix(),'sha256':before,'bytes':target.stat().st_size}
def main():
    control=read(DELIVERY/'CONTROLE-DE-INTEGRACAO.json')
    if control['owner']!='Codex' or control['state']!='PRODUCING':raise RuntimeError('Preserve current integration owner')
    rows=read(OUT/'art-integration.json')['deliveries'];byname={r['name']:r for r in rows}
    current=read(BANK/'catalog.json');selected={};origins={}
    def collect(folder,fragment):
        data=read(fragment);pages=data if isinstance(data,list) else data['pages']
        for page in pages:
            if page['skin']!=0 or page['form']!=0 or page['status'] not in ('visual-reviewed','imported','runtime-verified'):continue
            name=page['resourcePath'].rsplit('/',1)[-1]
            key=tuple(page.get(k,'') for k in ('skin','weapon','form','direction','action','attackKey','presentation'))
            selected[key]=page;origins[name]=folder
    initial={'pages':[p for p in current['pages'] if p['resourcePath'].rsplit('/',1)[-1] in byname]}
    seed=DELIVERY/'catalog-selected-current.json';write(seed,initial);collect(BANK,seed)
    for folder,fragment in [
        ('essential-states/staging','essential-states/catalog-fragment-katana.json'),
        ('essential-states/staging','essential-states/catalog-fragment-fists.json'),
        ('essential-states/staging','essential-states/catalog-fragment-moto.json'),
        ('moto-walk-close/staging','moto-walk-close/Walk-six-directions-pages.json')]:
        base=ROOT/'outputs/retomada-20261103'
        if (base/fragment).is_file():collect(base/folder,base/fragment)
    pages=list(selected.values());sheets=[]
    for name in sorted({p['resourcePath'].rsplit('/',1)[-1] for p in pages}):
        source=origins[name];descriptor=read(source/(name+'.json'));image=source/(name+'.png')
        original=(ROOT/descriptor['sourcePath']).resolve()
        if not original.is_relative_to(ROOT/'ArtSources'):raise RuntimeError('Unexpected source path')
        if descriptor.get('status') not in ('visual-reviewed','imported','runtime-verified') or not descriptor.get('wholeFigures'):
            raise RuntimeError('Unreviewed delivery: '+name)
        if sha(image)!=descriptor['imageSha256'] or sha(original)!=descriptor['sourceSha256']:
            raise RuntimeError('Changed source/image: '+name)
        files=[copy(image,DELIVERY/'staging'/image.name),copy(source/(name+'.json'),DELIVERY/'staging'/(name+'.json')),
            copy(original,DELIVERY/'sources'/original.relative_to(ROOT))]
        # Metas remain an explicit existing-workspace reference, not a request to regenerate IDs.
        meta={suffix:sha(BANK/(name+suffix)) for suffix in ('.png.meta','.json.meta') if (BANK/(name+suffix)).is_file()}
        old=byname.get(name,{})
        sheets.append({'name':name,'files':files,'target':'Assets/Darklight/Resources/Darklight/WeaponRedesign20261023/'+name,
            'existingMetasSha256':meta,'copiedToAssetsReceipt':old.get('copiedToAssets',False),
            'unityImportedReceipt':old.get('unityImported',False),'runtimeVerifiedReceipt':old.get('runtimeVerified',False),
            'figures':len(descriptor['frames']),'clips':sum(p['resourcePath'].endswith('/'+name) for p in pages)})
    write(DELIVERY/'PAGES.json',{'schemaVersion':1,'complete':False,'pages':pages})
    code=['Assets/Darklight/Runtime/Character/DarklightWeaponRedesignBank.cs','Assets/Darklight/Runtime/Character/DarklightCharacterView.cs',
        'Assets/Darklight/Runtime/World/DarklightActionParticles.cs','Assets/Darklight/Runtime/World/DarklightCombatImpacts.cs',
        'Assets/Darklight/Runtime/Character/DarklightFormController.cs','Tools/Delivery/integrate_reviewed_20261031.py',
        'Tools/Delivery/import_ready_packet_20261102.py','Tools/Delivery/run_guarded_build_20261101.py']
    codefiles=[copy(ROOT/p,DELIVERY/'workspace-code-snapshot'/p) for p in code]
    manifest={'generatedUtc':datetime.now(timezone.utc).isoformat(),'state':'reviewed-art-snapshot-not-runtime-approved',
        'root':str(ROOT),'sheets':sheets,'pagesFile':{'path':'PAGES.json','sha256':sha(DELIVERY/'PAGES.json')},
        'codeSnapshots':codefiles,'codeSnapshotIsNotAnAutomaticPatch':True,'filesAreImmutableAfterRelease':True,
        'totals':{'sheets':len(sheets),'clips':len(pages),'frames':sum(s['figures'] for s in sheets)},
        'candidatesExcluded':['FistsSW V16 (scale mismatch)','FistsSW V17 (fixed scale; gait not proven)',
            'MotoWalkW/E V1-V3 (support/camera unresolved)','superseded state sources'],
        'remaining':['FistsWalkSW gait closure','MotoWalkW/E gait closure','Current Unity import and functional gates',
            'Validate Moto guard sparks and occlusion','Fabius completion gate before Gothic/Lucius'],
        'runtimeApproved':False,'handoffReleased':False}
    write(DELIVERY/'manifest.json',manifest)
    (DELIVERY/'PENDENCIAS.md').write_text('# Pendências reais\n\n'+''.join('- '+x+'\n' for x in manifest['remaining'])+
        '\nCandidatos excluídos:\n'+''.join('- '+x+'\n' for x in manifest['candidatesExcluded'])+
        '\nSomente o controle RELEASED transfere a integração. Não inferir aprovação runtime pelos PNGs revisados.\n',encoding='utf-8')
    print(json.dumps(manifest['totals']))
if __name__=='__main__':main()
