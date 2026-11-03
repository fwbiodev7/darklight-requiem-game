"""Refresh factual animation coverage from the current catalog and delivery receipts."""
from datetime import datetime,timezone
import hashlib,json
from pathlib import Path

ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'outputs/retomada-20261102'
DIRS=['S','SW','W','NW','N','NE','E','SE']
def read(path):return json.loads(path.read_text(encoding='utf-8-sig'))
def write(path,data):path.write_text(json.dumps(data,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')

def main():
    catalog_path=ROOT/'Assets/Darklight/Resources/Darklight/WeaponRedesign20261023/catalog.json'
    catalog=read(catalog_path)
    deliveries=[d for d in read(OUT/'art-integration.json')['deliveries'] if not d.get('supersededBy')]
    by_name={d['name']:d for d in deliveries}
    coverage={}
    for page in catalog['pages']:
        if page['skin']!=0 or page['form']!=0 or page['weapon'] not in (0,2,3):continue
        name=page['resourcePath'].rsplit('/',1)[-1]
        action=page['action']+(':'+page['attackKey'] if page.get('attackKey') else '')
        mode=page.get('presentation') or 'OnFoot'
        weapon={0:'Katana',2:'Punhos',3:'Moto'}[page['weapon']]
        row=coverage.setdefault(weapon,{}).setdefault(mode,{}).setdefault(action,{'directions':[],'missingDirections':[],'clips':[]})
        row['directions'].append(DIRS[page['direction']])
        delivery=by_name.get(name)
        row['clips'].append({'direction':DIRS[page['direction']],'id':page['id'],'resourcePath':page['resourcePath'],
            'figures':len(page['spriteNames']),'visualReviewed':page['status'] in ('visual-reviewed','imported','runtime-verified'),
            'unityImported':delivery.get('unityImported',False) if delivery else None,
            'runtimeVerified':delivery.get('runtimeVerified',False) if delivery else None,
            'statusScope':'session-receipt' if delivery else 'previous-session-see-historical-receipts'})
    for modes in coverage.values():
        for actions in modes.values():
            for row in actions.values():
                row['directions']=sorted(set(row['directions']),key=DIRS.index)
                row['missingDirections']=[d for d in DIRS if d not in row['directions']]
    totals={'clips':sum(d['pages'] for d in deliveries),'figures':sum(d['figures'] for d in deliveries),
        'sheets':len(deliveries),'unityImportedClips':sum(d['pages'] for d in deliveries if d.get('unityImported')),
        'historicallyRuntimeVerifiedClips':sum(d['pages'] for d in deliveries if d.get('runtimeVerified'))}
    state={'updatedUtc':datetime.now(timezone.utc).isoformat(),'scope':'Réquiem Normal; session delivery totals only',
        'complete':False,'testsDeferredByUser':True,'currentRuntimeChangesTested':False,
        'catalogSha256':hashlib.sha256(catalog_path.read_bytes()).hexdigest(),'totals':totals,'coverage':coverage,
        'runtimeSupportPendingFinalQA':['Mounted/OnFoot explicit presentation contexts','Death StateTime while combat clock remains frozen','Contactless aerial chase phases'],
        'latestUserOrder':'Finalize Fabius Normal today, preserve approved compact proportions; integrate finished art immediately, defer test suites until production is complete.'}
    state.update(scopeAuthority='Docs/Darklight/ESCOPO-ENXUTO-FABIUS-20261102.md',
        requiredNewStateFamilies=['Parry','Hurt','Dead'],
        removedFromGenerationQueue=['4 extra Katana techniques','4 extra Fists techniques','5 unique aerial sequences per weapon','12 separate state/idle variants','18 unique Moto technique clips'],
        intentionalSharedAnimations=True,gameplayDamageAndCommandsUnchanged=True,
        remainingCoreWork=[weapon+' '+action+': '+','.join(DIRS[d] for d in range(8) if DIRS[d] not in modes.get('OnFoot',{}).get(action,{}).get('directions',[]))
            for weapon,modes in coverage.items() for action in ('Walk','Dodge','Parry','Hurt','Dead')
            if len(modes.get('OnFoot',{}).get(action,{}).get('directions',[]))<8])
    write(OUT/'animation-progress-current.json',state)
    continuation=read(OUT/'continuation-state.json')
    continuation.update(updatedUtc=state['updatedUtc'],buildCancelledByUser=False,transformationsDeferredByUser=True,
        testsDeferredByUser=True,currentFocus='Fabius Réquiem Normal; attacks and walks before remaining states',
        latestAnimationProgress='outputs/retomada-20261102/animation-progress-current.json',sessionDeliveryTotals=totals,
        currentRuntimeChangesTested=False,complete=False)
    continuation.update(scopeAuthority=state['scopeAuthority'],remainingCoreWork=state['remainingCoreWork'],
        cancelledExpansionQueue=state['removedFromGenerationQueue'])
    local=continuation.get('localImplementation',{})
    local.update(newClips=totals['clips'],newFigures=totals['figures'],allSessionClipsUnityImported=totals['unityImportedClips']==totals['clips'],
        runtimeVerified=False,buildCancelledByUser=False,transformationsDeferred=True,
        previousValidation='First15clips passed3isolatedPlayMode tests before latest runtime changes; see integration-play-115427.xml')
    continuation['localImplementation']=local
    write(OUT/'continuation-state.json',continuation)
    implementation_path=OUT/'implementation-receipt.json'
    implementation=read(implementation_path)
    if 'first15Checkpoint' not in implementation:
        implementation['first15Checkpoint']={k:implementation.get(k) for k in
            ('tasks','newClips','newFigures','unityImported','runtimeVerified','runtimeVerification')}
    implementation.update(updatedUtc=state['updatedUtc'],newClips=totals['clips'],newFigures=totals['figures'],
        sheets=totals['sheets'],unityImported=totals['unityImportedClips']==totals['clips'],runtimeVerified=False,
        testsDeferredByUser=True,currentRuntimeChangesTested=False,buildCancelledByUser=False,transformationsDeferred=False,
        currentProductionIncludesTransformations=False,tasksScope='Historical first15 packet; current deliveries in art-integration.json',
        latestAnimationProgress='outputs/retomada-20261102/animation-progress-current.json')
    write(implementation_path,implementation)
    print(json.dumps(totals))

if __name__=='__main__':main()
