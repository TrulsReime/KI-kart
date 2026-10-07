using KiKartAgder.Models;
using KiKartAgder.Services;

namespace KiKartAgder.ViewModels;

public class ProsjektDetaljViewModel
{
    public required Prosjekt Prosjekt { get; init; }
    public required IReadOnlyList<Prosjekt> Lignende { get; init; }
    public required IReadOnlyList<string> KommunerSomKanBliMed { get; init; }
    public required string TilbakeUrl { get; init; }
    public required string TilbakeTekst { get; init; }

    /// <summary>«samarbeid» eller «blimed» åpner det skjemaet med en gang.</summary>
    public string? Apne { get; init; }

    public (ProsjektLogikk.Livsstatus Status, string Tekst) Livssyklus { get; init; }
}

public class SamarbeidViewModel
{
    public int AntallKommunerISamarbeid { get; init; }
    public required IReadOnlyList<Prosjekt> Samarbeidsprosjekter { get; init; }
    public required IReadOnlyList<Prosjekt> SerEtterNoe { get; init; }
    public Prosjekt? Valgt { get; init; }
    public required IReadOnlyList<ProsjektLogikk.Partnertreff> Treff { get; init; }
}

public class ErfaringerViewModel
{
    public string? Q { get; init; }
    public string Status { get; init; } = "alle";
    public string? Sektor { get; init; }
    public string? Kommune { get; init; }
    public string? Dato { get; init; }

    public required IReadOnlyList<Prosjekt> Liste { get; init; }
    public required Dictionary<string, int> AntallPerStatus { get; init; }
    public required IReadOnlyList<string> Kommuner { get; init; }

    public bool HarFilter => !string.IsNullOrWhiteSpace(Q) || Status != "alle" || !string.IsNullOrEmpty(Sektor)
                             || !string.IsNullOrEmpty(Kommune) || !string.IsNullOrEmpty(Dato);

    public static readonly (string Verdi, string Etikett)[] Statuser =
        { ("alle", "Alle"), ("aktive", "Pågående og i drift"), ("avsluttet", "Fullført"), ("stoppet", "Avbrutt") };

    public static readonly (string Verdi, string Etikett)[] Datoer =
        { ("30", "Siste 30 dager"), ("180", "Siste 6 måneder"), ("365", "Siste 12 måneder"), ("old", "Eldre enn ett år") };
}
