namespace KiKartAgder.Models;

/// <summary>Faste verdier (kodelister) som brukes i hele løsningen.</summary>
public static class Katalog
{
    public const string Ide = "Idé";
    public const string Pilot = "Pilot";
    public const string Drift = "Drift";
    public const string Fullfort = "Fullført";
    public const string Avbrutt = "Avbrutt";

    public static readonly string[] Faser = { Ide, Pilot, Drift, Fullfort, Avbrutt };

    public static readonly Dictionary<string, string> FaseHjelp = new()
    {
        [Ide] = "Dere har en idé eller utreder",
        [Pilot] = "Dere tester i liten skala",
        [Drift] = "Løsningen er tatt i bruk",
        [Fullfort] = "Målet ble nådd",
        [Avbrutt] = "Avsluttet før målet var nådd",
    };

    /// <summary>CSS-klasse for fargeprikken til hver fase.</summary>
    public static string FaseKlasse(string fase) => fase switch
    {
        Ide => "f-ide",
        Pilot => "f-pilot",
        Drift => "f-drift",
        Fullfort => "f-avsl",
        Avbrutt => "f-stopp",
        _ => ""
    };

    public static readonly string[] Sektorer =
    {
        "Oppvekst", "E-helse", "Sosial og velferd", "Teknisk",
        "Administrativt og tverrfaglig", "Teknologi og infrastruktur"
    };

    public static readonly string[] StoppArsaker =
    {
        "Personvern og juss", "Kostnad større enn nytte", "Teknologien holdt ikke mål",
        "Leverandør eller avtale", "Behovet var ikke der"
    };

    public static readonly string[] Behov =
    {
        "Samarbeidspartner", "Pilotkommune", "Kompetanse", "Data",
        "Erfaringer fra andre", "Personvernvurdering", "Finansiering"
    };

    public static readonly string[] Organiseringer = { "Samarbeid", "Enkeltkommune" };
}
