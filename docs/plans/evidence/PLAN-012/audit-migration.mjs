// Run from the repository root. Compare captured pre-migration SO values with the frozen CSV baseline.
import fs from 'node:fs';
const root = 'docs/plans/evidence/PLAN-012/';
const read = p => fs.readFileSync(p, 'utf8').replace(/^\uFEFF/, '');
const before = JSON.parse(read(root + 'before-so.json'));
const baseline = JSON.parse(read(root + 'baseline-data.json'));
const field = (text, key) => text.match(new RegExp('^\\s+' + key + ': (.*)$', 'm'))?.[1];
const ids = new Map(Object.entries(before).filter(([p]) => fs.existsSync(p + '.meta')).map(([p, t]) =>
  [read(p + '.meta').match(/guid: (\w+)/)[1], field(t, 'definitionId')]));
const comparisons = [], differences = [], normalizedUnused = [];
for (const [path, yaml] of Object.entries(before)) {
  const id = field(yaml, 'definitionId');
  const row = id ? Object.values(baseline.tables).flat().find(r => r.definitionId === id) : baseline.tables.invasion[0];
  if (!row) throw new Error('Missing baseline row: ' + path);
  for (const [key, value] of Object.entries(row)) {
    if (!id && key === 'definitionId') continue;
    let original = field(yaml, key);
    if (key === 'displayName') original = JSON.parse(original);
    if (key === 'components') original = original === '00000000' ? 'ResourceStack' : 'ResourceContainer';
    if (key === 'role') original = path.includes('/Creature/') ? ['Prototype', 'Support', 'Invader'][Number(original ?? 0)]
      : ['None', 'Gathering', 'Storage', 'Processing', 'Cultivation'][Number(original)];
    const coordinate = {width:['footprint','x'],height:['footprint','y'],interactionX:['interactionOffset','x'],interactionY:['interactionOffset','y']}[key];
    if (coordinate) original = field(yaml, coordinate[0]).match(new RegExp(coordinate[1] + ': (-?\\d+)'))[1];
    if (['resource','inputResource','outputResource','supportOutput','invaderOutput'].includes(key))
      original = ids.get(original?.match(/guid: (\w+)/)?.[1]) ?? '';
    if (original === undefined && ['inputAmount','outputAmount','productionSeconds'].includes(key)) original = '0';
    const item = {id: row.definitionId, field: key, before: original, after: value};
    const unused = path.includes('/Facility/') && (
      key === 'storageCapacity' && row.role !== 'Storage' ||
      ['gatheringAmount','gatheringSeconds'].includes(key) && row.role !== 'Gathering' ||
      ['supportOutput','invaderOutput'].includes(key) && row.role !== 'Cultivation' ||
      ['inputAmount','outputAmount','productionSeconds'].includes(key) && !['Processing','Cultivation'].includes(row.role));
    if (unused) normalizedUnused.push(item);
    else { comparisons.push(item); if (String(original) !== String(value)) differences.push(item); }
  }
}
const result = {comparedFields: comparisons.length, differences, normalizedUnused, comparisons};
fs.writeFileSync(root + 'field-migration-audit.json', JSON.stringify(result, null, 2) + '\n');
console.log(JSON.stringify({comparedFields: comparisons.length, differences, normalizedUnused: normalizedUnused.length}));
if (differences.length) process.exitCode = 1;
