using KiKartAgder.Models;
using KiKartAgder.Services;
using static KiKartAgder.Services.ProsjektLogikk;

namespace KiKartAgder.Tests;

/// <summary>
/// Når et prosjekt er ferskt, eldre, snart må bekreftes, eller er inaktivt.
/// Inaktive prosjekter forsvinner fra kartet, så grensene er viktige å få riktig.
/// </summary>
public class AktualitetTester
{
    private static Aktualitet Niva(DateOnly oppdatert, string fase = Katalog.Pilot) =>
        Aktualitetsstatus(Lag.Prosjekt(fase: fase, oppdatert: oppdatert), Lag.Idag).Niva;

    [Fact]
    public void Oppdatert_i_dag_er_fersk() => Assert.Equal(Aktualitet.Fersk, Niva(Lag.Idag));

    [Fact]
    public void Dagen_for_seks_maneder_er_fortsatt_fersk() => Assert.Equal(Aktualitet.Fersk, Niva(Lag.Idag.AddMonths(-MndFersk).AddDays(1)));

    [Fact]
    public void Etter_seks_maneder_er_eldre() => Assert.Equal(Aktualitet.Eldre, Niva(Lag.Idag.AddMonths(-MndFersk)));

    [Fact]
    public void Etter_elleve_maneder_ma_det_bekreftes_snart() => Assert.Equal(Aktualitet.Snart, Niva(Lag.Idag.AddMonths(-MndPaminnelse)));

    [Fact]
    public void Pa_selve_fristen_er_prosjektet_fortsatt_aktivt()
    {
        var p = Lag.Prosjekt(oppdatert: Lag.Idag.AddMonths(-MndInaktiv));
        Assert.False(ErInaktiv(p, Lag.Idag));
    }

    [Fact]
    public void Dagen_etter_fristen_er_prosjektet_inaktivt()
    {
        var p = Lag.Prosjekt(oppdatert: Lag.Idag.AddMonths(-MndInaktiv).AddDays(-1));
        Assert.True(ErInaktiv(p, Lag.Idag));
    }

    [Theory]
    [InlineData(Katalog.Fullfort)]
    [InlineData(Katalog.Avbrutt)]
    public void Avsluttede_prosjekter_arkiveres_og_blir_aldri_inaktive(string fase)
    {
        var p = Lag.Prosjekt(fase: fase, oppdatert: Lag.Idag.AddYears(-5));
        Assert.Equal(Aktualitet.Arkiv, Aktualitetsstatus(p, Lag.Idag).Niva);
        Assert.False(ErInaktiv(p, Lag.Idag));
    }
}
