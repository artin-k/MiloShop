# MiloShop / ThreeUI Kage

The frontend is Razor + the actual Kage HTML/DOM/Three.js source. There is no React or Next.js build. ThreeUI's `KageLandingPage` React component frames the same HTML document; this project renders the adapted document directly so product text and forms are server-rendered and editable in `.cshtml`.

## Where to edit

```text
Controllers/                 Public and Admin HTTP endpoints (no Areas)
ViewModels/                  Form and page data, grouped by feature
Views/
  Home/Index.cshtml           Homepage section order
  Home/_Hero.cshtml           Opening headline, shop CTA, chapter index
  Home/_Story.cshtml          MiloShop story
  Home/_Collections.cshtml    Authored scene windows + actual category links
  Home/_NewProducts.cshtml    Database products + active coupon
  Home/_CallToAction.cshtml   Closing section
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
  Shared/_Scene.cshtml       Original environmental layers and preloader
  Shared/_AdminNav.cshtml    Management navigation
wwwroot/
  css/milo.css               Your typography, colors, RTL and commerce styles
  css/themes.css             Light/dark tokens and scene presentation
  js/theme.js                Before-paint theme preference and toggle
  js/milo.js                 Commerce menu, keyboard support and coupon copy
  js/scene-accessibility.js  Scene failure recovery
  vendor/threeui/            Authored scene CSS, JS, fonts and verified images
```

The `Views/Admin*` folders use ordinary MVC view discovery. Controllers are named `AdminProductsController`, `AdminOrdersController`, etc.; their URL prefixes remain `/Admin/Products`, `/Admin/Orders`, etc. Every admin controller retains its Admin role authorization. `/Admin` opens the dashboard.

## Local editing

Start your SQL Server and set the configured database connection, then run:

```powershell
dotnet watch --project src/MiloShop.Web/MiloShop.Web.csproj
```

Edit `.cshtml` text and HTML, save, and refresh the browser. Change styling in `wwwroot/css/milo.css`. Keep the large vendor scene files separate from your everyday component work.

Light/dark colors live in `wwwroot/css/themes.css`. The header switch saves the preference under `miloshop-theme` in local storage. Without an explicit choice the site follows the operating system, including changes while a tab is open. The light landing page uses paper-colored reading surfaces around the authored night scene; the ThreeUI world itself retains its intended night lighting. Theme controls also work on catalog, account and Admin pages. Reduced-motion preferences are respected by the authored scene and the extra UI.

Add your own section as a Razor partial and insert `<partial name="_MySection" model="Model" />` in `Views/Home/Index.cshtml`. If it should participate in Kage's camera path, adapt an existing section and retain its `id`, `data-cam`, `.fg` and reveal attributes. The camera currently has six authored stops: hero, story, collections, products, closing and footer. Additional ordinary content can live inside one of those stops without changing the camera engine.

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

The .NET test suite passes all 19 tests. Browser checks cover desktop (1440px) and mobile (390px) in both themes, saved/system preferences, mobile menu and Escape, catalog/product/account/Admin routes, antiforgery tokens and the no-JavaScript homepage. A local SQL test completed sign-up, add-to-cart and checkout and verified the order in the account page. The live Three.js renderer was also verified with eight wordmark glyphs and a changing camera rig on scroll; GPU-heavy checks used the source's low-quality query controls for the software-rendered cloud browser.
