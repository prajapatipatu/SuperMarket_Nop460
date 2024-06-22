using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Plugin.Widgets.AnywhereBanner.Models;

namespace Nop.Plugin.Widgets.AnywhereBanner.Factories
{
    public partial interface IAnywhereBannerFactory
    {
        /// <summary>
        /// Prepare AnywhereBanner models
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the list of homepage AnywhereBanner models
        /// </returns>
        Task<AnywhereBannerDetailsModel> PrepareAnywhereBannerDetailsModelsAsync(string widgetZone);
    }
}
