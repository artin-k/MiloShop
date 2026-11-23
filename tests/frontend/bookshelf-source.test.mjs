import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs/promises';
import path from 'node:path';
import { hashes, sha256, revision } from '../../scripts/import-bookshelf.mjs';

const root = path.resolve(import.meta.dirname, '../..');
const vendor = path.join(root, 'frontend/bookshelf/vendor');
const output = path.join(root, 'src/MiloShop.Web/wwwroot/vendor/bookshelf');

test('all seven registered Bookshelf files retain the requested exact hashes', async () => {
  const provenance = JSON.parse(
    await fs.readFile(path.join(vendor, 'provenance.json')),
  );
  assert.equal(provenance.revision, revision);
  assert.equal(provenance.files.length, 7);
  for (const [file, expected] of Object.entries(hashes)) {
    assert.equal(
      sha256(await fs.readFile(path.join(vendor, file))),
      expected,
      file,
    );
  }
});

test('the configured component uses the isolated Three.js r165 dependency', async () => {
  const pkg = JSON.parse(
    await fs.readFile(path.join(root, 'frontend/bookshelf/package.json')),
  );
  assert.equal(pkg.dependencies.three165, 'npm:three@0.165.0');
  const entry = await fs.readFile(
    path.join(root, 'frontend/bookshelf/entry.tsx'),
    'utf8',
  );
  assert.match(
    entry,
    /import \{ BookshelfScene \} from '@designcodeio\/threeui'/,
  );
  assert.match(entry, /className="shader-frame">\s*<BookshelfScene\s*\/>/);
  assert.doesNotMatch(entry, /<iframe/);
});

test('committed lazy loader and CSS reference real generated assets', async () => {
  const loader = await fs.readFile(
    path.join(output, 'bookshelf-loader.js'),
    'utf8',
  );
  assert.doesNotMatch(loader, /__BOOKSHELF_/);
  const script = loader.match(/bookshelf-[A-Z0-9]+\.js/)[0];
  const stylesheet = loader.match(/bookshelf-[A-Z0-9]+\.css/)[0];
  assert.ok((await fs.stat(path.join(output, script))).size > 0);
  const css = await fs.readFile(path.join(output, stylesheet), 'utf8');
  assert.match(css, /\.bookshelf__canvas\.is-ready/);
  for (const [, url] of css.matchAll(/url\(([^)]+)\)/g)) {
    if (/^["']?data:/.test(url)) continue;
    assert.ok(
      (await fs.stat(path.join(output, url.replaceAll('"', '')))).size > 0,
      url,
    );
  }
});
