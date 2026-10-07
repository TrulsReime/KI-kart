using System.ComponentModel.DataAnnotations;
using KiKartAgder.Models;

namespace KiKartAgder.ViewModels;

/// <summary>
/// Skjemaet for å melde inn eller oppdatere et prosjekt. Valideringen skjer på serveren,
/// og feilmeldingene vises ved feltene. JavaScript i nettleseren deler skjemaet i tre steg.
/// </summary>
public class MeldInnSkjema : IValidatableObject
{
    /// <summary>Satt når et eksisterende prosjekt oppdateres.</summary>
    public int? Id { get; set; }

    // Steg 1: Om prosjektet
    [Required(ErrorMessage = "Skriv navnet på prosjektet")]
    [StringLength(120, ErrorMessage = "Navnet kan ha maks 120 tegn")]
    [Display(Name = "Hva heter prosjektet?")]
    public string? Tittel { get; set; }

    [Required(ErrorMessage = "Beskriv kort hva løsningen gjør")]
    [StringLength(600, ErrorMessage = "Beskrivelsen kan ha maks 600 tegn")]
    [Display(Name = "Hva gjør løsningen?")]
    public string? Beskrivelse { get; set; }

    [Required(ErrorMessage = "Velg sektor")]
    public string? Sektor { get; set; }

    [Required(ErrorMessage = "Velg hvor langt dere har kommet")]
    public string? Fase { get; set; }

    [Display(Name = "Hvorfor ble prosjektet avbrutt?")]
    public string? Stopp { get; set; }

    /// <summary>Stikkord skilt med komma. JavaScript viser dem som merkelapper.</summary>
    [Display(Name = "Stikkord")]
    public string? Stikkord { get; set; }

    // Steg 2: Hvem står bak
    public string Organisering { get; set; } = "Enkeltkommune";

    [Required(ErrorMessage = "Skriv hvem som er ansvarlig")]
    [Display(Name = "Ansvarlig kommune eller virksomhet")]
    public string? Eier { get; set; }

    [Required(ErrorMessage = "Velg kommune for plassering i kartet")]
    [Display(Name = "Plassering i kartet")]
    public string? Kommune { get; set; }

    public List<string> Deltakere { get; set; } = new();
    public bool Apen { get; set; } = true;

    [Required(ErrorMessage = "Skriv navnet på kontaktpersonen")]
    [Display(Name = "Navn")]
    public string? KontaktNavn { get; set; }

    [Display(Name = "Rolle")]
    public string? KontaktRolle { get; set; }

    [Required(ErrorMessage = "Skriv e-postadressen til kontaktpersonen")]
    [EmailAddress(ErrorMessage = "Skriv en gyldig e-postadresse")]
    [Display(Name = "E-post")]
    public string? KontaktEpost { get; set; }

    // Steg 3: Del mer (valgfritt)
    public List<string> Soker { get; set; } = new();
    public List<string> Tilbyr { get; set; } = new();
    [Display(Name = "Mål")] public string? Mal { get; set; }
    [Display(Name = "Status nå")] public string? Status { get; set; }
    [Display(Name = "Gevinst, forventet eller oppnådd")] public string? Gevinst { get; set; }
    [Display(Name = "Dette fungerer eller fungerte")] public string? Fungerte { get; set; }
    [Display(Name = "Dette ville vi gjort annerledes")] public string? Annerledes { get; set; }
    [Display(Name = "Hva er nytt?")] public string? SistEndret { get; set; }

    public List<string> StikkordListe() =>
        (Stikkord ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(t => t.TrimStart('#').ToLowerInvariant().Replace(' ', '-'))
            .Where(t => t.Length > 0).Distinct().ToList();

    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (StikkordListe().Count == 0)
            yield return new ValidationResult("Legg til minst ett stikkord", new[] { nameof(Stikkord) });
        if (Sektor != null && !Katalog.Sektorer.Contains(Sektor))
            yield return new ValidationResult("Ukjent sektor", new[] { nameof(Sektor) });
        if (Fase != null && !Katalog.Faser.Contains(Fase))
            yield return new ValidationResult("Ukjent fase", new[] { nameof(Fase) });
        if (Organisering == "Samarbeid" && Deltakere.Distinct().Count() < 2)
            yield return new ValidationResult("Velg minst to kommuner når flere er med", new[] { nameof(Deltakere) });
    }

