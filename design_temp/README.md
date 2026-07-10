# Handoff: Uni-Shop — Aetheric Command Light Redesign

## Overview
A complete visual redesign of Uni-Shop (Alfajr For Trade), an Egyptian technology e-commerce site distributing DOOGEE, JisuLife and Dreame hardware. The redesign shifts the previous teal-and-dark aesthetic to the **Aetheric Command Light** design system — an airy, glassmorphic, professional treatment inspired by high-tech command-and-control interfaces. The design covers Home, Products catalog, Product Detail, Cart and Checkout.

## About the Design Files
The files in this bundle are **design references authored in HTML/CSS/vanilla JS** — high-fidelity prototypes that demonstrate the intended look, layout, and interaction of every page. They are **not production code to copy directly**.

Your task is to **recreate these HTML designs inside the target codebase's environment** (React, Vue, Next.js, Nuxt, SvelteKit, Astro, etc.) following the codebase's established component patterns, routing, state management, and styling conventions. If no environment exists yet, choose the framework most appropriate for the project (a modern React or Next.js e-commerce stack is the natural fit) and implement the designs there.

Use the HTML as your source of truth for **visual specifications and behavior**, and re-express it using idiomatic components in the target stack.

## Fidelity
**High-fidelity (hifi).** Every screen is pixel-precise with final colors, typography, spacing, radii, elevation and interaction states. The developer should recreate the UI pixel-perfectly using the codebase's existing libraries and patterns. Every design token below is authoritative — do not invent new colors, radii or type scales.

## Screens / Views

### 1. Home (`index.html`)
- **Purpose:** Land the customer, introduce the brand, showcase featured products, communicate credibility and provide contact.
- **Sections (in order):**
  1. **Sticky glass nav** (see Global Components)
  2. **Editorial hero** — 2-column grid (1.15fr / 1fr). Left: chip row ("LIVE · Best Seller Q3" + "New Arrivals · 24"), oversized headline `The command deck for modern tech.` (last two words in `--primary` with a `--accent` underline), body paragraph, primary + ghost CTAs, and a 3-column stat row (10+ Years / 50K+ Orders / 200+ SKUs) separated by 1px verticals. Right: circular orbit visualization — 3 concentric dashed/solid rings rotating at different speeds around a glass-blur central hub containing the current featured product's SVG mark. A floating white product card ("F40 Portable Fan · EGP 1,299") drifts on a 6s `floaty` animation, and a bottom-left "99.8% Uptime · Egypt Warehouse" glass panel with a sparkline SVG.
  3. **Brand marquee** — thin horizontal strip with `translateX` scroll animation, brand names + qualifiers separated by cyan dots (DOOGEE · JISULIFE · DREAME · CERTIFIED WARRANTY · ORIGINAL PRODUCTS · FAST DELIVERY).
  4. **Featured Products** — section header (left title + right description & "View All" secondary button), pill tab row (All / Tablets / Fans / Webcams — active tab = navy pill with white text), then a 4-column product-card grid (see Components).
  5. **About** — 2-column: left has eyebrow "About Us · 03", oversized headline "Delivering the best technology for a **better life**.", body copy, and a 3-column stat card row (10+ Years Exp., 50K+ Happy Clients, 200+ Products) — each stat card is a white card with a 3px cyan accent bar bleeding from its top-left. Right: a navy "Why Uni-Shop?" panel (glassy grid pattern + cyan/teal radial glows) listing 6 numbered features with cyan mini icons and thin dividers.
  6. **Mission & Vision** — full-bleed navy section (rounded 2rem, `--r-2xl`) inset from the page edges. Centered eyebrow + headline ("Building the **tech future** in Egypt."), then two glass cards side-by-side ("Our Mission" / "Our Vision") each with a cyan-tinted icon tile, top gradient hairline, and a corner tag "01" / "02".
  7. **Contact** — section header (eyebrow "Contact · 05" + "We're always here to help.") then a 2-column grid: left = white info card with 4 rows (Address / Phone / Email / Working Hours), a stylized map mock (grid pattern + pulsing pin), and social pills. Right = white form card ("Send us a message") with full-name / email / phone / message fields and a primary submit button.
  8. **Footer** (see Global Components)

