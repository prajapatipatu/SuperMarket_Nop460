using Nop.Data.Mapping;
using System;
using System.Collections.Generic;
using Nop.Plugin.Widgets.AnywhereBanner.Domains.AnywhereBanner;
using Nop.Plugin.Widgets.AnywhereBanner.Domains.GridBannerSetting;

namespace Nop.Plugin.Widgets.AnywhereBanner.Mapping
{
    public partial class NameCompatibility : INameCompatibility
    {
        public Dictionary<Type, string> TableNames => new Dictionary<Type, string>()
        {
            { typeof(AnywhereBannerDetails), "Nop_AnywhereBannerDetails" },
            { typeof(GridBannerSetting), "Nop_GridBannerSetting" }
        };

        public Dictionary<(Type, string), string> ColumnName => new Dictionary<(Type, string), string>();
    }
}