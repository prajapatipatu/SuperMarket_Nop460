using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Plugin.Widgets.AnywhereBanner.Areas.Admin.Factories;
using Nop.Plugin.Widgets.AnywhereBanner.Areas.Admin.Models.AnywhereBanner;
using Nop.Plugin.Widgets.AnywhereBanner.Domains.AnywhereBanner;
using Nop.Plugin.Widgets.AnywhereBanner.Domains.GridBannerSetting;
using Nop.Plugin.Widgets.AnywhereBanner.Services.AnywhereBanner;

namespace Nop.Plugin.Widgets.AnywhereBanner.Areas.Admin.Controllers
{
    public class AnywhereBannerController : BaseAdminController
    {
        #region Fields
        private readonly IPermissionService _permissionService;
        private readonly IAnywhereBannerFactory _anywhereBannerFactory;
        private readonly IAnywhereBannerService _anywhereBannerService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly IAclService _aclService;
        private readonly ICustomerService _customerService;
        protected readonly IStoreMappingService _storeMappingService;
        protected readonly IStoreService _storeService;
        #endregion       

        #region Ctor
        public AnywhereBannerController(IPermissionService permissionService,
            IAnywhereBannerFactory anywhereBannerFactory,
            IAnywhereBannerService anywhereBannerService,
            INotificationService notificationService,
            ILocalizationService localizationService,
            IAclService aclService,
            ICustomerService customerService,
            IStoreMappingService storeMappingService,
            IStoreService storeService)
        {
            _permissionService = permissionService;
            _anywhereBannerFactory = anywhereBannerFactory;
            _anywhereBannerService = anywhereBannerService;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _aclService = aclService;
            _customerService = customerService;
            _storeMappingService = storeMappingService;
            _storeService = storeService;
        }
        #endregion

        #region Utilities
        protected virtual async Task SaveAnywhereBannerAclAsync(AnywhereBannerDetails anywhereBannerDetails, AnywhereBannerModel model)
        {
            anywhereBannerDetails.SubjectToAcl = model.SelectedCustomerRoleIds.Any();
            await _anywhereBannerService.UpdateAnywhereBannerAsync(anywhereBannerDetails);

            var existingAclRecords = await _aclService.GetAclRecordsAsync(anywhereBannerDetails);
            var allCustomerRoles = await _customerService.GetAllCustomerRolesAsync(true);
            foreach (var customerRole in allCustomerRoles)
            {
                if (model.SelectedCustomerRoleIds.Contains(customerRole.Id))
                {
                    //new role
                    if (existingAclRecords.Count(acl => acl.CustomerRoleId == customerRole.Id) == 0)
                        await _aclService.InsertAclRecordAsync(anywhereBannerDetails, customerRole.Id);
                }
                else
                {
                    //remove role
                    var aclRecordToDelete = existingAclRecords.FirstOrDefault(acl => acl.CustomerRoleId == customerRole.Id);
                    if (aclRecordToDelete != null)
                        await _aclService.DeleteAclRecordAsync(aclRecordToDelete);
                }
            }
        }

        protected virtual async Task UpdateAnywhereBannerStoreMappingsAsync(AnywhereBannerDetails anywhereBannerDetails, IList<int> limitedToStoresIds)
        {
            anywhereBannerDetails.LimitedToStores = limitedToStoresIds.Any();
            await _anywhereBannerService.UpdateAnywhereBannerAsync(anywhereBannerDetails);

            var existingStoreMappings = await _storeMappingService.GetStoreMappingsAsync(anywhereBannerDetails);
            var allStores = await _storeService.GetAllStoresAsync();
            foreach (var store in allStores)
            {
                if (limitedToStoresIds.Contains(store.Id))
                {
                    //new store
                    if (!existingStoreMappings.Any(sm => sm.StoreId == store.Id))
                        await _storeMappingService.InsertStoreMappingAsync(anywhereBannerDetails, store.Id);
                }
                else
                {
                    //remove store
                    var storeMappingToDelete = existingStoreMappings.FirstOrDefault(sm => sm.StoreId == store.Id);
                    if (storeMappingToDelete != null)
                        await _storeMappingService.DeleteStoreMappingAsync(storeMappingToDelete);
                }
            }
        }

