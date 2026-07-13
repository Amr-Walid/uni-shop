# 🔍 Uni-Shop / FTD TechZone — Enterprise Code Audit Report

**Date:** 2026-07-13
**Scope:** FTD.Domain · FTD.Application · FTD.Infrastructure · FTD.Web · FTD.Api
**Auditor:** Principal .NET Architect (line-by-line review)
**Baseline build:** ✅ `dotnet build` — 0 errors / 0 warnings (before changes)

---

## 1. Findings Summary Table

| # | Severity | Area | Location | Issue |
|---|----------|------|----------|-------|
| I-01 | 🔴 High | EF Core / Schema | `AppDbContext.cs` (no explicit config) + `AppDbContextModelSnapshot.cs` L2215-2218 | `SalesOrderDetail → Product` FK uses **`DeleteBehavior.Cascade`** — deleting a Product would silently cascade-delete historical order lines, destroying financial history. |
| I-02 | 🔴 High | EF Core / Transactions | `ProductService.CreateProductAsync` L341-402, `DuplicateProductAsync` L474-548 | Two separate `SaveChangesAsync()` calls (product first, then images + attribute values). A failure between them leaves an orphan product without its children (partial DB state). |
| I-03 | 🔴 High | EF Core / N+1 | `OrderService.CreateOrderAsync` L48-58 | Stock verification loops `_db.Products.FindAsync(item.ProductId)` per cart item → **N+1 queries** during checkout. |
| I-04 | 🔴 High | EF Core / FK crash | `ProductService.DeleteAttributeAsync` L723-730, `DeleteAttributeValueAsync` L760-767 | `ProductAttributeValue → Attribute/AttributeValue` FKs are `Restrict`. Deleting an attribute (or a value) that is assigned to any product throws an **unhandled `SqlException` (FK constraint violation) → HTTP 500**. |
| I-05 | 🔴 High | Security / CSRF | `CartController` (`Add`, `Update`, `Remove`, `Clear`) `CartOrderController.cs` L29-57 | Cart-mutating POST endpoints have **no `[ValidateAntiForgeryToken]`** even though every calling form/JS already sends the token. |
| I-06 | 🔴 High | Validation | `ProductDto` (no annotations) + `AdminProductsController.Create/Edit` L77-182 | `ModelState.IsValid` is **never checked** and `ProductDto` carries no validation attributes → **negative prices, negative stock, over-length names** are accepted straight into the DB. |
| I-07 | 🔴 High | Validation / 500 crash | `ProductService.CreateProductAsync` / `UpdateProductAsync` / `CreateCategoryAsync` / `CreateBrandAsync` etc. | Slug uniqueness is **never pre-checked**. `Product.Slug`/`Category.Slug` have unique DB indexes → posting a duplicate slug raises `DbUpdateException` (uncaught) → HTTP 500 instead of a friendly validation message. |
| I-08 | 🟠 Medium | EF Core / Perf | `ProductService.GetAllCategoriesAsync` L636 | `Include(c => c.Products)` loads **every product entity in the DB into memory** just to compute `ProductsCount` for the admin grid. |
| I-09 | 🟠 Medium | EF Core / Perf + bug | `ProductService.GetAllBrandsAsync` L589 | No products projection → `Brand.ToDto()` maps `ProductsCount = 0` **always**; Admin Brands grid permanently shows "0 منتج". |
| I-10 | 🟠 Medium | EF Core / Perf | All read-only queries in `ProductService`, `ContentService`, `OrderService`, `DashboardService`, `MessageService`, `CartService` | **Zero usages of `.AsNoTracking()`** in the whole solution — every storefront read pays change-tracker overhead. |
| I-11 | 🟠 Medium | Data integrity | `OrderService.UpdateStatusAsync` L72-81 | `statusId` is not validated against `OrderStatuses` → posting an invalid id raises an FK `SqlException` → 500. |
| I-12 | 🟠 Medium | Validation | `CheckoutViewModel` (`ViewModels.cs` L87-114) | No `StringLength` limits. `SalesOrder.CustomerPhone` is `MaxLength(20)` in DB → a 25-char phone number triggers a `SqlException` (string truncation) at checkout → 500. |
| I-13 | 🟠 Medium | Validation | `HomeController.Contact` L79-94 | No server-side validation: `ContactMessage.Name/Email/Phone` have `MaxLength(100/100/20)`; over-length input → `SqlException` 500. Empty message accepted. |
| I-14 | 🟠 Medium | Cart edge case | `CartService.AddItem` L84-91 + `CartController.Add` | Quantity is not clamped — posting `qty=-5` yields negative cart quantities and negative totals. |
| I-15 | 🟠 Medium | Admin UX / Perf | `AdminCategoriesController.Edit` L363-369, `AdminBrandsController.Edit` L58-64, `AdminContentController.ToggleCategoryHomepage` L548-561 | "Load-all-then-FirstOrDefault" pattern: fetching **every** category/brand (incl. heavy products Include, see I-08) just to edit one row. |
| I-16 | 🟠 Medium | Error handling | `AdminProductsController.Edit/Create` catch blocks L103/L170 | Only `InvalidOperationException` is caught. `UpdateProductAsync` throws `ArgumentException` when the product vanished → unhandled 500 instead of a friendly redirect. |
| I-17 | 🟠 Medium | Startup robustness | `Program.cs` L185 | `try { db.Database.Migrate(); } catch { }` — migration failures are **silently swallowed**; app starts against a broken schema with zero diagnostics. |
| I-18 | 🟠 Medium | Security / DoS surface | `Program.cs` L17-23 | `FormOptions.ValueCountLimit / KeyLengthLimit / MultipartHeadersLengthLimit = int.MaxValue` removes all request-shape protections; 100 MB body limit is far beyond the 5 MB per-image rule. |
| I-19 | 🟡 Low | Localization / display bug | `Views/Home/Index.cshtml` L332-347 | `@Get("about.body.ar.ar")` & stats blocks use `Get()` (no fallback) → empty strings if a block is cleared. Stats seed values already contain `"+"` (`"10+"`) while the view appends another `+` → renders **"10++"**. |
| I-20 | 🟡 Low | Dead config | `ViewModels.cs` L27 | Default `SectionsOrder` still contains removed `"marquee"` section (stale — filtered out downstream, but misleading). |
| I-21 | 🟡 Low | Dead code | `IProductService.GetByCategoryAsync` / `GetByBrandAsync` + impls | Not referenced by any controller/view/API — dead public surface. |
| I-22 | 🟡 Low | Dead code | `CartService` L14-17 | `_productService` injected but never used; `CartKey` const duplicated (real key lives in `SessionCartStorage`). |
| I-23 | 🟡 Low | Code smell / DRY | `AdminProductsController.SaveImageAsync` L296-315, `AdminCategoriesController.SaveCategoryImageAsync` L396-413, `AdminBrandsController.SaveAsync` L94-113 | The exact same upload/validate/save routine is copy-pasted **3 times**. |
| I-24 | 🟡 Low | Code smell / DRY | `ProductService.SearchAsync` L304-330 vs `GetFilteredByIdAsync` L240-257 | 13-line text-search predicate duplicated verbatim. |
| I-25 | 🟡 Low | Determinism | `ProductService.GetRelatedAsync` L293-302 | `Take(4)` with **no `OrderBy`** → SQL Server returns arbitrary rows; related products shuffle randomly between requests. |
| I-26 | 🟡 Low | Unused usings | `HomeController.cs`, `ProductsController.cs`, `AdminMessagesController.cs` (`using FTD.Application.Services;`), `ViewModels.cs` (`using FTD.Domain.Entities;`) | Unused imports referencing concrete service layer from controllers (layering smell). |

