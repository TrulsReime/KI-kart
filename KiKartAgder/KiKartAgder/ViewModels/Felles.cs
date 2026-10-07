using KiKartAgder.Models;
using KiKartAgder.Services;

namespace KiKartAgder.ViewModels;

/// <summary>Én stolpe i et stolpediagram (fase eller sektor).</summary>
public record Stolpe(string Navn, int Antall, string Url, double Prosent, bool Kritisk);

/// <summary>Data til kartet: antall egne prosjekter og samarbeid per kommune.</summary>
public class KartViewModel
{
    public required KartData Kart { get; init; }
    public required Dictionary<string, int> Egne { get; init; }
    public required Dictionary<string, int> ISamarbeid { get; init; }

    public int Totalt(string kommune) => Egne.GetValueOrDefault(kommune) + ISamarbeid.GetValueOrDefault(kommune);

    private int Maks => Math.Max(1, Kart.Kommuner.Max(Totalt));

    /// <summary>Fargetrinn 0 til 6 (CSS-variablene --seq-0 til --seq-6).</summary>
    public int Trinn(string kommune)
    {
        var n = Totalt(kommune);
        if (n == 0) return 0;
        return Math.Min(6, (int)Math.Ceiling(n / (double)Maks * 6));
    }

    public static KartViewModel Lag(KartData kart, IEnumerable<Prosjekt> prosjekter)
    {
        var egne = new Dictionary<string, int>();
        var sam = new Dictionary<string, int>();
        foreach (var p in prosjekter)
        foreach (var k in p.Deltakere)
        {
            var d = p.Samarbeid ? sam : egne;
            d[k] = d.GetValueOrDefault(k) + 1;
        }
        return new KartViewModel { Kart = kart, Egne = egne, ISamarbeid = sam };
    }
}

public static class Stolper
{
    /// <summary>Lager stolper for fase (fast rekkefølge) eller sektor (flest først).</summary>
    public static List<Stolpe> Lag(IEnumerable<Prosjekt> prosjekter, string felt, IEnumerable<string> kategorier)
    {
        var liste = prosjekter.ToList();
        var rader = kategorier.Select(k => (k, n: liste.Count(p => (felt == "fase" ? p.Fase : p.Sektor) == k))).ToList();
        if (felt != "fase") rader = rader.OrderByDescending(r => r.n).ToList();
        var maks = Math.Max(1, rader.Max(r => r.n));
        return rader.Select(r => new Stolpe(r.k, r.n, $"/Prosjekter?{felt}={Uri.EscapeDataString(r.k)}",
            r.n * 100.0 / maks, r.k == Katalog.Avbrutt)).ToList();
    }
}

/// <summary>Innstillinger for søkefeltet med forslag.</summary>
public record SokefeltModel(string Id, string Placeholder, string? Verdi = null);
