using Nop.Core;
using Nop.Core.Domain.Localization;

namespace SS.Plugin.Widgets.ProductReview.Domains
{
    public partial class ProductReviewImages : BaseEntity, ILocalizedEntity
    {
        /// <summary>
        /// Gets or sets the Product Review identifire
        /// </summary>
        public int ProductReviewId { get; set; }
        /// <summary>
        /// Gets or sets the picture identifire
        /// </summary>
        public string PictureId { get; set; }
        /// <summary>
        /// Gets or sets the Video url
        /// </summary>
        public string VideoUrl { get; set; }
    }
}
