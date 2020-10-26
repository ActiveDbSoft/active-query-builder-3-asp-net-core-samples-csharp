using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ActiveQueryBuilder.Web.Core;
using ActiveQueryBuilder.Web.Core.Configuration;
using ActiveQueryBuilder.Web.Server.Configuration;
using ActiveQueryBuilder.Web.Server.Infrastructure.Providers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RedisStorage.Providers;

namespace RedisStorage
{
    public class Startup
    {
        public Startup(IWebHostEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddJsonFile("ActiveQueryBuilder.json")
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddStackExchangeRedisCache(o =>
            {
                o.Configuration = Configuration["RedisHost"];
                o.InstanceName = "AqbRedisDemo";
            });

            services.AddSession();
            services.AddHttpContextAccessor();

            var qbConfig = Configuration.GetSection("aspQueryBuilder");
            services.AddActiveQueryBuilder(qbConfig);
            services.AddSingleton<IQueryBuilderConfigurator>(_ =>
                new AspNetCoreQueryBuilderConfigurator(qbConfig));

            services.AddScoped<RedisQueryBuilderProvider>();
            services.AddScoped<IQueryBuilderProvider>(f => f.GetService<RedisQueryBuilderProvider>());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseCors(o => o.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseSession();
            app.UseActiveQueryBuilder();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}