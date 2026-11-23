// Original Milo stationery artwork. No demo atlas, AI-tool logos or remote media.
function line(ctx, points) {
  ctx.beginPath();
  points.forEach(([x, y], i) => (i ? ctx.lineTo(x, y) : ctx.moveTo(x, y)));
  ctx.stroke();
}
function circle(ctx, x, y, r) {
  ctx.beginPath();
  ctx.arc(x, y, r, 0, Math.PI * 2);
  ctx.stroke();
}

export function drawCategoryMotif(ctx, book, width, height) {
  ctx.save();
  ctx.translate(width * 0.5, height * 0.4);
  const size = Math.min(width, height) * 0.285;
  ctx.scale(size, size);
  ctx.strokeStyle = book.foil;
  ctx.fillStyle = book.foil;
  ctx.lineWidth = 0.026;
  ctx.lineCap = 'round';
  ctx.lineJoin = 'round';
  switch (book.motifKey) {
    case 'writing':
      line(ctx, [
        [-0.39, -0.83],
        [0.39, -0.83],
        [0.57, -0.18],
        [0, 0.98],
        [-0.57, -0.18],
        [-0.39, -0.83],
      ]);
      circle(ctx, 0, -0.12, 0.12);
      line(ctx, [
        [0, 0],
        [0, 0.98],
      ]);
      line(ctx, [
        [-0.42, -0.66],
        [0.42, -0.66],
      ]);
      break;
    case 'art':
      ctx.beginPath();
      ctx.moveTo(0.75, 0.2);
      ctx.bezierCurveTo(1.2, -0.85, -0.9, -1.35, -0.94, -0.12);
      ctx.bezierCurveTo(-1.03, 0.8, 0.08, 1.03, 0.33, 0.55);
      ctx.bezierCurveTo(0.6, 0.12, 0.42, 0.68, 0.75, 0.2);
      ctx.stroke();
      [
        [-0.5, -0.3],
        [-0.15, -0.55],
        [0.25, -0.5],
        [0.55, -0.18],
      ].forEach(([x, y]) => circle(ctx, x, y, 0.105));
      line(ctx, [
        [0.04, 0.78],
        [0.65, -0.02],
        [0.79, 0.09],
        [0.21, 0.91],
        [0.04, 0.78],
      ]);
      break;
    case 'planner':
      ctx.strokeRect(-0.77, -0.66, 1.54, 1.43);
      line(ctx, [
        [-0.77, -0.25],
        [0.77, -0.25],
      ]);
      line(ctx, [
        [-0.38, -0.86],
        [-0.38, -0.46],
      ]);
      line(ctx, [
        [0.38, -0.86],
        [0.38, -0.46],
      ]);
      for (let y = 0; y < 3; y++)
        for (let x = 0; x < 3; x++)
          ctx.strokeRect(-0.5 + x * 0.38, -0.02 + y * 0.24, 0.14, 0.09);
      break;
    case 'desk':
      line(ctx, [
        [-0.88, 0.73],
        [0.88, 0.73],
        [-0.88, -0.82],
        [-0.88, 0.73],
      ]);
      line(ctx, [
        [-0.58, 0.46],
        [0.13, 0.46],
        [-0.58, -0.17],
        [-0.58, 0.46],
      ]);
      for (let i = 0; i < 6; i++)
        line(ctx, [
          [-0.72 + i * 0.22, 0.73],
          [-0.72 + i * 0.22, 0.61],
        ]);
      line(ctx, [
        [0.2, -0.74],
        [0.38, -0.9],
        [0.93, -0.35],
        [0.77, -0.17],
        [0.2, -0.74],
      ]);
      break;
    case 'gift':
      ctx.strokeRect(-0.7, -0.12, 1.4, 0.92);
      ctx.strokeRect(-0.81, -0.33, 1.62, 0.21);
      line(ctx, [
        [0, -0.33],
        [0, 0.8],
      ]);
      ctx.beginPath();
      ctx.moveTo(0, -0.33);
      ctx.bezierCurveTo(-1, -1.05, -0.8, -1.25, 0, -0.33);
      ctx.bezierCurveTo(0.8, -1.25, 1, -1.05, 0, -0.33);
      ctx.stroke();
      break;
    case 'school':
      line(ctx, [
        [-0.64, -0.76],
        [0.37, -0.76],
        [0.71, -0.42],
        [0.71, 0.84],
        [-0.64, 0.84],
        [-0.64, -0.76],
      ]);
      line(ctx, [
        [0.37, -0.76],
        [0.37, -0.42],
        [0.71, -0.42],
      ]);
      for (let i = 0; i < 4; i++)
        line(ctx, [
          [-0.38, -0.08 + i * 0.2],
          [0.43, -0.08 + i * 0.2],
        ]);
      break;
    default:
      ctx.strokeRect(-0.64, -0.81, 1.27, 1.62);
      line(ctx, [
        [-0.4, -0.81],
        [-0.4, 0.81],
      ]);
      for (let i = 0; i < 4; i++)
        line(ctx, [
          [-0.2, -0.34 + i * 0.2],
          [0.4, -0.34 + i * 0.2],
        ]);
      [-0.53, -0.22, 0.1, 0.41, 0.7].forEach((y) => {
        line(ctx, [
          [-0.76, y],
          [-0.55, y],
        ]);
      });
  }
  ctx.restore();
}

