using Nop.Core;

namespace Nop.Plugin.Widgets.AnywhereBanner.Domains.GridBannerSetting
{
    public partial class GridBannerSetting : BaseEntity
    {
        public string WidgetName { get; set; }
        public int GridType { get; set; }
        public int Column { get; set; }
        public bool IsSlider { get; set; }
        public int LeftColumn { get; set; } 
        public int RightColumn { get; set; } 
    }
}
