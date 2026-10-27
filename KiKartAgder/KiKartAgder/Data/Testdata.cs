using System.Text.Json;
using System.Text.Json.Serialization;
using KiKartAgder.Data.Entiteter;
using Microsoft.EntityFrameworkCore;
using M = KiKartAgder.Models;

namespace KiKartAgder.Data;

/// <summary>
/// Legger de fiktive eksempelprosjektene fra Data/prosjekter.json inn i databasen.
/// Brukes bare i Development, og bare når det ikke finnes prosjekter fra før.
/// </summary>
public static class Testdata
{
    public static async Task LeggInnAsync(KiKartDbContext db, string contentRoot)
    {
        if (await db.Prosjekter.AnyAsync()) return;

        var sti = Path.Combine(contentRoot, "Data", "prosjekter.json");
        var opsjoner = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        opsjoner.Converters.Add(new JsonStringEnumConverter());
        var seed = JsonSerializer.Deserialize<Startdata>(await File.ReadAllTextAsync(sti), opsjoner)
                   ?? throw new InvalidOperationException("Fant ikke testdata");

        // Kodelister og kommuner er seedet av migrasjonen. Slå opp på navn.
        var kommuner = await db.Kommuner.ToDictionaryAsync(k => k.Navn, k => k.KommuneNr);
        var sektorer = await db.Sektorer.ToDictionaryAsync(s => s.Navn);
        var faser = await db.Faser.ToDictionaryAsync(f => f.Navn);
        var stopparsaker = await db.Stopparsaker.ToDictionaryAsync(s => s.Navn);
        var behov = await db.Behov.ToDictionaryAsync(b => b.Navn);
        var iktSamarbeid = await db.IktSamarbeider.ToDictionaryAsync(i => i.Navn);

        var organisasjoner = new Dictionary<string, Organisasjon>();
        var kontakter = new Dictionary<string, Kontaktperson>();
        var stikkord = new Dictionary<string, Stikkord>();
        var prosjekter = new Dictionary<int, Prosjekt>(); // Id i JSON-filen -> ny rad

        Organisasjon Organisasjon(string navn)
        {
            if (organisasjoner.TryGetValue(navn, out var o)) return o;
            var kommunenavn = navn.EndsWith(" kommune") ? navn[..^" kommune".Length] : null;
            o = new Organisasjon
            {
                Navn = navn,
                Type = kommunenavn != null ? OrganisasjonType.Kommune
                     : navn.EndsWith("fylkeskommune") ? OrganisasjonType.Fylkeskommune
                     : OrganisasjonType.Annet,
                KommuneNr = kommunenavn != null ? kommuner[kommunenavn] : null
            };
            return organisasjoner[navn] = o;
        }

        foreach (var p in seed.Prosjekter)
        {
            var eier = Organisasjon(p.Eier);

            if (!kontakter.TryGetValue(p.Kontakt.Epost, out var kontakt))
            {
                kontakt = kontakter[p.Kontakt.Epost] = new Kontaktperson
                {
                    Navn = p.Kontakt.Navn,
                    Rolle = p.Kontakt.Rolle,
                    Epost = p.Kontakt.Epost,
                    Organisasjon = eier,
                    // Fiktive personer, så kontaktinfo kan vises som i prototypen
                    SamtykkePublisering = true,
                    SamtykkeDato = p.Registrert
                };
            }

            var deltakere = p.Deltakere.Count > 0 ? p.Deltakere : new List<string> { p.Kommune };
            var utvalgt = seed.Utvalgte.IndexOf(p.Id);

            var ny = new Prosjekt
            {
                Tittel = p.Tittel,
                Beskrivelse = p.Beskrivelse,
                Mal = p.Mal,
                StatusNaa = p.Status,
                NesteSteg = p.NesteSteg,
                Teknologi = p.Teknologi,
                Gevinst = p.Gevinst,
                Fungerte = p.Fungerte,
                Annerledes = p.Annerledes,
                EierOrganisasjon = eier,
                KartKommuneNr = kommuner[p.Kommune],
                Kontaktperson = kontakt,
                Prosjekteier = kontakt, // som i prototypen: den som meldte inn, er eier
                Sektor = sektorer[p.Sektor],
                Fase = faser[p.Fase],
                Stopparsak = p.Stopp != null ? stopparsaker[p.Stopp] : null,
                ApenForFlere = p.Apen,
                Publiseringsstatus = p.TilGodkjenning ? Publiseringsstatus.TilGodkjenning : Publiseringsstatus.Publisert,
                UtvalgtRekkefolge = utvalgt >= 0 ? utvalgt + 1 : null,
                Registrert = p.Registrert,
                Oppdatert = p.Oppdatert,
                SistEndret = p.SistEndret,
                PaminnelseSendt = p.PaminnelseSendt,

                // Første kommune i listen leder samarbeidet
                Deltakere = deltakere.Select((navn, i) => new ProsjektDeltaker
                {
                    KommuneNr = kommuner[navn],
                    ErLeder = i == 0,
                    BleMed = p.Registrert,
                    Rekkefolge = i
                }).ToList(),

                Stikkord = p.Stikkord.Distinct().Select((ord, i) => new ProsjektStikkord
                {
                    Stikkord = stikkord.TryGetValue(ord, out var s) ? s : stikkord[ord] = new Stikkord { Navn = ord },
                    Rekkefolge = i
                }).ToList(),

                Behov = p.Soker.Distinct().Select((b, i) => new ProsjektBehov { Behov = behov[b], Retning = BehovRetning.Soker, Rekkefolge = i })
                    .Concat(p.Tilbyr.Distinct().Select((b, i) => new ProsjektBehov { Behov = behov[b], Retning = BehovRetning.Tilbyr, Rekkefolge = i }))
                    .ToList(),

                IktSamarbeid = p.IktSamarbeid.Distinct().Select((navn, i) => new ProsjektIktSamarbeid
                {
                    IktSamarbeid = iktSamarbeid[navn],
                    Rekkefolge = i
                }).ToList(),

                Logg = new() { new LoggOppforing { Dato = p.Registrert, Tekst = "Lagt inn i kartet" } }
            };
            if (p.Oppdatert != p.Registrert)
                ny.Logg.Add(new LoggOppforing { Dato = p.Oppdatert, Tekst = p.SistEndret });

            // Som i prototypen: prosjekter til godkjenning har en innmelding om nytt prosjekt
            if (p.TilGodkjenning)
                ny.Innmeldinger.Add(new Innmelding
                {
                    Type = M.InnmeldingType.NyttProsjekt,
                    Dato = p.Registrert,
                    FraNavn = p.Kontakt.Navn,
                    FraEpost = p.Kontakt.Epost,
                    Tekst = p.Beskrivelse
                });

            prosjekter[p.Id] = ny;
            db.Prosjekter.Add(ny);
        }

        foreach (var i in seed.Innmeldinger)
        {
            // «Fra» er fritekst i prototypen: enten en e-post eller «Navn, rolle»
            var erEpost = i.Fra.Contains('@');
            prosjekter[i.ProsjektId].Innmeldinger.Add(new Innmelding
            {
                Type = i.Type,
                Dato = i.Dato,
                FraNavn = erEpost ? null : i.Fra,
                FraEpost = erEpost ? i.Fra : null,
                Tekst = i.Tekst,
                KommuneNr = i.Kommune != null ? kommuner[i.Kommune] : null,
                Status = i.Status,
                Kommentar = i.Kommentar,
                BehandletDato = i.BehandletDato
            });
        }

        await db.SaveChangesAsync();
    }

    private sealed class Startdata
    {
        public List<M.Prosjekt> Prosjekter { get; set; } = new();
        public List<M.Innmelding> Innmeldinger { get; set; } = new();
        public List<int> Utvalgte { get; set; } = new();
    }
}
