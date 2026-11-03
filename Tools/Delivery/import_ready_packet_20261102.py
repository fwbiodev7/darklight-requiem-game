"""Import an explicitly reviewed packet without running tests or building the player."""
import argparse
from datetime import datetime,timezone
import json
from pathlib import Path
import subprocess
import finalize_20261027 as gate
import run_guarded_build_20261101 as guarded

ROOT=Path(__file__).resolve().parents[2]

def main():
    parser=argparse.ArgumentParser(description=__doc__)
    selection=parser.add_mutually_exclusive_group(required=True)
    selection.add_argument('--request',type=Path)
    selection.add_argument('--pending',action='store_true',help='Snapshot every reviewed delivery copied to Assets but not yet imported')
    args=parser.parse_args()
    out=ROOT/'outputs/retomada-20261102'
    if args.pending:
        deliveries=json.loads((out/'art-integration.json').read_text(encoding='utf-8-sig'))['deliveries']
        pending=[d for d in deliveries if d.get('copiedToAssets') and not d.get('unityImported') and not d.get('supersededBy')]
        if not pending:
            print(json.dumps({'pendingClips':0,'testsExecuted':False}));return
        stamp=datetime.now().strftime('%H%M%S')
        request_path=out/('ready-packet-'+stamp+'-request.json')
        request={'resources':['Darklight/WeaponRedesign20261023/'+d['name'] for d in pending],
            'clips':sum(d['pages'] for d in pending),'figures':sum(d['figures'] for d in pending),
            'receiptPath':'outputs/retomada-20261102/ready-packet-'+stamp+'-import.json'}
        request_path.write_text(json.dumps(request,indent=2)+'\n',encoding='utf-8')
    else:
        request_path=args.request.resolve()
    if not request_path.is_relative_to(ROOT/'outputs'):
        raise ValueError('Request must remain within project outputs')
    request=json.loads(request_path.read_text(encoding='utf-8-sig'))
    receipt_path=(ROOT/request['receiptPath']).resolve()
    if not receipt_path.is_relative_to(ROOT/'outputs'):
        raise ValueError('Receipt must remain within project outputs')
    # Do not race an Editor or revive the previous interrupted run.
    active=subprocess.run(['tasklist','/FI','IMAGENAME eq Unity.exe','/FO','CSV','/NH'],capture_output=True,text=True,check=True)
    if 'Unity.exe' in active.stdout:
        raise RuntimeError('Unity is already active')
    value=guarded.memory()
    if value.available<3.5*1024**3 or value.pageAvailable<4*1024**3:
        raise RuntimeError('Insufficient memory headroom to start import safely')
    gate.OUT=ROOT/'outputs/retomada-20261102'
    gate.WORK=ROOT/'work/retomada-20261102'/('packet-import-'+datetime.now().strftime('%H%M%S'))
    gate.WORK.mkdir(parents=True,exist_ok=True)
    progress=gate.OUT/'release-progress.json'
    gate.state=json.loads(progress.read_text()) if progress.exists() else {'stages':[]}
    gate.state.update(testsDeferredByUser=True,latestOperation='importReviewedPacketOnly',latestRequest=str(request_path.relative_to(ROOT)))
    gate.state.pop('error',None)
    guarded.MONITOR=receipt_path.with_name(receipt_path.stem+'-memory.json')
    try:
        started=datetime.now(timezone.utc)
        guarded.guarded_execute('import-reviewed',['-nographics','-quit','-executeMethod',
            'Darklight.Editor.DarklightReadyPacketDelivery.Import','-darklightImportRequest',str(request_path)])
        receipt=json.loads(receipt_path.read_text(encoding='utf-8-sig'))
        if not receipt.get('unityImported') or set(receipt['resources'])!=set(request['resources']):
            raise RuntimeError('Import receipt missing or mismatched')
        if datetime.fromisoformat(receipt['completedUtc'].replace('Z','+00:00'))<started:
            raise RuntimeError('Old import receipt cannot validate the new operation')
        integration=gate.OUT/'art-integration.json'
        data=json.loads(integration.read_text(encoding='utf-8-sig'))
        names={r.rsplit('/',1)[-1] for r in request['resources']}
        for delivery in data['deliveries']:
            if delivery['name'] in names:
                delivery.update(unityImported=True,runtimeVerified=False,testsDeferredByUser=True,importReceipt=str(receipt_path.relative_to(ROOT)))
        integration.write_text(json.dumps(data,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
        gate.state.update(latestPacketImported=True,latestPacketRuntimeVerified=False)
        gate.save()
        print(json.dumps({'imported':True,'clips':receipt['clips'],'figures':receipt['figures'],'testsExecuted':False}),flush=True)
    except Exception as error:
        gate.state.update(latestPacketImported=False,error=str(error));gate.save();raise

if __name__=='__main__':main()
