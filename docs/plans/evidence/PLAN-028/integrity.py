"""Record preserved art, model data, scenes, settings and user files for PLAN-028."""
import hashlib
import json
import subprocess
import sys
from pathlib import Path

repo = Path(__file__).resolve().parents[4]
project = repo / "UnityProject"
evidence = Path(__file__).resolve().parent
env = json.loads((evidence / "environment-before.json").read_text(encoding="utf-8-sig"))
persistent = Path(env["data"]["result"]["result"]["persistent"])
files = {}
for folder in ("Assets/Sprites", "Assets/JSON", "Assets/SO", "Assets/Scenes", "ProjectSettings"):
    for path in sorted((project / folder).rglob("*")):
        if path.is_file():
            files[str(path.relative_to(repo)).replace("\\", "/")] = hashlib.sha256(path.read_bytes()).hexdigest()
user = {}
if persistent.exists():
    for path in sorted(persistent.rglob("*")):
        if path.is_file() and not any(part in ("Unity", "Analytics", "Crashes") for part in path.relative_to(persistent).parts):
            user[str(path.relative_to(persistent)).replace("\\", "/")] = hashlib.sha256(path.read_bytes()).hexdigest()
result = {"head": subprocess.check_output(["git", "rev-parse", "HEAD"], cwd=repo, text=True).strip(),
          "files": files, "persistent": str(persistent), "userFiles": user}
if sys.argv[1] == "after":
    before = json.loads((evidence / "integrity-before.json").read_text(encoding="utf-8"))
    result["changedFiles"] = [key for key in set(before["files"]) | set(files) if before["files"].get(key) != files.get(key)]
    result["changedUserFiles"] = [key for key in set(before["userFiles"]) | set(user) if before["userFiles"].get(key) != user.get(key)]
(evidence / ("integrity-" + sys.argv[1] + ".json")).write_text(json.dumps(result, ensure_ascii=False, indent=2), encoding="utf-8")
print(json.dumps({"files": len(files), "userFiles": len(user), "changedFiles": result.get("changedFiles"), "changedUserFiles": result.get("changedUserFiles")}))
