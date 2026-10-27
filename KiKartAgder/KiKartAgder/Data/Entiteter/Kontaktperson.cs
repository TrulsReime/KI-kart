namespace KiKartAgder.Data.Entiteter;

public class Kontaktperson
{
    public int KontaktpersonId { get; set; }
    public string Navn { get; set; } = "";
    public string Rolle { get; set; } = "";
    public string Epost { get; set; } = "";

    public int OrganisasjonId { get; set; }
    public Organisasjon Organisasjon { get; set; } = null!;

    /// <summary>Navn og e-post vises bare i kartet når denne er sann.</summary>
    public bool SamtykkePublisering { get; set; }
    public DateOnly? SamtykkeDato { get; set; }

    public List<Prosjekt> Prosjekter { get; set; } = new();
    public List<Prosjekt> EidProsjekter { get; set; } = new();
}
