using System.Globalization;
using Microsoft.AspNetCore.Localization;
using oBiletCase.Application;
using oBiletCase.Infrastructure;
using oBiletCase.Web.AppUser;
using oBiletCase.Web.ErrorHandling;
using oBiletCase.Web.Localization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAppUserContext, AppUserContext>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[]
    {
        new CultureInfo(AppCultures.Turkish),
        new CultureInfo(AppCultures.English),
    };

    options.DefaultRequestCulture = new RequestCulture(AppCultures.Turkish);
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    options.RequestCultureProviders = [new CookieRequestCultureProvider { CookieName = AppCultures.CookieName }];
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRequestLocalization();
app.UseMiddleware<AppUserIdentityMiddleware>();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