### 2. Products (`products.html`)
- **Purpose:** Browse the full catalog with brand + category + price filtering.
- **Sections:**
  - Breadcrumb (`Home / Products`)
  - **Navy hero panel** (rounded 2rem) with eyebrow "All Products", headline "The complete catalog at a glance.", subtitle, and a horizontal brand pill row (All / DOOGEE / JisuLife / Dreame with count badges). Below a horizontal rule: a 4-tile stats strip (In Stock · Brands · Price Range · Warranty).
  - **Catalog body** — 2-column grid (260px sticky sidebar / 1fr main). Sidebar contains a "Filter Results" white card with collapsible groups (Category, Brand, Price Range with a two-thumb range slider, Status), then a navy "Need Help?" mini CTA card. Main column has a toolbar (live "Showing X of Y products" chip, sort dropdown, grid/list view toggle) then a 3-column product-card grid.

### 3. Product Detail (`product.html`)
- **Purpose:** Sell a single product with imagery, price, specs, and add-to-cart.
- **Sections:**
  - Breadcrumb (`Home / Products / <Brand> / <Name>`)
  - **Hero** — 2-column (1.15fr / 1fr). Left: sticky gallery — a large square media stage (grid background masked to a radial fade, product SVG mark centered with 6s float animation, cyan corner brackets, and a top-right "Studio · 360°" tag with pulsing cyan dot), plus 4 thumbnail tiles beneath. Right: brand pill + hot/new badge + SKU, oversized name, tagline, 5-star rating with reviews count, a price card (large primary-color EGP price + "0% for 12 months" green chip + green pulsing "In stock · Ships within 24h · Cairo warehouse"), a CTA row (qty stepper / primary Add-to-Cart button showing total / circular wishlist button), and a 3-tile trust strip (Warranty / Delivery / Support).
  - **Specs + Description** — 2-column: left = navy "Specifications" card with cyan eyebrow, subtitle "All measurements verified per manufacturer QA batch." and a key/value list. Right = white "What's in the box" card with description + a 2×2 feature grid (warranty / free shipping / return policy / installments).
  - **Related products** — header + 3-card grid.

### 4. Cart (`cart.html`)
- **Purpose:** Review items, adjust quantities, view live totals.
- **Sections:**
  - Breadcrumb (`Home / Shopping Cart`)
  - Header (eyebrow "Order · Draft", H1 "Shopping Cart", right-aligned "← Continue Shopping" link)
  - **Layout** — 2-column (1fr / 380px sticky summary).
    - Left: white **Items table card** with a labeled column header row, then one row per line item (72px thumbnail, brand + name + "In stock · SKU" tag / qty stepper / price with `EGP X each` sub-line / trash icon), and a footer row (Clear cart · "N items · Subtotal EGP X").
    - Right: **Order Summary** card — promo code input, subtotal / shipping (green "FREE" when total ≥ 5,000 EGP) / tax rows, a large `--primary` total, primary "Proceed to Checkout" button with gradient hover, a trust list (SSL / free shipping / returns), and a payment methods strip (VISA / MC / CASH / INSTA).
  - **Empty state** — centered card with circular dashed halo icon, "Your cart is empty" title, copy, and a primary "Browse Products" CTA.

### 5. Checkout (`checkout.html`)
- **Purpose:** Collect delivery info, method, and payment; place order.
- **Sections:**
  - Breadcrumb (`Home / Cart / Checkout`)
  - Header (eyebrow "Order · Placing", H1 "Checkout")
  - **Progress steps bar** — pill container with 4 steps (Cart ✓ / Details • active / Payment / Confirmation). Active step: primary-color circle with white text and `0 0 0 4px rgba(0,122,153,.15)` ring. Done step: green.
  - **Layout** — 2-column (1fr / 380px sticky summary).
    - Left: three stacked form cards each with a numbered step badge (`1`, `2`, `3`), a top-left 40×3px cyan accent bar, and a description subtitle:
      1. **Delivery Information** — Full Name / Phone + Email / Governorate select + City / Detailed address / Notes textarea.
      2. **Delivery Method** — radio-styled option cards (Standard FREE / Same-day EGP 120 / Store pickup FREE) each with icon tile.
      3. **Payment Method** — 2×2 grid of option cards (Cash on Delivery / Card / Bank Installments / Mobile Wallet).
    - Right: **Order Summary** — line items with 48px thumbnails and floating qty badges, subtotal rows, large primary total, and a primary "Place Order" button with SSL trust footnote.

## Global Components

