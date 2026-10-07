using AkademieTrutnov.Core.Entities;
using AkademieTrutnov.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AkademieTrutnov.Web.Pages;

public class KurzyModel : PageModel
{
    private readonly IKurzRepository _kurzRepo;
    private readonly IUnitOfWork _unitOfWork;

    public KurzyModel(IKurzRepository kurzRepo, IUnitOfWork unitOfWork)
    {
        _kurzRepo = kurzRepo;
        _unitOfWork = unitOfWork;
    }

    public List<Kurz> Kurzy { get; set; } = new();

    [BindProperty]
    public NewKurzInput Input { get; set; } = new();

    public class NewKurzInput
    {
        public string Nazev { get; set; } = string.Empty;
        public string Anotace { get; set; } = string.Empty;
        public int RozsahHodin { get; set; } = 40;
        public decimal Cena { get; set; } = 9900;
        public bool Akreditovano { get; set; } = true;
    }

    public async Task OnGetAsync()
    {
        Kurzy = await _kurzRepo.GetAllAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(Input.Nazev))
        {
            Kurzy = await _kurzRepo.GetAllAsync();
            return Page();
        }

        var kurz = new Kurz
        {
            Id = Guid.NewGuid(),
            Nazev = Input.Nazev,
            Anotace = Input.Anotace,
            RozsahHodin = Input.RozsahHodin,
            Cena = Input.Cena,
            Akreditovano = Input.Akreditovano
        };

        await _kurzRepo.AddAsync(kurz);
        await _unitOfWork.SaveChangesAsync();

        return RedirectToPage();
    }
}
