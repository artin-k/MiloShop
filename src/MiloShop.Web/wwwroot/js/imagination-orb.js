import {
  makeProj,
  finalizeFrame,
  paintFrame,
} from "../vendor/thinking-orbs/core.js";
import {
  PARTICLE_COUNT,
  HOLD_SECONDS,
  MORPH_SECONDS,
  CHAPTERS,
  createShapes,
  blendShape,
} from "./imagination-shapes.js";

function mountImagination(root) {
  const canvas = root.querySelector("canvas");
  let context;
  try {
    context = canvas.getContext("2d", { alpha: true });
  } catch {
    return;
  }
  if (!context) return; // The inline notebook remains visible without Canvas.

  const stage = root.querySelector(".orb-stage");
  const controls = root.querySelector(".orb-controls");
  const buttons = [...root.querySelectorAll("[data-orb-shape]")];
  const pause = root.querySelector("[data-orb-pause]");
  const reducedMotion = matchMedia("(prefers-reduced-motion: reduce)");
  let reduceMotion = reducedMotion.matches;
  const shapes = createShapes();
  const current = shapes[0].slice();
  const source = shapes[0].slice();
  // A small satellite orbit carries the visual language of Thinking Orbs.
  const dots = Array.from({ length: PARTICLE_COUNT + 36 }, () => ({
    x: 0,
    y: 0,
    z: 0,
    r: 1,
    white: 0.2,
    a: 1,
  }));
  let width = 0,
    height = 0,
    dark = false;
  let target = 0,
    morphing = false,
    phaseAge = 0,
    clock = 0;
  let paused = false,
    visible = false,
    pageActive = true,
    lost = false;
  let request = 0,
    previousTime = 0;

  function updateCaption() {
    const chapter = CHAPTERS[target];
    root.querySelector("[data-orb-title]").textContent = chapter.title;
    root.querySelector("[data-orb-description]").textContent =
      chapter.description;
    root.querySelector("[data-orb-count]").textContent = String(
      target + 1,
    ).padStart(2, "0");
    buttons.forEach((button, i) =>
      button.setAttribute("aria-pressed", String(i === target)),
    );
  }

  function beginMorph(index) {
    if (index === target) return;
    source.set(current); // Manual selection mid-morph starts from the actual frame.
    target = index;
    phaseAge = 0;
    morphing = !reduceMotion;
    if (!morphing) current.set(shapes[target]);
    updateCaption();
  }

  function advance(seconds) {
    clock += seconds;
    phaseAge += seconds;
    if (!morphing && phaseAge >= HOLD_SECONDS)
      beginMorph((target + 1) % shapes.length);
    if (morphing) {
      const progress = Math.min(1, phaseAge / MORPH_SECONDS);
      blendShape(source, shapes[target], progress, current);
      if (progress === 1) {
        morphing = false;
        phaseAge = 0;
      }
    }
  }

  function render() {
    if (!width || !height || lost) return;
    context.clearRect(0, 0, width, height);
    const size = Math.min(width, height);
    const still = reduceMotion;
    const time = still ? 0 : clock;
    const breathe = 1 + Math.sin(time * 0.8) * 0.012;
    const project = makeProj(
      0.13 + Math.sin(time * 0.3) * 0.12,
      -0.16 + Math.sin(time * 0.23) * 0.05,
      width / 2,
      height * 0.51,
      size * 0.345 * breathe,
    );

    for (let i = 0; i < dots.length; i++) {
      let x, y, z;
      if (i < PARTICLE_COUNT) {
        const offset = i * 3;
        x = current[offset];
        y = current[offset + 1];
        z = current[offset + 2];
      } else {
        const angle = ((i - PARTICLE_COUNT) / 36) * Math.PI * 2 + time * 0.085;
        x = Math.cos(angle) * 1.37;
        y = Math.sin(angle) * 0.85;
        z = Math.sin(angle) * 0.65;
      }
      const [px, py, depth] = project(x, y, z);
      const light = Math.max(0, Math.min(1, (depth + 0.7) / 1.4));
      const dot = dots[i];
      dot.x = px;
      dot.y = py;
      dot.z = depth;
      dot.r = Math.max(0.55, (size / 550) * (1.25 + light * 0.85));
      dot.white = 0.62 - light * 0.55;
      dot.a = i < PARTICLE_COUNT ? 0.8 + light * 0.2 : 0.25;
      if (i >= PARTICLE_COUNT) dot.r *= 0.65;
    }
    paintFrame(context, finalizeFrame(dots, []), dark);
  }

  function shouldRun() {
    return (
      visible &&
      pageActive &&
      !document.hidden &&
      !paused &&
      !reduceMotion &&
      !lost
    );
  }
  function tick(time) {
    request = 0;
    if (!shouldRun()) return;
    // Time freezes outside the viewport/background; returning never skips shapes.
    advance(previousTime ? Math.min(0.05, (time - previousTime) / 1000) : 0);
    previousTime = time;
    render();
    request = requestAnimationFrame(tick);
  }
  function syncPlayback() {
    if (request) cancelAnimationFrame(request);
    request = 0;
    previousTime = 0;
    pause.disabled = reduceMotion;
    pause.setAttribute("aria-pressed", String(paused));
    pause.setAttribute(
      "aria-label",
      reduceMotion
        ? "کاهش حرکت فعال است"
        : paused
          ? "ادامه پویانمایی"
          : "توقف پویانمایی",
    );
    root.querySelector("[data-orb-play-icon]").hidden = !paused;
    root.querySelector("[data-orb-pause-icon]").hidden = paused;
    if (shouldRun()) request = requestAnimationFrame(tick);
  }
  function resize() {
    const bounds = stage.getBoundingClientRect();
    width = bounds.width;
    height = bounds.height;
    const ratio = Math.min(window.devicePixelRatio || 1, 2);
    canvas.width = Math.round(width * ratio);
    canvas.height = Math.round(height * ratio);
    context.setTransform(ratio, 0, 0, ratio, 0, 0);
    render();
  }
  function readTheme() {
    dark = document.documentElement.dataset.theme === "dark";
    render(); // Repaint even when paused, reduced-motion, or outside the viewport.
  }

  buttons.forEach((button, i) =>
    button.addEventListener("click", () => {
      beginMorph(i);
      // A paused animation stays paused. Manual selection still shows its shape.
      if (paused) {
        current.set(shapes[target]);
        morphing = false;
        phaseAge = 0;
      }
      render();
    }),
  );
  pause.addEventListener("click", () => {
    paused = !paused;
    syncPlayback();
  });
  reducedMotion.addEventListener("change", (event) => {
    reduceMotion = event.matches;
    if (reduceMotion) {
      current.set(shapes[target]);
      morphing = false;
      phaseAge = 0;
    }
    render();
    syncPlayback();
  });
  document.addEventListener("milo:theme", readTheme);
  document.addEventListener("visibilitychange", syncPlayback);
  window.addEventListener("pagehide", () => {
    pageActive = false;
    syncPlayback();
  });
  window.addEventListener("pageshow", () => {
    pageActive = true;
    syncPlayback();
  });
  canvas.addEventListener("contextlost", (event) => {
    event.preventDefault();
    lost = true;
    root.classList.remove("is-ready");
    controls.hidden = true;
    syncPlayback();
  });
  canvas.addEventListener("contextrestored", () => {
    lost = false;
    resize();
    root.classList.add("is-ready");
    controls.hidden = false;
    syncPlayback();
  });
  new ResizeObserver(resize).observe(stage);
  new IntersectionObserver(
    (entries) => {
      visible = entries[0].isIntersecting;
      syncPlayback();
    },
    { threshold: 0 },
  ).observe(stage);

  readTheme();
  resize();
  root.classList.add("is-ready");
  controls.hidden = false;
  syncPlayback();
}

document.querySelectorAll("[data-imagination-orb]").forEach(mountImagination);