---

## 2. Technical Analysis (why these matter)

### I-01 — Cascade delete on order history (Schema)
`SalesOrderDetail.ProductId` was left to EF's convention (required FK ⇒ `Cascade`). The service layer *tries* to protect history (`DeleteProductAsync` soft-deletes when `OrderDetails.Any()`), but defense-in-depth demands the schema itself refuse the destructive path. Any future raw SQL, alternate code path, or admin script deleting a product row would silently erase sold-order lines and corrupt revenue reports. **Fix: explicit `DeleteBehavior.Restrict` + migration.**

### I-02 — Partial-write windows (Transactions)
`CreateProductAsync`/`DuplicateProductAsync` call `SaveChangesAsync()` twice. If the second save fails (e.g. FK violation from a stale attribute id), the product exists with no images/specs — invisible data corruption. Since EF Core persists a whole object graph atomically in one `SaveChanges`, the correct (and cleaner) fix is to **attach children to the navigation collections and save once** — a single implicit transaction, no need to leak `IDbContextTransaction` through the Clean-Architecture interface.

### I-03 — Checkout N+1
Each cart line triggers a `FindAsync` roundtrip inside the hottest business path (order placement). One `WHERE Id IN (...)` query resolves all products, halving checkout latency for multi-item carts and making the stock check consistent over one snapshot.

