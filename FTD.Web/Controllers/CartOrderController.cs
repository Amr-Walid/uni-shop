using FTD.Web.Services;
using FTD.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace FTD.Web.Controllers
{
    // ── CART ──────────────────────────────────────────────────────────────────
    public class CartController : Controller
    {
        private readonly CartService _cart;

        public CartController(CartService cart) => _cart = cart;

        public async Task<IActionResult> Index()
        {
            var vm = await _cart.GetCartAsync(HttpContext.Session);
            return View(vm);
        }

        [HttpPost]
        public IActionResult Add(int productId, int qty = 1)
        {
            _cart.AddItem(HttpContext.Session, productId, qty);
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = true, count = _cart.GetCount(HttpContext.Session) });
            return Redirect("/Cart");
        }

        [HttpPost]
        public IActionResult Update(int productId, int qty)
        {
            _cart.UpdateQty(HttpContext.Session, productId, qty);
            return Redirect("/Cart");
        }

        [HttpPost]
        public IActionResult Remove(int productId)
        {
            _cart.RemoveItem(HttpContext.Session, productId);
            return Redirect("/Cart");
        }

        [HttpPost]
        public IActionResult Clear()
        {
            _cart.ClearCart(HttpContext.Session);
            return Redirect("/Cart");
        }

        // AJAX: get cart count for nav badge
        [HttpGet]
        public IActionResult Count()
            => Json(new { count = _cart.GetCount(HttpContext.Session) });
    }

    // ── ORDER ─────────────────────────────────────────────────────────────────
    public class OrderController : Controller
    {
        private readonly CartService _cart;
        private readonly OrderService _orders;

        public OrderController(CartService cart, OrderService orders)
        {
            _cart = cart;
            _orders = orders;
        }

        // GET /Order/Checkout
        public async Task<IActionResult> Checkout()
        {
            var cart = await _cart.GetCartAsync(HttpContext.Session);
            if (!cart.Items.Any())
                return Redirect("/Cart");

            var vm = new CheckoutViewModel { Cart = cart };
            return View(vm);
        }

        // POST /Order/Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutViewModel vm)
        {
            vm.Cart = await _cart.GetCartAsync(HttpContext.Session);

            if (!vm.Cart.Items.Any())
                return Redirect("/Cart");

            if (!ModelState.IsValid)
                return View(vm);

            var order = await _orders.CreateOrderAsync(vm, vm.Cart);
            _cart.ClearCart(HttpContext.Session);

            return Redirect("/Order/Confirmation?orderNumber=" + order.OrderNumber);
        }

        // GET /Order/Confirmation/{orderNumber}
        public IActionResult Confirmation(string orderNumber)
        {
            ViewBag.OrderNumber = orderNumber;
            return View();
        }
    }

    // ── PAGE ──────────────────────────────────────────────────────────────────
    public class PageController : Controller
    {
        private readonly FTD.Web.Services.ContentService _content;

        public PageController(FTD.Web.Services.ContentService content) => _content = content;

        public async Task<IActionResult> Show(string slug)
        {
            var page = await _content.GetPageBySlugAsync(slug);
            if (page == null) return NotFound();

            var vm = new ContentPageViewModel { Page = page };
            return View(vm);
        }
    }
}
