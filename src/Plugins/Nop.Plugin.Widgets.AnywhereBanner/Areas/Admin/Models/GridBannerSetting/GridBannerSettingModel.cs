using Nop.Web.Framework.Models;

namespace Nop.Plugin.Widgets.AnywhereBanner.Areas.Admin.Models.GridBannerSetting
{
    public partial record GridBannerSettingModel : BaseNopEntityModel
    {
        public string WidgetName { get; set; }
        public int GridType { get; set; }
        public int Column { get; set; }
        public bool IsSlider { get; set; }
        public int LeftColumn { get; set; }
        public int RightColumn { get; set; }        
    }
}
