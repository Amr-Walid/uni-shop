using System.Linq;
using System.Threading.Tasks;
using FTD.Application.Services;
using FTD.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace FTD.Web.ViewComponents
{
    public class FooterViewComponent : ViewComponent
    {
        private readonly ContentService _contentService;

        public FooterViewComponent(ContentService contentService)
        {
            _contentService = contentService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var navItems = await _contentService.GetNavigationItemsAsync();
            var contactInfo = await _contentService.GetContactInfoAsync();
            var brands = await _contentService.GetActiveBrandsAsync();

            var vm = new FooterViewModel
            {
                FootItems = navItems.Where(n => n.Location == "Footer" || n.Location == "Both").ToList(),
                ContactInfo = contactInfo,
                NavBrands = brands
            };

            return View(vm);
        }
    }
}
