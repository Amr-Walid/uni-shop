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

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
