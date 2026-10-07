using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace KiKartAgder.Services;

/// <summary>
/// Forenklede kommunegrenser for Agder (2024) som SVG-stier. Lastes én gang fra Data/agder.json.
/// </summary>
public class KartData
{
    public double Bredde { get; }
    public double Hoyde { get; }
    public IReadOnlyDictionary<string, KommuneForm> Former { get; }

    /// <summary>Alle 25 kommunene i Agder, sortert alfabetisk på norsk (Å sist).</summary>
    public IReadOnlyList<string> Kommuner { get; }

    public KartData(IWebHostEnvironment env)
    {
        var sti = Path.Combine(env.ContentRootPath, "Data", "agder.json");
        var raw = JsonSerializer.Deserialize<RaKart>(File.ReadAllText(sti))
                  ?? throw new InvalidOperationException("Fant ikke kartdata");
        Bredde = raw.W;
        Hoyde = raw.H;
        Former = raw.K.ToDictionary(kv => kv.Key, kv => new KommuneForm(kv.Value.D, kv.Value.C[0], kv.Value.C[1]));
        var nb = StringComparer.Create(CultureInfo.GetCultureInfo("nb-NO"), false);
        Kommuner = Former.Keys.OrderBy(k => k, nb).ToList();
    }

    private class RaKart
    {
        [JsonPropertyName("w")] public double W { get; set; }
        [JsonPropertyName("h")] public double H { get; set; }
        [JsonPropertyName("k")] public Dictionary<string, RaForm> K { get; set; } = new();
    }

    private class RaForm
    {
        [JsonPropertyName("d")] public string D { get; set; } = "";
        [JsonPropertyName("c")] public double[] C { get; set; } = new double[2];
    }
}

/// <summary>SVG-sti for én kommune, og midtpunktet der tallet skrives.</summary>
public record KommuneForm(string Sti, double X, double Y);
