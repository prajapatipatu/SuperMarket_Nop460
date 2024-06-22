using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace SS.Plugin.Widgets.ProductReview.Areas.Admin.Models
{
    public partial record ConfigurationModel : BaseNopEntityModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("SS.Plugin.Widgets.ProductReview.Areas.Admin.Models.Fields.PictureSize")]
        public int PictureSize { get; set; }

        public bool PictureSize_OverrideForStore { get; set; }

        [NopResourceDisplayName("SS.Plugin.Widgets.ProductReview.Areas.Admin.Models.Fields.VideoSize")]
        public int VideoSize { get; set; }

        public bool Active_OverrideForStore { get; set; }

        [NopResourceDisplayName("SS.Plugin.Widgets.ProductReview.Areas.Admin.Models.Fields.MaxPicture")]
        public int MaxPicture { get; set; }
    }
}
