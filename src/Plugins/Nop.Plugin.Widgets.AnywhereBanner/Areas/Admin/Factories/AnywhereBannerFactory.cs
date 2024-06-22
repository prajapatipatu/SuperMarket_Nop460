using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Services.Helpers;
using Nop.Services.Media;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Models.Extensions;
using Nop.Plugin.Widgets.AnywhereBanner.Areas.Admin.Models.AnywhereBanner;
using Nop.Plugin.Widgets.AnywhereBanner.Areas.Admin.Models.GridBannerSetting;
using Nop.Plugin.Widgets.AnywhereBanner.Data;
using Nop.Plugin.Widgets.AnywhereBanner.Domains.AnywhereBanner;
using Nop.Plugin.Widgets.AnywhereBanner.Services.AnywhereBanner;

namespace Nop.Plugin.Widgets.AnywhereBanner.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the Anywhere Banner model factory implementation
    /// </summary>
    public partial class AnywhereBannerFactory : IAnywhereBannerFactory
    {
        #region Fields
        private readonly IAnywhereBannerService _anywhereBannerService;
        private readonly IPictureService _pictureService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IAclSupportedModelFactory _aclSupportedModelFactory;
        private readonly IStoreMappingSupportedModelFactory _storeMappingSupportedModelFactory;
        #endregion

        #region Ctor
        public AnywhereBannerFactory(IAnywhereBannerService anywhereBannerService,
            IPictureService pictureService,
            IDateTimeHelper dateTimeHelper,
            IAclSupportedModelFactory aclSupportedModelFactory,
            IStoreMappingSupportedModelFactory storeMappingSupportedModelFactory)
        {
            _anywhereBannerService = anywhereBannerService;
            _pictureService = pictureService;
            _dateTimeHelper = dateTimeHelper;
            _aclSupportedModelFactory = aclSupportedModelFactory;
            _storeMappingSupportedModelFactory = storeMappingSupportedModelFactory;
        }
        #endregion

        #region Utilities
        private IList<SelectListItem> PrepareWidgetsList(string widgetsPlace = "")
        {
            return Enum.GetValues(typeof(EnumWidgetZone))
               .Cast<EnumWidgetZone>()
               .Select(x => new SelectListItem
               {
                   Text = x.ToString(),
                   Value = x.ToString(),
                   Selected = x.ToString() == widgetsPlace
               }).ToList();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Prepare AnywhereBanner search model
        /// </summary>
        /// <param name="searchModel">Product search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the search model
        /// </returns>
        public virtual async Task<AnywhereBannerSearchModel> PrepareAnywhereBannerSearchModelAsync(AnywhereBannerSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var gridBanners = _anywhereBannerService.GetAllGridBanner();

            searchModel.WidgetNames = _anywhereBannerService.GetAnywhereBannerDetailsByWidgetNameListAsync();

            var deleteGridBanners = gridBanners.Where(g => searchModel.WidgetNames.All(sw => sw != g.WidgetName)).ToList();
            if (deleteGridBanners.Count > 0)
            {
                await _anywhereBannerService.DeleteGridBannersAsync(deleteGridBanners);
            }

            if (searchModel.WidgetNames.Count > 0)
            {
                foreach (var widget in searchModel.WidgetNames)
                {
                    var gridBanner = gridBanners.FirstOrDefault(p => p.WidgetName == widget);
                    searchModel.GridBannerListItems.Add(new GridBannerSettingModel
                    {
                        WidgetName = widget,
                        GridType = gridBanner != null ? gridBanner.GridType : 10,
                        Column = gridBanner != null ? gridBanner.Column : 1,
                        IsSlider = gridBanner != null ? gridBanner.IsSlider : false,
                        LeftColumn = gridBanner != null ? gridBanner.LeftColumn : 0,
                        RightColumn = gridBanner != null ? gridBanner.RightColumn : 0,
                    });
                }
            }
            //prepare grid
            searchModel.SetGridPageSize();

            return searchModel;
        }

        /// <summary>
        /// Prepare paged AnywhereBanner list model
        /// </summary>
        /// <param name="searchModel">AnywhereBanner search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the AnywhereBanner list model
        /// </returns>
        public virtual async Task<AnywhereBannerListModel> PrepareAnywhereBannerListModelAsync(AnywhereBannerSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get parameters to filter comments
            var startDateValue = !searchModel.StartDate.HasValue ? null
              : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);

            //get offerBanners
            var anywhereBanner = await _anywhereBannerService.SearchAnywhereBannerAsync(startDateUtc: startDateValue,
                endDateUtc: endDateValue,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare list model
            var model = await new AnywhereBannerListModel().PrepareToGridAsync(searchModel, anywhereBanner, () =>
            {
                return anywhereBanner.SelectAwait(async anywhereBanner =>
                {
                    return new AnywhereBannerModel()
                    {
                        Id = anywhereBanner.Id,
                        StartDate = anywhereBanner.StartDate,
                        EndDate = anywhereBanner.EndDate,
                        PictureThumbnailUrl = await _pictureService.GetPictureUrlAsync(anywhereBanner.PictureId, 75),
                        MobilePictureThumbnailUrl = await _pictureService.GetPictureUrlAsync(anywhereBanner.MobilePictureId, 75),
                        Widget = anywhereBanner.Widget,
                        Published = anywhereBanner.Published,
                        DisplayOrder = anywhereBanner.DisplayOrder
                    };
                });
            });
            return model;
        }

        /// <summary>
        /// Prepare AnywhereBanner model
        /// </summary>
        /// <param name="model">model</param>
        /// <param name="anywhereBannerDetails">anywhereBannerDetails</param>
        /// <param name="excludeProperties">excludeProperties</param>
        /// <returns>AnywhereBanner model </returns>
        public virtual async Task<AnywhereBannerModel> PrepareAnywhereBannerModelAsync(AnywhereBannerModel model, AnywhereBannerDetails anywhereBannerDetails, bool excludeProperties = false)
        {  
            if (anywhereBannerDetails == null)
            {
                model.WidgetList = PrepareWidgetsList();
            }
            if (anywhereBannerDetails != null)
            {
                List<string> widgets = new List<string>();

                if (!string.IsNullOrEmpty(anywhereBannerDetails.Widget))
                    widgets = anywhereBannerDetails.Widget.Split(',').ToList();

                model ??= new AnywhereBannerModel();
                model.WidgetList = PrepareWidgetsList(anywhereBannerDetails.Widget);
                model.Id = anywhereBannerDetails.Id;
                model.StartDate = anywhereBannerDetails.StartDate;
                model.EndDate = anywhereBannerDetails.EndDate;
                model.PictureId = anywhereBannerDetails.PictureId;
                model.MobilePictureId = anywhereBannerDetails.MobilePictureId;
                model.SelectedWidgets = widgets;
                model.CTA = anywhereBannerDetails.CTA;
                model.Published = anywhereBannerDetails.Published;
                model.DisplayOrder = anywhereBannerDetails.DisplayOrder;

            }           
            //prepare model customer roles
            await _aclSupportedModelFactory.PrepareModelCustomerRolesAsync(model, anywhereBannerDetails, false);

            //prepare model stores
            await _storeMappingSupportedModelFactory.PrepareModelStoresAsync(model, anywhereBannerDetails, false);

            return model;
        }
        #endregion
    }
}