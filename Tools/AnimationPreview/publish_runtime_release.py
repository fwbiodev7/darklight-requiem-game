"""Publish the explicitly authorized game release; use GCM credentials only in memory."""
from pathlib import Path
import argparse,json,os,subprocess,urllib.request,urllib.parse,urllib.error,http.client,hashlib,sys
ROOT=Path(__file__).resolve().parents[2];CLONE=ROOT/'work/publish-review-20261017'
OUT=ROOT/'outputs/revisao-runtime-20261019';REPO='fwbiodev7/darklight-requiem-game'
API='https://api.github.com/repos/'+REPO
def credentials():
    env=dict(os.environ,GCM_INTERACTIVE='Never',GIT_TERMINAL_PROMPT='0')
    result=subprocess.run(['git','credential','fill'],input='protocol=https\nhost=github.com\npath='+REPO+'.git\n\n',cwd=CLONE,env=env,capture_output=True,text=True,check=True)
    fields=dict(line.split('=',1) for line in result.stdout.splitlines() if '=' in line)
    if not fields.get('password'):raise RuntimeError('GitHub credentials unavailable')
    return fields['password']
def headers(token):return {'Authorization':'Bearer '+token,'Accept':'application/vnd.github+json','X-GitHub-Api-Version':'2026-03-10','User-Agent':'DARKLIGHT-release','Content-Type':'application/json'}
def api(token,path='',method='GET',data=None):
    request=urllib.request.Request(API+path,method=method,headers=headers(token),data=None if data is None else json.dumps(data).encode())
    with urllib.request.urlopen(request,timeout=90) as r:return json.load(r)
def asset_summary(a):return {k:a.get(k) for k in ['id','name','size','state','digest','browser_download_url']}
def upload(token,release,path):
    expected='sha256:'+hashlib.sha256(path.read_bytes()).hexdigest()
    old=next((a for a in api(token,'/releases/'+str(release['id'])+'/assets') if a['name']==path.name),None)
    if old:
        if old['size']==path.stat().st_size and old.get('digest')==expected:return old
        raise RuntimeError('Existing release asset differs; refusing to overwrite it')
    url=urllib.parse.urlsplit(release['upload_url'].split('{')[0]+'?'+urllib.parse.urlencode({'name':path.name}))
    if url.scheme!='https' or url.hostname!='uploads.github.com':raise RuntimeError('Unexpected asset host')
    conn=http.client.HTTPSConnection(url.hostname,timeout=600)
    h=headers(token);h.update({'Content-Type':'application/zip' if path.suffix=='.zip' else 'text/plain','Content-Length':str(path.stat().st_size)})
    conn.putrequest('POST',url.path+'?'+url.query)
    for k,v in h.items():conn.putheader(k,v)
    conn.endheaders()
    sent=0;last=0
    with path.open('rb') as f:
        for chunk in iter(lambda:f.read(4*1024*1024),b''):
            conn.send(chunk);sent+=len(chunk)
            if sent-last>=128*1024*1024:
                print(json.dumps(dict(upload=path.name,sentBytes=sent,totalBytes=path.stat().st_size)),flush=True);last=sent
    response=conn.getresponse();payload=response.read();conn.close()
    if response.status not in [200,201]:raise RuntimeError('Asset upload HTTP '+str(response.status))
    result=json.loads(payload)
    if result['size']!=path.stat().st_size or result.get('digest')!=expected:raise RuntimeError('Uploaded asset checksum/size mismatch')
    return result
def main():
    p=argparse.ArgumentParser();p.add_argument('--inspect',action='store_true');p.add_argument('--draft',action='store_true');p.add_argument('--upload',action='store_true');p.add_argument('--publish',action='store_true');p.add_argument('--commit');p.add_argument('--body-file');args=p.parse_args()
    token=credentials();releases=api(token,'/releases?per_page=20');r=next((x for x in releases if x['tag_name']=='v1.0.13'),None)
    if args.inspect:print(json.dumps([dict(id=x['id'],tag=x['tag_name'],draft=x['draft'],url=x['html_url'],assets=[asset_summary(a) for a in x['assets']]) for x in releases[:4]]));return
    if args.draft:
        if not args.commit or not args.body_file:raise RuntimeError('Exact pushed commit and reviewed body required')
        if r and not r['draft']:raise RuntimeError('Published version already exists')
        if not r:r=api(token,'/releases','POST',dict(tag_name='v1.0.13',target_commitish=args.commit,name='DARKLIGHT: REQUIEM 1.0.13 — revisão de combate',body=Path(args.body_file).read_text(encoding='utf-8'),draft=True,prerelease=False))
    if r is None:raise RuntimeError('Release draft not found')
    if args.upload:
        if not r['draft']:raise RuntimeError('Asset upload requires a draft')
        for name in ['DarklightRequiem-1.0.13-Windows-x64.zip','SHA256.txt']:upload(token,r,OUT/name)
        r=api(token,'/releases/'+str(r['id']))
    if args.publish:
        assets=api(token,'/releases/'+str(r['id'])+'/assets');expected=json.loads((OUT/'package-result.json').read_text())
        z=next((a for a in assets if a['name']==expected['archive']),None)
        if not z or z['size']!=expected['bytes'] or z.get('digest')!='sha256:'+expected['sha256']:raise RuntimeError('Validated build must be uploaded before publication')
        update=dict(draft=False,make_latest='true')
        if args.body_file:update['body']=Path(args.body_file).read_text(encoding='utf-8')
        try:r=api(token,'/releases/'+str(r['id']),'PATCH',update)
        except urllib.error.HTTPError as error:
            if error.code<500:raise
            observed=api(token,'/releases/'+str(r['id']))
            if observed['draft']:raise
            r=observed # An ambiguous server error may follow a successful publication.
    report=dict(id=r['id'],tag=r['tag_name'],draft=r['draft'],url=r['html_url'],target=r['target_commitish'],assets=[asset_summary(a) for a in r['assets']])
    (OUT/'github-release-result.json').write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8');print(json.dumps(report))
if __name__=='__main__':
    try:main()
    except Exception as error:print(type(error).__name__+': '+str(error),file=sys.stderr);sys.exit(1)
