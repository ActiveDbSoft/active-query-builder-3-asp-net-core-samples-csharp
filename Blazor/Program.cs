using ActiveQueryBuilder.Web.Core;
using ActiveQueryBuilder.Web.Server.Infrastructure.Factories;
using ActiveQueryBuilder.Web.SignalR;
using QueryBuilderFactory = Blazor.Factories.QueryBuilderFactory;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.Services.AddSession();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddScoped<IQueryBuilderInstanceFactory, QueryBuilderFactory>();
builder.Services.AddActiveQueryBuilder();
builder.Services.AddActiveQueryBuilderSignalR();
builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseSession();
app.UseActiveQueryBuilder();
app.UseActiveQueryBuilderSignalR();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();