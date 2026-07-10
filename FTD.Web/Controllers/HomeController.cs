using FTD.Application.Interfaces;
using FTD.Application.Services;
using FTD.Application.DTOs;
using FTD.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FTD.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductService _products;
        private readonly IContentService _content;
        private readonly IMessageService _messages;

        public HomeController(
            IProductService products,
            IContentService content,
            IMessageService messages)
        {
            _products = products;
            _content = content;
            _messages = messages;
        }

        public async Task<IActionResult> Index()
        {
            var featured = await _products.GetFeaturedAsync(6);
            var categoriesList = await _products.GetActiveCategoriesAsync();
            var settingsList = await _content.GetSettingsListAsync();

            var vm = new HomeViewModel
            {
                FeaturedProducts = featured,
                Categories = categoriesList,
                ContentBlocks = await _content.GetBlocksAsync(),
                ContactInfo = await _content.GetContactInfoAsync(),
                Settings = settingsList
            };
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(string name, string email, string phone, string message)
        {
            var dto = new ContactMessageDto
            {
                Name = name,
                Email = email,
                Phone = phone,
                Message = message
            };

            await _messages.SaveMessageAsync(dto);

            TempData["ContactSuccess"] = "true";
            return Redirect("/#contact");
        }

        public IActionResult Error() => View();
    }
}
