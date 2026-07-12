# Specification & Implementation Plan: Fully Dynamic Homepage Dashboard Control

## 1. Goal & Context
The goal is to eliminate all hardcoded content on the frontend homepage (`FTD.Web/Views/Home/Index.cshtml` and `FTD.Web/Views/Shared/Components/Footer/Default.cshtml`) and replace it with dynamic variables managed from the Admin Dashboard ("محتوى الرئيسية" / Home Content).
This includes:
* Controlling the Hero text, subtitles, buttons, links, and stats.
* Dynamically selecting which products appear in the Hero Showcase Slider.
* Dynamically selecting which products appear in the Featured Catalog Grid, and controlling their display order.
* Editing the 4 Value Props (icon choice, title, description in Arabic & English).
* Managing Category Showcase titles and descriptions.
* Adding Category Image uploads, visibility toggles (`ShowOnHomepage`), and auto-loading category data on the homepage.
* Managing About Us & Contact details text.

---

## 2. Database Changes (EF Core Migration)

### 2.1 Category Entity Modification
Modify `FTD.Domain/Entities/Category.cs` to add:
```csharp
public bool ShowOnHomepage { get; set; } = true;
```
Ensure `FTD.Application/DTOs/CategoryDto.cs` and the mapping profile are updated to include this field.

### 2.2 Seed Data in AppDbContext.cs
Seed the database with default values for new keys in `ContentBlocks` and `SiteSettings`:
* **Hero Content Blocks:**
  * `hero.title.line1.ar` / `hero.title.line1.en` (e.g., "منصة التحكم" / "The command deck")
  * `hero.title.line2.ar` / `hero.title.line2.en` (e.g., "للتكنولوجيا العصرية." / "for modern tech.")
  * `hero.subtitle.ar` / `hero.subtitle.en`
  * `hero.btn1.text.ar` / `hero.btn1.text.en`
  * `hero.btn1.url`
  * `hero.btn2.text.ar` / `hero.btn2.text.en`
  * `hero.btn2.url`
* **Value Props Blocks (`value.prop1` to `value.prop4`):**
  * `value.prop{N}.icon` (stores Bootstrap Icons class name, e.g., `bi-shield-check`)
  * `value.prop{N}.title.ar` / `value.prop{N}.title.en`
  * `value.prop{N}.desc.ar` / `value.prop{N}.desc.en`
* **Category Showcase Section Blocks:**
  * `cat.showcase.eyebrow.ar` / `cat.showcase.eyebrow.en` (e.g., "تسوق حسب الفئة · ٠١" / "Shop by Category · 01")
  * `cat.showcase.title.ar` / `cat.showcase.title.en`
  * `cat.showcase.desc.ar` / `cat.showcase.desc.en`
* **Featured Catalog Section Blocks:**
  * `feat.catalog.eyebrow.ar` / `feat.catalog.eyebrow.en`
  * `feat.catalog.title.ar` / `feat.catalog.title.en`
  * `feat.catalog.desc.ar` / `feat.catalog.desc.en`
* **CTA Band Section Blocks:**
  * `cta.title.ar` / `cta.title.en`
  * `cta.desc.ar` / `cta.desc.en`
  * `cta.btn.text.ar` / `cta.btn.text.en`
  * `cta.btn.url`
* **SiteSettings for Product Selections:**
  * `homepage.hero.products` (Value: comma-separated product IDs, e.g., `"1,2,3"`)
  * `homepage.featured.products` (Value: comma-separated product IDs, e.g., `"1,2,3,4,5,6"`)

### 2.3 Run EF Core Migration
Run the following commands to add and apply migrations:
```bash
dotnet ef migrations add AddShowOnHomepageAndHomepageSeeds --project FTD.Infrastructure --startup-project FTD.Web
dotnet ef database update --project FTD.Infrastructure --startup-project FTD.Web
```

---

## 3. Category Administration Enhancements

### 3.1 Category Form (`FTD.Web/Views/Admin/Categories/Form.cshtml`)
* Add `enctype="multipart/form-data"` to the HTML form.
* Add an `<input type="file" name="ImageFile">` field to upload category images.
* Display the current image preview if `ImagePath` is set.
* Add a checkbox for `ShowOnHomepage`.

