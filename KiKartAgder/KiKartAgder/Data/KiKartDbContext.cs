using KiKartAgder.Data.Entiteter;
using Katalog = KiKartAgder.Models.Katalog;
using InnmeldingStatus = KiKartAgder.Models.InnmeldingStatus;
using InnmeldingType = KiKartAgder.Models.InnmeldingType;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KiKartAgder.Data;

/// <summary>
/// Databasen for KI-kartet. Modellen følger docs/ER-diagram.md (versjon 2).
/// Tabellnavnene blir navnet på hver DbSet.
/// </summary>
public class KiKartDbContext : DbContext
{
    public KiKartDbContext(DbContextOptions<KiKartDbContext> options) : base(options) { }

    public DbSet<Kommune> Kommuner => Set<Kommune>();
    public DbSet<Organisasjon> Organisasjoner => Set<Organisasjon>();
    public DbSet<Kontaktperson> Kontaktpersoner => Set<Kontaktperson>();
    public DbSet<Sektor> Sektorer => Set<Sektor>();
    public DbSet<Fase> Faser => Set<Fase>();
    public DbSet<Stopparsak> Stopparsaker => Set<Stopparsak>();
    public DbSet<Behov> Behov => Set<Behov>();
    public DbSet<Stikkord> Stikkord => Set<Stikkord>();
    public DbSet<IktSamarbeid> IktSamarbeider => Set<IktSamarbeid>();
    public DbSet<Prosjekt> Prosjekter => Set<Prosjekt>();
    public DbSet<ProsjektDeltaker> ProsjektDeltakere => Set<ProsjektDeltaker>();
    public DbSet<ProsjektStikkord> ProsjektStikkord => Set<ProsjektStikkord>();
    public DbSet<ProsjektBehov> ProsjektBehov => Set<ProsjektBehov>();
    public DbSet<ProsjektIktSamarbeid> ProsjektIktSamarbeid => Set<ProsjektIktSamarbeid>();
    public DbSet<ProsjektBilde> ProsjektBilder => Set<ProsjektBilde>();
    public DbSet<LoggOppforing> Logg => Set<LoggOppforing>();
    public DbSet<Innmelding> Innmeldinger => Set<Innmelding>();
    public DbSet<Henvendelse> Henvendelser => Set<Henvendelse>();
    public DbSet<VarselAbonnent> VarselAbonnenter => Set<VarselAbonnent>();

    protected override void ConfigureConventions(ModelConfigurationBuilder b)
    {
        // Enumer lagres som tekst, så de er lesbare i databasen og i Power BI
        b.Properties<OrganisasjonType>().HaveConversion<string>().HaveMaxLength(20);
        b.Properties<Publiseringsstatus>().HaveConversion<string>().HaveMaxLength(20);
        b.Properties<BehovRetning>().HaveConversion<string>().HaveMaxLength(10);
        b.Properties<InnmeldingType>().HaveConversion<string>().HaveMaxLength(20);
        b.Properties<InnmeldingStatus>().HaveConversion<string>().HaveMaxLength(20);
    }

