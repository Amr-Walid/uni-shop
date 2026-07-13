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
        private readonly ICartStorage _storage;

        public CartController(ICartService cart, ICartStorage storage)
        {
            _cart = cart;
            _storage = storage;
        }

        public async Task<IActionResult> Index()
        {
            var cartDto = await _cart.GetCartAsync(_storage);
            var vm = CartViewModelMapper.FromDto(cartDto);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Add(int productId, int qty = 1)
        {
            _cart.AddItem(_storage, productId, qty);
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = true, count = _cart.GetCount(_storage) });
            return Redirect("/Cart");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Update(int productId, int qty)
        {
            _cart.UpdateQty(_storage, productId, qty);
            return Redirect("/Cart");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Remove(int productId)
        {
            _cart.RemoveItem(_storage, productId);
            return Redirect("/Cart");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Clear()
        {
            _cart.ClearCart(_storage);
            return Redirect("/Cart");
        }

        // AJAX: get cart count for nav badge
        [HttpGet]
        public IActionResult Count()
            => Json(new { count = _cart.GetCount(_storage) });
    }

    // ── ORDER ─────────────────────────────────────────────────────────────────
    public class OrderController : Controller
    {
        private readonly ICartService _cart;
        private readonly IOrderService _orders;
        private readonly ICartStorage _storage;

        public OrderController(ICartService cart, IOrderService orders, ICartStorage storage)
        {
            _cart = cart;
            _orders = orders;
            _storage = storage;
        }

        // GET /Order/Checkout
        public async Task<IActionResult> Checkout()
        {
            var cartDto = await _cart.GetCartAsync(_storage);
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
            var cartDto = await _cart.GetCartAsync(_storage);
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
                _cart.ClearCart(_storage);
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
