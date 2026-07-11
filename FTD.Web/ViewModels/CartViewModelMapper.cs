using FTD.Application.DTOs;
using FTD.Web.ViewModels;
using System.Linq;

namespace FTD.Web.ViewModels
{
    /// <summary>
    /// Maps Application-layer CartDto to Web-layer CartViewModel.
    /// This is the only place where CartDto ↔ CartViewModel translation happens.
    /// </summary>
    public static class CartViewModelMapper
    {
        public static CartViewModel FromDto(CartDto dto)
        {
            return new CartViewModel
            {
                ShippingFee = dto.ShippingFee,
                Items = dto.Items.Select(i => new CartItem
                {
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    ProductNameEn = i.ProductNameEn,
                    Emoji = i.Emoji,
                    ImagePath = i.ImagePath,
                    BrandName = i.BrandName,
                    UnitPrice = i.UnitPrice,
                    Quantity = i.Quantity
                }).ToList()
            };
        }
    }
}
