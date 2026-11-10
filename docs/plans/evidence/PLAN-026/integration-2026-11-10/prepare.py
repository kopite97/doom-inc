"""Capture the pre-integration baseline and immutable delivered frame metadata."""
import hashlib
import json
from pathlib import Path
import subprocess

root = Path(__file__).resolve().parents[5]
out = Path(__file__).parent
project = root / 'UnityProject'
art = root.parent / 'Art'
items = [
    ('backgrounds/LaboratoryFloor', 'r003', 'LaboratoryFloorSet', 'World/Laboratory/LaboratoryFloorSet.png'),
    ('buildings/Gathering', 'r003', 'Gathering', 'Require/PLAN-026/PLAN-026-SPR-002/r003/Gathering.png'),
    ('characters/PrototypeSupport', 'r002', 'PrototypeSupport', 'Require/PLAN-026/PLAN-026-SPR-003/r002/PrototypeSupport.png'),
    ('resources/OrganicMatter', 'r001', 'OrganicMatter', 'Require/PLAN-026/PLAN-026-SPR-004/r001/OrganicMatter.png'),
    ('ui/Build', 'r001', 'Build', 'Require/PLAN-014/PLAN-014-SPR-006/r001/Build.png'),
]
def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()

if not (out / 'baseline.json').exists():
    records = []
    for folder, revision, name, relative in items:
        source = art / 'assets' / folder / 'revisions' / revision / 'export' / (name + '.png')
        target = project / 'Assets/Sprites' / relative
        assert digest(source) == digest(target), name
        meta = Path(str(target) + '.meta')
        records.append(dict(name=name, revision=revision, source=str(source), originalUnityPath=str(target), sha256=digest(target), meta=meta.read_text() if meta.exists() else None))
    preserved = list((project / 'Assets/JSON/Balance').glob('*.json'))
    preserved += list((project / 'ProjectSettings').glob('*'))
    saved = Path.home() / 'AppData/LocalLow/DefaultCompany/UnityProject'
    preserved += [p for p in saved.rglob('*') if p.is_file() and p.suffix not in ('.log',)]
    baseline = dict(assets=records, preserved={str(p): digest(p) for p in preserved if p.is_file()},
                    editor=dict(version='6000.3.27f1', target='Android', scene='Assets/Scenes/Gameplay/Laboratory/Temp_Laboratory.unity', enterPlayModeOptionsEnabled=True, enterPlayModeOptions='DisableDomainReload', runInBackground=False))
    (out / 'baseline.json').write_text(json.dumps(baseline, indent=2), encoding='utf-8')
    (out / 'git-status-before.txt').write_bytes(subprocess.check_output(['git', 'status', '--short'], cwd=root))
    (out / 'PrototypeSupport-r002-frame-metadata.json').write_bytes((art / 'docs/plans/reviews/PLAN-003/r002/frame-metadata.json').read_bytes())
print('Baseline captured; 5 delivered PNG hashes match approved Art sources.')
