# ER-diagram for KI-kart Agder

Dette dokumentet viser hvordan dataene i KI-kartet henger sammen, først slik de ligger i prototypen i dag, og så hvordan de bør ligge i en database. ER står for entitet–relasjon (entity–relationship).

Diagrammene er skrevet i Mermaid, som GitHub viser som figurer direkte i nettleseren.

## 1. Prototypen i dag

I prototypen ligger alt i minnet og leses inn fra `Data/prosjekter.json`. Det er bare fire klasser. Mye av informasjonen ligger som tekst eller lister inne i `Prosjekt`, for eksempel sektor, fase, deltakerkommuner og stikkord.

```mermaid
erDiagram
    PROSJEKT ||--o{ INNMELDING : "får"
    PROSJEKT ||--o{ HENVENDELSE : "får"
    PROSJEKT ||--|| KONTAKTPERSON : "har kontakt og eier (innebygd)"
    PROSJEKT ||--o{ LOGGOPPFORING : "har (innebygd liste)"

    PROSJEKT {
        int Id PK
        string Tittel
        string Eier "fritekst, f.eks. Arendal kommune"
        string Kommune "plassering i kartet"
        list Deltakere "liste med kommunenavn"
        string Sektor "tekst fra Katalog"
        string Fase "tekst fra Katalog"
        string Stopp "tekst, bare ved Avbrutt"
        list Stikkord "liste med tekst"
        list IktSamarbeid "IKT Agder, DDV, Digi Agder"
        list Soker "liste med behov"
        list Tilbyr "liste med behov"
        bool Apen
        string Mal_Beskrivelse_Status_osv "flere tekstfelt"
        string Fungerte
        string Annerledes
        date Registrert
        date Oppdatert
        string SistEndret
        bool TilGodkjenning
        date PaminnelseSendt
    }
    KONTAKTPERSON {
        string Navn
        string Rolle
        string Epost
    }
    LOGGOPPFORING {
        date Dato
        string Tekst
    }
    INNMELDING {
        int Id PK
        string Type "NyttProsjekt, Oppdatering, Rettelse, BliMed"
        int ProsjektId FK
        date Dato
        string Fra
        string Tekst
        string Kommune "bare ved BliMed"
        object NyeVerdier "en hel kopi av Prosjekt"
        string Status "Ny, VenterPaInnsender, Godkjent, Avvist"
        string Kommentar
        date BehandletDato
    }
    HENVENDELSE {
        int ProsjektId FK
        string Fra
        string Melding
        date Dato
    }
```

Dette er greit for en prototype, men gir problemer i en ekte løsning:

- **Samme verdi skrives på mange måter.** «Arendal kommune» i `Eier` og «Arendal» i `Deltakere` er ikke koblet sammen. En skrivefeil gir en ny «kommune».
- **Lister inne i én rad kan ikke telles godt i Power BI.** Spørsmål som «hvor mange prosjekter har Vennesla med i samarbeid?» krever egne koblingstabeller.
- **Kodelistene ligger i C#-koden** (`Katalog.cs`). En ny sektor krever ny versjon av programmet.

## 2. Fremtidig database

Her er kodelistene egne tabeller, og alle «mange til mange»-forhold har egne koblingstabeller. Tabeller merket med «ny» finnes ikke i prototypen.

