import { fibDir } from "../vendor/thinking-orbs/core.js";

const PLANE_ROWS = 47;
export const PARTICLE_COUNT = 3 * PLANE_ROWS * (PLANE_ROWS + 1) / 2;
export const HOLD_SECONDS = 4.4;
export const MORPH_SECONDS = 2.8;

export function easeMorph(t) {
  const x = Math.max(0, Math.min(1, t));

  return x * x * x * (x * (x * 6 - 15) + 10);
}

function pathPoint(vertices, fraction) {
  const lengths = vertices.map((a, i) => {
    const b = vertices[(i + 1) % vertices.length];
    return Math.hypot(...a.map((value, axis) => b[axis] - value));
  });
  let distance = fraction * lengths.reduce((sum, length) => sum + length, 0);
  let segment = 0;
  while (segment < lengths.length - 1 && distance > lengths[segment])
    distance -= lengths[segment++];
  const a = vertices[segment],
    b = vertices[(segment + 1) % vertices.length];
  return a.map(
    (value, axis) => value + ((b[axis] - value) * distance) / lengths[segment],
  );
}

const PAGE_COLUMNS = 34;
const PAGE_ROWS = 44;
const PAGE_SIZE = PAGE_COLUMNS * PAGE_ROWS;
const SPINE_ROWS = 70;
const SPINE_END = 2 * (PAGE_SIZE + SPINE_ROWS);
const PAGE_EDGE_SIZE = (PARTICLE_COUNT - SPINE_END) / 2;

function notebook(i) {
  if (i < 2 * PAGE_SIZE) {
    const side = i < PAGE_SIZE ? -1 : 1;
    const local = i % PAGE_SIZE;
    const u = (local % PAGE_COLUMNS) / (PAGE_COLUMNS - 1);
    const v = Math.floor(local / PAGE_COLUMNS) / (PAGE_ROWS - 1);
    return [
      side * (0.035 + u * 0.95),
      (0.5 - v) * 1.42 + Math.sin((u * Math.PI) / 2) * 0.18,
      0.23 * u * u,
    ];
  }
  if (i < SPINE_END) {
    const side = i % 2 ? 1 : -1;
    return [
      side * 0.018,
      0.71 - (Math.floor((i - 2 * PAGE_SIZE) / 2) / (SPINE_ROWS - 1)) * 1.42,
      -0.025,
    ];
  }
  const edge = i - SPINE_END;
  const side = edge < PAGE_EDGE_SIZE ? -1 : 1;
  return pathPoint(
    [
      [side * 0.025, 0.74, -0.02],
      [side * 1.02, 0.92, 0.23],
      [side * 1.02, -0.55, 0.23],
      [side * 0.025, -0.75, -0.02],
    ],
    (edge % PAGE_EDGE_SIZE) / PAGE_EDGE_SIZE,
  );
}

const NIB_OUTLINE = [
  [-0.4, 0.88, 0],
  [0.4, 0.88, 0],
  [0.55, 0.22, 0],
  [0, -1.0, 0.14],
  [-0.55, 0.22, 0],
];

const NIB_COLUMNS = 48;
const NIB_ROWS = 60;
const NIB_BODY_SIZE = NIB_COLUMNS * NIB_ROWS;
const NIB_HOLE_SIZE = 96;
const NIB_SLIT_SIZE = 144;
const NIB_HOLE_END = NIB_BODY_SIZE + NIB_HOLE_SIZE;
const NIB_SLIT_END = NIB_HOLE_END + NIB_SLIT_SIZE;

