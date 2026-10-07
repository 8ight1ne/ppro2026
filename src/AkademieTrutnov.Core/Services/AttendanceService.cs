using AkademieTrutnov.Core.DTOs;
using AkademieTrutnov.Core.Entities;
using AkademieTrutnov.Core.Exceptions;
using AkademieTrutnov.Core.Interfaces;

namespace AkademieTrutnov.Core.Services;

public class AttendanceService : IAttendanceService
{
    private readonly IDochazkaRepository _dochazkaRepo;
    private readonly ITerminRepository _terminRepo;
    private readonly IStudentRepository _studentRepo;
    private readonly IUnitOfWork _unitOfWork;

    public AttendanceService(
        IDochazkaRepository dochazkaRepo,
        ITerminRepository terminRepo,
        IStudentRepository studentRepo,
        IUnitOfWork unitOfWork)
    {
        _dochazkaRepo = dochazkaRepo;
        _terminRepo = terminRepo;
        _studentRepo = studentRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task RecordAttendanceAsync(
        Guid denVyukyId, 
        List<DochazkaRecordDto> records, 
        CancellationToken ct = default)
    {
        var denVyuky = await _dochazkaRepo.GetDenVyukyWithAttendanceAsync(denVyukyId, ct)
            ?? throw new EntityNotFoundException(nameof(DenVyuky), denVyukyId);

        foreach (var rec in records)
        {
            var existing = denVyuky.Dochazky.FirstOrDefault(d => d.StudentId == rec.StudentId);
            if (existing != null)
            {
                existing.Pritomen = rec.Pritomen;
                existing.Poznamka = rec.Poznamka;
                _dochazkaRepo.Update(existing);
            }
            else
            {
                var newDochazka = new Dochazka
                {
                    Id = Guid.NewGuid(),
                    DenVyukyId = denVyukyId,
                    StudentId = rec.StudentId,
                    Pritomen = rec.Pritomen,
                    Poznamka = rec.Poznamka
                };
                await _dochazkaRepo.AddAsync(newDochazka, ct);
            }
        }

        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task<StudentAttendanceSummaryDto> GetStudentAttendanceSummaryAsync(
        Guid studentId, 
        Guid terminId, 
        CancellationToken ct = default)
    {
        var student = await _studentRepo.GetByIdAsync(studentId, ct)
            ?? throw new EntityNotFoundException(nameof(Student), studentId);

        var dnyVyuky = await _dochazkaRepo.GetDnyVyukyForTerminWithAttendanceAsync(terminId, ct);
        int celkemHodin = dnyVyuky.Sum(d => d.PocetHodin);

        int odchozenoHodin = dnyVyuky
            .Where(d => d.Dochazky.Any(doc => doc.StudentId == studentId && doc.Pritomen))
            .Sum(d => d.PocetHodin);

        double procento = celkemHodin > 0 ? ((double)odchozenoHodin / celkemHodin) * 100.0 : 0.0;

        return new StudentAttendanceSummaryDto(
            studentId,
            student.CeleJmeno,
            celkemHodin,
            odchozenoHodin,
            Math.Round(procento, 1),
            procento >= 70.0
        );
    }

    public async Task<List<StudentAttendanceSummaryDto>> GetTerminAttendanceSummariesAsync(
        Guid terminId, 
        CancellationToken ct = default)
    {
        var termin = await _terminRepo.GetWithDetailsAsync(terminId, ct)
            ?? throw new EntityNotFoundException(nameof(Termin), terminId);

        var enrolledStudents = termin.Prihlasky
            .Where(p => p.Stav == Enums.StavPrihlasky.Prihlasen || p.Stav == Enums.StavPrihlasky.Dokonceno)
            .Select(p => p.Student)
            .DistinctBy(s => s.Id)
            .ToList();

        var dnyVyuky = await _dochazkaRepo.GetDnyVyukyForTerminWithAttendanceAsync(terminId, ct);
        int celkemHodin = dnyVyuky.Sum(d => d.PocetHodin);

        var results = new List<StudentAttendanceSummaryDto>();

        foreach (var student in enrolledStudents)
        {
            int odchozenoHodin = dnyVyuky
                .Where(d => d.Dochazky.Any(doc => doc.StudentId == student.Id && doc.Pritomen))
                .Sum(d => d.PocetHodin);

            double procento = celkemHodin > 0 ? ((double)odchozenoHodin / celkemHodin) * 100.0 : 0.0;

            results.Add(new StudentAttendanceSummaryDto(
                student.Id,
                student.CeleJmeno,
                celkemHodin,
                odchozenoHodin,
                Math.Round(procento, 1),
                procento >= 70.0
            ));
        }

        return results;
    }
}
