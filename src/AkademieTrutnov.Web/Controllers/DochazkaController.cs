using AkademieTrutnov.Core.DTOs;
using AkademieTrutnov.Core.Exceptions;
using AkademieTrutnov.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AkademieTrutnov.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DochazkaController : ControllerBase
{
    private readonly IAttendanceService _attendanceService;

    public DochazkaController(IAttendanceService attendanceService)
    {
        _attendanceService = attendanceService;
    }

    [HttpPost("day/{denVyukyId:guid}")]
    public async Task<IActionResult> RecordDayAttendance(Guid denVyukyId, [FromBody] List<DochazkaRecordDto> records)
    {
        try
        {
            await _attendanceService.RecordAttendanceAsync(denVyukyId, records);
            return Ok(new { message = "Docházka byla úspěšně uložena." });
        }
        catch (DomainException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("student/{studentId:guid}/termin/{terminId:guid}")]
    public async Task<ActionResult<StudentAttendanceSummaryDto>> GetStudentSummary(Guid studentId, Guid terminId)
    {
        try
        {
            var summary = await _attendanceService.GetStudentAttendanceSummaryAsync(studentId, terminId);
            return Ok(summary);
        }
        catch (DomainException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("termin/{terminId:guid}/overview")]
    public async Task<ActionResult<List<StudentAttendanceSummaryDto>>> GetTerminOverview(Guid terminId)
    {
        try
        {
            var list = await _attendanceService.GetTerminAttendanceSummariesAsync(terminId);
            return Ok(list);
        }
        catch (DomainException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
