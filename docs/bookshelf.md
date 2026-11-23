# Exact ThreeUI Bookshelf integration

The current Razor homepage and imagination hero remain in place. `Views/Home/_Bookshelf.cshtml` adds the registered seven-volume Bookshelf immediately after the hero, before the stationery use cases. The homepage/footer now link to this section instead of promoting the older `/Studio` scene; that legacy URL is retained.

## Exact source

Source bundle: https://threeui.com/source-code/bookshelf.json

Requested revision: `6ef16625e670b0285bb689bdebffc1d728c6deb1`

All seven registered files are stored byte for byte at their original relative paths under `frontend/bookshelf/vendor/`. Their SHA-256 values are recorded in `vendor/provenance.json`, checked before every build, and covered by source-integrity tests. `.gitattributes` preserves these bytes on Windows checkouts as well. This includes `BookshelfScene.tsx`, the complete renderer, four r165 adapters and the complete shared stylesheet. The covers and walnut texture remain embedded exactly as delivered. No preview recreation, substitute collection or documentation iframe is used.

`frontend/bookshelf/entry.tsx` uses the requested `<div className="shader-frame"><BookshelfScene /></div>` with default props and the shared style import. Build aliases resolve `@designcodeio/threeui` to these verified files. React 18.3.1 renders only this component, with a private `three165` alias pinned to Three.js 0.165.0. It does not replace Razor, introduce Next.js or change shopping routes. The seven authored titles, cover art, foil, cloth, lighting, geometry, room, camera, opening and flexible page-turn systems are preserved.

The complete shared CSS also references Fragment Mono for another ThreeUI component. To keep that path resolvable during bundling without editing the registered CSS, its regular WOFF2 is included from the font publisher, with its OFL license. The supplemental source font hash is `fab90641dca81e9be14be5b047d7892d29ea51e4e2d31355d79a9574a5b74d9a`. Bookshelf itself uses the supplied renderer's system-font fallback. React, Three.js and ThreeUI license notices are retained with the built assets.

## Editing and building

| File | Purpose |
| --- | --- |
| `src/MiloShop.Web/Views/Home/_Bookshelf.cshtml` | Persian heading, instructions, visible controls and section placement markup |
| `src/MiloShop.Web/wwwroot/css/bookshelf.css` | Surrounding section, reserved scene height and responsive control layout |
| `frontend/bookshelf/entry.tsx` | Exact default component usage inside the Razor page |
| `frontend/bookshelf/loader.js` | Lazy loading, accessible control bridge, readiness, retry and focus handling |
| `frontend/bookshelf/vendor/` | Complete verified source; do not hand-edit |
| `frontend/bookshelf/build.mjs` | Source validation, local build, CSS/font bundling and license copying |
| `src/MiloShop.Web/wwwroot/vendor/bookshelf/` | Committed generated browser assets |

Normal `dotnet run`, build and publish use the committed assets and need no Node installation. After changing the frontend integration, run:

```sh
npm ci --prefix frontend/bookshelf
npm run build --prefix frontend/bookshelf
node --test tests/frontend/*.test.mjs
```

Use Node 22.7+ for the combined frontend tests. To reimport this registered revision, run `node scripts/import-bookshelf.mjs`; an optional JSON bundle path supports an offline import. The importer rejects missing files, changed hashes or a different bundle. A future ThreeUI revision needs an intentional source/hash update.

## Behavior and boundaries

The small loader watches the scene container and loads the hashed Three.js/React bundle and authored CSS when it comes within 100 pixels of the viewport. The fixed container height reserves space before download and GPU initialization. A visible start/retry button covers manual loading and failures. No runtime third-party network request is needed.

The source component hides its original DOM controls. The surrounding Persian buttons forward to those exact controls for volume selection, inspection, cover opening/closing, page navigation, reset and return. State is mirrored from the source DOM without changing the renderer. The bridge lets Tab reach visible controls instead of the component's hidden focus loop, and moves focus to the visible return/inspect buttons after mode changes. Canvas pointer/drag/wheel/orbit interactions remain authored behavior. The skip link above the scene provides a way past its wheel carousel.

The surrounding section follows Milo's light/dark tokens. Inside the canvas, the selected book drives the authored room palette; site theme changes do not recolor the original artwork or lighting. The source renderer retains its own reduced-motion, tab-visibility and disposal behavior. It is lazily initialized, but once mounted its original animation loop is preserved; no unsupported offscreen pause API is invented.

Without JavaScript or WebGL, the rest of the store stays usable and the section provides a notice and shop navigation. These are the template's imagined editorial volumes, not new purchasable catalog products. Cart, account, inventory and checkout are unchanged.

## Verification

All 20 .NET tests and 6 frontend tests passed. The latter include the hero's geometry tests and Bookshelf's exact-source, pinned runtime and generated asset-graph checks. Browser verification used the real WebGL renderer, including all seven volumes, carousel, inspection, cover opening, next/previous pages, cover closing and return. Further checks covered desktop/mobile layouts, both site themes, reduced motion, keyboard arrows, Tab/Escape and visible focus transfer, deferred loading, JavaScript/WebGL fallback and the absence of bookshelf code on `/Shop`.

One local Resource Timing capture recorded approximately 916 KB encoded for the complete runtime/embedded-art bundle and 8 KB for the authored CSS. The small initial loader and surrounding section CSS were approximately 2.4 KB and 1 KB. The large bundle was absent from initial viewport requests and loaded near the section; there were no external runtime asset requests. Cloud Chromium used software rendering, so these checks establish functional behavior rather than a production frame-rate guarantee.
