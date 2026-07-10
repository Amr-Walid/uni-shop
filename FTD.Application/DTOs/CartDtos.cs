namespace FTD.Application.DTOs
{
    public class CartItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public string? Emoji { get; set; }
        public string? ImagePath { get; set; }
        public string? BrandName { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal SubTotal => UnitPrice * Quantity;
    }

    public class CartDto
    {
        public List<CartItemDto> Items { get; set; } = new();
        public decimal SubTotal => Items.Sum(i => i.SubTotal);
        public decimal ShippingFee { get; set; }
        public decimal FreeShippingAbove { get; set; } = 5000;
        public bool FreeShipping => SubTotal >= FreeShippingAbove;
        public decimal Total => SubTotal + (FreeShipping ? 0 : ShippingFee);
    }
}
