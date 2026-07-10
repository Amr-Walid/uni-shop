using FTD.Application.DTOs;
using FTD.Application.Interfaces;
using FTD.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace FTD.Web.Controllers
{
    // ── CART ──────────────────────────────────────────────────────────────────
    public class CartController : Controller
    {
        private readonly ICartService _cart;

        public CartController(ICartService cart) => _cart = cart;

        public async Task<IActionResult> Index()
        {
            var cartDto = await _cart.GetCartAsync(HttpContext.Session);
            var vm = CartViewModelMapper.FromDto(cartDto);
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
        private readonly ICartService _cart;
        private readonly IOrderService _orders;

        public OrderController(ICartService cart, IOrderService orders)
        {
            _cart = cart;
            _orders = orders;
        }

        // GET /Order/Checkout
        public async Task<IActionResult> Checkout()
        {
            var cartDto = await _cart.GetCartAsync(HttpContext.Session);
            if (!cartDto.Items.Any())
                return Redirect("/Cart");

            var vm = new CheckoutViewModel { Cart = CartViewModelMapper.FromDto(cartDto) };
            return View(vm);
        }

        // POST /Order/Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutViewModel vm)
        {
            var cartDto = await _cart.GetCartAsync(HttpContext.Session);
            vm.Cart = CartViewModelMapper.FromDto(cartDto);

            if (!cartDto.Items.Any())
                return Redirect("/Cart");

            if (!ModelState.IsValid)
                return View(vm);

            var checkoutDto = new CheckoutDto
            {
                CustomerName = vm.CustomerName,
                CustomerPhone = vm.CustomerPhone,
                CustomerEmail = vm.CustomerEmail,
                Address = vm.Address,
                City = vm.City,
                Governorate = vm.Governorate,
                Notes = vm.Notes
            };

            try
            {
                var order = await _orders.CreateOrderAsync(checkoutDto, cartDto);
                _cart.ClearCart(HttpContext.Session);
                return Redirect("/Order/Confirmation?orderNumber=" + order.OrderNumber);
            }
            catch (System.InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(vm);
            }
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
        private readonly IContentService _content;

        public PageController(IContentService content) => _content = content;

        public async Task<IActionResult> Show(string slug)
        {
            var page = await _content.GetPageBySlugAsync(slug);
            if (page == null) return NotFound();

            var vm = new ContentPageViewModel { Page = page };
            return View(vm);
        }
    }
}
