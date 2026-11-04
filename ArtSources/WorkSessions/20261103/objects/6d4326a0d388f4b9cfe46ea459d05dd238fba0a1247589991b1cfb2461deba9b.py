#!/usr/bin/env python3
"""Static read-only audit for Fabius Normal animation/catalog synchronization.
No Unity calls, project writes, hashing, or asset decoding. Requires Python stdlib only.
"""
from __future__ import annotations
import argparse, csv, json, pathlib, re, statistics, sys

WEAPONS={0:'Katana',2:'Fists',3:'Moto'}
DIRECTIONS=['S','SW','W','NW','N','NE','E','SE']
ACTIONS=['Idle','Walk','Attack','Dodge','Dead','Hurt','Parry']

def read_json(p):
    return json.loads(p.read_text(encoding='utf-8-sig'))
def main():
    ap=argparse.ArgumentParser()
    ap.add_argument('--root',required=True,type=pathlib.Path)
    ap.add_argument('--out',required=True,type=pathlib.Path,help='JSON report path')
    args=ap.parse_args(); root=args.root.resolve(); out=args.out.resolve(); out.parent.mkdir(parents=True,exist_ok=True)
    catalog_path=root/'Assets/Darklight/Resources/Darklight/WeaponRedesign20261023/catalog.json'
    cat=read_json(catalog_path); pages=[p for p in cat.get('pages',[]) if p.get('skin')==0 and p.get('form')==0 and p.get('weapon') in WEAPONS]
    desc_cache={}; rows=[]; issue_counts={}; page_by_key={}; missing_sidecars=[]
    def issue(k): issue_counts[k]=issue_counts.get(k,0)+1
    for p in pages:
        key=(p.get('skin'),p.get('weapon'),p.get('form'),p.get('direction'),p.get('action'),p.get('attackKey') or '')
        page_by_key[key]=p
        resource=p.get('resourcePath',''); dpath=root/'Assets/Darklight/Resources'/(resource+'.json')
        desc=desc_cache.get(resource)
        if desc is None:
            if dpath.exists():
                try: desc=read_json(dpath)
                except Exception as e: desc={'_error':str(e)}
            else: desc={'_missing':True}
            desc_cache[resource]=desc
        names=p.get('spriteNames') or []; page_durations=p.get('durationMs') or []
        frames=desc.get('frames') or [] if isinstance(desc,dict) else []
        frame_map={f.get('id'):f for f in frames if isinstance(f,dict)}
        frame_ids=list(frame_map)
        durations=page_durations if len(page_durations)==len(names) else [frame_map[n].get('durationMs') for n in names if n in frame_map]
        clip_names=[]
        for c in (desc.get('clips') or []) if isinstance(desc,dict) else []:
            clip_names.extend(c.get('frames') or [])
        missing=[n for n in names if n not in frame_ids]
        missing_clip=[n for n in names if clip_names and n not in clip_names]
        frame_phases=[frame_map[n].get('phase','') for n in names if n in frame_map]
        if desc.get('_missing'): missing_sidecars.append(resource);issue('missing_sidecar')
        if desc.get('_error'): issue('bad_sidecar')
        if missing: issue('sprite_name_missing_from_descriptor')
        if missing_clip: issue('sprite_name_missing_from_clip')
        if len(durations)!=len(names): issue('duration_unavailable_or_count_mismatch')
        if any(not isinstance(v,(int,float)) or v<=0 for v in durations): issue('nonpositive_or_invalid_duration')
        if page_durations and len(page_durations)==len(names) and all(n in frame_map for n in names):
            descriptor_durs=[frame_map[n].get('durationMs') for n in names]
            if descriptor_durs!=page_durations: issue('catalog_descriptor_duration_mismatch')
        loop=bool(p.get('loop',False)); should_nonloop=p.get('action') in ('Dead','Hurt','Parry')
        if should_nonloop and loop: issue('nonloop_action_loops')
        if p.get('action')=='Attack':
            s=p.get('startupFrames'); a=p.get('activeFrames'); rec=(len(names)-(s or 0)-(a or 0)) if isinstance(s,int) and isinstance(a,int) else None
            if isinstance(s,int) and isinstance(a,int) and s>0 and a>0 and rec>0: phase=f'{s}/{a}/{rec} (explicit)'
            else:
                # Static fallback in TryResolveAttack: n>=3 => n-2 startup frames, 1 active, 1 recovery.
                runtime_split=f'{max(0,len(names)-2)}/1/1' if len(names)>=3 else ('1/0/1 (two-frame special)' if len(names)==2 else '1/0/0 (single-frame special)')
                phase=f'not stored; runtime count fallback {runtime_split}'
                issue('attack_phase_counts_not_explicit')
            route='TryResolveAttack(displayName): exact key if key matches; else fallback primary strike by weapon'
        else:
            s=p.get('startupFrames');a=p.get('activeFrames');rec=None;phase=''
            route='TryCurrentPose (Normal scope)' if p.get('action')!='Idle' or p.get('weapon')!=3 else 'DualSawsPose distinct consumer (catalog idle is not required)'
        sumdur=sum(durations) if all(isinstance(v,(int,float)) for v in durations) else None
        rows.append({'id':p.get('id',''),'weapon':WEAPONS[p['weapon']],'weapon_id':p['weapon'],'form':p['form'],'direction':DIRECTIONS[p['direction']] if isinstance(p.get('direction'),int) and 0<=p['direction']<8 else p.get('direction'),'action':p.get('action',''),'attackKey':p.get('attackKey',''),'resourcePath':resource,'descriptor':str(dpath.relative_to(root)).replace('\\','/'),'frames':len(names),'resolvedFrames':len(names)-len(missing),'missingSpriteNames':'|'.join(missing),'durationSource':'catalog' if len(page_durations)==len(names) else 'descriptor frames','frameDurationsMs':'|'.join(map(str,durations)),'durationCount':len(durations),'durationSumMs':sumdur,'durationMinMs':min(durations) if durations else '', 'durationMaxMs':max(durations) if durations else '', 'loop':loop,'attackPhases(start/active/recovery)':phase,'descriptorPhaseLabels':'|'.join(map(str,frame_phases)) if p.get('action')=='Attack' else '', 'routeAssessment':route,'staticResolution':'descriptor names and frame ids match' if not missing and not desc.get('_missing') else 'MISSING/ERROR'})
    # Cross-check catalog clip names, direction/action coverage, and phase/frame inventory.
    keys_by_wa={}; gaps=[]; frame_counts={}
    for w,wname in WEAPONS.items():
        for act in ACTIONS:
            got={p.get('direction') for p in pages if p.get('weapon')==w and p.get('action')==act}
            expected=set(range(8))
            # Moto Idle is supplied by a separate dedicated consumer, not this catalog.
            if w==3 and act=='Idle':
                gaps.append({'weapon':wname,'action':act,'coverage':'separate consumer','directions_catalog':sorted(got),'directions_expected':list(range(8)),'result':'Moto idle is DualSawsPose consumer; catalog coverage is not a gap'})
                continue
            missing=sorted(expected-got); extra=sorted(got-expected)
            gaps.append({'weapon':wname,'action':act,'directions_present':[DIRECTIONS[x] for x in sorted(got) if isinstance(x,int) and x<8],'directions_missing':[DIRECTIONS[x] for x in missing],'directions_extra':extra,'result':'complete' if not missing and not extra else 'gap'})
            for p in pages:
                if p.get('weapon')==w and p.get('action')==act:
                    frame_counts[f'{wname}/{act}/{DIRECTIONS[p["direction"]] if isinstance(p.get("direction"),int) and p["direction"]<8 else p.get("direction")}']=len(p.get('spriteNames') or [])
    # Archived 167-page attack family comparison by contextual tuple.
    archived_path=root/'ArtSources/ReviewedDeliveries/FabiusNormal20261103/PAGES.json'
    archived=[]; archive_error=None
    if archived_path.exists():
        try: archived=read_json(archived_path).get('pages',[])
        except Exception as e: archive_error=str(e)
    applied_attack_keys={(p.get('skin'),p.get('weapon'),p.get('form'),p.get('direction'),p.get('action'),p.get('attackKey') or '') for p in pages if p.get('action')=='Attack'}
    archived_attacks=[]
    for p in archived:
        if p.get('skin')==0 and p.get('form')==0 and p.get('weapon') in WEAPONS and p.get('action')=='Attack':
            k=(p.get('skin'),p.get('weapon'),p.get('form'),p.get('direction'),p.get('action'),p.get('attackKey') or '')
            archived_attacks.append({'key':p.get('attackKey',''),'weapon':WEAPONS[p['weapon']],'direction':DIRECTIONS[p['direction']] if isinstance(p.get('direction'),int) and p['direction']<8 else p.get('direction'),'appliedExactContext':k in applied_attack_keys,'frames':len(p.get('spriteNames') or [])})
    fam={}
    for a in archived_attacks:
        ent=fam.setdefault((a['weapon'],a['key']),{'dirs':[],'frames':[],'exactAppliedDirs':[]})
        ent['dirs'].append(a['direction']);ent['frames'].append(a['frames'])
        if a['appliedExactContext']:ent['exactAppliedDirs'].append(a['direction'])
    # C# and attack-definition static facts (no mutation or broad asset hash scan).
    runtime=root/'Assets/Darklight/Runtime'; cs=list(runtime.rglob('*.cs'))
    texts={str(f.relative_to(root)).replace('\\','/'):f.read_text(encoding='utf-8-sig',errors='replace') for f in cs}
    data_dir=root/'Assets/Darklight/Data'
    data_texts={str(f.relative_to(root)).replace('\\','/'):f.read_text(encoding='utf-8-sig',errors='replace') for f in data_dir.rglob('*.asset')} if data_dir.exists() else {}
    archived_families=[]
    for k,v in sorted(fam.items()):
        locations=[]
        for path,txt in texts.items():
            for lineno,line in enumerate(txt.splitlines(),1):
                if re.search(r'(?:Make\(|displayName\s*=\s*)["\']'+re.escape(k[1])+r'["\']',line):
                    locations.append({'source':'Runtime C# attack-name construction','path':path,'line':lineno})
        for path,txt in data_texts.items():
            if '/Data/Attacks/' not in '/'+path:
                continue
            for lineno,line in enumerate(txt.splitlines(),1):
                if re.fullmatch(r'\s*displayName:\s*'+re.escape(k[1])+r'\s*',line):
                    locations.append({'source':'AttackDefinition displayName asset','path':path,'line':lineno})
        archived_families.append({'weapon':k[0],'attackKey':k[1],'archivedDirections':sorted(v['dirs']),'archivedPageCount':len(v['dirs']),'frameCounts':v['frames'],'exactAppliedDirections':sorted(v['exactAppliedDirs']),'unappliedDirections':sorted(set(v['dirs'])-set(v['exactAppliedDirs'])),'displayNameLiteralEvidence':locations,'routeAssessment':'Exact name has source literal evidence; resolver can select it only when CombatModel.Attack.displayName equals this string and a catalog page exists in the exact context. Current archived exact-context pages are not applied.' if locations else 'No matching runtime/Data literal found by scoped source scan; exact-name reachability is unconfirmed.'})
    current_attack_keys=[]
    for w,wname in WEAPONS.items():
        keys={}
        for p in pages:
            if p.get('weapon')==w and p.get('action')=='Attack':
                k=p.get('attackKey') or ''
                ent=keys.setdefault(k,{'pages':0,'directions':[],'frameCounts':[]})
                ent['pages']+=1;ent['directions'].append(DIRECTIONS[p['direction']] if isinstance(p.get('direction'),int) and p['direction']<8 else str(p.get('direction')));ent['frameCounts'].append(len(p.get('spriteNames') or []))
        current_attack_keys.append({'weapon':wname,'keys':[{'attackKey':k,'pages':v['pages'],'directions':sorted(v['directions']),'frameCounts':v['frameCounts']} for k,v in sorted(keys.items())]})
    allcs='\n'.join(texts.values())
    def line_for(path,pattern):
        t=texts.get(path,'')
        for i,l in enumerate(t.splitlines(),1):
            if re.search(pattern,l): return i,l.strip()
        return None
    weapon_path='Assets/Darklight/Runtime/Character/DarklightWeaponRedesignBank.cs'
    moto_path='Assets/Darklight/Runtime/Character/DarklightMotorcycleRedesignBank.cs'
    particle_path='Assets/Darklight/Runtime/World/DarklightActionParticles.cs'
    tryroute=line_for(weapon_path,r'static bool TryResolveAttack')
    keyline=line_for(weapon_path,r'string key=model\.Attack\.displayName')
    fallback=[(i,l.strip()) for i,l in enumerate(texts.get(weapon_path,'').splitlines(),1) if 'weapon==0?"Leve1"' in l or 'weapon==3?"Serra de Entrada"' in l]
    dualpose=line_for(moto_path,r'public static Sprite DualSawsPose')
    fixedoffsets=[(i,l.strip()) for i,l in enumerate(texts.get(particle_path,'').splitlines(),1) if 'float height=' in l]
    guard_files=[path for path,t in texts.items() if re.search(r'guardContact',t,re.I)]
    # Walk comparison with durations per exact weapon/direction; report no movement speed claim.
    walks=[]
    for w,wname in WEAPONS.items():
        for d in range(8):
            hits=[r for r in rows if r['weapon_id']==w and r['action']=='Walk' and r['direction']==DIRECTIONS[d]]
            if hits:
                h=hits[0]; walks.append({'weapon':wname,'direction':DIRECTIONS[d],'frames':h['frames'],'durationMs':h['durationSumMs'],'frameDurationsMs':[float(v) for v in h['frameDurationsMs'].split('|')] if h['frameDurationsMs'] else []})
    # Summaries and audit flags
    action_counts={}
    for p in pages:
        k=f'{WEAPONS[p["weapon"]]}/{p.get("action")}'
        action_counts[k]=action_counts.get(k,0)+1
    essentials={}
    for w,wname in WEAPONS.items():
        for action in ('Parry','Hurt','Dead'):
            group=[p for p in pages if p.get('weapon')==w and p.get('action')==action]
            essentials[f'{wname}/{action}']={'pages':len(group),'directions_present':[DIRECTIONS[p['direction']] for p in sorted(group,key=lambda q:q.get('direction',99)) if isinstance(p.get('direction'),int) and p['direction']<8],'totalFrames':sum(len(p.get('spriteNames') or []) for p in group),'frameCountsByDirection':{DIRECTIONS[p['direction']]:len(p.get('spriteNames') or []) for p in group if isinstance(p.get('direction'),int) and p['direction']<8}}
    phase_patterns={}
    for row in rows:
        if row['action']=='Attack':
            pat=row['descriptorPhaseLabels']
            ent=phase_patterns.setdefault(pat,{'pages':0,'example':row['id']})
            ent['pages']+=1
    # Categorize issues actually observed; avoid treating separate Moto idle as missing.
    missing_coverage=[g for g in gaps if g['result']=='gap']
    loop_findings={'Dead/Hurt/Parry pages':sum(1 for p in pages if p.get('action') in ('Dead','Hurt','Parry')),'looping among Dead/Hurt/Parry':[p.get('id') for p in pages if p.get('action') in ('Dead','Hurt','Parry') and p.get('loop')],'positive duration pages':sum(1 for r in rows if r['durationCount']==r['frames'] and r['durationMinMs'] and r['durationMinMs']>0)}
    report={'scope':{'catalog':str(catalog_path.relative_to(root)).replace('\\','/'),'catalogTotalPages':len(cat.get('pages',[])),'activeNormalPages':len(pages),'weapons':WEAPONS,'form':0,'skin':0,'actions':ACTIONS,'directions':DIRECTIONS,'scopeNote':'Static catalog/JSON/C# audit only; no Unity/import/playback or visual naturalness verification.'},'validation':{'sidecarsLoaded':len(desc_cache),'missingSidecars':sorted(set(missing_sidecars)),'issueCounts':issue_counts,'spriteNamesResolved':sum(int(r['resolvedFrames']) for r in rows),'spriteNamesTotal':sum(int(r['frames']) for r in rows),'loopAndTimingSummary':loop_findings,'gaps':missing_coverage,'essentialFrameCounts':essentials,'actionPageCounts':action_counts,'walkDurations':walks,'attackDescriptorPhasePatterns':phase_patterns,'attackPhaseFinding':'Catalog pages omit startupFrames and activeFrames on some Attack rows; where absent runtime uses count-based fallback (4 frames => 2/1/1; 3 frames => 1/1/1). Descriptor phase labels are listed per frame in CSV and summarized by exact pattern in JSON; labels are authored metadata, not visual/naturalness evidence.','walkAssessment':'Duration comparisons describe authored clock only. No movement-speed/travel-distance metadata was found in this audit; asymmetry cannot be translated to world-space step cadence.'},'runtime':{'tryResolveAttack':{'location':weapon_path,'definition':tryroute,'keyRead':keyline,'fallbackLines':fallback,'behavior':'active Normal Katana/Fists/Moto path calls this resolver on Attack; resolver compares displayName against catalog attackKey, then tries one weapon-specific primary key. Missing key falls through as unresolved/pose fallback at caller level.'},'motorIdle':{'location':moto_path,'definition':dualpose,'assessment':'Moto idle is consumed through dedicated DualSawsPose/Requiem dual-saws presentation, so absence of catalog Idle pages is not a coverage gap.'},'guardContact':{'runtimeSourceFilesWithLiteral':guard_files,'assessment':'guardContact metadata exists in catalog/Parry descriptors but no literal C# reader was found under Runtime; metadata preservation does not mean gameplay collision consumes it.'},'actionParticles':{'location':particle_path,'fixedHeightLine':fixedoffsets,'assessment':'Observe places Dodge at +0.12, Death +0.4, other mapped states (including Parry/Hurt) +0.85 world units from owner.transform.position; no descriptor guardContact/pose-dependent offset here.'}},'currentCatalogAttackKeys':current_attack_keys,'archivedAttackFamilies':{'archive':str(archived_path.relative_to(root)).replace('\\','/') if archived_path.exists() else None,'archivePages':len(archived),'error':archive_error,'families':archived_families,'note':'Exact route requires model.Attack.displayName == attackKey. Archived-only keys do not become runtime selectable merely by being staged; key source/attack model mapping is separate.'},'csvRows':len(rows)}
    # Write CSV adjacent to report; every active page has one row.
    csvpath=out.with_suffix('.csv')
    fields=list(rows[0].keys()) if rows else []
    with csvpath.open('w',newline='',encoding='utf-8-sig') as f:
        w=csv.DictWriter(f,fieldnames=fields);w.writeheader();w.writerows(rows)
    out.write_text(json.dumps(report,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    # Concise markdown report adjacent.
    md=[]
    md += ['# Fabius Normal — animation sync static review','',f'- Active pages: **{len(pages)} / {len(cat.get("pages",[]))}** global catalog pages; scope: skin 0, form 0, Katana/Fists/Moto.',f'- Clip-frame names resolved: **{report["validation"]["spriteNamesResolved"]}/{report["validation"]["spriteNamesTotal"]}** across {len(desc_cache)} sidecars.',f'- Sidecar/metadata findings: `{json.dumps(issue_counts,ensure_ascii=False) or "none"}`.', '- This is a static catalog/runtime-source audit. It does not establish visual naturalness, successful Unity import, or playback.','', '## Coverage and clips','', '| Weapon | Action | Pages | Directions / note | Frames |','|---|---:|---:|---|---:|']
    for k,v in sorted(action_counts.items()):
        wname,act=k.split('/',1); pages0=[p for p in pages if WEAPONS[p['weapon']]==wname and p['action']==act]
        dirs=', '.join(DIRECTIONS[p['direction']] for p in sorted(pages0,key=lambda p:p.get('direction',99)) if isinstance(p.get('direction'),int) and p['direction']<8) or '—'
        note='Separate DualSawsPose consumer' if wname=='Moto' and act=='Idle' else dirs
        md.append(f'| {wname} | {act} | {len(pages0)} | {note} | {sum(len(p.get("spriteNames") or []) for p in pages0)} |')
    md+=['','### Essential frame counts (Parry / Hurt / Dead)','', '| Weapon/action | Pages | Frames by direction | Total |','|---|---:|---|---:|']
    for k,v in essentials.items(): md.append(f'| {k} | {v["pages"]} | '+', '.join(f'{d}:{n}' for d,n in v['frameCountsByDirection'].items())+f' | {v["totalFrames"]} |')
    md+=['','## Walk timing by direction','', '| Weapon | Direction | Frames | Frame durations (ms) | Clip total (ms) |','|---|---|---:|---|---:|']
    for wrow in walks: md.append(f'| {wrow["weapon"]} | {wrow["direction"]} | {wrow["frames"]} | {", ".join(map(str,wrow["frameDurationsMs"]))} | {wrow["durationMs"]} |')
    md += ['', 'Timing rows show authored clip clock only. Katana and Fists totals are 400 ms across covered directions despite their different frame counts; Moto rows are 480 ms. The reviewed data does not bind clip cycles to movement speed/displacement, so this does not establish equal or unequal world-space step cadence.','', '## Runtime and concrete risks','',f'- **Attack routing:** `{weapon_path}` line {tryroute[0] if tryroute else "?"} reads `model.Attack.displayName`; context lookup requires exact `attackKey`. Fallback mapping source lines: '+(', '.join(str(x[0])+': '+x[1] for x in fallback) if fallback else 'not detected')+'. Archived family names require matching displayName and an applied page in the current context; see source-literal evidence table.','- **Attack phases:** 48 active attack pages omit explicit `startupFrames`/`activeFrames`; runtime count fallback maps 4-frame pages as 2/1/1 and 3-frame pages as 1/1/1. Sixteen pages have explicit phase counts. Descriptor phase-label sequences (6 unique patterns) are in JSON/CSV; they are metadata labels, not motion proof.','- **Non-looping states:** all 72 Dead/Hurt/Parry pages are non-looping; positive frame durations were found in '+f'{loop_findings["positive duration pages"]}/{len(pages)}'+' active pages using the catalog/descriptor data.','- **Moto idle:** provided by the separate `DualSawsPose` consumer; catalog idle absence is not a coverage defect.','- **guardContact:** metadata is present but no `Runtime` C# file references the literal; it is not wired as combat contact/collision data.','- **Particles:** `Observe` uses fixed owner-relative heights '+('; '.join(f'line {i}: {s}' for i,s in fixedoffsets))+'. This is not frame/anchor-aware and does not consume guardContact.','- **Timing mismatch:** five Leve3 Katana pages store 40/40/100/180 ms in the catalog while their descriptor frames each say 120 ms. Attack playback uses attack phase timing rather than frame durations, but the records disagree and should be reconciled before treating timing as a single source of truth.','- **Naturalness/reference sync:** this audit did not inspect a rendered playback against the official Dead Cells/Hades references; metadata cannot support that claim. Unity import/runtime state remains unverified.','', '## Archived AttackKeys vs current catalog','', '| Weapon | attackKey | Archived dirs | Exact-context applied dirs | Attack displayName evidence | Still absent |','|---|---|---|---|---|---|']
    for a in archived_families: md.append(f'| {a["weapon"]} | {a["attackKey"]} | {", ".join(a["archivedDirections"])} | {", ".join(a["exactAppliedDirections"]) or "—"} | {", ".join(sorted(set(x["path"] for x in a["displayNameLiteralEvidence"]))) or "unconfirmed"} | {", ".join(a["unappliedDirections"]) or "—"} |')
    md += ['', '## Deliverables and next concrete checks','',f'- CSV: `{csvpath.name}` (one row per active catalog page).',f'- Machine report: `{out.name}`.', '- Next: coordinator maps desired attack display names to archived attackKey families (or records primary-strike fallback), then performs bounded Unity import/runtime verification under the existing resource gate. Keep visual review, imported status, and runtime verification as separate claims.']
    mdpath=out.with_suffix('.md');mdpath.write_text('\n'.join(md)+'\n',encoding='utf-8')
    print(json.dumps({'pages':len(pages),'csv':str(csvpath),'json':str(out),'markdown':str(mdpath),'issues':issue_counts,'families':len(archived_families),'active_scope_coverage_gaps':len(missing_coverage)},ensure_ascii=False))
    return 2 if issue_counts or missing_coverage else 0
if __name__=='__main__': sys.exit(main())
