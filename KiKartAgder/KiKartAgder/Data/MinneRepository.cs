using System.Text.Json;
using KiKartAgder.Models;
using KiKartAgder.Services;

namespace KiKartAgder.Data;

/// <summary>
/// Holder alle data i minnet. Startdataene leses fra Data/prosjekter.json (fiktive eksempelprosjekter).
/// Endringer forsvinner når applikasjonen startes på nytt.
/// </summary>
public class MinneRepository : IKiKartRepository
{
    private readonly object _las = new();
    private readonly List<Prosjekt> _prosjekter;
    private readonly List<Innmelding> _innmeldinger = new();
    private readonly List<Henvendelse> _henvendelser = new();
    private readonly List<int> _utvalgte;
    private readonly IDatoTjeneste _dato;

    public MinneRepository(IWebHostEnvironment env, IDatoTjeneste dato)
    {
        _dato = dato;
        var sti = Path.Combine(env.ContentRootPath, "Data", "prosjekter.json");
        var opsjoner = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        opsjoner.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        var seed = JsonSerializer.Deserialize<Startdata>(File.ReadAllText(sti), opsjoner)
                   ?? throw new InvalidOperationException("Fant ikke startdata");

        _prosjekter = seed.Prosjekter;
        _utvalgte = seed.Utvalgte;
        foreach (var p in _prosjekter)
        {
            if (p.Deltakere.Count == 0) p.Deltakere.Add(p.Kommune);
            p.Prosjekteier ??= p.Kontakt;
            p.Logg = new() { new(p.Registrert, "Lagt inn i kartet") };
            if (p.Oppdatert != p.Registrert) p.Logg.Add(new(p.Oppdatert, p.SistEndret));
            if (p.TilGodkjenning) LeggTilInnmelding(new Innmelding
            {
                Type = InnmeldingType.NyttProsjekt,
                ProsjektId = p.Id,
                Dato = p.Registrert,
                Fra = $"{p.Kontakt.Navn}, {p.Eier}",
                Tekst = p.Beskrivelse
            });
        }
        foreach (var i in seed.Innmeldinger) LeggTilInnmelding(i);
    }

    private sealed class Startdata
    {
        public List<Prosjekt> Prosjekter { get; set; } = new();
        public List<Innmelding> Innmeldinger { get; set; } = new();
        public List<int> Utvalgte { get; set; } = new();
    }

    private void LeggTilInnmelding(Innmelding i)
    {
        i.Id = _innmeldinger.Count == 0 ? 1 : _innmeldinger.Max(x => x.Id) + 1;
        _innmeldinger.Add(i);
    }

    private DateOnly Idag => _dato.Idag;

    private static void Merk(Prosjekt p, DateOnly dato, string hva)
    {
        p.PaminnelseSendt = null;
        p.Oppdatert = dato;
        p.SistEndret = hva;
        p.Logg.Add(new(dato, hva));
    }

    // ---------- lesing ----------
    public IReadOnlyList<Prosjekt> Publiserte()
    {
        lock (_las) return _prosjekter.Where(p => !p.TilGodkjenning).ToList();
    }

    public IReadOnlyList<Prosjekt> Aktive()
    {
        lock (_las) return _prosjekter.Where(p => !p.TilGodkjenning && !ProsjektLogikk.ErInaktiv(p, Idag)).ToList();
    }

    public IReadOnlyList<Prosjekt> Alle()
    {
        lock (_las) return _prosjekter.ToList();
    }

    public Prosjekt? Finn(int id)
    {
        lock (_las) return _prosjekter.FirstOrDefault(p => p.Id == id);
    }

    public IReadOnlyList<Prosjekt> Utvalgte()
    {
        lock (_las)
            return _utvalgte.Select(id => _prosjekter.FirstOrDefault(p => p.Id == id && !p.TilGodkjenning && !ProsjektLogikk.ErInaktiv(p, Idag)))
                .Where(p => p != null).Cast<Prosjekt>().ToList();
    }

    public IReadOnlyList<Innmelding> Innmeldinger()
    {
        lock (_las) return _innmeldinger.OrderByDescending(i => i.Dato).ThenByDescending(i => i.Id).ToList();
    }

    public IReadOnlyList<Henvendelse> Henvendelser()
    {
        lock (_las) return _henvendelser.ToList();
    }

    // ---------- innsending ----------
    public int MeldInnNytt(Prosjekt p)
    {
        lock (_las)
        {
            p.Id = _prosjekter.Count == 0 ? 1 : _prosjekter.Max(x => x.Id) + 1;
            p.Registrert = p.Oppdatert = Idag;
            p.TilGodkjenning = true;
            p.Prosjekteier = p.Kontakt; // den som melder inn, blir eier
            p.Logg = new() { new(Idag, p.SistEndret) };
            _prosjekter.Add(p);
            LeggTilInnmelding(new Innmelding
            {
                Type = InnmeldingType.NyttProsjekt, ProsjektId = p.Id, Dato = Idag,
                Fra = $"{p.Kontakt.Navn}, {p.Eier}", Tekst = p.Beskrivelse
            });
            return p.Id;
        }
    }

