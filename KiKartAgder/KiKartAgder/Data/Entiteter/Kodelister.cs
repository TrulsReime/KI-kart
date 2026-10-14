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