### I-04 — Attribute deletion FK crash
`ProductAttributeValue` rows are junction rows; the correct semantic when deleting an attribute (or one of its values) is to detach it from products first. Because both FKs are `Restrict` (intentionally, to break SQL Server multiple-cascade-path cycles), the service must remove junction rows explicitly before deleting the parent, otherwise the admin gets a raw 500.

### I-05/06/07 — Security & validation trifecta
- CSRF: all cart mutations are state-changing POSTs; tokens are already emitted by every caller, so enabling validation is zero-friction. A hidden global token is added to `_Layout` so AJAX quick-add works from any page.
- Model validation: without `ModelState.IsValid` + `[Range]`/`[StringLength]`, a hostile or clumsy admin can persist `Price = -100`, `Stock = -5`, or a 500-char name that explodes at the SQL layer.
- Slug uniqueness: unique indexes are the last line of defense; the service should pre-check and raise a *localized, catchable* `InvalidOperationException` that the controllers already know how to render back into the form.

### I-08/09/10/15 — Query hygiene
Loading full product collections to count them is `O(all products)` memory for an admin grid; a correlated `Count()` projection is `O(1)` transferred per row. `AsNoTracking()` on every read-only path removes snapshot-tracking allocations across the whole storefront (every page render executes 5-10 queries via view components).

### I-17/18 — Operational hardening
A swallowed `Migrate()` failure is the #1 cause of "the site is up but everything 500s" incidents. Logging the failure preserves the fail-open behavior (useful when SQL isn't reachable in dev) while making the cause visible. `int.MaxValue` form limits neutralize Kestrel's request-shape DoS protections for zero benefit — the largest legitimate form (bulk content blocks) is a few hundred fields.

---

## 3. Planned Fixes (implementation map)

| # | Fix | Files touched | Migration? |
|---|-----|---------------|-----------|
| I-01 | Explicit `Restrict` FK `SalesOrderDetail→Product` | `AppDbContext.cs` | ✅ `HardenOrderDetailProductFk` |
| I-02 | Single-`SaveChanges` object-graph writes | `ProductService.cs` | — |
| I-03 | Batch product fetch in checkout + qty guard | `OrderService.cs` | — |
| I-04 | Detach junction rows before attribute/value delete | `ProductService.cs` | — |
| I-05 | `[ValidateAntiForgeryToken]` on cart POSTs + global token in `_Layout` | `CartOrderController.cs`, `_Layout.cshtml` | — |
| I-06 | DataAnnotations on `ProductDto`/`CategoryDto`/`BrandDto` + `ModelState.IsValid` checks | `ProductDtos.cs`, `CatalogDtos.cs`, `AdminControllers.cs` | — |
| I-07 | Slug uniqueness pre-checks (product/category/brand) throwing localized `InvalidOperationException` | `ProductService.cs` | — |
| I-08/09 | Count-projection queries for categories & brands | `ProductService.cs` | — |
| I-10 | `.AsNoTracking()` on every read-only query | all services | — |
| I-11 | Validate status id exists before update | `OrderService.cs` | — |
| I-12 | `StringLength` limits mirroring entity constraints | `ViewModels.cs` | — |
| I-13 | Server-side validation + length clamping in contact endpoint | `HomeController.cs` | — |
| I-14 | Clamp cart quantities (≥1 add / ≤ 0 removes) | `CartService.cs`, `CartOrderController.cs` | — |
| I-15 | New `GetCategoryByIdAsync` / `GetBrandByIdAsync` service methods; controllers use them | `IProductService.cs`, `ProductService.cs`, `AdminControllers.cs`, `AdminBrandsController.cs` | — |
| I-16 | Catch `ArgumentException` in product Create/Edit | `AdminControllers.cs` | — |
| I-17 | Log migration failure instead of empty catch | `Program.cs` | — |
| I-18 | Sane form limits (2048-key / 5000-value / 32 MB multipart) | `Program.cs` | — |
| I-19 | `GetOr` fallbacks + `TrimEnd('+')` for stat blocks | `Views/Home/Index.cshtml` | — |
| I-20 | Remove stale `marquee` from default order | `ViewModels.cs` | — |
| I-21 | Remove dead `GetByCategoryAsync`/`GetByBrandAsync` | `IProductService.cs`, `ProductService.cs` | — |
| I-22 | Remove unused `_productService`/`CartKey` from `CartService` | `CartService.cs` | — |
| I-23 | Shared `ImageUploadHelper` (single implementation) | new `Helpers/ImageUploadHelper.cs` + 3 controllers | — |
| I-24 | Extract shared `ApplyTextSearch` predicate | `ProductService.cs` | — |
| I-25 | Deterministic ordering in `GetRelatedAsync` | `ProductService.cs` | — |
| I-26 | Remove unused using directives | 4 files | — |

