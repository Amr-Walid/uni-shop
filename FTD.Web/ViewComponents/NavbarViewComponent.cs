using System.Linq;
using System.Threading.Tasks;
using FTD.Application.Services;
using FTD.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace FTD.Web.ViewComponents
{
    public class NavbarViewComponent : ViewComponent
    {
        private readonly ContentService _contentService;

        public NavbarViewComponent(ContentService contentService)
        {
            _contentService = contentService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var navItems = await _contentService.GetNavigationItemsAsync();
            var brands = await _contentService.GetActiveBrandsAsync();

            var vm = new NavbarViewModel
            {
                NavItems = navItems.Where(n => n.Location == "Navbar" || n.Location == "Both").ToList(),
                NavBrands = brands
            };

            return View(vm);
        }
    }
}
