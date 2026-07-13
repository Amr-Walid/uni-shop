using System.ComponentModel.DataAnnotations;

namespace FTD.Api.Models.Requests
{
    /// <summary>
    /// طلب تحديث حالة الطلب من لوحة الأدمن.
    /// </summary>
    public class UpdateStatusRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "معرف الحالة غير صالح")]
        public int StatusId { get; set; }
    }
}
