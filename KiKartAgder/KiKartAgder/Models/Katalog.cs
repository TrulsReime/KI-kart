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

    /// <summary>IKT-samarbeidene i Agder som et prosjekt kan være en del av. Valgfritt ved innmelding.</summary>
    public static readonly string[] IktSamarbeid = { "IKT Agder", "DDV", "Digi Agder", "Kristiansand", "Lillesand og Birkenes" };

    public static readonly Dictionary<string, string> IktSamarbeidHjelp = new()
    {
        ["IKT Agder"] = "Interkommunalt IKT-selskap",
        ["DDV"] = "Det Digitale Vestre Agder",
        ["Digi Agder"] = "Digitaliseringssamarbeidet i Agder",
        ["Kristiansand"] = "Kristiansand kommunes egen IKT-tjeneste",
        ["Lillesand og Birkenes"] = "Felles IT-tjeneste for Lillesand og Birkenes",
    };

    public static readonly string[] Organiseringer = { "Samarbeid", "Enkeltkommune" };

    /// <summary>Kan velges som ansvarlig og deltaker, men har ingen egen flate i kartet.</summary>
    public const string Fylkeskommune = "Agder fylkeskommune";

    /// <summary>Navnet som vises som ansvarlig, f.eks. "Arendal kommune".</summary>
    public static string EierNavn(string kommune) => kommune == Fylkeskommune ? kommune : kommune + " kommune";

    private static readonly string[] AlleKommuner =
    {
        "Arendal", "Birkenes", "Bygland", "Bykle", "Evje og Hornnes", "Farsund", "Flekkefjord", "Froland",
        "Gjerstad", "Grimstad", "Hægebostad", "Iveland", "Kristiansand", "Kvinesdal", "Lillesand", "Lindesnes",
        "Lyngdal", "Risør", "Sirdal", "Tvedestrand", "Valle", "Vegårshei", "Vennesla", "Åmli", "Åseral"
    };

    /// <summary>
    /// Hvem som er med i hvert IKT-samarbeid. Velges automatisk i skjemaet.
    /// Kilder (november 2026): ikt-agder.no/Om-oss og ddv.no/om-oss/om-ddv-iks.
    /// Digi Agder er satt til alle kommunene og fylkeskommunen. Det bør bekreftes av DigiAgder.
    /// </summary>
    /// <summary>
    /// IKT-samarbeid som kan velges som ansvarlig for et prosjekt. Kristiansand er utelatt,
    /// fordi det er kommunens egen IKT-tjeneste og velges som «Kristiansand kommune».
    /// </summary>
    public static IEnumerable<string> AnsvarligeSamarbeid => IktSamarbeid.Where(s => IktSamarbeidMedlemmer[s].Length > 1);

    /// <summary>Sant når et IKT-samarbeid står som ansvarlig (eier) for prosjektet.</summary>
    public static bool EiesAvSamarbeid(Prosjekt p) => AnsvarligeSamarbeid.Contains(p.Eier);

    /// <summary>Kort navn på den ansvarlige: kommunen, fylkeskommunen eller IKT-samarbeidet.</summary>
    public static string AnsvarligNavn(Prosjekt p) => EiesAvSamarbeid(p) ? p.Eier : p.Kommune;

    public static readonly Dictionary<string, string[]> IktSamarbeidMedlemmer = new()
    {
        ["IKT Agder"] = new[] { "Arendal", "Bygland", "Bykle", "Evje og Hornnes", "Froland", "Gjerstad", "Grimstad",
                                "Iveland", "Risør", "Tvedestrand", "Valle", "Vegårshei", "Vennesla", "Åmli", Fylkeskommune },
        ["DDV"] = new[] { "Farsund", "Flekkefjord", "Hægebostad", "Kvinesdal", "Lindesnes", "Lyngdal", "Åseral" },
        ["Digi Agder"] = AlleKommuner.Append(Fylkeskommune).ToArray(),
        ["Kristiansand"] = new[] { "Kristiansand" },
        ["Lillesand og Birkenes"] = new[] { "Lillesand", "Birkenes" },
    };
}
