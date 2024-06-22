using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Services.Customers;
using Nop.Plugin.Widgets.AnywhereBanner.Infrastructure.Cache;
using Nop.Plugin.Widgets.AnywhereBanner.Models;
using Nop.Services.Media;
using Nop.Plugin.Widgets.AnywhereBanner.Services.AnywhereBanner;
using Nop.Plugin.Widgets.AnywhereBanner.Data;

namespace Nop.Plugin.Widgets.AnywhereBanner.Factories
{
    public partial class AnywhereBannerFactory : IAnywhereBannerFactory
    {
        #region Fields
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;
        private readonly IStoreContext _storeContext;
        private readonly IAnywhereBannerService _anywhereBannerService;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IPictureService _pictureService;
        #endregion

        #region Ctor
        public AnywhereBannerFactory(IWorkContext workContext,
            ICustomerService customerService,
            IStoreContext storeContext,
            IAnywhereBannerService anywhereBannerService,
            IStaticCacheManager staticCacheManager,
            IPictureService pictureService)
        {
            _workContext = workContext;
            _customerService = customerService;
            _storeContext = storeContext;
            _anywhereBannerService = anywhereBannerService;
            _staticCacheManager = staticCacheManager;
            _pictureService = pictureService;
        }
        #endregion

        #region AnyWhere Banner

        /// <summary>
        /// Prepare AnywhereBannerDetailsModel models
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the list of homepage Anywhere Banner models
        /// </returns>
        public virtual async Task<AnywhereBannerDetailsModel> PrepareAnywhereBannerDetailsModelsAsync(string widgetZone)
        {
            var language = await _workContext.GetWorkingLanguageAsync();
            var customer = await _workContext.GetCurrentCustomerAsync();
            var customerRoleIds = await _customerService.GetCustomerRoleIdsAsync(customer);
            var store = await _storeContext.GetCurrentStoreAsync();

            var anywhereBannerCacheKey = _staticCacheManager.PrepareKeyForDefaultCache(AnywhereBannerModelEventConsumer.AnywhereBannerpageKey,
               widgetZone, language, customerRoleIds, store);

            var model = await _staticCacheManager.GetAsync(anywhereBannerCacheKey, async () =>
            {
                var anywhereBannerDetails = await _anywhereBannerService.GetAnywhereBannerDetailsByWidgetAsync(widgetZone);
                
                var anywhereBannerModels = await anywhereBannerDetails.SelectAwait(async anywhereBanner =>
                {
                    var url = string.Empty;
                    var moburl = string.Empty;

                    var picture = await _pictureService.GetPictureByIdAsync(anywhereBanner.PictureId);
                    if (picture != null)
                    {
                        var pictureurl = await _pictureService.GetPictureUrlAsync(picture);
                        url = pictureurl.Url == null ? string.Empty : pictureurl.Url;
                    }

                    var mobpicture = await _pictureService.GetPictureByIdAsync(anywhereBanner.MobilePictureId);
                    if (mobpicture != null)
                    {
                        var mobpictureurl = await _pictureService.GetPictureUrlAsync(mobpicture);
                        moburl = mobpictureurl.Url == null ? string.Empty : mobpictureurl.Url;
                    }

                    var anywhereBannerDetailsModel = new BannerModel()
                    {
                        Id = anywhereBanner.Id,
                        PictureId = anywhereBanner.PictureId,
                        MobilePictureId = anywhereBanner.MobilePictureId,
                        PictureThumbnailUrl = url,
                        MobilePictureThumbnailUrl = moburl,
                        StartDate = anywhereBanner.StartDate,
                        EndDate = anywhereBanner.EndDate,
                        CTA = anywhereBanner.CTA,
                        Published = anywhereBanner.Published
                    };

                    return anywhereBannerDetailsModel;
                }).ToListAsync();

                var gridBanner = await _anywhereBannerService.GetByWidgetName(widgetZone);
                if (gridBanner == null)
                    return new AnywhereBannerDetailsModel();

                if (!gridBanner.IsSlider)
                {
                    switch (gridBanner.Column)
                    {
                        case (int)GridBannerColumn.One:
                            gridBanner.Column = 12;
                            break;
                        case (int)GridBannerColumn.Two:
                            gridBanner.Column = 6;
                            break;
                        case (int)GridBannerColumn.Three:
                            gridBanner.Column = 4;
                            break;
                        case (int)GridBannerColumn.Four:
                            gridBanner.Column = 3;
                            break;
                    }
                }

                var anywhereBannerDetailsModel = new AnywhereBannerDetailsModel()
                {
                    Id= gridBanner.Id,
                    GridType = gridBanner.GridType,
                    Column = gridBanner.Column,
                    IsSlider = gridBanner.IsSlider,
                    LeftColumn = gridBanner.LeftColumn,
                    RightColumn = gridBanner.RightColumn,
                    BannerModels = anywhereBannerModels
                };
                return anywhereBannerDetailsModel;
                                
            });

            return model;
        }
        #endregion
    }
}
