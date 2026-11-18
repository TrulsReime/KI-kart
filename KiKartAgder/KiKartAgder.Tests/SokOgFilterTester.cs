using KiKartAgder.Models;
using KiKartAgder.Services;
using KiKartAgder.ViewModels;

namespace KiKartAgder.Tests;

/// <summary>Søket og filtrene på prosjektsiden og oversikten.</summary>
public class SokOgFilterTester
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Tomt_sok_treffer_alt(string? sok) => Assert.True(ProsjektLogikk.Treffer(Lag.Prosjekt(), sok));

    [Theory]
    [InlineData("CHATBOT")]
    [InlineData("#chatbot")]
    [InlineData("arendal")]
    public void Sok_ignorerer_store_bokstaver_og_emneknagg(string sok) => Assert.True(ProsjektLogikk.Treffer(Lag.Prosjekt(), sok));

    [Fact]
    public void Sok_finner_tekst_i_erfaringene() =>
        Assert.True(ProsjektLogikk.Treffer(Lag.Prosjekt(fungerte: "Tidlig avklaring med personvernombudet"), "personvernombudet"));

    [Fact]
    public void Sok_finner_ikt_samarbeid_og_behov()
    {
        var p = Lag.Prosjekt(ikt: new[] { "IKT Agder" });
        p.Soker.Add("Pilotkommune");
        Assert.True(ProsjektLogikk.Treffer(p, "ikt agder"));
        Assert.True(ProsjektLogikk.Treffer(p, "pilotkommune"));
    }

    [Fact]
    public void Alle_ordene_i_soket_ma_treffe()
    {
        var p = Lag.Prosjekt(kommune: "Arendal");
        Assert.True(ProsjektLogikk.Treffer(p, "chatbot arendal"));
        Assert.False(ProsjektLogikk.Treffer(p, "chatbot lyngdal"));
    }

    [Fact]
    public void Kommunefilter_tar_med_samarbeid_der_kommunen_er_deltaker()
    {
        var f = new ProsjektFilter { Kommune = new() { "Grimstad" } };
        Assert.True(f.Passer(Lag.Prosjekt(kommune: "Arendal", deltakere: new[] { "Arendal", "Grimstad" })));
        Assert.False(f.Passer(Lag.Prosjekt(kommune: "Arendal")));
    }

    [Fact]
    public void Ulike_filtre_ma_alle_passe()
    {
        var f = new ProsjektFilter { Sektor = new() { "Oppvekst" }, Fase = new() { Katalog.Drift } };
        Assert.True(f.Passer(Lag.Prosjekt(sektor: "Oppvekst", fase: Katalog.Drift)));
        Assert.False(f.Passer(Lag.Prosjekt(sektor: "Oppvekst", fase: Katalog.Pilot)));
        Assert.False(f.Passer(Lag.Prosjekt(sektor: "E-helse", fase: Katalog.Drift)));
    }

    [Fact]
    public void Erfaringsfilter_viser_bare_prosjekter_med_erfaringer()
    {
        var f = new ProsjektFilter { Erfaringer = true };
        Assert.True(f.Passer(Lag.Prosjekt(fungerte: "Korte samlinger")));
        Assert.False(f.Passer(Lag.Prosjekt()));
    }

    [Fact]
    public void Ikt_filter_viser_prosjekter_merket_med_samarbeidet()
    {
        var f = new ProsjektFilter { Ikt = new() { "DDV" } };
        Assert.True(f.Passer(Lag.Prosjekt(ikt: new[] { "DDV" })));
        Assert.False(f.Passer(Lag.Prosjekt(ikt: new[] { "IKT Agder" })));
    }

    [Fact]
    public void Lenken_beholder_filtrene_og_kan_fjerne_ett()
    {
        var f = new ProsjektFilter { Q = "chatbot", Kommune = new() { "Arendal", "Grimstad" }, Erfaringer = true };
        var url = f.Url("kommune", "Arendal");
        Assert.Contains("q=chatbot", url);
        Assert.Contains("kommune=Grimstad", url);
        Assert.DoesNotContain("kommune=Arendal", url);
        Assert.Contains("erfaringer=true", url);
    }
}
