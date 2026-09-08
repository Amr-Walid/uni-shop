using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FTD.Application.DTOs
{
    // ═══════════════════════════════════════════════════════════════════════════
    //  CART / WISHLIST / ORDER CONTRACTS FOR THE MOBILE CLIENT
    // ═══════════════════════════════════════════════════════════════════════════

    public class AddCartItemRequestDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "معرف المنتج غير صالح")]
        public int ProductId { get; set; }

        [Range(1, 1000, ErrorMessage = "الكمية يجب أن تكون بين 1 و 1000")]
        public int Quantity { get; set; } = 1;
    }

    public class UpdateCartItemRequestDto
    {
        // 0 is allowed and means "remove this line", matching the existing
        // CartService.UpdateQty semantics.
        [Range(0, 1000, ErrorMessage = "الكمية يجب أن تكون بين 0 و 1000")]
        public int Quantity { get; set; }
    }

    /// <summary>
    /// A guest cart being merged into the signed-in account's server cart.
    /// </summary>
    public class MergeCartRequestDto
    {
        [Required(ErrorMessage = "عناصر السلة مطلوبة")]
        [MaxLength(200, ErrorMessage = "عدد العناصر كبير جداً")]
        public List<AddCartItemRequestDto> Items { get; set; } = new();

        /// <summary>
        /// When true the incoming guest cart replaces the stored cart; otherwise
        /// quantities are summed. Defaults to merging, which is the
        /// least-surprising behaviour — the customer never loses items.
        /// </summary>
        public bool Replace { get; set; } = false;
    }

    /// <summary>
    /// Request for the pre-checkout validation sweep.
    /// </summary>
    public class ValidateCartRequestDto
    {
        [Required(ErrorMessage = "عناصر السلة مطلوبة")]
        [MinLength(1, ErrorMessage = "سلة التسوق فارغة")]
        [MaxLength(200, ErrorMessage = "عدد العناصر كبير جداً")]
        public List<AddCartItemRequestDto> Items { get; set; } = new();
    }

    /// <summary>
    /// Result of re-validating a client-held cart against live data.
    ///
    /// WHY THIS ENDPOINT EXISTS: the mobile cart lives on the device and can be
    /// days old. Prices change, products get deactivated and stock runs out. If
    /// the app discovered that only at checkout, the customer would have already
    /// typed their whole address. Validating first turns a dead end into a
    /// correctable prompt.
    /// </summary>
    public class CartValidationResultDto
    {
        /// <summary>True when nothing changed and checkout can proceed.</summary>
        public bool IsValid { get; set; } = true;

        public List<CartItemDto> Items { get; set; } = new();
        public decimal SubTotal { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal Total { get; set; }
        public bool FreeShipping { get; set; }
        public decimal FreeShippingAbove { get; set; }

        /// <summary>Per-item problems the user must acknowledge.</summary>
        public List<CartItemIssueDto> Issues { get; set; } = new();
    }

    public class CartItemIssueDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";

        /// <summary>
        /// One of: REMOVED (gone/inactive), PRICE_CHANGED, QUANTITY_REDUCED,
        /// OUT_OF_STOCK. Stable code — the client decides the wording.
        /// </summary>
        public string IssueCode { get; set; } = "";
        public string Message { get; set; } = "";

        public decimal? OldPrice { get; set; }
        public decimal? NewPrice { get; set; }
        public int? RequestedQuantity { get; set; }
        public int? AvailableQuantity { get; set; }
    }

    /// <summary>Cart snapshot with money already totalled by the server.</summary>
    public class CartResponseDto
    {
        public List<CartItemDto> Items { get; set; } = new();
        public int ItemsCount { get; set; }
        public decimal SubTotal { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal Total { get; set; }
        public bool FreeShipping { get; set; }
        public decimal FreeShippingAbove { get; set; }

        /// <summary>How much more the customer must add to unlock free shipping.</summary>
        public decimal RemainingForFreeShipping { get; set; }
    }

    /// <summary>Wishlist row — reuses the list-item shape so one widget renders both.</summary>
    public class WishlistItemDto
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public ProductListItemDto Product { get; set; } = new();
    }

    // ── Orders ────────────────────────────────────────────────────────────────

    /// <summary>Mobile checkout request (works for guests and signed-in users).</summary>
    public class MobileCheckoutRequestDto
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
        public string? City { get; set; }

        [StringLength(100, ErrorMessage = "اسم المحافظة طويل جداً")]
        public string? Governorate { get; set; }

        [StringLength(1000, ErrorMessage = "الملاحظات طويلة جداً")]
        public string? Notes { get; set; }

        /// <summary>Only "cod" is supported today; reserved for online payment.</summary>
        [StringLength(20)]
        public string PaymentMethod { get; set; } = "cod";

        [Required(ErrorMessage = "سلة التسوق مطلوبة")]
        [MinLength(1, ErrorMessage = "سلة التسوق فارغة")]
        [MaxLength(200, ErrorMessage = "عدد العناصر في السلة كبير جداً")]
        public List<AddCartItemRequestDto> Items { get; set; } = new();
    }

    /// <summary>Confirmation returned right after a successful checkout.</summary>
    public class CheckoutResponseDto
    {
        public bool Success { get; set; } = true;
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = "";
        public decimal SubTotal { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal Total { get; set; }
        public DateTime CreatedAt { get; set; }
        public OrderStatusSummaryDto? Status { get; set; }
    }

    public class OrderStatusSummaryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string ColorHex { get; set; } = "";
        public string? Icon { get; set; }
        public int SortOrder { get; set; }
    }

    /// <summary>Order row in the "my orders" list — intentionally light.</summary>
    public class OrderListItemDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = "";
        public decimal Total { get; set; }
        public int ItemsCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public OrderStatusSummaryDto? Status { get; set; }

        /// <summary>Thumbnail of the first line, for a recognisable list row.</summary>
        public string? FirstItemImageUrl { get; set; }

        /// <summary>Precomputed so the app never re-implements the status rules.</summary>
        public bool CanCancel { get; set; }
    }

    /// <summary>Full order detail for the order page / tracking screen.</summary>
    public class OrderDetailDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = "";

        public string CustomerName { get; set; } = "";
        public string CustomerPhone { get; set; } = "";
        public string? CustomerEmail { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Governorate { get; set; }
        public string? Notes { get; set; }

        public decimal SubTotal { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal Total { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public OrderStatusSummaryDto? Status { get; set; }
        public bool CanCancel { get; set; }

        public List<OrderLineDto> Items { get; set; } = new();

        /// <summary>
        /// The full status ladder with the reached steps flagged, so the app can
        /// draw a progress tracker without hardcoding the workflow.
        /// </summary>
        public List<OrderTimelineStepDto> Timeline { get; set; } = new();
    }

    public class OrderLineDto
    {
        public int ProductId { get; set; }

        /// <summary>Historical snapshot taken at purchase time.</summary>
        public string ProductName { get; set; } = "";
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal SubTotal { get; set; }
        public string? ImageUrl { get; set; }

        /// <summary>Null when the product was since removed from the catalog.</summary>
        public string? ProductSlug { get; set; }
    }

    public class OrderTimelineStepDto
    {
        public int StatusId { get; set; }
        public string Name { get; set; } = "";
        public string ColorHex { get; set; } = "";
        public string? Icon { get; set; }
        public bool IsReached { get; set; }
        public bool IsCurrent { get; set; }
    }

    // ── Devices (push notifications) ──────────────────────────────────────────

    public class RegisterDeviceRequestDto
    {
        [Required(ErrorMessage = "رمز الجهاز مطلوب")]
        [StringLength(500, ErrorMessage = "رمز الجهاز طويل جداً")]
        public string Token { get; set; } = "";

        [Required(ErrorMessage = "نوع المنصة مطلوب")]
        [RegularExpression("^(android|ios|web)$", ErrorMessage = "المنصة يجب أن تكون android أو ios أو web")]
        public string Platform { get; set; } = "";

        [StringLength(100)] public string? DeviceModel { get; set; }
        [StringLength(30)] public string? AppVersion { get; set; }
        [StringLength(5)] public string? Language { get; set; }
    }
}
