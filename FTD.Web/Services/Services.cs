using FTD.Web.Data;
using FTD.Web.Models;
using FTD.Web.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace FTD.Web.Services
{
    // ── PRODUCT SERVICE ───────────────────────────────────────────────────────
    public class ProductService
    {
        private readonly AppDbContext _db;
        public ProductService(AppDbContext db) => _db = db;

        public async Task<List<Product>> GetFeaturedAsync(int take = 6)
            => await _db.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive && p.IsFeatured)
                .OrderBy(p => p.SortOrder)
                .Take(take)
                .ToListAsync();

        public async Task<List<Product>> GetAllActiveAsync()
            => await _db.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive)
                .OrderBy(p => p.Category.SortOrder).ThenBy(p => p.SortOrder)
                .ToListAsync();

        public async Task<List<Product>> GetByCategoryAsync(int categoryId)
            => await _db.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive && p.CategoryId == categoryId)
                .OrderBy(p => p.SortOrder)
                .ToListAsync();

        public async Task<List<Product>> GetByBrandAsync(string brandSlug)
            => await _db.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Where(p => p.IsActive && (
                    (p.Brand != null && p.Brand.Slug.ToLower() == brandSlug.ToLower()) ||
                    (p.BrandName != null && p.BrandName.ToLower() == brandSlug.ToLower())
                ))
                .OrderBy(p => p.SortOrder)
                .ToListAsync();

        // Filtered query for products page with AJAX filters
        public async Task<List<Product>> GetFilteredAsync(
            string? brandSlug, string? categorySlug,
            List<int>? attributeValueIds, string? sortBy)
        {
            // Step 1: simple query WITHOUT Include(AttributeValues) to avoid CTE
            var query = _db.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Where(p => p.IsActive)
                .AsQueryable();

            if (!string.IsNullOrEmpty(brandSlug))
                query = query.Where(p =>
                    (p.Brand != null && p.Brand.Slug.ToLower() == brandSlug.ToLower()) ||
                    (p.BrandName != null && p.BrandName.ToLower() == brandSlug.ToLower()));

            if (!string.IsNullOrEmpty(categorySlug))
                query = query.Where(p => p.Category.Slug == categorySlug);

            query = sortBy switch
            {
                "price_asc" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                "newest" => query.OrderByDescending(p => p.CreatedAt),
                _ => query.OrderByDescending(p => p.IsFeatured).ThenBy(p => p.SortOrder)
            };

            var products = await query.ToListAsync();

            // Step 2: filter by attributes if needed (fetch separately to avoid CTE)
            if (attributeValueIds != null && attributeValueIds.Any())
            {
                var productIds = products.Select(p => p.Id).ToHashSet();
                // Fetch all then filter in memory - avoids CTE error
                var allPavs = await _db.ProductAttributeValues.ToListAsync();
                var pavs = allPavs.Where(av => productIds.Contains(av.ProductId)).ToList();

                products = products.Where(p =>
                    attributeValueIds.All(vid =>
                        pavs.Any(av => av.ProductId == p.Id && av.AttributeValueId == vid))
                ).ToList();
            }

            return products;
        }

        public async Task<Product?> GetBySlugAsync(string slug)
            => await _db.Products
                .Include(p => p.Category)
                .Include(p => p.AttributeValues)
                    .ThenInclude(av => av.Attribute)
                .Include(p => p.AttributeValues)
                    .ThenInclude(av => av.AttributeValue)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Slug == slug && p.IsActive);

        public async Task<Product?> GetByIdAsync(int id)
            => await _db.Products
                .Include(p => p.Category)
                .Include(p => p.AttributeValues)
                    .ThenInclude(av => av.Attribute)
                .Include(p => p.AttributeValues)
                    .ThenInclude(av => av.AttributeValue)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);

        public async Task<List<Product>> GetRelatedAsync(int productId, int categoryId, int take = 4)
            => await _db.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive && p.CategoryId == categoryId && p.Id != productId)
                .Take(take)
                .ToListAsync();

        public async Task<List<Product>> SearchAsync(string query)
        {
            var q = query.ToLower().Trim();
            return await _db.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive && (
                    p.NameAr.ToLower().Contains(q) ||
                    p.NameEn.ToLower().Contains(q) ||
                    (p.BrandName != null && p.BrandName.ToLower().Contains(q)) ||
                    (p.ShortDescAr != null && p.ShortDescAr.ToLower().Contains(q)) ||
                    (p.ShortDescEn != null && p.ShortDescEn.ToLower().Contains(q)) ||
                    (p.DescAr != null && p.DescAr.ToLower().Contains(q)) ||
                    (p.DescEn != null && p.DescEn.ToLower().Contains(q)) ||
                    (p.Badge != null && p.Badge.ToLower().Contains(q)) ||
                    (p.Category != null && p.Category.NameAr.ToLower().Contains(q)) ||
                    (p.Category != null && p.Category.NameEn.ToLower().Contains(q))
                ))
                .OrderByDescending(p => p.IsFeatured)
                .ThenByDescending(p => p.CreatedAt)
                .Take(20)
                .ToListAsync();
        }
    }

    // ── CONTENT SERVICE ───────────────────────────────────────────────────────
    public class ContentService
    {
        private readonly AppDbContext _db;
        public ContentService(AppDbContext db) => _db = db;

        public async Task<Dictionary<string, string>> GetBlocksAsync()
        {
            var blocks = await _db.ContentBlocks.ToListAsync();
            var dict = new Dictionary<string, string>();
            foreach (var b in blocks)
            {
                if (!string.IsNullOrEmpty(b.BodyAr)) dict[b.Key + ".ar"] = b.BodyAr;
                if (!string.IsNullOrEmpty(b.BodyEn)) dict[b.Key + ".en"] = b.BodyEn;
                if (!string.IsNullOrEmpty(b.TitleAr)) dict[b.Key + ".icon"] = b.TitleAr;
                if (!string.IsNullOrEmpty(b.BodyAr)) dict[b.Key] = b.BodyAr;
            }
            return dict;
        }

        public async Task<string> GetBlockAsync(string key, string lang = "ar")
        {
            var block = await _db.ContentBlocks.FirstOrDefaultAsync(b => b.Key == key);
            if (block == null) return "";
            return lang == "en" ? (block.BodyEn ?? block.BodyAr ?? "") : (block.BodyAr ?? block.BodyEn ?? "");
        }

        public async Task<ContentPage?> GetPageBySlugAsync(string slug)
            => await _db.ContentPages
                .Include(p => p.Sections.OrderBy(s => s.SortOrder))
                .FirstOrDefaultAsync(p => p.Slug == slug && p.IsPublished);

        public async Task<ContactInfo?> GetContactInfoAsync()
            => await _db.ContactInfos.FirstOrDefaultAsync();

        public async Task<Dictionary<string, string>> GetSettingsAsync()
        {
            var settings = await _db.SiteSettings.ToListAsync();
            return settings.ToDictionary(s => s.Key, s => s.Value ?? "");
        }

        public async Task<string> GetSettingAsync(string key, string defaultValue = "")
        {
            var s = await _db.SiteSettings.FirstOrDefaultAsync(x => x.Key == key);
            return s?.Value ?? defaultValue;
        }
    }

    // ── ORDER SERVICE ─────────────────────────────────────────────────────────
    public class OrderService
    {
        private readonly AppDbContext _db;
        public OrderService(AppDbContext db) => _db = db;

        public async Task<SalesOrder> CreateOrderAsync(CheckoutViewModel checkout, CartViewModel cart)
        {
            var orderNumber = $"FTD{DateTime.UtcNow:yyyyMMddHHmmss}{Random.Shared.Next(100, 999)}";

            var order = new SalesOrder
            {
                OrderNumber = orderNumber,
                StatusId = 1, // New
                CustomerName = checkout.CustomerName,
                CustomerPhone = checkout.CustomerPhone,
                CustomerEmail = checkout.CustomerEmail,
                Address = checkout.Address,
                City = checkout.City,
                Governorate = checkout.Governorate,
                Notes = checkout.Notes,
                SubTotal = cart.SubTotal,
                ShippingFee = cart.FreeShipping ? 0 : cart.ShippingFee,
                TotalAmount = cart.Total,
                CreatedAt = DateTime.UtcNow
            };

            _db.SalesOrders.Add(order);
            await _db.SaveChangesAsync();

            foreach (var item in cart.Items)
            {
                _db.SalesOrderDetails.Add(new SalesOrderDetail
                {
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    SubTotal = item.SubTotal
                });
            }
            await _db.SaveChangesAsync();
            return order;
        }

        public async Task UpdateStatusAsync(int orderId, int statusId)
        {
            var order = await _db.SalesOrders.FindAsync(orderId);
            if (order != null)
            {
                order.StatusId = statusId;
                order.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }
        }
    }

    // ── CART SERVICE (Session-based) ──────────────────────────────────────────
    public class CartService
    {
        private readonly AppDbContext _db;
        private const string CartKey = "ftd_cart";

        public CartService(AppDbContext db) => _db = db;

        public async Task<CartViewModel> GetCartAsync(ISession session)
        {
            var cartData = session.GetString(CartKey);
            var items = new List<CartItem>();

            if (!string.IsNullOrEmpty(cartData))
            {
                var rawItems = System.Text.Json.JsonSerializer
                    .Deserialize<List<RawCartItem>>(cartData) ?? new();

                foreach (var raw in rawItems)
                {
                    var product = await _db.Products.FindAsync(raw.ProductId);
                    if (product != null && product.IsActive)
                    {
                        items.Add(new CartItem
                        {
                            ProductId = product.Id,
                            ProductName = product.NameAr,
                            Emoji = product.Emoji,
                            ImagePath = product.ImagePath,
                            BrandName = product.BrandName,
                            UnitPrice = product.Price,
                            Quantity = raw.Qty
                        });
                    }
                }
            }

            var shippingFee = decimal.TryParse(
                _db.SiteSettings.FirstOrDefault(s => s.Key == "shipping.fee")?.Value, out var fee)
                ? fee : 150m;

            var freeAbove = decimal.TryParse(
                _db.SiteSettings.FirstOrDefault(s => s.Key == "shipping.free.above")?.Value, out var fa)
                ? fa : 5000m;

            var cart = new CartViewModel { Items = items, ShippingFee = shippingFee };
            if (cart.SubTotal >= freeAbove) cart.ShippingFee = 0;
            return cart;
        }

        public void AddItem(ISession session, int productId, int qty = 1)
        {
            var items = GetRawItems(session);
            var existing = items.FirstOrDefault(i => i.ProductId == productId);
            if (existing != null) existing.Qty += qty;
            else items.Add(new RawCartItem { ProductId = productId, Qty = qty });
            SaveRawItems(session, items);
        }

        public void UpdateQty(ISession session, int productId, int qty)
        {
            var items = GetRawItems(session);
            var item = items.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                if (qty <= 0) items.Remove(item);
                else item.Qty = qty;
            }
            SaveRawItems(session, items);
        }

        public void RemoveItem(ISession session, int productId)
        {
            var items = GetRawItems(session).Where(i => i.ProductId != productId).ToList();
            SaveRawItems(session, items);
        }

        public void ClearCart(ISession session) => session.Remove(CartKey);

        public int GetCount(ISession session) => GetRawItems(session).Sum(i => i.Qty);

        private List<RawCartItem> GetRawItems(ISession session)
        {
            var data = session.GetString(CartKey);
            return string.IsNullOrEmpty(data)
                ? new List<RawCartItem>()
                : System.Text.Json.JsonSerializer.Deserialize<List<RawCartItem>>(data) ?? new();
        }

        private void SaveRawItems(ISession session, List<RawCartItem> items)
            => session.SetString(CartKey, System.Text.Json.JsonSerializer.Serialize(items));

        private class RawCartItem { public int ProductId { get; set; } public int Qty { get; set; } }
    }
}
