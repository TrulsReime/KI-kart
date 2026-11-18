using KiKartAgder.Data;
using KiKartAgder.Models;
using KiKartAgder.Services;
using KiKartAgder.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace KiKartAgder.Controllers;

/// <summary>
/// For DigiAgder: behandling av innmeldinger og påminnelser.
/// I en ferdig løsning skal denne kontrolleren kreve innlogging, f.eks. med [Authorize(Roles = "DigiAgder")].
/// </summary>
public class ForvaltningController : Controller
{
    private readonly IKiKartRepository _repo;
    private readonly IDatoTjeneste _dato;

    public ForvaltningController(IKiKartRepository repo, IDatoTjeneste dato)
    {
        _repo = repo;
        _dato = dato;
    }

    // GET /Forvaltning?fane=venter
    public IActionResult Index(string? fane, int? apen, string? q)
    {
        fane = ForvaltningViewModel.Faner.Any(f => f.Verdi == fane) ? fane! : "ny";
        var innmeldinger = _repo.Innmeldinger();
        var publiserte = _repo.Publiserte();
        var idag = _dato.Idag;

        var statuser = fane switch
        {
            "venter" => new[] { InnmeldingStatus.VenterPaInnsender },
            "ferdig" => new[] { InnmeldingStatus.Godkjent, InnmeldingStatus.Avvist },
            _ => new[] { InnmeldingStatus.Ny }
        };

        var rader = innmeldinger.Where(i => statuser.Contains(i.Status))
            .Select(i => (i, p: _repo.Finn(i.ProsjektId)))
            .Where(x => x.p != null)
            .Select(x => new InnmeldingRad(x.i, x.p!,
                x.i.Type == InnmeldingType.NyttProsjekt ? ProsjektLogikk.MuligeDuplikater(x.p!, publiserte) : new List<Prosjekt>()))
            .ToList();

        // Prosjekter som snart må bekreftes, og som er blitt inaktive
        var paminnelser = publiserte
            .Select(p => (p, a: ProsjektLogikk.Aktualitetsstatus(p, idag)))
            .Where(x => x.a.Niva is ProsjektLogikk.Aktualitet.Inaktiv or ProsjektLogikk.Aktualitet.Snart)
            .OrderBy(x => x.p.Oppdatert)
            .Select(x =>
            {
                var inaktiv = x.a.Niva == ProsjektLogikk.Aktualitet.Inaktiv;
                return new PaminnelseRad(x.p, inaktiv, x.a, ForvaltningViewModel.EpostTekst(x.p, x.a, inaktiv));
            })
            .ToList();

        // Utvalgte til karusellen på forsiden, og prosjekter som kan velges ut
        var utvalgteIder = _repo.UtvalgteIder();
        var utvalgte = utvalgteIder.Select(id => _repo.Finn(id)).Where(p => p != null && !p.TilGodkjenning)
            .Select(p => new UtvalgtRad(p!, ProsjektLogikk.ErInaktiv(p!, idag))).ToList();
        var kandidater = _repo.Aktive().Where(p => !utvalgteIder.Contains(p.Id) && ProsjektLogikk.Treffer(p, q))
            .OrderByDescending(p => p.Oppdatert).ToList();

        var henvendelser = _repo.Henvendelser()
            .Select(h => new HenvendelseRad(h, _repo.Finn(h.ProsjektId)?.Tittel ?? "Ukjent prosjekt")).ToList();

        return View(new ForvaltningViewModel
        {
            Fane = fane,
            Rader = rader,
            Paminnelser = paminnelser,
            Henvendelser = henvendelser,
            Utvalgte = utvalgte,
            Kandidater = kandidater,
            Sok = q,
            AntallNye = innmeldinger.Count(i => i.Status == InnmeldingStatus.Ny),
            AntallVenter = innmeldinger.Count(i => i.Status == InnmeldingStatus.VenterPaInnsender),
            AntallGodkjent = innmeldinger.Count(i => i.Status == InnmeldingStatus.Godkjent),
            AntallBehandlet = innmeldinger.Count(i => i.Status is InnmeldingStatus.Godkjent or InnmeldingStatus.Avvist),
            ApenId = apen
        });
    }

    /// <summary>Godkjenn, be om endring eller avvis. «handling» kommer fra knappen som ble trykket.</summary>
    [HttpPost]
    public IActionResult Behandle(int id, string handling, string? melding, string? fane)
    {
        var i = _repo.Innmeldinger().FirstOrDefault(x => x.Id == id);
        if (i == null) return NotFound();
        var tekst = melding?.Trim() ?? "";

        if (handling != "godkjenn" && tekst.Length == 0)
        {
            TempData["Melding"] = "Skriv en kort melding til innsender";
            return RedirectToAction(nameof(Index), new { fane, apen = id });
        }

        switch (handling)
        {
            case "godkjenn":
                _repo.Godkjenn(id);
                TempData["Melding"] = i.Type == InnmeldingType.NyttProsjekt
                    ? "Godkjent. Prosjektet er publisert med dagens dato"
                    : "Godkjent. Endringen er lagt inn med dagens dato";
                break;
            case "endring":
                _repo.BeOmEndring(id, tekst);
                TempData["Melding"] = "Prototype: e-post med ønske om endring ville gått til innsender";
                break;
            case "avvis":
                _repo.Avvis(id, tekst);
                TempData["Melding"] = "Innmeldingen er avvist, og innsender får beskjed";
                break;
        }
        return RedirectToAction(nameof(Index), new { fane });
    }

    // ---------- utvalgte prosjekter til karusellen på forsiden ----------
    [HttpPost]
    public IActionResult VelgUt(int id, string? q)
    {
        TempData["Melding"] = _repo.VelgUt(id)
            ? "Prosjektet vises nå i karusellen på forsiden"
            : $"Kunne ikke velge ut prosjektet. Det kan være maks {Katalog.MaksUtvalgte} utvalgte samtidig";
        return RedirectToAction(nameof(Index), new { fane = "utvalgte", q });
    }

    [HttpPost]
    public IActionResult FjernUtvalgt(int id)
    {
        if (_repo.FjernUtvalgt(id)) TempData["Melding"] = "Prosjektet er fjernet fra karusellen";
        return RedirectToAction(nameof(Index), new { fane = "utvalgte" });
    }

    [HttpPost]
    public IActionResult FlyttUtvalgt(int id, int retning)
    {
        _repo.FlyttUtvalgt(id, retning);
        return RedirectToAction(nameof(Index), new { fane = "utvalgte" });
    }

    /// <summary>
    /// Sender påminnelse til prosjekteieren om å bekrefte eller oppdatere prosjektet.
    /// I ferdig løsning sendes disse automatisk av en planlagt jobb, og knappen brukes bare ved behov.
    /// </summary>
    [HttpPost]
    public IActionResult Paminnelse(int id)
    {
        var p = _repo.Finn(id);
        if (p == null) return NotFound();
        _repo.MarkerPaminnelseSendt(id);
        TempData["Melding"] = $"Prototype: påminnelse ville blitt sendt til {p.Eierperson.Epost}";
        return RedirectToAction(nameof(Index), new { fane = "pam" });
    }
}
