using System.Collections.Generic;
using System.Threading.Tasks;
using FTD.Application.DTOs;

namespace FTD.Application.Interfaces
{
    public interface IOrderService
    {
        /// <summary>
        /// Creates an order. <paramref name="userId"/> links it to a customer
        /// account; leave it null for a guest checkout (the storefront's
        /// existing behaviour, which must keep working unchanged).
        /// </summary>
        Task<SalesOrderDto> CreateOrderAsync(CheckoutDto checkout, CartDto cart, string? userId = null);

        Task UpdateStatusAsync(int orderId, int statusId);
        Task<List<SalesOrderDto>> GetOrdersAsync(int? statusId);
        Task<SalesOrderDto?> GetOrderByIdAsync(int id);
        Task<List<OrderStatusDto>> GetAllStatusesAsync();

        // ── Mobile / customer-facing order access ─────────────────────────────

        /// <summary>
        /// One page of the signed-in customer's own orders.
        /// Filtering happens in the query by <paramref name="userId"/>, so a
        /// caller can never page through somebody else's history.
        /// </summary>
        Task<PagedResult<OrderListItemDto>> GetMyOrdersAsync(
            string userId, int page, int pageSize, string lang, string? mediaBaseUrl);

        /// <summary>
        /// Order detail scoped to its owner.
        ///
        /// Passing <paramref name="userId"/> here (instead of checking ownership
        /// in the controller) makes the authorization part of the query itself,
        /// so an IDOR cannot be introduced by forgetting a check at a call site.
        /// </summary>
        Task<OrderDetailDto?> GetMyOrderDetailAsync(
            int orderId, string userId, string lang, string? mediaBaseUrl);

        /// <summary>
        /// Guest order tracking by order number.
        ///
        /// SECURITY: order numbers follow a guessable pattern
        /// (FTD + timestamp + 3 digits), so the number alone is NOT a secret.
        /// The caller must also supply the last 4 digits of the phone number on
        /// the order; without that second factor this endpoint would leak
        /// customer names, phones and addresses to anyone enumerating numbers.
        /// </summary>
        Task<OrderDetailDto?> TrackOrderAsync(
            string orderNumber, string phoneLast4, string lang, string? mediaBaseUrl);

        /// <summary>
        /// Cancels an order the customer owns and restores its stock.
        /// Only allowed while the order is still New or Confirmed.
        /// </summary>
        Task<(bool Success, string? ErrorCode, string? Message)> CancelMyOrderAsync(int orderId, string userId);

        /// <summary>
        /// Whether an order number already exists — used to guarantee
        /// uniqueness at generation time.
        /// </summary>
        Task<bool> OrderNumberExistsAsync(string orderNumber);
    }
}
