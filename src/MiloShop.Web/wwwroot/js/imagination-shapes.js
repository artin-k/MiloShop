// Normalized 3D point clouds with the same particle budget for every form.
export const PARTICLE_COUNT = 3384;
export const HOLD_SECONDS = 2.8;
export const MORPH_SECONDS = 2.2;
const TURN = Math.PI * 2;

export function easeMorph(t) {
  const x = Math.max(0, Math.min(1, t));
  return x * x * x * (x * (x * 6 - 15) + 10);
}

const mix = (a, b, t) => a.map((value, axis) => value + (b[axis] - value) * t);
const fract = (value) => value - Math.floor(value);

function panel(a, b, c, density = 1) {
  const [x, y, z] = b.map((value, axis) => value - a[axis]);
  const [u, v, w] = c.map((value, axis) => value - a[axis]);
  return {
    weight: density * Math.hypot(y * w - z * v, z * u - x * w, x * v - y * u) / 2,
    point: (_, i) => {
      // Distribute dots over a square, then reflect its upper half into the triangle.
      let s = fract(0.5 + i * 0.754877666);
      let t = fract(0.5 + i * 0.569840291);
      if (s + t > 1) {
        s = 1 - s;
        t = 1 - t;
      }
      return a.map((value, axis) => value * (1 - s - t) + b[axis] * s + c[axis] * t);
    },
  };
}

function fold(a, b, density = 0.085) {
  return {
    weight: density * Math.hypot(...a.map((value, axis) => b[axis] - value)),
    point: (t) => mix(a, b, t),
  };
}

function outline(vertices, density) {
  return vertices.map((a, i) => fold(a, vertices[(i + 1) % vertices.length], density));
}

function ellipse(x, y, rx, ry, z, weight, depth = 0) {
  return {
    weight,
    point: (t) => {
      const angle = t * TURN;
      return [x + Math.cos(angle) * rx, y + Math.sin(angle) * ry, z + Math.sin(angle) * depth];
    },
  };
}

function origamiBird() {
  const tail = [-1.14, 0.35, -0.09];
  const back = [-0.51, 0.18, -0.02];
  const peak = [-0.35, 1.07, -0.38];
  const crest = [0.22, 0.23, 0.28];
  const chest = [0.49, -0.07, 0.05];
  const belly = [-0.34, -0.15, 0];
  const wing = [-0.81, -0.75, 0.27];
  const wingTip = [-0.40, -0.82, 0.41];
  const head = [0.68, 0.70, 0.06];
  const chin = [0.77, 0.49, 0.15];
  const beak = [1.1, 0.49, 0];
  return [
    panel(tail, back, belly, 1.25), panel(peak, crest, back, 0.95),
    panel(back, belly, crest), panel(crest, belly, chest, 0.8),
    panel(belly, wing, wingTip, 0.84), panel(wingTip, crest, belly, 1.15),
    panel(crest, head, chest, 1.3), panel(chest, head, chin),
    panel(head, beak, chin, 1.5),
    ...outline([tail, back, peak, crest, head, beak, chin, chest, crest, wingTip, wing, belly]),
    fold(back, crest), fold(belly, crest), fold(chin, crest),
  ];
}

function readingLamp() {
  const foot = [-0.36, -0.82, 0.12];
  const elbow = [-0.69, 0.06, 0.12];
  const mount = [0.18, 0.54, 0.08];
  const shade = [[0.12, 0.64, 0.05], [0.47, 0.77, 0.07], [0.99, 0.20, 0.15], [0.19, -0.09, 0.18]];
  const [a, b, c, d] = shade;
  const crown = [0.47, 0.36, 0.38];
  const [lx, ly] = d;
  return [
    ellipse(foot[0], -0.76, 0.52, 0.085, 0.05, 0.24, 0.28),
    ellipse(foot[0], -0.87, 0.52, 0.074, 0.05, 0.22, 0.28),
    fold([-0.88, -0.76, 0.05], [-0.88, -0.87, 0.05]),
    fold([0.16, -0.76, 0.05], [0.16, -0.87, 0.05]),
    fold(foot, elbow, 0.21), fold([-0.48, -0.76, 0.08], [-0.82, 0.08, 0.08], 0.19),
    fold(elbow, mount, 0.21), fold([-0.70, 0.2, 0.08], [0.10, 0.67, 0.08], 0.19),
    ellipse(-0.75, 0.13, 0.1, 0.1, 0.13, 0.09),
    panel(a, b, crown), panel(a, crown, d), panel(b, c, crown), panel(crown, c, d, 0.7),
    ...outline(shade, 0.15),
    ellipse((lx + c[0]) / 2, (ly + c[1]) / 2, 0.36, 0.07, 0.34, 0.12, 0.12),
    fold([0.34, -0.25, 0.08], [0.27, -0.50, 0.13], 0.075),
    fold([0.63, -0.15, 0.08], [0.75, -0.51, 0.13], 0.075),
    fold([0.87, -0.04, 0.08], [1.10, -0.23, 0.13], 0.075),
  ];
}

