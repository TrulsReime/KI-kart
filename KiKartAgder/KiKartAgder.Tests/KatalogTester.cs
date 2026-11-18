using KiKartAgder.Models;
using KiKartAgder.Services;

namespace KiKartAgder.Tests;

/// <summary>Kodelistene: at medlemslistene for IKT-samarbeid stemmer med kommunene i kartet.</summary>
public class KatalogTester
{
    // Data/agder.json kopieres til testmappen, så kartet leses akkurat som i appen
    private static readonly HashSet<string> KommunerIKartet =
        new KartData(new TestMiljo(AppContext.BaseDirectory)).Kommuner.ToHashSet();

    [Fact]
    public void Kartet_har_alle_25_kommunene() => Assert.Equal(25, KommunerIKartet.Count);

    [Fact]
    public void Kommunelisten_i_katalogen_er_lik_kartet() =>
        Assert.True(KommunerIKartet.SetEquals(Katalog.Kommuner), "Katalog.Kommuner og agder.json har ulike kommuner");

    [Fact]
    public void Alle_ikt_samarbeid_har_medlemmer_og_hjelpetekst()
    {
        foreach (var s in Katalog.IktSamarbeid)
        {
            Assert.True(Katalog.IktSamarbeidMedlemmer.TryGetValue(s, out var m) && m.Length > 0, $"{s} mangler medlemmer");
            Assert.True(Katalog.IktSamarbeidHjelp.ContainsKey(s), $"{s} mangler hjelpetekst");
        }
    }

    [Fact]
    public void Medlemslistene_har_bare_ekte_kommunenavn()
    {
        var ukjente = Katalog.IktSamarbeidMedlemmer
            .SelectMany(kv => kv.Value.Select(k => (Samarbeid: kv.Key, Kommune: k)))
            .Where(x => x.Kommune != Katalog.Fylkeskommune && !KommunerIKartet.Contains(x.Kommune))
            .Select(x => $"{x.Samarbeid}: {x.Kommune}").ToList();
        Assert.True(ukjente.Count == 0, "Ukjente kommuner: " + string.Join(", ", ukjente));
    }

    [Fact]
    public void Regionale_samarbeid_overlapper_ikke()
    {
        // Digi Agder dekker hele fylket og er derfor holdt utenfor
        var regionale = Katalog.IktSamarbeid.Where(s => s != "Digi Agder");
        var iFlere = regionale.SelectMany(s => Katalog.IktSamarbeidMedlemmer[s].Where(k => k != Katalog.Fylkeskommune))
            .GroupBy(k => k).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        Assert.True(iFlere.Count == 0, "Med i flere samarbeid: " + string.Join(", ", iFlere));
    }

    [Fact]
    public void Kristiansand_kan_ikke_velges_som_ansvarlig_samarbeid()
    {
        Assert.DoesNotContain("Kristiansand", Katalog.AnsvarligeSamarbeid);
        Assert.Contains("DDV", Katalog.AnsvarligeSamarbeid);
    }

    [Theory]
    [InlineData("Arendal", "Arendal kommune")]
    [InlineData("Evje og Hornnes", "Evje og Hornnes kommune")]
    [InlineData(Katalog.Fylkeskommune, Katalog.Fylkeskommune)]
    public void Eiernavn_lages_riktig(string kommune, string forventet) => Assert.Equal(forventet, Katalog.EierNavn(kommune));
}
