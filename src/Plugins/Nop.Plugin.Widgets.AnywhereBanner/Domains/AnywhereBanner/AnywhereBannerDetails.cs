using System;
using Nop.Core;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Stores;

namespace Nop.Plugin.Widgets.AnywhereBanner.Domains.AnywhereBanner
{
    public partial class AnywhereBannerDetails : BaseEntity, IAclSupported, IStoreMappingSupported, ILocalizedEntity
    {
        public int PictureId { get; set; }
        public int MobilePictureId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string CTA { get; set; }
        public string Widget { get; set; }
        public bool SubjectToAcl { get; set; }
        public bool LimitedToStores { get; set; }
        public bool Published { get; set; }
        public int DisplayOrder { get; set; }
    }
}
