using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;
using Nop.Plugin.Widgets.AnywhereBanner.Factories;
using System.Threading.Tasks;

namespace Nop.Plugin.Widgets.AnywhereBanner.Components
{   
    public class AnywhereBannerViewComponent : NopViewComponent
    {
        #region Fields        
        private readonly IAnywhereBannerFactory _anywhereBannerFactory;
        #endregion

        #region Ctor
        public AnywhereBannerViewComponent(IAnywhereBannerFactory anywhereBannerFactory)
        {
            _anywhereBannerFactory = anywhereBannerFactory;
        }
        #endregion

        #region Methods
        public async Task<IViewComponentResult> InvokeAsync(string widgetZone)
        {
            if (widgetZone == null)
                return Content(string.Empty);

            var model = await _anywhereBannerFactory.PrepareAnywhereBannerDetailsModelsAsync(widgetZone);

            if (model == null)
                return Content(string.Empty);

            return View(model);
        }
        #endregion
    }
}