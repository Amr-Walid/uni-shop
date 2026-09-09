using System.Threading;
using System.Threading.Tasks;
using FTD.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FTD.Application.Interfaces
{
    public interface IAppDbContext
    {
        DbSet<Brand> Brands { get; }
        DbSet<Category> Categories { get; }
        DbSet<Product> Products { get; }
        DbSet<ProductImage> ProductImages { get; }
        DbSet<ProductAttribute> ProductAttributes { get; }
        DbSet<AttributeValue> AttributeValues { get; }
        DbSet<ProductAttributeValue> ProductAttributeValues { get; }
        DbSet<OrderStatus> OrderStatuses { get; }
        DbSet<SalesOrder> SalesOrders { get; }
        DbSet<SalesOrderDetail> SalesOrderDetails { get; }
        DbSet<ContentBlock> ContentBlocks { get; }
        DbSet<ContentPage> ContentPages { get; }
        DbSet<PageSection> PageSections { get; }
        DbSet<NavigationItem> NavigationItems { get; }
        DbSet<ContactInfo> ContactInfos { get; }
        DbSet<SiteSetting> SiteSettings { get; }
        DbSet<ContactMessage> ContactMessages { get; }

        // ── Mobile/API support sets ───────────────────────────────────────────
        // Exposed through the same abstraction so the Application layer can
        // implement carts, wishlists, token rotation and idempotency without
        // taking a direct dependency on EF Core's DbContext.
        DbSet<AppUser> AppUsers { get; }
        DbSet<RefreshToken> RefreshTokens { get; }
        DbSet<WishlistItem> WishlistItems { get; }
        DbSet<UserCart> UserCarts { get; }
        DbSet<DeviceToken> DeviceTokens { get; }
        DbSet<IdempotencyRecord> IdempotencyRecords { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
