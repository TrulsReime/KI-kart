using KiKartAgder.Models;
using KiKartAgder.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;

namespace KiKartAgder.Tests;

/// <summary>Fast dato, så testene gir samme svar uansett når de kjøres.</summary>
public class FastDato(DateOnly idag) : IDatoTjeneste
{
    public DateOnly Idag { get; } = idag;
}

/// <summary>Minimal IWebHostEnvironment som peker på en midlertidig mappe.</summary>
public class TestMiljo(string rot) : IWebHostEnvironment
{
    public string WebRootPath { get; set; } = rot;
    public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
    public string ApplicationName { get; set; } = "KiKartAgder";
    public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    public string ContentRootPath { get; set; } = rot;
    public string EnvironmentName { get; set; } = "Test";
}

public static class Lag
{
    public static readonly DateOnly Idag = new(2026, 11, 17);

    public static Prosjekt Prosjekt(
        string tittel = "Testprosjekt",
        string kommune = "Arendal",
        string[]? deltakere = null,
        string sektor = "Oppvekst",
        string fase = Katalog.Pilot,
        string[]? stikkord = null,
        string[]? ikt = null,
        string? fungerte = null,
        DateOnly? oppdatert = null) => new()
    {
        Tittel = tittel,
        Eier = Katalog.EierNavn(kommune),
        Kommune = kommune,
        Deltakere = (deltakere ?? new[] { kommune }).ToList(),
        Sektor = sektor,
        Fase = fase,
        Stikkord = (stikkord ?? new[] { "chatbot" }).ToList(),
        IktSamarbeid = (ikt ?? Array.Empty<string>()).ToList(),
        Fungerte = fungerte,
        Kontakt = new Kontaktperson { Navn = "Ola Eksempel", Rolle = "Prosjektleder", Epost = "ola@eksempel.no" },
        Registrert = oppdatert ?? Idag,
        Oppdatert = oppdatert ?? Idag
    };
}
