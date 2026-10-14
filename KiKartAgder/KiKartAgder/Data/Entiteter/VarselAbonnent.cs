namespace KiKartAgder.Data.Entiteter;

/// <summary>Noen som følger kartet og får e-post om nye prosjekter (knappen «Følg»).</summary>
public class VarselAbonnent
{
    public int AbonnentId { get; set; }
    public string Epost { get; set; } = "";
    public DateTime Opprettet { get; set; }

    /// <summary>Tom til e-posten er bekreftet.</summary>
    public DateTime? Bekreftet { get; set; }

    public string AvmeldingToken { get; set; } = "";
    public DateTime? Avmeldt { get; set; }
}
