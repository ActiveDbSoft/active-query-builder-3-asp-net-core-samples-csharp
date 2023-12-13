using ActiveQueryBuilder.Web.Core;
using ActiveQueryBuilder.Web.Server.Infrastructure.Factories;
using ActiveQueryBuilder.Web.SignalR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using QueryBuilderFactory = Signalr.Factories.QueryBuilderFactory;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSession();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddScoped<IQueryBuilderInstanceFactory, QueryBuilderFactory>();
builder.Services.AddControllersWithViews();
builder.Services.AddActiveQueryBuilder();
builder.Services.AddActiveQueryBuilderSignalR();
builder.Services.AddSignalR();

var app = builder.Build();

app.UseCors();

app.UseSession();
app.UseActiveQueryBuilder();
app.UseActiveQueryBuilderSignalR();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();