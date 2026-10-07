using KiKartAgder.Models;

namespace KiKartAgder.ViewModels;

public class OversiktViewModel
{
    public DateOnly Idag { get; init; }
    public int AntallProsjekter { get; init; }
    public int AntallIDrift { get; init; }
    public int AntallKommuner { get; init; }
    public int AntallSamarbeid { get; init; }
    public int AntallApneSamarbeid { get; init; }
    public int AntallNye { get; init; }

    public required IReadOnlyList<Prosjekt> Utvalgte { get; init; }
    public required IReadOnlyList<Prosjekt> Nyeste { get; init; }
    public required IReadOnlyList<Prosjekt> SistOppdatert { get; init; }
    public required KartViewModel Kart { get; init; }
    public required List<Stolpe> Faser { get; init; }
    public required List<Stolpe> Sektorer { get; init; }
}
