import fs from 'node:fs';
import path from 'node:path';

// Supplementary current-only diagnostic. This never supplies a missing baseline.
const root = path.resolve(process.argv[2] ?? 'docs/plans/evidence/PLAN-028');
const files = [0, 2].map(level => `present-diagnostic-level${level}.json`);
const statistics = values => {
  const sorted = values.filter(value => Number.isFinite(value) && value >= 0).sort((a, b) => a - b);
  if (sorted.length === 0) return { valid: false };
  const middle = Math.floor(sorted.length / 2);
  return { valid: true, samples: sorted.length,
    median: sorted.length % 2 ? sorted[middle] : (sorted[middle - 1] + sorted[middle]) / 2,
    p95: sorted[Math.ceil(sorted.length * .95) - 1], max: sorted.at(-1),
    mean: sorted.reduce((sum, value) => sum + value, 0) / sorted.length };
};
const recorderKeys = ['renderLoop', 'presentFrame', 'renderCpuCounter', 'mainCpuCounter',
  'playerLoop', 'main', 'waitTarget', 'waitPresent', 'deviceWait'];
const timingKeys = ['cpuFrameMs', 'cpuMainMs', 'cpuPresentWaitMs', 'cpuRenderMs', 'gpuMs'];
const runs = files.map(file => {
  const data = JSON.parse(fs.readFileSync(path.join(root, file), 'utf8'));
  const timingRows = data.rows.filter(row => row.timingCount > 0 && row.freshTiming);
  return { file, implementation: data.implementation, formalComparison: data.formalComparison,
    level: data.level, warmupFrames: data.warmupFrames, sampleFrames: data.sampleFrames,
    fixedDelta: data.fixedDelta, releaseEveryCommands: data.releaseEveryCommands,
    elapsedWallSeconds: data.elapsedWallSeconds, resolution: data.resolution, zoom: data.zoom,
    graphics: data.graphics, threadedGraphics: data.threadedGraphics,
    vSync: data.vSync, targetFrameRate: data.targetFrameRate,
    frameTimingFeatureEnabled: data.frameTimingFeatureEnabled,
    freshTimingSamples: timingRows.length, frameTimingCpuFrequency: data.frameTimingCpuFrequency,
    recorders: data.names.map((name, index) => ({ name, category: data.categories[index],
      unit: data.units[index], available: data.valid[index] })),
    recorderMilliseconds: Object.fromEntries(recorderKeys.map(key => [key,
      statistics(data.rows.map(row => row[key] >= 0 ? row[key] / 1e6 : row[key]))])),
    frameTimingMilliseconds: Object.fromEntries(timingKeys.map(key => [key,
      statistics(timingRows.map(row => row[key]))])),
    presentMarkerAllZero: data.rows.every(row => row.presentFrame === 0),
    note: 'API fields are already milliseconds; ProfilerRecorder durations are converted from nanoseconds. '
      + 'FrameTiming values describe delayed captured frames, not necessarily the current logical command. '
      + 'No RenderLoop subtraction or paired before/after active-CPU estimate is made. '
      + 'Available all-zero Present markers in this single-threaded Editor do not split RenderLoop.' };
});
const summary = { generatedUtc: new Date().toISOString(), formalComparison: false,
  baselineAvailable: false, durationUnit: 'milliseconds', percentile: 'nearest rank; median averages middle pair',
  interpretation: 'Current implementation only, 300 warmup / 600 sample frames per condition. '
    + 'Direct FrameTimingManager and CPU frame-time counters are reported separately from RenderLoop. '
    + 'No baseline active-render CPU values were captured, so active-render CPU before/after change remains unmeasured.',
  runs };
fs.writeFileSync(path.join(root, 'present-diagnostic-summary.json'), `${JSON.stringify(summary, null, 2)}\n`);
for (const run of runs) {
  const api = run.frameTimingMilliseconds;
  console.log(`level ${run.level}: ${run.freshTimingSamples}/${run.sampleFrames} fresh timings; `
    + `cpuRender p95 ${api.cpuRenderMs.p95} ms; cpuMain p95 ${api.cpuMainMs.p95} ms; `
    + `Present marker all zero ${run.presentMarkerAllZero}; formal comparison false`);
}
