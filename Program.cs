using Microsoft.EntityFrameworkCore;
using ptc_IGH_Sys.Data;

var builder = WebApplication.CreateBuilder(args);

// ── DB
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Cookie Auth ONLY — no Identity needed
builder.Services.AddAuthentication(
        Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
    });

builder.Services.AddAuthorization();

// ── Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ── HttpClient for API calls
builder.Services.AddHttpClient();

// ── MVC + JSON options required by Kendo DataSourceRequest
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        // Preserve PascalCase property names — Kendo expects this
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

// ── Kendo UI for ASP.NET Core
// Requires NuGet: Telerik.UI.for.AspNet.Core (match your JS version 2018.2.516)
// Enables: [DataSourceRequest], .ToDataSourceResult(), @(Html.Kendo().*)
//builder.Services.AddKendo();

// ────────────────────────────────────────────────────────
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();           // ← must be before UseAuthentication
app.UseAuthentication();
app.UseAuthorization();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();