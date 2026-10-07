using AkademieTrutnov.Core.Entities;
using AkademieTrutnov.Core.Enums;
using AkademieTrutnov.Core.Interfaces;
using AkademieTrutnov.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AkademieTrutnov.Infrastructure.Repositories;

public class TerminRepository : EfRepository<Termin>, ITerminRepository
{
    public TerminRepository(AppDbContext context) : base(context) { }

    public async Task<Termin?> GetWithDetailsAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Terminy
            .Include(t => t.Kurz)
            .Include(t => t.Ucebna)
            .Include(t => t.TerminLektori)
                .ThenInclude(tl => tl.Lektor)
            .Include(t => t.Prihlasky)
                .ThenInclude(p => p.Student)
            .Include(t => t.Prihlasky)
                .ThenInclude(p => p.Osvedceni)
            .Include(t => t.DnyVyuky)
                .ThenInclude(d => d.Dochazky)
            .FirstOrDefaultAsync(t => t.Id == id, ct);
    }

    public async Task<List<Termin>> GetAllWithDetailsAsync(CancellationToken ct = default)
    {
        return await _context.Terminy
            .Include(t => t.Kurz)
            .Include(t => t.Ucebna)
            .Include(t => t.TerminLektori)
                .ThenInclude(tl => tl.Lektor)
            .Include(t => t.Prihlasky)
            .Include(t => t.DnyVyuky)
            .OrderBy(t => t.DatumOd)
            .ToListAsync(ct);
    }
}

public class PrihlaskaRepository : EfRepository<Prihlaska>, IPrihlaskaRepository
{
    public PrihlaskaRepository(AppDbContext context) : base(context) { }

    public async Task<Prihlaska?> GetWithDetailsAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Prihlasky
            .Include(p => p.Student)
            .Include(p => p.Termin)
                .ThenInclude(t => t.Kurz)
            .Include(p => p.Termin)
                .ThenInclude(t => t.Ucebna)
            .Include(p => p.Osvedceni)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<List<Prihlaska>> GetActiveApplicationsForStudentAndCourseAsync(Guid studentId, Guid kurzId, CancellationToken ct = default)
    {
        return await _context.Prihlasky
            .Include(p => p.Termin)
            .Where(p => p.StudentId == studentId && 
                        p.Termin.KurzId == kurzId && 
                        (p.Stav == StavPrihlasky.Prihlasen || 
                         p.Stav == StavPrihlasky.Poradnik || 
                         p.Stav == StavPrihlasky.Nabidnuto))
            .ToListAsync(ct);
    }

    public async Task<int> GetActiveEnrollmentCountForTerminAsync(Guid terminId, CancellationToken ct = default)
    {
        return await _context.Prihlasky
            .CountAsync(p => p.TerminId == terminId && 
                            (p.Stav == StavPrihlasky.Prihlasen || 
                             p.Stav == StavPrihlasky.Nabidnuto ||
                             p.Stav == StavPrihlasky.Dokonceno), ct);
    }

    public async Task<List<Prihlaska>> GetWaitlistForTerminAsync(Guid terminId, CancellationToken ct = default)
    {
        return await _context.Prihlasky
            .Include(p => p.Student)
            .Where(p => p.TerminId == terminId && p.Stav == StavPrihlasky.Poradnik)
            .OrderBy(p => p.PoradiVPoradniku ?? int.MaxValue)
            .ToListAsync(ct);
    }

    public async Task<Prihlaska?> GetFirstWaitlistedForTerminAsync(Guid terminId, CancellationToken ct = default)
    {
        return await _context.Prihlasky
            .Include(p => p.Student)
            .Where(p => p.TerminId == terminId && p.Stav == StavPrihlasky.Poradnik)
            .OrderBy(p => p.PoradiVPoradniku ?? int.MaxValue)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<List<Prihlaska>> GetExpiredOffersAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        return await _context.Prihlasky
            .Where(p => p.Stav == StavPrihlasky.Nabidnuto && p.NabidkaExpirace != null && p.NabidkaExpirace.Value <= now)
            .ToListAsync(ct);
    }
}

