using KiKartAgder.Data;
using KiKartAgder.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace KiKartAgder.Tests;

/// <summary>
/// JSON-lagringen: at innmeldinger går gjennom godkjenning, og at dataene overlever en omstart.
/// Hver test får sin egen midlertidige mappe.
/// </summary>
public class MinneRepositoryTester : IDisposable
{
    private readonly string _mappe = Path.Combine(Path.GetTempPath(), "kikart-test-" + Guid.NewGuid().ToString("N"));
    private string Datafil => Path.Combine(_mappe, "App_Data", "kikart-data.json");

    public MinneRepositoryTester()
    {
        Directory.CreateDirectory(Path.Combine(_mappe, "Data"));
        File.WriteAllText(Path.Combine(_mappe, "Data", "prosjekter.json"), """{"prosjekter":[],"innmeldinger":[],"utvalgte":[]}""");
    }

    public void Dispose()
    {
        try { Directory.Delete(_mappe, recursive: true); } catch (IOException) { }
    }

    /// <summary>Lager et nytt repository mot samme datafil, slik som når appen startes på nytt.</summary>
    private MinneRepository Start()
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["KiKart:Datafil"] = Datafil }).Build();
        return new MinneRepository(new TestMiljo(_mappe), new FastDato(Lag.Idag), config, NullLogger<MinneRepository>.Instance);
    }

    [Fact]
    public void Nytt_prosjekt_vises_ikke_for_det_er_godkjent()
    {
        var repo = Start();
        var id = repo.MeldInnNytt(Lag.Prosjekt());

        Assert.Empty(repo.Publiserte());
        Assert.True(repo.Finn(id)!.TilGodkjenning);

        var innmelding = Assert.Single(repo.Innmeldinger(), i => i.ProsjektId == id);
        Assert.True(repo.Godkjenn(innmelding.Id));

        Assert.Single(repo.Publiserte(), p => p.Id == id);
        Assert.Single(repo.Aktive(), p => p.Id == id);
    }

    [Fact]
    public void En_innmelding_kan_ikke_godkjennes_to_ganger()
    {
        var repo = Start();
        var id = repo.MeldInnNytt(Lag.Prosjekt());
        var innmelding = repo.Innmeldinger().Single(i => i.ProsjektId == id);

        Assert.True(repo.Godkjenn(innmelding.Id));
        Assert.False(repo.Godkjenn(innmelding.Id));
    }

    [Fact]
    public void Dataene_overlever_en_omstart()
    {
        var forst = Start();
        var id = forst.MeldInnNytt(Lag.Prosjekt(tittel: "Lagres til fil", fungerte: "Æ, ø og å skal overleve"));
        forst.Godkjenn(forst.Innmeldinger().Single(i => i.ProsjektId == id).Id);
        forst.SendHenvendelse(id, "Kari", "Hei");

        var etterOmstart = Start();
        var p = Assert.Single(etterOmstart.Publiserte());
        Assert.Equal("Lagres til fil", p.Tittel);
        Assert.Equal("Æ, ø og å skal overleve", p.Fungerte);
        Assert.Single(etterOmstart.Henvendelser());
        Assert.True(File.Exists(Datafil));
    }

    [Fact]
    public void Nye_prosjekter_far_hver_sin_id()
    {
        var repo = Start();
        var a = repo.MeldInnNytt(Lag.Prosjekt(tittel: "A"));
        var b = repo.MeldInnNytt(Lag.Prosjekt(tittel: "B"));
        Assert.NotEqual(a, b);
    }

    [Fact]
    public void Bli_med_legger_til_kommunen_etter_godkjenning()
    {
        var repo = Start();
        var id = repo.MeldInnNytt(Lag.Prosjekt(kommune: "Arendal"));
        repo.Godkjenn(repo.Innmeldinger().Single(i => i.ProsjektId == id).Id);

        repo.BeOmABliMed(id, "Grimstad", "Kari", "Vi vil være med");
        Assert.DoesNotContain("Grimstad", repo.Finn(id)!.Deltakere);

        var bliMed = repo.Innmeldinger().Single(i => i.Type == InnmeldingType.BliMed);
        repo.Godkjenn(bliMed.Id);
        Assert.Contains("Grimstad", repo.Finn(id)!.Deltakere);
    }
}
