const thumbs = new Swiper(".heroThumbs", {
    slidesPerView: 3,
    spaceBetween: 10,
    watchSlidesProgress: true,
});

const hero = new Swiper(".heroSwiper", {
    loop: true,
    effect: "fade",  // Ensure you have the 'fade' effect enabled correctly
    autoplay: { delay: 4500, disableOnInteraction: false },
    navigation: {
        nextEl: ".swiper-button-next",  // Ensure these classes are present in the HTML
        prevEl: ".swiper-button-prev"
    },
    thumbs: {
        swiper: thumbs // Link to the thumbnail swiper
    }
});


const heroEl = document.querySelector(".hero");
let t;

function showNavTemporarily() {
    clearTimeout(t);
    heroEl.classList.add("nav-show");
    t = setTimeout(() => heroEl.classList.remove("nav-show"), 1800);
}

heroEl.addEventListener("mousemove", showNavTemporarily);
heroEl.addEventListener("touchstart", showNavTemporarily, { passive: true });
hero.on("slideChange", showNavTemporarily);
showNavTemporarily();

    let slideIndex = 1;
    let remainingTime = 70000;


function setTime(){
    if(remainingTime == 0) return;
    let h = Math.floor(remainingTime/3600);
    let m = Math.floor((remainingTime%3600)/60);
    let s = (remainingTime%3600)%60;
    document.querySelector('#hours').innerHTML = h;
    document.querySelector('#minutes').innerHTML = m;
    document.querySelector('#seconds').innerHTML = s;
}

setInterval(()=>{
    remainingTime -= 1 ;
    setTime();

} , 1000)



            // setInterval(()=>{
            //     slideIndex +=1;
            //     if(slideIndex==4){
            //         slideIndex = 1;
            //     }
            //     setSlide(`slide${slideIndex}`, slideIndex)
            // } , 4000)