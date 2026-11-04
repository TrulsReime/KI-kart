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

    /// <summary>
    /// Den som meldte inn prosjektet. Kan oppdatere og bekrefte prosjektet, og får påminnelser.
    /// Tom = kontaktpersonen er eier.
    /// </summary>
    public int? ProsjekteierId { get; set; }
    public Kontaktperson? Prosjekteier { get; set; }

    public int SektorId { get; set; }
    public Sektor Sektor { get; set; } = null!;

    public int FaseId { get; set; }
    public Fase Fase { get; set; } = null!;

    /// <summary>Brukes bare når fasen er «Avbrutt».</summary>
    public int? StopparsakId { get; set; }
    public Stopparsak? Stopparsak { get; set; }

    /// <summary>Fritekst under «Annet» når prosjektet søker noe som ikke står i kodelisten Behov.</summary>
    public string? SokerAnnet { get; set; }

    /// <summary>Fritekst under «Annet» når prosjektet tilbyr noe som ikke står i kodelisten Behov.</summary>
    public string? TilbyrAnnet { get; set; }

    public bool ApenForFlere { get; set; }
    public Publiseringsstatus Publiseringsstatus { get; set; } = Publiseringsstatus.TilGodkjenning;

    /// <summary>Plass i karusellen på forsiden. Tom = ikke med.</summary>
    public int? UtvalgtRekkefolge { get; set; }

    public DateOnly Registrert { get; set; }
    public DateOnly Oppdatert { get; set; }
    public string SistEndret { get; set; } = "Lagt inn i kartet";
    public DateOnly? PaminnelseSendt { get; set; }

    /// <summary>
    /// Hindrer at to forvaltere overskriver hverandres endringer.
    /// I PostgreSQL er dette systemkolonnen xmin, som endres automatisk ved hver oppdatering.
    /// </summary>
    public uint RowVersion { get; set; }

    public List<ProsjektDeltaker> Deltakere { get; set; } = new();
    public List<ProsjektStikkord> Stikkord { get; set; } = new();
    public List<ProsjektBehov> Behov { get; set; } = new();
    public List<ProsjektIktSamarbeid> IktSamarbeid { get; set; } = new();
    public List<ProsjektBilde> Bilder { get; set; } = new();
    public List<LoggOppforing> Logg { get; set; } = new();
    public List<Innmelding> Innmeldinger { get; set; } = new();
    public List<Henvendelse> Henvendelser { get; set; } = new();
}
