using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ActiveQueryBuilder.Web.Core;
using ActiveQueryBuilder.Web.Server.Infrastructure.Factories;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CookieSecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy;

namespace AspNetCoreJavaScript
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                //.AddJsonFile("ActiveQueryBuilder.json") // Uncomment this line to read the settings from the JSON file
                //.AddXmlFile("ActiveQueryBuilder.xml") // Uncomment this line to read the settings from the XML file
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Active Query Builder requires support for Session HttpContext. 
            services.AddSession(options =>
            {
                // Prevent SameSite=lax cookie property for Electron
                options.Cookie.SameSite = SameSiteMode.None;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            });

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton(Configuration);
            services.AddScoped<IQueryBuilderInstanceFactory, Factories.QueryBuilderFactory>();
            services.AddScoped<IQueryTransformerInstanceFactory, Factories.QueryBuilderFactory>();

            // This service provides access to instances of QueryBuilder and QueryTranformer objects on the server.
            IConfiguration qbConfig = null;
            //qbConfig = Configuration.GetSection("aspQueryBuilder"); // Uncomment this line to apply settings from the configuration file
            services.AddActiveQueryBuilder(qbConfig);

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

            app.UseDefaultFiles();
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
