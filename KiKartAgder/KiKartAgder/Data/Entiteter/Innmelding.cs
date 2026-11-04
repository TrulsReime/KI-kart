using KiKartAgder.Models;

namespace KiKartAgder.Data.Entiteter;

/// <summary>Noe som må behandles av Digi Agder før det vises i kartet.</summary>
public class Innmelding
{
    public int InnmeldingId { get; set; }
    public InnmeldingType Type { get; set; }

    public int ProsjektId { get; set; }
    public Prosjekt Prosjekt { get; set; } = null!;

    public DateOnly Dato { get; set; }
    public string? FraNavn { get; set; }
    public string? FraEpost { get; set; }
    public string? Tekst { get; set; }

    /// <summary>Kommunen som vil bli med (bare ved BliMed).</summary>
    public string? KommuneNr { get; set; }
    public Kommune? Kommune { get; set; }

    /// <summary>Hele forslaget som JSON (bare ved Oppdatering).</summary>
    public string? NyeVerdier { get; set; }

    public InnmeldingStatus Status { get; set; } = InnmeldingStatus.Ny;
    public string? Kommentar { get; set; }
    public DateOnly? BehandletDato { get; set; }

    /// <summary>Id i AspNetUsers. Kobles til Identity når innlogging legges inn.</summary>
    public string? BehandletAvId { get; set; }
}
