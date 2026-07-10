using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using FTD.Application.DTOs;
using FTD.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace FTD.Application.Services
{
    public class CartService : ICartService
    {
        private readonly IProductService _productService;
        private readonly IContentService _contentService;
        private readonly IAppDbContext _db;
        private const string CartKey = "ftd_cart";

        public CartService(IProductService productService, IContentService contentService, IAppDbContext db)
        {
            _productService = productService;
            _contentService = contentService;
            _db = db;
        }

        public async Task<CartDto> GetCartAsync(ISession session)
        {
            var cartData = session.GetString(CartKey);
            var items = new List<CartItemDto>();

            if (!string.IsNullOrEmpty(cartData))
            {
                List<RawCartItem> rawItems;
                try
                {
                    rawItems = JsonSerializer.Deserialize<List<RawCartItem>>(cartData) ?? new();
                }
                catch (JsonException)
                {
                    rawItems = new List<RawCartItem>();
                }

                if (rawItems.Any())
                {
                    var productIds = rawItems.Select(x => x.ProductId).Distinct().ToList();

                    // Resolve N+1: Query database once for all matching active products
                    var products = await _db.Products
                        .Where(p => productIds.Contains(p.Id) && p.IsActive)
                        .ToListAsync();

                    foreach (var raw in rawItems)
                    {
                        var product = products.FirstOrDefault(p => p.Id == raw.ProductId);
                        if (product != null)
                        {
                            items.Add(new CartItemDto
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
            }

            var shippingFeeStr = await _contentService.GetSettingAsync("shipping.fee", "150");
            var shippingFee = decimal.TryParse(shippingFeeStr, out var fee) ? fee : 150m;

            var freeAboveStr = await _contentService.GetSettingAsync("shipping.free.above", "5000");
            var freeAbove = decimal.TryParse(freeAboveStr, out var fa) ? fa : 5000m;

            var cart = new CartDto { Items = items, ShippingFee = shippingFee, FreeShippingAbove = freeAbove };
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
            if (string.IsNullOrEmpty(data))
                return new List<RawCartItem>();
            try
            {
                return JsonSerializer.Deserialize<List<RawCartItem>>(data) ?? new();
            }
            catch (JsonException)
            {
                return new List<RawCartItem>();
            }
        }

        private void SaveRawItems(ISession session, List<RawCartItem> items)
            => session.SetString(CartKey, JsonSerializer.Serialize(items));

        private class RawCartItem { public int ProductId { get; set; } public int Qty { get; set; } }
    }
}
