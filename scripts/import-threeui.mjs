// Imports the authored ThreeUI Kage document, runtime and binary assets.
// Run with Node 20+: node scripts/import-threeui.mjs
import fs from 'node:fs/promises';
import path from 'node:path';
import crypto from 'node:crypto';
import { execFile } from 'node:child_process';
import { promisify } from 'node:util';

const root = path.resolve(import.meta.dirname, '..');
const base = 'https://threeui.com/';
const run = promisify(execFile);
const digest = value => crypto.createHash('sha256').update(value).digest('hex');
const write = async (name, content) => {
  const file = path.join(root, name);
  await fs.mkdir(path.dirname(file), { recursive: true });
  await fs.writeFile(file, content);
};
const get = async url => {
  const { stdout } = await run('curl', ['--fail', '--location', '--silent', '--show-error', String(url)],
    { encoding: 'buffer', maxBuffer: 8 * 1024 * 1024 });
  return stdout;
};
const bundle = JSON.parse(await get(base + 'source-code/kage-landing-page.json'));
const manifest = { component: 'KageLandingPage', source: base + 'landing-pages/kage.html', files: [] };
const texts = {};
for (const file of bundle.files) {
  const data = file.code !== undefined ? Buffer.from(file.code) : await get(new URL(file.sourceUrl, base));
  const hash = digest(data);
  // The public HTML/font responses rewrite local media paths to storage URLs.
  // Preserve that delivered source and record both hashes rather than concealing the difference.
  manifest.files.push({ path: file.path, registeredSha256: file.sha256, deliveredSha256: hash });
  texts[file.path] = data.toString('utf8');
}
let html = texts['public/landing-pages/kage.html'];
for (const asset of bundle.assets) {
  const relative = asset.path.replace(/^public\/landing-pages\//, '');
  const data = await get(base + 'landing-pages/' + relative);
  if (digest(data) !== asset.sha256) throw new Error(`Asset hash mismatch: ${relative}`);
  await write('src/MiloShop.Web/wwwroot/vendor/threeui/' + relative, data);
  manifest.files.push({ path: relative, sha256: asset.sha256 });
  const name = path.basename(relative);
  html = html.replaceAll(new RegExp('https://[^\\s\'"()]+/' + name.replaceAll('.', '\\.'), 'g'), '/vendor/threeui/' + relative);
}
let fonts = texts['public/landing-pages/secret-pathways-assets/fonts.css'].replaceAll('font-display:block', 'font-display:swap');
for (const url of new Set(fonts.match(/https:[^)]+/g))) {
  const name = path.basename(url);
  const data = await get(url);
  await write('src/MiloShop.Web/wwwroot/vendor/threeui/secret-pathways-assets/fonts/' + name, data);
  fonts = fonts.replaceAll(url, 'fonts/' + name);
}
await write('vendor/threeui/kage.original.html', texts['public/landing-pages/kage.html']);
await write('vendor/threeui/provenance.json', JSON.stringify(manifest, null, 2) + '\n');
await write('src/MiloShop.Web/wwwroot/vendor/threeui/secret-pathways-assets/fonts.css', fonts);
await write('src/MiloShop.Web/wwwroot/vendor/threeui/secret-pathways-assets/three.min.js', texts['public/landing-pages/secret-pathways-assets/three.min.js']);
await write('src/MiloShop.Web/wwwroot/vendor/threeui/kage.css', html.match(/<style>([\s\S]*?)<\/style>/)[1]);
let scene = html.match(/<script>\s*(\/\* =+[\s\S]*?)<\/script>/)[1];
scene = scene.replaceAll('new __threeuiStorageImage()', 'new Image()')
  .replace("const word = 'KAGE'", "const word = 'MILOSHOP'")
  .replace("const names = ['The Hidden Gate', 'The Sanmon', 'Still Gardens', 'Sacred Craft', 'Afterlight', 'Colophon'];", "const names = ['میلوشاپ', 'داستان میلو', 'مجموعه‌ها', 'محصولات', 'شروع خرید', 'ارتباط'];")
  .replace("SECS.indexOf(document.querySelector(l.getAttribute('href')))", "SECS.indexOf(l.hash ? document.querySelector(l.hash) : null)");
await write('src/MiloShop.Web/wwwroot/vendor/threeui/kage.js', scene);
console.log(`Imported ${bundle.assets.length} verified assets; source provenance saved.`);
