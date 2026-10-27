using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace KiKartAgder.Services;

/// <summary>
/// Forenklede kommunegrenser for Agder (2024) som SVG-stier. Lastes én gang fra Data/agder.json.
/// Filen bruker kommunenummer fra SSB som nøkkel, samme som tabellen Kommuner i databasen.
/// </summary>
public class KartData
{
    public double Bredde { get; }
    public double Hoyde { get; }
    public IReadOnlyDictionary<string, KommuneForm> Former { get; }

    /// <summary>Alle 25 kommunene i Agder, sortert alfabetisk på norsk (Å sist).</summary>
    public IReadOnlyList<string> Kommuner { get; }

    /// <summary>Kommunenummer for hver kommune, f.eks. «Birkenes» → «4216».</summary>
    public IReadOnlyDictionary<string, string> KommuneNr { get; }

    public KartData(IWebHostEnvironment env)
    {
        var sti = Path.Combine(env.ContentRootPath, "Data", "agder.json");
        var raw = JsonSerializer.Deserialize<RaKart>(File.ReadAllText(sti))
                  ?? throw new InvalidOperationException("Fant ikke kartdata");
        Bredde = raw.W;
        Hoyde = raw.H;
        Former = raw.K.Values.ToDictionary(f => f.N, f => new KommuneForm(f.D, f.C[0], f.C[1]));
        KommuneNr = raw.K.ToDictionary(kv => kv.Value.N, kv => kv.Key);
        var nb = StringComparer.Create(CultureInfo.GetCultureInfo("nb-NO"), false);
        Kommuner = Former.Keys.OrderBy(k => k, nb).ToList();
    }

    private class RaKart
    {
        [JsonPropertyName("w")] public double W { get; set; }
        [JsonPropertyName("h")] public double H { get; set; }
        /// <summary>Nøkkel = kommunenummer.</summary>
        [JsonPropertyName("k")] public Dictionary<string, RaForm> K { get; set; } = new();
    }

    private class RaForm
    {
        [JsonPropertyName("n")] public string N { get; set; } = "";
        [JsonPropertyName("d")] public string D { get; set; } = "";
        [JsonPropertyName("c")] public double[] C { get; set; } = new double[2];
    }
}

/// <summary>SVG-sti for én kommune, og midtpunktet der tallet skrives.</summary>
public record KommuneForm(string Sti, double X, double Y);
