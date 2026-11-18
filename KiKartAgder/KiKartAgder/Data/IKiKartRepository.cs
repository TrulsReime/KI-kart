using KiKartAgder.Models;

namespace KiKartAgder.Data;

/// <summary>
/// All lesing og skriving av data går gjennom dette grensesnittet. Det finnes to implementasjoner:
/// <see cref="MinneRepository"/> (JSON-fil) og <see cref="EfRepository"/> (PostgreSQL). Program.cs velger
/// mellom dem. Begge er forslag. DigiAgder eller IKT Agder kan lage sin egen implementasjon mot sin egen
/// database og bytte den inn i Program.cs, uten at kontrollerne eller visningene må endres.
/// </summary>
public interface IKiKartRepository
{
    /// <summary>Godkjente prosjekter som vises for alle.</summary>
    IReadOnlyList<Prosjekt> Publiserte();

    /// <summary>
    /// Publiserte prosjekter som ikke er inaktive. Det er disse som vises på oversikten, i kartet og i listene.
    /// Et prosjekt blir inaktivt når eieren ikke har bekreftet det på 12 måneder.
    /// </summary>
    IReadOnlyList<Prosjekt> Aktive();

    /// <summary>Alle prosjekter, også de som venter på godkjenning.</summary>
    IReadOnlyList<Prosjekt> Alle();

    Prosjekt? Finn(int id);

    /// <summary>Prosjektene DigiAgder har valgt ut til karusellen på forsiden. Inaktive prosjekter er filtrert bort.</summary>
    IReadOnlyList<Prosjekt> Utvalgte();

    /// <summary>Id-ene til alle utvalgte prosjekter i rekkefølgen i karusellen, også inaktive. Brukes i Forvaltning.</summary>
    IReadOnlyList<int> UtvalgteIder();

    /// <summary>
    /// Legger et publisert prosjekt sist i karusellen. Returnerer false hvis prosjektet ikke finnes,
    /// ikke er publisert, allerede er valgt ut, eller hvis det allerede er <see cref="Models.Katalog.MaksUtvalgte"/> utvalgte.
    /// </summary>
    bool VelgUt(int prosjektId);

    bool FjernUtvalgt(int prosjektId);

    /// <summary>Flytter et utvalgt prosjekt én plass opp (retning -1) eller ned (retning 1) i karusellen.</summary>
    bool FlyttUtvalgt(int prosjektId, int retning);

    IReadOnlyList<Innmelding> Innmeldinger();
    IReadOnlyList<Henvendelse> Henvendelser();

    /// <summary>Lagrer et nytt prosjekt som venter på godkjenning. Returnerer id-en.</summary>
    int MeldInnNytt(Prosjekt prosjekt);

    void MeldInnOppdatering(int prosjektId, Prosjekt nyeVerdier, string hvaErNytt, string fra);
    void ForeslaRettelse(int prosjektId, string tekst, string? fra);
    void BeOmABliMed(int prosjektId, string kommune, string navn, string melding);
    void SendHenvendelse(int prosjektId, string fra, string melding);

    bool Godkjenn(int innmeldingId);
    bool BeOmEndring(int innmeldingId, string kommentar);
    bool Avvis(int innmeldingId, string kommentar);
    void MarkerPaminnelseSendt(int prosjektId);

    /// <summary>Eieren bekrefter at alt stemmer. Datoen settes til i dag, og et inaktivt prosjekt blir aktivt igjen.</summary>
    bool Bekreft(int prosjektId);
}
