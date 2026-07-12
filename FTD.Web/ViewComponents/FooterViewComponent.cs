using System.Linq;
using System.Threading.Tasks;
using FTD.Application.Interfaces;
using FTD.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace FTD.Web.ViewComponents
{
    public class FooterViewComponent : ViewComponent
    {
        private readonly IContentService _contentService;

        public FooterViewComponent(IContentService contentService)
        {
            _contentService = contentService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var navItems = await _contentService.GetNavigationItemsAsync();
            var contactInfo = await _contentService.GetContactInfoAsync();
            var brands = await _contentService.GetActiveBrandsAsync();
            var blocks = await _contentService.GetBlocksAsync();
            var settings = await _contentService.GetSettingsAsync();

            var vm = new FooterViewModel
            {
                FootItems = navItems.Where(n => n.Location == "Footer" || n.Location == "Both").ToList(),
                ContactInfo = contactInfo,
                NavBrands = brands,
                Blocks = blocks,
                Settings = settings
            };

            return View(vm);
        }
    }
}