function makeCanvas(width = 768, height = 1152) {
  const canvas = document.createElement('canvas');
  canvas.width = width;
  canvas.height = height;
  const ctx = canvas.getContext('2d');
  ctx.direction = 'rtl';
  ctx.letterSpacing = '0px';
  return { canvas, ctx };
}
function linesFor(ctx, text, width) {
  const lines = [];
  let line = '';
  for (const word of String(text).split(/\s+/)) {
    const next = line ? `${line} ${word}` : word;
    if (line && ctx.measureText(next).width > width) {
      lines.push(line);
      line = word;
    } else line = next;
  }
  if (line) lines.push(line);
  return lines;
}
function text(ctx, value, x, y, width, size = 44, maxLines = 3) {
  let lines;
  do {
    ctx.font = `400 ${size}px MiloPersian, sans-serif`;
    lines = linesFor(ctx, value, width);
    if (
      lines.length <= maxLines &&
      lines.every((line) => ctx.measureText(line).width <= width)
    )
      break;
    size -= 2;
  } while (size > 18);
  ctx.textAlign = 'right';
  ctx.direction = 'rtl';
  ctx.letterSpacing = '0px';
  lines
    .slice(0, maxLines)
    .forEach((line, i) => ctx.fillText(line, x, y + i * size * 1.8));
  return y + Math.min(lines.length, maxLines) * size * 1.8;
}
function latin(ctx, value, x, y, size = 20) {
  ctx.save();
  ctx.direction = 'ltr';
  ctx.textAlign = 'left';
  ctx.letterSpacing = '1.5px';
  ctx.font = `400 ${size}px Onest, sans-serif`;
  ctx.fillText(value, x, y);
  ctx.restore();
}
function texture(THREE, canvas, configure, name) {
  const value = configure(new THREE.CanvasTexture(canvas));
  value.name = name;
  return value;
}

