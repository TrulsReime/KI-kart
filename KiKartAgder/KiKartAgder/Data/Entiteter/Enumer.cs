namespace KiKartAgder.Data.Entiteter;

// Lagres som tekst i databasen (settes opp i KiKartDbContext).
// InnmeldingType og InnmeldingStatus gjenbrukes fra Models.

public enum OrganisasjonType
{
    Kommune,
    Fylkeskommune,
    Annet
}

public enum Publiseringsstatus
{
    TilGodkjenning,
    Publisert,
    Avvist
}

public enum BehovRetning
{
    Soker,
    Tilbyr
}