function paperPlane() {
  const tip = [1.07, 0.78, 0.04];
  const corner = [-1.06, 0.19, -0.10];
  const ridge = [-0.18, -0.12, 0.35];
  const lower = [0.16, -0.95, -0.10];
  const tuck = [-0.37, -0.53, -0.13];
  return [panel(corner, ridge, tip), panel(ridge, tip, lower, 0.85), panel(ridge, lower, tuck),
    ...outline([corner, tip, lower, tuck, ridge]), fold(ridge, tip, 0.16)];
}

function star() {
  const vertices = Array.from({ length: 10 }, (_, i) => {
    const angle = Math.PI / 2 + i * Math.PI / 5;
    const radius = i % 2 ? 0.48 : 1.08;
    return [radius * Math.cos(angle), radius * Math.sin(angle), i % 2 ? 0.03 : -0.08];
  });
  const center = [0, 0, 0.4];
  return [
    ...vertices.map((a, i) => panel(a, vertices[(i + 1) % 10], center, i % 2 ? 0.6 : 1.45)),
    ...outline(vertices, 0.12),
  ];
}

function paperBoat() {
  const mast = [0.04, 0.89, -0.16];
  const bow = [1.1, 0.03, 0.09];
  const stern = [-1.1, 0.15, 0.03];
  const keelLeft = [-0.54, -0.61, 0.17];
  const keelRight = [0.54, -0.61, 0.2];
  const center = [-0.02, -0.18, 0.42];
  const crease = [0.02, 0.08, -0.38];
  return [
    panel(mast, [-0.53, -0.04, 0.03], crease), panel(mast, crease, [0.58, -0.11, 0.04], 0.8),
    panel(stern, crease, center, 0.75), panel(crease, bow, center, 0.7),
    panel(stern, center, keelLeft), panel(center, keelLeft, keelRight, 1.3),
    panel(center, bow, keelRight),
    ...outline([stern, crease, bow, keelRight, keelLeft]), fold(stern, center), fold(center, bow),
    fold(crease, mast, 0.1),
  ];
}

function gift() {
  const top = [-0.04, 0.55, 0.28];
  const back = [0.18, 0.67, -0.48];
  const left = [-0.81, 0.35, -0.12];
  const right = [0.82, 0.36, -0.11];
  const front = [-0.04, 0.17, 0.53];
  const leftBase = [-0.75, -0.41, -0.10];
  const rightBase = [0.75, -0.41, -0.09];
  const bottom = [-0.04, -0.72, 0.49];
  return [
    panel(left, back, front, 0.75), panel(back, right, front, 0.75),
    panel(left, front, leftBase), panel(front, leftBase, bottom),
    panel(front, right, rightBase, 0.65), panel(front, rightBase, bottom, 0.65),
    ...outline([left, back, right, rightBase, bottom, leftBase]),
    fold(left, front), fold(front, right), fold(front, bottom),
    fold(top, front, 0.12), fold([0.06, 0.17, 0.53], [0.06, -0.67, 0.49], 0.20),
    fold([-0.15, 0.19, 0.52], [-0.15, -0.68, 0.47], 0.20),
    ...outline([top, [-0.55, 0.82, 0.30], [-0.48, 0.98, 0.19], [-0.23, 0.91, 0.21]], 0.15),
    ...outline([top, [0.28, 0.99, 0.29], [0.59, 0.91, 0.27], [0.48, 0.74, 0.22]], 0.15),
  ];
}

const forms = { bird: origamiBird, lamp: readingLamp, plane: paperPlane, star, boat: paperBoat, gift };
export const SHAPE_NAMES = Object.keys(forms);

function sample(parts) {
  const total = parts.reduce((sum, part) => sum + part.weight, 0);
  const cloud = new Float32Array(PARTICLE_COUNT * 3);
  let start = 0;
  let collected = 0;
  for (const part of parts) {
    collected += part.weight;
    const end = Math.round(collected / total * PARTICLE_COUNT);
    for (let i = start; i < end; i++) {
      cloud.set(part.point((i - start + 0.5) / (end - start), i), i * 3);
    }
    start = end;
  }
  return cloud;
}

export function createShapes() {
  return Object.values(forms).map((create) => sample(create()));
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
