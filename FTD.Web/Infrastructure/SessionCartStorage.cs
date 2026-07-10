using FTD.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace FTD.Web.Infrastructure
{
    public class SessionCartStorage : ICartStorage
    {
        private readonly ISession _session;
        private const string CartKey = "ftd_cart";

        public SessionCartStorage(IHttpContextAccessor accessor)
        {
            var httpContext = accessor.HttpContext ?? throw new InvalidOperationException("HttpContext is not available.");
            _session = httpContext.Session;
        }

        public string? GetRaw() => _session.GetString(CartKey);

        public void SetRaw(string json) => _session.SetString(CartKey, json);

        public void Clear() => _session.Remove(CartKey);
    }
}
