namespace KiKartAgder.Models;

/// <summary>
/// Ett KI-prosjekt i kartet. Tilsvarer én rad i datasettet (i dag en JSON-fil, senere Power BI eller en database).
/// </summary>
public class Prosjekt
{
    public int Id { get; set; }
    public string Tittel { get; set; } = "";

    /// <summary>Kommunen eller virksomheten som har ansvaret, f.eks. «Arendal kommune».</summary>
    public string Eier { get; set; } = "";

    /// <summary>Kommunen prosjektet plasseres i på kartet.</summary>
    public string Kommune { get; set; } = "";

    /// <summary>Alle kommunene som er med. Første kommune leder samarbeidet.</summary>
    public List<string> Deltakere { get; set; } = new();

    public string Sektor { get; set; } = "";
    public string Fase { get; set; } = Katalog.Ide;

    /// <summary>Hvorfor prosjektet ble avbrutt. Brukes bare når fasen er «Avbrutt».</summary>
    public string? Stopp { get; set; }

    public List<string> Stikkord { get; set; } = new();

    public string? Mal { get; set; }
    public string? Beskrivelse { get; set; }
    public string? Status { get; set; }
    public string? NesteSteg { get; set; }
    public string? Teknologi { get; set; }
    public string? Gevinst { get; set; }

    public Kontaktperson Kontakt { get; set; } = new();

    public DateOnly Registrert { get; set; }
    public DateOnly Oppdatert { get; set; }

    /// <summary>Kort tekst om hva som ble endret sist, vises sammen med datoen.</summary>
    public string SistEndret { get; set; } = "Lagt inn i kartet";

    /// <summary>Det prosjektet ser etter (samarbeidspartner, data osv.).</summary>
    public List<string> Soker { get; set; } = new();

    /// <summary>Det prosjektet kan tilby andre.</summary>
    public List<string> Tilbyr { get; set; } = new();

    /// <summary>Om andre kommuner kan be om å bli med.</summary>
    public bool Apen { get; set; }

    public string? Fungerte { get; set; }
    public string? Annerledes { get; set; }

    /// <summary>Sant når prosjektet er meldt inn, men ikke godkjent av DigiAgder ennå.</summary>
    public bool TilGodkjenning { get; set; }

    public DateOnly? PaminnelseSendt { get; set; }

    public List<LoggOppforing> Logg { get; set; } = new();

    // ---------- avledede verdier ----------
    public bool Samarbeid => Deltakere.Count > 1;
    public bool ErAvsluttet => Fase is Katalog.Fullfort or Katalog.Avbrutt;
    public bool ErApenForFlere => Apen && !ErAvsluttet;
    public bool HarErfaringer => !string.IsNullOrWhiteSpace(Fungerte);
    public string Organisering => Samarbeid ? "Samarbeid" : "Enkeltkommune";
}

public class Kontaktperson
{
    public string Navn { get; set; } = "";
    public string Rolle { get; set; } = "";
    public string Epost { get; set; } = "";

    public string Fornavn => Navn.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? Navn;
}

public record LoggOppforing(DateOnly Dato, string Tekst);
