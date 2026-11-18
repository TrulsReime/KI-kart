# KI-kart for Agder (ASP.NET Core MVC)

Prototypen av KI-kartet, bygget om til en ASP.NET Core MVC-applikasjon (.NET 10, C#).
Løsningen starter uten data. Prosjektene legges inn via «Meld inn» og godkjennes under «Forvaltning».

## Kom i gang i Visual Studio Code

1. Installer **.NET 10 SDK** (Software Development Kit) fra https://dotnet.microsoft.com/download/dotnet/10.0
2. Åpne mappen `KiKartAgder` i VS Code. Godta forslaget om å installere utvidelsen **C# Dev Kit**.
3. Trykk **F5** (eller velg «KI-kart (start og åpne i nettleser)» under Run and Debug). Siden åpnes på http://localhost:5270

Fra terminalen i VS Code går det også an å skrive:

```
dotnet run
```

eller `dotnet watch run` for å laste siden på nytt automatisk når du endrer filer.

## Mappestruktur

| Mappe | Innhold |
|---|---|
| `Models/` | Domeneklassene: `Prosjekt`, `Innmelding`, `Henvendelse` og kodelistene i `Katalog` (faser, sektorer, stoppårsaker). |
| `Data/` | `IKiKartRepository` (grensesnittet for all datatilgang) med to implementasjoner: `MinneRepository` (JSON-fil) og `EfRepository` (PostgreSQL via Entity Framework Core). Databasetabellene ligger i `Data/Entiteter`, startdataene i `prosjekter.json` og kartet i `agder.json`. |
| `Migrations/` | Endringer i databasestrukturen, laget av Entity Framework Core. Ikke rediger for hånd. |
| `Services/` | Regler som brukes flere steder: søk, partnerforslag, lignende prosjekter, påminnelser (`ProsjektLogikk`), datoformat, kartdata og lenker. |
| `ViewModels/` | Det hver visning trenger, f.eks. `OversiktViewModel`, `ProsjektFilter` og skjemamodellen `MeldInnSkjema` med validering. |
| `Controllers/` | Én kontroller per fane: `Home` (Oversikt), `Prosjekter`, `MeldInn` og `Forvaltning`. `Erfaringer` sender bare gamle lenker videre til prosjektlisten, siden erfaringene nå ligger der. |
| `Views/` | Razor-visningene. Felles deler ligger i `Views/Shared` (meny, prosjektkort, kart, stolpediagram, søkefelt). |
| `wwwroot/` | `css/site.css` (utseendet fra prototypen) og `js/site.js` (karusell, søkeforslag, skjema i steg, iframe-høyde). |

## Slik henger M, V og C sammen

- **Model**: `Prosjekt` og `Innmelding` beskriver dataene. Et repository (`MinneRepository` eller `EfRepository`) henter og lagrer dem.
- **Controller**: tar imot forespørselen, henter data via repository og lager en ViewModel. Eksempel: `ProsjekterController.Index` leser filtrene fra adressen (`/Prosjekter?fase=Pilot&kommune=Arendal`) og sender resultatet til visningen.
- **View**: Razor-filene viser ViewModel-en som HTML. Visningene inneholder ingen forretningslogikk.

Alle filtre ligger i adressen, så et filtrert utvalg kan deles som lenke. Skjema sendes med POST til kontrollerne og valideres på serveren (DataAnnotations i `MeldInnSkjema`). JavaScript gjør siden raskere å bruke, men alt virker også uten.

## Adresser

| Side | Adresse |
|---|---|
| Oversikt | `/` |
| Prosjekter med filter | `/Prosjekter?org=Samarbeid&fase=Pilot` |
| Ett prosjekt | `/Prosjekter/Detaljer/24` |
| Prosjekter med erfaringer | `/Prosjekter?erfaringer=true` |
| Meld inn / oppdater | `/MeldInn`, `/MeldInn?id=12` |
| Forvaltning | `/Forvaltning`, `/Forvaltning?fane=pam` |

## Viktig å vite

- **Hvor dataene lagres** avgjøres i `Program.cs`. Finnes tilkoblingen `ConnectionStrings:KiKart`, brukes databasen. Ellers lagres alt i én JSON-fil: `App_Data/kikart-data.json` lokalt og `%HOME%\data\kikart\kikart-data.json` på Azure App Service. Filen beholdes ved omstart. Begge er forslag, siden DigiAgder eller IKT Agder skal bruke sin egen database.
- **IKT-samarbeid:** hvilke kommuner som er med i hvert samarbeid, står i `Katalog.IktSamarbeidMedlemmer` i `Models/Katalog.cs`. Listen for Digi Agder er ikke bekreftet av DigiAgder.
- **Dato for testing:** `KiKart:Dato` i `appsettings.json` er tom, så løsningen bruker dagens dato. Sett en dato der (f.eks. `2027-10-01`) for å teste påminnelser og inaktive prosjekter.
- **Forvaltning krever ingen innlogging** i prototypen. I en ferdig løsning bør `ForvaltningController` få `[Authorize]`, for eksempel med kommunenes Microsoft Entra ID (tidligere Azure AD).
- **Fonten Inter** hentes fra Google Fonts. Uten nett brukes systemfonten.

## Database lokalt (valgfritt)

1. Lag en fil `KiKartAgder/.env` med `DB_USER` og `DB_PASSWORD`. Filen skal ikke inn i git.
2. Start databasen fra mappen `KiKartAgder`: `docker compose up -d db`
3. Lagre tilkoblingen som User Secret, så passordet ikke havner i koden:
   `dotnet user-secrets set "ConnectionStrings:KiKart" "Host=localhost;Database=KiKart;Username=...;Password=..."`
4. Start appen. Lokalt kjøres migrasjonene automatisk, og testdata fra `prosjekter.json` legges inn hvis databasen er tom. I produksjon må migrasjonene kjøres manuelt med `dotnet ef database update`.

## Tester

Testene ligger i `KiKartAgder.Tests` og kjøres med `dotnet test` fra rotmappen, eller fra Testing-panelet i VS Code.

| Fil | Hva den sikrer |
|---|---|
| `MeldInnSkjemaTester` | Validering i skjemaet og at det blir riktig prosjekt, også med fylkeskommunen eller et IKT-samarbeid som ansvarlig |
| `SokOgFilterTester` | At søket finner tekst i alle feltene, og at filtrene kombineres riktig |
| `AktualitetTester` | Grensene for når et prosjekt blir eldre, må bekreftes og blir inaktivt |
| `KatalogTester` | At medlemslistene for IKT-samarbeid bare har ekte kommunenavn og ikke overlapper |
| `MinneRepositoryTester` | Godkjenning av innmeldinger og at dataene overlever en omstart |

`EfRepository` har ingen tester ennå.

## Veien videre

- **Egen database hos DigiAgder eller IKT Agder:** lag en ny klasse som implementerer `IKiKartRepository` og bytt den inn i `Program.cs`. Kontrollere og visninger trenger ikke endres. `EfRepository` og ER-diagrammet i `docs/` kan brukes som mal. Testene i `MinneRepositoryTester` viser hvordan en implementasjon skal oppføre seg.
- **E-post:** påminnelser og varsler er bare simulert. Koble til en e-posttjeneste for å sende dem.

## iframe på digiagder.no

`Program.cs` setter `Content-Security-Policy: frame-ancestors`, styrt av `KiKart:IframeVerter` i `appsettings.json`. Bare de nettstedene som står der, kan vise kartet i en iframe. I produksjon settes cookies til `SameSite=None; Secure`, så skjemaene virker inne i iframen. Det krever HTTPS (Hypertext Transfer Protocol Secure).

Når siden vises i en iframe, skjules den etterlignede DigiAgder-menyen, og siden sender høyden sin til foreldresiden:

```html
<iframe id="kikart" src="https://ki-kart.example.no/" style="width:100%;border:0" title="KI-kart for Agder"></iframe>
<script>
  addEventListener('message', e => {
    if (e.origin !== 'https://ki-kart.example.no') return; // godta bare meldinger fra KI-kartet
    const f = document.getElementById('kikart');
    if (e.data && e.data.type === 'ki-kart:height') f.style.height = e.data.height + 'px';
    if (e.data && e.data.type === 'ki-kart:scroll-top') f.scrollIntoView({ behavior: 'smooth' });
  });
</script>
```
