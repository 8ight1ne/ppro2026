using AkademieTrutnov.Core.DTOs;
using AkademieTrutnov.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AkademieTrutnov.Web.Pages;

public class ReportyModel : PageModel
{
    private readonly IReportingService _reportingService;

    public ReportyModel(IReportingService reportingService)
    {
        _reportingService = reportingService;
    }

    [BindProperty(SupportsGet = true)]
    public int Rok { get; set; } = DateTime.UtcNow.Year;

    public List<TerminUtilizationDto> Vytizenost { get; set; } = new();
    public List<LecturerHoursDto> HodinyLektoru { get; set; } = new();
    public List<MultiCourseStudentDto> StudentiViceKurzu { get; set; } = new();

    public async Task OnGetAsync()
    {
        Vytizenost = await _reportingService.GetTerminUtilizationReportAsync();
        HodinyLektoru = await _reportingService.GetLecturerHoursReportAsync();
        StudentiViceKurzu = await _reportingService.GetMultiCourseStudentsReportAsync(Rok);
    }
}
