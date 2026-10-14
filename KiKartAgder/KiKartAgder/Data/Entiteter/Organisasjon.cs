namespace KiKartAgder.Data.Entiteter;

/// <summary>Eier av prosjekter, f.eks. «Arendal kommune» eller «Agder fylkeskommune».</summary>
public class Organisasjon
{
    public int OrganisasjonId { get; set; }
    public string Navn { get; set; } = "";
    public OrganisasjonType Type { get; set; }

    /// <summary>Tom for organisasjoner som ikke er en kommune, f.eks. fylkeskommunen.</summary>
    public string? KommuneNr { get; set; }
    public Kommune? Kommune { get; set; }

    public List<Kontaktperson> Kontaktpersoner { get; set; } = new();
    public List<Prosjekt> Prosjekter { get; set; } = new();
}
