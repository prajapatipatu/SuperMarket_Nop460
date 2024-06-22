using System.Threading.Tasks;
using Nop.Core.Caching;
using Nop.Core.Events;
using Nop.Services.Events;
using Nop.Plugin.Widgets.AnywhereBanner.Domains.AnywhereBanner;
using Nop.Plugin.Widgets.AnywhereBanner.Domains.GridBannerSetting;

namespace Nop.Plugin.Widgets.AnywhereBanner.Infrastructure.Cache
{
    /// <summary>
    /// Model cache event consumer (used for caching of presentation layer models)
    /// </summary>
    public partial class AnywhereBannerModelEventConsumer :
        IConsumer<EntityInsertedEvent<AnywhereBannerDetails>>,
        IConsumer<EntityUpdatedEvent<AnywhereBannerDetails>>,
        IConsumer<EntityDeletedEvent<AnywhereBannerDetails>>,
        IConsumer<EntityUpdatedEvent<GridBannerSetting>>
    {
        /// <summary>
        /// Key for caching of quickinfo displayed on home page
        /// </summary>
        /// <remarks>
        /// {0} : widgetZone
        /// {1} : language ID
        /// {2} : roles of the current user
        /// {3} : current store ID
        /// </remarks>
        public static CacheKey AnywhereBannerpageKey => new("Nop.Plugin.Widgets.AnywhereBanner.anywherebannerpage-{0}-{1}-{2}-{3}", AnywhereBannerPagePrefixCacheKey);
        public static string AnywhereBannerPagePrefixCacheKey => "Nop.Plugin.Widgets.AnywhereBanner.anywherebannerpage";

        #region Fields
        private readonly IStaticCacheManager _staticCacheManager;
        #endregion

        #region Ctor
        public AnywhereBannerModelEventConsumer(IStaticCacheManager staticCacheManager)
        {
            _staticCacheManager = staticCacheManager;
        }
        #endregion

        public virtual async Task HandleEventAsync(EntityInsertedEvent<AnywhereBannerDetails> eventMessage)
        {
            await _staticCacheManager.RemoveByPrefixAsync(AnywhereBannerPagePrefixCacheKey);
        }

        public virtual async Task HandleEventAsync(EntityUpdatedEvent<AnywhereBannerDetails> eventMessage)
        {
            await _staticCacheManager.RemoveByPrefixAsync(AnywhereBannerPagePrefixCacheKey);
        }

        public virtual async Task HandleEventAsync(EntityDeletedEvent<AnywhereBannerDetails> eventMessage)
        {
            await _staticCacheManager.RemoveByPrefixAsync(AnywhereBannerPagePrefixCacheKey);
        }

        public virtual async Task HandleEventAsync(EntityUpdatedEvent<GridBannerSetting> eventMessage)
        {
            await _staticCacheManager.RemoveByPrefixAsync(AnywhereBannerPagePrefixCacheKey);
        }
    }
}
