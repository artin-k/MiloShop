// Normalized 3D point clouds. Every shape has the same particle identities so
// transitions move real dots rather than crossfading pictures or canvases.
import { fibDir } from "../vendor/thinking-orbs/core.js";

export const PARTICLE_COUNT = 900;
export const HOLD_SECONDS = 4.4;
export const MORPH_SECONDS = 2.8;
export const CHAPTERS = [
  {
    title: "صفحه‌ای برای شروع",
    description: "فکرهای کوچک را بنویس؛ این اولین شکل یک رویای بزرگ است.",
  },
  {
    title: "قلمی برای خیال",
    description: "از یک نقطه تا هزار داستان؛ خط بعدی را خودت بکش.",
  },
  {
    title: "رویایی آماده پرواز",
    description: "به خیال‌هایت بال بده؛ کاغذ می‌تواند آغاز یک سفر باشد.",
  },
  {
    title: "ایده‌ای که می‌درخشد",
    description: "آنچه امروز تصور می‌کنی، می‌تواند فردا شکل بگیرد.",
  },
];

// Quintic easing has zero velocity AND acceleration at both ends of a morph.
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

function notebook(i) {
  if (i < 720) {
    const side = i < 360 ? -1 : 1;
    const local = i % 360,
      u = (local % 15) / 14,
      v = Math.floor(local / 15) / 23;
    return [
      side * (0.035 + u * 0.95),
      (0.5 - v) * 1.42 + Math.sin((u * Math.PI) / 2) * 0.18,
      0.23 * u * u,
    ];
  }
  if (i < 780) {
    const side = i % 2 ? 1 : -1;
    return [
      side * 0.018,
      0.71 - (Math.floor((i - 720) / 2) / 29) * 1.42,
      -0.025,
    ];
  }
  const side = i < 840 ? -1 : 1;
  return pathPoint(
    [
      [side * 0.025, 0.74, -0.02],
      [side * 1.02, 0.92, 0.23],
      [side * 1.02, -0.55, 0.23],
      [side * 0.025, -0.75, -0.02],
    ],
    ((i - 780) % 60) / 60,
  );
}

const NIB_OUTLINE = [
  [-0.4, 0.88, 0],
  [0.4, 0.88, 0],
  [0.55, 0.22, 0],
  [0, -1.0, 0.14],
  [-0.55, 0.22, 0],
];

function nib(i) {
  if (i < 700) {
    const u = (i % 25) / 24,
      v = Math.floor(i / 25) / 27;
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
  if (i < 732) {
    const angle = ((i - 700) / 32) * Math.PI * 2;
    return [Math.cos(angle) * 0.115, 0.24 + Math.sin(angle) * 0.115, 0.21];
  }
  if (i < 780)
    return [(i % 2 ? 1 : -1) * 0.014, 0.12 - ((i - 732) / 47) * 1.04, 0.205];
  return pathPoint(NIB_OUTLINE, (i - 780) / 120);
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
    // 24 rows of a triangular lattice: 300 particles per folded face.
    for (let row = 0; row < 24; row++)
      for (let column = 0; column <= row; column++) {
        const v = row / 23,
          u = column / 23;
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