- **Nav (`navHTML` in `app.js`):** Sticky pill container, `72% white + 20px blur` (glassmorphism). Left: logo mark (10px rounded 34×34 navy square with cyan "U" and inner radial glow) + wordmark "Uni-**Shop**". Center: 4 nav links (Home / About / Products (with chevron) / Contact) — active link uses navy tint + a cyan glowing dot underneath. Right: 220px search button with `⌘K` kbd, dark circular cart icon with cyan `--accent` badge, "EN" language pill, and a mobile hamburger below 968px.
- **Footer:** Navy background with cyan/teal radial glows. 4-column grid: brand block (logo + tagline + 3 social circles) / Explore / Brands / Support. Bottom bar: copyright + "All systems operational · Cairo, EGY" status pill with green pulsing dot.
- **Product Card:** White card, 1px `--outline-variant` border, 2rem radius. Media area = aspect-ratio 1.05 with light violet gradient + faint 24px grid + product SVG mark. Badge in top-left, circular quick-view button in top-right (opacity 0 → 1 on hover). Info block: brand small-caps + SKU tabular / product name H3 / short desc / bottom-anchored price row with `EGP` price + dark pill "Details" button + circular cyan add-to-cart button. Hover: `translateY(-6px)`, cyan-tinted border, 4px cyan glow ring, and the SVG mark scales + rotates -2°.
- **Buttons:**
  - `.btn-primary` — `--primary` (#007A99) fill, white text, pill, 4/20/-4 cyan-tinted shadow. Hover: `translateY(-1px)` + cyan glow ring + subtle inner gradient sheen.
  - `.btn-secondary` — transparent, 1px primary border, primary text. Hover: primary fill + shadow.
  - `.btn-ghost` — 60% white + blur, 1px `--outline-variant`, navy text.
  - `.btn-dark` — navy fill, white text.
  - Sizes: `.btn-sm` (10/18 · 13px), default (14/24 · 14px), `.btn-lg` (18/32 · 15px).
- **Chip:** Cyan-tinted background, pulsing cyan dot on the left. Used for status indicators.
- **Badges:** `.badge-new` (navy + cyan), `.badge-hot` (orange gradient), `.badge-4k` (cyan + navy), `.badge-rugged` (navy + white). All small-caps, letter-spacing 0.1em.
- **Toast:** Bottom-center navy pill with cyan dot, 3s auto-dismiss. Triggered via `showToast(msg)`.

## Interactions & Behavior

- **Cart persistence:** `localStorage` under `unishop-cart`. All pages listen for `cart:change` to re-render.
- **Add to cart:** Any `+` icon on a product card calls `addToCart(id, qty=1)` → shows toast → updates all `[data-cart-count]` badges.
- **Filter/tab/sort on Products page:** Pure client-side filtering by brand and category set + sort by price asc/desc.
- **Home Featured tabs:** Filter by category, showing max 4 cards.
- **Reveal on scroll:** `.reveal` elements start at `opacity:0; translateY(20px)` and animate to `in` state via an `IntersectionObserver` (0.05 threshold, 200px bottom rootMargin). A 3s safety timer forces `.in` on all elements as a fallback.
- **Nav mobile menu:** Below 968px the pill collapses to logo + cart + hamburger; the hamburger toggles a full-width dropdown.
- **Hero orbit:** Two concentric dashed rings spin at 60s and 40s (reverse). Central glass hub has a conic-gradient shimmer sweeping in 8s.
- **Product card hover:** 350ms cubic-bezier `translateY(-6px)`, border → cyan, glow ring, SVG scales `1.08` + rotates `-2deg`.
- **Product detail thumbnails:** Clicking a thumb marks it active with a cyan ring; the main media is the same SVG mark (thumb variety is stylistic in the mock — in production, hook these up to real image variants).
- **Cart quantity steppers:** `updateCartItem(id, qty)` — setting `qty ≤ 0` removes the line item.
- **Checkout radios:** Clicking a delivery or payment card marks it active and de-selects siblings (single-select group).
- **Form submit:** Contact and Checkout forms `preventDefault`, show a toast, and (for Checkout) clear the cart then redirect to home after 1.5s.
- **Map pin ping:** 2s infinite `ping` keyframe (scale 0.5 → 2, opacity 1 → 0).
- **Marquee:** 30s linear infinite `translateX(0 → -50%)` — duplicate the item list twice to make the loop seamless.

## State Management

- **Cart state** — array of `{ id, qty }`, persisted to `localStorage`. Expose: `getCart`, `setCart`, `addToCart`, `updateCartItem`, `removeFromCart`, `clearCart`, `cartCount`, `cartTotal`. Fire a `cart:change` event on every mutation so all pages/UIs re-render.
- **Filters (Products page)** — `activeBrand: string`, `activeCats: Set<string>`, `sortMode: 'featured' | 'price-asc' | 'price-desc' | 'newest'`. Recompute the visible list on every change; sync between brand pills (hero) and brand checkboxes (sidebar).
- **Product detail** — `qty` local to the page (clamp 1–10). No global state beyond cart.
- **Checkout** — active delivery option + active payment option are pure DOM class toggles in the mock; in production, lift them into form state so `Place Order` submits a validated payload.
- **Data fetching (production):** The mock hardcodes `PRODUCTS` in `app.js`. In the real implementation, fetch this from the products API, keyed by `id`. `product.html` reads `?id=` from the URL; `products.html` reads `?brand=` for deep-linked brand filters.

## Design Tokens

All tokens are declared as CSS custom properties in `styles.css`. **Copy these verbatim** into the target design system's tokens.

### Colors (Aetheric Command Light)

**Surfaces**
- `--surface` `#faf8ff` (page background)
- `--surface-bright` `#ffffff` (cards)
- `--surface-container-low` `#f2f3ff` (input/pill backgrounds)
- `--surface-container` `#eaedff`
- `--surface-container-high` `#e2e7ff`

**Text**
- `--on-surface` / `--navy` `#0f172a` (primary text)
- `--on-surface-variant` `#3f484d` (body copy)
- `--on-surface-muted` `#64748b` (secondary/label text)
- `--navy-2` `#1e293b` (deep panels)
- `--navy-3` `#334155` (nav links)

**Outlines**
- `--outline` `#6f797e`
- `--outline-variant` `#e2e8f0` (default 1px borders)
- `--outline-soft` `#f1f5f9`

**Brand**
- `--primary` `#007a99` — high-contrast blue (buttons, links, price)
- `--primary-hover` `#006079`
- `--accent` `#00d4ff` — vibrant tech blue (glows, dots, hover accents; never used for body text on white)
- `--accent-glow` `rgba(0, 212, 255, 0.35)`
- `--accent-soft` `#e0f7ff`
- `--secondary` `#00677e`

**Semantic**
- `--error` `#ba1a1a`
- `--success` `#059669`
- `--warning` `#d97706`

**Effects**
- `--shadow-ambient` `0 20px 60px -20px rgba(15, 23, 42, 0.08)`
- `--shadow-elev` `0 2px 8px -2px rgba(15, 23, 42, 0.06), 0 20px 40px -20px rgba(15, 23, 42, 0.12)`
- `--shadow-hover` `0 4px 16px -4px rgba(0, 122, 153, 0.15), 0 30px 60px -20px rgba(0, 122, 153, 0.2)`
- `--glass-bg` `rgba(255, 255, 255, 0.72)`

### Typography
- **Font family:** `Sora` from Google Fonts (weights 300 / 400 / 500 / 600 / 700 / 800). Fallback stack: `system-ui, -apple-system, sans-serif`.
- **Headline XL:** `clamp(48px, 6.5vw, 88px)` · 700 · line-height 0.98 · letter-spacing -0.035em
- **Headline LG:** `clamp(32px, 3.5vw, 48px)` · 700 · 1.08 · -0.02em
- **Headline MD:** 24px · 600 · 32px · -0.01em
- **Body LG:** 18px · 400 · 28px
- **Body MD:** 15–16px · 400 · 24px
- **Label / eyebrow:** 10–11px · 600 · letter-spacing 0.14em–0.24em · UPPERCASE
- **Tabular numerals:** any price/spec value uses `font-variant-numeric: tabular-nums`.

### Radii
- `--r-sm` `0.5rem` (8px) — small pills, input inner, checkboxes
- `--r-md` `1rem` (16px) — inputs, thumbnails
- `--r-lg` `1.5rem` (24px) — sidebar cards, mini cards
- `--r-xl` `2rem` (32px) — main product cards, form cards
- `--r-2xl` `3rem` (48px) — full-bleed feature panels (Mission, Products hero)
- `--r-full` `9999px` — nav pill, buttons, chips, badges (except badge-new which uses `--r-sm`)

### Spacing
- Base 4px grid: xs 4 · sm 8 · md 16 · lg 24 · xl 40
- `--gutter` 24px (grid gutters)
- `--page-margin` 48px desktop, 20px mobile
- Section padding: 96px desktop, 64px mobile

### Elevation
- Rest state: 1px `--outline-variant` border, no shadow
- Hover / floating panels: `--shadow-hover` (primary-tinted)
- Modals / dropdowns / nav: glassmorphism (`--glass-bg` + `backdrop-filter: blur(20px) saturate(1.4)`) + `--shadow-ambient`
- **Do not use heavy drop shadows on rest states** — depth comes from tonal layering and borders.

### Motion
- Standard easing: `cubic-bezier(0.4, 0, 0.2, 1)` (Material standard)
- Fast transitions: 0.15s (small feedback)
- Standard transitions: 0.2s–0.25s (buttons, cards, links)
- Slow transitions: 0.3s–0.4s (card hover, reveals)
- Reveal-on-scroll: 0.6s fade + `translateY(20px → 0)`
- Marquee: 30s linear infinite
- Orbit rings: 40s / 60s linear infinite
- Float animation: 6s ease-in-out infinite (±8px Y)
- Pulse (status dots): 2s ease-in-out infinite

## Assets

- **Fonts:** `Sora` from Google Fonts (preconnect + `<link>` in each HTML head).
- **Icons:** All icons are inline SVG defined directly in the HTML — no external icon library. Reuse them verbatim or replace with the target framework's icon primitives (Lucide, Phosphor, or the codebase's existing set).
- **Product imagery:** The mock uses stylized inline-SVG abstract product marks generated by `productMark(product, size)` in `app.js` (one SVG generator per category: tablet, fan, camera, shield, wind, video). **These are design placeholders.** In production, replace them with real product photography or 3D renders — the SVG marks demonstrate composition, tinting and framing but are not final art.
- **Illustrations / backdrops:** All aurora, mesh, grid, and glow backgrounds are pure CSS (radial-gradient + linear-gradient + background-image grid overlays). No image files.
- **Logo:** The "Uni-Shop" logo is a text lockup — 34×34 navy rounded square with cyan letter "U" + wordmark. Replace with the client's official logo when supplied.

