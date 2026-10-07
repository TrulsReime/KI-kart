using KiKartAgder.Models;

namespace KiKartAgder.Services;

/// <summary>Regler som brukes flere steder: søk, livssyklus, partnerforslag og lignende prosjekter.</summary>
public static class ProsjektLogikk
{
    // ---------- søk ----------
    private static string Hoystakk(Prosjekt p) => string.Join(' ', new[]
    {
        p.Tittel, p.Eier, p.Beskrivelse, p.Mal, p.Status, p.Teknologi, p.Gevinst, p.Sektor, p.Fase, p.Stopp,
        p.Kontakt.Navn, p.Fungerte, p.Annerledes, p.Samarbeid ? "samarbeid" : "enkeltkommune",
        p.Fase == Katalog.Avbrutt ? "stoppet stoppede" : null
    }.Concat(p.Deltakere).Concat(p.Stikkord).Where(s => !string.IsNullOrEmpty(s))).ToLowerInvariant();

    /// <summary>Sant hvis alle ordene i søket finnes et sted i prosjektet.</summary>
    public static bool Treffer(Prosjekt p, string? sok)
    {
        var q = (sok ?? "").Trim().ToLowerInvariant().TrimStart('#');
        if (q.Length == 0) return true;
        var h = Hoystakk(p);
        return q.Split(' ', StringSplitOptions.RemoveEmptyEntries).All(h.Contains);
    }

    // ---------- livssyklus ----------
    public enum Livsstatus { Ok, Snart, Utlopt, Arkiv }

    /// <summary>Prosjekter må bekreftes av eier minst hver 12. måned.</summary>
    public static (Livsstatus Status, string Tekst) Livssyklus(Prosjekt p, DateOnly idag)
    {
        if (p.ErAvsluttet) return (Livsstatus.Arkiv, "Arkivert med erfaringer");
        var utloper = p.Oppdatert.AddMonths(12);
        var igjen = utloper.DayNumber - idag.DayNumber;
        if (igjen < 0) return (Livsstatus.Utlopt, "Ikke bekreftet på over 12 måneder");
        if (igjen <= 45) return (Livsstatus.Snart, $"Må bekreftes innen {igjen} dager");
        return (Livsstatus.Ok, $"Gyldig til {Datoformat.Lang(utloper)}");
    }

    // ---------- partnerforslag ----------
    public record Partnertreff(Prosjekt Prosjekt, int Poeng, List<string> Grunner);

    /// <summary>Finner prosjekter som kan passe som partner for <paramref name="a"/>.</summary>
    public static List<Partnertreff> Partnere(Prosjekt a, IEnumerable<Prosjekt> alle)
    {
        return alle.Where(b => b.Id != a.Id).Select(b =>
        {
            var gi = a.Soker.Where(b.Tilbyr.Contains).ToList();
            var fa = b.Soker.Where(a.Tilbyr.Contains).ToList();
            var felles = a.Stikkord.Where(b.Stikkord.Contains).ToList();
            var grunner = new List<string>();
            var s = 0;
            if (gi.Count > 0) { s += gi.Count * 3; grunner.Add("Kan tilby: " + string.Join(", ", gi).ToLowerInvariant()); }
            if (felles.Count > 0) { s += felles.Count * 2; grunner.Add("Felles stikkord: " + string.Join(" ", felles.Select(t => "#" + t))); }
            if (fa.Count > 0) { s += fa.Count * 2; grunner.Add("Ser etter det dere kan tilby: " + string.Join(", ", fa).ToLowerInvariant()); }
            if (b.Sektor == a.Sektor) { s += 1; grunner.Add("Samme sektor"); }
            if (b.ErApenForFlere && b.Samarbeid) { s += 1; grunner.Add("Åpent for flere kommuner"); }
            if (b.Fase == Katalog.Avbrutt && felles.Count > 0) grunner.Add("Avbrutt: se hvorfor før dere starter");
            return new Partnertreff(b, s, grunner);
        }).Where(t => t.Poeng >= 3).OrderByDescending(t => t.Poeng).Take(5).ToList();
    }

    /// <summary>Opptil fire prosjekter med felles stikkord eller samme sektor.</summary>
    public static List<Prosjekt> Lignende(Prosjekt p, IEnumerable<Prosjekt> alle) =>
        alle.Where(b => b.Id != p.Id)
            .Select(b => (b, s: b.Stikkord.Count(p.Stikkord.Contains) * 2 + (b.Sektor == p.Sektor ? 1 : 0)))
            .Where(x => x.s >= 2).OrderByDescending(x => x.s).Take(4).Select(x => x.b).ToList();

    /// <summary>Mulige duplikater av et nytt prosjekt: samme kommune og minst to felles stikkord.</summary>
    public static List<Prosjekt> MuligeDuplikater(Prosjekt p, IEnumerable<Prosjekt> publiserte) =>
        publiserte.Where(b => b.Id != p.Id && b.Deltakere.Contains(p.Kommune) && b.Stikkord.Count(p.Stikkord.Contains) >= 2).ToList();

    // ---------- små hjelpere ----------
    /// <summary>«Evje og Hornnes» blir «EH», «Kristiansand» blir «KR».</summary>
    public static string Initialer(string kommune)
    {
        var ord = kommune.Split(' ', StringSplitOptions.RemoveEmptyEntries).Where(w => w.Length > 2).ToList();
        var s = ord.Count > 1 ? $"{ord[0][0]}{ord[^1][0]}" : kommune[..Math.Min(2, kommune.Length)];
        return s.ToUpperInvariant();
    }

    /// <summary>Alle stikkord med antall, mest brukte først.</summary>
    public static List<(string Stikkord, int Antall)> Stikkord(IEnumerable<Prosjekt> prosjekter) =>
        prosjekter.SelectMany(p => p.Stikkord).GroupBy(t => t)
            .Select(g => (g.Key, g.Count())).OrderByDescending(x => x.Item2).ThenBy(x => x.Key, StringComparer.Ordinal).ToList();

    public static string Deltakerliste(Prosjekt p, int maks = 3) =>
        p.Deltakere.Count <= maks ? string.Join(", ", p.Deltakere)
            : string.Join(", ", p.Deltakere.Take(maks)) + $" og {p.Deltakere.Count - maks} til";
}
