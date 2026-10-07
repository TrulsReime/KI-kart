using System.Globalization;
using KiKartAgder.Data;
using KiKartAgder.Services;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// MVC med automatisk sjekk av antiforgery-token på alle POST-skjema
builder.Services.AddControllersWithViews(o => o.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));

// Tjenester. Repository er singleton fordi dataene ligger i minnet i prototypen.
builder.Services.AddSingleton<IDatoTjeneste, DatoTjeneste>();
builder.Services.AddSingleton<KartData>();
builder.Services.AddSingleton<IKiKartRepository, MinneRepository>();

// KI-kartet vises i en iframe på digiagder.no. Da må cookies tillates på tvers av nettsteder
// (krever HTTPS), og standardhodet som blokkerer iframe må slås av.
var iProduksjon = !builder.Environment.IsDevelopment();
builder.Services.AddAntiforgery(o =>
{
    o.SuppressXFrameOptionsHeader = true;
    if (iProduksjon)
    {
        o.Cookie.SameSite = SameSiteMode.None;
        o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    }
});
builder.Services.Configure<CookieTempDataProviderOptions>(o =>
{
    o.Cookie.IsEssential = true;
    if (iProduksjon)
    {
        o.Cookie.SameSite = SameSiteMode.None;
        o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    }
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Feil");
    app.UseHsts();
}

// Norsk språk og datoformat
var norsk = new CultureInfo("nb-NO");
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(norsk),
    SupportedCultures = new[] { norsk },
    SupportedUICultures = new[] { norsk }
});

// Bare sidene som står i KiKart:IframeVerter kan vise KI-kartet i en iframe
var verter = builder.Configuration["KiKart:IframeVerter"] ?? "'self'";
app.Use(async (ctx, next) =>
{
    ctx.Response.Headers["Content-Security-Policy"] = $"frame-ancestors {verter}";
    await next();
});

app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
