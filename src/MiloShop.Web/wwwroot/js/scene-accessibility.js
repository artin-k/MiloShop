// Commerce must remain usable when WebGL or JavaScript assets fail to initialize.
document.documentElement.classList.add('scene-enhanced');
const recoverScene = () => {
    document.documentElement.classList.add('no-webgl');
    document.body.classList.remove('is-locked');
    document.getElementById('pre')?.classList.add('done');
    document.querySelectorAll('[data-rv], .mask-line').forEach(el => el.classList.add('rv-in'));
};
// The authored boot already handles WebGL failures. A timer would incorrectly
// hide a valid scene while a slow GPU is still compiling the original shaders.
window.addEventListener('error', event => {
    if (event.filename?.includes('/vendor/threeui/kage.js')) recoverScene();
    if (event.target instanceof HTMLScriptElement && event.target.src.includes('/vendor/threeui/')) recoverScene();
}, true);
