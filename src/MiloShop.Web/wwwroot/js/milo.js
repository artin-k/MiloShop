// The /Studio scene uses Kage's authored menu. Shopping pages use this small equivalent.
const navigation = document.getElementById('nav');
const burger = navigation?.querySelector('.nav-burger');
const closeMenu = () => {
    navigation?.classList.remove('menu-open');
    burger?.classList.remove('active');
    burger?.setAttribute('aria-expanded', 'false');
    document.documentElement.classList.remove('nav-open');
};
if (!document.body.classList.contains('scene-page')) {
    burger?.addEventListener('click', () => {
        const open = burger.getAttribute('aria-expanded') !== 'true';
        navigation.classList.toggle('menu-open', open);
        burger.classList.toggle('active', open);
        burger.setAttribute('aria-expanded', String(open));
        document.documentElement.classList.toggle('nav-open', open);
    });
}
document.addEventListener('keydown', event => {
    if (event.key === 'Escape') { closeMenu(); burger?.focus(); }
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
