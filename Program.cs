/*
 * Program.cs — PTC IGH System
 * ════════════════════════════════════════════════════════════════
 * ALL crash-prevention settings included.
 * Works on .NET 6, 7, 8, 9, 10 — any future version.
 * ════════════════════════════════════════════════════════════════
 */

using Microsoft.EntityFrameworkCore;
using ptc_IGH_Sys.Data;


var builder = WebApplication.CreateBuilder(args);

/* ── DB ─────────────────────────────────────────────────────── */
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));


/* ── Second DB: SHReportPortal (for stored procedure) ───────── */
builder.Services.AddDbContext<SHReportDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("SHReportPortalConnection")));

/* ── Cookie Auth ─────────────────────────────────────────────── */
builder.Services.AddAuthentication(
    Microsoft.AspNetCore.Authentication.Cookies
        .CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy =
            Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
    });

builder.Services.AddAuthorization();

/* ── Session ─────────────────────────────────────────────────── */
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

/* ── HttpClient ──────────────────────────────────────────────── */
builder.Services.AddHttpClient();

/* ── MVC ─────────────────────────────────────────────────────── */
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        /*
         * CRASH PREVENTION #1 — PropertyNamingPolicy = null
         *
         * WHY: .NET default serializer converts to camelCase.
         *      Kendo JS grid expects PascalCase (Data, Total, Errors).
         *      Without this: grid shows empty data even though
         *      controller returns correct data.
         *
         * SYMPTOM WITHOUT IT:
         *   Grid shows 0 records even though DB has data.
         *   Browser console shows: { data: [...], total: 5 }
         *   But schema.data is 'Data' (capital D) — mismatch.
         */
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

/*
 * CRASH PREVENTION #2 — DO NOT ADD AddKendo()
 *
 * WHY: builder.Services.AddKendo() calls ClientValidatorCache
 *      which was removed in .NET Core 3+.
 *      Kendo 2019 NuGet was built for .NET Core 2.x internals.
 *
 * SYMPTOM:
 *   App crashes on startup with:
 *   "Could not load type 'ClientValidatorCache' from assembly
 *    Microsoft.AspNetCore.Mvc.Core, Version=8.0.0.0"
 *
 * RULE: Never add this unless using Kendo 2023+ with .NET 8
 */
// builder.Services.AddKendo();   ← NEVER uncomment for .NET 8 + Kendo 2019

/*
 * CRASH PREVENTION #3 — DO NOT USE [DataSourceRequest]
 *
 * WHY: DataSourceRequestModelBinder calls ModelBindingHelper
 *      which was removed in .NET Core 3+.
 *
 * SYMPTOM:
 *   Grid shows 500 error:
 *   "Could not load type 'ModelBindingHelper' from assembly
 *    Microsoft.AspNetCore.Mvc.Core, Version=8.0.0.0"
 *
 * FIX: Use [FromForm] KendoGridRequest instead.
 *      See Helpers/KendoGrid.cs
 */

// ────────────────────────────────────────────────────────────────
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();        /* ← MUST be before UseAuthentication */
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
