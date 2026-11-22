# Thinking Orbs engine primitives

Source: https://github.com/Jakubantalik/thinking-orbs

Pinned revision: `de85557ca220332586d070d8788c0e1d6e877a0d`

`core.js` is the upstream `src/engine/core.ts`, transpiled without geometry changes using esbuild 0.25.12 (`--format=esm --target=es2020`). Original copyright and MIT terms are in `LICENSE`. No React dependency or runtime build step is needed.

Discovered through https://github.com/haplollc/ThinkingOrbs, the MIT-licensed SwiftUI port by Haplo LLC. No Swift source is bundled here. The source library's Swift and original web frame benchmarks do not describe this custom hero's performance.

MiloShop uses the original Fibonacci sampling, orthographic 3D projection, depth sorting and Canvas painter. `js/imagination-shapes.js` supplies original stationery geometry and continuous morphing; `js/imagination-orb.js` supplies browser lifecycle, controls and accessibility. These are custom designs, not unmodified upstream orb presets.

To regenerate `core.js`, download the pinned `src/engine/core.ts` and run:

```sh
npx --yes esbuild@0.25.12 core.ts --format=esm --target=es2020 --outfile=core.js
```

Keep the MIT license and the source attribution banner with the generated file.
