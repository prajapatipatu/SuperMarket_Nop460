using Nop.Plugin.Widgets.AnywhereBanner.Areas.Admin.Models.AnywhereBanner;
using Nop.Plugin.Widgets.AnywhereBanner.Domains.AnywhereBanner;
using System.Threading.Tasks;

namespace Nop.Plugin.Widgets.AnywhereBanner.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the category model factory
    /// </summary>
    public partial interface IAnywhereBannerFactory
    {
        /// <summary>
        /// Prepare paged AnywhereBanner list model
        /// </summary>
        /// <param name="searchModel">AnywhereBanner search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the AnywhereBanner list model
        /// </returns>
        Task<AnywhereBannerListModel> PrepareAnywhereBannerListModelAsync(AnywhereBannerSearchModel searchModel);

        /// <summary>
        /// Prepare AnywhereBanner search model
        /// </summary>
        /// <param name="searchModel"> searchModel</param>
        /// <returns> AnywhereBanner search model </returns>
        Task<AnywhereBannerSearchModel> PrepareAnywhereBannerSearchModelAsync(AnywhereBannerSearchModel searchModel);

        /// <summary>
        /// Prepare AnywhereBanner model
        /// </summary>
        /// <param name="model">model</param>
        /// <param name="anywhereBannerDetails">anywhereBannerDetails</param>
        /// <param name="excludeProperties">excludeProperties</param>
        /// <returns>AnywhereBanner model </returns>
        Task<AnywhereBannerModel> PrepareAnywhereBannerModelAsync(AnywhereBannerModel model, AnywhereBannerDetails anywhereBannerDetails, bool excludeProperties = false);
    }
}