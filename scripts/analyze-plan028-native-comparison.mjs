import fs from 'node:fs';
import path from 'node:path';
import crypto from 'node:crypto';

// Analyze the supplementary pre/post API diagnosis separately from the formal 20 runs.
const root = path.resolve(process.argv[2] ?? 'docs/plans/evidence/PLAN-028/native-comparison');
if (!fs.existsSync(root)) throw new Error(`No native comparison directory: ${root}`);
const expectedWarmupFrames = 1800;
const expectedSampleFrames = 3600;
const readJson = file => JSON.parse(fs.readFileSync(file, 'utf8').replace(/^\uFEFF/, ''));
const sha256 = file => crypto.createHash('sha256').update(fs.readFileSync(file)).digest('hex');
const median = values => {
  const sorted = [...values].sort((a, b) => a - b);
  if (!sorted.length) return null;
  const middle = Math.floor(sorted.length / 2);
  return sorted.length % 2 ? sorted[middle] : (sorted[middle - 1] + sorted[middle]) / 2;
};
const statistics = values => {
  const sorted = values.filter(value => Number.isFinite(value) && value >= 0).sort((a, b) => a - b);
  if (!sorted.length) return { valid: false, samples: 0 };
  return { valid: true, samples: sorted.length, median: median(sorted),
    p95: sorted[Math.ceil(sorted.length * .95) - 1], max: sorted.at(-1),
    mean: sorted.reduce((sum, value) => sum + value, 0) / sorted.length };
};
const apiMetrics = { apiCpuFrame: 'cpuFrameMs', apiMain: 'cpuMainMs', apiRender: 'cpuRenderMs',
  apiPresentWait: 'cpuPresentWaitMs', apiGpu: 'gpuMs' };
const recorderMetrics = { recorderRenderLoop: 'renderLoop', recorderPresent: 'presentFrame',
  recorderCpuRender: 'renderCpuCounter', recorderCpuMain: 'mainCpuCounter', recorderPlayerLoop: 'playerLoop',
  recorderMain: 'main', recorderWaitTarget: 'waitTarget', recorderWaitPresent: 'waitPresent', recorderDeviceWait: 'deviceWait' };
const recorderNames = { renderLoop: 'RenderLoop', presentFrame: 'Gfx.PresentFrame',
  renderCpuCounter: 'CPU Render Thread Frame Time', mainCpuCounter: 'CPU Main Thread Frame Time',
  playerLoop: 'PlayerLoop', main: 'Main Thread', waitTarget: 'WaitForTargetFPS',
  waitPresent: 'Gfx.WaitForPresentOnGfxThread', deviceWait: 'GfxDeviceD3D12.WaitForLastPresent' };
const conditionKeys = ['warmupFrames', 'sampleFrames', 'fixedDelta', 'releaseEveryCommands', 'resolution', 'zoom',
  'graphics', 'threadedGraphics', 'vSync', 'targetFrameRate', 'frameTimingFeatureEnabled', 'frameTimingCpuFrequency'];
const missing = [];
const runs = [];
const provenance = [];
const globalProvenance = fs.readdirSync(root).filter(name => /^(source|sources|environment|condition|collection-conditions).*\.json$/.test(name))
  .map(name => ({ name, sha256: sha256(path.join(root, name)) }));
