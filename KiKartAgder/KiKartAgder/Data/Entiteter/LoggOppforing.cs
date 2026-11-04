namespace KiKartAgder.Data.Entiteter;

public class LoggOppforing
{
    public int LoggId { get; set; }

    public int ProsjektId { get; set; }
    public Prosjekt Prosjekt { get; set; } = null!;

    public DateOnly Dato { get; set; }
    public string Tekst { get; set; } = "";
}
