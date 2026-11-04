using System.Globalization;
using KiKartAgder.Data;
using KiKartAgder.Services;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// MVC med automatisk sjekk av antiforgery-token på alle POST-skjema
builder.Services.AddControllersWithViews(o => o.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));

// Tjenester
builder.Services.AddSingleton<IDatoTjeneste, DatoTjeneste>();
builder.Services.AddSingleton<KartData>();

// Med ConnectionStrings:KiKart (User Secrets lokalt) brukes databasen. Én repository per forespørsel,
// samme levetid som KiKartDbContext. Uten brukes MinneRepository, som lagrer alt i én JSON-fil
// (singleton). Da virker sider som er satt opp uten database, fortsatt.
var kiKartDb = builder.Configuration.GetConnectionString("KiKart");
var brukDatabase = !string.IsNullOrWhiteSpace(kiKartDb);
if (brukDatabase)
{
    builder.Services.AddDbContext<KiKartDbContext>(o => o.UseNpgsql(kiKartDb));
    builder.Services.AddScoped<IKiKartRepository, EfRepository>();
}
else
{
    builder.Services.AddSingleton<IKiKartRepository, MinneRepository>();
}

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

// Lokalt med database: kjør nye migrasjoner og legg inn testdata fra prosjekter.json hvis databasen er tom.
app.Logger.LogInformation("Data lagres i {Lagring}", brukDatabase ? "databasen (EfRepository)" : "JSON-fil (MinneRepository)");
if (app.Environment.IsDevelopment() && brukDatabase)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<KiKartDbContext>();
    try
    {
        await db.Database.MigrateAsync();
        await Testdata.LeggInnAsync(db, app.Environment.ContentRootPath);

        // Kartet og databasen må ha samme kommunenummer og navn
        var iKartet = app.Services.GetRequiredService<KartData>().KommuneNr.Select(kv => (Navn: kv.Key, Nr: kv.Value));
        var iDatabasen = await db.Kommuner.Where(k => !k.ErFylkeskommune).Select(k => new { k.Navn, k.KommuneNr }).ToListAsync();
        var avvik = iKartet.Except(iDatabasen.Select(k => (k.Navn, Nr: k.KommuneNr)))
            .Concat(iDatabasen.Select(k => (k.Navn, Nr: k.KommuneNr)).Except(iKartet)).ToList();
        if (avvik.Count > 0)
            app.Logger.LogWarning("agder.json og Kommuner-tabellen er ulike: {Avvik}", string.Join(", ", avvik.Select(a => $"{a.Nr} {a.Navn}")));
    }
    catch (Exception ex) when (ex is Npgsql.NpgsqlException or InvalidOperationException { InnerException: Npgsql.NpgsqlException })
    {
        app.Logger.LogCritical(ex, "Fikk ikke kontakt med databasen. Er den startet? (docker compose up -d db)");
        throw;
    }
}

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
