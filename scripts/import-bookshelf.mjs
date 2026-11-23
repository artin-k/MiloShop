// Preserve the complete registered ThreeUI source; fail closed on any mismatch.
import fs from 'node:fs/promises';
import path from 'node:path';
import crypto from 'node:crypto';
import { execFileSync } from 'node:child_process';

const root = path.resolve(import.meta.dirname, '..');
export const sourceUrl = 'https://threeui.com/source-code/bookshelf.json';
export const revision = '6ef16625e670b0285bb689bdebffc1d728c6deb1';
export const hashes = {
  'src/shaders/bookshelf/BookshelfScene.tsx':
    '919856487ff38d66aa49548e70e5461e587a502b1642cf9f0d819f3b70acb516',
  'src/shaders/bookshelf/bookshelfRenderer.js':
    '94c5141ecf3aa154dcb3208262dff2da20f1f85f9f3987eaa811d0d5512016a8',
  'src/shaders/bookshelf/three165/OrbitControls.js':
    'd55c9c1593dd2f89722065b00bbb8a510895a6afc8f68517b9ea88452571be16',
  'src/shaders/bookshelf/three165/RoomEnvironment.js':
    'ac34881abcacd5f728aea3c4abbcab13da3dbc9a95dc03c32432a05f87799b30',
  'src/shaders/bookshelf/three165/RoundedBoxGeometry.js':
    '453681bf9c792a83d527c78a66025f7188accdac3a36e26d74b4382a7e41e933',
  'src/shaders/bookshelf/three165/RectAreaLightUniformsLib.js':
    'a6728b0e9f5b5adae258cc0596106cd7e1a96ae8b2ec6450e80db6eb12586c59',
  'src/shaders/threeui.css':
    'efe4447139f1358dd8e9be68edf6fa46cbefbd1de423a4d6c439ca61d2c8eccf',
};
export const sha256 = (data) =>
  crypto.createHash('sha256').update(data).digest('hex');

if (process.argv[1] === import.meta.filename) {
  const input = process.argv[2];
  const bytes = input
    ? await fs.readFile(input)
    : execFileSync('curl', ['-fsSL', sourceUrl], {
        maxBuffer: 4 * 1024 * 1024,
      });
  const bundle = JSON.parse(bytes);
  if (
    bundle.id !== 'bookshelf' ||
    bundle.files.length !== Object.keys(hashes).length ||
    bundle.assets.length
  ) {
    throw new Error(
      'Unexpected Bookshelf bundle. Review the registered source before importing.',
    );
  }
  const files = bundle.files.map((file) => {
    if (
      typeof file.code !== 'string' ||
      !hashes[file.path] ||
      sha256(Buffer.from(file.code)) !== hashes[file.path]
    ) {
      throw new Error(`Registered source verification failed: ${file.path}`);
    }
    return file;
  });
  if (
    new Set(files.map((file) => file.path)).size !== Object.keys(hashes).length
  )
    throw new Error('Duplicate source path.');
  const destination = path.join(root, 'frontend/bookshelf/vendor');
  for (const file of files) {
    const target = path.join(destination, file.path);
    await fs.mkdir(path.dirname(target), { recursive: true });
    await fs.writeFile(target, file.code);
  }
  await fs.writeFile(
    path.join(destination, 'provenance.json'),
    JSON.stringify(
      {
        component: 'BookshelfScene',
        sourceUrl,
        revision,
        runtime: 'Three.js r165',
        files: files.map(({ path, sha256, bytes }) => ({
          path,
          sha256,
          bytes,
        })),
        assets:
          'Cover atlas and walnut texture are embedded in the exact renderer; no separate binary assets.',
      },
      null,
      2,
    ) + '\n',
  );
  console.log(
    `Verified and imported all ${files.length} exact Bookshelf source files.`,
  );
}