        #endregion

        #region GridBanner Utilities
        protected virtual async Task InsertGridBannerSetting(string widgetName, int gridBannerType, int column, bool slider, int leftColumn, int rightColumn)
        {
            //insert GridBanner
            var gridBanner = new GridBannerSetting()
            {
                WidgetName = widgetName,
                GridType = gridBannerType,
                Column = column,
                IsSlider = slider,
                LeftColumn = leftColumn,
                RightColumn = rightColumn,
            };
            await _anywhereBannerService.InsertGridBannerAsync(gridBanner);
        }
        protected virtual async Task UpdateGridBannerSetting(string widgetName, int gridBannerType, int column, bool slider, int leftColumn, int rightColumn)
        {
            //update GridBanner
            var gridBanner = await _anywhereBannerService.GetByWidgetName(widgetName);
            if (gridBanner != null)
            {
                gridBanner.WidgetName = widgetName;
                gridBanner.GridType = gridBannerType;
                gridBanner.Column = column;
                gridBanner.IsSlider = slider;
                gridBanner.LeftColumn = leftColumn;
                gridBanner.RightColumn = rightColumn;

                await _anywhereBannerService.UpdateGridBannerAsync(gridBanner);
            }
        }
        #endregion

        #region methods

        public virtual IActionResult Index()
        {
            return RedirectToAction("List");
        }
        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var model = await _anywhereBannerFactory.PrepareAnywhereBannerSearchModelAsync(new AnywhereBannerSearchModel())
                         ?? throw new ArgumentException(await _localizationService.GetResourceAsync("Nop.Widgets.AnywhereBanner.Areas.Admin.NotFound"));

            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> List(AnywhereBannerSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
                return await AccessDeniedDataTablesJson();

            var model = await _anywhereBannerFactory.PrepareAnywhereBannerListModelAsync(searchModel);

            return Json(model);
        }

        public virtual async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            //prepare model
            var model = await _anywhereBannerFactory.PrepareAnywhereBannerModelAsync(new AnywhereBannerModel(), null);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        public virtual async Task<IActionResult> Create(AnywhereBannerModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var anywhereBannerDetails = new AnywhereBannerDetails();
            if (ModelState.IsValid)
            {
                var selectedWidget = string.Empty;
                if (model.SelectedWidgets.Count > 0)
                    selectedWidget = string.Join(",", model.SelectedWidgets);

                anywhereBannerDetails.PictureId = model.PictureId;
                anywhereBannerDetails.MobilePictureId = model.MobilePictureId;
                anywhereBannerDetails.StartDate = model.StartDate;
                anywhereBannerDetails.EndDate = model.EndDate;
                anywhereBannerDetails.Widget = selectedWidget;
                anywhereBannerDetails.CTA = model.CTA;
                anywhereBannerDetails.Published = model.Published;
                anywhereBannerDetails.DisplayOrder = model.DisplayOrder;

                await _anywhereBannerService.InsertAnywhereBannerAsync(anywhereBannerDetails);
                
                // customer role
                await SaveAnywhereBannerAclAsync(anywhereBannerDetails, model);
               
                //stores
                await UpdateAnywhereBannerStoreMappingsAsync(anywhereBannerDetails, model.SelectedStoreIds);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Nop.Widgets.OfferBanner.Areas.Admin.OfferBanners.Added.Success.Message"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = anywhereBannerDetails.Id });
            }

            model = await _anywhereBannerFactory.PrepareAnywhereBannerModelAsync(model, null, true);

            return View(model);
        }

        public virtual async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            //try to get a customer with the specified id
            var anywhereBannerDetails = await _anywhereBannerService.GetAnywhereBannerByIdAsync(id);
            if (anywhereBannerDetails == null)
                return RedirectToAction("List");