const optionalJson = name => fs.existsSync(path.join(root, name)) ? readJson(path.join(root, name)) : null;
const backupManifest = optionalJson('source-backup-manifest.json');
const baselineManifest = optionalJson('sources-baseline.json');
const restoreManifest = optionalJson('sources-restore.json');
const originalManifestFile = path.resolve(root, '../source-baseline.json');
const originalManifest = fs.existsSync(originalManifestFile) ? readJson(originalManifestFile) : null;
const collectionConditions = optionalJson('collection-conditions.json');
const repositoryRoot = path.resolve(root, '../../../../..');
const collectionFiles = Object.entries(collectionConditions?.files ?? {}).map(([role, entry]) => {
  const file = path.resolve(repositoryRoot, entry.path);
  const current = fs.existsSync(file) ? sha256(file) : null;
  return { role, path: entry.path, expected: entry.sha256, current,
    valid: !!current && current.toLowerCase() === entry.sha256.toLowerCase() };
});
const compareHashes = (expected, observed, includeExtra) => {
  if (!expected || !observed) return { valid: false, missingManifest: true };
  const missing = Object.keys(expected).filter(key => !(key in observed));
  const different = Object.keys(expected).filter(key => key in observed
    && String(expected[key]).toLowerCase() !== String(observed[key]).toLowerCase());
  const extra = includeExtra ? Object.keys(observed).filter(key => !(key in expected)) : [];
  return { valid: !missing.length && !different.length && !extra.length,
    expectedCount: Object.keys(expected).length, observedCount: Object.keys(observed).length, missing, different, extra };
};
const readCurrentHashes = expected => Object.fromEntries(Object.keys(expected ?? {}).map(name => {
  const file = path.resolve(repositoryRoot, name);
  return [name, fs.existsSync(file) ? sha256(file) : null];
}));
const sourceChecks = {
  initial137VersusBackup: compareHashes(originalManifest?.productSources, backupManifest?.originalBaselineSources, true),
  initial137VersusNativeBaseline: compareHashes(originalManifest?.productSources, baselineManifest?.productSources, false),
  final140VersusRestored: compareHashes(backupManifest?.productSources, restoreManifest?.productSources, true),
  baselineMetaUnchanged: baselineManifest?.metaUnchanged === true,
  restoredMetaUnchanged: restoreManifest?.metaUnchanged === true,
  exactFinalByteRestore: restoreManifest?.exactFinalRestored === true,
  protectedByteBackupCount: Object.keys(backupManifest?.exactByteBackup ?? {}).length,
  current140VersusFinalBackup: compareHashes(backupManifest?.productSources, readCurrentHashes(backupManifest?.productSources), true),
  currentProtectedBytesVersusBackup: compareHashes(backupManifest?.exactByteBackup, readCurrentHashes(backupManifest?.exactByteBackup), true),
};
const fixtures = [0, 2].map(level => {
  const name = `movement-fixture-${level}.json`;
  const file = path.resolve(root, '..', name);
  const expected = originalManifest?.fixtures?.[name] ?? null;
  const actual = fs.existsSync(file) ? sha256(file) : null;
  return { name, expected, actual, valid: !!expected && expected.toLowerCase() === actual?.toLowerCase() };
});
const provenanceComplete = sourceChecks.initial137VersusBackup.valid && sourceChecks.initial137VersusNativeBaseline.valid
  && sourceChecks.final140VersusRestored.valid && sourceChecks.baselineMetaUnchanged && sourceChecks.restoredMetaUnchanged
  && sourceChecks.exactFinalByteRestore && sourceChecks.protectedByteBackupCount === 26 && fixtures.every(fixture => fixture.valid)
  && sourceChecks.current140VersusFinalBackup.valid && sourceChecks.currentProtectedBytesVersusBackup.valid
  && collectionFiles.length === 7 && collectionFiles.every(file => file.valid)
  && collectionConditions.warmupFrames === expectedWarmupFrames && collectionConditions.sampleFrames === expectedSampleFrames;
