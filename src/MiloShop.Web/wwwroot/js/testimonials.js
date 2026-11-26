function mountTestimonials(root) {
    const quotes = [...root.querySelectorAll('.review-quote')];
    const photos = [...root.querySelectorAll('.review-photo')];
    const controls = root.querySelector('.review-controls');
    if (!controls) return;

    const play = controls.querySelector('[data-play]');
    const motion = matchMedia('(prefers-reduced-motion: reduce)');
    let playing = !motion.matches;
    let visible = false;
    let current = 0;
    let timer;

    for (const quote of quotes) {
        const text = quote.querySelector('blockquote');
        const words = text.textContent.trim().split(/\s+/);
        text.replaceChildren();
        words.forEach((word, index) => {
            const span = document.createElement('span');
            span.textContent = word;
            span.style.setProperty('--word-delay', Math.min(index * 25, 450) + 'ms');
            text.append(span, ' ');
        });
    }

    function show(index) {
        current = (index + quotes.length) % quotes.length;
        quotes.forEach((quote, i) => { quote.hidden = i !== current; });
        photos.forEach((photo, i) => {
            const offset = (i - current + photos.length) % photos.length;
            let position = 'away';
            if (offset === 0) position = 'current';
            else if (offset === 1) position = 'next';
            else if (offset === photos.length - 1) position = 'previous';
            photo.dataset.position = position;
        });
    }

    function syncPlayback() {
        clearInterval(timer);
        play.textContent = playing ? 'Ⅱ' : '▷';
        play.setAttribute('aria-label', playing ? 'توقف نمایش خودکار' : 'شروع نمایش خودکار');
        if (playing && visible && !document.hidden) {
            timer = setInterval(() => show(current + 1), 5000);
        }
    }

    function step(amount) {
        playing = false;
        show(current + amount);
        syncPlayback();
    }

    controls.querySelectorAll('[data-step]').forEach(button => {
        button.addEventListener('click', () => step(Number(button.dataset.step)));
    });
    root.addEventListener('keydown', event => {
        if (event.key !== 'ArrowLeft' && event.key !== 'ArrowRight') return;
        event.preventDefault();
        step(event.key === 'ArrowLeft' ? 1 : -1);
        controls.querySelector(event.key === 'ArrowLeft' ? '[data-step="1"]' : '[data-step="-1"]').focus();
    });
    root.addEventListener('focusin', event => {
        if (!event.target.closest('.review-quote')) return;
        playing = false;
        syncPlayback();
    });
    play.addEventListener('click', () => {
        playing = !playing;
        syncPlayback();
    });
    motion.addEventListener('change', () => {
        playing = !motion.matches;
        syncPlayback();
    });
    new IntersectionObserver(([entry]) => {
        visible = entry.isIntersecting;
        syncPlayback();
    }).observe(root);
    document.addEventListener('visibilitychange', syncPlayback);
    show(0);
    syncPlayback();
    controls.hidden = false;
}

document.querySelectorAll('[data-testimonials]').forEach(mountTestimonials);
