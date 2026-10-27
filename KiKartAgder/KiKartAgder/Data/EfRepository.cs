using System.Text.Json;
using KiKartAgder.Data.Entiteter;
using KiKartAgder.Services;
using Microsoft.EntityFrameworkCore;
using M = KiKartAgder.Models;

namespace KiKartAgder.Data;

/// <summary>
/// Leser og skriver KI-kartet i databasen. Oversetter mellom databasetabellene (Data/Entiteter)
/// og klassene i Models som kontrollerne og visningene bruker, så de ikke trenger å endres.
/// </summary>
public class EfRepository : IKiKartRepository
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private readonly KiKartDbContext _db;
    private readonly IDatoTjeneste _dato;

    // Alle prosjektene lastes én gang per forespørsel (layout og kontroller spør flere ganger).
    private List<(M.Prosjekt Prosjekt, Publiseringsstatus Status, int? Utvalgt)>? _alle;

    public EfRepository(KiKartDbContext db, IDatoTjeneste dato)
    {
        _db = db;
        _dato = dato;
    }

    private DateOnly Idag => _dato.Idag;

    // ---------- lesing ----------
    public IReadOnlyList<M.Prosjekt> Publiserte() =>
        LastAlle().Where(x => x.Status == Publiseringsstatus.Publisert).Select(x => x.Prosjekt).ToList();

    /// <summary>Publiserte prosjekter som ikke er inaktive (ikke bekreftet på 12 måneder).</summary>
    public IReadOnlyList<M.Prosjekt> Aktive() =>
        Publiserte().Where(p => !ProsjektLogikk.ErInaktiv(p, Idag)).ToList();

    public IReadOnlyList<M.Prosjekt> Alle() => LastAlle().Select(x => x.Prosjekt).ToList();

    public M.Prosjekt? Finn(int id) => LastAlle().Select(x => x.Prosjekt).FirstOrDefault(p => p.Id == id);

    public IReadOnlyList<M.Prosjekt> Utvalgte() =>
        LastAlle().Where(x => x.Status == Publiseringsstatus.Publisert && x.Utvalgt != null && !ProsjektLogikk.ErInaktiv(x.Prosjekt, Idag))
            .OrderBy(x => x.Utvalgt).Select(x => x.Prosjekt).ToList();

    public IReadOnlyList<M.Innmelding> Innmeldinger() =>
        _db.Innmeldinger.AsNoTracking().Include(i => i.Kommune)
            .OrderByDescending(i => i.Dato).ThenByDescending(i => i.InnmeldingId)
            .AsEnumerable()
            .Select(i => new M.Innmelding
            {
                Id = i.InnmeldingId,
                Type = i.Type,
                ProsjektId = i.ProsjektId,
                Dato = i.Dato,
                Fra = Fra(i.FraNavn, i.FraEpost),
                Tekst = i.Tekst,
                Kommune = i.Kommune?.Navn,
                NyeVerdier = i.NyeVerdier == null ? null : JsonSerializer.Deserialize<M.Prosjekt>(i.NyeVerdier, Json),
                Status = i.Status,
                Kommentar = i.Kommentar,
                BehandletDato = i.BehandletDato
            }).ToList();

    public IReadOnlyList<M.Henvendelse> Henvendelser() =>
        _db.Henvendelser.AsNoTracking().OrderBy(h => h.HenvendelseId).AsEnumerable()
            .Select(h => new M.Henvendelse(h.ProsjektId, Fra(h.FraNavn, h.FraEpost), h.Melding, h.Dato)).ToList();

    private List<(M.Prosjekt Prosjekt, Publiseringsstatus Status, int? Utvalgt)> LastAlle()
    {
        if (_alle != null) return _alle;
        _alle = MedAltInkludert(_db.Prosjekter.AsNoTracking())
            .OrderBy(p => p.ProsjektId)
            .AsEnumerable()
            .Select(p => (TilModell(p), p.Publiseringsstatus, p.UtvalgtRekkefolge))
            .ToList();
        return _alle;
    }

    private static IQueryable<Prosjekt> MedAltInkludert(IQueryable<Prosjekt> q) => q
        .Include(p => p.EierOrganisasjon)
        .Include(p => p.KartKommune)
        .Include(p => p.Kontaktperson)
        .Include(p => p.Prosjekteier)
        .Include(p => p.Sektor)
        .Include(p => p.Fase)
        .Include(p => p.Stopparsak)
        .Include(p => p.Deltakere).ThenInclude(d => d.Kommune)
        .Include(p => p.Stikkord).ThenInclude(s => s.Stikkord)
        .Include(p => p.Behov).ThenInclude(b => b.Behov)
        .Include(p => p.IktSamarbeid).ThenInclude(i => i.IktSamarbeid)
        .Include(p => p.Logg)
        .AsSplitQuery();

    private static M.Prosjekt TilModell(Prosjekt p) => new()
    {
        Id = p.ProsjektId,
        Tittel = p.Tittel,
        Eier = p.EierOrganisasjon.Navn,
        Kommune = p.KartKommune.Navn,
        Deltakere = p.Deltakere.OrderBy(d => d.Rekkefolge).Select(d => d.Kommune.Navn).ToList(),
        Sektor = p.Sektor.Navn,
        Fase = p.Fase.Navn,
        Stopp = p.Stopparsak?.Navn,
        Stikkord = p.Stikkord.OrderBy(s => s.Rekkefolge).Select(s => s.Stikkord.Navn).ToList(),
        IktSamarbeid = p.IktSamarbeid.OrderBy(i => i.Rekkefolge).Select(i => i.IktSamarbeid.Navn).ToList(),
        Mal = p.Mal,
        Beskrivelse = p.Beskrivelse,
        Status = p.StatusNaa,
        NesteSteg = p.NesteSteg,
        Teknologi = p.Teknologi,
        Gevinst = p.Gevinst,
        // TODO: vis bare navn og e-post når Kontaktperson.SamtykkePublisering er satt (krever samtykke i skjemaet)
        Kontakt = TilModell(p.Kontaktperson),
        Prosjekteier = p.Prosjekteier == null ? null : TilModell(p.Prosjekteier),
        Registrert = p.Registrert,
        Oppdatert = p.Oppdatert,
        SistEndret = p.SistEndret,
        Soker = Behovliste(p, BehovRetning.Soker, p.SokerAnnet),
        Tilbyr = Behovliste(p, BehovRetning.Tilbyr, p.TilbyrAnnet),
        Apen = p.ApenForFlere,
        Fungerte = p.Fungerte,
        Annerledes = p.Annerledes,
        TilGodkjenning = p.Publiseringsstatus != Publiseringsstatus.Publisert,
        PaminnelseSendt = p.PaminnelseSendt,
        Logg = p.Logg.OrderBy(l => l.Dato).ThenBy(l => l.LoggId).Select(l => new M.LoggOppforing(l.Dato, l.Tekst)).ToList()
    };

    /// <summary>Behovene fra kodelisten i valgt rekkefølge, med «Annet»-teksten til slutt (som i skjemaet).</summary>
    private static List<string> Behovliste(Prosjekt p, BehovRetning retning, string? annet) =>
        p.Behov.Where(b => b.Retning == retning).OrderBy(b => b.Rekkefolge).Select(b => b.Behov.Navn)
            .Concat(string.IsNullOrWhiteSpace(annet) ? Array.Empty<string>() : new[] { annet })
            .ToList();

    private static M.Kontaktperson TilModell(Kontaktperson k) => new() { Navn = k.Navn, Rolle = k.Rolle, Epost = k.Epost };

    /// <summary>Slik innsenderen vises i Forvaltning: «Navn, e-post», bare én av dem, eller «Anonym».</summary>
    private static string Fra(string? navn, string? epost)
    {
        var deler = new[] { navn, epost }.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
        return deler.Count > 0 ? string.Join(", ", deler) : "Anonym";
    }

    /// <summary>«Fra» er fritekst i prototypen: enten en e-post eller et navn.</summary>
    private static (string? Navn, string? Epost) DelFra(string? fra) =>
        string.IsNullOrWhiteSpace(fra) ? (null, null)
        : fra.Contains('@') ? (null, fra.Trim())
        : (fra.Trim(), null);

    // ---------- innsending ----------
    public int MeldInnNytt(M.Prosjekt p)
    {
        var ny = new Prosjekt
        {
            Registrert = Idag,
            Publiseringsstatus = Publiseringsstatus.TilGodkjenning
        };
        KopierVerdier(p, ny, godkjent: false);
        ny.Prosjekteier = ny.Kontaktperson; // den som melder inn, blir eier
        ny.Beskrivelse = p.Beskrivelse;
        ny.NesteSteg = p.NesteSteg;
        ny.Teknologi = p.Teknologi;
        ny.Oppdatert = Idag;
        ny.SistEndret = p.SistEndret;
        ny.Logg.Add(new LoggOppforing { Dato = Idag, Tekst = p.SistEndret });
        ny.Innmeldinger.Add(new Innmelding
        {
            Type = M.InnmeldingType.NyttProsjekt,
            Dato = Idag,
            FraNavn = p.Kontakt.Navn,
            FraEpost = p.Kontakt.Epost,
            Tekst = p.Beskrivelse
        });
        _db.Prosjekter.Add(ny);
        Lagre();
        return ny.ProsjektId;
    }

    public void MeldInnOppdatering(int prosjektId, M.Prosjekt nye, string hvaErNytt, string fra)
    {
        var (navn, epost) = DelFra(fra);
        _db.Innmeldinger.Add(new Innmelding
        {
            Type = M.InnmeldingType.Oppdatering, ProsjektId = prosjektId, Dato = Idag,
            FraNavn = navn, FraEpost = epost,
            Tekst = "Oppdatering fra eier: " + hvaErNytt,
            NyeVerdier = JsonSerializer.Serialize(nye, Json)
        });
        Lagre();
    }

    public void ForeslaRettelse(int prosjektId, string tekst, string? fra)
    {
        var (navn, epost) = DelFra(fra);
        _db.Innmeldinger.Add(new Innmelding
        {
            Type = M.InnmeldingType.Rettelse, ProsjektId = prosjektId, Dato = Idag,
            FraNavn = navn, FraEpost = epost, Tekst = tekst
        });
        Lagre();
    }

    public void BeOmABliMed(int prosjektId, string kommune, string navn, string melding)
    {
        _db.Innmeldinger.Add(new Innmelding
        {
            Type = M.InnmeldingType.BliMed, ProsjektId = prosjektId, Dato = Idag,
            FraNavn = navn, KommuneNr = KommuneNr(kommune), Tekst = melding
        });
        Lagre();
    }

    public void SendHenvendelse(int prosjektId, string fra, string melding)
    {
        // Skjemaet spør bare etter «navn og kommune» i dag, ikke e-post
        _db.Henvendelser.Add(new Henvendelse
        {
            ProsjektId = prosjektId, FraNavn = fra, FraEpost = "", Melding = melding, Dato = Idag
        });
        Lagre();
    }

    // ---------- forvaltning ----------
    public bool Godkjenn(int innmeldingId)
    {
        var i = _db.Innmeldinger.Include(x => x.Kommune).FirstOrDefault(x => x.InnmeldingId == innmeldingId);
        if (i == null || !KanBehandles(i)) return false;
        var p = MedAltInkludert(_db.Prosjekter).FirstOrDefault(x => x.ProsjektId == i.ProsjektId);
        if (p == null) return false;

        switch (i.Type)
        {
            case M.InnmeldingType.NyttProsjekt:
                p.Publiseringsstatus = Publiseringsstatus.Publisert;
                p.Oppdatert = Idag;
                break;
            case M.InnmeldingType.Oppdatering when i.NyeVerdier != null:
                var nye = JsonSerializer.Deserialize<M.Prosjekt>(i.NyeVerdier, Json)!;
                KopierVerdier(nye, p, godkjent: true);
                p.Beskrivelse = nye.Beskrivelse;
                Merk(p, nye.SistEndret);
                break;
            case M.InnmeldingType.Rettelse:
                var t = i.Tekst ?? "";
                Merk(p, "Rettet: " + (t.Length > 60 ? t[..60] : t));
                break;
            case M.InnmeldingType.BliMed when i.KommuneNr != null:
                if (p.Deltakere.All(d => d.KommuneNr != i.KommuneNr))
                    p.Deltakere.Add(new ProsjektDeltaker
                    {
                        KommuneNr = i.KommuneNr, BleMed = Idag,
                        Rekkefolge = p.Deltakere.Select(d => d.Rekkefolge).DefaultIfEmpty(-1).Max() + 1
                    });
                Merk(p, $"{i.Kommune!.Navn} ble med");
                break;
        }
        i.Status = M.InnmeldingStatus.Godkjent;
        i.BehandletDato = Idag;
        Lagre();
        return true;
    }

    public bool BeOmEndring(int innmeldingId, string kommentar) => SettStatus(innmeldingId, M.InnmeldingStatus.VenterPaInnsender, kommentar);
    public bool Avvis(int innmeldingId, string kommentar) => SettStatus(innmeldingId, M.InnmeldingStatus.Avvist, kommentar);

    private bool SettStatus(int id, M.InnmeldingStatus status, string kommentar)
    {
        var i = _db.Innmeldinger.Include(x => x.Prosjekt).FirstOrDefault(x => x.InnmeldingId == id);
        if (i == null || !KanBehandles(i)) return false;
        i.Status = status;
        i.Kommentar = kommentar;
        i.BehandletDato = Idag;
        // Et avvist nytt prosjekt blir liggende, men merkes som avvist
        if (status == M.InnmeldingStatus.Avvist && i.Type == M.InnmeldingType.NyttProsjekt)
            i.Prosjekt.Publiseringsstatus = Publiseringsstatus.Avvist;
        Lagre();
        return true;
    }

    public void MarkerPaminnelseSendt(int prosjektId)
    {
        var p = _db.Prosjekter.Find(prosjektId);
        if (p == null) return;
        p.PaminnelseSendt = Idag;
        Lagre();
    }

    public bool Bekreft(int prosjektId)
    {
        var p = _db.Prosjekter.Include(x => x.Logg).FirstOrDefault(x => x.ProsjektId == prosjektId);
        if (p == null || p.Publiseringsstatus != Publiseringsstatus.Publisert) return false;
        Merk(p, "Eier bekreftet at informasjonen stemmer");
        Lagre();
        return true;
    }

    private static bool KanBehandles(Innmelding i) =>
        i.Status is M.InnmeldingStatus.Ny or M.InnmeldingStatus.VenterPaInnsender;

    /// <summary>Registrerer en endring. En ny endring nullstiller påminnelsen.</summary>
    private void Merk(Prosjekt p, string hva)
    {
        p.PaminnelseSendt = null;
        p.Oppdatert = Idag;
        p.SistEndret = hva;
        p.Logg.Add(new LoggOppforing { Dato = Idag, Tekst = hva });
    }

    private void Lagre()
    {
        _db.SaveChanges();
        _alle = null;
    }

    // ---------- fra skjemaet til databasen ----------

    /// <summary>
    /// Feltene som kan endres fra innmeldingsskjemaet (samme som i MinneRepository).
    /// Beskrivelse, NesteSteg og Teknologi settes av den som kaller.
    /// </summary>
    /// <param name="godkjent">Bare godkjente endringer kan endre navn og rolle på en eksisterende kontaktperson.</param>
    private void KopierVerdier(M.Prosjekt fra, Prosjekt til, bool godkjent)
    {
        til.Tittel = fra.Tittel;
        til.EierOrganisasjon = Organisasjon(fra.Eier);
        til.KartKommuneNr = KommuneNr(fra.Kommune);
        til.ApenForFlere = fra.Apen;
        til.Sektor = _db.Sektorer.Single(s => s.Navn == fra.Sektor);
        til.Fase = _db.Faser.Single(f => f.Navn == fra.Fase);
        til.Stopparsak = fra.Stopp == null ? null : _db.Stopparsaker.Single(s => s.Navn == fra.Stopp);
        til.Mal = fra.Mal;
        til.StatusNaa = fra.Status;
        til.Gevinst = fra.Gevinst;
        til.Fungerte = fra.Fungerte;
        til.Annerledes = fra.Annerledes;
        til.Kontaktperson = Kontaktperson(fra.Kontakt, til.EierOrganisasjon, oppdaterFinnes: godkjent);

        // Deltakere: første kommune leder. Beholder BleMed for kommuner som allerede er med.
        var navn = fra.Deltakere.Count > 0 ? fra.Deltakere : new List<string> { fra.Kommune };
        var nr = navn.Select(KommuneNr).Distinct().ToList();
        til.Deltakere.RemoveAll(d => !nr.Contains(d.KommuneNr));
        for (var i = 0; i < nr.Count; i++)
        {
            var d = til.Deltakere.FirstOrDefault(x => x.KommuneNr == nr[i]);
            if (d == null) til.Deltakere.Add(d = new ProsjektDeltaker { KommuneNr = nr[i], BleMed = Idag });
            d.ErLeder = i == 0;
            d.Rekkefolge = i;
        }

        var ordene = fra.Stikkord.Select(s => s.Trim()).Where(s => s.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        til.Stikkord.RemoveAll(s => !ordene.Contains(s.Stikkord.Navn));
        for (var i = 0; i < ordene.Count; i++)
        {
            var s = til.Stikkord.FirstOrDefault(x => x.Stikkord.Navn == ordene[i]);
            if (s == null) til.Stikkord.Add(s = new ProsjektStikkord { Stikkord = Stikkord(ordene[i]) });
            s.Rekkefolge = i;
        }

        var ikt = fra.IktSamarbeid.Distinct().ToList();
        til.IktSamarbeid.RemoveAll(x => !ikt.Contains(x.IktSamarbeid.Navn));
        for (var i = 0; i < ikt.Count; i++)
        {
            var x = til.IktSamarbeid.FirstOrDefault(y => y.IktSamarbeid.Navn == ikt[i]);
            if (x == null) til.IktSamarbeid.Add(x = new ProsjektIktSamarbeid { IktSamarbeid = _db.IktSamarbeider.Single(y => y.Navn == ikt[i]) });
            x.Rekkefolge = i;
        }

        til.SokerAnnet = OppdaterBehov(til, fra.Soker, BehovRetning.Soker);
        til.TilbyrAnnet = OppdaterBehov(til, fra.Tilbyr, BehovRetning.Tilbyr);
    }

    /// <summary>
    /// Kobler prosjektet til behovene fra kodelisten. Det som ikke står i kodelisten («Annet» i skjemaet)
    /// returneres som tekst og lagres i SokerAnnet/TilbyrAnnet.
    /// </summary>
    private string? OppdaterBehov(Prosjekt til, List<string> verdier, BehovRetning retning)
    {
        var kodeliste = _db.Behov.Select(b => b.Navn).ToHashSet();
        var navn = verdier.Distinct().Where(kodeliste.Contains).ToList();
        var annet = verdier.Where(v => !kodeliste.Contains(v) && !string.IsNullOrWhiteSpace(v)).Select(v => v.Trim()).Distinct().ToList();

        til.Behov.RemoveAll(b => b.Retning == retning && !navn.Contains(b.Behov.Navn));
        for (var i = 0; i < navn.Count; i++)
        {
            var b = til.Behov.FirstOrDefault(x => x.Retning == retning && x.Behov.Navn == navn[i]);
            if (b == null) til.Behov.Add(b = new ProsjektBehov { Behov = _db.Behov.Single(x => x.Navn == navn[i]), Retning = retning });
            b.Rekkefolge = i;
        }
        return annet.Count > 0 ? string.Join(", ", annet) : null;
    }

    private string KommuneNr(string navn) =>
        _db.Kommuner.Where(k => k.Navn == navn).Select(k => k.KommuneNr).FirstOrDefault()
        ?? throw new InvalidOperationException($"Ukjent kommune: {navn}");

    /// <summary>Finner organisasjonen på navn, eller lager en ny. «X kommune» kobles til kommunen X.</summary>
    private Organisasjon Organisasjon(string navn)
    {
        var o = _db.Organisasjoner.Local.FirstOrDefault(x => x.Navn == navn)
                ?? _db.Organisasjoner.FirstOrDefault(x => x.Navn == navn);
        if (o != null) return o;

        var kommunenavn = navn.EndsWith(" kommune") ? navn[..^" kommune".Length] : null;
        var kommuneNr = kommunenavn == null ? null
            : _db.Kommuner.Where(k => k.Navn == kommunenavn).Select(k => k.KommuneNr).FirstOrDefault();
        return new Organisasjon
        {
            Navn = navn,
            Type = kommuneNr != null ? OrganisasjonType.Kommune
                 : navn.EndsWith("fylkeskommune") ? OrganisasjonType.Fylkeskommune
                 : OrganisasjonType.Annet,
            KommuneNr = kommuneNr
        };
    }

    /// <summary>Finner kontaktpersonen på e-post, eller lager en ny (uten samtykke til publisering).</summary>
    private Kontaktperson Kontaktperson(M.Kontaktperson k, Organisasjon org, bool oppdaterFinnes)
    {
        var finnes = _db.Kontaktpersoner.Local.FirstOrDefault(x => x.Epost == k.Epost)
                     ?? _db.Kontaktpersoner.FirstOrDefault(x => x.Epost == k.Epost);
        if (finnes != null && oppdaterFinnes)
        {
            finnes.Navn = k.Navn;
            finnes.Rolle = k.Rolle;
        }
        return finnes ?? new Kontaktperson { Navn = k.Navn, Rolle = k.Rolle, Epost = k.Epost, Organisasjon = org };
    }

    private Stikkord Stikkord(string navn) =>
        _db.Stikkord.Local.FirstOrDefault(s => s.Navn == navn)
        ?? _db.Stikkord.FirstOrDefault(s => s.Navn == navn)
        ?? new Stikkord { Navn = navn };
}
