using System.Threading.Tasks;
using SS.Plugin.Widgets.ProductReview.Areas.Admin.Models;
using SS.Plugin.Widgets.ProductReview.Services;

namespace SS.Plugin.Widgets.ProductReview.Areas.Admin.Factories
{
    public partial class ProductReviewImagesModelFactory : IProductReviewImagesModelFactory
    {
        #region fields
        private readonly IWidgetProductReviewService _widgetProductReviewService;
        #endregion

        #region Ctor
        public ProductReviewImagesModelFactory(IWidgetProductReviewService widgetProductReviewService)
        {
            _widgetProductReviewService = widgetProductReviewService;            
        }
        #endregion

        #region Methods

        /// <summary>
        /// Prepare product review images model
        /// </summary>
        /// <param name="reviewId">ReviewId</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product review images model
        /// </returns>
        public virtual async Task<ProductReviewImagesModel> PrepareProductReviewImagesModelAsync(int reviewId)
        {
            var model = new ProductReviewImagesModel();
            var productReviewImages = await _widgetProductReviewService.GetProductReviewImagesByProductReviewIdAsync(reviewId);
            if(productReviewImages != null)
            {
                model.Id = productReviewImages.Id;
                model.PictureIds = productReviewImages.PictureId;
                model.VideoUrl = productReviewImages.VideoUrl;
            }
            return model;
        }
        #endregion
    }
}
