namespace FTD.Application.DTOs
{
    public class OrderStatusDto
    {
        public int Id { get; set; }
        public string NameAr { get; set; } = "";
        public string NameEn { get; set; } = "";
        public string ColorHex { get; set; } = "";
        public string? Icon { get; set; }
        public int SortOrder { get; set; }
    }

    public class SalesOrderDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = "";
        public int StatusId { get; set; }
        public string CustomerName { get; set; } = "";
        public string CustomerPhone { get; set; } = "";
        public string? CustomerEmail { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Governorate { get; set; }
        public decimal SubTotal { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Notes { get; set; }
        public string? AdminNotes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public OrderStatusDto? Status { get; set; }
        public List<SalesOrderDetailDto> Details { get; set; } = new();
    }

    public class SalesOrderDetailDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal SubTotal { get; set; }
        public ProductDto? Product { get; set; }
    }

    public class CheckoutDto
    {
        public string CustomerName { get; set; } = "";
        public string CustomerPhone { get; set; } = "";
        public string? CustomerEmail { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Governorate { get; set; }
        public string? Notes { get; set; }
    }
}
