"""Review-only GIFs from actual Game View captures; source PNG artwork is never edited."""
import os
import sys
from pathlib import Path
sys.path.insert(0, str(Path(os.environ['TEMP']) / 'plan026-report-tools'))
from PIL import Image

base = Path(__file__).parent / 'screens/animation'
for name, duration in [('working', 120), ('complete', 100)]:
    files = sorted((base / name).glob('frame-*.png'))
    if len(files) != 12:
        raise RuntimeError(f'{name}: expected 12 recorded frames, got {len(files)}')
    frames = [Image.open(path).convert('RGB') for path in files]
    options = dict(save_all=True, append_images=frames[1:], duration=duration, disposal=2)
    if name == 'working': options['loop'] = 0
    frames[0].save(base / (name + '.gif'), **options)
    print(name, len(files), 'frames', duration, 'ms per captured sample')
