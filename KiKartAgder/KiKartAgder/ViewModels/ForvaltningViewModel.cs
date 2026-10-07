using KiKartAgder.Models;
using KiKartAgder.Services;

namespace KiKartAgder.ViewModels;

public record InnmeldingRad(Innmelding Innmelding, Prosjekt Prosjekt, IReadOnlyList<Prosjekt> MuligeDuplikater);

public record PaminnelseRad(Prosjekt Prosjekt, bool Utlopt, string Epost);

public record HenvendelseRad(Henvendelse Henvendelse, string ProsjektTittel);

public class ForvaltningViewModel
{
    public required string Fane { get; init; }
    public required IReadOnlyList<InnmeldingRad> Rader { get; init; }
    public required IReadOnlyList<PaminnelseRad> Paminnelser { get; init; }
    public required IReadOnlyList<HenvendelseRad> Henvendelser { get; init; }

    public int AntallNye { get; init; }
    public int AntallVenter { get; init; }
    public int AntallGodkjent { get; init; }
    public int AntallBehandlet { get; init; }

    /// <summary>Raden som skal være åpen (f.eks. etter en feil i skjemaet).</summary>
    public int? ApenId { get; init; }

    public static readonly (string Verdi, string Etikett)[] Faner =
        { ("ny", "Til behandling"), ("venter", "Venter på innsender"), ("ferdig", "Behandlet"), ("pam", "Påminnelser") };

    public int Antall(string fane) => fane switch
    {
        "ny" => AntallNye,
        "venter" => AntallVenter,
        "ferdig" => AntallBehandlet,
        "pam" => Paminnelser.Count,
        _ => 0
    };

    public static string EpostTekst(Prosjekt p) =>
        $"Til: {p.Kontakt.Epost}\nEmne: Er «{p.Tittel}» fortsatt oppdatert?\n\nHei {p.Kontakt.Fornavn}!\n\n" +
        $"Prosjektet ditt i KI-kartet ble sist oppdatert {Datoformat.Lang(p.Oppdatert)} («{p.SistEndret}»). " +
        "Bekreft at informasjonen stemmer, eller oppdater den. Datoen i kartet oppdateres når du svarer.\n\n" +
        "[ Alt stemmer ]   [ Oppdater prosjektet ]\n\nHilsen DigiAgder";
}
