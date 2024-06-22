using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using SS.Plugin.Widgets.ProductReview.Areas.Admin.Models;

namespace SS.Plugin.Widgets.ProductReview.Areas.Admin.Validators.ProductReview
{
    public partial class ConfigurationModelValidator : BaseNopValidator<ConfigurationModel>
    {
        public ConfigurationModelValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.PictureSize)
                .GreaterThanOrEqualTo(0)
                .WithMessageAwait(localizationService.GetResourceAsync("SS.Plugin.Widgets.ProductReview.Areas.Admin.Models.Fields.PictureSize.Positive"));
            RuleFor(x => x.VideoSize)
                .GreaterThanOrEqualTo(0)
                .WithMessageAwait(localizationService.GetResourceAsync("SS.Plugin.Widgets.ProductReview.Areas.Admin.Models.Fields.VideoSize.Positive"));
        }
    }
}
