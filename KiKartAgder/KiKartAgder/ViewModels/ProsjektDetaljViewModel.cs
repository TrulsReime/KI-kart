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

    public required ProsjektLogikk.AktualitetInfo Aktualitet { get; init; }
}
