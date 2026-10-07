using AkademieTrutnov.Core.DTOs;
using AkademieTrutnov.Core.Entities;

namespace AkademieTrutnov.Core.Interfaces;

public interface IAttendanceService
{
    Task RecordAttendanceAsync(
        Guid denVyukyId, 
        List<DochazkaRecordDto> records, 
        CancellationToken ct = default);

    Task<StudentAttendanceSummaryDto> GetStudentAttendanceSummaryAsync(
        Guid studentId, 
        Guid terminId, 
        CancellationToken ct = default);

    Task<List<StudentAttendanceSummaryDto>> GetTerminAttendanceSummariesAsync(
        Guid terminId, 
        CancellationToken ct = default);
}
