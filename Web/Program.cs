using BLL;
using BLL.Common;
using BLL.Identity;
using BLL.Services.Interfaces;
using Web.Services.Implementations;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("CinemaDb")
    ?? throw new InvalidOperationException(
        "Connection string 'CinemaDb' is missing. Add it to the ConnectionStrings section of appsettings.json.");

var defaultAdmin = builder.Configuration.GetSection("DefaultAdmin").Get<DefaultAdminOptions>()
    ?? throw new InvalidOperationException("The DefaultAdmin section is missing from appsettings.json.");

builder.Services.AddControllersWithViews();

builder.Services.AddBusinessLogic(connectionString, builder.Configuration["AutoMapper:LicenseKey"]);

builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<ICurrentUserService, HttpContextCurrentUserService>();

builder.Services.AddWebOptimizer(pipeline =>
{
    pipeline.AddCssBundle("/css/bundle.css", "lib/bootstrap/dist/css/bootstrap.css", "css/site.css");
});

var app = builder.Build();

await app.Services.SeedIdentityAsync(defaultAdmin);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRequestLocalization("en-US");

app.UseWebOptimizer();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