## Files (in this bundle)

- **`README.md`** — this document
- **`index.html`** — Home page (hero, brand marquee, featured products, about, mission/vision, contact)
- **`products.html`** — Full catalog with filter sidebar, brand pills, sort, view toggle
- **`product.html`** — Product detail (`?id=<productId>`)
- **`cart.html`** — Shopping cart with live summary
- **`checkout.html`** — Multi-step checkout (details / method / payment) with sticky summary
- **`styles.css`** — Global design system, tokens, layout primitives, nav, buttons, cards, footer, forms
- **`app.js`** — `PRODUCTS` data (6 SKUs across DOOGEE / JisuLife / Dreame), `productMark()` SVG generator, cart state (localStorage), nav + footer templates (`mountShell(activePage)`), `showToast()`, reveal-on-scroll observer, and `fmtPrice()` helper

## Implementation Notes for the Developer

- **Use the target codebase's routing.** The mock uses static `.html` files. In React/Next.js, these become routes: `/` (home), `/products`, `/products/[id]`, `/cart`, `/checkout`.
- **Componentize the shell.** `navHTML()` and `footerHTML()` in `app.js` should become `<Nav activePage="..." />` and `<Footer />` layout components.
- **Componentize repeating UI.** `ProductCard`, `Badge`, `Chip`, `Button` (with `variant` prop), `FormInput`, `FormSelect`, `Stepper`, `StatCard`, `InfoRow`, and `GlassPanel` are the natural component boundaries.
- **Cart state → context/store.** Move the `localStorage` cart into React Context, Zustand, Pinia, or the codebase's chosen state library — keep the same API surface (`addToCart`, `updateItem`, `remove`, `clear`, `total`, `count`).
- **Product data → API.** Replace the hardcoded `PRODUCTS` array with a data source (CMS, database, or JSON file). Preserve the shape: `{ id, slug, name, brand, category, price, badge, tagline, desc, specs, color, icon }`.
- **Accessibility:** All interactive elements have `aria-label` where visual-only. Preserve those. Ensure focus states have visible outlines (the current mock relies on `box-shadow` focus rings — carry them over).
- **RTL / i18n:** The previous site had Arabic (AR) content. This redesign is English-first but keeps an "EN" language pill in the nav. Wire it up to a real i18n toggle when Arabic content is ready — the layout should mirror cleanly under `dir="rtl"` because it uses flexbox and CSS grid throughout.
- **Responsive breakpoints:** 1100px, 968px, 800px, 768px, 560px, 480px. Consult each page's `<style>` block for the exact behavior.
- **Reveal-on-scroll:** In React, use an `useInView` hook (framer-motion, react-intersection-observer) rather than the vanilla observer. Preserve the safety timeout so no content stays hidden if observer misfires.
- **Do not touch the design tokens.** They are the whole point of this redesign — the "Aetheric Command Light" system must be the ground truth for future features too.

