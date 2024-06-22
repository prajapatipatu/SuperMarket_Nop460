using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Web.Areas.Admin.Controllers;
using SS.Plugin.Widgets.ProductReview.Areas.Admin.Models;
using SS.Plugin.Widgets.ProductReview.Services;

namespace SS.Plugin.Widgets.ProductReview.Areas.Admin.Controllers
{
    public class ProductReviewController : BaseAdminController
    {
        #region Fields
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly IWidgetProductReviewService _widgetProductReviewService;
        #endregion

        #region Ctor
        public ProductReviewController(IStoreContext storeContext,
            ISettingService settingService,
            INotificationService notificationService,
            ILocalizationService localizationService,
            IWidgetProductReviewService widgetProductReviewService)
        {
            _storeContext = storeContext;
            _settingService = settingService;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _widgetProductReviewService= widgetProductReviewService;
        }
        #endregion

        #region Method
        public async Task<IActionResult> Configure()
        {
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var productReviewSettings = await _settingService.LoadSettingAsync<ProductReviewSettings>(storeScope);
            var model = new ConfigurationModel
            {
                PictureSize = productReviewSettings.PictureSize,
                MaxPicture = productReviewSettings.MaxPicture,
                ActiveStoreScopeConfiguration = storeScope
            };

            if (storeScope > 0)
            {
                model.PictureSize_OverrideForStore = await _settingService.SettingExistsAsync(productReviewSettings, x => x.PictureSize, storeScope);
                model.Active_OverrideForStore = await _settingService.SettingExistsAsync(productReviewSettings, x => x.MaxPicture, storeScope);
            }
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (ModelState.IsValid)
            {
                var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
                var productReviewSettings = await _settingService.LoadSettingAsync<ProductReviewSettings>(storeScope);

                productReviewSettings.PictureSize = model.PictureSize;
                productReviewSettings.MaxPicture = model.MaxPicture;

                await _settingService.SaveSettingOverridablePerStoreAsync(productReviewSettings, x => x.PictureSize, model.PictureSize_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(productReviewSettings, x => x.MaxPicture, model.Active_OverrideForStore, storeScope, false);
                //now clear settings cache
                await _settingService.ClearCacheAsync();

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

            }
            return await Configure();
        }

        [HttpPost]
        public virtual async Task<IActionResult> DeleteProductReviewImage(int id, int picId)
        {
            var productReviewImages = await _widgetProductReviewService.GetProductReviewImagesByIdAsync(id);
            if(productReviewImages != null)
            {
                var picIds = new List<string>();
                if (!string.IsNullOrEmpty(productReviewImages.PictureId))
                {
                    var picturesIds = productReviewImages.PictureId.Split(",");
                    foreach (var pictureId in picturesIds)
                    {
                        if (!string.IsNullOrEmpty(pictureId))
                        {
                            if (Convert.ToInt32(pictureId) != picId)
                            {
                                picIds.Add(pictureId);
                            }
                        }
                    }
                }
                productReviewImages.PictureId = string.Join(",", picIds);

                await _widgetProductReviewService.UpdateProductReviewImagesAsync(productReviewImages);

                return Json(new
                {
                    success = true
                });
            }
            return Json(new
            {
                success = false
            });
        }

        [HttpPost]
        public virtual async Task<IActionResult> UpdateVideoUrl(int id, string videoUrl)
        {
            var productReviewImages = await _widgetProductReviewService.GetProductReviewImagesByIdAsync(id);
            if (productReviewImages != null)
            {
                productReviewImages.VideoUrl = videoUrl;

                await _widgetProductReviewService.UpdateProductReviewImagesAsync(productReviewImages);

                return Json(new
                {
                    success = true
                });
            }
            return Json(new
            {
                success = false
            });
        }
        #endregion
    }
}
