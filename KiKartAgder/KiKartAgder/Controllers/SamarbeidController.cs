using KiKartAgder.Data;
using KiKartAgder.Services;
using KiKartAgder.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace KiKartAgder.Controllers;

/// <summary>Fanen «Samarbeid»: hvorfor samarbeide, samarbeidsprosjekter og partnerforslag.</summary>
public class SamarbeidController : Controller
{
    private readonly IKiKartRepository _repo;

    public SamarbeidController(IKiKartRepository repo) => _repo = repo;

    // GET /Samarbeid?valgt=8
    public IActionResult Index(int? valgt)
    {
        var alle = _repo.Aktive();
        var samarbeid = alle.Where(p => p.Samarbeid)
            .OrderByDescending(p => p.ErApenForFlere).ThenByDescending(p => p.Deltakere.Count).ToList();
        var sokere = alle.Where(p => p.ErApenForFlere && p.Soker.Count > 0).OrderByDescending(p => p.Oppdatert).ToList();
        var a = sokere.FirstOrDefault(p => p.Id == valgt) ?? sokere.FirstOrDefault();

        return View(new SamarbeidViewModel
        {
            AntallKommunerISamarbeid = samarbeid.SelectMany(p => p.Deltakere).Distinct().Count(),
            Samarbeidsprosjekter = samarbeid,
            SerEtterNoe = sokere,
            Valgt = a,
            Treff = a == null ? new List<ProsjektLogikk.Partnertreff>() : ProsjektLogikk.Partnere(a, alle)
        });
    }
}
