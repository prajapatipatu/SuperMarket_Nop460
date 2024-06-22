using System.Collections.Generic;
using System.Threading.Tasks;
using SS.Plugin.Widgets.ProductReview.Domains;
using SS.Plugin.Widgets.ProductReview.Models;

namespace SS.Plugin.Widgets.ProductReview.Services
{
    public partial interface IWidgetProductReviewService
    {
        /// <summary>
        /// Insert pictureId, VideoUrl and PictureId
        /// </summary>
        /// <param name="productReviewImages"></param>
        /// <returns></returns>
        Task InsertProductReviewImages(ProductReviewImages productReviewImages);

        /// <summary>
        /// Gets a product review images
        /// </summary>
        /// <param name="productReviewId">product reviewId identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product review images
        /// </returns>
        Task<ProductReviewImages> GetProductReviewImagesByProductReviewIdAsync(int productReviewId);

        /// <summary>
        /// Gets all rating
        /// </summary>
        /// <param name="productId">The product identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the reviews rating
        /// </returns>
        Task<IList<RatingModel>> GetAllRatingAsync(int productId);

        /// <summary>
        /// Gets product review images
        /// </summary>
        /// <param name="id">product review images identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product review images
        /// </returns>
        Task<ProductReviewImages> GetProductReviewImagesByIdAsync(int id);

        /// <summary>
        /// Updates the product review images
        /// </summary>
        /// <param name="productreviewimages">ProductReviewImages</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task UpdateProductReviewImagesAsync(ProductReviewImages productReviewImages);
    }
}
