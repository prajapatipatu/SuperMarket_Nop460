using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Plugin.Widgets.AnywhereBanner.Areas.Admin.Models.GridBannerSetting;

namespace Nop.Plugin.Widgets.AnywhereBanner.Areas.Admin.Models.AnywhereBanner
{
    public partial record AnywhereBannerSearchModel : BaseSearchModel
    {
        #region Ctor
        public AnywhereBannerSearchModel()
        {
            GridBannerListItems = new List<GridBannerSettingModel>();
            WidgetNames = new List<string>();                    
        }

        #endregion

        #region Properties
        [NopResourceDisplayName("Nop.Widgets.AnywhereBanner.Areas.Admin.Models.AnywhereBanner.Fields.StartDate")]
        [UIHint("DateNullable")]
        public DateTime? StartDate { get; set; }

        [NopResourceDisplayName("Nop.Widgets.AnywhereBanner.Areas.Admin.Models.AnywhereBanner.Fields.EndDate")]
        [UIHint("DateNullable")]
        public DateTime? EndDate { get; set; }
        public IList<GridBannerSettingModel> GridBannerListItems { get; set; }        
        public IList<string> WidgetNames { get; set; }        
        #endregion  
    }
}
