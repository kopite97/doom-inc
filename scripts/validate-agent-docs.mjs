import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

// Read-only checks. PLAN type/status are read from front matter, never an index.
const root = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..');
const errors = [];
const allowedStates = new Set(['proposed', 'active', 'completed', 'rejected', 'on hold']);
const allowedTypes = new Set(['development', 'art']);
const relative = file => path.relative(root, file).replaceAll(path.sep, '/');
const read = file => fs.readFileSync(file, 'utf8').replace(/^\uFEFF/, '');
const lineCount = text => text.length ? text.replace(/\r?\n$/, '').split(/\r?\n/).length : 0;

function markdownFiles(directory) {
  if (!fs.existsSync(directory)) {
    errors.push(`Missing directory: ${relative(directory)}`);
    return [];
  }
  return fs.readdirSync(directory, { withFileTypes: true }).flatMap(entry => {
    const file = path.join(directory, entry.name);
    if (entry.isDirectory()) return markdownFiles(file);
    return entry.isFile() && entry.name.endsWith('.md') ? [file] : [];
  }).sort();
}

const rootGuide = path.join(root, 'AGENTS.md');
const planGuide = path.join(root, 'docs/plans/AGETNS.md');
const guides = [rootGuide, planGuide, ...markdownFiles(path.join(root, 'docs/agents'))];
for (const file of guides) {
  if (!fs.existsSync(file)) {
    errors.push(`Missing guide: ${relative(file)}`);
    continue;
  }
  const count = lineCount(read(file));
  const limit = file === rootGuide ? 100 : 150;
  console.log(`LINES ${relative(file)}: ${count}/${limit}`);
  if (count > limit) errors.push(`Line limit exceeded: ${relative(file)} (${count}/${limit})`);
}

const planFiles = markdownFiles(path.join(root, 'docs/plans'))
  .filter(file => !['README.md', 'AGETNS.md', '_template.md'].includes(path.basename(file)));
const ids = new Set();
const activePlans = [];
const planMetadata = new Map();
for (const file of planFiles) {
  const name = path.basename(file);
  if (!/^PLAN-\d{3,}-.+\.md$/.test(name)) errors.push(`Invalid PLAN filename: ${relative(file)}`);
  const frontMatter = read(file).match(/^---\r?\n([\s\S]*?)\r?\n---(?:\r?\n|$)/);
  if (!frontMatter) {
    errors.push(`Missing PLAN front matter: ${relative(file)}`);
    continue;
  }
  const fields = new Map();
  for (const line of frontMatter[1].split(/\r?\n/)) {
    if (!line.trim()) continue;
    const match = line.match(/^([a-z_]+):\s*(.*?)\s*$/);
    if (!match) {
      errors.push(`Invalid PLAN metadata line in ${relative(file)}: ${line}`);
      continue;
    }
    if (fields.has(match[1])) errors.push(`Duplicate PLAN field ${match[1]}: ${relative(file)}`);
    fields.set(match[1], match[2]);
  }
  for (const key of ['id', 'type', 'status', 'title', 'updated']) {
    if (!fields.get(key)) errors.push(`Missing PLAN field ${key}: ${relative(file)}`);
  }
  const id = fields.get('id');
  const type = fields.get('type');
  const status = fields.get('status');
  planMetadata.set(file, { id, type, status });
  if (!/^PLAN-\d{3,}$/.test(id ?? '') || !name.startsWith(`${id}-`)) {
    errors.push(`PLAN ID and filename mismatch: ${relative(file)}`);
  }
  if (ids.has(id)) errors.push(`Duplicate PLAN ID: ${id}`);
  ids.add(id);
  if (!allowedTypes.has(type)) errors.push(`Invalid PLAN type ${JSON.stringify(type)}: ${relative(file)}`);
  if (!allowedStates.has(status)) errors.push(`Invalid PLAN status ${JSON.stringify(status)}: ${relative(file)}`);
  if (status === 'active') activePlans.push(file);
}
if (activePlans.length !== 1) errors.push(`Expected exactly 1 active PLAN; found ${activePlans.length}`);
console.log(`PLANS total=${planFiles.length}, active=${activePlans.length}`);
for (const file of activePlans) console.log(`ACTIVE ${relative(file)}`);

// README summarizes type/status; each PLAN's front matter remains authoritative.
const planIndex = path.join(root, 'docs/plans/README.md');
const indexedPlans = new Set();
if (!fs.existsSync(planIndex)) {
  errors.push('Missing PLAN index: docs/plans/README.md');
} else {
  for (const line of read(planIndex).split(/\r?\n/)) {
    if (!line.trim().startsWith('|')) continue;
    const columns = line.split('|').slice(1, -1).map(value => value.trim());
    const link = columns[0]?.match(/^\[(PLAN-\d{3,})\]\(([^)]+)\)$/);
    if (!link) continue;
    const file = path.resolve(path.dirname(planIndex), link[2]);
    const metadata = planMetadata.get(file);
    const summaryType = columns[2]?.replace(/^`|`$/g, '');
    const summaryStatus = columns[3]?.replace(/^`|`$/g, '');
    if (!metadata || metadata.id !== link[1]) errors.push(`Unknown PLAN index entry: ${link[1]}`);
    if (indexedPlans.has(file)) errors.push(`Duplicate PLAN index entry: ${link[1]}`);
    if (metadata && summaryType !== metadata.type) errors.push(`PLAN index type mismatch: ${link[1]}`);
    if (metadata && summaryStatus !== metadata.status) errors.push(`PLAN index status mismatch: ${link[1]}`);
    indexedPlans.add(file);
  }
  for (const file of planFiles) {
    if (!indexedPlans.has(file)) errors.push(`PLAN missing from README: ${relative(file)}`);
  }
}
console.log(`PLAN_INDEX entries=${indexedPlans.size}`);

// Validate local inline Markdown link targets; heading fragments and web URLs
// are deliberately outside this check. It does not execute Unity or tests.
let checkedLinks = 0;
const documents = [...new Set([rootGuide, ...markdownFiles(path.join(root, 'docs'))])];
for (const file of documents) {
  if (!fs.existsSync(file)) continue;
  for (const match of read(file).matchAll(/!?\[[^\]\n]*\]\((<[^>]+>|[^)\n]+)\)/g)) {
    let target = match[1].trim().replace(/^<|>$/g, '');
    if (/^[a-z][a-z\d+.-]*:/i.test(target) || target.startsWith('#')) continue;
    target = target.split('#')[0];
    if (!target) continue;
    try {
      const resolved = path.resolve(path.dirname(file), decodeURIComponent(target));
      checkedLinks++;
      if (!fs.existsSync(resolved)) errors.push(`Broken local link in ${relative(file)}: ${target}`);
    } catch {
      errors.push(`Invalid local link in ${relative(file)}: ${target}`);
    }
  }
}
console.log(`LOCAL_LINKS checked=${checkedLinks} (file targets only)`);
if (errors.length) {
  for (const error of errors) console.error(`ERROR ${error}`);
  process.exitCode = 1;
} else {
  console.log('PASS agent guidance, PLAN type/status metadata/index, single active PLAN, local file links');
}
