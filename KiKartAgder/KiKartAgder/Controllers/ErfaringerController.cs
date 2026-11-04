using Microsoft.AspNetCore.Mvc;

namespace KiKartAgder.Controllers;

/// <summary>
/// Erfaringer er flyttet inn i Prosjekter. Gamle lenker til /Erfaringer sendes videre til
/// prosjektlisten med filteret «Har delt erfaringer».
/// </summary>
public class ErfaringerController : Controller
{
    // GET /Erfaringer?q=personvern&sektor=Oppvekst&kommune=Arendal
    public IActionResult Index(string? q, string? sektor, string? kommune)
    {
        var verdier = new Dictionary<string, object?> { ["erfaringer"] = true };
        if (!string.IsNullOrWhiteSpace(q)) verdier["q"] = q;
        if (!string.IsNullOrWhiteSpace(sektor)) verdier["sektor"] = sektor;
        if (!string.IsNullOrWhiteSpace(kommune)) verdier["kommune"] = kommune;
        return RedirectToActionPermanent("Index", "Prosjekter", verdier);
    }
}
