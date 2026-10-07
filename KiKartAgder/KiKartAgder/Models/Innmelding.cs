namespace KiKartAgder.Models;

public enum InnmeldingType
{
    NyttProsjekt,
    Oppdatering,
    Rettelse,
    BliMed
}

public enum InnmeldingStatus
{
    Ny,
    VenterPaInnsender,
    Godkjent,
    Avvist
}

/// <summary>
/// Alt som må behandles av DigiAgder før det vises i kartet:
/// nye prosjekter, oppdateringer fra eier, rettelser og forespørsler om å bli med.
/// </summary>
public class Innmelding
{
    public int Id { get; set; }
    public InnmeldingType Type { get; set; }
    public int ProsjektId { get; set; }
    public DateOnly Dato { get; set; }

    /// <summary>Hvem som sendte inn (navn, e-post eller «Anonym»).</summary>
    public string Fra { get; set; } = "";

    /// <summary>Fritekst: rettelsen, meldingen eller hva som er nytt.</summary>
    public string? Tekst { get; set; }

    /// <summary>Kommunen som vil bli med (bare for <see cref="InnmeldingType.BliMed"/>).</summary>
    public string? Kommune { get; set; }

    /// <summary>De nye verdiene (bare for <see cref="InnmeldingType.Oppdatering"/>).</summary>
    public Prosjekt? NyeVerdier { get; set; }

    public InnmeldingStatus Status { get; set; } = InnmeldingStatus.Ny;
    public string? Kommentar { get; set; }
    public DateOnly? BehandletDato { get; set; }

    public string TypeTekst => Type switch
    {
        InnmeldingType.NyttProsjekt => "Nytt prosjekt",
        InnmeldingType.Oppdatering => "Oppdatering",
        InnmeldingType.Rettelse => "Rettelse",
        InnmeldingType.BliMed => "Ønsker å bli med",
        _ => ""
    };

    public string StatusTekst => Status switch
    {
        InnmeldingStatus.Ny => "Ny",
        InnmeldingStatus.VenterPaInnsender => "Venter på innsender",
        InnmeldingStatus.Godkjent => "Godkjent",
        InnmeldingStatus.Avvist => "Avvist",
        _ => ""
    };

    public string StatusKlasse => Status switch
    {
        InnmeldingStatus.Ny => "q-new",
        InnmeldingStatus.VenterPaInnsender => "q-warn",
        InnmeldingStatus.Godkjent => "q-ok",
        InnmeldingStatus.Avvist => "q-stop",
        _ => ""
    };

    public bool KanBehandles => Status is InnmeldingStatus.Ny or InnmeldingStatus.VenterPaInnsender;
}

/// <summary>En henvendelse om samarbeid sendt fra en prosjektside.</summary>
public record Henvendelse(int ProsjektId, string Fra, string Melding, DateOnly Dato);
