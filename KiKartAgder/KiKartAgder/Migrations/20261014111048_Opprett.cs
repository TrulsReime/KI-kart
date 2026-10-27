using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace KiKartAgder.Migrations
{
    /// <inheritdoc />
    public partial class Opprett : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Behov",
                columns: table => new
                {
                    BehovId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Navn = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Rekkefolge = table.Column<int>(type: "integer", nullable: false),
                    ErAktiv = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Behov", x => x.BehovId);
                });

            migrationBuilder.CreateTable(
                name: "Faser",
                columns: table => new
                {
                    FaseId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Navn = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Rekkefolge = table.Column<int>(type: "integer", nullable: false),
                    ErAvsluttet = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Faser", x => x.FaseId);
                });

            migrationBuilder.CreateTable(
                name: "Kommuner",
                columns: table => new
                {
                    KommuneNr = table.Column<string>(type: "char(4)", nullable: false),
                    Navn = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Fylke = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ErAktiv = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kommuner", x => x.KommuneNr);
                });

            migrationBuilder.CreateTable(
                name: "Sektorer",
                columns: table => new
                {
                    SektorId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Navn = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Rekkefolge = table.Column<int>(type: "integer", nullable: false),
                    ErAktiv = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sektorer", x => x.SektorId);
                });

            migrationBuilder.CreateTable(
                name: "Stikkord",
                columns: table => new
                {
                    StikkordId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Navn = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stikkord", x => x.StikkordId);
                });

            migrationBuilder.CreateTable(
                name: "Stopparsaker",
                columns: table => new
                {
                    StopparsakId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Navn = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ErAktiv = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stopparsaker", x => x.StopparsakId);
                });

            migrationBuilder.CreateTable(
                name: "VarselAbonnenter",
                columns: table => new
                {
                    AbonnentId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Epost = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    Opprettet = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Bekreftet = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AvmeldingToken = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Avmeldt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VarselAbonnenter", x => x.AbonnentId);
                });

            migrationBuilder.CreateTable(
                name: "Organisasjoner",
                columns: table => new
                {
                    OrganisasjonId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Navn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    KommuneNr = table.Column<string>(type: "char(4)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organisasjoner", x => x.OrganisasjonId);
                    table.ForeignKey(
                        name: "FK_Organisasjoner_Kommuner_KommuneNr",
                        column: x => x.KommuneNr,
                        principalTable: "Kommuner",
                        principalColumn: "KommuneNr",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Kontaktpersoner",
                columns: table => new
                {
                    KontaktpersonId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Navn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Rolle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Epost = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    OrganisasjonId = table.Column<int>(type: "integer", nullable: false),
                    SamtykkePublisering = table.Column<bool>(type: "boolean", nullable: false),
                    SamtykkeDato = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kontaktpersoner", x => x.KontaktpersonId);
                    table.ForeignKey(
                        name: "FK_Kontaktpersoner_Organisasjoner_OrganisasjonId",
                        column: x => x.OrganisasjonId,
                        principalTable: "Organisasjoner",
                        principalColumn: "OrganisasjonId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Prosjekter",
                columns: table => new
                {
                    ProsjektId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Tittel = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Beskrivelse = table.Column<string>(type: "text", nullable: true),
                    Mal = table.Column<string>(type: "text", nullable: true),
                    StatusNaa = table.Column<string>(type: "text", nullable: true),
                    NesteSteg = table.Column<string>(type: "text", nullable: true),
                    Teknologi = table.Column<string>(type: "text", nullable: true),
                    Gevinst = table.Column<string>(type: "text", nullable: true),
                    Fungerte = table.Column<string>(type: "text", nullable: true),
                    Annerledes = table.Column<string>(type: "text", nullable: true),
                    EierOrganisasjonId = table.Column<int>(type: "integer", nullable: false),
                    KartKommuneNr = table.Column<string>(type: "char(4)", nullable: false),
                    KontaktpersonId = table.Column<int>(type: "integer", nullable: false),
                    SektorId = table.Column<int>(type: "integer", nullable: false),
                    FaseId = table.Column<int>(type: "integer", nullable: false),
                    StopparsakId = table.Column<int>(type: "integer", nullable: true),
                    ApenForFlere = table.Column<bool>(type: "boolean", nullable: false),
                    Publiseringsstatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    UtvalgtRekkefolge = table.Column<int>(type: "integer", nullable: true),
                    Registrert = table.Column<DateOnly>(type: "date", nullable: false),
                    Oppdatert = table.Column<DateOnly>(type: "date", nullable: false),
                    SistEndret = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PaminnelseSendt = table.Column<DateOnly>(type: "date", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prosjekter", x => x.ProsjektId);
                    table.ForeignKey(
                        name: "FK_Prosjekter_Faser_FaseId",
                        column: x => x.FaseId,
                        principalTable: "Faser",
                        principalColumn: "FaseId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Prosjekter_Kommuner_KartKommuneNr",
                        column: x => x.KartKommuneNr,
                        principalTable: "Kommuner",
                        principalColumn: "KommuneNr",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Prosjekter_Kontaktpersoner_KontaktpersonId",
                        column: x => x.KontaktpersonId,
                        principalTable: "Kontaktpersoner",
                        principalColumn: "KontaktpersonId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Prosjekter_Organisasjoner_EierOrganisasjonId",
                        column: x => x.EierOrganisasjonId,
                        principalTable: "Organisasjoner",
                        principalColumn: "OrganisasjonId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Prosjekter_Sektorer_SektorId",
                        column: x => x.SektorId,
                        principalTable: "Sektorer",
                        principalColumn: "SektorId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Prosjekter_Stopparsaker_StopparsakId",
                        column: x => x.StopparsakId,
                        principalTable: "Stopparsaker",
                        principalColumn: "StopparsakId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Henvendelser",
                columns: table => new
                {
                    HenvendelseId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProsjektId = table.Column<int>(type: "integer", nullable: false),
                    FraNavn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FraEpost = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    Melding = table.Column<string>(type: "text", nullable: false),
                    Dato = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Henvendelser", x => x.HenvendelseId);
                    table.ForeignKey(
                        name: "FK_Henvendelser_Prosjekter_ProsjektId",
                        column: x => x.ProsjektId,
                        principalTable: "Prosjekter",
                        principalColumn: "ProsjektId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Innmeldinger",
                columns: table => new
                {
                    InnmeldingId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ProsjektId = table.Column<int>(type: "integer", nullable: false),
                    Dato = table.Column<DateOnly>(type: "date", nullable: false),
                    FraNavn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    FraEpost = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: true),
                    Tekst = table.Column<string>(type: "text", nullable: true),
                    KommuneNr = table.Column<string>(type: "char(4)", nullable: true),
                    NyeVerdier = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Kommentar = table.Column<string>(type: "text", nullable: true),
                    BehandletDato = table.Column<DateOnly>(type: "date", nullable: true),
                    BehandletAvId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Innmeldinger", x => x.InnmeldingId);
                    table.ForeignKey(
                        name: "FK_Innmeldinger_Kommuner_KommuneNr",
                        column: x => x.KommuneNr,
                        principalTable: "Kommuner",
                        principalColumn: "KommuneNr",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Innmeldinger_Prosjekter_ProsjektId",
                        column: x => x.ProsjektId,
                        principalTable: "Prosjekter",
                        principalColumn: "ProsjektId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Logg",
                columns: table => new
                {
                    LoggId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProsjektId = table.Column<int>(type: "integer", nullable: false),
                    Dato = table.Column<DateOnly>(type: "date", nullable: false),
                    Tekst = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Logg", x => x.LoggId);
                    table.ForeignKey(
                        name: "FK_Logg_Prosjekter_ProsjektId",
                        column: x => x.ProsjektId,
                        principalTable: "Prosjekter",
                        principalColumn: "ProsjektId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProsjektBehov",
                columns: table => new
                {
                    ProsjektId = table.Column<int>(type: "integer", nullable: false),
                    BehovId = table.Column<int>(type: "integer", nullable: false),
                    Retning = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProsjektBehov", x => new { x.ProsjektId, x.BehovId, x.Retning });
                    table.ForeignKey(
                        name: "FK_ProsjektBehov_Behov_BehovId",
                        column: x => x.BehovId,
                        principalTable: "Behov",
                        principalColumn: "BehovId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProsjektBehov_Prosjekter_ProsjektId",
                        column: x => x.ProsjektId,
                        principalTable: "Prosjekter",
                        principalColumn: "ProsjektId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProsjektBilder",
                columns: table => new
                {
                    BildeId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProsjektId = table.Column<int>(type: "integer", nullable: false),
                    Url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    AltTekst = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Rekkefolge = table.Column<int>(type: "integer", nullable: false),
                    ErForside = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProsjektBilder", x => x.BildeId);
                    table.ForeignKey(
                        name: "FK_ProsjektBilder_Prosjekter_ProsjektId",
                        column: x => x.ProsjektId,
                        principalTable: "Prosjekter",
                        principalColumn: "ProsjektId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProsjektDeltakere",
                columns: table => new
                {
                    ProsjektId = table.Column<int>(type: "integer", nullable: false),
                    KommuneNr = table.Column<string>(type: "char(4)", nullable: false),
                    ErLeder = table.Column<bool>(type: "boolean", nullable: false),
                    BleMed = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProsjektDeltakere", x => new { x.ProsjektId, x.KommuneNr });
                    table.ForeignKey(
                        name: "FK_ProsjektDeltakere_Kommuner_KommuneNr",
                        column: x => x.KommuneNr,
                        principalTable: "Kommuner",
                        principalColumn: "KommuneNr",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProsjektDeltakere_Prosjekter_ProsjektId",
                        column: x => x.ProsjektId,
                        principalTable: "Prosjekter",
                        principalColumn: "ProsjektId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProsjektStikkord",
                columns: table => new
                {
                    ProsjektId = table.Column<int>(type: "integer", nullable: false),
                    StikkordId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProsjektStikkord", x => new { x.ProsjektId, x.StikkordId });
                    table.ForeignKey(
                        name: "FK_ProsjektStikkord_Prosjekter_ProsjektId",
                        column: x => x.ProsjektId,
                        principalTable: "Prosjekter",
                        principalColumn: "ProsjektId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProsjektStikkord_Stikkord_StikkordId",
                        column: x => x.StikkordId,
                        principalTable: "Stikkord",
                        principalColumn: "StikkordId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Behov",
                columns: new[] { "BehovId", "ErAktiv", "Navn", "Rekkefolge" },
                values: new object[,]
                {
                    { 1, true, "Samarbeidspartner", 1 },
                    { 2, true, "Pilotkommune", 2 },
                    { 3, true, "Kompetanse", 3 },
                    { 4, true, "Data", 4 },
                    { 5, true, "Erfaringer fra andre", 5 },
                    { 6, true, "Personvernvurdering", 6 },
                    { 7, true, "Finansiering", 7 }
                });

            migrationBuilder.InsertData(
                table: "Faser",
                columns: new[] { "FaseId", "ErAvsluttet", "Navn", "Rekkefolge" },
                values: new object[,]
                {
                    { 1, false, "Idé", 1 },
                    { 2, false, "Pilot", 2 },
                    { 3, false, "Drift", 3 },
                    { 4, true, "Fullført", 4 },
                    { 5, true, "Avbrutt", 5 }
                });

            migrationBuilder.InsertData(
                table: "Kommuner",
                columns: new[] { "KommuneNr", "ErAktiv", "Fylke", "Navn" },
                values: new object[,]
                {
                    { "4201", true, "Agder", "Risør" },
                    { "4202", true, "Agder", "Grimstad" },
                    { "4203", true, "Agder", "Arendal" },
                    { "4204", true, "Agder", "Kristiansand" },
                    { "4205", true, "Agder", "Lindesnes" },
                    { "4206", true, "Agder", "Farsund" },
                    { "4207", true, "Agder", "Flekkefjord" },
                    { "4211", true, "Agder", "Gjerstad" },
                    { "4212", true, "Agder", "Vegårshei" },
                    { "4213", true, "Agder", "Tvedestrand" },
                    { "4214", true, "Agder", "Froland" },
                    { "4215", true, "Agder", "Lillesand" },
                    { "4216", true, "Agder", "Birkenes" },
                    { "4217", true, "Agder", "Åmli" },
                    { "4218", true, "Agder", "Iveland" },
                    { "4219", true, "Agder", "Evje og Hornnes" },
                    { "4220", true, "Agder", "Bygland" },
                    { "4221", true, "Agder", "Valle" },
                    { "4222", true, "Agder", "Bykle" },
                    { "4223", true, "Agder", "Vennesla" },
                    { "4224", true, "Agder", "Åseral" },
                    { "4225", true, "Agder", "Lyngdal" },
                    { "4226", true, "Agder", "Hægebostad" },
                    { "4227", true, "Agder", "Kvinesdal" },
                    { "4228", true, "Agder", "Sirdal" }
                });

            migrationBuilder.InsertData(
                table: "Sektorer",
                columns: new[] { "SektorId", "ErAktiv", "Navn", "Rekkefolge" },
                values: new object[,]
                {
                    { 1, true, "Oppvekst", 1 },
                    { 2, true, "E-helse", 2 },
                    { 3, true, "Sosial og velferd", 3 },
                    { 4, true, "Teknisk", 4 },
                    { 5, true, "Administrativt og tverrfaglig", 5 },
                    { 6, true, "Teknologi og infrastruktur", 6 }
                });

            migrationBuilder.InsertData(
                table: "Stopparsaker",
                columns: new[] { "StopparsakId", "ErAktiv", "Navn" },
                values: new object[,]
                {
                    { 1, true, "Personvern og juss" },
                    { 2, true, "Kostnad større enn nytte" },
                    { 3, true, "Teknologien holdt ikke mål" },
                    { 4, true, "Leverandør eller avtale" },
                    { 5, true, "Behovet var ikke der" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Behov_Navn",
                table: "Behov",
                column: "Navn",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Faser_Navn",
                table: "Faser",
                column: "Navn",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Henvendelser_ProsjektId",
                table: "Henvendelser",
                column: "ProsjektId");

            migrationBuilder.CreateIndex(
                name: "IX_Innmeldinger_KommuneNr",
                table: "Innmeldinger",
                column: "KommuneNr");

            migrationBuilder.CreateIndex(
                name: "IX_Innmeldinger_ProsjektId",
                table: "Innmeldinger",
                column: "ProsjektId");

            migrationBuilder.CreateIndex(
                name: "IX_Innmeldinger_Status",
                table: "Innmeldinger",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Kontaktpersoner_Epost",
                table: "Kontaktpersoner",
                column: "Epost",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Kontaktpersoner_OrganisasjonId",
                table: "Kontaktpersoner",
                column: "OrganisasjonId");

            migrationBuilder.CreateIndex(
                name: "IX_Logg_ProsjektId",
                table: "Logg",
                column: "ProsjektId");

            migrationBuilder.CreateIndex(
                name: "IX_Organisasjoner_KommuneNr",
                table: "Organisasjoner",
                column: "KommuneNr");

            migrationBuilder.CreateIndex(
                name: "IX_ProsjektBehov_BehovId",
                table: "ProsjektBehov",
                column: "BehovId");

            migrationBuilder.CreateIndex(
                name: "IX_ProsjektBilder_ProsjektId",
                table: "ProsjektBilder",
                column: "ProsjektId");

            migrationBuilder.CreateIndex(
                name: "IX_ProsjektDeltakere_KommuneNr",
                table: "ProsjektDeltakere",
                column: "KommuneNr");

            migrationBuilder.CreateIndex(
                name: "IX_Prosjekter_EierOrganisasjonId",
                table: "Prosjekter",
                column: "EierOrganisasjonId");

            migrationBuilder.CreateIndex(
                name: "IX_Prosjekter_FaseId",
                table: "Prosjekter",
                column: "FaseId");

            migrationBuilder.CreateIndex(
                name: "IX_Prosjekter_KartKommuneNr",
                table: "Prosjekter",
                column: "KartKommuneNr");

            migrationBuilder.CreateIndex(
                name: "IX_Prosjekter_KontaktpersonId",
                table: "Prosjekter",
                column: "KontaktpersonId");

            migrationBuilder.CreateIndex(
                name: "IX_Prosjekter_Publiseringsstatus",
                table: "Prosjekter",
                column: "Publiseringsstatus");

            migrationBuilder.CreateIndex(
                name: "IX_Prosjekter_SektorId",
                table: "Prosjekter",
                column: "SektorId");

            migrationBuilder.CreateIndex(
                name: "IX_Prosjekter_StopparsakId",
                table: "Prosjekter",
                column: "StopparsakId");

            migrationBuilder.CreateIndex(
                name: "IX_ProsjektStikkord_StikkordId",
                table: "ProsjektStikkord",
                column: "StikkordId");

            migrationBuilder.CreateIndex(
                name: "IX_Sektorer_Navn",
                table: "Sektorer",
                column: "Navn",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Stikkord_Navn",
                table: "Stikkord",
                column: "Navn",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Stopparsaker_Navn",
                table: "Stopparsaker",
                column: "Navn",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VarselAbonnenter_AvmeldingToken",
                table: "VarselAbonnenter",
                column: "AvmeldingToken",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VarselAbonnenter_Epost",
                table: "VarselAbonnenter",
                column: "Epost",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Henvendelser");

            migrationBuilder.DropTable(
                name: "Innmeldinger");

            migrationBuilder.DropTable(
                name: "Logg");

            migrationBuilder.DropTable(
                name: "ProsjektBehov");

            migrationBuilder.DropTable(
                name: "ProsjektBilder");

            migrationBuilder.DropTable(
                name: "ProsjektDeltakere");

            migrationBuilder.DropTable(
                name: "ProsjektStikkord");

            migrationBuilder.DropTable(
                name: "VarselAbonnenter");

            migrationBuilder.DropTable(
                name: "Behov");

            migrationBuilder.DropTable(
                name: "Prosjekter");

            migrationBuilder.DropTable(
                name: "Stikkord");

            migrationBuilder.DropTable(
                name: "Faser");

            migrationBuilder.DropTable(
                name: "Kontaktpersoner");

            migrationBuilder.DropTable(
                name: "Sektorer");

            migrationBuilder.DropTable(
                name: "Stopparsaker");

            migrationBuilder.DropTable(
                name: "Organisasjoner");

            migrationBuilder.DropTable(
                name: "Kommuner");
        }
    }
}
