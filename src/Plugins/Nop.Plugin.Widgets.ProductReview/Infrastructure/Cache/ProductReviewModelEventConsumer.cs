using System.Threading.Tasks;
using Nop.Core.Caching;
using Nop.Core.Events;
using Nop.Services.Events;
using SS.Plugin.Widgets.ProductReview.Domains;


namespace SS.Plugin.Widgets.ProductReview.Infrastructure.Cache
{
    public partial class ProductReviewModelEventConsumer:
        IConsumer<EntityInsertedEvent<Nop.Core.Domain.Catalog.ProductReview>>,
        IConsumer<EntityUpdatedEvent<Nop.Core.Domain.Catalog.ProductReview>>,
        IConsumer<EntityDeletedEvent<Nop.Core.Domain.Catalog.ProductReview>>,
        IConsumer<EntityInsertedEvent<ProductReviewImages>>,
        IConsumer<EntityUpdatedEvent<ProductReviewImages>>
    {
        /// <summary>
        /// Key for caching of quickinfo displayed on home page
        /// </summary>
        /// <remarks>
        /// {1} : language ID
        /// {2} : roles of the current user
        /// {3} : current store ID
        /// </remarks>
        #region product review cache key
        public static CacheKey ProductReviewpageKey => new("SS.Plugin.Widgets.ProductReview.productReviewpage-{0}-{1}-{2}-{3}", ProductReviewPagePrefixCacheKey);
        public static string ProductReviewPagePrefixCacheKey => "SS.Plugin.Widgets.ProductReview.productReviewpage";
        #endregion

        #region product review  rating cache key
        public static CacheKey ProductReviewRatingpageKey => new("SS.Plugin.Widgets.ProductReview.productReviewpage-{0}-{1}-{2}-{3}", ProductReviewRatingPagePrefixCacheKey);
        public static string ProductReviewRatingPagePrefixCacheKey => "SS.Plugin.Widgets.ProductReview.productReviewpage";
        #endregion

        #region Fields
        private readonly IStaticCacheManager _staticCacheManager;
        #endregion

        #region Ctor
        public ProductReviewModelEventConsumer(IStaticCacheManager staticCacheManager)
        {
            _staticCacheManager = staticCacheManager;
        }
        #endregion

        #region Method
        public virtual async Task HandleEventAsync(EntityInsertedEvent<ProductReviewImages> eventMessage)
        {
            await _staticCacheManager.RemoveByPrefixAsync(ProductReviewPagePrefixCacheKey);
            await _staticCacheManager.RemoveByPrefixAsync(ProductReviewRatingPagePrefixCacheKey);
        }
        public virtual async Task HandleEventAsync(EntityUpdatedEvent<ProductReviewImages> eventMessage)
        {
            await _staticCacheManager.RemoveByPrefixAsync(ProductReviewPagePrefixCacheKey);
            await _staticCacheManager.RemoveByPrefixAsync(ProductReviewRatingPagePrefixCacheKey);
        }
        public virtual async Task HandleEventAsync(EntityInsertedEvent<Nop.Core.Domain.Catalog.ProductReview> eventMessage)
        {
            await _staticCacheManager.RemoveByPrefixAsync(ProductReviewPagePrefixCacheKey);
            await _staticCacheManager.RemoveByPrefixAsync(ProductReviewRatingPagePrefixCacheKey);
        }
        public virtual async Task HandleEventAsync(EntityUpdatedEvent<Nop.Core.Domain.Catalog.ProductReview> eventMessage)
        {
            await _staticCacheManager.RemoveByPrefixAsync(ProductReviewPagePrefixCacheKey);
            await _staticCacheManager.RemoveByPrefixAsync(ProductReviewRatingPagePrefixCacheKey);
        }
        public virtual async Task HandleEventAsync(EntityDeletedEvent<Nop.Core.Domain.Catalog.ProductReview> eventMessage)
        {
            await _staticCacheManager.RemoveByPrefixAsync(ProductReviewPagePrefixCacheKey);
            await _staticCacheManager.RemoveByPrefixAsync(ProductReviewRatingPagePrefixCacheKey);
        }
        #endregion
    }
}
