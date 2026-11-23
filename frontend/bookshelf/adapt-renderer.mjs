// Keep the licensed upstream snapshot byte-exact; adapt its content at build time.
// Geometry, materials, carousel, camera, cover/page physics and disposal remain upstream.
import { parse } from 'acorn';

export function adaptRenderer(source, dataModule, artModule) {
  const ast = parse(source, { ecmaVersion: 'latest', sourceType: 'module' });
  const renderer = ast.body.find(
    (node) =>
      node.type === 'ExportNamedDeclaration' &&
      node.declaration?.id?.name === 'createBookshelfRenderer',
  )?.declaration;
  if (!renderer) throw new Error('Registered renderer entry was not found.');
  const edits = [];
  const statements = renderer.body.body;
  const variable = (name) =>
    statements.find(
      (node) =>
        node.type === 'VariableDeclaration' &&
        node.declarations.some((value) => value.id.name === name),
    );
  const fn = (name) =>
    statements.find(
      (node) => node.type === 'FunctionDeclaration' && node.id.name === name,
    );
  const replace = (node, value) => {
    if (!node)
      throw new Error(
        'The registered source no longer matches the category adapter.',
      );
    edits.push({ start: node.start, end: node.end, value });
  };

  replace(
    variable('BOOKS'),
    `const BOOKS = createCategoryVolumes(
    JSON.parse(host.closest('[data-bookshelf-section]').querySelector('[data-bookshelf-categories]').textContent),
    document.documentElement.dataset.theme === 'dark');
  function refreshCategoryTheme() {
    const palette = categoryRoomPalette(document.documentElement.dataset.theme === 'dark');
    BOOKS.forEach(book => Object.assign(book.palette, palette));
    applyBookTheme(BOOKS[selectedIndex]); requestFrame();
  }`,
  );
  // Remove the demo atlas entirely, not just its labels. The authored walnut map stays.
  for (const name of [
    'COVER_ATLAS_DATA',
    'COVER_CROPS',
    'coverAtlasImage',
    'coverAtlasReady',
  ])
    replace(variable(name), '');
  for (const node of statements) {
    if (
      node.type === 'ExpressionStatement' &&
      node.expression.type === 'AssignmentExpression' &&
      node.expression.left.object?.name === 'coverAtlasImage'
    )
      replace(node, '');
  }
  replace(
    fn('drawMotif'),
    'function drawMotif(ctx, book, width, height) { drawCategoryMotif(ctx, book, width, height); }',
  );
  const artwork = {
    makeCoverTexture: 'makeCategoryCover',
    makeFoilTexture: 'makeCategoryFoil',
    makeSpineFoilTexture: 'makeCategorySpine',
    makeBackFoilTexture: 'makeCategoryBack',
    makeInteriorPageTextures: 'makeCategoryPages',
  };
  for (const [name, painter] of Object.entries(artwork))
    replace(
      fn(name),
      `function ${name}(book) { return ${painter}(THREE, book, configureCanvasTexture); }`,
    );
  const init = fn('initialize');
  for (const node of init.body.body) {
    const code = source.slice(node.start, node.end);
    if (
      node.type === 'TryStatement' &&
      code.includes('coverAtlasImage.decode()')
    )
      replace(node, '');
    if (node.type === 'TryStatement' && code.includes('document.fonts.load'))
      replace(
        node,
        `try { await Promise.all([document.fonts.load('400 48px MiloPersian'), document.fonts.load('400 24px Onest')]); } catch { /* system fonts remain available */ }`,
      );
  }
  const ready = init.body.body.find((node) =>
    source.slice(node.start, node.end).includes('callbacks.onReady?.()'),
  );
  if (!ready) throw new Error('Registered ready callback was not found.');
  edits.push({
    start: ready.start,
    end: ready.start,
    value: `document.addEventListener('milo:theme', refreshCategoryTheme); refreshCategoryTheme();\n`,
  });
  const dispose = fn('disposeExperience');
  edits.push({
    start: dispose.body.start + 1,
    end: dispose.body.start + 1,
    value: `\n document.removeEventListener('milo:theme', refreshCategoryTheme);\n`,
  });
  for (const edit of edits.sort((a, b) => b.start - a.start))
    source = source.slice(0, edit.start) + edit.value + source.slice(edit.end);
  source = source
    .replaceAll('WORKING VOLUMES', 'MILO / STATIONERY')
    .replaceAll('AN IMAGINED EDITION', 'MILO COLLECTION');
  return `// Milo category adaptation of the verified ThreeUI renderer.\nimport {createCategoryVolumes, categoryRoomPalette} from ${JSON.stringify(dataModule)};\nimport {drawCategoryMotif, makeCategoryCover, makeCategoryFoil, makeCategorySpine, makeCategoryBack, makeCategoryPages} from ${JSON.stringify(artModule)};\n${source}`;
}