### 3.2 Category Controller (`AdminCategoriesController` in `AdminControllers.cs`)
* Update `Create` and `Edit` POST actions to accept `IFormFile? ImageFile` and `bool ShowOnHomepage`.
* Process the uploaded image, saving it to `wwwroot/images/categories/` using a unique filename (e.g., `guid.ext`), and set `model.ImagePath = "/images/categories/filename.ext"`.
* Save the new properties (`ImagePath` and `ShowOnHomepage`) to the database.

### 3.3 Homepage Category Showcase Rendering
* Update `FTD.Web/Views/Home/Index.cshtml` to check for `ImagePath`:
  * If `ImagePath` is present, render it: `<img src="@cat.ImagePath" alt="@cat.NameEn" class="cat-tile-image">`
  * If empty, fallback to the `Emoji` text.
* Filter the category showcase to only display active categories where `ShowOnHomepage == true`.

---

## 4. Homepage Product Selection Dashboard

### 4.1 Admin Content Controller (`AdminContentController` in `AdminControllers.cs`)
* Update the `Blocks` GET action to load the list of all active products:
  `ViewBag.Products = await _productService.GetActiveProductsAsync();` (or equivalent method returning product list).
* Add a POST endpoint `SaveProductSelections` that takes `List<int> heroProductIds` and `List<int> featuredProductIds`, serializes them as comma-separated strings, and updates `homepage.hero.products` and `homepage.featured.products` settings.

### 4.2 Tabbed Home Content View (`FTD.Web/Views/Admin/Content/Blocks.cshtml`)
Redesign the interface using a modern, tabbed interface:
* **Tab 1: 🌟 الهيرو والإحصائيات (Hero & Stats):**
  * Inputs for titles, taglines, button texts/URLs, and stats.
  * Searchable multi-select dropdown for **Hero Showcase Slider Products** (displays all active products, allowing selection and ordering).
* **Tab 2: ✨ مزايا المتجر (Value Props):**
  * Forms for the 4 Value Props.
  * Dropdown select for **Icon Library** displaying standard icons (e.g., Warranty 🛡️ -> `bi-shield-check`, Delivery 🚚 -> `bi-truck`, Support 💬 -> `bi-headset`, Returns ↩ -> `bi-arrow-counterclockwise`, Installments 💳 -> `bi-credit-card`, Quality ⭐ -> `bi-patch-check`).
* **Tab 3: 📦 كتالوج المنتجات (Featured Catalog Products):**
  * Searchable multi-select dropdown for **Featured Catalog Grid Products**.
  * Form fields to change eyebrow text, main title, and description.
* **Tab 4: 🏷️ الأقسام والفوتر (Categories, CTA, Newsletter):**
  * Forms to edit Titles and Descriptions for: Category Showcase, CTA Band, Newsletter, and Footer credits.
* **Tab 5: ℹ️ من نحن واتصل بنا (About & Contact):**
  * Textareas for About Us body texts, Vision, Mission, and form inputs for contact details (Phone, Email, Hours, Socials).

---

## 5. Homepage Integration & Rendering

### 5.1 Load Selected Products in HomeController
In `HomeController.Index()`:
1. Parse the comma-separated IDs from `homepage.hero.products` and `homepage.featured.products`.
2. Fetch the corresponding products.
3. Order the fetched products exactly matching the sequence specified in the list of IDs.
4. Pass these ordered lists to the `HomeViewModel` (e.g., `HeroProducts` and `FeaturedProducts`).

### 5.2 Frontend Updates in Index.cshtml
* Replace hardcoded Hero titles, subtitles, buttons, and stats with variables: `@Get("hero.title.line1")`, `@Get("hero.subtitle")`, etc.
* Replace the 4 hardcoded Value Props with values from `value.prop1` to `value.prop4`, rendering icon class names inside `<i class="bi @Get("value.prop1.icon")"></i>`.
* Ensure Bootstrap Icons CSS CDN link is included in `_Layout.cshtml`:
  `<link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css">`
* Render the categories dynamically based on `ShowOnHomepage` check and display category image when available.