            //prepare model
            var model = await _anywhereBannerFactory.PrepareAnywhereBannerModelAsync(null, anywhereBannerDetails);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        public virtual async Task<IActionResult> Edit(AnywhereBannerModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            //try to get a customer with the specified id
            var anywhereBannerDetails = await _anywhereBannerService.GetAnywhereBannerByIdAsync(model.Id);

            if (anywhereBannerDetails == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                var selectedWidget = string.Empty;
                if (model.SelectedWidgets.Count > 0)
                    selectedWidget = string.Join(",", model.SelectedWidgets);

                try
                {
                    anywhereBannerDetails.PictureId = model.PictureId;
                    anywhereBannerDetails.MobilePictureId = model.MobilePictureId;
                    anywhereBannerDetails.StartDate = model.StartDate;
                    anywhereBannerDetails.EndDate = model.EndDate;
                    anywhereBannerDetails.Widget = selectedWidget;
                    anywhereBannerDetails.CTA = model.CTA;
                    anywhereBannerDetails.Published = model.Published;
                    anywhereBannerDetails.DisplayOrder = model.DisplayOrder;

                    await _anywhereBannerService.UpdateAnywhereBannerAsync(anywhereBannerDetails);

                    // customer role
                    await SaveAnywhereBannerAclAsync(anywhereBannerDetails, model);

                    //stores
                    await UpdateAnywhereBannerStoreMappingsAsync(anywhereBannerDetails, model.SelectedStoreIds);

                    _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Nop.Widgets.AnywhereBanner.Areas.Admin.AnywhereBanner.Edited.Success.Message"));

                    if (!continueEditing)
                        return RedirectToAction("List");

                    return RedirectToAction("Edit", new { id = anywhereBannerDetails.Id });
                }
                catch (Exception exc)
                {
                    _notificationService.ErrorNotification(exc.Message);
                }
            }
            model = await _anywhereBannerFactory.PrepareAnywhereBannerModelAsync(model, anywhereBannerDetails, true);
            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var anywhereBannerDetails = await _anywhereBannerService.GetAnywhereBannerByIdAsync(id);

            if (anywhereBannerDetails == null)
                return RedirectToAction("List");

            await _anywhereBannerService.DeleteAnywhereBannerAsync(anywhereBannerDetails);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Nop.Widgets.OfferBanner.Areas.Admin.OfferBanners.Deleted"));

            return RedirectToAction("List");
        }

        [HttpPost]
        public virtual async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            if (selectedIds == null || selectedIds.Count == 0)
                return NoContent();

            await _anywhereBannerService.DeleteSelectAnywhereBannerAsync((await _anywhereBannerService.GetAnywhereBannerByIdsAsync(selectedIds.ToArray())).ToList());

            return Json(new { Result = true });
        }

        public virtual async Task<IActionResult> DeleteAnyWhereBanner(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();

            var anywhereBannerDetails = await _anywhereBannerService.GetAnywhereBannerByIdAsync(id);

            if (anywhereBannerDetails == null)
                return RedirectToAction("List");

            await _anywhereBannerService.DeleteAnywhereBannerAsync(anywhereBannerDetails);

            return new NullJsonResult();            
        }
        #endregion

        #region  Grid Banner
        [HttpPost]
        public virtual async Task<IActionResult> GridBannerList(IFormCollection form)
        {
            var results = _anywhereBannerService.GetAnywhereBannerDetailsByWidgetNameListAsync();

            foreach (var widgetName in results)
            {
                int.TryParse(form["GridBanner_" + widgetName], out var gridBannerType);
                int.TryParse(form["GridBannerColumn_" + widgetName], out var column);
                int.TryParse(form["LeftColumn_" + widgetName], out var leftColumn);
                int.TryParse(form["RightColumn_" + widgetName], out var rightColumn);
                var isSlider = form["IsSlider_" + widgetName];
                var slider = false;
                if (isSlider == "on")
                {
                    slider = true;
                }
                //update GridBanner
                if (gridBannerType == 10)
                {
                    await UpdateGridBannerSetting(widgetName, gridBannerType, column, slider, 0, 0);
                }
                else if (gridBannerType == 20)
                {
                    await UpdateGridBannerSetting(widgetName, gridBannerType, 2, false, leftColumn, rightColumn);
                }
            }
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Nop.Widgets.AnywhereBanner.Areas.Admin.UpdateGridBanner"));
            return RedirectToAction("List");
        }
        #endregion
    }
}
