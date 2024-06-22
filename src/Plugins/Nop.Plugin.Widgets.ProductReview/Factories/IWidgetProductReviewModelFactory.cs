using Nop.Core.Domain.Catalog;
using SS.Plugin.Widgets.ProductReview.Models;
using System.Threading.Tasks;

namespace SS.Plugin.Widgets.ProductReview.Factories
{
    public partial interface IWidgetProductReviewModelFactory
    {
        /// <summary>
        /// Prepare the product reviews model
        /// </summary>
        /// <param name="model">Product reviews model</param>
        /// <param name="product">Product</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product reviews model
        /// </returns>
        Task<ProductReviewsModel> PrepareWidgetProductReviewsModelAsync(ProductReviewsModel model, Product product);

        /// <summary>
        /// Prepare the product reviews model
        /// </summary>
        /// <param name="productId">Product</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product reviews model
        /// </returns>
        Task<ProductReviewsModel> PrepareWidgetProductReviewsDetailsModelAsync(int productId);

        /// <summary>
        /// Prepare the product reviews model
        /// </summary>
        /// <param name="productId">Product</param>
        /// <param name="pageIndex">pageIndex</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product reviews model
        /// </returns>
        Task<ProductReviewsModel> PrepareProductReviewsModelAjaxAsync(int productId, int pageIndex);
    }
}
