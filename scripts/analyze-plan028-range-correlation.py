"""Compare paired logical frames; correlations are diagnostics, not exclusive CPU attribution."""
import json
import math
import statistics
import sys
from pathlib import Path

root = Path(sys.argv[1] if len(sys.argv) > 1 else "docs/plans/evidence/PLAN-028").resolve()
fields = ("main", "playerLoop", "render", "waitForTargetFps", "waitForPresent")


def stats(values):
    if not values:
        return None
    ordered = sorted(values)
    return {"n": len(values), "mean": statistics.fmean(values), "median": statistics.median(values),
            "p95": ordered[math.ceil(len(ordered) * .95) - 1], "min": ordered[0], "max": ordered[-1],
            "constant": ordered[0] == ordered[-1]}


def correlation(xs, ys):
    if len(xs) < 2:
        return None
    mx, my = statistics.fmean(xs), statistics.fmean(ys)
    xx = sum((value - mx) ** 2 for value in xs)
    yy = sum((value - my) ** 2 for value in ys)
    if xx == 0 or yy == 0:
        return None
    return sum((x - mx) * (y - my) for x, y in zip(xs, ys)) / math.sqrt(xx * yy)


results = []
for before_path in sorted(root.glob("baseline-level*-run*.json")):
    after_path = root / before_path.name.replace("baseline-", "after-", 1)
    if not after_path.exists():
        continue
    before = json.loads(before_path.read_text(encoding="utf-8-sig"))
    after = json.loads(after_path.read_text(encoding="utf-8-sig"))
    if before["zoom"] != (8 if before["mostlyOffscreen"] else 1):
        continue
    after_by_step = {row["step"]: row for row in after["rows"]}
    pairs = [(row, after_by_step[row["step"]]) for row in before["rows"] if row["step"] in after_by_step]
    values = {stage: {} for stage in ("before", "after", "delta")}
    summaries = {}
    for field in fields:
        valid = [(a[field] / 1e6, b[field] / 1e6) for a, b in pairs
                 if a.get(field, -1) >= 0 and b.get(field, -1) >= 0]
        values["before"][field] = [a for a, _ in valid]
        values["after"][field] = [b for _, b in valid]
        values["delta"][field] = [b - a for a, b in valid]
        summaries[field] = {stage: stats(values[stage][field]) for stage in values}
    correlations = {}
    for stage, stage_values in values.items():
        correlations[stage] = {}
        for other in fields:
            if other == "render":
                continue
            if len(stage_values["render"]) != len(stage_values[other]):
                correlations[stage]["render_vs_" + other] = None
            else:
                correlations[stage]["render_vs_" + other] = correlation(stage_values["render"], stage_values[other])
    results.append({"baseline": before_path.name, "after": after_path.name, "level": before["level"],
                    "run": before["run"], "mostlyOffscreen": before["mostlyOffscreen"], "pairedRows": len(pairs),
                    "milliseconds": summaries, "pearson": correlations})

output = {"note": "Pairs match fixed logical command step, not physical frame numbers. Durations overlap and may belong to different profiler scopes/threads; never subtract them here to invent exclusive CPU work. Pearson correlation is descriptive, not a cause. Null Pearson means invalid/unavailable or constant data. Zero/constant wait observations cannot prove absence of unmeasured Present waiting. No Gfx.PresentFrame baseline was collected.",
          "pairs": results}
(root / "range-correlation.json").write_text(json.dumps(output, indent=2) + "\n", encoding="utf-8")
print(json.dumps({"pairs": len(results), "output": str(root / "range-correlation.json"),
                  "summary": [{"level": item["level"], "run": item["run"], "offscreen": item["mostlyOffscreen"],
                               "meanDeltasMs": {key: value["delta"]["mean"] if value["delta"] else None
                                                for key, value in item["milliseconds"].items()},
                               "deltaPearson": item["pearson"]["delta"]} for item in results]}, indent=2))
