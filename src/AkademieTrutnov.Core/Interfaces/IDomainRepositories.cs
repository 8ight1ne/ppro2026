using AkademieTrutnov.Core.Entities;
using AkademieTrutnov.Core.Enums;

namespace AkademieTrutnov.Core.Interfaces;

public interface ITerminRepository : IRepository<Termin>
{
    Task<Termin?> GetWithDetailsAsync(Guid id, CancellationToken ct = default);
    Task<List<Termin>> GetAllWithDetailsAsync(CancellationToken ct = default);
}

public interface IPrihlaskaRepository : IRepository<Prihlaska>
{
    Task<Prihlaska?> GetWithDetailsAsync(Guid id, CancellationToken ct = default);
    Task<List<Prihlaska>> GetActiveApplicationsForStudentAndCourseAsync(Guid studentId, Guid kurzId, CancellationToken ct = default);
    Task<int> GetActiveEnrollmentCountForTerminAsync(Guid terminId, CancellationToken ct = default);
    Task<List<Prihlaska>> GetWaitlistForTerminAsync(Guid terminId, CancellationToken ct = default);
    Task<Prihlaska?> GetFirstWaitlistedForTerminAsync(Guid terminId, CancellationToken ct = default);
    Task<List<Prihlaska>> GetExpiredOffersAsync(CancellationToken ct = default);
}

public interface IStudentRepository : IRepository<Student>
{
    Task<Student?> GetWithDetailsAsync(Guid id, CancellationToken ct = default);
}

public interface IKurzRepository : IRepository<Kurz>
{
    Task<Kurz?> GetWithTerminyAsync(Guid id, CancellationToken ct = default);
}

public interface IOsvedceniRepository : IRepository<Osvedceni>
{
    Task<Osvedceni?> GetByNumberAsync(string cisloOsvedceni, CancellationToken ct = default);
    Task<int> GetCountForYearAsync(int year, CancellationToken ct = default);
    Task<List<Osvedceni>> GetForTerminAsync(Guid terminId, CancellationToken ct = default);
}

public interface ILektorRepository : IRepository<Lektor>
{
    Task<List<Lektor>> GetAllWithAssignmentsAsync(CancellationToken ct = default);
}

public interface IDochazkaRepository : IRepository<Dochazka>
{
    Task<DenVyuky?> GetDenVyukyWithAttendanceAsync(Guid denVyukyId, CancellationToken ct = default);
    Task<List<DenVyuky>> GetDnyVyukyForTerminWithAttendanceAsync(Guid terminId, CancellationToken ct = default);
}
