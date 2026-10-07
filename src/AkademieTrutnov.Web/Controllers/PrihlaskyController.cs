using AkademieTrutnov.Core.DTOs;
using AkademieTrutnov.Core.Entities;
using AkademieTrutnov.Core.Exceptions;
using AkademieTrutnov.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AkademieTrutnov.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PrihlaskyController : ControllerBase
{
    private readonly IRegistrationService _regService;
    private readonly IPrihlaskaRepository _prihlaskaRepo;

    public PrihlaskyController(IRegistrationService regService, IPrihlaskaRepository prihlaskaRepo)
    {
        _regService = regService;
        _prihlaskaRepo = prihlaskaRepo;
    }

    [HttpGet]
    public async Task<ActionResult<List<Prihlaska>>> GetAll([FromQuery] Guid? terminId)
    {
        var all = await _prihlaskaRepo.GetAllAsync();
        if (terminId.HasValue)
        {
            all = all.Where(p => p.TerminId == terminId.Value).ToList();
        }
        return Ok(all);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Prihlaska>> GetById(Guid id)
    {
        var p = await _prihlaskaRepo.GetWithDetailsAsync(id);
        if (p == null) return NotFound();
        return Ok(p);
    }

    public record RegisterRequestDto(
        Guid StudentId,
        Guid TerminId,
        bool IsLateAdminEnrollment = false,
        string? AdminNote = null
    );

    [HttpPost("register")]
    public async Task<ActionResult<RegistrationResultDto>> Register([FromBody] RegisterRequestDto dto)
    {
        try
        {
            var result = await _regService.RegisterStudentAsync(
                dto.StudentId, 
                dto.TerminId, 
                dto.IsLateAdminEnrollment, 
                dto.AdminNote);

            return Ok(result);
        }
        catch (DomainException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    public record CancelRequestDto(string Duvod);

    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<Prihlaska>> Cancel(Guid id, [FromBody] CancelRequestDto dto)
    {
        try
        {
            var result = await _regService.CancelRegistrationAsync(id, dto.Duvod);
            return Ok(result);
        }
        catch (DomainException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{id:guid}/accept-offer")]
    public async Task<ActionResult<Prihlaska>> AcceptOffer(Guid id)
    {
        try
        {
            var result = await _regService.AcceptOfferAsync(id);
            return Ok(result);
        }
        catch (DomainException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{id:guid}/decline-offer")]
    public async Task<ActionResult<Prihlaska>> DeclineOffer(Guid id)
    {
        try
        {
            var result = await _regService.DeclineOfferAsync(id);
            return Ok(result);
        }
        catch (DomainException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("process-expired")]
    public async Task<ActionResult<int>> ProcessExpired()
    {
        int count = await _regService.ProcessExpiredOffersAsync();
        return Ok(new { processedExpiredCount = count });
    }
}
