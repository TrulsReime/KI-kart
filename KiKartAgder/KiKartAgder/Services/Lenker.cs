using Microsoft.AspNetCore.Http;

namespace KiKartAgder.Services;

/// <summary>
/// Lenker til prosjektsider som husker hvor brukeren kom fra, så «Tilbake»-knappen
/// går til samme liste med de samme filtrene.
/// </summary>
public static class Lenker
{
    public const string DetaljSti = "/Prosjekter/Detaljer";

    public static string Prosjekt(HttpContext ctx, int id, string? apne = null)
    {
        var tilbake = NaverendeTilbake(ctx);
        var url = $"{DetaljSti}/{id}?tilbake={Uri.EscapeDataString(tilbake)}";
        return apne == null ? url : url + "&apne=" + apne;
    }

    /// <summary>Står man allerede på en prosjektside, beholdes den opprinnelige tilbake-lenken.</summary>
    private static string NaverendeTilbake(HttpContext ctx)
    {
        var req = ctx.Request;
        if (req.Path.StartsWithSegments(DetaljSti, StringComparison.OrdinalIgnoreCase))
            return TryggTilbake(req.Query["tilbake"]);
        return req.Path + req.QueryString;
    }

    /// <summary>Godtar bare lokale adresser, så lenken ikke kan sende brukeren til en annen side.</summary>
    public static string TryggTilbake(string? url) =>
        !string.IsNullOrEmpty(url) && url.StartsWith('/') && !url.StartsWith("//") && !url.StartsWith("/\\")
            ? url : "/Prosjekter";

    public static string TilbakeTekst(string url)
    {
        var sti = url.Split('?')[0].TrimEnd('/').ToLowerInvariant();
        return sti switch
        {
            "" or "/home" or "/home/index" => "oversikten",
            "/erfaringer" => "erfaringene",
            "/forvaltning" => "forvaltning",
            _ when sti.StartsWith("/meldinn/takk") => "kvitteringen",
            "/meldinn" => "skjemaet",
            _ => "prosjektlisten"
        };
    }
}
