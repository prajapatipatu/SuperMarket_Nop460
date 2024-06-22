using Microsoft.AspNetCore.Mvc.Razor;
using Nop.Web.Framework.Themes;
using System.Collections.Generic;
using System.Linq;

namespace SS.Plugin.Widgets.ProductReview.Infrastructure
{
    public class ViewLocationExpander : IViewLocationExpander
    {
        public void PopulateValues(ViewLocationExpanderContext context)
        {
        }

        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            var themeContext = (IThemeContext)context.ActionContext.HttpContext.RequestServices.GetService(typeof(IThemeContext));
            var themeName = themeContext.GetWorkingThemeNameAsync().Result;

            if (context.AreaName == "Admin")
            {
                viewLocations = new[] { $"/Plugins/SS.Plugin.Widgets.ProductReview/Areas/Admin/Views/{context.ControllerName}/{context.ViewName}.cshtml",
                                        $"/Plugins/SS.Plugin.Widgets.ProductReview/Areas/Admin/Views/Shared/{context.ViewName}.cshtml"}.Concat(viewLocations);
            }
            else
            {
                viewLocations = new[] {
                    $"/Plugins/SS.Plugin.Widgets.ProductReview/Themes/{themeName}/Views/{context.ControllerName}/{context.ViewName}.cshtml",
                    $"/Plugins/SS.Plugin.Widgets.ProductReview/Themes/{themeName}/Views/Shared/{context.ViewName}.cshtml",
                    $"/Plugins/SS.Plugin.Widgets.ProductReview/Views/{context.ControllerName}/{context.ViewName}.cshtml",
                    $"/Plugins/SS.Plugin.Widgets.ProductReview/Views/Shared/{context.ViewName}.cshtml",
                }.Concat(viewLocations);
            }

            return viewLocations;
        }
    }
}
