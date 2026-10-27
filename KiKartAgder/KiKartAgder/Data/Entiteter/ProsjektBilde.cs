namespace KiKartAgder.Data.Entiteter;

/// <summary>Bilde til et prosjekt. Selve filen ligger i blob-lagring, databasen har bare adressen.</summary>
public class ProsjektBilde
{
    public int BildeId { get; set; }

    public int ProsjektId { get; set; }
    public Prosjekt Prosjekt { get; set; } = null!;

    public string Url { get; set; } = "";
    public string AltTekst { get; set; } = "";
    public int Rekkefolge { get; set; }
    public bool ErForside { get; set; }
}
