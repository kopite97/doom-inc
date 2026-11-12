import fs from 'node:fs';
import path from 'node:path';

// Analyze only the new PLAN-028 controlled measurements, never historical PLAN-027 samples.
const root = path.resolve(process.argv[2] ?? 'docs/plans/evidence/PLAN-028');
const files = fs.readdirSync(root).filter(name => /^(baseline|after)-level[012](?:-offscreen)?-run[123]\.json$/.test(name));
if (files.length === 0) throw new Error(`No controlled measurements in ${root}`);

const percentile = (values, fraction) => {
  const sorted = values.filter(value => Number.isFinite(value) && value >= 0).sort((a, b) => a - b);
  if (sorted.length === 0) return null;
  if (fraction === .5 && sorted.length % 2 === 0) return (sorted[sorted.length / 2 - 1] + sorted[sorted.length / 2]) / 2;
  return sorted[Math.max(0, Math.min(sorted.length - 1, Math.ceil(sorted.length * fraction) - 1))];
};
const statistics = values => {
  const valid = values.filter(value => Number.isFinite(value) && value >= 0);
  return valid.length === 0 ? { valid: false } : { valid: true, samples: valid.length,
    median: percentile(valid, .5), p95: percentile(valid, .95), max: Math.max(...valid),
    mean: valid.reduce((sum, value) => sum + value, 0) / valid.length };
};
const durations = ['main', 'playerLoop', 'render', 'canvasBuild', 'canvasSend', 'evaluate', 'draw', 'sort', 'status', 'waitForTargetFps', 'waitForPresent'];
const counters = ['gc', 'batches', 'workers', 'moving', 'waiting', 'carrying', 'visible'];
const runs = files.sort().map(file => {
  const data = JSON.parse(fs.readFileSync(path.join(root, file), 'utf8'));
  const expectedWorkers = data.level === 0 ? 1 : data.level === 1 ? 4 : 24;
  const issues = [];
  if (data.sampleCount !== 3600 || data.simulationSteps !== 5400) issues.push('fixed-command count differs');
  if (data.fixedSimulationDelta !== 1 / 60 || data.releaseEverySteps !== 600) issues.push('fixed-command specification differs');
  if (data.zoom !== (data.mostlyOffscreen ? 8 : 1)) issues.push('actual zoom differs from requested scenario');
  if (!data.rows.every(row => row.workers === expectedWorkers)) issues.push('worker count differs');
  const meanVisible = data.rows.reduce((sum, row) => sum + row.visible, 0) / data.rows.length;
  if (data.mostlyOffscreen && meanVisible >= expectedWorkers * .5) issues.push('most workers were actually visible');
  const metrics = Object.fromEntries([...durations, ...counters].map(key => [key,
    statistics((key === 'moving' ? data.rows.slice(1) : data.rows)
      .map(row => durations.includes(key) && row[key] >= 0 ? row[key] / 1e6 : row[key]))]));
  return { file, stage: data.stage, level: data.level, run: data.run, mostlyOffscreen: data.mostlyOffscreen,
    scenarioValid: issues.length === 0, scenarioIssues: issues,
    sampleCount: data.sampleCount, simulationSteps: data.simulationSteps,
    fixedSimulationDelta: data.fixedSimulationDelta, releaseEverySteps: data.releaseEverySteps ?? null,
    warmupSeconds: data.actualWarmupSeconds, sampleSeconds: data.actualSampleSeconds,
    modelStart: data.modelStart, modelEnd: data.modelEnd,
    resolution: data.resolution, zoom: data.zoom, vSync: data.vSync, targetFrameRate: data.targetFrameRate,
    recorderNames: data.recorderNames, recorderValid: data.recorderValid, recorderUnits: data.recorderUnits,
    metrics, pathQueries: data.pathsAfter - data.pathsBefore, visitedCells: data.visitedAfter - data.visitedBefore,
    managedMemoryChange: data.memoryAfter - data.memoryBefore,
    unityMemoryChange: data.unityMemoryAfter - data.unityMemoryBefore,
    collections: data.collectionsAfter.map((value, index) => value - data.collectionsBefore[index]),
    activeTransforms: data.activeTransforms };
});

