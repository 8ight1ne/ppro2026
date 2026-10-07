using AkademieTrutnov.Core.Entities;
using AkademieTrutnov.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AkademieTrutnov.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class KurzyController : ControllerBase
{
    private readonly IKurzRepository _kurzRepo;
    private readonly IUnitOfWork _unitOfWork;

    public KurzyController(IKurzRepository kurzRepo, IUnitOfWork unitOfWork)
    {
        _kurzRepo = kurzRepo;
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<ActionResult<List<Kurz>>> GetAll()
    {
        var list = await _kurzRepo.GetAllAsync();
        return Ok(list);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Kurz>> GetById(Guid id)
    {
        var kurz = await _kurzRepo.GetWithTerminyAsync(id);
        if (kurz == null) return NotFound();
        return Ok(kurz);
    }

    [HttpPost]
    public async Task<ActionResult<Kurz>> Create([FromBody] Kurz kurz)
    {
        kurz.Id = Guid.NewGuid();
        await _kurzRepo.AddAsync(kurz);
        await _unitOfWork.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = kurz.Id }, kurz);
    }
}
