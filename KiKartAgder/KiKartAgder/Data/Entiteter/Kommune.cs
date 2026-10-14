namespace KiKartAgder.Data.Entiteter;

/// <summary>En kommune i Agder. Nøkkelen er kommunenummeret fra SSB, f.eks. «4204».</summary>
public class Kommune
{
    public string KommuneNr { get; set; } = "";
    public string Navn { get; set; } = "";
    public string Fylke { get; set; } = "";
    public bool ErAktiv { get; set; } = true;

    public List<Organisasjon> Organisasjoner { get; set; } = new();
    public List<ProsjektDeltaker> Deltakelser { get; set; } = new();
}
