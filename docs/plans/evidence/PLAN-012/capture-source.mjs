// Run from repository root after tests and Editor setting restoration.
import fs from 'node:fs';
import crypto from 'node:crypto';
const output = 'docs/plans/evidence/PLAN-012/';
const walk = p => fs.readdirSync(p, {withFileTypes:true}).flatMap(e => e.isDirectory() ? walk(p + '/' + e.name) : [p + '/' + e.name]);
const paths = ['UnityProject/Assets/Scripts', 'UnityProject/Assets/SO', 'UnityProject/Assets/CSV', 'UnityProject/Assets/Scenes',
  'UnityProject/Packages', 'UnityProject/ProjectSettings'].flatMap(walk).sort();
const hash = p => crypto.createHash('sha256').update(fs.readFileSync(p)).digest('hex');
const before = JSON.parse(fs.readFileSync(output + 'before-manifest.json', 'utf8').replace(/^\uFEFF/, ''));
const audit = {changedExistingMeta: [], removedMeta: [], missingMeta: []};
for (const entry of before.filter(e => e.path.endsWith('.meta'))) {
  if (!fs.existsSync(entry.path)) audit.removedMeta.push(entry.path);
  else if (hash(entry.path) !== entry.sha256) audit.changedExistingMeta.push(entry.path);
}
for (const p of paths.filter(p => p.startsWith('UnityProject/Assets/') && !p.endsWith('.meta')))
  if (!fs.existsSync(p + '.meta')) audit.missingMeta.push(p);
fs.writeFileSync(output + 'meta-audit-final.json', JSON.stringify(audit, null, 2) + '\n');
fs.writeFileSync(output + 'final-source-manifest.json', JSON.stringify({capturedUtc: new Date().toISOString(),
  files: paths.map(path => ({path, sha256:hash(path)}))}, null, 2) + '\n');
console.log(JSON.stringify({files:paths.length, ...audit}));