public class StudentRepository : EfRepository<Student>, IStudentRepository
{
    public StudentRepository(AppDbContext context) : base(context) { }

    public async Task<Student?> GetWithDetailsAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Studenti
            .Include(s => s.Prihlasky)
                .ThenInclude(p => p.Termin)
                    .ThenInclude(t => t.Kurz)
            .Include(s => s.Prihlasky)
                .ThenInclude(p => p.Osvedceni)
            .FirstOrDefaultAsync(s => s.Id == id, ct);
    }
}

public class KurzRepository : EfRepository<Kurz>, IKurzRepository
{
    public KurzRepository(AppDbContext context) : base(context) { }

    public async Task<Kurz?> GetWithTerminyAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Kurzy
            .Include(k => k.Terminy)
                .ThenInclude(t => t.Ucebna)
            .FirstOrDefaultAsync(k => k.Id == id, ct);
    }
}

public class OsvedceniRepository : EfRepository<Osvedceni>, IOsvedceniRepository
{
    public OsvedceniRepository(AppDbContext context) : base(context) { }

    public async Task<Osvedceni?> GetByNumberAsync(string cisloOsvedceni, CancellationToken ct = default)
    {
        return await _context.Osvedceni
            .Include(o => o.Prihlaska)
                .ThenInclude(p => p.Student)
            .Include(o => o.Prihlaska)
                .ThenInclude(p => p.Termin)
                    .ThenInclude(t => t.Kurz)
            .FirstOrDefaultAsync(o => o.CisloOsvedceni == cisloOsvedceni, ct);
    }

    public async Task<int> GetCountForYearAsync(int year, CancellationToken ct = default)
    {
        return await _context.Osvedceni
            .CountAsync(o => o.DatumVydani.Year == year, ct);
    }

    public async Task<List<Osvedceni>> GetForTerminAsync(Guid terminId, CancellationToken ct = default)
    {
        return await _context.Osvedceni
            .Include(o => o.Prihlaska)
                .ThenInclude(p => p.Student)
            .Where(o => o.Prihlaska.TerminId == terminId)
            .OrderBy(o => o.DatumVydani)
            .ToListAsync(ct);
    }
}

public class LektorRepository : EfRepository<Lektor>, ILektorRepository
{
    public LektorRepository(AppDbContext context) : base(context) { }

    public async Task<List<Lektor>> GetAllWithAssignmentsAsync(CancellationToken ct = default)
    {
        return await _context.Lektori
            .Include(l => l.TerminLektori)
                .ThenInclude(tl => tl.Termin)
                    .ThenInclude(t => t.Kurz)
            .ToListAsync(ct);
    }
}

public class DochazkaRepository : EfRepository<Dochazka>, IDochazkaRepository
{
    public DochazkaRepository(AppDbContext context) : base(context) { }

    public async Task<DenVyuky?> GetDenVyukyWithAttendanceAsync(Guid denVyukyId, CancellationToken ct = default)
    {
        return await _context.DnyVyuky
            .Include(d => d.Dochazky)
                .ThenInclude(doc => doc.Student)
            .FirstOrDefaultAsync(d => d.Id == denVyukyId, ct);
    }

    public async Task<List<DenVyuky>> GetDnyVyukyForTerminWithAttendanceAsync(Guid terminId, CancellationToken ct = default)
    {
        return await _context.DnyVyuky
            .Include(d => d.Dochazky)
            .Where(d => d.TerminId == terminId)
            .OrderBy(d => d.Datum)
            .ToListAsync(ct);
    }
}

public class UcebnaRepository : EfRepository<Ucebna>, IUcebnaRepository
{
    public UcebnaRepository(AppDbContext context) : base(context) { }
}
