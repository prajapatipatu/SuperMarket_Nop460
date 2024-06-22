using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Events;
using Nop.Data;
using SS.Plugin.Widgets.ProductReview.Domains;
using SS.Plugin.Widgets.ProductReview.Models;

namespace SS.Plugin.Widgets.ProductReview.Services
{
    public partial class WidgetProductReviewService : IWidgetProductReviewService
    {
        #region Fields
        private readonly IRepository<ProductReviewImages> _productReviewImagesRepository;
        protected readonly IRepository<Nop.Core.Domain.Catalog.ProductReview> _productReviewRepository;
        private readonly IEventPublisher _eventPublisher;
        #endregion

        #region Ctor
        public WidgetProductReviewService(IRepository<ProductReviewImages> productReviewImagesRepository,
            IRepository<Nop.Core.Domain.Catalog.ProductReview> productReviewRepository,
            IEventPublisher eventPublisher)
        {
            _productReviewImagesRepository = productReviewImagesRepository;
            _productReviewRepository = productReviewRepository;
            _eventPublisher = eventPublisher;
        }
        #endregion

        #region Method
        /// <summary>
        /// Insert pictureId, VideoUrl and PictureId
        /// </summary>
        /// <param name="productReviewImages"></param>
        /// <returns></returns>
        public virtual async Task InsertProductReviewImages(ProductReviewImages productReviewImages)
        {
            await _productReviewImagesRepository.InsertAsync(productReviewImages);

            await _eventPublisher.EntityInsertedAsync(productReviewImages);
        }

        /// <summary>
        /// Gets a product review images
        /// </summary>
        /// <param name="productReviewId">product reviewId identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product review images
        /// </returns>
        public async Task<ProductReviewImages> GetProductReviewImagesByProductReviewIdAsync(int productReviewId)
        {
            return await _productReviewImagesRepository.Table.Where(x => x.ProductReviewId == productReviewId).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Gets all rating
        /// </summary>
        /// <param name="productId">The product identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the reviews rating
        /// </returns>
        public virtual async Task<IList<RatingModel>> GetAllRatingAsync(int productId)
        {
            var productReview = _productReviewRepository.Table.Where(x=>x.ProductId == productId).GroupBy(x => x.Rating).Select(x=> new RatingModel { Rating = x.Key, Count =x.Count() });
            return await productReview.ToListAsync();            
        }

        /// <summary>
        /// Gets product review images
        /// </summary>
        /// <param name="id">product review images identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product review images
        /// </returns>
        public async Task<ProductReviewImages> GetProductReviewImagesByIdAsync(int id)
        {
            return await _productReviewImagesRepository.GetByIdAsync(id);
        }

        /// <summary>
        /// Updates the product review images
        /// </summary>
        /// <param name="productreviewimages">ProductReviewImages</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task UpdateProductReviewImagesAsync(ProductReviewImages productReviewImages)
        {
            await _productReviewImagesRepository.UpdateAsync(productReviewImages);

            await _eventPublisher.EntityUpdatedAsync(productReviewImages);
        }
        #endregion
    }
}
