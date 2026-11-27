// Keep the real product link available for new tabs and browsers without dialogs.
const shelf = document.querySelector('[data-product-shelf]');
const reducedMotion = matchMedia('(prefers-reduced-motion: reduce)');

function shelfTransition(source, target, returning = false) {
    if (reducedMotion.matches || !target.animate) return;
    const from = source.getBoundingClientRect();
    const to = target.getBoundingClientRect();
    if (!from.width || !to.width) return;
    const x = from.left + from.width / 2 - to.left - to.width / 2;
    const y = from.top + from.height / 2 - to.top - to.height / 2;
    const offset = 'translate(' + x + 'px,' + y + 'px) scale(' + from.height / to.height + ')';
    const frames = [{ transform: offset + ' rotateY(-16deg)' }, { transform: 'none' }];
    return target.animate(returning ? frames.reverse() : frames, {
        duration: returning ? 240 : 440, easing: 'cubic-bezier(.22,.74,.2,1)'
    });
}

function enableInspector(link) {
    const dialog = document.getElementById(link.dataset.inspector);
    if (!dialog?.showModal) return;
    link.setAttribute('aria-haspopup', 'dialog');
    link.setAttribute('aria-controls', dialog.id);
    const source = link.querySelector('.shelf-art');
    const photo = dialog.querySelector('.shelf-inspector__photo');
    const art = photo.querySelector('.shelf-art');
    let drag;
    let closing = false;

    link.addEventListener('click', event => {
        if (event.button || event.metaKey || event.ctrlKey || event.shiftKey || event.altKey) return;
        event.preventDefault();
        dialog.showModal();
        shelfTransition(source, art);
    });

    async function close() {
        if (closing) return;
        closing = true;
        art.getAnimations().forEach(animation => animation.cancel());
        const transition = shelfTransition(source, art, true);
        if (transition) await transition.finished.catch(() => {});
        dialog.close();
    }
    dialog.querySelector('.shelf-back').addEventListener('click', close);
    dialog.addEventListener('cancel', event => { event.preventDefault(); close(); });
    dialog.addEventListener('click', event => {
        if (event.target !== dialog) return;
        const rect = dialog.getBoundingClientRect();
        if (event.clientX < rect.left || event.clientX > rect.right ||
            event.clientY < rect.top || event.clientY > rect.bottom) close();
    });
    dialog.addEventListener('close', () => {
        closing = false;
        drag = null;
        art.style.transform = '';
        link.focus({ preventScroll: true });
    });

    photo.addEventListener('pointerdown', event => {
        if (event.button || !event.isPrimary || reducedMotion.matches || closing) return;
        art.getAnimations().forEach(animation => animation.cancel());
        drag = { start: event.clientX, angle: 0, velocity: 0, time: event.timeStamp };
        photo.setPointerCapture(event.pointerId);
    });
    photo.addEventListener('pointermove', event => {
        if (!drag || !photo.hasPointerCapture(event.pointerId)) return;
        // A single catalog photo has one printed face. Tilt it without exposing a mirrored back.
        const angle = Math.max(-52, Math.min(52, (event.clientX - drag.start) * .45));
        drag.velocity = (angle - drag.angle) / Math.max(1, event.timeStamp - drag.time);
        drag.time = event.timeStamp;
        drag.angle = angle;
        art.style.transform = 'rotateY(' + angle + 'deg)';
    });
    photo.addEventListener('lostpointercapture', () => {
        if (!drag) return;
        const coast = Math.max(-60, Math.min(60, drag.angle + drag.velocity * 40));
        if (!reducedMotion.matches && !closing) {
            art.animate([art.style.transform || 'none', 'rotateY(' + coast + 'deg)', 'none']
                .map(transform => ({ transform })), { duration: 460, easing: 'ease-out' });
        }
        art.style.transform = '';
        drag = null;
    });
}

shelf?.querySelectorAll('[data-inspector]').forEach(enableInspector);
