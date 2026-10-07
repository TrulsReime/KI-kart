using KiKartAgder.Data;
using KiKartAgder.Services;
using KiKartAgder.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace KiKartAgder.Controllers;

/// <summary>Fanen «Meld inn»: nytt prosjekt eller oppdatering av et eksisterende.</summary>
public class MeldInnController : Controller
{
    private readonly IKiKartRepository _repo;
    private readonly KartData _kart;

    public MeldInnController(IKiKartRepository repo, KartData kart)
    {
        _repo = repo;
        _kart = kart;
    }

    // GET /MeldInn            nytt prosjekt
    // GET /MeldInn?id=12      oppdater prosjekt 12
    [HttpGet]
    public IActionResult Index(int? id)
    {
        var p = id.HasValue ? _repo.Finn(id.Value) : null;
        var skjema = p != null ? MeldInnSkjema.Fra(p) : new MeldInnSkjema { SistEndret = "Lagt inn i kartet" };
        return View(Lag(skjema, 1));
    }

    [HttpPost]
    public IActionResult Index(MeldInnSkjema skjema)
    {
        if (skjema.Id.HasValue && _repo.Finn(skjema.Id.Value) == null) return NotFound();
        if (skjema.Kommune != null && !_kart.Kommuner.Contains(skjema.Kommune))
            ModelState.AddModelError(nameof(skjema.Kommune), "Velg en kommune fra listen");

        if (!ModelState.IsValid)
        {
            // Vis steget der den første feilen er
            var steg = 3;
            for (var i = 0; i < MeldInnSkjema.FeltPerSteg.Length; i++)
                if (MeldInnSkjema.FeltPerSteg[i].Any(f => ModelState.TryGetValue(f, out var e) && e.Errors.Count > 0))
                { steg = i + 1; break; }
            return View(Lag(skjema, steg));
        }

        var prosjekt = skjema.TilProsjekt();
        if (skjema.Id.HasValue)
        {
            _repo.MeldInnOppdatering(skjema.Id.Value, prosjekt, prosjekt.SistEndret, prosjekt.Kontakt.Epost);
            return RedirectToAction(nameof(Takk), new { id = skjema.Id.Value, oppdatering = true });
        }
        var nyId = _repo.MeldInnNytt(prosjekt);
        return RedirectToAction(nameof(Takk), new { id = nyId });
    }

    // GET /MeldInn/Takk/36
    public IActionResult Takk(int id, bool oppdatering = false)
    {
        var p = _repo.Finn(id);
        if (p == null) return RedirectToAction(nameof(Index));
        return View(new TakkViewModel { Prosjekt = p, Oppdatering = oppdatering });
    }

    private MeldInnViewModel Lag(MeldInnSkjema skjema, int steg) => new()
    {
        Skjema = skjema,
        Redigerer = skjema.Id.HasValue ? _repo.Finn(skjema.Id.Value) : null,
        Publiserte = _repo.Publiserte(),
        Kommuner = _kart.Kommuner,
        PopulareStikkord = ProsjektLogikk.Stikkord(_repo.Publiserte()).Select(t => t.Stikkord).Take(12).ToList(),
        StartSteg = steg
    };
}
