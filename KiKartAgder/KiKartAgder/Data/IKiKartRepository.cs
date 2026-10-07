using KiKartAgder.Models;

namespace KiKartAgder.Data;

/// <summary>
/// All lesing og skriving av data går gjennom dette grensesnittet. I prototypen ligger alt i minnet
/// (<see cref="MinneRepository"/>). I en ferdig løsning byttes klassen ut med en som leser fra
/// Power BI-datasettet eller en database, uten at kontrollerne eller visningene må endres.
/// </summary>
public interface IKiKartRepository
{
    /// <summary>Godkjente prosjekter som vises for alle.</summary>
    IReadOnlyList<Prosjekt> Publiserte();

    /// <summary>Alle prosjekter, også de som venter på godkjenning.</summary>
    IReadOnlyList<Prosjekt> Alle();

    Prosjekt? Finn(int id);

    /// <summary>Prosjektene DigiAgder har valgt ut til karusellen på forsiden.</summary>
    IReadOnlyList<Prosjekt> Utvalgte();

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
}
