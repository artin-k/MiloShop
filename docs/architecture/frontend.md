# Frontend direction

MiloShop uses ASP.NET Core MVC/Razor and the authored ThreeUI Kage HTML/DOM/Three.js template. All public and management pages use standard Controllers, ViewModels and Views folders. There are no Areas.

Razor renders product content and forms on the server. The interactive scene enhances the homepage; shopping does not depend on WebGL. Moving to Next.js would not inherently improve SEO and is not required by this template's canonical implementation.

The source scene, shaders, assets and fonts live under `wwwroot/vendor/threeui`. Everyday changes belong in small Razor partials and `wwwroot/css/milo.css`. The [editing guide](../storefront-editing.md) explains the structure, source provenance, import command and configuration.

Catalog reads go through Application's `ICatalogQueries` and its Infrastructure implementation. Cart, account, reviews and management writes still need incremental migration into Application use cases. This frontend rebuild does not claim those use cases are fully migrated.
