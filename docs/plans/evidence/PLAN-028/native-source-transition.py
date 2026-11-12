"""Reversible source-only baseline probe; root holds Unity auto-refresh during each transition."""
import hashlib
import json
import os
import subprocess
import sys
from pathlib import Path

root = Path(__file__).resolve().parents[4]
evidence = Path(__file__).resolve().parent
backup = Path(os.environ['TEMP']) / 'doom-inc-plan028-native-source'
changed = [
    'Gameplay/Creature/WorkerComponent.cs', 'Gameplay/Creature/WorkerManager.cs',
    'Presentation/Gameplay/Laboratory/LaboratoryMapView.cs',
    'Presentation/Gameplay/Laboratory/LaboratoryMapView.Art.cs',
    'Presentation/Gameplay/Laboratory/LaboratoryMapView.Status.cs',
    'Presentation/Gameplay/Laboratory/LaboratoryView.cs',
]
excluded = [
    'Gameplay/Creature/WorkerMoveSegment.cs', 'Gameplay/Creature/WorkerMoveObservation.cs',
    'Presentation/Gameplay/Laboratory/CreatureMotion.cs',
    'Tests/EditMode/Foundation/Gameplay/WorkerMovementObservationTests.cs',
    'Tests/EditMode/Foundation/Presentation/CreatureMotionTests.cs',
    'Tests/PlayMode/Laboratory/LaboratoryViewTests.Movement.cs',
    'Tests/PlayMode/Laboratory/LaboratoryViewTests.MovementCosts.cs',
]
prefix = 'UnityProject/Assets/Scripts/'
changed = [prefix + name for name in changed]
excluded = [prefix + name for name in excluded]
sha = lambda data: hashlib.sha256(data).hexdigest()
original = json.loads((evidence / 'source-baseline.json').read_text(encoding='utf-8-sig'))['productSources']

def product_hashes():
    return {str(p.relative_to(root)).replace('\\', '/'): sha(p.read_bytes())
            for p in sorted((root / prefix).rglob('*.cs'))
            if '/Tests/' not in p.as_posix() and '/Editor/' not in p.as_posix()}

def git_baseline(name):
    data = subprocess.check_output(['git', 'show', 'HEAD:' + name], cwd=root)
    if sha(data) == original[name]:
        return data
    windows_data = data.replace(b'\r\n', b'\n').replace(b'\n', b'\r\n')
    if sha(windows_data) == original[name]:
        return windows_data
    raise RuntimeError('HEAD does not reproduce original baseline hash: ' + name)

mode = sys.argv[1]
manifest_path = backup / 'manifest.json'
if mode == 'prepare':
    if manifest_path.exists():
        raise RuntimeError('Refusing to replace an existing exact-byte backup')
    backup.mkdir(parents=True, exist_ok=True)
    sources = product_hashes()
    for name, expected in original.items():
        if name not in changed and sources.get(name) != expected:
            raise RuntimeError('Unexpected baseline source change: ' + name)
    records = {}
    for name in changed + excluded:
        source = root / name
        for item in (source, Path(str(source) + '.meta')):
            relative = str(item.relative_to(root)).replace('\\', '/')
            destination = backup / relative
            destination.parent.mkdir(parents=True, exist_ok=True)
            data = item.read_bytes()
            destination.write_bytes(data)
            records[relative] = sha(data)
    for name in changed:
        git_baseline(name)
    manifest = {'backup': str(backup), 'productSources': sources, 'exactByteBackup': records,
                'originalBaselineSources': original, 'changed': changed, 'excluded': excluded}
    manifest_path.write_text(json.dumps(manifest, indent=2), encoding='utf-8')
    (evidence / 'native-comparison').mkdir(exist_ok=True)
    (evidence / 'native-comparison' / 'source-backup-manifest.json').write_text(json.dumps(manifest, indent=2), encoding='utf-8')
    print(json.dumps({'backupFiles': len(records), 'productFiles': len(sources), 'originalFiles': len(original)}))
elif mode in ('baseline', 'restore'):
    manifest = json.loads(manifest_path.read_text(encoding='utf-8'))
    for name, expected in manifest['exactByteBackup'].items():
        if sha((backup / name).read_bytes()) != expected:
            raise RuntimeError('Backup integrity failed: ' + name)
    if mode == 'baseline':
        if product_hashes() != manifest['productSources']:
            raise RuntimeError('Current product sources no longer match backed-up final implementation')
        for name in changed:
            (root / name).write_bytes(git_baseline(name))
        for name in excluded:
            (root / name).write_bytes(b'#if false\n' + (backup / name).read_bytes() + b'\n#endif\n')
        current = product_hashes()
        if any(current.get(name) != expected for name, expected in original.items()):
            raise RuntimeError('Original compiled product source hashes differ')
    else:
        for name in changed + excluded:
            (root / name).write_bytes((backup / name).read_bytes())
        if product_hashes() != manifest['productSources']:
            raise RuntimeError('Final product source restoration failed')
    for name, expected in manifest['exactByteBackup'].items():
        if name.endswith('.meta') and sha((root / name).read_bytes()) != expected:
            raise RuntimeError('Unity meta changed: ' + name)
    output = {'stage': mode, 'productSources': product_hashes(),
              'original137Match': mode == 'baseline', 'exactFinalRestored': mode == 'restore',
              'metaUnchanged': True, 'excludedFromBaselineCompilation': excluded if mode == 'baseline' else []}
    (evidence / 'native-comparison' / ('sources-' + mode + '.json')).write_text(json.dumps(output, indent=2), encoding='utf-8')
    print(json.dumps({'stage': mode, 'productFiles': len(output['productSources']), 'metaUnchanged': True}))
else:
    raise RuntimeError('Unknown operation')
