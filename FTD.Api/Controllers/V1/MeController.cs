using System.Threading.Tasks;
using FTD.Api.Infrastructure;
using FTD.Application.Common;
using FTD.Application.DTOs;
using FTD.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FTD.Api.Controllers.V1
{
    /// <summary>
    /// The signed-in customer's own profile and wishlist.
    ///
    /// Every action derives the user id from the validated token, so no route or
    /// body parameter can be used to reach another customer's data.
    /// </summary>
    [ApiController]
    [Route("api/v1")]
    [Authorize]
    [Tags("Profile & Wishlist")]
    public class MeController : ApiControllerBase
    {
        private readonly IAuthService _auth;
        private readonly IWishlistService _wishlist;

        public MeController(IAuthService auth, IWishlistService wishlist)
        {
            _auth = auth;
            _wishlist = wishlist;
        }

        /// <summary>The caller's profile.</summary>
        [HttpGet("me")]
        [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProfile()
        {
            var profile = await _auth.GetProfileAsync(CurrentUserId!);

            return profile == null
                ? NotFoundProblem("الحساب غير موجود", ApiErrorCodes.NotFound)
                : Ok(profile);
        }

        /// <summary>
        /// Updates the caller's profile.
        /// Omitted (null) fields are left unchanged, so the client can send a
        /// single field without having to resend the whole profile.
        /// </summary>
        [HttpPut("me")]
        [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequestDto request)
        {
            var profile = await _auth.UpdateProfileAsync(CurrentUserId!, request);

            return profile == null
                ? BadRequestProblem("تعذّر تحديث البيانات", ApiErrorCodes.BadRequest)
                : Ok(profile);
        }

        /// <summary>
        /// The caller's saved default address, used to pre-fill checkout.
        ///
        /// Exposed as a single address rather than a collection: the schema
        /// stores one default address per account today, and returning a list
        /// would imply multi-address support that does not exist yet.
        /// </summary>
        [HttpGet("me/address")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDefaultAddress()
        {
            var profile = await _auth.GetProfileAsync(CurrentUserId!);

            if (profile == null)
                return NotFoundProblem("الحساب غير موجود", ApiErrorCodes.NotFound);

            return Ok(new
            {
                fullName = profile.FullName,
                phone = profile.Phone,
                address = profile.DefaultAddress,
                city = profile.City,
                governorate = profile.Governorate
            });
        }

        // ── Wishlist ──────────────────────────────────────────────────────────

        /// <summary>
        /// One page of the caller's wishlist.
        /// Products the admin has since deactivated are filtered out so nothing
        /// unbuyable is shown.
        /// </summary>
        [HttpGet("wishlist")]
        [ProducesResponseType(typeof(PagedResult<WishlistItemDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetWishlist([FromQuery] int? page, [FromQuery] int? pageSize)
        {
            var (normalizedPage, normalizedSize) = PagedResult<WishlistItemDto>.Normalize(page, pageSize);

            var result = await _wishlist.GetAsync(
                CurrentUserId!, normalizedPage, normalizedSize, RequestLanguage, MediaBaseUrl);

            return Ok(result);
        }

        /// <summary>
        /// Saves a product to the wishlist.
        /// Idempotent — re-adding an already-saved product still returns 200, so
        /// a retried request on a flaky connection is safe.
        /// </summary>
        [HttpPost("wishlist/{productId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddToWishlist(int productId)
        {
            var added = await _wishlist.AddAsync(CurrentUserId!, productId);

            return added
                ? Ok(new { success = true, message = "تم إضافة المنتج إلى المفضلة" })
                : NotFoundProblem("المنتج غير موجود أو غير متاح", ApiErrorCodes.ProductNotFound);
        }

        /// <summary>Removes a product from the wishlist.</summary>
        [HttpDelete("wishlist/{productId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveFromWishlist(int productId)
        {
            var removed = await _wishlist.RemoveAsync(CurrentUserId!, productId);

            return removed
                ? Ok(new { success = true, message = "تم إزالة المنتج من المفضلة" })
                : NotFoundProblem("المنتج غير موجود في المفضلة", ApiErrorCodes.NotFound);
        }

        /// <summary>
        /// Whether a product is in the wishlist, plus the total count.
        /// Lets the product page render the heart icon without downloading the
        /// entire wishlist.
        /// </summary>
        [HttpGet("wishlist/{productId:int}/status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetWishlistStatus(int productId)
        {
            var inWishlist = await _wishlist.ContainsAsync(CurrentUserId!, productId);
            var count = await _wishlist.GetCountAsync(CurrentUserId!);

            return Ok(new { productId, inWishlist, totalCount = count });
        }
    }
}
