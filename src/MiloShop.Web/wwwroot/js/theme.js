// Apply the preference before styles paint. Without a saved choice, follow the OS.
(() => {
    const key = 'miloshop-theme';
    const media = matchMedia('(prefers-color-scheme: light)');
    let saved;
    try { saved = localStorage.getItem(key); } catch { /* storage may be unavailable */ }
    const valid = value => value === 'light' || value === 'dark';
    let explicit = valid(saved);
    function apply(theme) {
        document.documentElement.dataset.theme = theme;
        document.documentElement.style.colorScheme = theme;
        document.querySelector('meta[name="theme-color"]')?.setAttribute('content', theme === 'light' ? '#f5f2e9' : '#101b19');
        const button = document.querySelector('.theme-toggle');
        button?.setAttribute('aria-pressed', String(theme === 'light'));
        button?.setAttribute('aria-label', theme === 'light' ? 'تغییر به حالت تاریک' : 'تغییر به حالت روشن');
        document.dispatchEvent(new CustomEvent('milo:theme', { detail: theme }));
    }
    apply(explicit ? saved : media.matches ? 'light' : 'dark');
    document.addEventListener('DOMContentLoaded', () => {
        apply(document.documentElement.dataset.theme);
        document.querySelector('.theme-toggle')?.addEventListener('click', () => {
            const theme = document.documentElement.dataset.theme === 'light' ? 'dark' : 'light';
            explicit = true;
            try { localStorage.setItem(key, theme); } catch { /* preference still works in this tab */ }
            apply(theme);
        });
    });
    media.addEventListener('change', event => { if (!explicit) apply(event.matches ? 'light' : 'dark'); });
    window.addEventListener('storage', event => {
        if (event.key !== key) return;
        explicit = valid(event.newValue);
        apply(explicit ? event.newValue : media.matches ? 'light' : 'dark');
    });
})();
