using AkademieTrutnov.Core.DTOs;
using AkademieTrutnov.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AkademieTrutnov.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportyController : ControllerBase
{
    private readonly IReportingService _reportingService;

    public ReportyController(IReportingService reportingService)
    {
        _reportingService = reportingService;
    }

    [HttpGet("utilization")]
    public async Task<ActionResult<List<TerminUtilizationDto>>> GetUtilization()
    {
        var result = await _reportingService.GetTerminUtilizationReportAsync();
        return Ok(result);
    }

    [HttpGet("lecturer-hours")]
    public async Task<ActionResult<List<LecturerHoursDto>>> GetLecturerHours()
    {
        var result = await _reportingService.GetLecturerHoursReportAsync();
        return Ok(result);
    }

    [HttpGet("multi-course-students")]
    public async Task<ActionResult<List<MultiCourseStudentDto>>> GetMultiCourseStudents([FromQuery] int? rok)
    {
        int year = rok ?? DateTime.UtcNow.Year;
        var result = await _reportingService.GetMultiCourseStudentsReportAsync(year);
        return Ok(result);
    }
}
