import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs/promises';
import {
  createCategoryVolumes,
  categoryRoomPalette,
} from '../../frontend/bookshelf/category-data.js';
import { adaptRenderer } from '../../frontend/bookshelf/adapt-renderer.mjs';
import { parse } from '../../frontend/bookshelf/node_modules/acorn/dist/acorn.mjs';

const categories = [
  {
    id: 12,
    title: 'دفتر و کاغذ',
    description: 'برای فکرهای تازه',
    productCount: 4,
  },
  { id: 31, title: 'قلم و خودکار', description: '', productCount: 0 },
  { id: 42, title: 'ابزار نقاشی', description: 'رنگ ایده‌ها', productCount: 7 },
];

test('stationery volumes preserve real category identifiers, names and counts', () => {
  const books = createCategoryVolumes(categories);
  assert.deepEqual(
    books.map((book) => book.categoryId),
    [12, 31, 42],
  );
  assert.deepEqual(
    books.map((book) => book.title),
    categories.map((category) => category.title),
  );
  assert.deepEqual(
    books.map((book) => book.motifKey),
    ['notebook', 'writing', 'art'],
  );
  assert.equal(books[0].deck, categories[0].description);
  assert.equal(books[1].productCount, 0);
  assert.ok(books[1].deck.length > 0);
});

test('the category collection handles one, many and renamed categories', () => {
  assert.equal(createCategoryVolumes(categories.slice(0, 1)).length, 1);
  const many = Array.from({ length: 11 }, (_, i) => ({
    id: i + 100,
    title: `دسته ${i}`,
    productCount: i,
  }));
  assert.equal(createCategoryVolumes(many).length, 11);
  const renamed = createCategoryVolumes([
    { ...categories[0], title: 'دفترهای میلو' },
  ])[0];
  assert.equal(renamed.color, createCategoryVolumes(categories)[0].color);
  assert.equal(renamed.categoryId, 12);
  assert.equal(renamed.title, 'دفترهای میلو');
  assert.throws(() => createCategoryVolumes([]));
  assert.throws(() => createCategoryVolumes([categories[0], categories[0]]));
});

test('paper and studio room themes change lighting without changing the covers', () => {
  const day = createCategoryVolumes(categories),
    night = createCategoryVolumes(categories, true);
  assert.notEqual(categoryRoomPalette().wall, categoryRoomPalette(true).wall);
  assert.deepEqual(
    day.map((book) => book.color),
    night.map((book) => book.color),
  );
  assert.equal(night[0].palette.wall, categoryRoomPalette(true).wall);
});

test('the build adaptation replaces the demo atlas and content while retaining the interaction engine', async () => {
  const source = await fs.readFile(
    new URL(
      '../../frontend/bookshelf/vendor/src/shaders/bookshelf/bookshelfRenderer.js',
      import.meta.url,
    ),
    'utf8',
  );
  const adapted = adaptRenderer(
    source,
    './category-data.js',
    './category-art.js',
  );
  parse(adapted, { sourceType: 'module', ecmaVersion: 'latest' });
  assert.doesNotMatch(adapted, /COVER_ATLAS_DATA|COVER_CROPS|coverAtlasImage/);
  assert.match(adapted, /const BOOKS = createCategoryVolumes/);
  assert.match(
    adapted,
    /makeCategoryPages\(THREE, book, configureCanvasTexture\)/,
  );
  for (const name of [
    'createBookRig',
    'updateFlexiblePage',
    'onPagePointerDown',
    'onCanvasClick',
    'onWheel',
    'closeDetail',
  ])
    assert.match(adapted, new RegExp(`function ${name}\\(`));
  assert.match(
    adapted,
    /removeEventListener\('milo:theme', refreshCategoryTheme\)/,
  );
  assert.ok(adapted.length < source.length / 2);
});
