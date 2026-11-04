namespace KiKartAgder.Data.Entiteter;

/// <summary>En kommune som er med i et prosjekt.</summary>
public class ProsjektDeltaker
{
    public int ProsjektId { get; set; }
    public Prosjekt Prosjekt { get; set; } = null!;

    public string KommuneNr { get; set; } = "";
    public Kommune Kommune { get; set; } = null!;

    public bool ErLeder { get; set; }
    public DateOnly? BleMed { get; set; }

    /// <summary>Rekkefølgen kommunene vises i. Lederkommunen er først.</summary>
    public int Rekkefolge { get; set; }
}

public class ProsjektStikkord
{
    public int ProsjektId { get; set; }
    public Prosjekt Prosjekt { get; set; } = null!;

    public int StikkordId { get; set; }
    public Stikkord Stikkord { get; set; } = null!;

    /// <summary>Rekkefølgen innsenderen skrev stikkordene i. Kartet viser ofte bare de første.</summary>
    public int Rekkefolge { get; set; }
}

/// <summary>Noe prosjektet søker eller tilbyr. Samme behov kan stå med begge retninger.</summary>
public class ProsjektBehov
{
    public int ProsjektId { get; set; }
    public Prosjekt Prosjekt { get; set; } = null!;

    public int BehovId { get; set; }
    public Behov Behov { get; set; } = null!;

    public BehovRetning Retning { get; set; }

    /// <summary>Rekkefølgen innenfor samme retning.</summary>
    public int Rekkefolge { get; set; }
}

public class ProsjektIktSamarbeid
{
    public int ProsjektId { get; set; }
    public Prosjekt Prosjekt { get; set; } = null!;

    public int IktSamarbeidId { get; set; }
    public IktSamarbeid IktSamarbeid { get; set; } = null!;

    public int Rekkefolge { get; set; }
}
