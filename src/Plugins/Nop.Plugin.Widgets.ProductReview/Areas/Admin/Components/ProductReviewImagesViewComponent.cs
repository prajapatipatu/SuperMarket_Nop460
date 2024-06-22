using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Components;
using SS.Plugin.Widgets.ProductReview.Areas.Admin.Factories;


namespace SS.Plugin.Widgets.ProductReview.Areas.Admin.Components
{
    public class ProductReviewImagesViewComponent : NopViewComponent
    {
        #region Fields        
        private readonly IProductReviewImagesModelFactory _productReviewImagesModelFactory;
        #endregion

        #region Ctor
        public ProductReviewImagesViewComponent(IProductReviewImagesModelFactory productReviewImagesModelFactory)
        {
            _productReviewImagesModelFactory  = productReviewImagesModelFactory; 
        }
        #endregion

        #region Method
        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            if (widgetZone == null)
                return Content(string.Empty);

            if (!(additionalData is ProductReviewModel productReviewModel))
                return Content("");

            var model = await _productReviewImagesModelFactory.PrepareProductReviewImagesModelAsync(productReviewModel.Id);

            if (model == null)
                return Content(string.Empty);

            return View(model);
        }
        #endregion
    }
}