```mermaid
erDiagram
    KOMMUNE ||--o{ ORGANISASJON : "kan være"
    ORGANISASJON ||--o{ PROSJEKT : "er ansvarlig for"
    BRUKER ||--o{ PROSJEKT : "eier"
    BRUKER ||--o{ INNMELDING : "sender inn"
    PROSJEKT ||--o{ PAMINNELSE : "får"
    BRUKER ||--o{ PAMINNELSE : "mottar"
    ORGANISASJON ||--o{ KONTAKTPERSON : "ansetter"
    KONTAKTPERSON ||--o{ PROSJEKT : "er kontakt for"
    KOMMUNE ||--o{ PROSJEKT : "plassering i kartet"
    SEKTOR ||--o{ PROSJEKT : "tilhører"
    FASE ||--o{ PROSJEKT : "er i"
    STOPPARSAK |o--o{ PROSJEKT : "forklarer avbrudd"

    PROSJEKT ||--|{ PROSJEKT_DELTAKER : "har"
    KOMMUNE ||--o{ PROSJEKT_DELTAKER : "deltar i"
    PROSJEKT ||--o{ PROSJEKT_STIKKORD : "har"
    STIKKORD ||--o{ PROSJEKT_STIKKORD : "brukes i"
    PROSJEKT ||--o{ PROSJEKT_IKT_SAMARBEID : "er del av"
    IKT_SAMARBEID ||--o{ PROSJEKT_IKT_SAMARBEID : "brukes i"
    PROSJEKT ||--o{ PROSJEKT_BEHOV : "søker eller tilbyr"
    BEHOV ||--o{ PROSJEKT_BEHOV : "brukes i"

    PROSJEKT ||--o{ LOGGOPPFORING : "har historikk"
    PROSJEKT ||--o{ INNMELDING : "får"
    KOMMUNE |o--o{ INNMELDING : "vil bli med"
    BRUKER |o--o{ INNMELDING : "behandler"
    ORGANISASJON ||--o{ BRUKER : "ansetter"
    PROSJEKT ||--o{ HENVENDELSE : "får"

    KOMMUNE {
        char4 kommune_nr PK "SSB-nummer, f.eks. 4204"
        string navn
        string fylke
    }
    ORGANISASJON {
        int org_id PK "ny"
        string navn "Arendal kommune, Agder fylkeskommune"
        string type "kommune, fylkeskommune, annet"
        char4 kommune_nr FK "tom for fylkeskommunen"
    }
    KONTAKTPERSON {
        int kontakt_id PK
        string navn
        string rolle
        string epost UK
        int org_id FK
    }
    SEKTOR {
        int sektor_id PK
        string navn UK
    }
    FASE {
        int fase_id PK
        string navn UK "Idé, Pilot, Drift, Fullført, Avbrutt"
        int rekkefolge
        bool er_avsluttet
    }
    STOPPARSAK {
        int stopparsak_id PK
        string navn UK
    }
    BEHOV {
        int behov_id PK
        string navn UK "Samarbeidspartner, Data, Kompetanse ..."
    }
    IKT_SAMARBEID {
        int ikt_samarbeid_id PK "ny"
        string kortnavn UK "IKT Agder, DDV, Digi Agder"
        string navn "Det Digitale Vestre Agder"
    }
    PROSJEKT_IKT_SAMARBEID {
        int prosjekt_id PK, FK
        int ikt_samarbeid_id PK, FK
    }
    STIKKORD {
        int stikkord_id PK
        string navn UK
    }
    PROSJEKT {
        int prosjekt_id PK
        string tittel
        text beskrivelse
        text mal
        text status_naa
        text neste_steg
        text teknologi
        text gevinst
        text fungerte
        text annerledes
        int ansvarlig_org_id FK
        int eier_bruker_id FK "den som meldte inn"
        char4 kart_kommune_nr FK
        int kontakt_id FK
        int sektor_id FK
        int fase_id FK
        int stopparsak_id FK "bare ved Avbrutt"
        bool apen_for_flere
        string publiseringsstatus "til_godkjenning, publisert, avvist"
        int utvalgt_rekkefolge "tom = ikke i karusellen"
        date registrert
        date oppdatert
        string sist_endret
    }
    PROSJEKT_DELTAKER {
        int prosjekt_id PK, FK
        char4 kommune_nr PK, FK
        bool er_leder
        date ble_med
    }
    PROSJEKT_STIKKORD {
        int prosjekt_id PK, FK
        int stikkord_id PK, FK
    }
    PROSJEKT_BEHOV {
        int prosjekt_id PK, FK
        int behov_id PK, FK
        string retning PK "soker eller tilbyr"
    }
    LOGGOPPFORING {
        int logg_id PK
        int prosjekt_id FK
        date dato
        string tekst
    }
    INNMELDING {
        int innmelding_id PK
        string type "nytt_prosjekt, oppdatering, rettelse, bli_med"
        int prosjekt_id FK
        date dato
        string fra_navn
        string fra_epost
        text tekst
        char4 kommune_nr FK "bare ved bli_med"
        json nye_verdier "bare ved oppdatering"
        string status "ny, venter, godkjent, avvist"
        text kommentar
        date behandlet_dato
        int innsendt_av FK "bruker, blir eier ved nytt prosjekt"
        int behandlet_av FK
    }
    PAMINNELSE {
        int paminnelse_id PK "ny"
        int prosjekt_id FK
        int bruker_id FK "eieren som fikk den"
        date sendt
    }
    HENVENDELSE {
        int henvendelse_id PK
        int prosjekt_id FK
        string fra
        text melding
        date dato
    }
    BRUKER {
        int bruker_id PK "ny"
        string navn
        string epost UK
        int org_id FK
        string rolle "innmelder eller forvalter"
    }
    VARSELABONNENT {
        int abonnent_id PK "ny"
        string epost UK
        date opprettet
        date avmeldt
    }
```

`VARSELABONNENT` står alene. Den er knyttet til knappen «Følg» på forsiden, ikke til et bestemt prosjekt.

## 3. Fra prototype til database

