using AkademieTrutnov.Core.Entities;
using AkademieTrutnov.Core.Enums;
using AkademieTrutnov.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AkademieTrutnov.Web.Pages;

public class IndexModel : PageModel
{
    private readonly IKurzRepository _kurzRepo;
    private readonly ITerminRepository _terminRepo;
    private readonly IPrihlaskaRepository _prihlaskaRepo;
    private readonly IOsvedceniRepository _osvedceniRepo;
    private readonly IRegistrationService _regService;

    public IndexModel(
        IKurzRepository kurzRepo,
        ITerminRepository terminRepo,
        IPrihlaskaRepository prihlaskaRepo,
        IOsvedceniRepository osvedceniRepo,
        IRegistrationService regService)
    {
        _kurzRepo = kurzRepo;
        _terminRepo = terminRepo;
        _prihlaskaRepo = prihlaskaRepo;
        _osvedceniRepo = osvedceniRepo;
        _regService = regService;
    }

    public int CelkemKurzu { get; set; }
    public int AktivnichTerminu { get; set; }
    public int PocetVPoradniku { get; set; }
    public int CelkemOsvedceni { get; set; }

    public List<Termin> AktivniTerminy { get; set; } = new();
    public List<Prihlaska> CekajiciNabidky { get; set; } = new();

    public async Task OnGetAsync()
    {
        var kurzy = await _kurzRepo.GetAllAsync();
        CelkemKurzu = kurzy.Count;

        var terminy = await _terminRepo.GetAllWithDetailsAsync();
        AktivniTerminy = terminy
            .Where(t => t.Stav == StavTerminu.Planovany || t.Stav == StavTerminu.Probiha)
            .ToList();
        AktivnichTerminu = AktivniTerminy.Count;

        var vsechnyPrihlasky = await _prihlaskaRepo.GetAllAsync();
        PocetVPoradniku = vsechnyPrihlasky.Count(p => p.Stav == StavPrihlasky.Poradnik);

        var osvedceni = await _osvedceniRepo.GetAllAsync();
        CelkemOsvedceni = osvedceni.Count;

        // Čekající nabídky uvolněných míst (Stav NABIDNUTO)
        CekajiciNabidky = vsechnyPrihlasky
            .Where(p => p.Stav == StavPrihlasky.Nabidnuto)
            .ToList();

        // Automatické zpracování případných expirovaných nabídek
        await _regService.ProcessExpiredOffersAsync();
    }

    public async Task<IActionResult> OnPostAcceptOfferAsync(Guid prihlaskaId)
    {
        await _regService.AcceptOfferAsync(prihlaskaId);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeclineOfferAsync(Guid prihlaskaId)
    {
        await _regService.DeclineOfferAsync(prihlaskaId);
        return RedirectToPage();
    }
}
