namespace FTD.Application.DTOs
{
    public class DashboardDto
    {
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public int NewOrders { get; set; }
        public int PendingOrders { get; set; }
        public decimal TodayRevenue { get; set; }
        public decimal MonthRevenue { get; set; }
        public List<SalesOrderDto> RecentOrders { get; set; } = new();
        public List<OrderStatusCountDto> OrdersByStatus { get; set; } = new();
    }

    public class OrderStatusCountDto
    {
        public string StatusName { get; set; } = "";
        public string ColorHex { get; set; } = "";
        public int Count { get; set; }
    }

    public class AttributeFilterGroupDto
    {
        public int AttributeId { get; set; }
        public string NameAr { get; set; } = "";
        public string NameEn { get; set; } = "";
        public List<AttributeFilterOptionDto> Options { get; set; } = new();
    }

    public class AttributeFilterOptionDto
    {
        public int ValueId { get; set; }
        public string ValueAr { get; set; } = "";
        public string ValueEn { get; set; } = "";
        public int Count { get; set; }
    }
}