    /// <summary>Gjør skjemaet om til et prosjekt.</summary>
    public Prosjekt TilProsjekt()
    {
        var deltakere = Organisering == "Samarbeid"
            ? new[] { Kommune! }.Concat(Deltakere.Where(k => k != Kommune)).Distinct().ToList()
            : new List<string> { Kommune! };
        var harNytt = !string.IsNullOrWhiteSpace(SistEndret);
        return new Prosjekt
        {
            Id = Id ?? 0,
            Tittel = Tittel!.Trim(),
            Beskrivelse = Beskrivelse?.Trim(),
            Sektor = Sektor!,
            Fase = Fase!,
            Stopp = Fase == Katalog.Avbrutt ? Stopp : null,
            Stikkord = StikkordListe(),
            Eier = Eier!.Trim(),
            Kommune = Kommune!,
            Deltakere = deltakere,
            Apen = Organisering == "Samarbeid" && Apen,
            Kontakt = new Kontaktperson { Navn = KontaktNavn!.Trim(), Rolle = KontaktRolle?.Trim() ?? "", Epost = KontaktEpost!.Trim() },
            Soker = Soker, Tilbyr = Tilbyr,
            Mal = Mal, Status = Status, Gevinst = Gevinst, Fungerte = Fungerte, Annerledes = Annerledes,
            SistEndret = harNytt ? SistEndret!.Trim() : (Id.HasValue ? "Oppdatert" : "Lagt inn i kartet")
        };
    }

    /// <summary>Fyller skjemaet med verdiene til et eksisterende prosjekt.</summary>
    public static MeldInnSkjema Fra(Prosjekt p) => new()
    {
        Id = p.Id, Tittel = p.Tittel, Beskrivelse = p.Beskrivelse, Sektor = p.Sektor, Fase = p.Fase, Stopp = p.Stopp,
        Stikkord = string.Join(", ", p.Stikkord), Organisering = p.Organisering, Eier = p.Eier, Kommune = p.Kommune,
        Deltakere = p.Deltakere.ToList(), Apen = p.Apen, KontaktNavn = p.Kontakt.Navn, KontaktRolle = p.Kontakt.Rolle,
        KontaktEpost = p.Kontakt.Epost, Soker = p.Soker.ToList(), Tilbyr = p.Tilbyr.ToList(), Mal = p.Mal, Status = p.Status,
        Gevinst = p.Gevinst, Fungerte = p.Fungerte, Annerledes = p.Annerledes, SistEndret = ""
    };

    /// <summary>Hvilke felt som hører til hvert steg, så riktig steg kan vises når noe mangler.</summary>
    public static readonly string[][] FeltPerSteg =
    {
        new[] { nameof(Tittel), nameof(Beskrivelse), nameof(Sektor), nameof(Fase), nameof(Stopp), nameof(Stikkord) },
        new[] { nameof(Organisering), nameof(Eier), nameof(Kommune), nameof(Deltakere), nameof(KontaktNavn), nameof(KontaktRolle), nameof(KontaktEpost) },
    };
}

public class MeldInnViewModel
{
    public required MeldInnSkjema Skjema { get; init; }
    public Prosjekt? Redigerer { get; init; }
    public required IReadOnlyList<Prosjekt> Publiserte { get; init; }
    public required IReadOnlyList<string> Kommuner { get; init; }
    public required IReadOnlyList<string> PopulareStikkord { get; init; }

    /// <summary>Steget som vises først (1 til 3). Settes til steget med første feil.</summary>
    public int StartSteg { get; init; } = 1;
}

public class TakkViewModel
{
    public required Prosjekt Prosjekt { get; init; }
    public bool Oppdatering { get; init; }
}
