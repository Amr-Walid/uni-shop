using System.ComponentModel.DataAnnotations;

namespace FTD.Api.Models.Requests
{
    /// <summary>
    /// طلب إتمام الدفع وإنشاء طلب جديد عبر واجهة برمجية.
    /// Validation attributes mirror the SalesOrder entity column limits so that
    /// oversized input is rejected with a clean 400 (via [ApiController]
    /// automatic model validation) instead of surfacing as a DB-level exception.
    /// </summary>
    public class ApiCheckoutRequest
    {
        [Required(ErrorMessage = "الاسم مطلوب")]
        [StringLength(150, ErrorMessage = "الاسم طويل جداً")]
        public string CustomerName { get; set; } = "";

        [Required(ErrorMessage = "رقم الهاتف مطلوب")]
        [StringLength(20, ErrorMessage = "رقم الهاتف طويل جداً")]
        public string CustomerPhone { get; set; } = "";

        [EmailAddress(ErrorMessage = "صيغة البريد الإلكتروني غير صحيحة")]
        [StringLength(200, ErrorMessage = "البريد الإلكتروني طويل جداً")]
        public string? CustomerEmail { get; set; }

        [Required(ErrorMessage = "العنوان مطلوب")]
        [StringLength(300, ErrorMessage = "العنوان طويل جداً")]
        public string Address { get; set; } = "";

        [StringLength(100, ErrorMessage = "اسم المدينة طويل جداً")]
        public string City { get; set; } = "";

        [StringLength(100, ErrorMessage = "اسم المحافظة طويل جداً")]
        public string Governorate { get; set; } = "";

        [StringLength(1000, ErrorMessage = "الملاحظات طويلة جداً")]
        public string? Notes { get; set; }

        [Required(ErrorMessage = "سلة التسوق مطلوبة")]
        [MinLength(1, ErrorMessage = "سلة التسوق فارغة")]
        [MaxLength(200, ErrorMessage = "عدد العناصر في السلة كبير جداً")]
        public List<ApiCartItem> Items { get; set; } = new();
    }

    /// <summary>
    /// عنصر السلة المُرسل ضمن طلب الدفع.
    /// </summary>
    public class ApiCartItem
    {
        [Range(1, int.MaxValue, ErrorMessage = "معرف المنتج غير صالح")]
        public int ProductId { get; set; }

        [Range(1, 1000, ErrorMessage = "الكمية يجب أن تكون بين 1 و 1000")]
        public int Quantity { get; set; }
    }
}
