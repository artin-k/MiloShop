# Frontend direction

MiloShop uses ASP.NET Core MVC/Razor for the storefront and management pages, with optional interactive graphics. All public and management pages use standard Controllers, ViewModels and Views folders. There are no Areas.

Razor renders product content and forms on the server. The default homepage is a lightweight stationery studio; the interactive scene is available at `/Studio`. Shopping pages load neither the scene stylesheet nor its runtime. Moving to Next.js would not inherently improve SEO and is not required.

The homepage now includes the exact ThreeUI Bookshelf after the existing hero. This is an isolated React component with its own pinned Three.js r165 bundle, lazily loaded near the viewport. The rest of the application remains Razor, and .NET builds serve committed assets without requiring Node. See [Bookshelf integration](../bookshelf.md) for exact-source verification, rebuilding and controls.

The source scene, shaders, assets and fonts live under `wwwroot/vendor/threeui`. Everyday changes belong in Razor views, `css/base.css`, `css/stationery.css`, `css/milo.css` and `css/themes.css`. The [editing guide](../storefront-editing.md) explains the structure, source provenance, import command and configuration. [Performance notes](../performance.md) explain asset delivery and measurement limits.

Catalog reads go through Application's `ICatalogQueries` and its Infrastructure implementation. Cart, account, reviews and management writes still need incremental migration into Application use cases. This frontend rebuild does not claim those use cases are fully migrated.
