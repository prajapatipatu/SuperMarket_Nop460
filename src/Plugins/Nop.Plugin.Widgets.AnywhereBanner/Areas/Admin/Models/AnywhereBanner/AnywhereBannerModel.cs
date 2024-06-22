using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Widgets.AnywhereBanner.Areas.Admin.Models.AnywhereBanner
{
    public partial record AnywhereBannerModel : BaseNopEntityModel,
        IAclSupportedModel, IStoreMappingSupportedModel
    {
        #region Ctor
        public AnywhereBannerModel()
        {
            WidgetList = new List<SelectListItem>();
            SelectedWidgets = new List<string>();

            SelectedStoreIds = new List<int>();
            AvailableStores = new List<SelectListItem>();

            SelectedCustomerRoleIds = new List<int>();
            AvailableCustomerRoles = new List<SelectListItem>();
        }
        #endregion

        #region Properties

        [NopResourceDisplayName("Nop.Widgets.AnywhereBanner.Areas.Admin.Models.AnywhereBanner.Fields.AnywhereBannerImage")]
        [UIHint("Picture")]
        public int PictureId { get; set; }
        [NopResourceDisplayName("Nop.Widgets.AnywhereBanner.Areas.Admin.Models.AnywhereBanner.Fields.AnywhereMobileBannerImage")]
        [UIHint("Picture")]
        public int MobilePictureId { get; set; }
        [NopResourceDisplayName("Nop.Widgets.AnywhereBanner.Areas.Admin.Models.AnywhereBanner.Fields.StartDate")]
        [UIHint("DateTimeNullable")]
        public DateTime? StartDate { get; set; }
        [NopResourceDisplayName("Nop.Widgets.AnywhereBanner.Areas.Admin.Models.AnywhereBanner.Fields.EndDate")]
        [UIHint("DateTimeNullable")]
        public DateTime? EndDate { get; set; }
        [NopResourceDisplayName("Nop.Widgets.AnywhereBanner.Areas.Admin.Models.AnywhereBanner.Fields.CTA")]
        public string CTA { get; set; }
        [NopResourceDisplayName("Nop.Widgets.AnywhereBanner.Areas.Admin.Models.AnywhereBanner.Fields.Widget")]
        public string Widget { get; set; }

        [NopResourceDisplayName("Nop.Widgets.AnywhereBanner.Areas.Admin.Models.AnywhereBanner.Fields.Widget")]
        public IList<string> SelectedWidgets { get; set; }
        public IList<SelectListItem> WidgetList { get; set; }
        
        [NopResourceDisplayName("Nop.Widgets.AnywhereBanner.Areas.Admin.AnywhereBanner.PictureThumbnailUrl")]
        public string PictureThumbnailUrl { get; set; }
        [NopResourceDisplayName("Nop.Widgets.AnywhereBanner.Areas.Admin.AnywhereBanner.MobilePictureThumbnailUrl")]
        public string MobilePictureThumbnailUrl { get; set; }

        //ACL (customer roles)
        [NopResourceDisplayName("Nop.Widgets.AnywhereBanner.Areas.Admin.AnywhereBanner.AclCustomerRoles")]
        public IList<int> SelectedCustomerRoleIds { get; set; }
        public IList<SelectListItem> AvailableCustomerRoles { get; set; }

        //store mapping
        [NopResourceDisplayName("Nop.Widgets.AnywhereBanner.Areas.Admin.AnywhereBanner.LimitedToStores")]
        public IList<int> SelectedStoreIds { get; set; }
        public IList<SelectListItem> AvailableStores { get; set; }

        [NopResourceDisplayName("Nop.Widgets.AnywhereBanner.Areas.Admin.AnywhereBanner.Published")]
        public bool Published { get; set; }

        [NopResourceDisplayName("Nop.Widgets.AnywhereBanner.Areas.Admin.AnywhereBanner.DisplayOrder")]
        public int DisplayOrder { get; set; }

        #endregion
    }    
}
