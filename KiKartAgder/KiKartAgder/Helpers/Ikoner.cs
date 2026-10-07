using Microsoft.AspNetCore.Html;

namespace KiKartAgder.Helpers;

/// <summary>Små SVG-ikoner som brukes i visningene.</summary>
public static class Ikoner
{
    private static readonly Dictionary<string, string> Stier = new()
    {
        ["eye"] = "M2 12s3.6-7 10-7 10 7 10 7-3.6 7-10 7S2 12 2 12Z|M12 9a3 3 0 1 0 0 6 3 3 0 0 0 0-6Z",
        ["people"] = "M16 19v-1.5a3.5 3.5 0 0 0-3.5-3.5h-5A3.5 3.5 0 0 0 4 17.5V19|M10 11a3.5 3.5 0 1 0 0-7 3.5 3.5 0 0 0 0 7Z|M20 19v-1.5a3.5 3.5 0 0 0-2.5-3.3|M15.5 4.2a3.5 3.5 0 0 1 0 6.6",
        ["bulb"] = "M9 18h6|M10 21h4|M12 3a6 6 0 0 0-3.5 10.9c.6.5 1 1.2 1 2.1h5c0-.9.4-1.6 1-2.1A6 6 0 0 0 12 3Z",
        ["clock"] = "M12 7v5l3 2|M12 21a9 9 0 1 0 0-18 9 9 0 0 0 0 18Z",
        ["check"] = "M5 12.5l4.5 4.5L19 7.5",
    };

    public static IHtmlContent Ikon(string navn, int storrelse = 22)
    {
        var stier = string.Concat(Stier[navn].Split('|').Select(d => $"<path d=\"{d}\"/>"));
        return new HtmlString($"<svg width=\"{storrelse}\" height=\"{storrelse}\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.8\" stroke-linecap=\"round\" stroke-linejoin=\"round\" aria-hidden=\"true\">{stier}</svg>");
    }

    public static IHtmlContent Sok { get; } = new HtmlString(
        "<svg width=\"16\" height=\"16\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2.4\" aria-hidden=\"true\"><circle cx=\"11\" cy=\"11\" r=\"7\"/><path d=\"m20 20-3.5-3.5\"/></svg>");
}
