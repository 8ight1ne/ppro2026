using AkademieTrutnov.Core.DTOs;
using AkademieTrutnov.Core.Entities;
using AkademieTrutnov.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AkademieTrutnov.Web.Pages;

public class DochazkaModel : PageModel
{
    private readonly ITerminRepository _terminRepo;
    private readonly IDochazkaRepository _dochazkaRepo;
    private readonly IAttendanceService _attService;
    private readonly IUnitOfWork _unitOfWork;

    public DochazkaModel(
        ITerminRepository terminRepo,
        IDochazkaRepository dochazkaRepo,
        IAttendanceService attService,
        IUnitOfWork unitOfWork)
    {
        _terminRepo = terminRepo;
        _dochazkaRepo = dochazkaRepo;
        _attService = attService;
        _unitOfWork = unitOfWork;
    }

    [BindProperty(SupportsGet = true)]
    public Guid? TerminId { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid? VybranyDenId { get; set; }

    public List<Termin> VsechnyTerminy { get; set; } = new();
    public Termin? AktualniTermin { get; set; }
    public List<DenVyuky> DnyVyuky { get; set; } = new();
    public DenVyuky? AktualniDen { get; set; }
    public List<StudentAttendanceSummaryDto> SouhrnDochazky { get; set; } = new();
    public List<Student> ZapsaniStudenti { get; set; } = new();

    public string? AlertMessage { get; set; }

    [BindProperty]
    public Dictionary<Guid, bool> PritomenMap { get; set; } = new();

    [BindProperty]
    public Dictionary<Guid, string> PoznamkaMap { get; set; } = new();

    public async Task OnGetAsync()
    {
        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        VsechnyTerminy = await _terminRepo.GetAllWithDetailsAsync();

        if (!TerminId.HasValue && VsechnyTerminy.Any())
        {
            TerminId = VsechnyTerminy.First().Id;
        }

        if (TerminId.HasValue)
        {
            AktualniTermin = await _terminRepo.GetWithDetailsAsync(TerminId.Value);
            if (AktualniTermin != null)
            {
                DnyVyuky = await _dochazkaRepo.GetDnyVyukyForTerminWithAttendanceAsync(TerminId.Value);
                SouhrnDochazky = await _attService.GetTerminAttendanceSummariesAsync(TerminId.Value);

                ZapsaniStudenti = AktualniTermin.Prihlasky
                    .Where(p => p.Stav == Core.Enums.StavPrihlasky.Prihlasen || p.Stav == Core.Enums.StavPrihlasky.Dokonceno)
                    .Select(p => p.Student)
                    .DistinctBy(s => s.Id)
                    .ToList();

                if (!VybranyDenId.HasValue && DnyVyuky.Any())
                {
                    VybranyDenId = DnyVyuky.First().Id;
                }

                if (VybranyDenId.HasValue)
                {
                    AktualniDen = await _dochazkaRepo.GetDenVyukyWithAttendanceAsync(VybranyDenId.Value);
                    if (AktualniDen != null)
                    {
                        foreach (var doc in AktualniDen.Dochazky)
                        {
                            PritomenMap[doc.StudentId] = doc.Pritomen;
                            PoznamkaMap[doc.StudentId] = doc.Poznamka ?? "";
                        }
                    }
                }
            }
        }
    }

    public async Task<IActionResult> OnPostSaveAttendanceAsync(Guid denVyukyId)
    {
        var records = new List<DochazkaRecordDto>();
        foreach (var (studentId, pritomen) in PritomenMap)
        {
            string? poznamka = PoznamkaMap.TryGetValue(studentId, out var val) ? val : null;
            records.Add(new DochazkaRecordDto(studentId, pritomen, poznamka));
        }

        await _attService.RecordAttendanceAsync(denVyukyId, records);
        AlertMessage = "Docházka pro vybraný den byla úspěšně uložena.";

        VybranyDenId = denVyukyId;
        await LoadDataAsync();
        return Page();
    }
}
