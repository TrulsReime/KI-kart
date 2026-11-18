using System.ComponentModel.DataAnnotations;
using KiKartAgder.Models;
using KiKartAgder.ViewModels;

namespace KiKartAgder.Tests;

/// <summary>Skjemaet for innmelding: hva som godtas, og hvilket prosjekt det blir til.</summary>
public class MeldInnSkjemaTester
{
    private static MeldInnSkjema Gyldig(Action<MeldInnSkjema>? endre = null)
    {
        var s = new MeldInnSkjema
        {
            Tittel = "Chatbot for innbyggere",
            Beskrivelse = "Svarer på spørsmål",
            Sektor = "Oppvekst",
            Fase = Katalog.Pilot,
            Stikkord = "chatbot",
            Organisering = "Enkeltkommune",
            Ansvarlig = "Arendal",
            KontaktNavn = "Ola Eksempel",
            KontaktEpost = "ola@eksempel.no"
        };
        endre?.Invoke(s);
        return s;
    }

    private static List<ValidationResult> Valider(MeldInnSkjema s)
    {
        var feil = new List<ValidationResult>();
        Validator.TryValidateObject(s, new ValidationContext(s), feil, validateAllProperties: true);
        return feil;
    }

    private static bool HarFeilPa(MeldInnSkjema s, string felt) => Valider(s).Any(f => f.MemberNames.Contains(felt));

    [Fact]
    public void Gyldig_skjema_har_ingen_feil() => Assert.Empty(Valider(Gyldig()));

    [Theory]
    [InlineData(nameof(MeldInnSkjema.Tittel))]
    [InlineData(nameof(MeldInnSkjema.Ansvarlig))]
    [InlineData(nameof(MeldInnSkjema.KontaktEpost))]
    public void Pakrevde_felt_ma_fylles_ut(string felt)
    {
        var s = Gyldig();
        typeof(MeldInnSkjema).GetProperty(felt)!.SetValue(s, null);
        Assert.True(HarFeilPa(s, felt));
    }

    [Fact]
    public void Ugyldig_epost_avvises() => Assert.True(HarFeilPa(Gyldig(s => s.KontaktEpost = "ikke-en-epost"), nameof(MeldInnSkjema.KontaktEpost)));

    [Fact]
    public void Minst_ett_stikkord_kreves() => Assert.True(HarFeilPa(Gyldig(s => s.Stikkord = " , "), nameof(MeldInnSkjema.Stikkord)));

    [Fact]
    public void Ukjent_sektor_avvises() => Assert.True(HarFeilPa(Gyldig(s => s.Sektor = "Finnes ikke"), nameof(MeldInnSkjema.Sektor)));

    [Fact]
    public void Ukjent_ikt_samarbeid_avvises() =>
        Assert.True(HarFeilPa(Gyldig(s => s.IktSamarbeid = new() { "Finnes ikke" }), nameof(MeldInnSkjema.IktSamarbeid)));

    [Fact]
    public void Samarbeid_krever_minst_to_kommuner()
    {
        var s = Gyldig(s => { s.Organisering = "Samarbeid"; s.Deltakere = new() { "Arendal" }; });
        Assert.True(HarFeilPa(s, nameof(MeldInnSkjema.Deltakere)));
    }

    [Fact]
    public void Stikkord_ryddes_og_gjores_like()
    {
        var s = Gyldig(s => s.Stikkord = "#Chat Bot, språkmodell,  , chat bot");
        Assert.Equal(new[] { "chat-bot", "språkmodell" }, s.StikkordListe());
    }

    [Fact]
    public void En_kommune_blir_eier_og_eneste_deltaker()
    {
        var p = Gyldig().TilProsjekt();
        Assert.Equal("Arendal kommune", p.Eier);
        Assert.Equal("Arendal", p.Kommune);
        Assert.Equal(new[] { "Arendal" }, p.Deltakere);
        Assert.False(p.Apen);
    }

    [Fact]
    public void Ansvarlig_kommune_star_forst_i_samarbeid()
    {
        var p = Gyldig(s => { s.Organisering = "Samarbeid"; s.Ansvarlig = "Grimstad"; s.Deltakere = new() { "Arendal", "Grimstad" }; }).TilProsjekt();
        Assert.Equal(new[] { "Grimstad", "Arendal" }, p.Deltakere);
        Assert.True(p.Samarbeid);
    }

    [Fact]
    public void Fylkeskommunen_far_riktig_navn_som_eier()
    {
        var p = Gyldig(s => s.Ansvarlig = Katalog.Fylkeskommune).TilProsjekt();
        Assert.Equal(Katalog.Fylkeskommune, p.Eier);
    }

    [Fact]
    public void Ikt_samarbeid_som_ansvarlig_blir_eier_med_kommunene_som_deltakere()
    {
        var ddv = Katalog.IktSamarbeidMedlemmer["DDV"];
        var s = Gyldig(s => { s.Ansvarlig = "DDV"; s.Deltakere = ddv.ToList(); });

        Assert.Empty(Valider(s));
        var p = s.TilProsjekt();
        Assert.Equal("DDV", p.Eier);
        Assert.Equal(ddv, p.Deltakere);
        Assert.Equal(ddv[0], p.Kommune);
        Assert.Contains("DDV", p.IktSamarbeid);
        Assert.True(Katalog.EiesAvSamarbeid(p));
    }

    [Fact]
    public void Kristiansand_som_ansvarlig_er_kommunen_ikke_samarbeidet()
    {
        var s = Gyldig(s => s.Ansvarlig = "Kristiansand");
        Assert.Null(s.AnsvarligSamarbeid);
        Assert.Equal("Kristiansand kommune", s.TilProsjekt().Eier);
    }

    [Theory]
    [InlineData("Oslo")]
    [InlineData("ikt:DDV")]
    public void Ukjent_ansvarlig_avvises(string ansvarlig) =>
        Assert.True(HarFeilPa(Gyldig(s => s.Ansvarlig = ansvarlig), nameof(MeldInnSkjema.Ansvarlig)));

    [Fact]
    public void Ukjent_deltakerkommune_avvises()
    {
        var s = Gyldig(s => { s.Organisering = "Samarbeid"; s.Deltakere = new() { "Arendal", "Oslo" }; });
        Assert.True(HarFeilPa(s, nameof(MeldInnSkjema.Deltakere)));
    }

    [Fact]
    public void Stopparsak_lagres_bare_for_avbrutte_prosjekter()
    {
        Assert.Null(Gyldig(s => s.Stopp = "Personvern og juss").TilProsjekt().Stopp);
        Assert.Equal("Personvern og juss", Gyldig(s => { s.Fase = Katalog.Avbrutt; s.Stopp = "Personvern og juss"; }).TilProsjekt().Stopp);
    }

    [Fact]
    public void Prosjekt_eid_av_samarbeid_fylles_riktig_inn_ved_oppdatering()
    {
        var p = Lag.Prosjekt(kommune: "Farsund", deltakere: Katalog.IktSamarbeidMedlemmer["DDV"], ikt: new[] { "DDV" });
        p.Eier = "DDV";

        var s = MeldInnSkjema.Fra(p);
        Assert.Equal("DDV", s.Ansvarlig);
        Assert.Equal("Samarbeid", s.Organisering);
        Assert.Equal("DDV", s.TilProsjekt().Eier);
    }
}