function nib(i) {
  if (i < NIB_BODY_SIZE) {
    const u = (i % NIB_COLUMNS) / (NIB_COLUMNS - 1);
    const v = Math.floor(i / NIB_COLUMNS) / (NIB_ROWS - 1);
    const y = 0.88 - v * 1.88;
    const width =
      y > 0.22
        ? 0.4 + ((0.88 - y) / 0.66) * 0.15
        : Math.max(0.008, ((y + 1) / 1.22) * 0.55);
    let x = (u * 2 - 1) * width;
    // Open a breather hole and a slit in the metal instead of drawing over it.
    const hole =
      Math.abs(y - 0.24) < 0.115 ? Math.sqrt(0.115 ** 2 - (y - 0.24) ** 2) : 0;
    const gap = Math.min(
      width * 0.95,
      y < 0.24 ? Math.max(hole, Math.min(0.014, width * 0.25)) : hole,
    );
    if (Math.abs(x) < gap) x = (u < 0.5 ? -1 : 1) * gap;
    return [x, y, 0.2 * (1 - (x / width) ** 2)];
  }
  if (i < NIB_HOLE_END) {
    const angle = ((i - NIB_BODY_SIZE) / NIB_HOLE_SIZE) * Math.PI * 2;
    return [Math.cos(angle) * 0.115, 0.24 + Math.sin(angle) * 0.115, 0.21];
  }
  if (i < NIB_SLIT_END) {
    const row = Math.floor((i - NIB_HOLE_END) / 2);
    return [(i % 2 ? 1 : -1) * 0.014, 0.12 - (row / (NIB_SLIT_SIZE / 2 - 1)) * 1.04, 0.205];
  }
  return pathPoint(NIB_OUTLINE, (i - NIB_SLIT_END) / (PARTICLE_COUNT - NIB_SLIT_END));
}

function plane() {
  const tip = [1.06, 0.79, 0],
    fold = [-0.19, -0.17, 0.29];
  const triangles = [
    [[-1.02, 0.18, -0.12], tip, fold],
    [fold, tip, [0.16, -0.92, -0.08]],
    [fold, [0.16, -0.92, -0.08], [-0.39, -0.58, -0.11]],
  ];
  const points = [];
  for (const [a, b, c] of triangles) {
    for (let row = 0; row < PLANE_ROWS; row++)
      for (let column = 0; column <= row; column++) {
        const v = row / (PLANE_ROWS - 1);
        const u = column / (PLANE_ROWS - 1);
        points.push(
          a.map(
            (value, axis) => value * (1 - v) + b[axis] * (v - u) + c[axis] * u,
          ),
        );
      }
  }
  return points;
}

function star(i) {
  const [x, y, z] = fibDir(i, PARTICLE_COUNT);
  const theta = Math.atan2(y, x);
  const step = Math.PI / 5;
  const angle =
    (((theta - Math.PI / 2) % (Math.PI * 2)) + Math.PI * 2) % (Math.PI * 2);
  const sector = Math.min(9, Math.floor(angle / step));
  const a = Math.PI / 2 + sector * step,
    b = a + step;
  const ra = sector % 2 ? 0.48 : 1.06,
    rb = sector % 2 ? 1.06 : 0.48;
  const ax = Math.cos(a) * ra,
    ay = Math.sin(a) * ra;
  const dx = Math.cos(b) * rb - ax,
    dy = Math.sin(b) * rb - ay;
  const radius =
    (ax * dy - ay * dx) / (Math.cos(theta) * dy - Math.sin(theta) * dx);
  return [x * radius, y * radius, z * 0.22];
}

export function createShapes() {
  const points = Array.from({ length: PARTICLE_COUNT }, (_, i) => i);
  return [points.map(notebook), points.map(nib), plane(), points.map(star)].map(
    (shape) => Float32Array.from(shape.flat()),
  );
}

/** Write into a reusable buffer: no per-particle arrays during interpolation. */
export function blendShape(source, destination, progress, output) {
  if (progress <= 0) {
    output.set(source);
    return;
  }
  if (progress >= 1) {
    output.set(destination);
    return;
  }
  const amount = easeMorph(progress);
  for (let i = 0; i < output.length; i++)
    output[i] = source[i] + (destination[i] - source[i]) * amount;
}
