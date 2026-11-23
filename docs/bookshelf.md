# Milo stationery category shelf

The carousel after the hero is now a storefront category selector, adapted from the ThreeUI Bookshelf engine. The demo's AI-tool volumes and cover atlas are replaced with Milo stationery artwork, Persian category names and useful collection copy. The original room, materials, foil, carousel, inspection, opening, orbit and flexible page-turn mechanics remain.

## Real catalog data

`ICatalogQueries.GetHomeAsync` returns `CategoryCollection` records from the stored categories. An aggregate query counts active products with stock greater than zero, matching the default Shop availability filter. Counts are independent of the homepage's eight-product limit. Categories with no available products remain visible and say the collection is being completed.

The Razor partial serializes IDs, names, descriptions, counts and generated `/Shop?categoryId=...` URLs using ASP.NET's HTML-safe JSON helper. It also renders native links for every category. Selection updates the main “دیدن محصولات” link, description and count. Following the link opens the actual filtered Shop page; the books themselves are collection artwork, not new catalog products.

Manage category names/descriptions in `/Admin/Categories` and assign products through `/Admin/Products`. The next homepage request reflects those changes. There are no hardcoded category IDs, fixed seven-category limit, database seeds or schema changes in this feature. With one category the carousel arrows are disabled. With no categories the section shows a simple store link without initializing WebGL. The category directory and shopping links work without JavaScript or WebGL.

## Editing guide

| File | Purpose |
| --- | --- |
| `src/MiloShop.Web/Views/Home/_Bookshelf.cshtml` | Heading, safely serialized category data, shopping links, controls and empty state |
| `src/MiloShop.Web/wwwroot/css/bookshelf.css` | Section layout, responsive frame, category summary and directory |
| `frontend/bookshelf/category-data.js` | Stationery profiles, stable cover colors, dimensions, copy and light/dark room palettes |
| `frontend/bookshelf/category-art.js` | Original Canvas stationery symbols, Persian covers, spine/foil and interior pages |
| `frontend/bookshelf/adapt-renderer.mjs` | Build-time adaptation of the verified engine's demo data and texture painters |
| `frontend/bookshelf/loader.js` | Lazy loading, selected-category shopping link, visible controls, readiness and retry |
| `frontend/bookshelf/entry.tsx` | Isolated React component inside the Razor page |
| `frontend/bookshelf/vendor/` | Unmodified, licensed ThreeUI reference snapshot |
| `src/MiloShop.Web/wwwroot/vendor/bookshelf/` | Generated browser assets committed for ordinary .NET builds |

Profiles infer only the illustration style from category names: notebook, nib, palette/brush, planner, ruler, gift or school paper. The actual category identity/name always comes from the database. Unknown names receive notebook artwork. Cover colors use stable category IDs so a rename or sort change does not randomly recolor the collection. Category-specific details are editable without digging into the renderer. Persian lettering uses the existing local `MiloPersian` font; no external image or font request is needed.

Normal `dotnet run`, build and publish use committed browser assets. After changing frontend JavaScript/artwork, run:

```sh
npm ci --prefix frontend/bookshelf
npm run build --prefix frontend/bookshelf
node --test tests/frontend/*.test.mjs
```

Use Node 22.7+ for the combined frontend tests. Changes to Admin categories require no frontend rebuild.

## Engine provenance and adaptation

Original source: https://threeui.com/source-code/bookshelf.json

Reference revision: `6ef16625e670b0285bb689bdebffc1d728c6deb1`

All seven registered source files remain byte-exact under `frontend/bookshelf/vendor/`, with hashes checked before every build. `.gitattributes` preserves them on Windows as well. They are now a reference snapshot: the browser renderer is intentionally adapted in response to the stationery/category requirement, rather than being an unchanged demo.

The esbuild plugin uses Acorn's syntax tree to replace the demo's `BOOKS` data, atlas and texture painters with category data and original Milo artwork. It removes the embedded demo atlas from the browser bundle and supplies local-font loading and live theme updates. Geometry, shaders/material construction, carousel, camera, cover/page physics and disposal come from the source. The adapter fails if the expected source structure changes. Reimporting a newer upstream version requires deliberate hash and adapter review.

The complete shared stylesheet is retained. Its unrelated Fragment Mono reference resolves to a local publisher font with its OFL license. React, Three.js r165, ThreeUI and font licenses remain included. The application continues to use Razor for its pages and forms; React is isolated to this component, with no Next.js migration or documentation iframe.

## Loading, accessibility and themes

The large renderer loads only as the section approaches the viewport, with reserved space and a loading placeholder. Native Persian buttons proxy the original hidden controls, and selected category details use normal text/links outside the canvas. Tab can reach those visible controls, and mode changes move focus to the visible return/inspect button. The category directory remains accessible in fallback states. The original pointer, drag, wheel, keyboard, reduced-motion and tab-visibility behavior is retained.

Both the surrounding section and the scene's room palette now follow the site's light/dark preference. Physical stationery covers keep their own stable colors. The preference listener is removed when the renderer is disposed. The renderer still uses the source's continuous animation once mounted; lazy initialization is not a claim of automatic offscreen suspension.

## Verification

Frontend tests cover source provenance, generated assets, variable category counts, stable IDs/colors, category text/counts, light/dark palettes and the build adaptation. .NET tests verify empty catalogs and that the category counts match the actual Shop filter, including hidden/out-of-stock exclusions and inventories larger than the home product preview.

Browser checks use the real WebGL renderer and an isolated local SQL database: one and multiple categories, selected-category URLs and counts, actual filtered results, opening/turning/returning, keyboard, both themes, mobile framing, lazy loading and JavaScript/WebGL fallback. Software-rendered cloud browser results establish functionality; they do not guarantee production frame rates.
