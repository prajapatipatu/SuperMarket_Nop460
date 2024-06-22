using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Plugin.InstantOnePageCheckout.Models;

namespace Nop.Plugin.InstantOnePageCheckout.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    [AutoValidateAntiforgeryToken]
    public partial class OnePageCheckoutSettingController : BasePluginController
    {
        #region MyRegion

        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;

        #endregion

        #region Ctor
        public OnePageCheckoutSettingController(IStoreContext storeContext,
            ISettingService settingService,
            INotificationService notificationService,
            ILocalizationService localizationService)
        {
            _settingService = settingService;
            _storeContext = storeContext;
            _notificationService = notificationService;
            _localizationService = localizationService;
        }
        #endregion

        #region Method

        public async Task<IActionResult> Settings()
        {
            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var onePageCheckoutSetting = await _settingService.LoadSettingAsync<InstantOnePageCheckOutSetting>(storeScope);

            var model = new SettingModel()
            {
                InstantOnePageCheckoutDisabled = onePageCheckoutSetting.InstantOnePageCheckoutDisabled,
                ActiveStoreScopeConfiguration = storeScope
            };
            if (storeScope <= 0)
                return View("~/Plugins/Nop.Plugin.InstantOnePageCheckout/Views/Setting.cshtml", model);

            await _settingService.SettingExistsAsync(onePageCheckoutSetting, x => x.InstantOnePageCheckoutDisabled, storeScope);

            return View("~/Plugins/Nop.Plugin.InstantOnePageCheckout/Views/Setting.cshtml", model);
        }

        [HttpPost]
        [FormValueRequired("save")]
        public async Task<IActionResult> Settings(SettingModel model)
        {
            if (!ModelState.IsValid)
                return await Settings();
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var onePageCheckoutSetting = await _settingService.LoadSettingAsync<InstantOnePageCheckOutSetting>(storeScope);

            onePageCheckoutSetting.InstantOnePageCheckoutDisabled = model.InstantOnePageCheckoutDisabled;

            await _settingService.SaveSettingOverridablePerStoreAsync(onePageCheckoutSetting, x => x.InstantOnePageCheckoutDisabled, model.InstantOnePageCheckoutDisabled_OverrideForStore, storeScope, false);

            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

            return await Settings();
        }

        #endregion
    }
}
