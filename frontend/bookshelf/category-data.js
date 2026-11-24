// Presentation only: IDs, names, descriptions, counts and URLs come from Razor.
const colors = [
  ['#285448', '#e2cb96'],
  ['#9c5741', '#f4dfb6'],
  ['#405a76', '#e4d4ad'],
  ['#76633f', '#f9e5b5'],
  ['#65526e', '#e7d7d6'],
  ['#426e68', '#f0d9ae'],
];
const profiles = [
  {
    match: /برنامه|پلنر|تقویم|planner|calendar/i,
    kind: 'planner',
    note: 'برای روزهایی که می‌خواهی با فکر و آرامش پیش ببری.',
  },
  {
    match: /نقاش|طراح|هنر|رنگ|art|paint|sketch/i,
    kind: 'art',
    note: 'از اولین خط تا ایده‌ای که رنگ می‌گیرد.',
  },
  {
    match: /خودکار|روان|قلم|مداد|نوشت|pen|pencil|writing/i,
    kind: 'writing',
    note: 'هر فکر تازه، از یک خط کوچک شروع می‌شود.',
  },
  {
    match: /نظم|جامدادی|میز|اداری|desk|office/i,
    kind: 'desk',
    note: 'ابزارهایت را کنار هم بگذار و جایی برای فکرهای تازه بساز.',
  },
  {
    match: /هدیه|کادو|gift/i,
    kind: 'gift',
    note: 'یک انتخاب کوچک، برای خوشحال کردن یک ذهن خلاق.',
  },
  {
    match: /مدرسه|دانش|school/i,
    kind: 'school',
    note: 'همراه فکرهای تازه، در کلاس و بیرون از آن.',
  },
  {
    match: /.*/,
    kind: 'notebook',
    note: 'صفحه‌ای تازه برای یادداشت، طرح و رویاهای تو.',
  },
];

export function categoryRoomPalette(dark = false) {
  return dark
    ? {
        paper: '#14241e',
        paperDeep: '#101c18',
        paperPale: '#f5edda',
        ink: '#f4efdf',
        inkSoft: '#c8ccb7',
        wall: '#20382e',
        shelf: '#513727',
        shelfDark: '#251b14',
        light: '#f2dbb1',
        fill: '#acc5bd',
      }
    : {
        paper: '#eee9dc',
        paperDeep: '#dfd9ca',
        paperPale: '#fff8e9',
        ink: '#253e34',
        inkSoft: '#647464',
        wall: '#eee8da',
        shelf: '#72513b',
        shelfDark: '#3e2c20',
        light: '#fff0d4',
        fill: '#c7d9db',
      };
}

export function createCategoryVolumes(categories, dark = false) {
  if (!Array.isArray(categories) || categories.length === 0)
    throw new Error('No category collection was supplied.');
  const seen = new Set();
  return categories.map((category, index) => {
    if (
      !Number.isSafeInteger(category.id) ||
      category.id <= 0 ||
      seen.has(category.id) ||
      typeof category.title !== 'string' ||
      !category.title.trim()
    ) {
      throw new Error(
        'Category collection has invalid or duplicate identifiers.',
      );
    }
    seen.add(category.id);
    const [color, foil] = colors[(category.id - 1) % colors.length];
    const profile = profiles.find((profile) =>
      profile.match.test(category.title),
    );
    const count = Number.isSafeInteger(category.productCount)
      ? Math.max(0, category.productCount)
      : 0;
    const description =
      typeof category.description === 'string' && category.description.trim()
        ? category.description.trim()
        : profile.note;
    return {
      id: `category-${category.id}`,
      categoryId: category.id,
      title: category.title.trim(),
      roman: String(index + 1).padStart(2, '0'),
      sequence: index + 1,
      discipline: 'نوشت‌افزار و ابزار خلاقیت',
      note: profile.note,
      deck: description,
      binding: 'مجموعه‌های میلو',
      format: 'راهنمای انتخاب نوشت‌افزار',
      theme: category.title.trim(),
      motif: 'MILO / STATIONERY',
      motifKey: profile.kind,
      paletteLabel: 'کاغذ، جوهر و خیال',
      color,
      foil,
      palette: categoryRoomPalette(dark),
      productCount: count,
      width: 1.02 + (category.id % 3) * 0.035,
      height: 1.5 + (category.id % 4) * 0.035,
      depth: 0.25 + (category.id % 3) * 0.012,
      seed: category.id * 17,
    };
  });
}
