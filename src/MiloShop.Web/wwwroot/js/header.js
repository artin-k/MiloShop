window.addEventListener('scroll', function () {
    const header = document.querySelector('header');

    // Toggle the class 'scrolled' if window.scrollY is greater than 50
    if (window.scrollY > 150) {
        header.classList.add('scrolled');
    } else {
        header.classList.remove('scrolled');
    }
});