# Editing the Razor storefront

The public website uses ordinary MVC folders. Only management screens use `Areas/Admin`.

```text
src/MiloShop.Web/
  Controllers/                 HTTP routes and page preparation
  ViewModels/Home/             Homepage data
  ViewModels/Shop/             Catalog filters and results
  ViewModels/Auth/             Login and registration forms
  ViewModels/Profile/          Account forms and display data
  Views/
    Home/Index.cshtml          Homepage section order
    Home/_Hero.cshtml          Headline, main image, main action
    Home/_Collections.cshtml   Category links
    Home/_Offer.cshtml         Active offer or coupon
    Home/_NewProducts.cshtml   Latest products
    Home/_ShoppingSteps.cshtml Purchase explanation
    Home/_CallToAction.cshtml  Closing section
    Shop/Index.cshtml          Catalog page
    Shop/_Filters.cshtml       Search, category, price, sorting
    Products/Details.cshtml    Public product page
    Auth/                      Login and sign-up
    Profile/                   Customer account
    Shared/_Layout.cshtml      HTML document and asset references
    Shared/_Header.cshtml      Navigation
    Shared/_Footer.cshtml      Footer
    Shared/_ProductCard.cshtml Product card used on home and shop
  wwwroot/
    css/home/milo-landing.css  Shared colors, buttons, cards, home sections
    css/storefront.css         Catalog and product details
    css/header.css             Navigation styling
    css/footer.css             Footer styling
    js/storefront.js           Coupon-copy enhancement
```

## Daily workflow

Start your local SQL Server (or your configured Docker SQL Server) and prepare the environment using the development setup. Keep your database connection environment variable set in your PowerShell terminal, then run:

```powershell
dotnet watch --project src/MiloShop.Web/MiloShop.Web.csproj
```

Open the address printed by ASP.NET. Change markup in a `.cshtml` file, save, and refresh. CSS can also be tried in browser DevTools before saving it. Mobile layouts are controlled by the media queries at the bottom of each stylesheet.

To add a homepage component, create `Views/Home/_MySection.cshtml` and put `<partial name="_MySection" model="Model" />` in `Views/Home/Index.cshtml`. Moving that line changes its position. Keep page asset references in the parent view's `Styles` and `Scripts` sections; partials should contain the component markup.

Use ordinary HTML for static content. `@Model` supplies page data, `@foreach` repeats markup, and `@if` controls whether something appears. Preserve `asp-controller`, `asp-action`, and antiforgery support on purchase forms.

## Data boundaries

Homepage, catalog, search, and product-detail reads use `Application/Catalog/ICatalogQueries`. `Infrastructure/Services/CatalogQueries` implements database access. Controllers prepare web view models and Razor renders the result. Cart, authentication, profile, reviews, and admin still contain legacy persistence access; those use cases have not all been migrated yet.

## URLs

Public URLs remain `/`, `/Shop`, `/Auth/Login`, `/Auth/SignUp`, `/Profile`, `/Profile/Edit`, `/Profile/Terms`, `/Cart`, and `/Cart/Checkout`. Product links use `/products/{id}/{slug}`. An outdated product slug permanently redirects to the correct URL; inactive products return 404. Out-of-stock products keep their page with purchasing disabled.

Product structured data converts displayed toman prices to IRR by multiplying by ten. Keep that conversion aligned with the actual catalog currency.

## Design approach

The Dribbble reference informs whitespace, typography, clear section order, and focused actions. The storefront uses simple sections and one reusable product card. There is no frontend build pipeline or React requirement. Razor already returns the product content as HTML for search engines.
