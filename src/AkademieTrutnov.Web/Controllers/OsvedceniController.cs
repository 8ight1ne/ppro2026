using AkademieTrutnov.Core.Entities;
using AkademieTrutnov.Core.Enums;
using AkademieTrutnov.Core.Exceptions;
using AkademieTrutnov.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AkademieTrutnov.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OsvedceniController : ControllerBase
{
    private readonly ICertificateService _certService;

    public OsvedceniController(ICertificateService certService)
    {
        _certService = certService;
    }

    public record IssueCertificateRequestDto(
        Guid PrihlaskaId,
        VysledekHodnoceni Vysledek,
        string Vystavil
    );

    [HttpPost("issue")]
    public async Task<ActionResult<Osvedceni>> IssueCertificate([FromBody] IssueCertificateRequestDto dto)
    {
        try
        {
            var cert = await _certService.IssueCertificateAsync(dto.PrihlaskaId, dto.Vysledek, dto.Vystavil);
            return Ok(cert);
        }
        catch (DomainException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("by-number/{cisloOsvedceni}")]
    public async Task<ActionResult<Osvedceni>> GetByNumber(string cisloOsvedceni)
    {
        var cert = await _certService.GetCertificateByNumberAsync(cisloOsvedceni);
        if (cert == null) return NotFound(new { error = "Osvědčení s tímto evidenčním číslem neexistuje." });
        return Ok(cert);
    }

    [HttpGet("termin/{terminId:guid}")]
    public async Task<ActionResult<List<Osvedceni>>> GetForTermin(Guid terminId)
    {
        var certs = await _certService.GetCertificatesForTerminAsync(terminId);
        return Ok(certs);
    }
}
