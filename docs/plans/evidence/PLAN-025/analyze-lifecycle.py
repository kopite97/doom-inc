"""Derive PLAN-025 device observations from preserved saves; no device mutations."""
import hashlib, json, pathlib

root = pathlib.Path(__file__).parent / 'device'
def read(name):
    p = root / (name + '.json')
    d = json.loads(p.read_text(encoding='utf-8-sig'))
    return d, hashlib.sha256(p.read_bytes()).hexdigest()
def state(name):
    d, sha = read(name)
    i = d['invasion']
    return {'file': name + '.json', 'sha256': sha, 'phase': d['phase'],
            'gameSeconds': float(i['elapsed']), 'score': i['score'],
            'activitySequence': i['activitySequence'], 'receipts': i['receipts'],
            'things': len(d['things']), 'reservations': len(d['laboratory']['reservations'])}

pending = state('supplies-paused-session')
restored = state('supplies-restored-session')
admitted = state('supplies-admitted-session')
before, _ = read('supplies-paused-session')
after, _ = read('supplies-restored-session')
bottleneck, _ = read('bottleneck-observations')
blocked_a, blocked_b, _, running_a, running_b = bottleneck
def cultivation(s):
    return next(x for x in s['production'] if x['definition']=='facility.cultivation')
dt = running_b['elapsed'] - running_a['elapsed']
work = cultivation(running_b)['progress'] - cultivation(running_a)['progress']
report = {
    'supplyRestore': {'before':pending, 'after':restored,
        'receiptExactlyPreserved':pending['receipts']==restored['receipts'],
        'thingsExactlyPreserved':before['things']==after['things'],
        'foregroundGameSeconds':restored['gameSeconds']-pending['gameSeconds'],
        'admittedLater':admitted,
        'deliveredIncrease':admitted['receipts'][0]['delivered']-restored['receipts'][0]['delivered'],
        'note':'After admission, later round 18 created a new pending supply. The persisted pendingRound=18 is not the original pendingRound=6.'},
    'bottleneck': {
        'blockedObservedSeconds':blocked_b['elapsed']-blocked_a['elapsed'],
        'blockedCultivationWorkPer60GameSeconds':0,
        'runningObservedSeconds':dt, 'runningCultivationWork':work,
        'runningCultivationWorkPer60GameSeconds':work/dt*60,
        'note':'Rates normalized to the same 60 game seconds. Observation windows were not exactly equal; no interpolation of completion counts.'},
    'cycle1':state('cycle1-results-session'),
    'cycle2':{'resultScreenshot':'cycle2-results.png','score':120,'activitySequence':40,
        'displayedGameSeconds':700.2,'external':2,
        'resultSaveCaptured':False,
        'note':'Result screenshot was captured at 13:56:01 UTC. Before the 13:56:26 save pull, the app had changed to a new-game save and Title. The intervening input was not observed. No result save or automated restart is claimed.'},
    'newStateAfterCycle2':state('post-cycle2-new-session')
}
(root/'lifecycle-summary.json').write_text(json.dumps(report,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
print(json.dumps(report,ensure_ascii=False,indent=2))
