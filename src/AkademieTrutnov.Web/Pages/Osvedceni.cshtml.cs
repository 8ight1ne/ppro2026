using AkademieTrutnov.Core.DTOs;
using AkademieTrutnov.Core.Entities;
using AkademieTrutnov.Core.Enums;
using AkademieTrutnov.Core.Exceptions;
using AkademieTrutnov.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AkademieTrutnov.Web.Pages;

public class OsvedceniModel : PageModel
{
    private readonly ICertificateService _certService;
    private readonly IAttendanceService _attService;
    private readonly ITerminRepository _terminRepo;
    private readonly IPrihlaskaRepository _prihlaskaRepo;
    private readonly IOsvedceniRepository _osvedceniRepo;

    public OsvedceniModel(
        ICertificateService certService,
        IAttendanceService attService,
        ITerminRepository terminRepo,
        IPrihlaskaRepository prihlaskaRepo,
        IOsvedceniRepository osvedceniRepo)
    {
        _certService = certService;
        _attService = attService;
        _terminRepo = terminRepo;
        _prihlaskaRepo = prihlaskaRepo;
        _osvedceniRepo = osvedceniRepo;
    }

    [BindProperty(SupportsGet = true)]
    public Guid? TerminId { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Cislo { get; set; }

    public List<Termin> VsechnyTerminy { get; set; } = new();
    public Termin? AktualniTermin { get; set; }
    public List<CandidateDto> Kandidati { get; set; } = new();
    public List<Osvedceni> VydanaOsvedceni { get; set; } = new();
    public Osvedceni? VyhledaneOsvedceni { get; set; }

    public string? AlertSuccess { get; set; }
    public string? AlertError { get; set; }

    public record CandidateDto(
        Guid PrihlaskaId,
        string StudentJmeno,
        string StudentEmail,
        double ProcentoDochazky,
        bool SplnujeDochazku,
        bool MaJizOsvedceni,
        string? CisloOsvedceni
    );

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
                var summaries = await _attService.GetTerminAttendanceSummariesAsync(TerminId.Value);
                var summaryDict = summaries.ToDictionary(s => s.StudentId);

                var candidates = new List<CandidateDto>();
                foreach (var p in AktualniTermin.Prihlasky.Where(p => p.Stav != StavPrihlasky.Zruseno && p.Stav != StavPrihlasky.Expirovano))
                {
                    summaryDict.TryGetValue(p.StudentId, out var sum);
                    double pct = sum?.ProcentoDochazky ?? 0.0;
                    bool splnuje = pct >= 70.0;

                    candidates.Add(new CandidateDto(
                        p.Id,
                        p.Student?.CeleJmeno ?? "Student",
                        p.Student?.Email ?? "",
                        pct,
                        splnuje,
                        p.Osvedceni != null,
                        p.Osvedceni?.CisloOsvedceni
                    ));
                }

                Kandidati = candidates;
                VydanaOsvedceni = await _certService.GetCertificatesForTerminAsync(TerminId.Value);
            }
        }

        if (!string.IsNullOrWhiteSpace(Cislo))
        {
            VyhledaneOsvedceni = await _certService.GetCertificateByNumberAsync(Cislo.Trim());
        }
    }

    public async Task<IActionResult> OnPostIssueAsync(Guid prihlaskaId, VysledekHodnoceni vysledek, string vystavil)
    {
        try
        {
            var cert = await _certService.IssueCertificateAsync(prihlaskaId, vysledek, vystavil);
            AlertSuccess = $"Osvědčení č. {cert.CisloOsvedceni} bylo úspěšně vystaveno a zapsáno do neměnného auditního registru.";
            Cislo = cert.CisloOsvedceni;
        }
        catch (DomainException ex)
        {
            AlertError = ex.Message;
        }

        await LoadDataAsync();
        return Page();
    }
}
