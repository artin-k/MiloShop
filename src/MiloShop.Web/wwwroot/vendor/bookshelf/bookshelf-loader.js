// Site adapter only. The registered React component and renderer stay unchanged.
const section = document.querySelector('[data-bookshelf-section]');
if (section) {
  const mount = section.querySelector('[data-bookshelf-mount]');
  const status = section.querySelector('[data-bookshelf-status]');
  const start = section.querySelector('[data-bookshelf-start]');
  const actions = [...section.querySelectorAll('[data-bookshelf-action]')];
  const volumes = [...section.querySelectorAll('[data-bookshelf-volume]')];
  let task, unmount, observer;
  let lastDetail = false;

  function sync() {
    const host = mount.querySelector('.bookshelf');
    const state = host?.dataset.state;
    const ready = state === 'ready';
    section.dataset.bookshelfState = state || 'loading';
    if (state === 'unavailable') {
      status.textContent =
        'نمایش سه‌بعدی در این مرورگر در دسترس نیست. فروشگاه همچنان در دسترس شماست.';
      start.hidden = false;
      start.disabled = false;
      start.textContent = 'تلاش دوباره';
    } else if (ready) {
      status.textContent = 'یک جلد انتخاب کن، آن را از نزدیک ببین و ورق بزن.';
      start.hidden = true;
    }
    const detail = host?.classList.contains('mode-detail');
    const busy =
      host?.classList.contains('is-opening') ||
      host?.querySelector('#live-region')?.textContent.startsWith('Returning ');
    const source = (id) => host?.querySelector(`#${id}`);
    actions.forEach((button) => {
      const id = button.dataset.bookshelfAction;
      const inBrowse = ['previous', 'next', 'inspect'].includes(id);
      button.hidden = detail ? inBrowse : !inBrowse;
      button.disabled = !ready || busy || source(id)?.disabled;
      if (id === 'toggle-book') {
        const open = source(id)?.getAttribute('aria-pressed') === 'true';
        button.textContent = open ? 'بستن کتاب' : 'باز کردن کتاب';
        button.setAttribute('aria-pressed', String(open));
      }
    });
    volumes.forEach((button, i) => {
      button.disabled = !ready || detail || busy;
      button.setAttribute(
        'aria-pressed',
        String(
          source('markers')?.children[i]?.getAttribute('aria-selected') ===
            'true',
        ),
      );
    });
    section.querySelector('[data-bookshelf-title]').textContent =
      source('selection-title')?.textContent || 'Working Volumes';
    section.querySelector('[data-bookshelf-counter]').textContent =
      source('counter')?.textContent || '01 / 07';
    section.querySelector('[data-bookshelf-page]').textContent = detail
      ? source('page-counter')?.textContent || ''
      : '';
    if (ready && !busy && Boolean(detail) !== lastDetail) {
      section
        .querySelector(
          `[data-bookshelf-action="${detail ? 'close-detail' : 'inspect'}"]`,
        )
        .focus({ preventScroll: true });
      lastDetail = Boolean(detail);
    }
  }

  function loadStyles() {
    const href = new URL('bookshelf-INFP3QEQ.css', import.meta.url).href;
    if ([...document.styleSheets].some((sheet) => sheet.href === href))
      return Promise.resolve();
    return new Promise((resolve, reject) => {
      const link = document.createElement('link');
      link.rel = 'stylesheet';
      link.href = href;
      link.onload = resolve;
      link.onerror = () => {
        link.remove();
        reject(new Error('Styles could not load'));
      };
      document.head.append(link);
    });
  }

  async function load() {
    if (task) return task;
    start.disabled = true;
    status.textContent = 'کتابخانه در حال آماده شدن است…';
    section.dataset.bookshelfState = 'loading';
    task = (async () => {
      await loadStyles();
      const module = await import(
        new URL('bookshelf-6XFMMR2C.js', import.meta.url)
      );
      unmount = module.mountBookshelf(mount);
      observer = new MutationObserver(sync);
      observer.observe(mount, {
        subtree: true,
        childList: true,
        characterData: true,
        attributes: true,
        attributeFilter: [
          'data-state',
          'class',
          'disabled',
          'inert',
          'aria-pressed',
          'aria-selected',
        ],
      });
    })().catch(() => {
      status.textContent =
        'بارگذاری کتابخانه انجام نشد. دوباره تلاش کن یا به فروشگاه برو.';
      section.dataset.bookshelfState = 'unavailable';
      start.disabled = false;
      start.hidden = false;
      start.textContent = 'تلاش دوباره';
      task = undefined;
    });
    return task;
  }

  start.addEventListener('click', () => {
    if (section.dataset.bookshelfState === 'unavailable') {
      observer?.disconnect();
      unmount?.();
      unmount = undefined;
      task = undefined;
    }
    load();
  });
  actions.forEach((button) =>
    button.addEventListener('click', () => {
      mount.querySelector(`#${button.dataset.bookshelfAction}`)?.click();
      sync();
    }),
  );
  volumes.forEach((button) =>
    button.addEventListener('click', () => {
      mount
        .querySelector('#markers')
        ?.children[Number(button.dataset.bookshelfVolume)]?.click();
      sync();
    }),
  );
  // The exact component hides its source controls. Let Tab reach our visible
  // controls instead of cycling through those hidden buttons during inspection.
  mount.addEventListener(
    'keydown',
    (event) => {
      if (event.key === 'Tab') event.stopPropagation();
    },
    true,
  );
  mount.addEventListener('keydown', (event) => {
    if (event.key === 'Escape') event.stopPropagation();
  });
  section.addEventListener('keydown', (event) => {
    if (event.key === 'Escape' && !mount.contains(event.target)) {
      mount.querySelector('#close-detail')?.click();
      event.stopPropagation();
    }
  });
  if ('IntersectionObserver' in window) {
    const visibility = new IntersectionObserver(
      (entries) => {
        if (entries.some((entry) => entry.isIntersecting)) {
          visibility.disconnect();
          load();
        }
      },
      { rootMargin: '100px 0px' },
    );
    visibility.observe(mount);
  }
}