export function makeCategoryCover(THREE, book, configure) {
  const { canvas, ctx } = makeCanvas();
  ctx.fillStyle = book.color;
  ctx.fillRect(0, 0, 768, 1152);
  ctx.strokeStyle = '#ffffff0d';
  ctx.lineWidth = 1;
  for (let i = 0; i < 768; i += 5)
    line(ctx, [
      [i, 0],
      [i, 1152],
    ]);
  const shade = ctx.createLinearGradient(0, 0, 768, 0);
  shade.addColorStop(0, '#0005');
  shade.addColorStop(0.1, '#fff1');
  shade.addColorStop(0.8, '#fff0');
  shade.addColorStop(1, '#0002');
  ctx.fillStyle = shade;
  ctx.fillRect(0, 0, 768, 1152);
  ctx.fillStyle = book.foil;
  latin(ctx, 'MILO', 65, 112, 48);
  latin(ctx, 'STATIONERY / COLLECTION', 65, 149, 13);
  ctx.globalAlpha = 0.5;
  drawCategoryMotif(ctx, book, 768, 1152);
  ctx.globalAlpha = 1;
  text(ctx, book.title, 697, 850, 620, 58, 2);
  ctx.fillStyle = '#fff9';
  text(ctx, 'ابزار ایده‌های تازه', 697, 1050, 600, 25, 1);
  return texture(THREE, canvas, configure, `${book.id}-milo-cover`);
}
export function makeCategoryFoil(THREE, book, configure) {
  const { canvas, ctx } = makeCanvas();
  ctx.strokeStyle = '#fff';
  ctx.lineWidth = 2;
  ctx.strokeRect(35, 35, 698, 1082);
  drawCategoryMotif(ctx, { ...book, foil: '#fff' }, 768, 1152);
  return texture(THREE, canvas, configure, `${book.id}-milo-foil`);
}
export function makeCategorySpine(THREE, book, configure) {
  const { canvas, ctx } = makeCanvas(384, 1536);
  ctx.fillStyle = '#fff';
  ctx.strokeStyle = '#fff';
  ctx.lineWidth = 2;
  ctx.strokeRect(35, 35, 314, 1466);
  latin(ctx, 'MILO', 95, 135, 45);
  ctx.save();
  ctx.translate(195, 1260);
  ctx.rotate(Math.PI / 2);
  text(ctx, book.title, 0, 0, 980, 62, 1);
  ctx.restore();
  return texture(THREE, canvas, configure, `${book.id}-milo-spine`);
}
export function makeCategoryBack(THREE, book, configure) {
  const { canvas, ctx } = makeCanvas();
  ctx.fillStyle = '#fff';
  ctx.strokeStyle = '#fff';
  ctx.lineWidth = 2;
  latin(ctx, 'MILO / PAPER, PENS & POSSIBILITIES', 64, 100, 20);
  ctx.globalAlpha = 0.4;
  drawCategoryMotif(ctx, { ...book, foil: '#fff' }, 768, 1152);
  ctx.globalAlpha = 1;
  text(ctx, book.title, 690, 910, 620, 44, 2);
  text(ctx, 'صفحه بعدی را، خودت بنویس.', 690, 1060, 620, 26, 1);
  return texture(THREE, canvas, configure, `${book.id}-milo-back-foil`);
}
export function makeCategoryPages(THREE, book, configure) {
  const count = new Intl.NumberFormat('fa-IR').format(book.productCount);
  const available = book.productCount
    ? `${count} محصول موجود در این دسته`
    : 'این مجموعه در حال تکمیل است.';
  const pages = [
    [book.title, book.note],
    ['ابزار مناسب تو', book.deck],
    ['از یک ایده شروع کن', book.note],
    [
      'با خیال راحت انتخاب کن',
      'مشخصات، قیمت و موجودی هر محصول را در صفحه فروشگاه ببین و انتخاب متناسب با نیازت را پیدا کن.',
    ],
    ['جایی برای فکرهای تو', 'یادداشت کن، طرح بزن و به ایده بعدی‌ات فرصت بده.'],
    ['مجموعه‌های میلو', available],
    ['خیال، شکل می‌گیرد', book.note],
    [
      'انتخاب بعدی تو',
      `${available} برای ادامه، دکمه دیدن محصولات این مجموعه را انتخاب کن.`,
    ],
  ];
  return pages.map(([heading, body], i) => {
    const { canvas, ctx } = makeCanvas(512, 768);
    ctx.fillStyle = '#f5efdf';
    ctx.fillRect(0, 0, 512, 768);
    ctx.strokeStyle = '#b0a58d25';
    ctx.lineWidth = 1;
    for (let y = 110; y < 700; y += 30)
      line(ctx, [
        [36, y],
        [476, y],
      ]);
    ctx.fillStyle = '#546255';
    latin(ctx, `MILO / ${book.roman}`, 36, 47, 11);
    ctx.fillStyle = '#294336';
    text(ctx, heading, 465, 150, 420, 34, 2);
    if (i === 0 || i === 2 || i === 6) {
      ctx.globalAlpha = 0.65;
      ctx.save();
      ctx.translate(0, 160);
      drawCategoryMotif(ctx, { ...book, foil: book.color }, 512, 550);
      ctx.restore();
      ctx.globalAlpha = 1;
      text(ctx, body, 465, 550, 420, 20, 3);
    } else text(ctx, body, 465, 300, 420, 23, 7);
    ctx.fillStyle = '#546255';
    text(ctx, book.title, 465, 722, 420, 13, 1);
    return texture(THREE, canvas, configure, `${book.id}-milo-page-${i + 1}`);
  });
}
