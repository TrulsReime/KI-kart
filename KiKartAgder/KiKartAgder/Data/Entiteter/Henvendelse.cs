namespace KiKartAgder.Data.Entiteter;

/// <summary>En henvendelse om samarbeid sendt fra en prosjektside.</summary>
public class Henvendelse
{
    public int HenvendelseId { get; set; }

    public int ProsjektId { get; set; }
    public Prosjekt Prosjekt { get; set; } = null!;

    public string FraNavn { get; set; } = "";
    public string FraEpost { get; set; } = "";
    public string Melding { get; set; } = "";
    public DateOnly Dato { get; set; }
}
