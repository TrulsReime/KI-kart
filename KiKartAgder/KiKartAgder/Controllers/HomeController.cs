using System.ComponentModel.DataAnnotations;
using KiKartAgder.Data;
using KiKartAgder.Models;
using KiKartAgder.Services;
using KiKartAgder.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace KiKartAgder.Controllers;

/// <summary>Forsiden (fanen «Oversikt»).</summary>
public class HomeController : Controller
{
    private readonly IKiKartRepository _repo;
    private readonly KartData _kart;
    private readonly IDatoTjeneste _dato;

    public HomeController(IKiKartRepository repo, KartData kart, IDatoTjeneste dato)
    {
        _repo = repo;
        _kart = kart;
        _dato = dato;
    }

    public IActionResult Index()
    {
        var liste = _repo.Aktive();
        var idag = _dato.Idag;
        var samarbeid = liste.Where(p => p.Samarbeid).ToList();

        var vm = new OversiktViewModel
        {
            Idag = idag,
            AntallProsjekter = liste.Count,
            AntallIDrift = liste.Count(p => p.Fase == Katalog.Drift),
            AntallKommuner = liste.SelectMany(p => p.Deltakere).Distinct().Count(),
            AntallSamarbeid = samarbeid.Count,
            AntallApneSamarbeid = samarbeid.Count(p => p.ErApenForFlere),
            AntallNye = liste.Count(p => Datoformat.DagerSiden(p.Registrert, idag) <= 30),
            Utvalgte = _repo.Utvalgte(),
            Nyeste = liste.OrderByDescending(p => p.Registrert).Take(4).ToList(),
            SistOppdatert = liste.OrderByDescending(p => p.Oppdatert).Take(6).ToList(),
            Kart = KartViewModel.Lag(_kart, liste),
            Faser = Stolper.Lag(liste, "fase", Katalog.Faser),
            Sektorer = Stolper.Lag(liste, "sektor", Katalog.Sektorer)
        };
        return View(vm);
    }

    /// <summary>Påmelding til e-postvarsel. I prototypen lagres ingenting.</summary>
    [HttpPost]
    public IActionResult Folg(string? epost)
    {
        if (string.IsNullOrWhiteSpace(epost) || !new EmailAddressAttribute().IsValid(epost))
            TempData["Melding"] = "Skriv inn en gyldig e-postadresse";
        else
            TempData["Melding"] = $"Prototype: {epost.Trim()} ville fått varsel om nye prosjekter";
        return Redirect(Url.Action(nameof(Index))! + "#varsel");
    }

    public IActionResult Feil() => View();
}
