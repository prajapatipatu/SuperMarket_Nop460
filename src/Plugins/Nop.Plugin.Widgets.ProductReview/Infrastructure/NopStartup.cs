using System;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using SS.Plugin.Widgets.ProductReview.Areas.Admin.Factories;
using SS.Plugin.Widgets.ProductReview.Areas.Admin.Models;
using SS.Plugin.Widgets.ProductReview.Areas.Admin.Validators.ProductReview;
using SS.Plugin.Widgets.ProductReview.Factories;
using SS.Plugin.Widgets.ProductReview.Models;
using SS.Plugin.Widgets.ProductReview.Services;
using SS.Plugin.Widgets.ProductReview.Validators;

namespace SS.Plugin.Widgets.ProductReview.Infrastructure
{
    /// <summary>
    /// Represents object for the configuring services on application startup
    /// </summary>
    public class NopStartup : INopStartup
    {
        /// <summary>
        /// Add and configure any of the middleware
        /// </summary>
        /// <param name="application"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            //register custom services
            services.AddScoped<IWidgetProductReviewService, WidgetProductReviewService>();

            //Register Admin factories
            services.AddScoped<IProductReviewImagesModelFactory, ProductReviewImagesModelFactory>();

            //register Front factories
            services.AddScoped<IWidgetProductReviewModelFactory, WidgetProductReviewModelFactory>();

            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.ViewLocationExpanders.Add(new ViewLocationExpander());
            });

            //register validator
            services.AddScoped<IValidator<ConfigurationModel>, ConfigurationModelValidator>();
            services.AddScoped<IValidator<ProductReviewsModel>, ProductReviewsValidator>();
            
        }

        /// <summary>
        /// Configure the using of added middleware
        /// </summary>
        /// <param name="application"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void Configure(IApplicationBuilder application)
        {
        }

        /// <summary>
        /// Gets order of this startup configuration implementation
        /// </summary>    
        public int Order => int.MaxValue;
    }
}
