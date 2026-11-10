"""Analyze a completed continuous heavy-load device run from raw snapshots."""
import datetime,json,pathlib,re
root=pathlib.Path(__file__).parent
out=root/'device/heavy-operation'
def read(path):return json.loads(path.read_text())
def number(text,pattern):
    m=re.search(pattern,text)
    return int(m.group(1)) if m else None
def state(label):
    d=read(out/(label+'.json'));s=read(out/(label+'-session.json'))
    return {'label':label,'utc':d['utc'],'pid':d['pid'],'elapsedSeconds':d['elapsedSeconds'],
        'pssKiB':number(d['meminfo'],r'TOTAL PSS:\s+(\d+)'),
        'rssKiB':number(d['meminfo'],r'TOTAL RSS:\s+(\d+)'),
        'thermalStatus':number(d['thermal'],r'Thermal Status:\s+(\d+)'),
        'batteryDeciC':number(d['battery'],r'\n\s*temperature:\s+(\d+)'),
        'androidViews':number(d['meminfo'],r'Views:\s+(\d+)'),
        'androidActivities':number(d['meminfo'],r'Activities:\s+(\d+)'),
        'gameSeconds':float(s['invasion']['elapsed']),
        'creatures':sum(x['definitionId'].startswith('creature.') for x in s['things']),
        'facilities':sum(x['definitionId'].startswith('facility.') for x in s['things']),
        'external':len(s['invasion']['invaders']),'score':s['invasion']['score']}
manifest=read(out/'manifest.json')
assert manifest['status']=='completed','Do not report an unfinished run as complete'
rows=[state(p.stem) for p in sorted(out.glob('cycle-??.json'))]
before=state('before');after=state('after');baseline=rows[0]
load=read(root/'device/final-heavy-load.json')
continuous=(datetime.datetime.fromisoformat(after['utc'])-datetime.datetime.fromisoformat(load['finishedUtc'])).total_seconds()
limit=max(32*1024,baseline['pssKiB']*.1)
growth=after['pssKiB']-baseline['pssKiB']
report={'manifest':manifest,'continuousSecondsFromConfirmedLoad':continuous,
    'operatingSeconds':manifest['elapsedSeconds'],'baselineReason':'First full cycle after every menu and both languages were used; fixed before the end comparison.',
    'baseline':baseline,'after':after,'cycles':rows,
    'sameProcess':all(r['pid']==before['pid']==load['pidAfterLaunch'] for r in rows+[after]),
    'gameSecondsDuringOperations':after['gameSeconds']-before['gameSeconds'],
    'pssGrowthKiB':growth,'allowedPssGrowthKiB':limit,'memoryGrowthPass':growth<=limit,
    'absolutePssPass':max(r['pssKiB'] for r in [before,after]+rows)*1024<=1073741824,
    'thermalPass':max(r['thermalStatus'] for r in [before,after]+rows)<=2,
    'durationPass':continuous>=1200 and manifest['elapsedSeconds']>=900,
    'objectLimit':'Android Views/Activities only in final APK. Unity object/GC counts are not inferred from these OS values.',
    'samplingLimit':'Memory is sampled after each return to the full map; transient peaks during UI transitions are not a continuous memory trace.'}
(root/'heavy-operation-summary.json').write_text(json.dumps(report,indent=2)+'\n')
print(json.dumps({k:v for k,v in report.items() if k not in ['cycles','manifest','baseline','after']},indent=2))
