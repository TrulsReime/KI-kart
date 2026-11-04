namespace KiKartAgder.Data.Entiteter;

/// <summary>
/// En kommune i Agder. Nøkkelen er kommunenummeret fra SSB, f.eks. «4204».
/// Agder fylkeskommune ligger også her (4200), fordi den kan være ansvarlig og deltaker, men har ingen flate i kartet.
/// </summary>
public class Kommune
{
    public string KommuneNr { get; set; } = "";
    public string Navn { get; set; } = "";
    public string Fylke { get; set; } = "";
    public bool ErAktiv { get; set; } = true;

    /// <summary>Sann bare for Agder fylkeskommune. Filtrer bort denne når kommuner skal telles.</summary>
    public bool ErFylkeskommune { get; set; }

    public List<Organisasjon> Organisasjoner { get; set; } = new();
    public List<ProsjektDeltaker> Deltakelser { get; set; } = new();
}
