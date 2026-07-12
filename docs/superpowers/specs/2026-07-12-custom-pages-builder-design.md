# Specification & Design Spec: Premium Visual Page Builder for Custom Pages

## 1. Context & Present Limitations
Currently, custom static pages (like Privacy Policy, Shipping Terms) in the admin dashboard have a split editing flow:
1. **Metadata Edit Page (`PageForm.cshtml`):** Controls titles, slug, publication status, SEO metadata, and a legacy raw HTML body textarea.
2. **Sections Management (`PageSections.cshtml`):** Controls dynamic layout blocks (RichText, FAQ, Cards, Hero). It forces the user to move sections up/down using page-reloading arrow buttons.
3. **Content Constraints:** Editors have no rich text capabilities (forcing them to write raw HTML tags like `<h2>` or `<p>`), and they cannot upload custom images/banners directly to sections.

---

## 2. Proposed Improvements & UX Redesign

We propose converting the custom page editing interface into a **unified visual page builder**:

### A. Sidebar-Based Unified Layout
Combine Page Settings and Page Sections into a single screen:
* **Left Sidebar (Metadata & SEO):** Contains Slug, publication toggle, SEO Meta Title, and Meta Description.
* **Right Workspace (Active Canvas):**
  * Displays a list of sections currently added to the page.
  * Drag handles to reorder sections.
  * Accordion-style expandable section settings where forms are populated directly.
  * In-place Rich Text editors (Quill.js) instead of raw textareas.
  * Add Section menu to insert new layout blocks dynamically.

### B. Drag-and-Drop Reordering (No Page Reloads)
* Integrate a lightweight, native JavaScript drag-and-drop listener (or load SortableJS from CDN).
* The user drags a section using a grab handle (`bi-grip-vertical`).
* Once dropped, the new sequence of Section IDs is read from the DOM, and a background AJAX request is sent to `/Admin/AdminPageSections/UpdateSortOrders` to commit changes to the database instantly.

### C. Rich Text Editor (Quill.js integration)
* Load Quill.js via jsDelivr CDN inside the admin template.
* Automatically mount Quill on textareas inside `RichText` sections and the legacy page body.
* Provides non-technical users with bold, italic, lists, links, alignment, and simple text formatting options.

### D. Multi-Format Layout Support & Image Uploads
We will extend the flexible `ContentJson` schema of existing sections to support custom images:

#### 1. Rich Text Section (`RichText`)
* Integrate Quill's image handler to upload images to the server via AJAX, saving files to `wwwroot/images/uploads/` and returning URLs.

#### 2. Split-Layout Hero Banner (`Hero`)
* Add an image upload input to the Hero section editor form.
* Save the path `image_path` inside `ContentJson`.
* In `_Section_Hero.cshtml` (storefront), if an image is provided:
  * Render a premium 50/50 split layout grid: text content on one side (with an elegant background) and the uploaded banner image on the other side.
  * If no image is provided, fallback to the current centered dark banner.

#### 3. Image-Supported Cards (`Cards`)
* Add option for each card: either a Bootstrap Icon class name (e.g. `bi-truck`) OR an uploaded card image (`card_image_path`).
* Render the uploaded image in place of the icon if present.

---

## 3. Database & API Endpoints

### 3.1 AJAX Reorder Endpoint
```csharp
[HttpPost, Route("Admin/AdminPageSections/UpdateSortOrders")]
public async Task<IActionResult> UpdateSortOrders([FromBody] List<int> orderedSectionIds)
{
    // Sets the SortOrder of sections in the exact order received.
    await _contentService.UpdatePageSectionsOrderAsync(orderedSectionIds);
    return Json(new { success = true });
}
```

### 3.2 Image Upload Endpoint
```csharp
[HttpPost, Route("Admin/AdminPageSections/UploadImage")]
public async Task<IActionResult> UploadImage(IFormFile file)
{
    if (file == null || file.Length == 0) return BadRequest("Invalid file");
    
    // Validate image format (.jpg, .png, .webp, .svg) and size (< 5MB)
    var ext = Path.GetExtension(file.FileName).ToLower();
    if (ext != ".png" && ext != ".jpg" && ext != ".jpeg" && ext != ".webp" && ext != ".svg")
        return BadRequest("Unsupported extension");

    var filename = Guid.NewGuid().ToString() + ext;
    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/uploads", filename);
    
    using (var stream = new FileStream(path, FileMode.Create))
    {
        await file.CopyToAsync(stream);
    }
    
    return Json(new { url = $"/images/uploads/{filename}" });
}
```
