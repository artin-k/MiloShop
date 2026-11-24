const navigation = document.getElementById('nav');
const burger = navigation?.querySelector('.nav-burger');
const menu = navigation?.querySelector('.tubelight-nav');

if (menu) {
    const lamp = menu.querySelector('.tubelight-lamp');
    const home = menu.querySelector('[data-home-link][aria-current="page"]');
    const collections = menu.querySelector('[data-collections-link]');
    const updateLamp = () => {
        const active = menu.querySelector('a[aria-current]');
        menu.classList.toggle('is-tracking', Boolean(active));
        if (!active) return;
        lamp.style.setProperty('--lamp-x', `${active.offsetLeft}px`);
        lamp.style.setProperty('--lamp-width', `${active.offsetWidth}px`);
    };
    const setCurrentSection = () => {
        if (home && collections) {
            const onCollections = location.hash === '#bookshelf';
            if (onCollections) {
                home.removeAttribute('aria-current');
                collections.setAttribute('aria-current', 'location');
            } else {
                home.setAttribute('aria-current', 'page');
                collections.removeAttribute('aria-current');
            }
        }
        updateLamp();
    };
    setCurrentSection();
    addEventListener('hashchange', setCurrentSection);
    addEventListener('pageshow', setCurrentSection);
    const layout = new ResizeObserver(updateLamp);
    menu.querySelectorAll('a').forEach(link => layout.observe(link));
    requestAnimationFrame(() => menu.classList.add('is-animated'));

    const reflectLight = matchMedia('(hover: hover) and (prefers-reduced-motion: no-preference)');
    navigation.querySelectorAll('.brand, .tubelight-nav, .theme-toggle').forEach(surface => {
        surface.addEventListener('pointermove', event => {
            if (!reflectLight.matches) return;
            const bounds = surface.getBoundingClientRect();
            surface.style.setProperty('--glass-x', `${100 * (event.clientX - bounds.left) / bounds.width}%`);
            surface.style.setProperty('--glass-y', `${100 * (event.clientY - bounds.top) / bounds.height}%`);
        });
        surface.addEventListener('pointerleave', () => {
            surface.style.removeProperty('--glass-x');
            surface.style.removeProperty('--glass-y');
        });
    });
}

document.addEventListener('keydown', event => {
    if (event.key === 'Escape' && navigation?.classList.contains('menu-open')) {
        navigation.classList.remove('menu-open');
        burger?.classList.remove('active');
        burger?.setAttribute('aria-expanded', 'false');
        document.documentElement.classList.remove('nav-open');
        burger?.focus();
    }
    if (event.key !== 'Tab' || !navigation?.classList.contains('menu-open')) return;
    const nodes = [...navigation.querySelectorAll('a,button')].filter(el => el.getClientRects().length);
    const first = nodes[0], last = nodes[nodes.length - 1];
    if (event.shiftKey && document.activeElement === first) { event.preventDefault(); last.focus(); }
    else if (!event.shiftKey && document.activeElement === last) { event.preventDefault(); first.focus(); }
});
document.addEventListener('click', async event => {
    const button = event.target.closest('[data-code]');
    if (!button) return;
    try { await navigator.clipboard.writeText(button.dataset.code); button.textContent = 'کپی شد'; }
    catch { button.textContent = button.dataset.code; }
});
let comparison;
document.addEventListener('change', event => {
    if (!event.target.matches('[data-compare]')) return;
    const input = event.target;
    comparison ??= import('./comparison.js');
    comparison.then(module => module.update(input)).catch(() => {
        input.checked = false;
        document.getElementById('compare-count').textContent = 'بارگذاری مقایسه انجام نشد؛ دوباره تلاش کنید.';
        document.getElementById('compare-bar').hidden = false;
        comparison = undefined;
    });
});
