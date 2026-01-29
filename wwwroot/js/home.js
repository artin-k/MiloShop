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
            if (i === currentIndex) t.classList.add('active');
            else t.classList.remove('active');
        });

        showNavTemporarily();
    }

    nextBtn.addEventListener('click', () => {
        showSlide(currentIndex + 1);
        resetTimer();
    });

    prevBtn.addEventListener('click', () => {
        showSlide(currentIndex - 1);
        resetTimer();
    });

    thumbs.forEach((thumb, index) => {
        thumb.addEventListener('click', () => {
            showSlide(index);
            resetTimer();
        });
    });

    // Nav Auto-Hide (5 Seconds)
    function showNavTemporarily() {
        sliderNav.classList.remove('hidden');
        clearTimeout(navTimeout);
        navTimeout = setTimeout(() => {
            sliderNav.classList.add('hidden');
        }, 5000);
    }

    heroSection.addEventListener('mousemove', showNavTemporarily);
    heroSection.addEventListener('touchstart', showNavTemporarily, { passive: true });

    // Auto Play
    function startTimer() {
        slideInterval = setInterval(() => {
            showSlide(currentIndex + 1);
        }, 4000);
    }

    function stopTimer() {
        clearInterval(slideInterval);
    }

    function resetTimer() {
        stopTimer();
        startTimer();
    }

    // Init Slider
    if (slides.length > 0) {
        showSlide(0);
        startTimer();
    }


    // ==========================================
    // PART 2: SALE COUNTER LOGIC 
    // ==========================================

    // Check if the counter elements exist on the page
    const hoursEl = document.querySelector('#hours');
    const minutesEl = document.querySelector('#minutes');
    const secondsEl = document.querySelector('#seconds');

    if (hoursEl && minutesEl && secondsEl) {
        let remainingTime = 70000; // Total seconds

        function updateSaleCounter() {
            if (remainingTime <= 0) return;

            let h = Math.floor(remainingTime / 3600);
            let m = Math.floor((remainingTime % 3600) / 60);
            let s = remainingTime % 60;

            // Add leading zero (e.g., 5 -> 05)
            const pad = (num) => num < 10 ? '0' + num : num;

            hoursEl.innerHTML = pad(h);
            minutesEl.innerHTML = pad(m);
            secondsEl.innerHTML = pad(s);
        }

        // Run immediately then every second
        updateSaleCounter();
        setInterval(() => {
            if (remainingTime > 0) {
                remainingTime -= 1;
                updateSaleCounter();
            }
        }, 1000);
    }


    showNavTemporarily();
});

