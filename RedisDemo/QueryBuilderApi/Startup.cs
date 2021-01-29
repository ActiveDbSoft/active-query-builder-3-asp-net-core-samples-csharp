using ActiveQueryBuilder.Web.Core;
using ActiveQueryBuilder.Web.Core.Configuration;
using ActiveQueryBuilder.Web.Server.Configuration;
using ActiveQueryBuilder.Web.Server.Infrastructure.Providers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QueryBuilderApi.Middlewares;
using QueryBuilderApi.Providers;

namespace QueryBuilderApi
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
                o.InstanceName = "master";
            });

            services.AddSingleton(new RedlockFactoryProvider(Configuration["RedisHost"]));
            services.AddSession();
            services.AddHttpContextAccessor();

            var qbConfig = Configuration.GetSection("aspQueryBuilder");
            services.AddActiveQueryBuilder(qbConfig);
            services.AddSingleton<IQueryBuilderConfigurator>(_ =>
                new AspNetCoreQueryBuilderConfigurator(qbConfig));

            services.AddScoped<RedisQueryBuilderProvider>();
            services.AddScoped<IQueryBuilderProvider>(f => f.GetService<RedisQueryBuilderProvider>());
            services.AddScoped<IQueryTransformerProvider, RedisQueryTransformerProvider>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseMiddleware<ProcessedByHeaderMiddleware>();
            app.UseCors(o => o.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());

            app.UseRouting();

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
