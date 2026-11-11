# KI-kart for Agder

KI-kartet gir en oversikt over KI-prosjekter i kommunene i Agder. Kommunene melder inn prosjekter via et skjema, DigiAgder godkjenner dem under «Forvaltning», og godkjente prosjekter vises i kartet, i prosjektlisten og under «Erfaringer». Løsningen vises i en iframe på digiagder.no.

**Status:** prototype. Se [Mangler før produksjon](#mangler-før-produksjon).

**Teknologi:** ASP.NET Core MVC (.NET 10, C#), Razor-visninger, Entity Framework Core med PostgreSQL. Vanlig CSS og JavaScript, uten rammeverk eller byggesteg.

---

## Kom i gang

### Det du trenger

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (for databasen)
- VS Code med utvidelsen **C# Dev Kit** (eller Visual Studio / Rider)

### 1. Start databasen

```bash
cd KiKartAgder
cp .env.example .env        # endre passordet i .env
docker compose up -d db     # PostgreSQL 16 på localhost:5432
```

### 2. Legg inn tilkoblingen (User Secrets)

Bruk samme brukernavn og passord som i `.env`:

```bash
cd KiKartAgder/KiKartAgder
dotnet user-secrets set "ConnectionStrings:KiKart" "Host=localhost;Port=5432;Database=KiKart;Username=kikart;Password=<passordet ditt>"
```

### 3. Kjør appen

Trykk **F5** i VS Code, eller:

```bash
dotnet watch run
```

Siden åpnes på http://localhost:5270.

I utviklingsmodus kjører appen nye migrasjoner automatisk ved oppstart. Er databasen tom, legges testdata inn fra `Data/prosjekter.json`.

### Uten database

Hopper du over steg 1 og 2, bruker appen `MinneRepository`. Da lagres alt i JSON-filen `App_Data/kikart-data.json`, som lages fra `Data/prosjekter.json` første gang. Slett filen for å starte på nytt. Loggen ved oppstart viser hvilken lagring som brukes.

---

## Struktur

```
KI-kart/
├── docs/ER-diagram.md          Datamodellen (Mermaid), prototype og database
├── dotnet-tools.json           dotnet-ef (kjør «dotnet tool restore»)
└── KiKartAgder/
    ├── compose.yaml            PostgreSQL (+ app-container)
    ├── .env.example            Brukernavn og passord til databasen
    └── KiKartAgder/            Selve applikasjonen
        ├── Program.cs          Oppsett: velger lagring, cookies og iframe-regler
        ├── Controllers/        Én per fane: Home, Prosjekter, Erfaringer, MeldInn, Forvaltning
        ├── Models/             Domeneklassene som kontrollere og visninger bruker, og kodelister (Katalog)
        ├── ViewModels/         Det hver side trenger, og skjemaet MeldInnSkjema med validering
        ├── Views/              Razor. Felles deler i Views/Shared
        ├── Services/           Regler: søk, duplikater, inaktivitet (ProsjektLogikk), dato, lenker, kartdata
        ├── Data/
        │   ├── IKiKartRepository.cs   All lesing og skriving går hit
        │   ├── EfRepository.cs        Databasen (oversetter Entiteter ↔ Models)
        │   ├── MinneRepository.cs     Reserve: minne + JSON-fil
        │   ├── KiKartDbContext.cs     EF-oppsett
        │   ├── Entiteter/             Databasetabellene
        │   ├── Testdata.cs            Legger inn prosjekter.json i en tom database
        │   ├── prosjekter.json        Testdata
        │   └── agder.json             Kommunegrensene i kartet
        ├── Migrations/         EF-migrasjoner
        └── wwwroot/            css/site.css og js/site.js
```

### Slik henger det sammen

```
Nettleser → Controller → IKiKartRepository (+ ProsjektLogikk) → ViewModel → Razor-visning
```

- **Kontrollerne kjenner bare `IKiKartRepository`.** `Program.cs` velger `EfRepository` hvis `ConnectionStrings:KiKart` er satt, og ellers `MinneRepository`.
- **To modeller:**
  - `Data/Entiteter` er tabellene i databasen, normalisert med kodelister og koblingstabeller.
  - `Models` er den flate formen som visningene bruker.
  - `EfRepository` oversetter mellom dem. Derfor trengte ikke visningene å endres da databasen kom.
- **Filtre ligger i adressen** (`/Prosjekter?fase=Pilot&kommune=Arendal`), så et utvalg kan deles som lenke.
- **Skjemaene er vanlige POST-skjema** som valideres på serveren. Antiforgery-token sjekkes på alle.
- **Alt virker uten JavaScript.** `site.js` legger bare til forbedringer: karusell, søkeforslag, skjema i steg og iframe-høyde.

Datamodellen er beskrevet i [docs/ER-diagram.md](docs/ER-diagram.md).

---

## Sider

| Side | Adresse |
|---|---|
| Oversikt | `/` |
| Prosjekter med filter | `/Prosjekter?org=Samarbeid&fase=Pilot` |
| Ett prosjekt | `/Prosjekter/Detaljer/24` |
| Erfaringer | `/Erfaringer?status=stoppet` |
| Meld inn / oppdater | `/MeldInn`, `/MeldInn?id=12` |
| Forvaltning (DigiAgder) | `/Forvaltning`, `/Forvaltning?fane=pam` |

---

## Vanlige oppgaver

**Endre databasen:** endre klassene i `Data/Entiteter` og/eller `KiKartDbContext`, og lag en migrasjon:

```bash
dotnet tool restore                          # fra rotmappen, første gang
cd KiKartAgder/KiKartAgder
dotnet ef migrations add BeskrivendeNavn
```

Migrasjonen kjøres automatisk neste gang appen starter i utviklingsmodus.

**Nullstille databasen:** `docker compose down -v` i `KiKartAgder/` sletter volumet. Start på nytt for å få ferske testdata.

**Teste påminnelser og inaktive prosjekter:** sett `KiKart:Dato` i `appsettings.json` (f.eks. `2027-10-01`). Da later appen som om det er den datoen. Er feltet tomt, brukes dagens dato.

**Endre kodelister** (faser, sektorer, stoppårsaker): de ligger både i `Models/Katalog.cs` og i databasetabellene (`Data/Entiteter/Kodelister.cs` + migrasjon). Begge stedene må endres.

**Endre kommuner:** `Data/agder.json` og tabellen `Kommuner` må ha samme kommunenummer og navn. Appen logger en advarsel ved oppstart hvis de er ulike.

---

## iframe på digiagder.no

`Program.cs` setter `Content-Security-Policy: frame-ancestors` ut fra `KiKart:IframeVerter` i `appsettings.json`. Bare nettstedene som står der, kan vise kartet. Utenfor utviklingsmodus settes cookies til `SameSite=None; Secure`, så skjemaene virker i iframen. Det krever HTTPS.

Inne i iframen skjules DigiAgder-menyen, og siden sender høyden sin til foreldresiden:

```html
<iframe id="kikart" src="https://ki-kart.example.no/" style="width:100%;border:0" title="KI-kart for Agder"></iframe>
<script>
  addEventListener('message', e => {
    if (e.origin !== 'https://ki-kart.example.no') return;
    const f = document.getElementById('kikart');
    if (e.data?.type === 'ki-kart:height') f.style.height = e.data.height + 'px';
    if (e.data?.type === 'ki-kart:scroll-top') f.scrollIntoView({ behavior: 'smooth' });
  });
</script>
```

---

## Mangler før produksjon

- [ ] **Innlogging på Forvaltning.** `ForvaltningController` er åpen for alle. Legg på `[Authorize]`, f.eks. med Microsoft Entra ID.
- [ ] **Personvern.** Kontaktpersonens navn og e-post vises uten at samtykke er hentet inn (TODO i `EfRepository.cs`).
- [ ] **E-post.** Påminnelser og beskjeder til innsender er bare simulert.
- [ ] **Docker for appen.** `Dockerfile` bruker Windows-images (`nanoserver`) og bygger ikke på Mac/Linux. App-tjenesten i `compose.yaml` får ingen tilkoblingsstreng, og bruker derfor JSON-fil.
- [ ] **Migrasjoner i produksjon.** De kjøres bare automatisk i utviklingsmodus. Velg en løsning for drift.
- [ ] **Tester og CI.** Det finnes ingen tester ennå. `Services/ProsjektLogikk.cs` er et godt sted å starte.
- [ ] **Faste pakkeversjoner.** `.csproj` bruker `10.*`, som kan gi ulike versjoner fra bygg til bygg.
- [ ] **`postMessage` til `'*'`** i `site.js` bør begrenses til digiagder.no.
