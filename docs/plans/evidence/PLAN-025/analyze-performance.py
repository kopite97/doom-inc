"""Summarize preserved SurfaceFlinger/OS and diagnostic samples, without replacing raw evidence."""
import csv, json, math, pathlib, re, statistics

root = pathlib.Path(__file__).parent
criteria = json.loads((root/'mobile-acceptance-criteria.json').read_text())
def metric(text, pattern):
    match = re.search(pattern,text)
    return int(match.group(1)) if match else None
def describe(values):
    ordered = sorted(values)
    return {'count':len(ordered), 'median':statistics.median(ordered),
            'p95':ordered[math.ceil(len(ordered)*.95)-1], 'maximum':ordered[-1], 'total':sum(ordered)} if ordered else None
def os_state(path):
    d = json.loads(path.read_text())
    return {'utc':d['utc'],'pid':d['pid'],
            'pssKiB':metric(d['meminfo'],r'TOTAL PSS:\s+(\d+)'),
            'rssKiB':metric(d['meminfo'],r'TOTAL RSS:\s+(\d+)'),
            'thermalStatus':metric(d['thermal'],r'Thermal Status:\s+(\d+)'),
            'batteryDeciC':metric(d['battery'],r'\n\s*temperature:\s+(\d+)')}
def game_state(path):
    d=json.loads(path.read_text());things=d['things']
    return {'gameSeconds':float(d['invasion']['elapsed']),
        'facilities':sum(x['definitionId'].startswith('facility.') for x in things),
        'workers':sum(x['definitionId'].startswith('creature.support') for x in things),
        'creatures':sum(x['definitionId'].startswith('creature.') for x in things),
        'external':len(d['invasion']['invaders']),'score':d['invasion']['score'],
        'activitySequence':d['invasion']['activitySequence']}
runs=[]; excluded=[]
exclusion_file=root/'performance-exclusions.json'
exclusions=json.loads(exclusion_file.read_text()) if exclusion_file.exists() else {}
for path in sorted((root/'device').glob('performance-*/*/[1-9]/summary.json')):
    summary=json.loads(path.read_text()); directory=path.parent
    before=os_state(directory/'before.json');after=os_state(directory/'after.json')
    row={'path':path.relative_to(root).as_posix(),'surface':summary,'before':before,'after':after,
         'stateAfterRun':game_state(directory.parent/f'after-{directory.name}-session.json'),
         'framePass':summary['valid'] and summary['medianMs']<=criteria['surfaceFrames']['medianMsAtMost']
            and summary['p95Ms']<=criteria['surfaceFrames']['p95MsAtMost']
            and summary['maximumMs']<=criteria['surfaceFrames']['maximumMsAtMost'],
         'memoryAbsolutePass':max(before['pssKiB'],after['pssKiB'])*1024<=criteria['memory']['steadyStatePssBytesAtMost'],
         'thermalPass':max(before['thermalStatus'],after['thermalStatus'])<=criteria['thermal']['maximumStatus']}
    probe=directory.parent/('probe-'+directory.name)
    if (probe/'result.json').exists():
        result=json.loads((probe/'result.json').read_text())
        samples=list(csv.DictReader((probe/'samples.csv').open()))
        gc=[int(x['gc_allocated_bytes']) for x in samples if x['gc_valid']=='true']
        row['diagnostic']={'result':result,'gcAllocatedBytes':describe(gc),
            'allGcSamplesValid':len(gc)==len(samples) and bool(gc),
            'updateIntervalMs':describe([float(x['unity_update_interval_ms']) for x in samples]),
            'note':'Unity update intervals are not presentation intervals. GC allocation is not GC collection pause time.'}
        row['diagnosticValid']=result['status']=='completed' and not result['interrupted'] \
            and result['gcCounterValid'] and row['diagnostic']['allGcSamplesValid'] \
            and all(x['application_paused']=='false' for x in samples)
    key=path.relative_to(root).as_posix()
    if key in exclusions:
        row['exclusionReason']=exclusions[key];excluded.append(row)
    else:
        runs.append(row)
groups={f'{flavor}/{state}':sum(f'performance-{flavor}/{state}/' in x['path'] for x in runs)
        for flavor in ['final','diagnostic'] for state in ['initial','middle','heavy']}
report={'fixedCriteriaUtc':criteria['fixedAtUtc'],'completedRuns':len(runs),
        'allCompletedRunsPass':bool(runs) and all(x['framePass'] and x['memoryAbsolutePass'] and x['thermalPass']
            and x.get('diagnosticValid',True) for x in runs),
        'expectedMainRuns':18,'complete':len(runs)==18 and all(n==3 for n in groups.values()),
        'groupCounts':groups,'runs':runs,'excludedRuns':excluded}
(root/'performance-summary.json').write_text(json.dumps(report,indent=2)+'\n')
for row in runs:
    s=row['surface']
    print(row['path'],s['medianMs'],s['p95Ms'],s['maximumMs'],row['after']['pssKiB'],row['framePass'])
