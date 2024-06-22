using Nop.Core.Configuration;

namespace SS.Plugin.Widgets.ProductReview
{
    public class ProductReviewSettings : ISettings
    {
        public int PictureSize { get; set; }        
        public int MaxPicture { get; set; }
    }
}
