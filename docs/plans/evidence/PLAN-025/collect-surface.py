"""PLAN-025 read-only Android SurfaceFlinger capture; standard library only."""
import argparse, csv, datetime, hashlib, json, pathlib, statistics, subprocess, time

p = argparse.ArgumentParser()
p.add_argument('--adb', required=True)
p.add_argument('--out', required=True)
p.add_argument('--package', default='com.DefaultCompany.urp_2d')
p.add_argument('--warmup', type=float, default=30)
p.add_argument('--seconds', type=float, default=60)
a = p.parse_args()
out = pathlib.Path(a.out)
out.mkdir(parents=True, exist_ok=False)

def shell(*args):
    return subprocess.check_output([a.adb, 'shell', *args], text=True, timeout=15).strip()

def utc():
    return datetime.datetime.now(datetime.timezone.utc).isoformat()

def snapshot(label):
    values = {'utc':utc(), 'pid':shell('pidof', a.package), 'uptime':shell('cat', '/proc/uptime'),
              'meminfo':shell('dumpsys', 'meminfo', a.package),
              'thermal':shell('dumpsys', 'thermalservice'), 'battery':shell('dumpsys', 'battery')}
    (out/(label+'.json')).write_text(json.dumps(values, indent=2), encoding='utf-8')
    return values

layers = shell('dumpsys','SurfaceFlinger','--list').splitlines()
matches = [x for x in layers if x.startswith('SurfaceView - '+a.package+'/') and '(BLAST)' in x]
if len(matches) != 1: raise RuntimeError('Expected exactly one app BLAST surface: '+str(matches))
layer = matches[0]
start_info = {'utc':utc(), 'layer':layer, 'warmupSeconds':a.warmup, 'sampleSeconds':a.seconds,
              'columns':'desired_present_ns,actual_present_ns,frame_ready_ns',
              'clock':'SurfaceFlinger monotonic nanoseconds', 'pollSeconds':0.75,
              'measure':'Consecutive actual present intervals; not CPU/GPU execution duration.'}
(out/'started.json').write_text(json.dumps(start_info, indent=2), encoding='utf-8')
print('warming '+str(out), flush=True)
time.sleep(a.warmup)
before = snapshot('before')
baseline = shell('dumpsys','SurfaceFlinger','--latency', "'"+layer+"'")

def rows(raw):
    values=[]
    for line in raw.splitlines()[1:]:
        fields=line.split()
        if len(fields)==3:
            v=tuple(map(int,fields))
            if 0<v[1]<9223372036854775807: values.append(v)
    return values

initial=rows(baseline)
if not initial: raise RuntimeError('No valid app present timestamps')
boundary=max(v[1] for v in initial)
all_rows={}; poll_durations=[]; started=time.monotonic()
with (out/'polls.jsonl').open('w',encoding='utf-8') as raw_file:
    while time.monotonic()-started<a.seconds:
        t=time.monotonic(); raw=shell('dumpsys','SurfaceFlinger','--latency', "'"+layer+"'")
        elapsed=time.monotonic()-t; poll_durations.append(elapsed)
        raw_file.write(json.dumps({'elapsedSeconds':time.monotonic()-started,'querySeconds':elapsed,'raw':raw})+'\n')
        for v in rows(raw):
            if v[1]>boundary: all_rows[v[1]]=v
        time.sleep(max(0,0.75-elapsed))
after=snapshot('after')
ordered=sorted(all_rows.values(),key=lambda x:x[1])
with (out/'frames.csv').open('w',newline='',encoding='utf-8') as f:
    writer=csv.writer(f);writer.writerow(['desired_present_ns','actual_present_ns','frame_ready_ns'])
    writer.writerows(ordered)
intervals=[(y[1]-x[1])/1e6 for x,y in zip(ordered,ordered[1:])]
sorted_intervals=sorted(intervals)
summary={'utc':utc(),'count':len(ordered),'spanSeconds':(ordered[-1][1]-ordered[0][1])/1e9 if ordered else 0,
         'medianMs':statistics.median(intervals) if intervals else None,
         'p95Ms':sorted_intervals[min(len(intervals)-1,int(len(intervals)*.95))] if intervals else None,
         'maximumMs':max(intervals) if intervals else None, 'maxPollSeconds':max(poll_durations),
         'sameProcess':before['pid']==after['pid'],'framesSha256':hashlib.sha256((out/'frames.csv').read_bytes()).hexdigest(),
         'valid':len(ordered)>1 and before['pid']==after['pid'] and max(poll_durations)<2}
(out/'summary.json').write_text(json.dumps(summary,indent=2),encoding='utf-8')
print(json.dumps(summary),flush=True)
