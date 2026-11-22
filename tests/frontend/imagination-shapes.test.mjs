import test from 'node:test';
import assert from 'node:assert/strict';
import { createShapes, PARTICLE_COUNT, blendShape, easeMorph } from '../../src/MiloShop.Web/wwwroot/js/imagination-shapes.js';

test('stationery forms share stable, bounded particle identities', () => {
  const shapes = createShapes();
  assert.equal(shapes.length, 4);
  for (const shape of shapes) {
    assert.equal(shape.length, PARTICLE_COUNT * 3);
    assert.ok(shape.every(value => Number.isFinite(value) && Math.abs(value) < 1.5));
  }
  const repeated = createShapes();
  for (let i = 0; i < shapes.length; i++) assert.deepEqual(shapes[i], repeated[i]);
});

test('morphs reach exact endpoints, including the last-to-first loop', () => {
  const shapes = createShapes();
  const output = new Float32Array(PARTICLE_COUNT * 3);
  for (let i = 0; i < shapes.length; i++) {
    const next = shapes[(i + 1) % shapes.length];
    blendShape(shapes[i], next, 0, output);
    assert.deepEqual(output, shapes[i]);
    blendShape(shapes[i], next, 1, output);
    assert.deepEqual(output, next);
    blendShape(shapes[i], next, 0.5, output);
    for (let j = 0; j < output.length; j++) assert.ok(Math.abs(output[j] - (shapes[i][j] + next[j]) / 2) < 1e-6);
  }
});

test('interrupted morphs continue from the visible particle positions', () => {
  const shapes = createShapes();
  const visible = shapes[0].slice();
  blendShape(shapes[0], shapes[1], 0.4, visible);
  const source = visible.slice();
  blendShape(source, shapes[2], 0, visible);
  assert.deepEqual(visible, source);
  blendShape(source, shapes[2], 1, visible);
  assert.deepEqual(visible, shapes[2]);
  assert.ok(easeMorph(0.001) < 1e-7);
  assert.ok(1 - easeMorph(0.999) < 1e-7);
});
