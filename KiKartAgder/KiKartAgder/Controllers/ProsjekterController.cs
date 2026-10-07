using System.Globalization;
using KiKartAgder.Data;
using KiKartAgder.Models;
using KiKartAgder.Services;
using KiKartAgder.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace KiKartAgder.Controllers;

/// <summary>Prosjektlisten med filtre, prosjektsidene og skjemaene på dem.</summary>
public class ProsjekterController : Controller
{
    private readonly IKiKartRepository _repo;
    private readonly KartData _kart;
    private readonly IDatoTjeneste _dato;

    public ProsjekterController(IKiKartRepository repo, KartData kart, IDatoTjeneste dato)
    {
        _repo = repo;
        _kart = kart;
        _dato = dato;
    }

    // GET /Prosjekter?kommune=Arendal&fase=Pilot&q=chatbot
    public IActionResult Index([FromQuery] ProsjektFilter filter)
    {
        var alle = _repo.Publiserte();
        var resultat = filter.Sorter(alle.Where(filter.Passer)).ToList();

        int Antall(Func<Prosjekt, bool> f) => alle.Count(f);
        var nb = StringComparer.Create(CultureInfo.GetCultureInfo("nb-NO"), false);

        var kommuner = _kart.Kommuner.Select(k => new FilterValg(k, k, Antall(p => p.Deltakere.Contains(k)), filter.Kommune.Contains(k))).ToList();
        if (filter.Ksort == "antall") kommuner = kommuner.OrderByDescending(v => v.Antall).ThenBy(v => v.Verdi, nb).ToList();

        var grupper = new List<FilterGruppe>
        {
            new("Organisering", "org", new()
            {
                new("Samarbeid", "Samarbeid mellom kommuner", Antall(p => p.Samarbeid), filter.Org.Contains("Samarbeid")),
                new("Enkeltkommune", "Enkeltkommune", Antall(p => !p.Samarbeid), filter.Org.Contains("Enkeltkommune")),
            }, true),
            new("Kommune", "kommune", kommuner, filter.Kommune.Count > 0),
            new("Sektor", "sektor", Katalog.Sektorer.Select(s => new FilterValg(s, s, Antall(p => p.Sektor == s), filter.Sektor.Contains(s))).ToList(), true),
            new("Fase", "fase", Katalog.Faser.Select(f => new FilterValg(f, f, Antall(p => p.Fase == f), filter.Fase.Contains(f))).ToList(), true),
            new("Stikkord", "stikkord", ProsjektLogikk.Stikkord(alle).Select(t => new FilterValg(t.Stikkord, "#" + t.Stikkord, t.Antall, filter.Stikkord.Contains(t.Stikkord))).ToList(), filter.Stikkord.Count > 0),
        };

        return View(new ProsjekterViewModel { Filter = filter, Resultat = resultat, Grupper = grupper });
    }

    // GET /Prosjekter/Detaljer/5?tilbake=/Samarbeid
    public IActionResult Detaljer(int id, string? tilbake, string? apne)
    {
        var p = _repo.Finn(id);
        if (p == null) return NotFound();
        var tilbakeUrl = Lenker.TryggTilbake(tilbake);
        var alle = _repo.Publiserte();
        return View(new ProsjektDetaljViewModel
        {
            Prosjekt = p,
            Lignende = ProsjektLogikk.Lignende(p, alle),
            KommunerSomKanBliMed = _kart.Kommuner.Where(k => !p.Deltakere.Contains(k)).ToList(),
            TilbakeUrl = tilbakeUrl,
            TilbakeTekst = Lenker.TilbakeTekst(tilbakeUrl),
            Apne = apne,
            Livssyklus = ProsjektLogikk.Livssyklus(p, _dato.Idag)
        });
    }

    // GET /Prosjekter/Forslag?q=tale  (brukes av søkefeltet for å vise forslag mens man skriver)
    public IActionResult Forslag(string? q)
    {
        var sok = (q ?? "").Trim();
        if (sok.Length < 2) return Json(Array.Empty<object>());
        var ql = sok.ToLowerInvariant().TrimStart('#');
        var treff = _repo.Publiserte().Where(p => ProsjektLogikk.Treffer(p, sok))
            .OrderByDescending(p => p.Tittel.ToLowerInvariant().Contains(ql))
            .Take(6)
            .Select(p => new
            {
                id = p.Id,
                tittel = p.Tittel,
                info = $"{p.Eier} · {p.Fase}" + (p.Stopp != null ? " · " + p.Stopp.ToLowerInvariant() : "")
            });
        return Json(treff);
    }

    [HttpPost]
    public IActionResult Rettelse(int id, string? tekst, string? fra, string? tilbake)
    {
        if (_repo.Finn(id) == null) return NotFound();
        if (string.IsNullOrWhiteSpace(tekst))
            TempData["Melding"] = "Skriv hva som bør rettes";
        else
        {
            _repo.ForeslaRettelse(id, tekst.Trim(), fra?.Trim());
            TempData["Melding"] = "Takk! Rettelsen er sendt til DigiAgder";
        }
        return TilbakeTilProsjekt(id, tilbake);
    }

    [HttpPost]
    public IActionResult Henvendelse(int id, string? fra, string? melding, string? tilbake)
    {
        var p = _repo.Finn(id);
        if (p == null) return NotFound();
        if (string.IsNullOrWhiteSpace(fra))
        {
            TempData["Melding"] = "Skriv inn navn og kommune";
            return TilbakeTilProsjekt(id, tilbake, "samarbeid");
        }
        var m = (melding ?? "").Trim();
        _repo.SendHenvendelse(id, fra.Trim(), m.Length > 80 ? m[..80] : m);
        TempData["Melding"] = $"Prototype: henvendelsen ville gått til {p.Kontakt.Epost}";
        return TilbakeTilProsjekt(id, tilbake);
    }

    [HttpPost]
    public IActionResult BliMed(int id, string? kommune, string? navn, string? melding, string? tilbake)
    {
        var p = _repo.Finn(id);
        if (p == null) return NotFound();
        if (string.IsNullOrWhiteSpace(kommune) || string.IsNullOrWhiteSpace(navn) || !_kart.Kommuner.Contains(kommune))
        {
            TempData["Melding"] = "Velg kommune og skriv navn";
            return TilbakeTilProsjekt(id, tilbake, "blimed");
        }
        var m = (melding ?? "").Trim();
        _repo.BeOmABliMed(id, kommune, navn.Trim(), m.Length > 80 ? m[..80] : m);
        TempData["Melding"] = $"Forespørselen er sendt til {p.Kontakt.Navn} og DigiAgder";
        return TilbakeTilProsjekt(id, tilbake);
    }

    private IActionResult TilbakeTilProsjekt(int id, string? tilbake, string? apne = null)
    {
        var url = $"{Lenker.DetaljSti}/{id}?tilbake={Uri.EscapeDataString(Lenker.TryggTilbake(tilbake))}";
        if (apne != null) url += "&apne=" + apne;
        return Redirect(url);
    }
}
