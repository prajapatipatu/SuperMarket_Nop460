using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Events;
using Nop.Data;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Plugin.Widgets.AnywhereBanner.Domains.AnywhereBanner;
using Nop.Plugin.Widgets.AnywhereBanner.Domains.GridBannerSetting;

namespace Nop.Plugin.Widgets.AnywhereBanner.Services.AnywhereBanner
{
    /// <summary>
    /// AnywhereBanner service
    /// </summary>
    public partial class AnywhereBannerService : IAnywhereBannerService
    {
        #region Fields

        private readonly IEventPublisher _eventPublisher;
        private readonly IRepository<AnywhereBannerDetails> _anywhereBannerDetailsRepository;
        private readonly IAclService _aclService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IRepository<GridBannerSetting> _gridBannerSettingRepository;

        #endregion

        #region Ctor

        public AnywhereBannerService(IEventPublisher eventPublisher,
            IRepository<AnywhereBannerDetails> anywhereBannerDetailsRepository,
            IAclService aclService,
            IStoreMappingService storeMappingService,
            IRepository<GridBannerSetting> gridBannerSettingRepository)
        {
            _anywhereBannerDetailsRepository = anywhereBannerDetailsRepository;
            _eventPublisher = eventPublisher;
            _aclService = aclService;
            _storeMappingService = storeMappingService;
            _gridBannerSettingRepository = gridBannerSettingRepository;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Inserts a anywhere banner
        /// </summary>
        /// <param name="anywhereBanner">anywhereBanner</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task InsertAnywhereBannerAsync(AnywhereBannerDetails anywhereBannerDetails)
        {
            if (anywhereBannerDetails == null)
                throw new NotImplementedException();

            if (anywhereBannerDetails.Id == 0)
                await _anywhereBannerDetailsRepository.InsertAsync(anywhereBannerDetails);

            if (!string.IsNullOrEmpty(anywhereBannerDetails.Widget))
                await AddDefaultSetting(anywhereBannerDetails.Widget);

            await _eventPublisher.EntityInsertedAsync(anywhereBannerDetails);
        }

        /// <summary>
        /// Updates the anywhere banner
        /// </summary>
        /// <param name="anywhereBanner">anywhereBanner</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task UpdateAnywhereBannerAsync(AnywhereBannerDetails anywhereBannerDetails)
        {
            if (anywhereBannerDetails == null)
                throw new NotImplementedException();

            if (anywhereBannerDetails.Id > 0)
                await _anywhereBannerDetailsRepository.UpdateAsync(anywhereBannerDetails);

            if (!string.IsNullOrEmpty(anywhereBannerDetails.Widget))
                await AddDefaultSetting(anywhereBannerDetails.Widget);

            await _eventPublisher.EntityUpdatedAsync(anywhereBannerDetails);
        }

        /// <summary>
        /// Delete the anywhere banner
        /// </summary>
        /// <param name="anywhereBanner">anywhereBanner</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task DeleteAnywhereBannerAsync(AnywhereBannerDetails anywhereBannerDetails)
        {
            if (anywhereBannerDetails == null)
                throw new NotImplementedException();

            await _anywhereBannerDetailsRepository.DeleteAsync(anywhereBannerDetails);

            await _eventPublisher.EntityDeletedAsync(anywhereBannerDetails);
        }

        /// <summary>
        /// Delete anywhere banner
        /// </summary>
        /// <param name="AnywhereBannerDetails">AnywhereBannerDetails</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task DeleteSelectAnywhereBannerAsync(IList<AnywhereBannerDetails> anywhereBannerDetails)
        {
            if (anywhereBannerDetails == null)
                throw new NotImplementedException();

            await _anywhereBannerDetailsRepository.DeleteAsync(anywhereBannerDetails);

            foreach (var item in anywhereBannerDetails)
            {
                await _eventPublisher.EntityDeletedAsync(item);
            }   
        }

        /// <summary>
        /// Get anywhere banner by Id
        /// </summary>
        /// <param name="anywhereBanner">anywhereBanner</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<AnywhereBannerDetails> GetAnywhereBannerByIdAsync(int id)
        {
            if (id == 0)
                throw new NotImplementedException();

            return await _anywhereBannerDetailsRepository.GetByIdAsync(id);
        }

        /// <summary>
        /// Get anywhere banner by identifiers
        /// </summary>
        /// <param name="anywhereBannerIds">Anywhere banner identifiers</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the anywhere banner
        /// </returns>
        public virtual async Task<IList<AnywhereBannerDetails>> GetAnywhereBannerByIdsAsync(int[] anywhereBannerIds)
        {
            return await _anywhereBannerDetailsRepository.GetByIdsAsync(anywhereBannerIds, cache => default, false);
        }

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
        public virtual async Task<IPagedList<AnywhereBannerDetails>> SearchAnywhereBannerAsync(
            DateTime? startDateUtc = null, DateTime? endDateUtc = null,int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false)
        {
            var query = _anywhereBannerDetailsRepository.Table;

            if (startDateUtc.HasValue)
                query = query.Where(o => startDateUtc.Value <= o.StartDate);

            if (endDateUtc.HasValue)
                query = query.Where(o => endDateUtc.Value >= o.EndDate);

            query = query.OrderByDescending(o => o.StartDate);

            //database layer paging
            return await query.ToPagedListAsync(pageIndex, pageSize, getOnlyTotalCount);
        }

        /// <summary>
        /// Gets all AnywhereBannerDetails widget
        /// </summary>
        /// <param name="widgetZone">widgetZone</param>
        public virtual async Task<IList<AnywhereBannerDetails>> GetAnywhereBannerDetailsByWidgetAsync(string widgetZone, bool showHidden = false)
        {
            var query = _anywhereBannerDetailsRepository.Table.Where(s => s.Widget.Contains(widgetZone));

            if (!showHidden)
                query = query.Where(a => a.Published && (!a.StartDate.HasValue || a.StartDate <= DateTime.UtcNow) && (!a.EndDate.HasValue || a.EndDate >= DateTime.UtcNow));  

            query = query.OrderBy(a => a.DisplayOrder);

            return await query
                .WhereAwait(async c => await _aclService.AuthorizeAsync(c) && await _storeMappingService.AuthorizeAsync(c))
                .ToListAsync();
        }
        #endregion

        #region Grid Banner

        public virtual async Task AddDefaultSetting(string widgetName)
        {
            var widgets = widgetName.Split(",");
            foreach (var widget in widgets)
            {
                if (!await IsWidgetNameExits(widget))
                {
                    var gridBannerSetting = new GridBannerSetting()
                    {
                        WidgetName = widget,
                        GridType = 10,
                        Column = 1,
                        IsSlider = false,
                        LeftColumn = 0,
                        RightColumn = 0,
                    };
                    await InsertGridBannerAsync(gridBannerSetting);
                }
            }
        }

        public virtual IList<string> GetAnywhereBannerDetailsByWidgetNameListAsync()
        {
            var widgetLists = _anywhereBannerDetailsRepository.Table.Select(x => x.Widget).ToList();
            var widgetResult = new List<string>();
            foreach (var item in widgetLists)
            {
                if (!string.IsNullOrEmpty(item))
                {
                    var widgets = item.Split(",").ToList();
                    foreach (var widget in widgets)
                    {
                        widgetResult.Add(widget);
                    }
                }
            }
            return widgetResult.Distinct().ToList();
        }
        public virtual async Task<bool> IsWidgetNameExits(string widgetName)
        {
            var widget = await GetByWidgetName(widgetName);

            return widget != null;
        }
        public virtual async Task InsertGridBannerAsync(GridBannerSetting gridBanner)
        {
            if (gridBanner == null)
                throw new NotImplementedException();

            await _gridBannerSettingRepository.InsertAsync(gridBanner);
        }
        public virtual async Task UpdateGridBannerAsync(GridBannerSetting gridBanner)
        {
            if (gridBanner == null)
                throw new NotImplementedException();

            await _gridBannerSettingRepository.UpdateAsync(gridBanner);

            await _eventPublisher.EntityUpdatedAsync(gridBanner);
        }
        public virtual async Task<GridBannerSetting> GetByWidgetName(string widgetName)
        {
            var query = _gridBannerSettingRepository.Table.Where(x => x.WidgetName == widgetName);
            return await query.FirstOrDefaultAsync();
        }
        public virtual IList<GridBannerSetting> GetAllGridBanner()
        {
            var query = _gridBannerSettingRepository.Table;

            return query.ToList();
        }
        public virtual async Task DeleteGridBannersAsync(IList<GridBannerSetting> gridBanner)
        {
            await _gridBannerSettingRepository.DeleteAsync(gridBanner);
        }
        #endregion
    }
}