"""Derive PLAN-025 zoom response from preserved phone logs and video geometry."""
import csv, hashlib, json, pathlib, re, statistics

root = pathlib.Path(__file__).parent / 'device/input-final'
geometry = json.loads((root/'frame-geometry.json').read_text())
events = [float(x) for x in re.findall(r'([0-9]+\.[0-9]+).*ViewPostIme pointer 1', (root/'pointer-log.txt').read_text())][-10:]
# Visually transcribed screenrecord --bugreport overlay, same phone wall clock as logcat.
anchors = [(2514.1, 1793971765.856), (3519.2444444444445, 1793971766.861), (13666.333333333334, 1793971777.008)]
bases = [epoch - pts/1000 for pts, epoch in anchors]
base = statistics.mean(bases)
changes = []
previous = None
for frame in geometry:
    if frame['left'] not in (552,700):
        continue
    if previous is not None and frame['left'] != previous['left']:
        changes.append((previous, frame))
    previous = frame
assert len(changes) == len(events) == 10
rows = []
for i, (event, (prev, frame)) in enumerate(zip(events, changes)):
    rows.append({'sample':i+1, 'androidPointerUpEpoch':event, 'previousFrame':prev['frame'],
                 'firstChangedFrame':frame['frame'], 'previousPtsMs':prev['ptsMs'],
                 'changedPtsMs':frame['ptsMs'],
                 'lowerBoundMs':round((base+prev['ptsMs']/1000-event)*1000,3),
                 'upperBoundMs':round((base+frame['ptsMs']/1000-event)*1000,3)})
with (root/'latency.csv').open('w',newline='') as f:
    w=csv.DictWriter(f,fieldnames=rows[0].keys());w.writeheader();w.writerows(rows)
values=sorted(r['upperBoundMs'] for r in rows)
summary={'measure':'Android ViewRootImpl ACTION_UP log to first changed screenrecord composed frame; ADB injected zoom taps',
         'notMeasured':'Physical digitizer contact or display photon latency', 'clock':'Phone epoch logcat and screenrecord bugreport overlay; host timestamps excluded',
         'anchors':[{'ptsMs':p,'epoch':e} for p,e in anchors], 'anchorSpreadMs':(max(bases)-min(bases))*1000,
         'overlayRoundingMs':1,'frameResolutionMs':max(r['changedPtsMs']-r['previousPtsMs'] for r in rows),
         'samples':10,'medianUpperBoundMs':statistics.median(values),'p95UpperBoundMs':values[-1],
         'maximumUpperBoundMs':values[-1], 'p95Method':'nearest rank, ceil(0.95*N)',
         'passForMeasuredBoundary':values[-1]+1<=150,
         'videoSha256':hashlib.sha256((root/'screen.mp4').read_bytes()).hexdigest(),
         'apkSha256':'3e9b33badc4fba23a432b77c1d932851e6d36f30a29125e2764b60283958bb0d'}
(root/'latency-summary.json').write_text(json.dumps(summary,indent=2))
print(json.dumps(summary,indent=2))
