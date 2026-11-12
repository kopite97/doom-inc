"""Encode the Editor's unmodified PNG recording as an MP4 and a looping preview."""
import subprocess
import sys
from pathlib import Path
import imageio_ffmpeg

root = Path(__file__).resolve().parent
stage = sys.argv[1]
assert stage in ("baseline", "after")
frames = root / ("movement-" + stage + "-zoom2-v2-frames")
assert len(list(frames.glob("frame-*.png"))) == 150
ffmpeg = imageio_ffmpeg.get_ffmpeg_exe()
common = [ffmpeg, "-hide_banner", "-loglevel", "error", "-y", "-framerate", "30", "-i", str(frames / "frame-%03d.png")]
subprocess.run(common + ["-c:v", "libx264", "-crf", "18", "-pix_fmt", "yuv420p", "-movflags", "+faststart",
                        str(root / ("movement-" + stage + ".mp4"))], check=True)
subprocess.run(common + ["-filter_complex", "split[a][b];[a]palettegen=max_colors=128[p];[b][p]paletteuse=dither=none",
                        "-loop", "0", str(root / ("movement-" + stage + ".gif"))], check=True)
print(root / ("movement-" + stage + ".mp4"))
