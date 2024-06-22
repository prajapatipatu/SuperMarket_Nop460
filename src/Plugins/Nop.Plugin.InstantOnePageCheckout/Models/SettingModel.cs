using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.InstantOnePageCheckout.Models
{
    public partial record SettingModel : BaseNopEntityModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Nop.Plugin.InstantOnePageCheckout.OnePage.Checkout")]
        public bool InstantOnePageCheckoutDisabled { get; set; }
        public bool InstantOnePageCheckoutDisabled_OverrideForStore { get; set; }
    }
}