| I prototypen | I databasen |
|---|---|
| `Prosjekt.Eier` (tekst, ansvarlig kommune) | `ORGANISASJON`, koblet med `ansvarlig_org_id` |
| `Prosjekt.Prosjekteier` (den som meldte inn) | `BRUKER`, koblet med `eier_bruker_id` |
| `PaminnelseSendt` | `PAMINNELSE`, én rad per utsendelse |
| Aktiv eller inaktiv (regnes ut) | Lagres ikke. Regnes ut fra `oppdatert` |
| `Prosjekt.Kommune` (tekst) | `kart_kommune_nr` mot `KOMMUNE` |
| `Prosjekt.Deltakere` (liste) | `PROSJEKT_DELTAKER`, med `er_leder` for kommunen som leder |
| `Prosjekt.Sektor`, `Fase`, `Stopp` (tekst) | `SEKTOR`, `FASE`, `STOPPARSAK` |
| `Prosjekt.Stikkord` (liste) | `STIKKORD` og `PROSJEKT_STIKKORD` |
| `Prosjekt.IktSamarbeid` (liste, valgfri) | `IKT_SAMARBEID` og `PROSJEKT_IKT_SAMARBEID` |
| `Prosjekt.Soker` og `Tilbyr` (to lister) | `PROSJEKT_BEHOV` med `retning` |
| `Prosjekt.Kontakt` (innebygd) | `KONTAKTPERSON`, kan være kontakt for flere prosjekter |
| `Prosjekt.TilGodkjenning` (ja/nei) | `publiseringsstatus` |
| `Utvalgte` i JSON-filen | `utvalgt_rekkefolge` på prosjektet |
| `Prosjekt.Samarbeid` (regnes ut) | Lagres ikke. Regnes ut som flere enn én rad i `PROSJEKT_DELTAKER` |
| `Katalog.cs` | Kodelistetabellene |
| Finnes ikke | `VARSELABONNENT` |

## 4. Eierskap og aktualitet

- **Den som melder inn, blir eier.** Ved et nytt prosjekt settes `eier_bruker_id` til brukeren som sendte innmeldingen (`innsendt_av`). Eieren kan oppdatere og bekrefte prosjektet og får påminnelser. `KONTAKTPERSON` er den som vises utad, og kan være en annen person.
- **Eier og ansvarlig er to forskjellige ting.** `ansvarlig_org_id` er kommunen eller virksomheten som står bak. `eier_bruker_id` er personen som har ansvaret for at informasjonen er riktig.
- **Fargen ved «sist oppdatert»** regnes ut fra `oppdatert`: grønn de første 6 månedene, gul fra 6 til 11 måneder og oransje den siste måneden før fristen.
- **Påminnelse.** Etter 11 måneder får eieren en påminnelse om å bekrefte eller oppdatere prosjektet. DigiAgder kan også sende den manuelt, for eksempel til eiere av inaktive prosjekter. Hver utsendelse lagres i `PAMINNELSE`, så DigiAgder kan se hvem som har fått beskjed og når.
- **Inaktiv lagres ikke.** Et prosjekt er inaktivt når det har gått mer enn 12 måneder siden `oppdatert`, og det ikke er fullført eller avbrutt. Når eieren bekrefter eller oppdaterer, flyttes `oppdatert` til dagens dato, og prosjektet er aktivt igjen. Inaktive prosjekter tas ikke med på oversikten, i kartet eller i Power BI-tallene, men erfaringene deres er fortsatt søkbare.

## 5. Valg og ting å diskutere

- **IKT-samarbeid er valgfritt** og kan være flere. Derfor en koblingstabell, ikke et felt på prosjektet. Hvilke kommuner som er med i hvert IKT-samarbeid, er ikke lagret. Det kan legges til som en egen koblingstabell mellom `KOMMUNE` og `IKT_SAMARBEID` hvis Power BI skal vise det.
- **Eierskap når folk slutter.** Når eieren slutter i kommunen, får ingen påminnelsene. DigiAgder bør kunne overføre eierskap til en annen bruker, og kommunen bør kanskje ha en fast reserve.
- **`samarbeid` lagres ikke.** Det følger av antall deltakere. Lagres det i tillegg, kan de to verdiene bli uenige.
- **`nye_verdier` er JSON** (JavaScript Object Notation). En oppdatering kan endre mange felt, også lister. Det er enklere å lagre hele forslaget samlet til det godkjennes, enn å lage egne tabeller for forslag. Ulempen er at databasen ikke sjekker innholdet.
- **Kommunenummer som nøkkel.** SSB-numrene (Statistisk sentralbyrå) er standarden Power BI og andre kilder bruker. De endres ved kommunesammenslåinger, og da må tabellen oppdateres.
- **Personopplysninger.** Navn og e-post til kontaktpersoner og innsendere er personopplysninger etter GDPR (personvernforordningen). Det må være klart hvor lenge de lagres, og avviste innmeldinger og avmeldte abonnenter bør slettes etter en fast tid.
- **Power BI.** Power BI bør lese fra denne databasen, ikke være databasen. Koblingstabellene gjør at deltakere, stikkord og behov kan telles og filtreres direkte i Power BI.
- **Endringshistorikk.** `LOGGOPPFORING` viser bare en kort tekst om hva som ble endret. Trenger DigiAgder å vite nøyaktig hvilke felt som ble endret og av hvem, må det lages en mer detaljert historikktabell.
