# Milo's imagination hero

The homepage replaces its photograph with one dotted 3D Canvas illustration. The same 900 particles morph through an open notebook, fountain-pen nib, folded paper plane and star, surrounded by 36 subtle orbit dots. Each form holds for 4.4 seconds, then transforms over 2.8 seconds. Quintic easing brings the movement to rest at each endpoint. Selecting another chapter mid-transition starts from the currently visible positions, preserving continuity. This is actual geometry interpolation, not a slideshow or image crossfade.

The supplied [ThinkingOrbs repository](https://github.com/haplollc/ThinkingOrbs) is a SwiftUI port for Apple platforms. Its credits identify Jakub Antalik's [original web engine](https://github.com/Jakubantalik/thinking-orbs). Milo reuses that engine's Fibonacci sampler, 3D projection, depth sorting and Canvas painter, with original stationery geometry and browser controls. React, Swift, Three.js and a frontend build step are unnecessary. The local vendor directory records the pinned source revision and MIT license. These custom forms are not upstream presets.

## Files to edit

| File under `src/MiloShop.Web` | Purpose |
| --- | --- |
| `Views/Home/Index.cshtml` | Hero headline, supporting text and shopping links |
| `Views/Home/_ImaginationOrb.cshtml` | Illustration frame, accessible buttons, initial caption and inline SVG fallback |
| `wwwroot/css/imagination-orb.css` | Component layout, responsive sizing and controls |
| `wwwroot/js/imagination-shapes.js` | `CHAPTERS` copy, `HOLD_SECONDS`, `MORPH_SECONDS`, geometry and easing |
| `wwwroot/js/imagination-orb.js` | Drawing, playback, manual selection, preference/visibility handling |
| `wwwroot/vendor/thinking-orbs/` | Pinned upstream primitives, attribution and MIT terms |

All four shapes must retain the same particle count and point identity. The current shape generators are tuned for 900 particles; changing the count requires updating every generator, not just the constant. Coordinates are normalized and scale with the stage. Routine edits need only `.cshtml`, captions or CSS. Font and frame colors follow existing light/dark tokens; the engine mirrors grayscale ink for dark surfaces.

## Playback and accessibility

- Controls select chapters and pause/resume. A selected chapter's caption is displayed as its morph begins. Captions do not announce continuously through a live region.
- While paused, manual selection displays a complete static form. Resuming starts its normal hold period.
- Reduced motion shows a static form, disables autoplay/pause and keeps manual selection available. Changes to this preference while the page is open are supported.
- Frames stop while the stage is outside the viewport, the tab is hidden, or the page is in the back/forward cache. Returning resumes rather than skipping through elapsed wall time.
- Without JavaScript, a working Canvas context, or a successful module load, the inline dotted notebook stays visible. Nonfunctional controls stay hidden. Rendering-context loss also restores the fallback.
- Canvas is decorative and hidden from assistive technology; the figure caption and labelled native buttons provide its accessible equivalent. Shopping links and content remain server rendered.

The stage has a reserved aspect ratio, and the controls reserve space before initialization to avoid an extra layout jump. Canvas pixel density is capped at 2×. There are no hero image requests, WebGL shaders, filters or third-party network calls. Only the homepage loads these modules/styles; `/Studio` and shopping routes retain their separate behavior. Historical photo-hero performance figures in `performance.md` should not be interpreted as measurements of this version.

## Checks

Run the geometry regression tests with Node 22.7+ (Node 24 was used during development):

```sh
node --test tests/frontend/imagination-shapes.test.mjs
```

They verify deterministic finite geometry, equal particle identities, exact endpoints including the loop, interpolation and interrupted transitions. The .NET suite covers server behavior. Browser verification covers both themes at 1440/390 pixels, autoplay, all forms, interrupted morphing, pause, live theme changes, reduced motion including preference changes, offscreen suspension, resize and JavaScript/Canvas fallback. Local frame measurements describe only the cloud Chromium run; no universal frame-rate guarantee is made.

One unthrottled local Chromium run recorded 529 rendered frames: median Canvas render time 1.9 ms and 95th percentile 3.6 ms, with lifecycle checks confirming no new frames while paused/offscreen or in reduced motion. The hero's four stylesheet/module responses totaled approximately 8.1 KB encoded, replacing the previous 86.8 KB desktop hero photo. Its observed CLS was 0.00053. These are local checks with instrumentation, not real-user metrics or a complete before/after page benchmark.
