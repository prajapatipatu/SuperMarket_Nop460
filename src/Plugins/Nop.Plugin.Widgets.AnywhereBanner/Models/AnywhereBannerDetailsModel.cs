using System;
using System.Collections.Generic;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Widgets.AnywhereBanner.Models
{
    public partial record AnywhereBannerDetailsModel : BaseNopEntityModel
    {
        public AnywhereBannerDetailsModel()
        {
            BannerModels = new List<BannerModel>();
        }
        public int GridType { get; set; }
        public int Column { get; set; }
        public bool IsSlider { get; set; }
        public int LeftColumn { get; set; }
        public int RightColumn { get; set; }    
        
        public List<BannerModel> BannerModels { get; set; }

    }

    public partial record BannerModel : BaseNopEntityModel
    {
        public int PictureId { get; set; }
        public int MobilePictureId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string CTA { get; set; }
        public string PictureThumbnailUrl { get; set; }
        public string MobilePictureThumbnailUrl { get; set; }
        public bool Published { get; set; }
    }
}