---

## 4. Resolution Log (Phase 4 — updated after implementation)

All 26 findings are resolved. Final verification: `dotnet build` on **FTD.Web** and **FTD.Api** → `0 Warning(s), 0 Error(s)`.

- [x] **I-01 Fixed** — Explicit `.OnDelete(DeleteBehavior.Restrict)` added for `SalesOrderDetail → Product` in `AppDbContext.OnModelCreating`. Accompanying migration **`20260713091943_HardenOrderDetailProductFk`** generated (drops + re-adds the FK with `ReferentialAction.Restrict`; `Down()` restores `Cascade`). Applied automatically at startup via `db.Database.Migrate()`. `DeleteProductAsync` already soft-deletes products with order history, so `Restrict` can never surface as a runtime FK error.
- [x] **I-02 Fixed** — `CreateProductAsync` and `DuplicateProductAsync` now attach images and attribute values to the `product.Images` / `product.AttributeValues` navigation collections and call `SaveChangesAsync()` **once**. EF Core persists the whole object graph in a single implicit transaction — no more orphan-product window. (Chosen over leaking `IDbContextTransaction` through the `IAppDbContext` abstraction.)
- [x] **I-03 Fixed** — `OrderService.CreateOrderAsync` fetches all cart products in one `WHERE Id IN (...)` query into a dictionary; stock deduction + order insert still share one `SaveChangesAsync` (atomic). Also added a `Quantity <= 0` guard per line.
- [x] **I-04 Fixed** — `DeleteAttributeAsync` / `DeleteAttributeValueAsync` now `RemoveRange` the matching `ProductAttributeValues` junction rows before deleting the parent — no more FK-violation 500s when the attribute is in use.
- [x] **I-05 Fixed** — `[ValidateAntiForgeryToken]` added to `CartController.Add/Update/Remove/Clear`. A global `@Html.AntiForgeryToken()` was added to `_Layout.cshtml` so `site.js`'s AJAX cart calls (which already read `input[name=__RequestVerificationToken]`) find a token on every page.
- [x] **I-06 Fixed** — `ProductDto` gained `[Required]/[StringLength]` on names/slug and `[Range(0, …)]` on `Price`/`OldPrice`/`Stock`; `CategoryDto`/`BrandDto` gained matching annotations. `AdminProductsController.Create/Edit`, `AdminCategoriesController.Create/Edit`, `AdminBrandsController.Create/Edit` now check `ModelState.IsValid` (existing `ModelState.Remove(...)` calls for unbound navigation members preserved).
- [x] **I-07 Fixed** — `EnsureProductSlugUniqueAsync` / `EnsureCategorySlugUniqueAsync` pre-check slugs (with `excludeId` for edits) and throw a localized `InvalidOperationException` which the admin controllers surface as a form error. (Note: `Brand.Slug` has no unique DB index, so no pre-check is required there.)
- [x] **I-08 Fixed** — `GetAllCategoriesAsync` uses a `{ Category, Count = c.Products.Count }` projection → `COUNT(*)` in SQL, no product entities materialized.
- [x] **I-09 Fixed** — `GetAllBrandsAsync` uses the same projection; the admin Brands grid now shows real product counts instead of a permanent 0.
- [x] **I-10 Fixed** — `.AsNoTracking()` applied to every read-only query across `ProductService`, `OrderService`, `CartService`, `ContentService`, `DashboardService`, `MessageService`. Write paths that rely on tracked entities (`FindAsync` + mutate + save) were deliberately left tracked.
- [x] **I-11 Fixed** — `UpdateStatusAsync` validates the status id via `AnyAsync` and throws a localized `InvalidOperationException`; `AdminOrdersController.UpdateStatus` catches it into `TempData["Error"]`.
- [x] **I-12 Fixed** — `CheckoutViewModel` now mirrors the `SalesOrder` column limits: Name 150, Phone 20, Email 200, Address 300, City/Governorate 100, Notes 1000 — oversized input fails model validation instead of throwing `SqlException`.
- [x] **I-13 Fixed** — `HomeController.Contact` rejects empty name/message and clamps Name→100, Email→100, Phone→20, Message→4000 (matching the `ContactMessage` entity `MaxLength`s) before saving.
- [x] **I-14 Fixed** — `CartService.AddItem` clamps `qty = Math.Max(1, qty)`; `UpdateQty`'s "≤ 0 removes the line" behavior was intentional and kept.
- [x] **I-15 Fixed** — New `GetCategoryByIdAsync` / `GetBrandByIdAsync` single-row lookups added to `IProductService`/`ProductService`; now used by `AdminCategoriesController.Edit`, `AdminBrandsController.Edit` (GET + POST) and `AdminContentController.ToggleCategoryHomepage`.
- [x] **I-16 Fixed** — Product/category/brand Create/Edit actions catch `ex is InvalidOperationException or ArgumentException`; the product form re-population was extracted into a `RedisplayFormAsync` helper.
- [x] **I-17 Fixed** — Startup migration failures are logged via `ILogger<Program>` (`LogError` with the exception) instead of an empty catch; the app still boots so it can run against a read-only replica.
- [x] **I-18 Fixed** — Form limits bounded: `ValueCountLimit = 5000`, `KeyLengthLimit = 8 KB`, `MultipartHeadersLengthLimit = 64 KB`, `MultipartBodyLengthLimit = 32 MB`; Kestrel `MaxRequestBodySize` aligned at 32 MB (covers the bulk content editor + multiple 5 MB images).
- [x] **I-19 Fixed** — `about.body.*` now uses `GetOr(...)` with the seeded copy as fallback; the three stat cards use `GetOr(...).TrimEnd('+')` so `"10+"` + the styled `+` span renders **"10+"**, never "10++" or an empty card.
- [x] **I-20 Fixed** — stale `"marquee"` removed from the default `SectionsOrder` string in `HomeViewModel`.
- [x] **I-21 Fixed** — dead `GetByCategoryAsync` / `GetByBrandAsync` removed from `IProductService` and `ProductService` (grep-verified unreferenced in Web + Api).
- [x] **I-22 Fixed** — unused `_productService` injection and duplicate `CartKey` const removed from `CartService` (the `IContentService` dependency is genuinely used for shipping settings and was kept).
- [x] **I-23 Fixed** — new `FTD.Web/Helpers/ImageUploadHelper.SaveAsync(file, env, folder)` (extension whitelist, 5 MB cap, GUID filename) replaces the three copy-pasted private upload methods in `AdminProductsController`, `AdminCategoriesController`, `AdminBrandsController`.
- [x] **I-24 Fixed** — shared `static ApplyTextSearch(IQueryable<Product>, string)` predicate extracted; used by `SearchAsync` and `GetFilteredByIdAsync`.
- [x] **I-25 Fixed** — `GetRelatedAsync` orders by `IsFeatured desc, SortOrder, Id` before `Take(4)` — deterministic related products.
- [x] **I-26 Fixed** — unused `using FTD.Application.Services;` removed from `HomeController.cs`, `ProductsController.cs`, `AdminMessagesController.cs`; unused `using FTD.Domain.Entities;` removed from `ViewModels.cs`.

### Documentation
- [x] `PROJECT_COMPLETE_DOCUMENTATION.md` — new **Section 11** documents the FK schema change + migration, the `IProductService` signature changes, and the architectural hardening (per the mandatory documentation rule).

### Build verification (final)
```text
FTD.Web  → Build succeeded. 0 Warning(s) / 0 Error(s)
FTD.Api  → Build succeeded. 0 Warning(s) / 0 Error(s)
```
> Note: `dotnet ef migrations add HardenOrderDetailProductFk --project FTD.Infrastructure --startup-project FTD.Web` executed successfully. `database update` was not run here (no SQL Server in this environment) — the app self-applies migrations at startup via `db.Database.Migrate()`.
