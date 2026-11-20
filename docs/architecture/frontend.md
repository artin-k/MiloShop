# Frontend direction

## Decision

MiloShop will continue with ASP.NET Core MVC and Razor views while the commerce domain, checkout workflow, and visual system are rebuilt.

Razor views fit the current application well because authentication, validation, admin screens, and the storefront already run in one server process. This keeps the redesign focused on customer experience and order correctness. ASP.NET Core MVC remains actively supported in .NET 8.

## When Next.js becomes valuable

Reconsider Next.js after the application exposes a stable HTTP API and one of these product needs becomes concrete:

- a mobile app or multiple independent storefront clients
- extensive client-side product discovery and personalization
- a frontend team that deploys independently from the backend
- server-side React rendering shared across several commerce experiences

At that point, the Application layer can serve both MVC controllers and API endpoints. The new project boundaries keep that migration possible without moving business rules into React.

## UI plan

The Razor redesign will use a small shared design system for typography, color, spacing, forms, buttons, product cards, empty states, and responsive layouts. Storefront pages will be rebuilt around the customer journey from discovery through cart, delivery details, payment, and order confirmation.
