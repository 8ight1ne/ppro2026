using AkademieTrutnov.Core.Entities;
using AkademieTrutnov.Core.Enums;
using AkademieTrutnov.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AkademieTrutnov.Web.Pages;

public class TerminyModel : PageModel
{
    private readonly ITerminRepository _terminRepo;
    private readonly IKurzRepository _kurzRepo;
    private readonly IRepository<Ucebna> _ucebnaRepo;
    private readonly ILektorRepository _lektorRepo;
    private readonly IUnitOfWork _unitOfWork;

    public TerminyModel(
        ITerminRepository terminRepo,
        IKurzRepository kurzRepo,
        IRepository<Ucebna> ucebnaRepo,
        ILektorRepository lektorRepo,
        IUnitOfWork unitOfWork)
    {
        _terminRepo = terminRepo;
        _kurzRepo = kurzRepo;
        _ucebnaRepo = ucebnaRepo;
        _lektorRepo = lektorRepo;
        _unitOfWork = unitOfWork;
    }

    public List<Termin> Terminy { get; set; } = new();
    public List<Kurz> Kurzy { get; set; } = new();
    public List<Ucebna> Ucebny { get; set; } = new();
    public List<Lektor> Lektori { get; set; } = new();

    [BindProperty]
    public NewTerminInput Input { get; set; } = new();

    public class NewTerminInput
    {
        public Guid KurzId { get; set; }
        public Guid UcebnaId { get; set; }
        public DateTime DatumOd { get; set; } = DateTime.UtcNow.Date.AddDays(7);
        public DateTime DatumDo { get; set; } = DateTime.UtcNow.Date.AddDays(30);
        public List<Guid> LektorIds { get; set; } = new();
        public int OduceneHodinyNaLektora { get; set; } = 40;
    }

    public async Task OnGetAsync()
    {
        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        Terminy = await _terminRepo.GetAllWithDetailsAsync();
        Kurzy = await _kurzRepo.GetAllAsync();
        Ucebny = await _ucebnaRepo.GetAllAsync();
        Lektori = await _lektorRepo.GetAllAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Input.KurzId == Guid.Empty || Input.UcebnaId == Guid.Empty)
        {
            await LoadDataAsync();
            return Page();
        }

        var ucebna = await _ucebnaRepo.GetByIdAsync(Input.UcebnaId);
        if (ucebna == null)
        {
            await LoadDataAsync();
            return Page();
        }

        // Kapacita termínu se striktně odvíjí od učebny (obchodní pravidlo klienta!)
        var termin = new Termin
        {
            Id = Guid.NewGuid(),
            KurzId = Input.KurzId,
            UcebnaId = Input.UcebnaId,
            DatumOd = Input.DatumOd,
            DatumDo = Input.DatumDo,
            Kapacita = ucebna.Kapacita,
            Stav = StavTerminu.Planovany
        };

        if (Input.LektorIds != null && Input.LektorIds.Count > 0)
        {
            foreach (var lektorId in Input.LektorIds)
            {
                termin.TerminLektori.Add(new TerminLektor
                {
                    TerminId = termin.Id,
                    LektorId = lektorId,
                    PocetHodin = Input.OduceneHodinyNaLektora,
                    Role = "Vyučující lektor"
                });
            }
        }

        await _terminRepo.AddAsync(termin);
        await _unitOfWork.SaveChangesAsync();

        return RedirectToPage();
    }
}
