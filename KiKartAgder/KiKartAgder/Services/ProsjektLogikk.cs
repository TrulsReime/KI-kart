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

    // ---------- aktualitet ----------
    /// <summary>Etter så mange måneder uten oppdatering skifter fargen fra grønn til gul.</summary>
    public const int MndFersk = 6;
    /// <summary>Etter så mange måneder sendes en påminnelse til prosjekteieren.</summary>
    public const int MndPaminnelse = 11;
    /// <summary>Etter så mange måneder uten bekreftelse blir prosjektet inaktivt.</summary>
    public const int MndInaktiv = 12;

    public enum Aktualitet { Fersk, Eldre, Snart, Inaktiv, Arkiv }

    /// <summary>Hvor oppdatert et prosjekt er. Klasse er CSS-klassen som gir fargen.</summary>
    public record AktualitetInfo(Aktualitet Niva, string Tekst, string Klasse, DateOnly Frist);

    /// <summary>
    /// Prosjekter må bekreftes av eier minst hver 12. måned. Grønn de første 6 månedene, gul fram til
    /// påminnelsen ved 11 måneder, oransje den siste måneden og inaktiv etter fristen.
    /// Fullførte og avbrutte prosjekter arkiveres og blir aldri inaktive.
    /// </summary>
    public static AktualitetInfo Aktualitetsstatus(Prosjekt p, DateOnly idag)
    {
        var frist = p.Oppdatert.AddMonths(MndInaktiv);
        if (p.ErAvsluttet) return new(Aktualitet.Arkiv, "Avsluttet og arkivert med erfaringer", "akt-arkiv", frist);
        var igjen = frist.DayNumber - idag.DayNumber;
        if (igjen < 0) return new(Aktualitet.Inaktiv, $"Inaktiv siden {Datoformat.Lang(frist)}", "akt-inaktiv", frist);
        if (idag >= p.Oppdatert.AddMonths(MndPaminnelse))
            return new(Aktualitet.Snart, $"Må bekreftes innen {igjen} {(igjen == 1 ? "dag" : "dager")}", "akt-snart", frist);
        if (idag >= p.Oppdatert.AddMonths(MndFersk))
            return new(Aktualitet.Eldre, "Oppdatert for over et halvt år siden", "akt-eldre", frist);
        return new(Aktualitet.Fersk, "Oppdatert siste 6 måneder", "akt-fersk", frist);
    }

    public static bool ErInaktiv(Prosjekt p, DateOnly idag) => Aktualitetsstatus(p, idag).Niva == Aktualitet.Inaktiv;

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
