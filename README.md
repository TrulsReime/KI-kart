# KI-kart for Agder

En oversikt over prosjekter der kommunene i Agder tar i bruk kunstig intelligens (KI). Kommunene kan melde inn egne prosjekter, finne andre som jobber med det samme og lære av erfaringene deres. KI-kartet er laget for DigiAgder og skal vises i en iframe på digiagder.no.

Løsningen er laget av studenter ved Universitetet i Agder (UiA) høsten 2026 og er en **prototype**. Den er tenkt som grunnlag for en løsning som DigiAgder eller IKT Agder skal drifte videre.

## Hva som finnes i repoet

| Mappe eller fil | Innhold |
|---|---|
| `KiKartAgder/KiKartAgder/` | Selve nettsiden, en ASP.NET Core MVC-applikasjon i C# (.NET 10). Se [README](KiKartAgder/KiKartAgder/README.md) der for detaljer om koden. |
| `KiKartAgder/KiKartAgder.Tests/` | Tester for de viktigste reglene: skjemaet, søk og filtre, når prosjekter blir inaktive, kodelistene og lagringen. |
| `KiKartAgder/compose.yaml` | Oppsett for å kjøre en PostgreSQL-database lokalt med Docker. |
| `docs/` | ER-diagram (entitet og relasjon) som viser hvordan dataene henger sammen. |
| `KiKart.slnx` | Løsningsfil som samler appen og testene. |

## Teknologier

| Teknologi | Hva den gjør i KI-kartet |
|---|---|
| **C# og .NET 10** | Programmeringsspråket og plattformen alt på serveren kjører på. .NET 10 får sikkerhetsoppdateringer fra Microsoft til 2028. |
| **ASP.NET Core MVC** | Rammeverket som gjør appen til en nettside. Koden er delt i *Model* (dataene, f.eks. et prosjekt), *View* (sidene brukeren ser) og *Controller* (tar imot forespørselen, henter data og velger side). |
| **Razor** (`.cshtml`) | HTML med litt C# inni. Serveren fyller inn dataene, så nettleseren får en ferdig side. |
| **CSS** (`wwwroot/css/site.css`) | Bestemmer utseendet: farger, skrift, avstander og tilpasning til mobil. Skrevet uten rammeverk. Fargene ligger som variabler (f.eks. `--navy`) og endres ett sted. |
| **JavaScript** (`wwwroot/js/site.js`) | Gjør siden mer smidig: søkeforslag, karusell, skjema i tre steg, avkrysning av kommuner ved valg av IKT-samarbeid og høyden på iframen. Uten rammeverk. Siden virker også uten JavaScript. |
| **SVG-kart** | Kommunegrensene ligger forenklet i `Data/agder.json`. Serveren tegner én form per kommune og fargelegger den etter antall prosjekter. Ingen kartbibliotek trengs. |
| **JSON-fil eller PostgreSQL** | To forslag til lagring. Uten databasetilkobling lagres alt i én JSON-fil. Med tilkobling brukes PostgreSQL, en database med tabeller som henger sammen og tåler mange samtidige brukere. |
| **Entity Framework Core og Npgsql** | Oversetter mellom C#-klassene og tabellene i databasen, så man slipper å skrive SQL selv. Npgsql er driveren for PostgreSQL. *Migrasjoner* holder databasestrukturen i takt med koden. |
| **Docker Compose** | Starter PostgreSQL lokalt med én kommando, så alle i gruppen har samme database uten å installere noe. |
| **xUnit** | Automatiske tester som sjekker at reglene virker, f.eks. at et prosjekt ikke vises før det er godkjent. Kjøres med `dotnet test`. |
| **Azure App Service** | Microsofts skytjeneste for nettsider. Prototypen kjørte der under workshopen. |
| **iframe** | Et vindu som viser én nettside inni en annen. KI-kartet skal vises inni digiagder.no, og bare digiagder.no får lov til det. |
| **GitHub og VS Code** | GitHub lagrer og deler koden. Endringer gjøres på egne grener og slås sammen med pull requests. VS Code med C# Dev Kit brukes til å skrive og kjøre koden. |

## Kom raskt i gang

Krever .NET 10 SDK (Software Development Kit).

```
dotnet run --project KiKartAgder/KiKartAgder   # starter siden på http://localhost:5270
dotnet test                                    # kjører alle testene
```

I Visual Studio Code holder det å åpne denne mappen og trykke F5.

## Viktig å vite før man bygger videre på prosjektet

- **Lagringen er et forslag.** Appen kan lagre i en JSON-fil eller i PostgreSQL. DigiAgder eller IKT Agder skal etter planen bruke sin egen database. All lesing og skriving går derfor gjennom grensesnittet `IKiKartRepository`, slik at lagringen kan byttes uten å endre resten av koden.
- **Forvaltning har ingen innlogging ennå.** Den må på plass før løsningen publiseres.
- **Personvern:** navn, rolle og e-post til kontaktpersonen vises på prosjektsiden, slik at andre kan ta kontakt. Personvernerklæring og hvem som er behandlingsansvarlig må avklares med DigiAgder før publisering.
- **E-post sendes ikke.** Påminnelser, varsler og henvendelser er bare simulert.
