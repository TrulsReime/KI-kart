using KiKartAgder.Models;
using KiKartAgder.Services;
using Microsoft.AspNetCore.Http.Extensions;

namespace KiKartAgder.ViewModels;

/// <summary>
/// Filtrene på prosjektsiden. Alt ligger i adressefeltet (query string), slik at et filtrert utvalg
/// kan deles som lenke og tilbakeknappen i nettleseren virker.
/// </summary>
public class ProsjektFilter
{
    public string? Q { get; set; }
    public List<string> Kommune { get; set; } = new();
    public List<string> Sektor { get; set; } = new();
    public List<string> Fase { get; set; } = new();
    public List<string> Stikkord { get; set; } = new();
    public List<string> Org { get; set; } = new();

    /// <summary>opp (sist oppdatert), kommune eller tittel.</summary>
    public string Sort { get; set; } = "opp";

    /// <summary>Sortering av kommunelisten i filteret: alfa eller antall.</summary>
    public string Ksort { get; set; } = "alfa";

    public bool HarFilter => !string.IsNullOrWhiteSpace(Q) || Kommune.Count + Sektor.Count + Fase.Count + Stikkord.Count + Org.Count > 0;

    public bool Passer(Prosjekt p) =>
        (Kommune.Count == 0 || p.Deltakere.Any(Kommune.Contains))
        && (Sektor.Count == 0 || Sektor.Contains(p.Sektor))
        && (Fase.Count == 0 || Fase.Contains(p.Fase))
        && (Org.Count == 0 || Org.Contains(p.Organisering))
        && (Stikkord.Count == 0 || Stikkord.All(p.Stikkord.Contains))
        && ProsjektLogikk.Treffer(p, Q);

    public IEnumerable<Prosjekt> Sorter(IEnumerable<Prosjekt> liste)
    {
        var nb = StringComparer.Create(System.Globalization.CultureInfo.GetCultureInfo("nb-NO"), false);
        return Sort switch
        {
            "kommune" => liste.OrderBy(p => p.Kommune, nb),
            "tittel" => liste.OrderBy(p => p.Tittel, nb),
            _ => liste.OrderByDescending(p => p.Oppdatert)
        };
    }

    /// <summary>Lenke til prosjektsiden med de samme filtrene, eventuelt uten én verdi.</summary>
    public string Url(string? fjernNokkel = null, string? fjernVerdi = null)
    {
        var qb = new QueryBuilder();
        if (!string.IsNullOrWhiteSpace(Q) && fjernNokkel != "q") qb.Add("q", Q);
        void Legg(string navn, List<string> verdier)
        {
            foreach (var v in verdier)
                if (!(fjernNokkel == navn && fjernVerdi == v)) qb.Add(navn, v);
        }
        Legg("org", Org); Legg("kommune", Kommune); Legg("sektor", Sektor); Legg("fase", Fase); Legg("stikkord", Stikkord);
        if (Sort != "opp") qb.Add("sort", Sort);
        if (Ksort != "alfa") qb.Add("ksort", Ksort);
        return "/Prosjekter" + qb.ToQueryString();
    }

    /// <summary>Alle valgte filterverdier som «chips» med lenke som fjerner dem.</summary>
    public IEnumerable<(string Etikett, string FjernUrl)> Valgte()
    {
        foreach (var (navn, liste) in new[] { ("org", Org), ("kommune", Kommune), ("sektor", Sektor), ("fase", Fase), ("stikkord", Stikkord) })
            foreach (var v in liste)
                yield return (navn == "stikkord" ? "#" + v : v, Url(navn, v));
    }
}

public record FilterValg(string Verdi, string Etikett, int Antall, bool Valgt);

public record FilterGruppe(string Tittel, string Navn, List<FilterValg> Valg, bool Apen);

public class ProsjekterViewModel
{
    public required ProsjektFilter Filter { get; init; }
    public required IReadOnlyList<Prosjekt> Resultat { get; init; }
    public required List<FilterGruppe> Grupper { get; init; }
}
