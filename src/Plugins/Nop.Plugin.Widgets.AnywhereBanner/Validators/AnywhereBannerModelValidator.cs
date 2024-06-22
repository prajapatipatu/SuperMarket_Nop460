using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using Nop.Plugin.Widgets.AnywhereBanner.Areas.Admin.Models.AnywhereBanner;

namespace Nop.Plugin.Widgets.AnywhereBanner.Validators
{
    public partial class AnywhereBannerModelValidator : BaseNopValidator<AnywhereBannerModel>
    {
        public AnywhereBannerModelValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.PictureId).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Nop.Widgets.AnywhereBanner.Areas.Admin.Models.AnywhereBanner.Fields.AnywhereBannerImage.Required"));

            RuleFor(x => x.MobilePictureId).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Nop.Widgets.AnywhereBanner.Areas.Admin.Models.AnywhereBanner.Fields.AnywhereBannerImage.Required"));
            
            RuleFor(x => x.EndDate).Must((x, context) =>
            {
                if (x.StartDate != null)
                {
                    // id start is greater then end date 
                    if (x.StartDate > x.EndDate)
                        return false;
                }

                return true;
            }).WithMessageAwait(localizationService.GetResourceAsync("Nop.Widgets.AnywhereBanner.Areas.Admin.Models.AnywhereBanner.Fields.EndDate.GreaterThanorEqualStartDate"));
        }
    }
}
