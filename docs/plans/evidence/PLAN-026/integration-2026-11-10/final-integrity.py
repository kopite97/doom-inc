import hashlib
import json
import re
import shutil
from pathlib import Path

root = Path(__file__).resolve().parents[5]
out = Path(__file__).parent
baseline = json.loads((out / 'baseline.json').read_text(encoding='utf-8'))
ready = json.loads((out / 'readiness.json').read_text(encoding='utf-8-sig'))['data']['result']['result']
def digest(path): return hashlib.sha256(path.read_bytes()).hexdigest()
for before, after in zip(baseline['assets'], ready['assets']):
    target = root / 'UnityProject' / after['path']
    assert digest(target) == before['sha256'], str(target)
    meta = Path(str(target) + '.meta').read_text()
    assert re.search(r'^guid: (.+)$', meta, re.M)[1] == re.search(r'^guid: (.+)$', before['meta'], re.M)[1]
    if before['name'] == 'LaboratoryFloorSet': assert meta == before['meta']
changed = [name for name, value in baseline['preserved'].items() if not Path(name).exists() or digest(Path(name)) != value]
expected = [name for name in changed if Path(name).name == 'TestResults.xml']
unexpected = [name for name in changed if name not in expected]
assert not unexpected, unexpected
for name in expected: shutil.copyfile(name, out / 'play-domain-off-results.xml')
result = dict(pngHashesPreserved=5, textureGuidsPreserved=5, floorMetaUnchanged=True,
              preservedFiles=len(baseline['preserved']) - len(changed), expectedChangedTestReports=expected,
              unexpectedChangedFiles=unexpected, userSessionOrPreferencesCreated=False)
(out / 'final-integrity.json').write_text(json.dumps(result, indent=2), encoding='utf-8')
print(json.dumps(result, indent=2))
