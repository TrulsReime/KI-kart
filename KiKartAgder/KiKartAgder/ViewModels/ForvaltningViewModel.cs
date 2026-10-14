using KiKartAgder.Models;
using KiKartAgder.Services;

namespace KiKartAgder.ViewModels;

public record InnmeldingRad(Innmelding Innmelding, Prosjekt Prosjekt, IReadOnlyList<Prosjekt> MuligeDuplikater);

public record PaminnelseRad(Prosjekt Prosjekt, bool Inaktiv, ProsjektLogikk.AktualitetInfo Aktualitet, string Epost);

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

    /// <summary>Påminnelsen som går til prosjekteieren. Teksten er litt annerledes hvis prosjektet allerede er inaktivt.</summary>
    public static string EpostTekst(Prosjekt p, ProsjektLogikk.AktualitetInfo a, bool inaktiv)
    {
        var eier = p.Eierperson;
        var start = $"Til: {eier.Epost}\n";
        if (inaktiv)
            return start + $"Emne: Påminnelse: «{p.Tittel}» er ikke lenger synlig i KI-kartet\n\nHei {eier.Fornavn}!\n\n" +
                   $"Du er eier av prosjektet, og det er ikke bekreftet siden {Datoformat.Lang(p.Oppdatert)}. " +
                   $"Derfor ble det inaktivt {Datoformat.Lang(a.Frist)} og vises ikke lenger på oversikten eller i kartet. " +
                   "Bekreft at informasjonen stemmer, eller oppdater den, så blir prosjektet synlig igjen med en gang. " +
                   "Er prosjektet avsluttet, kan du sette fasen til Fullført eller Avbrutt.\n\n" +
                   "[ Alt stemmer ]   [ Oppdater prosjektet ]\n\nHilsen DigiAgder";
        return start + $"Emne: Påminnelse: Er «{p.Tittel}» fortsatt oppdatert?\n\nHei {eier.Fornavn}!\n\n" +
               $"Du er eier av prosjektet i KI-kartet. Det ble sist oppdatert {Datoformat.Lang(p.Oppdatert)} («{p.SistEndret}»). " +
               $"Bekreft at informasjonen stemmer, eller oppdater den, innen {Datoformat.Lang(a.Frist)}. " +
               "Ellers blir prosjektet inaktivt og skjules fra oversikten.\n\n" +
               "[ Alt stemmer ]   [ Oppdater prosjektet ]\n\nHilsen DigiAgder";
    }
}
