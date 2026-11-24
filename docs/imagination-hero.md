# Milo's imagination hero

One ruled-paper banner holds the storefront headline, shopping links and a dotted Canvas illustration. The same 7,200 particles morph through an origami bird, reading lamp, folded paper plane, star, paper boat and gift box, with 36 smaller dots orbiting the forms. Each form holds for 2.4 seconds, then transforms over 2.2 seconds. Quintic easing brings the movement to rest at each endpoint. Selecting another shape mid-transition starts from the currently visible positions.

Milo reuses the 3D projection from Jakub Antalik's Thinking Orbs web engine. The storefront defines its own stationery geometry and draws the dots in Canvas 2D with 12 depth bands and a separate orbit path. The local vendor directory records the pinned source revision and MIT license. [Original web engine](https://github.com/Jakubantalik/thinking-orbs) · [SwiftUI port](https://github.com/haplollc/ThinkingOrbs)

## Files to edit

| File under `src/MiloShop.Web` | Purpose |
| --- | --- |
| `Views/Home/Index.cshtml` | Hero headline and shopping links |
| `Views/Home/_ImaginationOrb.cshtml` | Illustration stage, icon buttons, accessible labels and inline SVG fallback |
| `wwwroot/css/stationery.css` | Shared hero surface, placement and viewport sizing |
| `wwwroot/css/imagination-orb.css` | Component layout, responsive sizing and controls |
| `wwwroot/js/imagination-shapes.js` | `HOLD_SECONDS`, `MORPH_SECONDS`, geometry and easing |
| `wwwroot/js/imagination-orb.js` | Drawing, playback, manual selection, preference/visibility handling |
| `wwwroot/vendor/thinking-orbs/` | Pinned upstream primitives, attribution and MIT terms |

All six shapes must retain the same particle count and point identity. The sampler divides the particle budget across the faces and creases of each form. Coordinates are normalized and scale with the stage. Edit the Razor views for headline and button labels, the stylesheets for layout and the palette in `imagination-orb.js` for light and dark particle colors.

## Playback and accessibility

- Icon buttons select shapes and pause or resume playback. Native button labels and pressed states expose the controls to assistive technology.
- While paused, manual selection displays a complete static form. Resuming starts its normal hold period.
- Reduced motion starts playback paused. Shape buttons show static forms, and the play button starts motion on request. Changes to the preference while the page is open update playback.
- Frames stop while the stage is outside the viewport, the tab is hidden, or the page is in the back/forward cache. Returning resumes rather than skipping through elapsed wall time.
- Without JavaScript, a working Canvas context, or a successful module load, the inline stippled bird stays visible. Nonfunctional controls stay hidden. Rendering-context loss also restores the fallback.
- Canvas is decorative and hidden from assistive technology. Shopping links and content remain server rendered.

CSS reserves space for the stage and controls before initialization. Canvas pixel density is capped at 2×. Only the homepage loads these modules and styles. Razor versions both the animation entry point and the geometry module so deployments load the matching files.

## Checks

Run the geometry regression tests with Node 22.7+ (Node 24 was used during development):

```sh
node --test tests/frontend/imagination-shapes.test.mjs
```

The tests verify deterministic finite geometry, equal particle identities, exact endpoints including the loop, interpolation and interrupted transitions. The .NET suite covers server behavior. Chromium checks covered both themes, the headline and controls fitting within desktop and mobile viewports, autoplay, manual selection, pause, offscreen suspension, mobile menu links, the theme toggle and navigation without JavaScript.
