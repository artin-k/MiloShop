# Stationery theme and performance

## Audit and implementation

Previously the homepage presented the Kage temple world before shopping. Every page loaded its 52,269-byte stylesheet and seven font declarations. The homepage loaded Three.js (608,081 bytes) and Kage (173,714 bytes), then scene textures and foregrounds. Those are source sizes before HTTP compression, not measured transfer sizes.

The default homepage now uses paper surfaces, botanical ink, an editorial stationery photograph, journaling/sketching/desk use cases and “Complete the Desk” links. Light and dark modes share semantic tokens. Existing shopping routes and forms remain server rendered. The original authored experience is available at `/Studio`; isolating the GPU scene is an intentional change to initial homepage behavior, preserving access without making shopping wait for it.

Shopping pages load a small foundation sheet instead of Kage CSS. Scripts are deferred except for the small before-paint theme preference script, which prevents a theme flash. Only the homepage loads stationery CSS; only `/Studio` loads Kage assets. Comparison is imported after the first selection. Product lists are bounded to 24 records per page with stable ordering, attribute filters and navigation that preserves filters. Database access stays outside views.

Hero/editorial imagery has responsive 360/736-pixel WebP versions, explicit dimensions and CSS aspect ratios. The hero is preloaded and eager/high priority; product and secondary editorial images are lazy/async. The mobile hero is 24,076 bytes; desktop is 86,772 bytes. Persian WOFF2 is 19,072 bytes, subset with Persian shaping/layout features retained. All local font faces use `font-display: swap`; external origins are unnecessary, so no external preconnect is added. The original TTF remains the source asset.

Public static files use Brotli/Gzip, ETag/Last-Modified and cache headers: versioned assets one year/immutable, other assets one hour. HTML is not publicly cached by this change; cart/account responses stay personalized. Keep essential CSS render blocking to avoid unstyled content and layout shift.

## Verification and measurement limits

The .NET build passed without warnings; all 20 tests passed in Debug and Release. A new catalog test covers brand/material filtering, inactive facet exclusion, pagination without duplicates, stable price ordering and extreme-page clamping. Browser checks used Chromium at 1440 and 390 pixels, both system themes, persisted theme changes, menu/Escape, product and shopping routes and `/Studio?nogl=1`. Further checks covered comparison's native dialog, actual attribute filtering, login, add/remove cart, checkout form, profile, antiforgery fields and a JavaScript-disabled homepage/mobile navigation. The live ThreeUI renderer and camera movement on scroll also passed with software WebGL and the authored low-quality controls. Test data was restricted to an isolated local SQL database.

Computed contrast for primary, secondary, muted and accent text tokens is at least 5.56:1 on their intended reading surfaces in both themes; primary buttons are 8.56:1. Light input borders are 3.59:1 against white. This validates these tokens, not a complete WCAG certification of every legacy screen or uploaded image.

A fresh browser context for each viewport/theme recorded Resource Timing and PerformanceObserver entries on localhost without network or CPU throttling, with three local test products:

| Initial viewport | Resource bodies (encoded, excluding HTML) | Local LCP | Local CLS |
| --- | ---: | ---: | ---: |
| Desktop, 1440 px | about 405 KB | 160–668 ms | 0.00059 |
| Mobile, 390 px | about 87 KB | 128–148 ms | 0.00047 |

Lazy images load as the page is scrolled; these totals are not full-page download budgets. The desktop viewport prefetched more product images. Initial shopping requests contained no Three.js, Kage runtime/CSS, scene textures or comparison module. Static response checks confirmed Brotli and immutable cache headers. The 781,795 bytes of scene JavaScript are avoided by default; that source-size reduction is not a claim of a like-for-like compressed network reduction. No production before/after LCP baseline, real-user INP or 60 fps guarantee is claimed.

Expected effects: prioritized smaller hero media and fewer initial dependencies help LCP; reserved media sizes and early theme selection limit CLS; removing continuous scene work from shopping reduces CPU competition for input. Validate these with deployed Lighthouse/WebPageTest on constrained devices and real-user LCP/CLS/INP monitoring. Browser-local timings do not establish a production Core Web Vitals pass.

## Follow-up work

- Add upload-time image resizing/AVIF/WebP with stored rendition dimensions. Existing product uploads are retained; no fabricated `srcset` URLs are emitted for them.
- Add typed stationery fields (GSM, nib, ruling, binding, format, ink color) and Admin validation before exposing those filters. Current badges/comparison use actual brand, material and SKU, with explicit missing-value labels.
- Consider indexes for measured catalog query plans, and short-lived caching of public facet data. Do not share an EF DbContext across parallel queries.
- Serve versioned public assets through a CDN/reverse proxy with HTTPS and compression configured consistently. Persist Data Protection keys for production instances; ephemeral development keys invalidate cookies on restart.
- Introduce public catalog output caching only with inventory/price invalidation and explicit exclusion of user-specific navigation, antiforgery forms and account/cart responses. Caching the current shared HTML indiscriminately would be incorrect.

## Updating generated media

The committed `images/studio/desk-1-*` and `desk-2-*` WebPs were generated from the repository's existing `images/hero/hero1.jpg` and `hero2.jpg`, respectively, with ImageMagick resize widths 360/736, auto orientation, metadata stripping and WebP quality 82. The Persian subset is generated from `fonts/iran.ttf` with fonttools `pyftsubset --flavor=woff2`, Latin-1, Persian/Arabic, punctuation, currency/arrows and Arabic presentation-form Unicode ranges, retaining `--layout-features='*'`. Generated files are committed; serving/building the .NET application requires neither ImageMagick nor fonttools.
