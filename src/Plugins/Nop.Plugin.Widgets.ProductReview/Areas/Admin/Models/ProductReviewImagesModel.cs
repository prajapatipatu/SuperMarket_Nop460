using Nop.Web.Framework.Models;

namespace SS.Plugin.Widgets.ProductReview.Areas.Admin.Models
{
    public partial record ProductReviewImagesModel : BaseNopEntityModel
    {
        public string PictureIds { get; set; }
        public string VideoUrl { get; set; }
    }
}
