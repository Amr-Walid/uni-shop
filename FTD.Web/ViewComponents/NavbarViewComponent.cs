using System.Linq;
using System.Threading.Tasks;
using FTD.Application.Interfaces;
using FTD.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace FTD.Web.ViewComponents
{
    public class NavbarViewComponent : ViewComponent
    {
        private readonly IContentService _contentService;
        private readonly IProductService _productService;

        public NavbarViewComponent(IContentService contentService, IProductService productService)
        {
            _contentService = contentService;
            _productService = productService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var navItems = await _contentService.GetNavigationItemsAsync();
            var brands = await _contentService.GetActiveBrandsAsync();
            var categories = await _productService.GetActiveCategoriesAsync();

            var vm = new NavbarViewModel
            {
                NavItems = navItems.Where(n => n.Location == "Navbar" || n.Location == "Both").ToList(),
                NavBrands = brands,
                NavCategories = categories
            };

            return View(vm);
        }
    }
}
