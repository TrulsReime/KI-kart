namespace KiKartAgder.Data.Entiteter;

/// <summary>Ett KI-prosjekt slik det lagres i databasen. Visningene bruker fortsatt Models.Prosjekt.</summary>
public class Prosjekt
{
    public int ProsjektId { get; set; }
    public string Tittel { get; set; } = "";

    public string? Beskrivelse { get; set; }
    public string? Mal { get; set; }
    public string? StatusNaa { get; set; }
    public string? NesteSteg { get; set; }
    public string? Teknologi { get; set; }
    public string? Gevinst { get; set; }
    public string? Fungerte { get; set; }
    public string? Annerledes { get; set; }

    public int EierOrganisasjonId { get; set; }
    public Organisasjon EierOrganisasjon { get; set; } = null!;

    /// <summary>Kommunen prosjektet plasseres i på kartet.</summary>
    public string KartKommuneNr { get; set; } = "";
    public Kommune KartKommune { get; set; } = null!;

    public int KontaktpersonId { get; set; }
    public Kontaktperson Kontaktperson { get; set; } = null!;

    public int SektorId { get; set; }
    public Sektor Sektor { get; set; } = null!;

    public int FaseId { get; set; }
    public Fase Fase { get; set; } = null!;

    /// <summary>Brukes bare når fasen er «Avbrutt».</summary>
    public int? StopparsakId { get; set; }
    public Stopparsak? Stopparsak { get; set; }

    public bool ApenForFlere { get; set; }
    public Publiseringsstatus Publiseringsstatus { get; set; } = Publiseringsstatus.TilGodkjenning;

    /// <summary>Plass i karusellen på forsiden. Tom = ikke med.</summary>
    public int? UtvalgtRekkefolge { get; set; }

    public DateOnly Registrert { get; set; }
    public DateOnly Oppdatert { get; set; }
    public string SistEndret { get; set; } = "Lagt inn i kartet";
    public DateOnly? PaminnelseSendt { get; set; }

    /// <summary>Hindrer at to forvaltere overskriver hverandres endringer.</summary>
    public byte[] RowVersion { get; set; } = null!;

    public List<ProsjektDeltaker> Deltakere { get; set; } = new();
    public List<ProsjektStikkord> Stikkord { get; set; } = new();
    public List<ProsjektBehov> Behov { get; set; } = new();
    public List<ProsjektBilde> Bilder { get; set; } = new();
    public List<LoggOppforing> Logg { get; set; } = new();
    public List<Innmelding> Innmeldinger { get; set; } = new();
    public List<Henvendelse> Henvendelser { get; set; } = new();
}
