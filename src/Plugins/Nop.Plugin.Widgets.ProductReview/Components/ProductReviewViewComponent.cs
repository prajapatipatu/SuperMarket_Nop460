using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;
using Nop.Web.Models.Catalog;
using SS.Plugin.Widgets.ProductReview.Factories;

namespace SS.Plugin.Widgets.ProductReview.Components
{
    public class ProductReviewViewComponent : NopViewComponent
    {
        #region Fields        
        private readonly IWidgetProductReviewModelFactory _widgetProductReviewModelFactory;
        #endregion

        #region Ctor
        public ProductReviewViewComponent(IWidgetProductReviewModelFactory widgetProductReviewModelFactory)
        {
            _widgetProductReviewModelFactory = widgetProductReviewModelFactory;            
        }
        #endregion

        #region Method
        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            if (widgetZone == null)
                return Content(string.Empty);

            if (!(additionalData is ProductDetailsModel productDetailsModel))
                return Content("");

            var model = await _widgetProductReviewModelFactory.PrepareWidgetProductReviewsDetailsModelAsync(productDetailsModel.Id);

            if (model == null)
                return Content(string.Empty);

            return View(model);
        }
        #endregion
    }
}
