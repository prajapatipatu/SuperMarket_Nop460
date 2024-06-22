using System.Threading.Tasks;
using SS.Plugin.Widgets.ProductReview.Areas.Admin.Models;

namespace SS.Plugin.Widgets.ProductReview.Areas.Admin.Factories
{
    public partial interface IProductReviewImagesModelFactory
    {
        /// <summary>
        /// Prepare product review images model
        /// </summary>
        /// <param name="reviewId">ReviewId</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product review images model
        /// </returns>
        Task<ProductReviewImagesModel> PrepareProductReviewImagesModelAsync(int reviewId);
    }
}