    protected override void OnModelCreating(ModelBuilder mb)
    {
        // ---------- kommuner og organisasjoner ----------
        mb.Entity<Kommune>(e =>
        {
            e.HasKey(k => k.KommuneNr);
            KommuneNr(e.Property(k => k.KommuneNr));
            e.Property(k => k.Navn).HasMaxLength(100);
            e.Property(k => k.Fylke).HasMaxLength(100);
            e.HasData(AgderKommuner);
        });

        mb.Entity<Organisasjon>(e =>
        {
            e.Property(o => o.Navn).HasMaxLength(200);
            KommuneNr(e.Property(o => o.KommuneNr));
            e.HasOne(o => o.Kommune).WithMany(k => k.Organisasjoner).HasForeignKey(o => o.KommuneNr);
        });

        mb.Entity<Kontaktperson>(e =>
        {
            e.Property(k => k.Navn).HasMaxLength(200);
            e.Property(k => k.Rolle).HasMaxLength(200);
            e.Property(k => k.Epost).HasMaxLength(254);
            e.HasIndex(k => k.Epost).IsUnique();
        });

        // ---------- kodelister ----------
        mb.Entity<Sektor>(e =>
        {
            e.Property(x => x.Navn).HasMaxLength(100);
            e.HasIndex(x => x.Navn).IsUnique();
            e.HasData(Katalog.Sektorer.Select((navn, i) =>
                new Sektor { SektorId = i + 1, Navn = navn, Rekkefolge = i + 1, ErAktiv = true }));
        });

        mb.Entity<Fase>(e =>
        {
            e.Property(x => x.Navn).HasMaxLength(50);
            e.HasIndex(x => x.Navn).IsUnique();
            e.HasData(Katalog.Faser.Select((navn, i) =>
                new Fase { FaseId = i + 1, Navn = navn, Rekkefolge = i + 1, ErAvsluttet = navn is Katalog.Fullfort or Katalog.Avbrutt }));
        });

        mb.Entity<Stopparsak>(e =>
        {
            e.Property(x => x.Navn).HasMaxLength(100);
            e.HasIndex(x => x.Navn).IsUnique();
            e.HasData(Katalog.StoppArsaker.Select((navn, i) =>
                new Stopparsak { StopparsakId = i + 1, Navn = navn, ErAktiv = true }));
        });

        mb.Entity<Behov>(e =>
        {
            e.Property(x => x.Navn).HasMaxLength(100);
            e.HasIndex(x => x.Navn).IsUnique();
            e.HasData(Katalog.Behov.Select((navn, i) =>
                new Behov { BehovId = i + 1, Navn = navn, Rekkefolge = i + 1, ErAktiv = true }));
        });

        mb.Entity<IktSamarbeid>(e =>
        {
            e.Property(x => x.Navn).HasMaxLength(50);
            e.Property(x => x.Beskrivelse).HasMaxLength(200);
            e.HasIndex(x => x.Navn).IsUnique();
            e.HasData(Katalog.IktSamarbeid.Select((navn, i) => new IktSamarbeid
            {
                IktSamarbeidId = i + 1, Navn = navn, Beskrivelse = Katalog.IktSamarbeidHjelp.GetValueOrDefault(navn),
                Rekkefolge = i + 1, ErAktiv = true
            }));
        });

        mb.Entity<Stikkord>(e =>
        {
            e.Property(x => x.Navn).HasMaxLength(100);
            e.HasIndex(x => x.Navn).IsUnique();
        });

        // ---------- prosjekt ----------
        mb.Entity<Prosjekt>(e =>
        {
            e.Property(p => p.Tittel).HasMaxLength(200);
            e.Property(p => p.SistEndret).HasMaxLength(200);
            e.Property(p => p.SokerAnnet).HasMaxLength(200);
            e.Property(p => p.TilbyrAnnet).HasMaxLength(200);
            e.Property(p => p.RowVersion).IsRowVersion();
            KommuneNr(e.Property(p => p.KartKommuneNr));

            e.HasOne(p => p.EierOrganisasjon).WithMany(o => o.Prosjekter).HasForeignKey(p => p.EierOrganisasjonId);
            e.HasOne(p => p.KartKommune).WithMany().HasForeignKey(p => p.KartKommuneNr);
            e.HasOne(p => p.Kontaktperson).WithMany(k => k.Prosjekter).HasForeignKey(p => p.KontaktpersonId);
            e.HasOne(p => p.Prosjekteier).WithMany(k => k.EidProsjekter).HasForeignKey(p => p.ProsjekteierId);

            e.HasIndex(p => p.Publiseringsstatus);
        });

        mb.Entity<ProsjektDeltaker>(e =>
        {
            e.HasKey(d => new { d.ProsjektId, d.KommuneNr });
            KommuneNr(e.Property(d => d.KommuneNr));
            e.HasOne(d => d.Prosjekt).WithMany(p => p.Deltakere).HasForeignKey(d => d.ProsjektId);
            e.HasOne(d => d.Kommune).WithMany(k => k.Deltakelser).HasForeignKey(d => d.KommuneNr);
        });

        mb.Entity<ProsjektStikkord>(e =>
        {
            e.HasKey(s => new { s.ProsjektId, s.StikkordId });
            e.HasOne(s => s.Prosjekt).WithMany(p => p.Stikkord).HasForeignKey(s => s.ProsjektId);
            e.HasOne(s => s.Stikkord).WithMany(s => s.Prosjekter).HasForeignKey(s => s.StikkordId);
        });

        mb.Entity<ProsjektBehov>(e =>
        {
            e.HasKey(b => new { b.ProsjektId, b.BehovId, b.Retning });
            e.HasOne(b => b.Prosjekt).WithMany(p => p.Behov).HasForeignKey(b => b.ProsjektId);
        });

        mb.Entity<ProsjektIktSamarbeid>(e =>
        {
            e.HasKey(s => new { s.ProsjektId, s.IktSamarbeidId });
            e.HasOne(s => s.Prosjekt).WithMany(p => p.IktSamarbeid).HasForeignKey(s => s.ProsjektId);
            e.HasOne(s => s.IktSamarbeid).WithMany(i => i.Prosjekter).HasForeignKey(s => s.IktSamarbeidId);
        });

        mb.Entity<ProsjektBilde>(e =>
        {
            e.HasKey(b => b.BildeId);
            e.Property(b => b.Url).HasMaxLength(500);
            e.Property(b => b.AltTekst).HasMaxLength(300);
        });

        mb.Entity<LoggOppforing>(e =>
        {
            e.HasKey(l => l.LoggId);
            e.Property(l => l.Tekst).HasMaxLength(500);
        });

        // ---------- innmeldinger og henvendelser ----------
        mb.Entity<Innmelding>(e =>
        {
            e.Property(i => i.FraNavn).HasMaxLength(200);
            e.Property(i => i.FraEpost).HasMaxLength(254);
            e.Property(i => i.BehandletAvId).HasMaxLength(450); // samme lengde som AspNetUsers.Id
            KommuneNr(e.Property(i => i.KommuneNr));
            e.HasOne(i => i.Kommune).WithMany().HasForeignKey(i => i.KommuneNr);
            e.HasIndex(i => i.Status);
        });

        mb.Entity<Henvendelse>(e =>
        {
            e.Property(h => h.FraNavn).HasMaxLength(200);
            e.Property(h => h.FraEpost).HasMaxLength(254);
        });

        mb.Entity<VarselAbonnent>(e =>
        {
            e.HasKey(a => a.AbonnentId);
            e.Property(a => a.Epost).HasMaxLength(254);
            e.Property(a => a.AvmeldingToken).HasMaxLength(100);
            e.HasIndex(a => a.Epost).IsUnique();
            e.HasIndex(a => a.AvmeldingToken).IsUnique();
        });

        // Sletting: bare det som hører til et prosjekt slettes sammen med prosjektet.
        // Alt annet (kommuner, organisasjoner, kodelister) kan ikke slettes så lenge noe peker på det.
        // Det hindrer også feilen «multiple cascade paths» i SQL Server.
        foreach (var fk in mb.Model.GetEntityTypes().SelectMany(t => t.GetForeignKeys()))
        {
            var hørerTilProsjekt = fk.PrincipalEntityType.ClrType == typeof(Prosjekt)
                && fk.DeclaringEntityType.ClrType != typeof(Prosjekt);
            fk.DeleteBehavior = hørerTilProsjekt ? DeleteBehavior.Cascade : DeleteBehavior.Restrict;
        }
    }

