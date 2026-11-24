import { makeProj } from "../vendor/thinking-orbs/core.js";
import {
  PARTICLE_COUNT,
  HOLD_SECONDS,
  MORPH_SECONDS,
  createShapes,
  blendShape,
} from "./imagination-shapes.js";

const DEPTH_BANDS = 12;
const ORBIT_DOTS = 36;
const FULL_TURN = Math.PI * 2;

function addDot(path, x, y, radius) {
  path.moveTo(x + radius, y);
  path.arc(x, y, radius, 0, FULL_TURN);
}

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
  const palette = Array.from({ length: DEPTH_BANDS }, (_, i) => {
    const shade = i / (DEPTH_BANDS - 1);
    return {
      light: `rgb(${74 - shade * 47} ${109 - shade * 56} ${95 - shade * 48} / ${0.55 + shade * 0.4})`,
      dark: `rgb(${119 + shade * 93} ${160 + shade * 69} ${144 + shade * 68} / ${0.5 + shade * 0.45})`,
    };
  });
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

  function updateControls() {
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
    updateControls();
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
      height / 2,
      size * 0.405 * breathe,
    );

    // Draw depth groups together to avoid sorting and filling thousands of times per frame.
    const bands = Array.from({ length: DEPTH_BANDS }, () => new Path2D());
    const orbit = new Path2D();
    const radius = Math.max(0.45, size / 380);
    for (let i = 0; i < ORBIT_DOTS; i++) {
      const angle = i / ORBIT_DOTS * FULL_TURN + time * 0.085;
      const wave = Math.sin(angle);
      const [x, y] = project(Math.cos(angle) * 1.27, wave * 0.9, wave * 0.65);
      addDot(orbit, x, y, radius * 0.75);
    }
    context.fillStyle = dark ? "rgb(188 223 207 / 0.27)" : "rgb(55 105 85 / 0.24)";
    context.fill(orbit);
    for (let i = 0; i < PARTICLE_COUNT * 3; i += 3) {
      const [x, y, depth] = project(current[i], current[i + 1], current[i + 2]);
      const shade = Math.max(0, Math.min(1, (depth + 0.5) / 1.1));
      const band = Math.round(shade * (DEPTH_BANDS - 1));
      addDot(bands[band], x, y, radius * (0.65 + shade * 0.63));
    }
    for (const [i, band] of bands.entries()) {
      context.fillStyle = palette[i][dark ? "dark" : "light"];
      context.fill(band);
    }
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
