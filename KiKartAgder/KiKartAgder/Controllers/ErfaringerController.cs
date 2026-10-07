using KiKartAgder.Data;
using KiKartAgder.Models;
using KiKartAgder.Services;
using KiKartAgder.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace KiKartAgder.Controllers;

/// <summary>Fanen «Erfaringer»: hva som fungerte, og hva prosjektene ville gjort annerledes.</summary>
public class ErfaringerController : Controller
{
    private readonly IKiKartRepository _repo;
    private readonly KartData _kart;
    private readonly IDatoTjeneste _dato;

    public ErfaringerController(IKiKartRepository repo, KartData kart, IDatoTjeneste dato)
    {
        _repo = repo;
        _kart = kart;
        _dato = dato;
    }

    private static readonly Dictionary<string, Func<Prosjekt, bool>> StatusFilter = new()
    {
        ["alle"] = _ => true,
        ["aktive"] = p => p.Fase is Katalog.Ide or Katalog.Pilot or Katalog.Drift,
        ["avsluttet"] = p => p.Fase == Katalog.Fullfort,
        ["stoppet"] = p => p.Fase == Katalog.Avbrutt,
    };

    private static readonly Dictionary<string, (int Fra, int Til)> DatoIntervall = new()
    {
        ["30"] = (0, 30), ["180"] = (0, 183), ["365"] = (0, 365), ["old"] = (366, int.MaxValue)
    };

    // GET /Erfaringer?status=stoppet&sektor=Oppvekst
    public IActionResult Index(string? q, string? status, string? sektor, string? kommune, string? dato)
    {
        status = status != null && StatusFilter.ContainsKey(status) ? status : "alle";
        var idag = _dato.Idag;
        var medErfaringer = _repo.Publiserte().Where(p => p.HarErfaringer).ToList();

        bool Ekstra(Prosjekt p)
        {
            if (!string.IsNullOrEmpty(sektor) && p.Sektor != sektor) return false;
            if (!string.IsNullOrEmpty(kommune) && !p.Deltakere.Contains(kommune)) return false;
            if (!string.IsNullOrEmpty(dato) && DatoIntervall.TryGetValue(dato, out var iv))
            {
                var d = Datoformat.DagerSiden(p.Oppdatert, idag);
                if (d < iv.Fra || d > iv.Til) return false;
            }
            return ProsjektLogikk.Treffer(p, q);
        }

        var grunnlag = medErfaringer.Where(Ekstra).ToList();
        return View(new ErfaringerViewModel
        {
            Q = q, Status = status, Sektor = sektor, Kommune = kommune, Dato = dato,
            Liste = grunnlag.Where(StatusFilter[status]).OrderByDescending(p => p.Oppdatert).ToList(),
            AntallPerStatus = StatusFilter.ToDictionary(kv => kv.Key, kv => grunnlag.Count(kv.Value)),
            Kommuner = _kart.Kommuner.Where(k => medErfaringer.Any(p => p.Deltakere.Contains(k))).ToList()
        });
    }
}
