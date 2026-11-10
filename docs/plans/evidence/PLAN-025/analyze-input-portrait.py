"""Portrait zoom geometry / phone-clock latency. Run with the recorded OpenCV analysis environment."""
import csv, datetime, hashlib, json, math, pathlib, re, statistics, sys
import cv2

root=pathlib.Path(__file__).parent/'device'/('input-'+sys.argv[1])
config=json.loads((root/'anchors.json').read_text())
bases=[datetime.datetime.fromisoformat(a['phoneTime']).timestamp()-a['ptsMs']/1000 for a in config]
base=statistics.mean(bases)
events=[float(x) for x in re.findall(r'([0-9]+\.[0-9]+).*ViewPostIme pointer 1',(root/'pointer-log.txt').read_text())][-10:]
capture=cv2.VideoCapture(str(root/'screen.mp4'))
geometry=[];changes=[];previous=None;n=0
while True:
    ok,frame=capture.read()
    if not ok:break
    pts=capture.get(cv2.CAP_PROP_POS_MSEC)
    # x=400 is an empty top grid column in these full-map views. The fit/1.3x
    # views have top edges at about y=810/y=714. Ignore the screenrecord intro.
    scan=frame[500:1000,400]
    candidates=[i+500 for i,c in enumerate(scan) if int(c[0])>35 and int(c[1])>30]
    top=candidates[0] if candidates else None
    if pts>1000 and top is not None and (700<=top<=730 or 800<=top<=825):
        row={'frame':n,'ptsMs':pts,'top':top,'zoomed':top<760}
        geometry.append(row)
        if previous is not None and row['zoomed']!=previous['zoomed']:changes.append((previous,row))
        previous=row
    n+=1
capture.release()
(root/'frame-geometry.json').write_text(json.dumps(geometry,indent=2)+'\n')
assert len(changes)==len(events)==10,(len(changes),len(events))
rows=[]
for n,(event,(prev,cur)) in enumerate(zip(events,changes),1):
    rows.append({'sample':n,'androidPointerUpEpoch':event,'previousFrame':prev['frame'],
        'firstChangedFrame':cur['frame'],'previousPtsMs':prev['ptsMs'],'changedPtsMs':cur['ptsMs'],
        'lowerBoundMs':round((base+prev['ptsMs']/1000-event)*1000,3),
        'upperBoundMs':round((base+cur['ptsMs']/1000-event)*1000,3)})
with (root/'latency.csv').open('w',newline='') as f:
    w=csv.DictWriter(f,fieldnames=rows[0]);w.writeheader();w.writerows(rows)
values=sorted(r['upperBoundMs'] for r in rows)
report={'measure':'Android ViewRootImpl ACTION_UP to first changed screenrecord composed frame; ADB injected zoom taps',
    'notMeasured':'Physical digitizer or panel photon latency','samples':len(rows),
    'medianUpperBoundMs':statistics.median(values),'p95UpperBoundMs':values[math.ceil(.95*len(values))-1],
    'maximumUpperBoundMs':max(values),'frameResolutionMs':max(r['changedPtsMs']-r['previousPtsMs'] for r in rows),
    'anchorSpreadMs':(max(bases)-min(bases))*1000,'overlayRoundingMs':1,
    'passForMeasuredBoundary':max(values)+1<=150,
    'videoSha256':hashlib.sha256((root/'screen.mp4').read_bytes()).hexdigest(),
    'apkSha256':'3e9b33badc4fba23a432b77c1d932851e6d36f30a29125e2764b60283958bb0d'}
(root/'latency-summary.json').write_text(json.dumps(report,indent=2)+'\n')
print(json.dumps(report,indent=2))
