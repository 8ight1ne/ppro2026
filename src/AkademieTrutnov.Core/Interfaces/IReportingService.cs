using AkademieTrutnov.Core.DTOs;

namespace AkademieTrutnov.Core.Interfaces;

public interface IReportingService
{
    Task<List<TerminUtilizationDto>> GetTerminUtilizationReportAsync(CancellationToken ct = default);
    Task<List<LecturerHoursDto>> GetLecturerHoursReportAsync(CancellationToken ct = default);
    Task<List<MultiCourseStudentDto>> GetMultiCourseStudentsReportAsync(int rok, CancellationToken ct = default);
}
