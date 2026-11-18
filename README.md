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

## Kom raskt i gang

Krever .NET 10 SDK (Software Development Kit).

```
dotnet run --project KiKartAgder/KiKartAgder   # starter siden på http://localhost:5270
dotnet test                                    # kjører alle testene
```

I Visual Studio Code holder det å åpne denne mappen og trykke F5.

## Viktig å vite før du bygger videre

- **Lagringen er et forslag.** Appen kan lagre i en JSON-fil eller i PostgreSQL. DigiAgder eller IKT Agder skal etter planen bruke sin egen database. All lesing og skriving går derfor gjennom grensesnittet `IKiKartRepository`, slik at lagringen kan byttes uten å endre resten av koden.
- **Forvaltning har ingen innlogging ennå.** Den må på plass før løsningen publiseres.
- **Personvern:** navn, rolle og e-post til kontaktpersonen vises på prosjektsiden, slik at andre kan ta kontakt. Personvernerklæring og hvem som er behandlingsansvarlig må avklares med DigiAgder før publisering.
- **E-post sendes ikke.** Påminnelser, varsler og henvendelser er bare simulert.
