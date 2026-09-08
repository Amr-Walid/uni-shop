using System;
using System.Linq;
using System.Threading.Tasks;
using FTD.Api.Infrastructure;
using FTD.Application.Common;
using FTD.Application.DTOs;
using FTD.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace FTD.Api.Controllers.V1
{
    /// <summary>
    /// CMS content, contact details, the contact form, push-device
    /// registration and the remote app configuration.
    /// </summary>
    [ApiController]
    [Route("api/v1")]
    [Tags("Content & App")]
    public class ContentController : ApiControllerBase
    {
        private static readonly TimeSpan ContentCacheDuration = TimeSpan.FromMinutes(5);

        private readonly IContentService _content;
        private readonly IMessageService _messages;
        private readonly IEmailService _email;
        private readonly IDeviceTokenService _devices;
        private readonly ICatalogQueryService _catalog;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _config;

        public ContentController(
            IContentService content,
            IMessageService messages,
            IEmailService email,
            IDeviceTokenService devices,
            ICatalogQueryService catalog,
            IMemoryCache cache,
            IConfiguration config)
        {
            _content = content;
            _messages = messages;
            _email = email;
            _devices = devices;
            _catalog = catalog;
            _cache = cache;
            _config = config;
        }

        /// <summary>
        /// A published CMS page with its sections (terms, privacy policy,
        /// about us…).
        ///
        /// The app renders these instead of embedding legal text in the bundle,
        /// so wording can be corrected without a store release — and the store
        /// listings can link to a live privacy-policy URL.
        /// </summary>
        /// <response code="404">No published page with this slug (PAGE_NOT_FOUND).</response>
        [HttpGet("content/pages/{slug}")]
        [ProducesResponseType(typeof(ContentPageDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPage(string slug)
        {
            var page = await _content.GetPageBySlugAsync(slug);

            return page == null
                ? NotFoundProblem("الصفحة غير موجودة", ApiErrorCodes.PageNotFound)
                : Ok(page);
        }

        /// <summary>
        /// Store contact details and social links.
        ///
        /// Respects the per-field visibility flags the admin controls, so a
        /// channel hidden on the website is not exposed through the API either.
        /// </summary>
        [HttpGet("content/contact-info")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetContactInfo()
        {
            var info = await _cache.GetOrCreateAsync("api:contact-info", entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = ContentCacheDuration;
                return _content.GetContactInfoAsync();
            });

            if (info == null) return Ok(new { });

            var lang = RequestLanguage;

            // Only emit values whose Show* flag is enabled — the flags exist so
            // the admin can hide a channel, and the API must honour that.
            return Ok(new
            {
                phone = info.ShowPhone ? info.Phone : null,
                phone2 = info.ShowPhone2 ? info.Phone2 : null,
                email = info.ShowEmail ? info.Email : null,
                address = info.ShowAddress
                    ? LocalizationHelper.PickOptional(info.AddressAr, info.AddressEn, lang)
                    : null,
                city = info.ShowAddress ? info.City : null,
                mapEmbedUrl = info.ShowMap ? info.MapEmbedUrl : null,
                workingHours = info.ShowWorkingHours
                    ? LocalizationHelper.PickOptional(info.WorkingHoursAr, info.WorkingHoursEn, lang)
                    : null,
                social = new
                {
                    facebook = info.ShowFacebook ? info.Facebook : null,
                    instagram = info.ShowInstagram ? info.Instagram : null,
                    whatsapp = info.ShowWhatsApp ? info.WhatsApp : null,
                    tiktok = info.ShowTikTok ? info.TikTok : null
                }
            });
        }

        /// <summary>
        /// Submits a contact-form message and notifies the admin by email.
        /// Rate limited to 3 per 10 minutes per IP to keep the inbox clean.
        /// </summary>
        [HttpPost("contact")]
        [EnableRateLimiting("contact-policy")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SendContactMessage([FromBody] ContactMessageDto dto)
        {
            await _messages.SaveMessageAsync(dto);

            // Notify after persisting: the message must survive even if SMTP is
            // down. EmailService already swallows and logs its own failures.
            await _email.SendContactNotificationAsync(
                dto.Name ?? string.Empty,
                dto.Email ?? string.Empty,
                dto.Phone ?? string.Empty,
                dto.Message ?? string.Empty);

            return Ok(new { success = true, message = "تم إرسال رسالتك بنجاح. سنتواصل معك قريباً." });
        }

        /// <summary>
        /// Registers this installation for push notifications.
        ///
        /// Anonymous calls are allowed on purpose: guests must still receive
        /// order-status notifications. When the same device later signs in, the
        /// token is re-registered and bound to the account.
        /// </summary>
        [HttpPost("devices/register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> RegisterDevice([FromBody] RegisterDeviceRequestDto request)
        {
            // CurrentUserId is null for anonymous callers, which the service
            // handles by storing an unowned token.
            await _devices.RegisterAsync(request, CurrentUserId);

            return Ok(new { success = true, message = "تم تسجيل الجهاز للإشعارات" });
        }

        /// <summary>
        /// Deactivates a push token. Call this on sign-out so the next account
        /// on this handset does not inherit the previous user's notifications.
        /// </summary>
        [HttpPost("devices/unregister")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> UnregisterDevice([FromBody] RegisterDeviceRequestDto request)
        {
            await _devices.UnregisterAsync(request.Token);
            return Ok(new { success = true });
        }

        /// <summary>
        /// Remote app configuration: minimum supported version, maintenance
        /// mode and the public store settings.
        ///
        /// The app should call this at launch. It is the ONLY mechanism for
        /// reacting to a critical bug in a build that is already installed —
        /// otherwise you must wait weeks for users to update voluntarily.
        /// </summary>
        [HttpGet("app/config")]
        [ProducesResponseType(typeof(AppConfigDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAppConfig()
        {
            var settings = await _catalog.GetPublicSettingsAsync();

            // Read from configuration so operations can flip maintenance mode or
            // raise the minimum version without a code deployment.
            var section = _config.GetSection("MobileApp");

            var config = new AppConfigDto
            {
                MinSupportedVersion = section["MinSupportedVersion"] ?? "1.0.0",
                LatestVersion = section["LatestVersion"] ?? "1.0.0",
                ForceUpdate = bool.TryParse(section["ForceUpdate"], out var force) && force,
                UpdateMessageAr = section["UpdateMessageAr"]
                    ?? "يتوفر إصدار جديد من التطبيق. حدّث الآن للاستمرار.",
                UpdateMessageEn = section["UpdateMessageEn"]
                    ?? "A new version is available. Please update to continue.",
                AndroidStoreUrl = section["AndroidStoreUrl"],
                IosStoreUrl = section["IosStoreUrl"],
                MaintenanceMode = bool.TryParse(section["MaintenanceMode"], out var maintenance) && maintenance,
                MaintenanceMessageAr = section["MaintenanceMessageAr"]
                    ?? "المتجر تحت الصيانة حالياً. نعود قريباً.",
                MaintenanceMessageEn = section["MaintenanceMessageEn"]
                    ?? "The store is under maintenance. We will be back shortly.",
                Settings = settings
            };

            return Ok(config);
        }
    }
}
