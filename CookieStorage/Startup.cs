using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ActiveQueryBuilder.Web.Core;
using ActiveQueryBuilder.Web.Server.Infrastructure.Providers;
using AspNetCoreCookieStorage.Providers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCoreCookieStorage
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Active Query Builder requires support for Session HttpContext. 
            services.AddSession();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // Register CookieQueryBuilderProvider
            services.AddScoped<IQueryBuilderProvider, CookieQueryBuilderProvider>();
            services.AddActiveQueryBuilder();

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            // Active Query Builder requires support for Session HttpContext.
            app.UseSession();

            // Active Query Builder server requests handler.
            app.UseActiveQueryBuilder();

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
