using System.Globalization;

namespace KiKartAgder.Services;

/// <summary>
/// Gir «dagens dato». Eksempeldataene er laget rundt 7. oktober 2026, så prototypen låser datoen
/// via appsettings.json (KiKart:Dato). Tom verdi betyr ekte dagens dato.
/// </summary>
public interface IDatoTjeneste
{
    DateOnly Idag { get; }
}

public class DatoTjeneste : IDatoTjeneste
{
    private readonly DateOnly? _fast;

    public DatoTjeneste(IConfiguration config)
    {
        var verdi = config["KiKart:Dato"];
        if (DateOnly.TryParse(verdi, CultureInfo.InvariantCulture, out var d)) _fast = d;
    }

    public DateOnly Idag => _fast ?? DateOnly.FromDateTime(DateTime.Today);
}

/// <summary>Datoformat på norsk, likt det som brukes i prototypen.</summary>
public static class Datoformat
{
    private static readonly CultureInfo Nb = CultureInfo.GetCultureInfo("nb-NO");

    /// <summary>«26. sep. 2026»</summary>
    public static string Lang(DateOnly d) => d.ToString("d. MMM yyyy", Nb);

    /// <summary>«26. sep.»</summary>
    public static string Kort(DateOnly d) => d.ToString("d. MMM", Nb);

    public static int DagerSiden(DateOnly d, DateOnly idag) => idag.DayNumber - d.DayNumber;
}
