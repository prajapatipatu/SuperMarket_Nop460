using Microsoft.AspNetCore.Routing;
using Nop.Core.Domain.Cms;
using Nop.Core.Infrastructure;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Menu;
using SS.Plugin.Widgets.ProductReview.Areas.Admin.Components;
using SS.Plugin.Widgets.ProductReview.Components;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SS.Plugin.Widgets.ProductReview
{
    /// <summary>
    /// Rename this file and change to the correct type
    /// </summary>
    public class ProductReviewPlugin : BasePlugin, IAdminMenuPlugin, IWidgetPlugin
    {
        #region Fields
        private readonly ILanguageService _languageService;
        private readonly INopFileProvider _fileProvider;
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly WidgetSettings _widgetSettings;
        #endregion

        #region ctor
        public ProductReviewPlugin(ILanguageService languageService,
            INopFileProvider fileProvider,
            ILocalizationService localizationService,
            ISettingService settingService,
            WidgetSettings widgetSettings)
        {
            _languageService = languageService;
            _fileProvider = fileProvider;
            _localizationService = localizationService;
            _settingService = settingService;
            _widgetSettings = widgetSettings;
        }
        #endregion

        #region Method

        #region Resource
        protected virtual async Task InstallLocaleResourcesAsync()
        {
            var languages = (await _languageService.GetAllLanguagesAsync()).Where(l => l.Published);
            foreach (var language in languages)
            {
                if (language != null)
                {
                    foreach (var filePath in Directory.EnumerateFiles(_fileProvider.MapPath(ProductReviewDefaults.ResourceFilePath),
                        "ResourceString.xml", SearchOption.TopDirectoryOnly))
                    {
                        using (var streamReader = new StreamReader(filePath))
                        {
                            await _localizationService.ImportResourcesFromXmlAsync(language, streamReader);
                        }

                    }
                }
            }
        }
        protected virtual async Task DeleteLocalResourcesAsync()
        {
            var file = Path.Combine(_fileProvider.MapPath(ProductReviewDefaults.ResourceFilePath), "ResourceString.xml");
            var languageResourceNames = from name in XDocument.Load(file).Document.Descendants("LocaleResource")
                                        select name.Attribute("Name").Value;
            foreach (var languge in languageResourceNames)
            {
                await _localizationService.DeleteLocaleResourcesAsync(languge);
            }
        }
        #endregion

        #region Sitemape
        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            //var ssCoreMenu = rootNode.ChildNodes.FirstOrDefault(x => x.SystemName == ShivaayCoreDefault.MenuCoreSystemName);
            //if (ssCoreMenu != null)
            //{
            //    var pluginMenu = ssCoreMenu.ChildNodes.FirstOrDefault(x => x.SystemName == ShivaayCoreDefault.MenuPluginSystemName);
            //    if (pluginMenu != null)
            //    {
            //        pluginMenu.ChildNodes.Add(new SiteMapNode()
            //        {
            //            SystemName = "SS.Plugin.Widgets.ProductReview",
            //            Title = await _localizationService.GetResourceAsync("SS.Plugin.Widgets.ProductReview"),
            //            IconClass = "nav-icon nav-icon far fa-dot-circle",
            //            Visible = true,
            //            ControllerName = "ProductReview",
            //            ActionName = "Configure",
            //            RouteValues = new RouteValueDictionary() { { "area", "admin" } }
            //        });
            //    }
            //}
            rootNode.ChildNodes.Add(new SiteMapNode()
            {
                SystemName = "SS.Plugin.Widgets.ProductReview",
                Title = await _localizationService.GetResourceAsync("SS.Plugin.Widgets.ProductReview"),
                IconClass = "nav-icon nav-icon far fa-dot-circle",
                Visible = true,
                ControllerName = "ProductReview",
                ActionName = "Configure",
                RouteValues = new RouteValueDictionary() { { "area", "admin" } }
            });
            await Task.CompletedTask;
        }
        #endregion

        #region Widget

        public Task<IList<string>> GetWidgetZonesAsync()
        {
            return Task.FromResult<IList<string>>(new List<string> 
            {
                PublicWidgetZones.ProductDetailsBottom,
                AdminWidgetZones.ProductReviewDetailsBottom

            });
        }

        public Type GetWidgetViewComponent(string widgetZone)
        {
            if (widgetZone is null)
                throw new ArgumentNullException(nameof(widgetZone));

            if (PublicWidgetZones.ProductDetailsBottom == widgetZone)
                return typeof(ProductReviewViewComponent);
            
            if (AdminWidgetZones.ProductReviewDetailsBottom == widgetZone)
                return typeof(ProductReviewImagesViewComponent);

            return null;
        }

        public bool HideInWidgetList => false;

        #endregion

        #region   Install and unInstall plugin
        public override async Task InstallAsync()
        {
            await InstallLocaleResourcesAsync();

            //enable widget
            //await ActiveWidget(this.PluginDescriptor.SystemName);
            if (!_widgetSettings.ActiveWidgetSystemNames.Contains(this.PluginDescriptor.SystemName))
            {
                _widgetSettings.ActiveWidgetSystemNames.Add(this.PluginDescriptor.SystemName);
                await _settingService.SaveSettingAsync(_widgetSettings);
            }

            //save settings
            await _settingService.SaveSettingAsync(new ProductReviewSettings
            {
                PictureSize = 10,
                MaxPicture = 5
            });

            await base.InstallAsync();            
        }
        public override async Task UninstallAsync()
        {
            await DeleteLocalResourcesAsync();

            //disable widget
            //await DisableWidget(this.PluginDescriptor.SystemName);
            if (_widgetSettings.ActiveWidgetSystemNames.Contains(this.PluginDescriptor.SystemName))
            {
                _widgetSettings.ActiveWidgetSystemNames.Remove(this.PluginDescriptor.SystemName);
                await _settingService.SaveSettingAsync(_widgetSettings);
            }

            //delete settings
            await _settingService.DeleteSettingAsync<ProductReviewSettings>();

            await base.UninstallAsync();
        }
        #endregion

        #endregion
    }
}
