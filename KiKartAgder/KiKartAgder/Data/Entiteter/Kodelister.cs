namespace KiKartAgder.Data.Entiteter;

// Kodelistene seedes fra Models/Katalog.cs med HasData() i KiKartDbContext.

public class Sektor
{
    public int SektorId { get; set; }
    public string Navn { get; set; } = "";
    public int Rekkefolge { get; set; }
    public bool ErAktiv { get; set; } = true;
}

public class Fase
{
    public int FaseId { get; set; }
    public string Navn { get; set; } = "";
    public int Rekkefolge { get; set; }

    /// <summary>Sann for «Fullført» og «Avbrutt».</summary>
    public bool ErAvsluttet { get; set; }
}

public class Stopparsak
{
    public int StopparsakId { get; set; }
    public string Navn { get; set; } = "";
    public bool ErAktiv { get; set; } = true;
}

public class Behov
{
    public int BehovId { get; set; }
    public string Navn { get; set; } = "";
    public int Rekkefolge { get; set; }
    public bool ErAktiv { get; set; } = true;
}

public class Stikkord
{
    public int StikkordId { get; set; }
    public string Navn { get; set; } = "";

    public List<ProsjektStikkord> Prosjekter { get; set; } = new();
}

/// <summary>IKT-samarbeid i Agder som et prosjekt kan være en del av, f.eks. «IKT Agder» eller «DDV».</summary>
public class IktSamarbeid
{
    public int IktSamarbeidId { get; set; }

    /// <summary>Kortnavnet som vises i kartet, f.eks. «DDV».</summary>
    public string Navn { get; set; } = "";

    /// <summary>Forklaring, f.eks. «Det Digitale Vestre Agder».</summary>
    public string? Beskrivelse { get; set; }

    public int Rekkefolge { get; set; }
    public bool ErAktiv { get; set; } = true;

    public List<ProsjektIktSamarbeid> Prosjekter { get; set; } = new();
}
