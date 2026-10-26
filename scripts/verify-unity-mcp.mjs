import { spawn } from 'node:child_process';
import { createInterface } from 'node:readline';
import { mkdir, readFile, writeFile } from 'node:fs/promises';
import path from 'node:path';

// PLAN-002 environment check: stdio discovery and an optional explicitly supplied tool call.
// No LLM turn is started, and no Codex configuration is changed by this script.
const [mode, executable, projectPath, outputPath, callFile] = process.argv.slice(2);
if (!['codex', 'unity'].includes(mode) || !executable || !projectPath || !outputPath) {
  throw new Error('Usage: node scripts/verify-unity-mcp.mjs codex|unity <exe> <project> <output.json> [call.json]');
}
const args = mode === 'codex' ? ['app-server', '--stdio'] :
  ['mcp', '--project-path', path.resolve(projectPath), '--no-banner', '--non-interactive'];
const child = spawn(executable, args, { windowsHide: true, stdio: ['pipe', 'pipe', 'pipe'] });
const pending = new Map();
let nextId = 0;
const evidence = { startedAt: new Date().toISOString(), mode, executable, args, projectPath: path.resolve(projectPath), checksPassed: false };
// Diagnostics are intentionally not persisted wholesale: external servers can log credentials.
child.stderr.on('data', () => {});
const lines = createInterface({ input: child.stdout });
lines.on('line', line => {
  let message;
  try { message = JSON.parse(line); } catch { return; }
  const waiter = pending.get(message.id);
  if (waiter) {
    pending.delete(message.id);
    clearTimeout(waiter.timer);
    message.error ? waiter.reject(new Error(JSON.stringify(message.error))) : waiter.resolve(message.result);
  }
});
function rejectPending(error) {
  for (const waiter of pending.values()) { clearTimeout(waiter.timer); waiter.reject(error); }
  pending.clear();
}
child.on('error', rejectPending);
child.on('exit', code => rejectPending(new Error(`Server exited (${code})`)));
function send(method, params, notification = false) {
  if (notification) {
    child.stdin.write(JSON.stringify({ jsonrpc: '2.0', method, params }) + '\n');
    return;
  }
  const id = ++nextId;
  return new Promise((resolve, reject) => {
    const timer = setTimeout(() => { pending.delete(id); reject(new Error(`Timed out: ${method}`)); }, 90000);
    pending.set(id, { resolve, reject, timer });
    child.stdin.write(JSON.stringify({ jsonrpc: '2.0', id, method, params }) + '\n');
  });
}
try {
  if (mode === 'codex') {
    evidence.initialize = await send('initialize', {
      clientInfo: { name: 'plan002_environment_check', version: '1.0.0' },
      capabilities: { experimentalApi: true }
    });
    send('initialized', {}, true);
    evidence.discovery = await send('mcpServerStatus/list', {
      serverName: 'unity-mobile-game', detail: 'toolsAndAuthOnly'
    });
    const servers = evidence.discovery.data ?? [];
    evidence.checksPassed = servers.some(server => server.name === 'unity-mobile-game' && !server.toolsError && Object.keys(server.tools ?? {}).length > 0);
    console.log(JSON.stringify({ mode, checksPassed: evidence.checksPassed, servers: servers.map(server => ({ name: server.name, serverInfo: server.serverInfo, toolCount: Object.keys(server.tools ?? {}).length, toolsError: server.toolsError })) }, null, 2));
  } else {
    evidence.initialize = await send('initialize', {
      protocolVersion: '2025-03-26', capabilities: {},
      clientInfo: { name: 'plan002_environment_check', version: '1.0.0' }
    });
    send('notifications/initialized', {}, true);
    evidence.discovery = await send('tools/list', {});
    if (callFile) {
      const call = JSON.parse(await readFile(callFile, 'utf8'));
      if (!evidence.discovery.tools.some(tool => tool.name === call.name)) throw new Error('Tool was not discovered');
      evidence.toolCall = call;
      evidence.toolResult = await send('tools/call', call);
      const payloads = (evidence.toolResult.content ?? []).filter(item => item.type === 'text').map(item => {
        try { return JSON.parse(item.text); } catch { return null; }
      });
      evidence.checksPassed = evidence.toolResult.isError !== true && !payloads.some(payload => payload?.success === false);
      console.log(JSON.stringify({ mode, checksPassed: evidence.checksPassed, toolResult: evidence.toolResult }, null, 2));
    } else {
      evidence.checksPassed = evidence.discovery.tools.length > 0;
      console.log(JSON.stringify({ mode, checksPassed: evidence.checksPassed, serverInfo: evidence.initialize.serverInfo, toolCount: evidence.discovery.tools.length, tools: evidence.discovery.tools.map(tool => ({ name: tool.name, description: tool.description })) }, null, 2));
    }
  }
} catch (error) {
  evidence.error = error.message;
  console.error(error.message);
} finally {
  evidence.finishedAt = new Date().toISOString();
  await mkdir(path.dirname(path.resolve(outputPath)), { recursive: true });
  await writeFile(outputPath, JSON.stringify(evidence, null, 2) + '\n');
  lines.close();
  child.stdin.end();
  child.kill();
}
if (!evidence.checksPassed) process.exitCode = 1;
