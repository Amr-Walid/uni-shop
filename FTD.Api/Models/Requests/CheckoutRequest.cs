namespace FTD.Api.Models.Requests
{
    /// <summary>
    /// طلب إتمام الدفع وإنشاء طلب جديد عبر واجهة برمجية.
    /// </summary>
    public class ApiCheckoutRequest
    {
        public string CustomerName { get; set; } = "";
        public string CustomerPhone { get; set; } = "";
        public string? CustomerEmail { get; set; }
        public string Address { get; set; } = "";
        public string City { get; set; } = "";
        public string Governorate { get; set; } = "";
        public string? Notes { get; set; }
        public List<ApiCartItem> Items { get; set; } = new();
    }

    /// <summary>
    /// عنصر السلة المُرسل ضمن طلب الدفع.
    /// </summary>
    public class ApiCartItem
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
