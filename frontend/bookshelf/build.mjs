import { build } from 'esbuild';
import fs from 'node:fs/promises';
import path from 'node:path';
import { hashes, sha256 } from '../../scripts/import-bookshelf.mjs';

const here = import.meta.dirname;
const vendor = path.join(here, 'vendor');
for (const [file, expected] of Object.entries(hashes)) {
  if (sha256(await fs.readFile(path.join(vendor, file))) !== expected)
    throw new Error(`Exact source changed: ${file}`);
}
const outdir = path.resolve(
  here,
  '../../src/MiloShop.Web/wwwroot/vendor/bookshelf',
);
const result = await build({
  absWorkingDir: here,
  entryPoints: { bookshelf: 'entry.tsx' },
  outdir,
  entryNames: '[name]-[hash]',
  bundle: true,
  format: 'esm',
  minify: true,
  target: 'es2020',
  jsx: 'automatic',
  metafile: true,
  loader: { '.woff2': 'file' },
  assetNames: 'fonts/[name]-[hash]',
  define: { 'process.env.NODE_ENV': '"production"' },
  alias: {
    '@designcodeio/threeui/style.css': path.join(
      vendor,
      'src/shaders/threeui.css',
    ),
    '@designcodeio/threeui': path.join(
      vendor,
      'src/shaders/bookshelf/BookshelfScene.tsx',
    ),
  },
  legalComments: 'linked',
  logLevel: 'info',
});
const output = Object.entries(result.metafile.outputs).find(
  ([, meta]) => meta.entryPoint,
);
const css = output[1].cssBundle;
if (!css) throw new Error('The authored ThreeUI styles were not emitted.');
const loader = (await fs.readFile(path.join(here, 'loader.js'), 'utf8'))
  .replaceAll('__BOOKSHELF_ENTRY__', path.basename(output[0]))
  .replaceAll('__BOOKSHELF_STYLES__', path.basename(css));
await fs.writeFile(path.join(outdir, 'bookshelf-loader.js'), loader);
await fs.mkdir(path.join(outdir, 'licenses'), { recursive: true });
for (const name of ['react', 'react-dom', 'three165']) {
  await fs.copyFile(
    path.join(here, 'node_modules', name, 'LICENSE'),
    path.join(outdir, 'licenses', `${name}.txt`),
  );
}
await fs.copyFile(
  path.join(vendor, 'LICENSE'),
  path.join(outdir, 'licenses/threeui.txt'),
);
await fs.copyFile(
  path.join(vendor, 'src/shaders/fonts/OFL.txt'),
  path.join(outdir, 'licenses/fragment-mono.txt'),
);
console.log(
  'Exact Bookshelf built; generated assets are committed for normal dotnet builds.',
);
