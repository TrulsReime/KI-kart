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
    public IActionResult Index(string? fane, int? apen)
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

        var paminnelser = publiserte
            .Select(p => (p, l: ProsjektLogikk.Livssyklus(p, idag).Status))
            .Where(x => x.l is ProsjektLogikk.Livsstatus.Utlopt or ProsjektLogikk.Livsstatus.Snart)
            .OrderBy(x => x.p.Oppdatert)
            .Select(x => new PaminnelseRad(x.p, x.l == ProsjektLogikk.Livsstatus.Utlopt, ForvaltningViewModel.EpostTekst(x.p)))
            .ToList();

        var henvendelser = _repo.Henvendelser()
            .Select(h => new HenvendelseRad(h, _repo.Finn(h.ProsjektId)?.Tittel ?? "Ukjent prosjekt")).ToList();

        return View(new ForvaltningViewModel
        {
            Fane = fane,
            Rader = rader,
            Paminnelser = paminnelser,
            Henvendelser = henvendelser,
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

    [HttpPost]
    public IActionResult Paminnelse(int id)
    {
        var p = _repo.Finn(id);
        if (p == null) return NotFound();
        _repo.MarkerPaminnelseSendt(id);
        TempData["Melding"] = $"Prototype: påminnelse ville blitt sendt til {p.Kontakt.Epost}";
        return RedirectToAction(nameof(Index), new { fane = "pam" });
    }
}
