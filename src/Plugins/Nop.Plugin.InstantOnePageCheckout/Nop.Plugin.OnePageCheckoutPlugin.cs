using Microsoft.AspNetCore.Routing;
using Nop.Core.Infrastructure;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Web.Framework.Menu;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Nop.Core.Domain.Cms;
using Nop.Services.Configuration;

namespace Nop.Plugin.InstantOnePageCheckout
{

    /// <summary>
    /// Rename this file and change to the correct type
    /// </summary>
    public class OnePageCheckoutPlugin : BasePlugin, IAdminMenuPlugin 
    {
        #region Fields
        private readonly ILanguageService _languageService;
        private readonly INopFileProvider _fileProvider;
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly WidgetSettings _widgetSettings;
        #endregion

        #region Ctor
        public OnePageCheckoutPlugin(ILanguageService languageService,
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

        #region Utilities
        protected virtual async Task InstallLocaleResourcesAsync()
        {
            var languages = await _languageService.GetAllLanguagesAsync();
            var language = languages.Where(p => p.Name == "EN").FirstOrDefault();

            if (language != null)
            {
                //save resources
                foreach (var filePath in Directory.EnumerateFiles(_fileProvider.MapPath(OnePageCheckOutDefaults.ResourceFilePath),
                    "ResourceString.xml", SearchOption.TopDirectoryOnly))
                {
                    using (var streamReader = new StreamReader(filePath))
                    {
                        await _localizationService.ImportResourcesFromXmlAsync(language, streamReader);
                    }
                }
            }
        }

        protected virtual async Task DeleteLocalResourcesAsync()
        {
            var file = Path.Combine(_fileProvider.MapPath(OnePageCheckOutDefaults.ResourceFilePath), "ResourceString.xml");
            var languageResourceNames = from name in XDocument.Load(file).Document.Descendants("LocaleResource")
                                        select name.Attribute("Name").Value;

            foreach (var item in languageResourceNames)
            {
                await _localizationService.DeleteLocaleResourceAsync(item);
            }
        }
        #endregion

        #region Methods
       
        public override async Task InstallAsync()
        {
            //Local resource
            await InstallLocaleResourcesAsync();
            await base.InstallAsync();
        }
        public override async Task UninstallAsync()
        {
            //Local resource
            await DeleteLocalResourcesAsync();
            await base.UninstallAsync();
        }

        #endregion
        
        #region Menu
        public virtual async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            //var ssCoreMenu = rootNode.ChildNodes.FirstOrDefault(x => x.SystemName == ShivaayCoreDefault.MenuCoreSystemName);
            //if (ssCoreMenu != null)
            //{
            //    var pluginMenu = ssCoreMenu.ChildNodes.FirstOrDefault(x => x.SystemName == ShivaayCoreDefault.MenuPluginSystemName);
            //    if (pluginMenu != null)
            //    {
            //        pluginMenu.ChildNodes.Add(new SiteMapNode()
            //        {
            //            SystemName = "InstantOnePageCheckout",
            //            Title = await _localizationService.GetResourceAsync("SS.Plugin.InstantOnePageCheckout"),
            //            IconClass = "nav-icon far fa-dot-circle",
            //            Visible = true,
            //            ControllerName = "OnePageCheckoutSetting",
            //            ActionName = "Settings",
            //            RouteValues = new RouteValueDictionary() { { "area", "admin" } }
            //        });
            //    }
            //}
            rootNode.ChildNodes.Add(new SiteMapNode()
            {
                SystemName = "InstantOnePageCheckout",
                Title = await _localizationService.GetResourceAsync("Nop.Plugin.InstantOnePageCheckout"),
                IconClass = "nav-icon far fa-dot-circle",
                Visible = true,
                ControllerName = "OnePageCheckoutSetting",
                ActionName = "Settings",
                RouteValues = new RouteValueDictionary() { { "area", "admin" } }
            });
            await Task.CompletedTask;
        }
        #endregion
    }
}
