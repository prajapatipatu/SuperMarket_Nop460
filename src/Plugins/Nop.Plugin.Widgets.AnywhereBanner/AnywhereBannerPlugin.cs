using Microsoft.AspNetCore.Routing;
using Nop.Core.Domain.Cms;
using Nop.Core.Infrastructure;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Web.Framework.Menu;
using Nop.Plugin.Widgets.AnywhereBanner.Components;
using Nop.Plugin.Widgets.AnywhereBanner.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Nop.Plugin.Widgets.AnywhereBanner
{
    /// <summary>
    /// Rename this file and change to the correct type
    /// </summary>
    public class AnywhereBannerPlugin : BasePlugin, IAdminMenuPlugin, IWidgetPlugin
    {
        #region Fields
        private readonly ILanguageService _languageService;
        private readonly INopFileProvider _fileProvider;
        private readonly ILocalizationService _localizationService;
        private readonly WidgetSettings _widgetSettings;
        private readonly ISettingService _settingService;
        #endregion

        #region Ctor
        public AnywhereBannerPlugin(ILanguageService languageService,
            INopFileProvider fileProvider,
            ILocalizationService localizationService,
            WidgetSettings widgetSettings,
            ISettingService settingService)
        {
            _languageService = languageService;
            _fileProvider = fileProvider;
            _localizationService = localizationService;
            _widgetSettings = widgetSettings;
            _settingService = settingService;
        }
        #endregion

        #region Utilities
        protected virtual async Task InstallLocaleResourcesAsync()
        {
            var languages = (await _languageService.GetAllLanguagesAsync()).Where(l => l.Published);
            foreach (var language in languages)
            {
                if (language != null)
                {
                    //save resources
                    foreach (var filePath in Directory.EnumerateFiles(_fileProvider.MapPath(AnyWhereBannerDefaults.ResourceFilePath),
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
            var file = Path.Combine(_fileProvider.MapPath(AnyWhereBannerDefaults.ResourceFilePath), "ResourceString.xml");
            var languageResourceNames = from name in XDocument.Load(file).Document.Descendants("LocaleResource")
                                        select name.Attribute("Name").Value;

            foreach (var item in languageResourceNames)
            {
                await _localizationService.DeleteLocaleResourceAsync(item);
            }
        }
        #endregion

        #region Methods

        #region Widget

        public Type GetWidgetViewComponent(string widgetZone)
        {
            return typeof(AnywhereBannerViewComponent);
        }

        public Task<IList<string>> GetWidgetZonesAsync()
        {
            var widgets = Enum.GetValues(typeof(EnumWidgetZone))
                .Cast<EnumWidgetZone>()
                .Select(x => x.ToString()).ToList();

            return Task.FromResult<IList<string>>(widgets);
        }
        public bool HideInWidgetList => false;

        #endregion

        #region Install and Uninstall

        public override async Task InstallAsync()
        {
            //Local resource
            await InstallLocaleResourcesAsync();

            //enable widget
            if (!_widgetSettings.ActiveWidgetSystemNames.Contains(this.PluginDescriptor.SystemName))
            {
                _widgetSettings.ActiveWidgetSystemNames.Add(this.PluginDescriptor.SystemName);
                await _settingService.SaveSettingAsync(_widgetSettings);
            }
            //await ActiveWidget(this.PluginDescriptor.SystemName);
            await base.InstallAsync();
        }
        public override async Task UninstallAsync()
        {
            //Local resource
            await DeleteLocalResourcesAsync();

            //disable widget
            //settings
            if (_widgetSettings.ActiveWidgetSystemNames.Contains(this.PluginDescriptor.SystemName))
            {
                _widgetSettings.ActiveWidgetSystemNames.Remove(this.PluginDescriptor.SystemName);
                await _settingService.SaveSettingAsync(_widgetSettings);
            }
            //await DisableWidget(this.PluginDescriptor.SystemName);
            await base.UninstallAsync();
        }

        #endregion

        #region Admin Menu
        public virtual async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            rootNode.ChildNodes.Add(new SiteMapNode()
            {
                SystemName = "AnywhereBanner",
                Title = await _localizationService.GetResourceAsync("Nop.Widgets.AnywhereBanner.Areas.Admin.AnywhereBanner"),
                IconClass = "nav-icon far fa-dot-circle",
                Visible = true,
                ControllerName = "AnywhereBanner",
                ActionName = "List",
                RouteValues = new RouteValueDictionary() { { "area", "admin" } }
            });
            await Task.CompletedTask;
        }

      
        #endregion

        #endregion
    }
}