    public void MeldInnOppdatering(int prosjektId, Prosjekt nye, string hvaErNytt, string fra)
    {
        lock (_las) LeggTilInnmelding(new Innmelding
        {
            Type = InnmeldingType.Oppdatering, ProsjektId = prosjektId, Dato = Idag,
            Fra = fra, Tekst = "Oppdatering fra eier: " + hvaErNytt, NyeVerdier = nye
        });
    }

    public void ForeslaRettelse(int prosjektId, string tekst, string? fra)
    {
        lock (_las) LeggTilInnmelding(new Innmelding
        {
            Type = InnmeldingType.Rettelse, ProsjektId = prosjektId, Dato = Idag,
            Fra = string.IsNullOrWhiteSpace(fra) ? "Anonym" : fra, Tekst = tekst
        });
    }

    public void BeOmABliMed(int prosjektId, string kommune, string navn, string melding)
    {
        lock (_las) LeggTilInnmelding(new Innmelding
        {
            Type = InnmeldingType.BliMed, ProsjektId = prosjektId, Dato = Idag,
            Fra = navn, Kommune = kommune, Tekst = melding
        });
    }

    public void SendHenvendelse(int prosjektId, string fra, string melding)
    {
        lock (_las) _henvendelser.Add(new Henvendelse(prosjektId, fra, melding, Idag));
    }

    // ---------- forvaltning ----------
    public bool Godkjenn(int innmeldingId)
    {
        lock (_las)
        {
            var i = _innmeldinger.FirstOrDefault(x => x.Id == innmeldingId);
            var p = i == null ? null : _prosjekter.FirstOrDefault(x => x.Id == i.ProsjektId);
            if (i == null || p == null || !i.KanBehandles) return false;

            switch (i.Type)
            {
                case InnmeldingType.NyttProsjekt:
                    p.TilGodkjenning = false;
                    p.Oppdatert = Idag;
                    break;
                case InnmeldingType.Oppdatering when i.NyeVerdier != null:
                    KopierVerdier(i.NyeVerdier, p);
                    Merk(p, Idag, i.NyeVerdier.SistEndret);
                    break;
                case InnmeldingType.Rettelse:
                    var t = i.Tekst ?? "";
                    Merk(p, Idag, "Rettet: " + (t.Length > 60 ? t[..60] : t));
                    break;
                case InnmeldingType.BliMed when i.Kommune != null:
                    if (!p.Deltakere.Contains(i.Kommune)) p.Deltakere.Add(i.Kommune);
                    Merk(p, Idag, $"{i.Kommune} ble med");
                    break;
            }
            i.Status = InnmeldingStatus.Godkjent;
            i.BehandletDato = Idag;
            return true;
        }
    }

    public bool BeOmEndring(int innmeldingId, string kommentar) => SettStatus(innmeldingId, InnmeldingStatus.VenterPaInnsender, kommentar);
    public bool Avvis(int innmeldingId, string kommentar) => SettStatus(innmeldingId, InnmeldingStatus.Avvist, kommentar);

    private bool SettStatus(int id, InnmeldingStatus status, string kommentar)
    {
        lock (_las)
        {
            var i = _innmeldinger.FirstOrDefault(x => x.Id == id);
            if (i == null || !i.KanBehandles) return false;
            i.Status = status;
            i.Kommentar = kommentar;
            i.BehandletDato = Idag;
            return true;
        }
    }

    public void MarkerPaminnelseSendt(int prosjektId)
    {
        lock (_las)
        {
            var p = _prosjekter.FirstOrDefault(x => x.Id == prosjektId);
            if (p != null) p.PaminnelseSendt = Idag;
        }
    }

    public bool Bekreft(int prosjektId)
    {
        lock (_las)
        {
            var p = _prosjekter.FirstOrDefault(x => x.Id == prosjektId);
            if (p == null || p.TilGodkjenning) return false;
            Merk(p, Idag, "Eier bekreftet at informasjonen stemmer");
            return true;
        }
    }

    private static void KopierVerdier(Prosjekt fra, Prosjekt til)
    {
        til.Tittel = fra.Tittel; til.Eier = fra.Eier; til.Kommune = fra.Kommune;
        til.Deltakere = fra.Deltakere; til.Apen = fra.Apen; til.IktSamarbeid = fra.IktSamarbeid; til.Sektor = fra.Sektor;
        til.Fase = fra.Fase; til.Stopp = fra.Stopp; til.Beskrivelse = fra.Beskrivelse;
        til.Mal = fra.Mal; til.Status = fra.Status; til.Gevinst = fra.Gevinst;
        til.Stikkord = fra.Stikkord; til.Kontakt = fra.Kontakt; til.Soker = fra.Soker;
        til.Tilbyr = fra.Tilbyr; til.Fungerte = fra.Fungerte; til.Annerledes = fra.Annerledes;
    }
}