const grouped = [];
for (const level of [0, 1, 2]) {
  for (const mostlyOffscreen of [false, true]) {
    const matching = runs.filter(run => run.scenarioValid && run.level === level && run.mostlyOffscreen === mostlyOffscreen);
    if (matching.length === 0) continue;
    const stages = {};
    for (const stage of ['baseline', 'after']) {
      const selected = matching.filter(run => run.stage === stage);
      if (selected.length === 0) continue;
      stages[stage] = { runs: selected.length, metrics: {} };
      for (const metric of [...durations, ...counters]) {
        const valid = selected.filter(run => run.metrics[metric].valid);
        if (valid.length === 0) { stages[stage].metrics[metric] = { valid: false }; continue; }
        const medians = valid.map(run => run.metrics[metric].median);
        const p95s = valid.map(run => run.metrics[metric].p95);
        stages[stage].metrics[metric] = { valid: true, median: percentile(medians, .5),
          medianMin: Math.min(...medians), medianMax: Math.max(...medians),
          p95: percentile(p95s, .5), p95Min: Math.min(...p95s), p95Max: Math.max(...p95s),
          max: Math.max(...valid.map(run => run.metrics[metric].max)) };
      }
    }
    const pairs = matching.filter(run => run.stage === 'baseline').map(before => {
      const after = matching.find(run => run.stage === 'after' && run.run === before.run);
      if (!after) return { run: before.run, afterMissing: true };
      const equal = (key) => JSON.stringify(before[key]) === JSON.stringify(after[key]);
      const comparable = ['sampleCount', 'simulationSteps', 'fixedSimulationDelta', 'releaseEverySteps', 'modelStart', 'modelEnd', 'resolution', 'zoom', 'vSync', 'targetFrameRate'].every(equal);
      const workload = ['workers', 'moving', 'waiting', 'carrying'].map(key => ({ metric: key,
        beforeMean: before.metrics[key].mean, afterMean: after.metrics[key].mean,
        beforeMedian: before.metrics[key].median, afterMedian: after.metrics[key].median }));
      return { run: before.run, comparable, samePathQueries: equal('pathQueries'), sameVisitedCells: equal('visitedCells'), workload };
    });
    const differences = {};
    if (stages.baseline && stages.after) {
      for (const metric of [...durations, ...counters]) {
        const before = stages.baseline.metrics[metric];
        const after = stages.after.metrics[metric];
        if (!before?.valid || !after?.valid) continue;
        differences[metric] = { medianChange: after.median - before.median,
          medianChangePercent: before.median === 0 ? null : (after.median / before.median - 1) * 100,
          p95Change: after.p95 - before.p95,
          allAfterP95AboveBaselineRange: after.p95Min > before.p95Max };
      }
    }
    grouped.push({ level, mostlyOffscreen, stages, pairs, differences });
  }
}

const result = { generatedUtc: new Date().toISOString(), durationUnit: 'milliseconds',
  gcUnit: 'bytes/frame', workloadUnit: 'observed worker count/frame', percentile: 'nearest rank; median averages middle pair',
  note: 'Main Thread, PlayerLoop and wait-marker costs overlap; never sum or subtract them as exclusive CPU work. Invalid counters stay invalid. Moving-count summaries exclude the first collected row because the raw transition reference was initialized before warmup; only that row can include warmup displacement. Timing, GC and other counters retain all samples. Body visibility can change due to interpolation. Pair equality checks fixed input/model duration and aggregate logical workload, not product state identity. Missing runs and workloads must be reviewed before a pass claim. Total GC is not isolated interpolation GC.', runs, groups: grouped };
const output = path.join(root, 'performance-summary.json');
fs.writeFileSync(output, JSON.stringify(result, null, 2) + '\n');
console.log(JSON.stringify({ output, runs: runs.length, groups: grouped.length,
  excludedScenarios: runs.filter(run => !run.scenarioValid).map(run => ({ file: run.file, issues: run.scenarioIssues })),
  comparisons: grouped.map(group => ({ level: group.level, mostlyOffscreen: group.mostlyOffscreen,
    baselineRuns: group.stages.baseline?.runs ?? 0, afterRuns: group.stages.after?.runs ?? 0,
    pairs: group.pairs, differences: group.differences })) }, null, 2));
