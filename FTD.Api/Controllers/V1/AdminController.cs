using System.Collections.Generic;
using System.Threading.Tasks;
using FTD.Api.Infrastructure;
using FTD.Api.Models.Requests;
using FTD.Application.Common;
using FTD.Application.DTOs;
using FTD.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FTD.Api.Controllers.V1
{
    /// <summary>
    /// Administrative endpoints. Requires a Bearer token whose account holds
    /// the Admin role.
    /// </summary>
    [ApiController]
    [Route("api/v1/admin")]
    [Authorize(Roles = "Admin")]
    [Tags("Admin")]
    public class AdminController : ApiControllerBase
    {
        private readonly IDashboardService _dashboard;
        private readonly IOrderService _orders;
        private readonly IMessageService _messages;

        public AdminController(
            IDashboardService dashboard,
            IOrderService orders,
            IMessageService messages)
        {
            _dashboard = dashboard;
            _orders = orders;
            _messages = messages;
        }

        /// <summary>Dashboard KPIs: totals, revenue and recent orders.</summary>
        [HttpGet("dashboard")]
        [ProducesResponseType(typeof(DashboardDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDashboard()
            => Ok(await _dashboard.GetDashboardDataAsync());

        /// <summary>All orders, optionally filtered by status.</summary>
        /// <param name="statusId">Optional status filter.</param>
        [HttpGet("orders")]
        [ProducesResponseType(typeof(List<SalesOrderDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetOrders([FromQuery] int? statusId)
            => Ok(await _orders.GetOrdersAsync(statusId));

        /// <summary>Full detail of any order (admin scope — no ownership filter).</summary>
        [HttpGet("orders/{id:int}")]
        [ProducesResponseType(typeof(SalesOrderDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetOrder(int id)
        {
            var order = await _orders.GetOrderByIdAsync(id);

            return order == null
                ? NotFoundProblem("الطلب غير موجود", ApiErrorCodes.OrderNotFound)
                : Ok(order);
        }

        /// <summary>
        /// Moves an order to a new status.
        ///
        /// Exposed as PUT (the documented contract) with POST kept as an alias,
        /// because the previously shipped API used POST and existing callers
        /// must not break.
        /// </summary>
        /// <response code="400">Unknown status id (ORDER_STATUS_INVALID).</response>
        [HttpPut("orders/{id:int}/status")]
        [HttpPost("orders/{id:int}/status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateStatusRequest request)
        {
            var order = await _orders.GetOrderByIdAsync(id);
            if (order == null)
                return NotFoundProblem("الطلب غير موجود", ApiErrorCodes.OrderNotFound);

            // UpdateStatusAsync validates the status id against the table and
            // throws InvalidOperationException for an unknown value, which
            // GlobalExceptionHandler turns into a 400 rather than an FK 500.
            await _orders.UpdateStatusAsync(id, request.StatusId);

            return Ok(new { success = true, message = "تم تحديث حالة الطلب بنجاح" });
        }

        /// <summary>All order statuses, for building an admin status picker.</summary>
        [HttpGet("orders/statuses")]
        [ProducesResponseType(typeof(List<OrderStatusDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetStatuses()
            => Ok(await _orders.GetAllStatusesAsync());

        /// <summary>Contact-form messages received from customers.</summary>
        [HttpGet("messages")]
        [ProducesResponseType(typeof(List<ContactMessageDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMessages()
            => Ok(await _messages.GetAllMessagesAsync());

        /// <summary>Number of unread messages, for the admin badge.</summary>
        [HttpGet("messages/unread-count")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUnreadCount()
            => Ok(new { unreadCount = await _messages.GetUnreadCountAsync() });

        /// <summary>Marks a message as read.</summary>
        [HttpPost("messages/{id:int}/read")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> MarkMessageRead(int id)
        {
            await _messages.MarkReadAsync(id);
            return Ok(new { success = true });
        }

        /// <summary>Deletes a message.</summary>
        [HttpDelete("messages/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteMessage(int id)
        {
            await _messages.DeleteAsync(id);
            return Ok(new { success = true });
        }
    }
}
