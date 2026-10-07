using AkademieTrutnov.Core.DTOs;
using AkademieTrutnov.Core.Enums;
using AkademieTrutnov.Core.Interfaces;

namespace AkademieTrutnov.Core.Services;

public class ReportingService : IReportingService
{
    private readonly ITerminRepository _terminRepo;
    private readonly ILektorRepository _lektorRepo;
    private readonly IStudentRepository _studentRepo;
    private readonly IPrihlaskaRepository _prihlaskaRepo;

    public ReportingService(
        ITerminRepository terminRepo,
        ILektorRepository lektorRepo,
        IStudentRepository studentRepo,
        IPrihlaskaRepository prihlaskaRepo)
    {
        _terminRepo = terminRepo;
        _lektorRepo = lektorRepo;
        _studentRepo = studentRepo;
        _prihlaskaRepo = prihlaskaRepo;
    }

    public async Task<List<TerminUtilizationDto>> GetTerminUtilizationReportAsync(CancellationToken ct = default)
    {
        var terminy = await _terminRepo.GetAllWithDetailsAsync(ct);
        var report = new List<TerminUtilizationDto>();

        foreach (var t in terminy)
        {
            int prihlaseni = t.Prihlasky.Count(p => p.Stav == StavPrihlasky.Prihlasen || p.Stav == StavPrihlasky.Dokonceno);
            int nabidnuto = t.Prihlasky.Count(p => p.Stav == StavPrihlasky.Nabidnuto);
            int poradnik = t.Prihlasky.Count(p => p.Stav == StavPrihlasky.Poradnik);

            int obsazenoCelkem = prihlaseni + nabidnuto;
            double procento = t.Kapacita > 0 ? ((double)obsazenoCelkem / t.Kapacita) * 100.0 : 0.0;

            report.Add(new TerminUtilizationDto(
                t.Id,
                t.Kurz?.Nazev ?? "Neznámý kurz",
                t.Ucebna?.Nazev ?? "Neznámá učebna",
                t.DatumOd,
                t.DatumDo,
                t.Kapacita,
                prihlaseni,
                nabidnuto,
                poradnik,
                Math.Round(procento, 1),
                t.Stav
            ));
        }

        return report.OrderBy(r => r.DatumOd).ToList();
    }

    public async Task<List<LecturerHoursDto>> GetLecturerHoursReportAsync(CancellationToken ct = default)
    {
        var lektori = await _lektorRepo.GetAllWithAssignmentsAsync(ct);
        var report = new List<LecturerHoursDto>();

        foreach (var l in lektori)
        {
            int pocetTerminu = l.TerminLektori.Select(tl => tl.TerminId).Distinct().Count();
            int oduceneHodiny = l.TerminLektori.Sum(tl => tl.PocetHodin);

            report.Add(new LecturerHoursDto(
                l.Id,
                l.CeleJmeno,
                l.Email,
                l.Externista,
                pocetTerminu,
                oduceneHodiny
            ));
        }

        return report.OrderByDescending(r => r.CelkemOducenychHodin).ToList();
    }

    public async Task<List<MultiCourseStudentDto>> GetMultiCourseStudentsReportAsync(int rok, CancellationToken ct = default)
    {
        var allPrihlasky = await _prihlaskaRepo.GetAllAsync(ct);
        var allTerminy = await _terminRepo.GetAllWithDetailsAsync(ct);
        var terminDict = allTerminy.ToDictionary(t => t.Id);

        // Filtrujeme přihlášky z daného roku (které nebyly stornovány)
        var prihlaskyVRoce = allPrihlasky
            .Where(p => p.DatumPrihlaseni.Year == rok && p.Stav != StavPrihlasky.Zruseno && p.Stav != StavPrihlasky.Expirovano)
            .ToList();

        var groupedByStudent = prihlaskyVRoce.GroupBy(p => p.StudentId);
        var report = new List<MultiCourseStudentDto>();

        foreach (var group in groupedByStudent)
        {
            var kurzy = new HashSet<string>();
            foreach (var p in group)
            {
                if (terminDict.TryGetValue(p.TerminId, out var termin) && termin.Kurz != null)
                {
                    kurzy.Add(termin.Kurz.Nazev);
                }
            }

            // Klient chce: kdo se letos přihlásil na víc než jeden kurz (kurzy count >= 2)
            if (kurzy.Count >= 2)
            {
                var student = await _studentRepo.GetByIdAsync(group.Key, ct);
                if (student != null)
                {
                    report.Add(new MultiCourseStudentDto(
                        student.Id,
                        student.CeleJmeno,
                        student.Email,
                        kurzy.Count,
                        kurzy.OrderBy(k => k).ToList()
                    ));
                }
            }
        }

        return report.OrderByDescending(r => r.PocetKurzuVRoce).ToList();
    }
}
