using AkademieTrutnov.Core.Entities;
using AkademieTrutnov.Core.Enums;
using AkademieTrutnov.Core.Exceptions;
using AkademieTrutnov.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AkademieTrutnov.Web.Pages;

public class PrihlaskyModel : PageModel
{
    private readonly IRegistrationService _regService;
    private readonly IPrihlaskaRepository _prihlaskaRepo;
    private readonly ITerminRepository _terminRepo;
    private readonly IStudentRepository _studentRepo;
    private readonly IUnitOfWork _unitOfWork;

    public PrihlaskyModel(
        IRegistrationService regService,
        IPrihlaskaRepository prihlaskaRepo,
        ITerminRepository terminRepo,
        IStudentRepository studentRepo,
        IUnitOfWork unitOfWork)
    {
        _regService = regService;
        _prihlaskaRepo = prihlaskaRepo;
        _terminRepo = terminRepo;
        _studentRepo = studentRepo;
        _unitOfWork = unitOfWork;
    }

    [BindProperty(SupportsGet = true)]
    public Guid? TerminFilterId { get; set; }

    public List<Termin> VsechnyTerminy { get; set; } = new();
    public List<Student> VsechniStudenti { get; set; } = new();
    public List<Prihlaska> Prihlasky { get; set; } = new();

    public string? AlertMessage { get; set; }
    public string? AlertError { get; set; }

    [BindProperty]
    public RegisterInput NewRegistration { get; set; } = new();

    public class RegisterInput
    {
        public Guid StudentId { get; set; }
        public Guid TerminId { get; set; }
        public bool IsLateAdminEnrollment { get; set; }
        public string? AdminNote { get; set; }

        // Volitelné vytvoření nového studenta přímo z formuláře
        public bool CreateNewStudent { get; set; }
        public string? NoveJmeno { get; set; }
        public string? NovePrijmeni { get; set; }
        public string? NovyEmail { get; set; }
        public string? NovyTelefon { get; set; }
        public string? NovaOrganizace { get; set; }
    }

    [BindProperty]
    public CancelInput CancelData { get; set; } = new();

    public class CancelInput
    {
        public Guid PrihlaskaId { get; set; }
        public string Duvod { get; set; } = string.Empty;
    }

    public async Task OnGetAsync()
    {
        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        VsechnyTerminy = await _terminRepo.GetAllWithDetailsAsync();
        VsechniStudenti = await _studentRepo.GetAllAsync();

        var allPrihlasky = await _prihlaskaRepo.GetAllAsync();
        if (TerminFilterId.HasValue)
        {
            allPrihlasky = allPrihlasky.Where(p => p.TerminId == TerminFilterId.Value).ToList();
        }

        // Seřadíme přehledně: nejprve řádně přihlášení a nabídky, pak pořadník, pak dokončení a zrušení
        Prihlasky = allPrihlasky
            .OrderBy(p => p.Stav switch
            {
                StavPrihlasky.Nabidnuto => 0,
                StavPrihlasky.Prihlasen => 1,
                StavPrihlasky.Poradnik => 2,
                StavPrihlasky.Dokonceno => 3,
                _ => 4
            })
            .ThenBy(p => p.PoradiVPoradniku ?? int.MaxValue)
            .ThenByDescending(p => p.DatumPrihlaseni)
            .ToList();
    }

    public async Task<IActionResult> OnPostRegisterAsync()
    {
        Guid targetStudentId = NewRegistration.StudentId;

        if (NewRegistration.CreateNewStudent)
        {
            if (string.IsNullOrWhiteSpace(NewRegistration.NoveJmeno) || string.IsNullOrWhiteSpace(NewRegistration.NovePrijmeni) || string.IsNullOrWhiteSpace(NewRegistration.NovyEmail))
            {
                AlertError = "Vyplňte prosím jméno, příjmení a e-mail pro nového studenta.";
                await LoadDataAsync();
                return Page();
            }

            var novyStudent = new Student
            {
                Id = Guid.NewGuid(),
                Jmeno = NewRegistration.NoveJmeno.Trim(),
                Prijmeni = NewRegistration.NovePrijmeni.Trim(),
                Email = NewRegistration.NovyEmail.Trim(),
                Telefon = NewRegistration.NovyTelefon?.Trim() ?? "",
                Organizace = NewRegistration.NovaOrganizace?.Trim()
            };

            await _studentRepo.AddAsync(novyStudent);
            await _unitOfWork.SaveChangesAsync();
            targetStudentId = novyStudent.Id;
        }

        if (targetStudentId == Guid.Empty || NewRegistration.TerminId == Guid.Empty)
        {
            AlertError = "Vyberte prosím studenta a termín kurzu.";
            await LoadDataAsync();
            return Page();
        }

        try
        {
            var res = await _regService.RegisterStudentAsync(
                targetStudentId,
                NewRegistration.TerminId,
                NewRegistration.IsLateAdminEnrollment,
                NewRegistration.AdminNote);

            AlertMessage = res.Zprava;
        }
        catch (DomainException ex)
        {
            AlertError = ex.Message;
        }

        await LoadDataAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        if (CancelData.PrihlaskaId == Guid.Empty)
        {
            return RedirectToPage();
        }

        try
        {
            await _regService.CancelRegistrationAsync(CancelData.PrihlaskaId, CancelData.Duvod);
            AlertMessage = "Přihláška byla stornována. Záznam byl archivován a případné uvolněné místo bylo nabídnuto prvnímu náhradníkovi v pořadníku.";
        }
        catch (DomainException ex)
        {
            AlertError = ex.Message;
        }

        await LoadDataAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAcceptOfferAsync(Guid prihlaskaId)
    {
        try
        {
            await _regService.AcceptOfferAsync(prihlaskaId);
            AlertMessage = "Nabídka místa byla úspěšně potvrzena. Student je nyní řádně přihlášen.";
        }
        catch (DomainException ex)
        {
            AlertError = ex.Message;
        }

        await LoadDataAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostDeclineOfferAsync(Guid prihlaskaId)
    {
        try
        {
            await _regService.DeclineOfferAsync(prihlaskaId);
            AlertMessage = "Nabídka byla odmítnuta. Místo bylo automaticky postoupeno dalšímu náhradníkovi v pořadí.";
        }
        catch (DomainException ex)
        {
            AlertError = ex.Message;
        }

        await LoadDataAsync();
        return Page();
    }
}
