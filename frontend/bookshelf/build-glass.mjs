import { build } from 'esbuild';
import path from 'node:path';

// Share the storefront's existing pinned React and esbuild installation.
const here = import.meta.dirname;
await build({
  absWorkingDir: here,
  entryPoints: ['../liquid-glass/navigation.tsx'],
  outfile: '../../src/MiloShop.Web/wwwroot/vendor/liquid-glass/navigation.js',
  nodePaths: [path.join(here, 'node_modules')],
  bundle: true,
  minify: true,
  jsx: 'automatic',
  format: 'esm',
  target: 'es2020',
  define: { 'process.env.NODE_ENV': '"production"' },
  legalComments: 'linked',
});
