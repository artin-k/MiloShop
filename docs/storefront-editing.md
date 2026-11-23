# MiloShop stationery storefront

The storefront uses server-rendered Razor, small JavaScript enhancements and paper/ink theme tokens. The homepage adds the exact ThreeUI Bookshelf as an isolated React component after the hero. The rest of the site stays in Razor. Its generated assets are committed, so ordinary .NET builds need no Node step. The older Kage scene remains at `/Studio`, with links now directing users to the bookshelf. There is no Next.js migration.

## Where to edit

```text
Controllers/                 Public and Admin HTTP endpoints (no Areas)
ViewModels/                  Form and page data, grouped by feature
Views/
  Home/Index.cshtml           Paper studio homepage, use cases and desk editorial
  Home/_ImaginationOrb.cshtml Animated hero markup, labels and static fallback
  Home/_Bookshelf.cshtml      Exact seven-volume ThreeUI collection and visible controls
  Studio/Index.cshtml         Optional ThreeUI experience and section order
  Studio/_*.cshtml            Original scene sections, product list and CTA
  Shop/                      Catalog and search filters
  Products/Details.cshtml     Crawlable product details and structured data
  Cart/                      Cart and checkout
  Auth/                      Login and sign-up
  Profile/                   Account details and recent orders
  Admin*/                    Product, order, category, coupon and settings forms
  Shared/_Layout.cshtml      Document, metadata and asset references
  Shared/_Header.cshtml      Navigation
  Shared/_Footer.cshtml      Footer
  Shared/_ProductCard.cshtml  Card shared by home and catalog
  Shared/_Comparison.cshtml   Progressive product comparison dialog
  Shared/_Scene.cshtml       Original environmental layers and preloader
  Shared/_AdminNav.cshtml    Management navigation
wwwroot/
  css/base.css               Core layout, local fonts and navigation
  css/stationery.css         Homepage only: hero, use cases and desk editorial
  css/imagination-orb.css    Homepage particle hero, captions and controls
  css/bookshelf.css          Surrounding bookshelf section, themes and responsive controls
  css/milo.css               Typography, RTL, cards, forms and commerce styles
  css/themes.css             Light/dark tokens and scene presentation
  js/theme.js                Before-paint theme preference and toggle
  js/milo.js                 Commerce menu, keyboard support and coupon copy
  js/comparison.js           Loaded on first comparison selection; up to 3 items
  js/imagination-orb.js      Hero animation, controls, theme and browser lifecycle
  js/imagination-shapes.js   Notebook, pen nib, paper plane and star geometry
  js/scene-accessibility.js  Scene failure recovery
  vendor/threeui/            Authored scene CSS, JS, fonts and verified images
  vendor/thinking-orbs/      Original web projection/painter, source credit, MIT license
```

The `Views/Admin*` folders use ordinary MVC view discovery. Controllers are named `AdminProductsController`, `AdminOrdersController`, etc.; their URL prefixes remain `/Admin/Products`, `/Admin/Orders`, etc. Every admin controller retains its Admin role authorization. `/Admin` opens the dashboard.

## Local editing

Start your SQL Server and set the configured database connection, then run:

```powershell
dotnet watch --project src/MiloShop.Web/MiloShop.Web.csproj
```

Edit `.cshtml` text and HTML, save, and refresh the browser. Change styling in `wwwroot/css/milo.css`. Keep the large vendor scene files separate from your everyday component work.

Light/dark colors live in `wwwroot/css/themes.css`. Light uses warm ivory and botanical ink; dark uses green-black surfaces and pale text. The header switch saves `miloshop-theme` in local storage. Without an explicit choice the site follows the operating system, including changes while a tab is open. Theme controls also work on catalog, account and Admin pages. The optional ThreeUI world retains its authored night lighting. Reduced-motion preferences are respected.

Add a homepage section as a Razor partial under `Views/Home` and insert `<partial name="_MySection" model="Model" />` in `Views/Home/Index.cshtml`. Use `.studio-shell`, `.studio-section`, `.studio-heading`, `.button` and semantic color variables. No camera attributes are required. For the separate `/Studio` experience, adapt its existing sections and retain `id`, `data-cam`, `.fg` and reveal attributes. Its partial references use explicit paths because the action belongs to HomeController.

The hero uses a custom browser adaptation of Thinking Orbs, with an open notebook → pen nib → paper plane → star sequence. There are no hero photos or WebGL dependencies. See the [hero guide](imagination-hero.md) to change its words, shapes or timing. Shape labels live in one `CHAPTERS` array; the initial fallback caption also lives in the Razor partial so it works without JavaScript.

The [Bookshelf guide](bookshelf.md) describes the exact source, isolated build, visible controls and performance boundaries. Edit the section's `.cshtml`, its surrounding CSS or the loader, then rebuild if JavaScript changes. Keep the registered vendor files unchanged so their source hashes remain verifiable.

Brand/material filters use actual stored attributes, and catalog queries return at most 24 products per page. Pagination preserves all filters. Comparison stays within the current page and displays up to three products using title, price, brand, material and SKU; absent values say “ثبت نشده”. It loads on the first selection. GSM, nib, ruling and binding require structured catalog fields before they can be advertised or filtered accurately. “Complete the Desk” links separate products; it does not promise bundle discounts or bundle checkout.

Product cards come from the real catalog. Preserve the cart form tag helpers so ASP.NET generates URLs and antiforgery tokens. Forms and product navigation work without WebGL or JavaScript. If rendering fails, the page reveals its content and uses the authored static fallback.

## Template source and configuration

Source: https://threeui.com/landing-pages/kage.html

Registered bundle: https://threeui.com/source-code/kage-landing-page.json

The provided defaults map to Onest headings/body, heading weight 400, body weight 300, primary `#e0231c`, heading size 46, body size 17 and heading letter spacing -0.012em. `milo.css` adds a Persian fallback font and RTL adjustments. The shader pipeline, scene geometry, camera rig, foreground transitions, cloth simulation and adaptive rendering come from the supplied implementation. The world wordmark is changed from KAGE to MILOSHOP, and navigation and copy are adapted for commerce.

`vendor/threeui/kage.original.html` preserves the delivered source. `vendor/threeui/provenance.json` records registered and delivered hashes. ThreeUI's public exporter rewrites the HTML and fonts to storage URLs, so their delivered hashes differ from the registered hashes. All 14 required binary images were verified against their registered SHA-256 hashes; fonts and the Three.js runtime are hosted locally. Local path rewrites and the MiloShop wordmark/navigation changes are generated by the importer.

To refresh the vendor import with Node 20.11+ and curl available:

```powershell
node scripts/import-threeui.mjs
```

Review vendor changes before committing. The importer refreshes vendor assets, not your Razor sections or `milo.css`. This is a native integration of the canonical template rather than an iframe embedding the ThreeUI website.

## Boundaries and current commerce behavior

Catalog reads use Application contracts implemented in Infrastructure. Cart, account, reviews and Admin still contain legacy persistence code; moving folders has not completed every Application use case. Checkout records an order for review and reserves stock; it does not collect an online payment. Recent orders are scoped to the logged-in user. Existing user uploads and catalog media are retained.

## Verification of this integration

See [performance notes](performance.md) for the current measurements and verification. Catalog tests cover attribute filtering, stable pagination and page clamping. Shopping content, links and forms are server rendered; comparison and theme switching are optional JavaScript enhancements.