for (const stage of ['baseline', 'after']) for (const run of [1, 2, 3]) {
  const directory = `${stage}-run${run}`;
  const folder = path.join(root, directory);
  if (!fs.existsSync(folder)) { missing.push(directory); continue; }
  const resultFile = path.join(folder, 'test-result.json');
  const testResult = fs.existsSync(resultFile) ? readJson(resultFile)?.data?.result : null;
  const testPassed = testResult?.status === 'completed' && testResult.summary?.total === 1
    && testResult.summary?.passed === 1 && testResult.summary?.failed === 0;
  const stateFile = path.join(folder, 'source-state.json');
  const state = fs.existsSync(stateFile) ? readJson(stateFile) : null;
  const stageSourceCheck = compareHashes(stage === 'baseline' ? baselineManifest?.productSources : backupManifest?.productSources,
    state?.productSources, true);
  provenance.push({ directory, testPassed, testDurationSeconds: testResult?.duration ?? null,
    stageSourceCheck, supportingFiles: fs.readdirSync(folder)
    .filter(name => !/^present-diagnostic-level[02]\.json$/.test(name))
    .filter(name => fs.statSync(path.join(folder, name)).isFile())
    .map(name => ({ name, sha256: sha256(path.join(folder, name)) })) });
  for (const level of [0, 2]) {
    const relative = `${directory}/present-diagnostic-level${level}.json`;
    const file = path.join(root, relative);
    if (!fs.existsSync(file)) { missing.push(relative); continue; }
    const data = readJson(file);
    const rows = data.rows ?? [];
    const issues = [];
    const check = (condition, issue) => { if (!condition) issues.push(issue); };
    check(testPassed, 'containing test run did not complete and pass 1/1');
    check(stageSourceCheck.valid, 'containing run source state is absent or mismatches its stage');
    check(data.level === level, 'directory/file level mismatch');
    check(data.implementation === directory, 'implementation label does not match verified stage/run directory');
    check(data.warmupFrames === expectedWarmupFrames && data.sampleFrames === expectedSampleFrames
      && rows.length === expectedSampleFrames, 'expected 1800 warmup / 3600 sample frames');
    check(data.fixedDelta === 1 / 60 && data.releaseEveryCommands === 600, 'fixed delta or command schedule mismatch');
    check(data.zoom === 1 && JSON.stringify(data.resolution) === '[1280,720]', 'zoom/resolution mismatch');
    check(data.vSync === 0 && data.targetFrameRate === 60, 'frame cap mismatch');
    check(data.frameTimingFeatureEnabled === true && data.frameTimingCpuFrequency > 0, 'FrameTiming unavailable');
    check(rows.every((row, index) => row.command === expectedWarmupFrames + 1 + index), 'command sequence is not 1801..5400');
    check(rows.every(row => Math.abs(row.simulationSeconds - row.command / 60) < 1e-9), 'logical elapsed time mismatch');
    check(rows.every((row, index) => index === 0 || row.frame > rows[index - 1].frame), 'render frame did not advance');
    const timingRows = rows.filter(row => row.timingCount > 0 && row.freshTiming && row.timingTimestamp > 0);
    check(timingRows.length === expectedSampleFrames, 'fewer than 3600 fresh valid FrameTiming observations');
    check(timingRows.every((row, index) => index === 0 || row.timingTimestamp > timingRows[index - 1].timingTimestamp), 'FrameTiming timestamps are not strictly increasing');
    check(timingRows.every(row => ['cpuMainMs', 'cpuRenderMs', 'cpuFrameMs'].every(key => Number.isFinite(row[key]) && row[key] > 0)), 'non-positive or non-finite direct CPU timing');
    const conditions = Object.fromEntries(conditionKeys.map(key => [key, data[key] ?? null]));
    const metrics = Object.fromEntries(Object.entries(apiMetrics).map(([metric, key]) => [metric,
      statistics(timingRows.map(row => row[key]))]));
    for (const [metric, key] of Object.entries(recorderMetrics)) {
      const index = data.names.indexOf(recorderNames[key]);
      const available = index >= 0 && data.valid[index] && data.units[index] === 'TimeNanoseconds';
      metrics[metric] = available
        ? statistics(rows.map(row => row[key] >= 0 ? row[key] / 1e6 : row[key]))
        : { valid: false, samples: 0 };
    }
    runs.push({ file: relative, sha256: sha256(file), stage, run, level, conditions,
      scenarioValid: issues.length === 0, scenarioIssues: issues, rawImplementationLabel: data.implementation,
      rawFormalComparison: data.formalComparison,
      implementationIdentity: 'Stage/run directory, matching raw label, and root-captured source manifest; label alone is insufficient.',
      freshTimingSamples: timingRows.length, elapsedWallSeconds: data.elapsedWallSeconds,
      logicalCommands: rows.length ? [rows[0].command, rows.at(-1).command] : [],
      logicalElapsed: rows.length ? [rows[0].simulationSeconds, rows.at(-1).simulationSeconds] : [],
      presentMarkerAllZero: rows.every(row => row.presentFrame === 0), metrics });
  }
}
const groups = [0, 2].map(level => {
  const stages = {};
  for (const stage of ['baseline', 'after']) {
    const selected = runs.filter(run => run.level === level && run.stage === stage && run.scenarioValid);
    const metrics = {};
    for (const metric of [...Object.keys(apiMetrics), ...Object.keys(recorderMetrics)]) {
      const values = selected.map(run => run.metrics[metric]).filter(value => value.valid);
      metrics[metric] = !values.length ? { valid: false, runs: 0 } : { valid: true, runs: values.length,
        median: median(values.map(value => value.median)), p95: median(values.map(value => value.p95)),
        medianMin: Math.min(...values.map(value => value.median)), medianMax: Math.max(...values.map(value => value.median)),
        p95Min: Math.min(...values.map(value => value.p95)), p95Max: Math.max(...values.map(value => value.p95)),
        max: Math.max(...values.map(value => value.max)) };
    }
    stages[stage] = { runs: selected.length, metrics };
  }
  const pairs = [1, 2, 3].map(run => {
    const before = runs.find(item => item.level === level && item.stage === 'baseline' && item.run === run);
    const after = runs.find(item => item.level === level && item.stage === 'after' && item.run === run);
    if (!before || !after) return { run, complete: false };
    const conditionMismatches = conditionKeys.filter(key => JSON.stringify(before.conditions[key]) !== JSON.stringify(after.conditions[key]));
    const differences = {};
    for (const metric of Object.keys(before.metrics)) {
      const left = before.metrics[metric]; const right = after.metrics[metric];
      if (left.valid && right.valid) differences[metric] = { medianChange: right.median - left.median, p95Change: right.p95 - left.p95 };
    }
    return { run, complete: true, comparable: before.scenarioValid && after.scenarioValid && !conditionMismatches.length,
      conditionMismatches, differences };
  });
  const differences = {};
  for (const metric of Object.keys(stages.baseline.metrics)) {
    const before = stages.baseline.metrics[metric]; const after = stages.after.metrics[metric];
    if (!before.valid || !after.valid) continue;
    differences[metric] = { medianChange: after.median - before.median, p95Change: after.p95 - before.p95,
      p95ChangePercent: before.p95 === 0 ? null : (after.p95 / before.p95 - 1) * 100,
      allAfterP95AboveBaselineRange: before.runs === 3 && after.runs === 3 ? after.p95Min > before.p95Max : null,
      allAfterP95BelowBaselineRange: before.runs === 3 && after.runs === 3 ? after.p95Max < before.p95Min : null };
  }
  return { level, stages, pairs, differences };
});
const result = { generatedUtc: new Date().toISOString(), comparison: 'Supplementary before/after direct native frame timing at the same 1800/3600-frame logical workload as the formal 20 runs; separately collected.',
  durationUnit: 'milliseconds', percentile: 'nearest rank; medians average middle pair; stage medians/p95s are medians of per-run statistics',
  implementationLabels: 'Each run raw label must equal its stage/run directory. Verified source manifests additionally identify the actual implementation.',
  performanceAcceptance: 'Requires review. Scenario validity does not mean a CPU regression passed.',
  limitations: ['Three 1800/3600-frame runs per stage and level cover logical seconds 30..90. Only levels 0 and 2 are measured here; the formal 20-run measurements remain separate.',
    'FrameTiming API observations are delayed frames. Do not pair them to the same logical row or subtract CPU/GPU/RenderLoop samples.',
    'No Present estimate, no unmeasured baseline reconstruction, and no ad-hoc RenderLoop subtraction.',
    'This diagnostic contains no GC allocation locations, logical worker/cargo/path counts, or state snapshots; formal measurements and functional tests supply separate evidence.',
    'Source and fixture hashes and successful test-run results must be verified from root-captured provenance; the implementation label alone is insufficient.'],
  collectionComplete: missing.length === 0 && runs.length === 12 && runs.every(run => run.scenarioValid)
    && groups.every(group => group.pairs.every(pair => pair.comparable))
    && provenance.every(run => run.testPassed && run.stageSourceCheck.valid),
  provenanceComplete, missing, globalProvenance, collectionFiles, sourceChecks, fixtures, provenance, runs, groups };
result.complete = result.collectionComplete && result.provenanceComplete;
fs.writeFileSync(path.join(root, 'summary.json'), `${JSON.stringify(result, null, 2)}\n`);
console.log(JSON.stringify({ output: path.join(root, 'summary.json'), complete: result.complete, runs: runs.length,
  missing, invalid: runs.filter(run => !run.scenarioValid).map(run => ({ file: run.file, issues: run.scenarioIssues })),
  groups: groups.map(group => ({ level: group.level, baselineRuns: group.stages.baseline.runs,
    afterRuns: group.stages.after.runs, differences: group.differences })) }, null, 2));