    /// <summary>Kommunenummer fra SSB er alltid fire siffer.</summary>
    private static void KommuneNr(PropertyBuilder p) => p.HasColumnType("char(4)");

    /// <summary>De 25 kommunene i Agder med kommunenummer fra SSB (gjeldende fra 2024).</summary>
    private static readonly Kommune[] AgderKommuner = new (string Nr, string Navn)[]
    {
        ("4201", "Risør"), ("4202", "Grimstad"), ("4203", "Arendal"), ("4204", "Kristiansand"),
        ("4205", "Lindesnes"), ("4206", "Farsund"), ("4207", "Flekkefjord"), ("4211", "Gjerstad"),
        ("4212", "Vegårshei"), ("4213", "Tvedestrand"), ("4214", "Froland"), ("4215", "Lillesand"),
        ("4216", "Birkenes"), ("4217", "Åmli"), ("4218", "Iveland"), ("4219", "Evje og Hornnes"),
        ("4220", "Bygland"), ("4221", "Valle"), ("4222", "Bykle"), ("4223", "Vennesla"),
        ("4224", "Åseral"), ("4225", "Lyngdal"), ("4226", "Hægebostad"), ("4227", "Kvinesdal"),
        ("4228", "Sirdal"),
    }.Select(k => new Kommune { KommuneNr = k.Nr, Navn = k.Navn, Fylke = "Agder", ErAktiv = true }).ToArray();
}
