﻿using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Plugin.Widgets.AnywhereBanner.Areas.Admin.Models.AnywhereBanner;
using Nop.Plugin.Widgets.AnywhereBanner.Factories;
using Nop.Plugin.Widgets.AnywhereBanner.Services.AnywhereBanner;
using Nop.Plugin.Widgets.AnywhereBanner.Validators;
using Admin = Nop.Plugin.Widgets.AnywhereBanner.Areas.Admin.Factories;

namespace Nop.Plugin.Widgets.AnywhereBanner.Infrastructure
{
    /// <summary>
    /// Represents object for the configuring services on application startup
    /// </summary>
    public class NopStartup : INopStartup
    {
        /// <summary>
        /// Add and configure any of the middleware
        /// </summary>
        /// <param name="services">Collection of service descriptors</param>
        /// <param name="configuration">Configuration of the application</param>
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            //register custom services
            services.AddScoped<IAnywhereBannerService, AnywhereBannerService>();

            //register Admin factories
            services.AddScoped<Admin.IAnywhereBannerFactory, Admin.AnywhereBannerFactory>();

            //register Front factories
            services.AddScoped<IAnywhereBannerFactory, AnywhereBannerFactory>();            

            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.ViewLocationExpanders.Add(new ViewLocationExpander());
            });

            //register validatore
            services.AddScoped<IValidator<AnywhereBannerModel>, AnywhereBannerModelValidator>();

        }

        /// <summary>
        /// Configure the using of added middleware
        /// </summary>
        /// <param name="application">Builder for configuring an application's request pipeline</param>
        public void Configure(IApplicationBuilder application)
        {
        }

        /// <summary>
        /// Gets order of this startup configuration implementation
        /// </summary>
        public int Order => int.MaxValue;
    }
}
