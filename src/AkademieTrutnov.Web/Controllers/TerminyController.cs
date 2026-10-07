using AkademieTrutnov.Core.Entities;
using AkademieTrutnov.Core.Enums;
using AkademieTrutnov.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AkademieTrutnov.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TerminyController : ControllerBase
{
    private readonly ITerminRepository _terminRepo;
    private readonly IRepository<Ucebna> _ucebnaRepo;
    private readonly IUnitOfWork _unitOfWork;

    public TerminyController(
        ITerminRepository terminRepo, 
        IRepository<Ucebna> ucebnaRepo, 
        IUnitOfWork unitOfWork)
    {
        _terminRepo = terminRepo;
        _ucebnaRepo = ucebnaRepo;
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<ActionResult<List<Termin>>> GetAll()
    {
        var list = await _terminRepo.GetAllWithDetailsAsync();
        return Ok(list);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Termin>> GetById(Guid id)
    {
        var termin = await _terminRepo.GetWithDetailsAsync(id);
        if (termin == null) return NotFound();
        return Ok(termin);
    }

    public record CreateTerminDto(
        Guid KurzId,
        Guid UcebnaId,
        DateTime DatumOd,
        DateTime DatumDo,
        List<Guid>? LektorIds
    );

    [HttpPost]
    public async Task<ActionResult<Termin>> Create([FromBody] CreateTerminDto dto)
    {
        var ucebna = await _ucebnaRepo.GetByIdAsync(dto.UcebnaId);
        if (ucebna == null) return BadRequest("Zvolená učebna neexistuje.");

        // Obchodní pravidlo: Kapacita termínu se striktně odvíjí od kapacity učebny
        var termin = new Termin
        {
            Id = Guid.NewGuid(),
            KurzId = dto.KurzId,
            UcebnaId = dto.UcebnaId,
            DatumOd = dto.DatumOd,
            DatumDo = dto.DatumDo,
            Kapacita = ucebna.Kapacita,
            Stav = StavTerminu.Planovany
        };

        if (dto.LektorIds != null && dto.LektorIds.Count > 0)
        {
            foreach (var lektorId in dto.LektorIds)
            {
                termin.TerminLektori.Add(new TerminLektor
                {
                    TerminId = termin.Id,
                    LektorId = lektorId,
                    PocetHodin = 40,
                    Role = "Lektor"
                });
            }
        }

        await _terminRepo.AddAsync(termin);
        await _unitOfWork.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = termin.Id }, termin);
    }
}
