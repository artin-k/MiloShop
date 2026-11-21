# Frontend direction

## Decision

MiloShop will continue with ASP.NET Core MVC and Razor views while the commerce domain, checkout workflow, and visual system are rebuilt.

Razor views fit the current application well because authentication, validation, admin screens, and the storefront already run in one server process. This keeps the redesign focused on customer experience and order correctness. ASP.NET Core MVC remains actively supported in .NET 11.

This is also the right SEO choice for the current product. Razor returns complete semantic HTML from the server, just as a server-rendered Next.js page does. Search performance will depend more on metadata, structured data, useful content, image performance, internal linking, and Core Web Vitals than on whether the templates use C# or React.

Moving now would require a public commerce API, a second authentication integration, duplicated validation and caching decisions, and another deployment pipeline. That cost is not justified while the catalog, cart, checkout, and order lifecycle are still being completed.

## When Next.js becomes valuable

Reconsider Next.js after the application exposes a stable HTTP API and one of these product needs becomes concrete:

- a mobile app or multiple independent storefront clients
- extensive client-side product discovery and personalization
- a frontend team that deploys independently from the backend
- server-side React rendering shared across several commerce experiences

At that point, the Application layer can serve both MVC controllers and API endpoints. The new project boundaries keep that migration possible without moving business rules into React.

## Landing-page direction

The November 2026 redesign uses the Agentix Dribbble presentation as mood and composition research, not as a template. The transferable principles are its calm narrative pacing, oversized typography, modular content, open space, and single-purpose closing call to action. MiloShop's result is intentionally distinct: an RTL editorial stationery identity, a warm paper palette, original product imagery, commerce components, and a customer journey from discovery to cart.

The landing page tells this story:

1. a clear promise and two useful actions
2. dynamic product categories
3. a real offer or coupon when one is active
4. new products with working add-to-cart forms
5. a short explanation of the purchase flow
6. one focused final call to action

The shared layout supplies canonical URLs, page descriptions, Open Graph metadata, and an optional head section. The home page adds `WebSite` and `SearchAction` structured data.

## UI plan

The Razor redesign uses a small shared design system for typography, color, spacing, forms, buttons, product cards, empty states, and responsive layouts. Storefront pages will continue to be rebuilt around the customer journey from discovery through cart, delivery details, payment, and order confirmation.
