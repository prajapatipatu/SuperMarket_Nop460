using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Plugin.Widgets.AnywhereBanner.Domains.AnywhereBanner;
using Nop.Plugin.Widgets.AnywhereBanner.Domains.GridBannerSetting;

namespace Nop.Plugin.Widgets.AnywhereBanner.Services.AnywhereBanner
{
    /// <summary>
    /// AnywhereBanner service interface
    /// </summary>
    public partial interface IAnywhereBannerService
    {
        /// <summary>
        /// Inserts a anywhere banner
        /// </summary>
        /// <param name="anywhereBanner">anywhereBanner</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task InsertAnywhereBannerAsync(AnywhereBannerDetails anywhereBannerDetails);

        /// <summary>
        /// Updates the anywhere banner
        /// </summary>
        /// <param name="anywhereBanner">anywhereBanner</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task UpdateAnywhereBannerAsync(AnywhereBannerDetails anywhereBannerDetails);
        /// <summary>
        /// Delete the anywhere banner
        /// </summary>
        /// <param name="anywhereBanner">anywhereBanner</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task DeleteAnywhereBannerAsync(AnywhereBannerDetails anywhereBannerDetails);
        /// <summary>
        /// Delete anywhere banner
        /// </summary>
        /// <param name="AnywhereBannerDetails">AnywhereBannerDetails</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task DeleteSelectAnywhereBannerAsync(IList<AnywhereBannerDetails> anywhereBannerDetails);
        /// <summary>
        /// Get anywhere banner by Id
        /// </summary>
        /// <param name="anywhereBanner">anywhereBanner</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task<AnywhereBannerDetails> GetAnywhereBannerByIdAsync(int id);
        /// <summary>
        /// Get anywhere banner by identifiers
        /// </summary>
        /// <param name="anywhereBannerIds">Anywhere banner identifiers</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the anywhere banner
        /// </returns>
        Task<IList<AnywhereBannerDetails>> GetAnywhereBannerByIdsAsync(int[] anywhereBannerIds);
    
        /// <summary>
        /// Search Anywhere Banner
        /// </summary>
        /// <param name="startDateUtc">Created date from (UTC); null to load all records</param>
        /// <param name="endDateUtc">Created date to (UTC); null to load all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="getOnlyTotalCount">A value in indicating whether you want to load only total number of records. Set to "true" if you don't want to load data from database</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the offer banner
        /// </returns>
        Task<IPagedList<AnywhereBannerDetails>> SearchAnywhereBannerAsync(
            DateTime? startDateUtc = null, DateTime? endDateUtc = null, int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false);

        /// <summary>
        /// Gets all AnywhereBannerDetails widget
        /// </summary>
        /// <param name="widgetZone">widgetZone</param>
        Task<IList<AnywhereBannerDetails>> GetAnywhereBannerDetailsByWidgetAsync(string widgetZone, bool showHidden = false);

        #region Grid Banner
        IList<string> GetAnywhereBannerDetailsByWidgetNameListAsync();
        Task<bool> IsWidgetNameExits(string widgetName);
        Task InsertGridBannerAsync(GridBannerSetting gridBanner);
        Task UpdateGridBannerAsync(GridBannerSetting gridBanner);
        Task<GridBannerSetting> GetByWidgetName(string widgetName);
        IList<GridBannerSetting> GetAllGridBanner();
        Task DeleteGridBannersAsync(IList<GridBannerSetting> gridBanner);
        #endregion
    }
}