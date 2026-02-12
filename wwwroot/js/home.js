document.addEventListener('DOMContentLoaded', function () {

    // ==========================================
    // PART 1: HERO SLIDER LOGIC
    // ==========================================
    const slides = document.querySelectorAll('.slide-item');
    const thumbs = document.querySelectorAll('.thumb-card');
    const nextBtn = document.querySelector('.next-arrow');
    const prevBtn = document.querySelector('.prev-arrow');
    const sliderNav = document.querySelector('.slider-nav');
    const heroSection = document.querySelector('.aparat-hero');

    let currentIndex = 0;
    const totalSlides = slides.length;
    let slideInterval;
    let navTimeout;

    function showSlide(index) {
        if (index >= totalSlides) currentIndex = 0;
        else if (index < 0) currentIndex = totalSlides - 1;
        else currentIndex = index;

        slides.forEach(slide => {
            slide.classList.remove('active');
            slide.style.display = 'none';
        });

        const activeSlide = slides[currentIndex];
        activeSlide.style.display = 'block';
        setTimeout(() => activeSlide.classList.add('active'), 10);

        thumbs.forEach((t, i) => {
            t.classList.toggle('active', i === currentIndex);
        });

        showNavTemporarily();
    }

    function showNavTemporarily() {
        if (!sliderNav) return;
        sliderNav.classList.remove('hidden');
        clearTimeout(navTimeout);
        navTimeout = setTimeout(() => {
            sliderNav.classList.add('hidden');
        }, 5000);
    }

    function startTimer() {
        slideInterval = setInterval(() => showSlide(currentIndex + 1), 4000);
    }
    function stopTimer() {
        clearInterval(slideInterval);
    }
    function resetTimer() {
        stopTimer();
        startTimer();
    }

    if (nextBtn) nextBtn.addEventListener('click', () => { showSlide(currentIndex + 1); resetTimer(); });
    if (prevBtn) prevBtn.addEventListener('click', () => { showSlide(currentIndex - 1); resetTimer(); });

    thumbs.forEach((thumb, index) => {
        thumb.addEventListener('click', () => { showSlide(index); resetTimer(); });
    });

    if (heroSection) {
        heroSection.addEventListener('mousemove', showNavTemporarily);
        heroSection.addEventListener('touchstart', showNavTemporarily, { passive: true });
    }

    if (slides.length > 0) {
        showSlide(0);
        startTimer();
    }


    // ==========================================
    // PART 2: SALE COUNTER LOGIC (FIXED)
    // ==========================================
    const offerSection = document.querySelector(".supper-offer-section");
    if (offerSection) {
        const endsStr = offerSection.getAttribute("data-ends");

        // Only run timer if you actually have an end time
        if (endsStr) {
            const ends = new Date(endsStr);

            const oh = document.getElementById("oh");
            const om = document.getElementById("om");
            const os = document.getElementById("os");

            // if timer elements are not on the page, don't crash
            if (oh && om && os && !isNaN(ends.getTime())) {

                const pad = (n) => String(n).padStart(2, "0");

                const tick = () => {
                    const now = new Date();
                    const diff = Math.max(0, ends - now);

                    const s = Math.floor(diff / 1000);
                    const h = Math.floor(s / 3600);
                    const m = Math.floor((s % 3600) / 60);
                    const sec = s % 60;

                    oh.textContent = pad(h);
                    om.textContent = pad(m);
                    os.textContent = pad(sec);

                    if (diff <= 0) clearInterval(timer);
                };

                tick();
                var timer = setInterval(tick, 1000);
            }
        }
    }

});

// ==========================================
// PART 3: COPY COUPON (works anywhere)
// ==========================================
document.addEventListener("click", async (e) => {
    const btn = e.target.closest(".copy-btn");
    if (!btn) return;

    const code = btn.getAttribute("data-code");
    if (!code) return;

    try {
        await navigator.clipboard.writeText(code);
        const old = btn.textContent;
        btn.textContent = "کپی شد ✅";
        setTimeout(() => (btn.textContent = old || "کپی کردن کد"), 1200);
    } catch {
        alert("کپی نشد. دستی کپی کن: " + code);
    }
});

(function () {
    const track = document.getElementById('catRail');
    if (!track) return;

    const root = track.closest('.cat-rail');
    const btnL = root.querySelector('.cat-rail__nav.is-left');
    const btnR = root.querySelector('.cat-rail__nav.is-right');

    const isRTL = getComputedStyle(track).direction === 'rtl';

    function step() {
        // scroll by ~4 tiles
        const col = track.querySelector('.cat-tile')?.getBoundingClientRect().width || 100;
        return Math.round(col * 4);
    }

    function scrollByDir(dir) {
        // dir: -1 left, +1 right (logical)
        const amount = step() * dir;
        // In RTL some browsers invert scrollLeft behavior; this "good enough" approach:
        track.scrollBy({ left: isRTL ? -amount : amount, behavior: 'smooth' });
    }

    function updateButtons() {
        // tolerance because of fractional scroll positions
        const tol = 2;
        const max = track.scrollWidth - track.clientWidth;

        // Some RTL modes produce negative scrollLeft; normalize roughly:
        let x = track.scrollLeft;
        if (isRTL && x < 0) x = Math.abs(x);

        btnL.disabled = x <= tol;
        btnR.disabled = x >= (max - tol);
    }

    btnL?.addEventListener('click', () => scrollByDir(-1));
    btnR?.addEventListener('click', () => scrollByDir(+1));
    track.addEventListener('scroll', updateButtons, { passive: true });
    window.addEventListener('resize', updateButtons);

    updateButtons();
})();

(() => {
  const track = document.getElementById('posterTrack');
  const dotsHost = document.getElementById('posterDots');
  if(!track) return;

  const wrap = track.closest('.poster-rail__wrap');
  const prev = wrap?.querySelector('.poster-nav.is-prev');
  const next = wrap?.querySelector('.poster-nav.is-next');

  const cards = Array.from(track.querySelectorAll('.poster-card'));

  // create dots
  if (dotsHost && cards.length){
    dotsHost.innerHTML = cards.map((_,i)=>`<span class="dot" data-i="${i}"></span>`).join('');
  }
  const dots = dotsHost ? Array.from(dotsHost.querySelectorAll('.dot')) : [];

  const step = () => {
    const card = cards[0]?.getBoundingClientRect().width || 320;
    return Math.round(card + 18); // card + gap
  };

  function update(){
    const max = track.scrollWidth - track.clientWidth;
    const x = track.scrollLeft;
    if(prev) prev.disabled = x <= 2;
    if(next) next.disabled = x >= max - 2;

    // active dot based on nearest card
    if(dots.length){
      const idx = Math.round(x / step());
      dots.forEach((d,i)=>d.classList.toggle('is-active', i === Math.min(cards.length-1, Math.max(0, idx))));
    }
  }

  prev?.addEventListener('click', () => track.scrollBy({ left: -step()*1.2, behavior:'smooth' }));
  next?.addEventListener('click', () => track.scrollBy({ left:  step()*1.2, behavior:'smooth' }));

  dotsHost?.addEventListener('click', (e) => {
    const dot = e.target.closest('.dot');
    if(!dot) return;
    const i = +dot.dataset.i;
    track.scrollTo({ left: i * step(), behavior:'smooth' });
  });

  track.addEventListener('scroll', update, { passive:true });
  window.addEventListener('resize', update);
  update();
})();
