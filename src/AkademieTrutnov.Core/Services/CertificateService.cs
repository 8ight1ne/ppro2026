using AkademieTrutnov.Core.Entities;
using AkademieTrutnov.Core.Enums;
using AkademieTrutnov.Core.Exceptions;
using AkademieTrutnov.Core.Interfaces;

namespace AkademieTrutnov.Core.Services;

public class CertificateService : ICertificateService
{
    private readonly IOsvedceniRepository _osvedceniRepo;
    private readonly IPrihlaskaRepository _prihlaskaRepo;
    private readonly IAttendanceService _attendanceService;
    private readonly IUnitOfWork _unitOfWork;

    public CertificateService(
        IOsvedceniRepository osvedceniRepo,
        IPrihlaskaRepository prihlaskaRepo,
        IAttendanceService attendanceService,
        IUnitOfWork unitOfWork)
    {
        _osvedceniRepo = osvedceniRepo;
        _prihlaskaRepo = prihlaskaRepo;
        _attendanceService = attendanceService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Osvedceni> IssueCertificateAsync(
        Guid prihlaskaId, 
        VysledekHodnoceni vysledek, 
        string vystavil, 
        CancellationToken ct = default)
    {
        var prihlaska = await _prihlaskaRepo.GetWithDetailsAsync(prihlaskaId, ct)
            ?? throw new EntityNotFoundException(nameof(Prihlaska), prihlaskaId);

        if (prihlaska.Stav == StavPrihlasky.Zruseno || prihlaska.Stav == StavPrihlasky.Expirovano)
        {
            throw new DomainValidationException("Osvědčení nelze vystavit pro zrušenou či expirovanou přihlášku.");
        }

        // Obchodní pravidlo klienta: Již vydané osvědčení je neměnné, číslo se nesmí opakovat
        if (prihlaska.Osvedceni != null)
        {
            throw new DomainValidationException(
                $"K této přihlášce již bylo vystaveno osvědčení č. {prihlaska.Osvedceni.CisloOsvedceni} a nelze jej změnit.");
        }

        // Krok 1 (Rozhodnutí č. 1): Technické ověření minimální docházky >= 70 %
        var attendanceSummary = await _attendanceService.GetStudentAttendanceSummaryAsync(
            prihlaska.StudentId, 
            prihlaska.TerminId, 
            ct);

        if (attendanceSummary.CelkemHodin > 0 && attendanceSummary.ProcentoDochazky < 70.0)
        {
            throw new InvalidAttendanceException(
                $"Student {attendanceSummary.CeleJmeno} dosáhl pouze {attendanceSummary.ProcentoDochazky:F1}% docházky. " +
                $"Pro akreditované vydání osvědčení je zákonnou podmínkou minimálně 70.0 % docházky.");
        }

        // Krok 2 (Rozhodnutí č. 1): Finální potvrzení lektorem
        if (vysledek != VysledekHodnoceni.Prospel && vysledek != VysledekHodnoceni.ProspelSVyznamenanim)
        {
            throw new DomainValidationException(
                "Osvědčení lze vystavit pouze pro studenty, kteří u lektora prospěli.");
        }

        // Krok 3: Generování globálně jedinečného evidenčního čísla (např. AT-2026-0001)
        int currentYear = DateTime.UtcNow.Year;
        int existingCountThisYear = await _osvedceniRepo.GetCountForYearAsync(currentYear, ct);
        int sequenceNumber = existingCountThisYear + 1;
        string cisloOsvedceni = $"AT-{currentYear}-{sequenceNumber:D4}";

        // Ověření absolutní unikátnosti čísla
        var existingWithNumber = await _osvedceniRepo.GetByNumberAsync(cisloOsvedceni, ct);
        if (existingWithNumber != null)
        {
            // Bezpečnostní pojistka proti souběhu
            sequenceNumber++;
            cisloOsvedceni = $"AT-{currentYear}-{sequenceNumber:D4}";
        }

        var osvedceni = new Osvedceni
        {
            Id = Guid.NewGuid(),
            PrihlaskaId = prihlaskaId,
            CisloOsvedceni = cisloOsvedceni,
            DatumVydani = DateOnly.FromDateTime(DateTime.UtcNow),
            Vysledek = vysledek,
            DosazenaDochazkaProcent = attendanceSummary.ProcentoDochazky,
            Vystavil = string.IsNullOrWhiteSpace(vystavil) ? "Ing. Lucie Bartošová" : vystavil
        };

        // Změna stavu přihlášky na Dokončeno
        prihlaska.Stav = StavPrihlasky.Dokonceno;
        _prihlaskaRepo.Update(prihlaska);

        await _osvedceniRepo.AddAsync(osvedceni, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return osvedceni;
    }

    public async Task<Osvedceni?> GetCertificateByNumberAsync(string cisloOsvedceni, CancellationToken ct = default)
    {
        return await _osvedceniRepo.GetByNumberAsync(cisloOsvedceni, ct);
    }

    public async Task<List<Osvedceni>> GetCertificatesForTerminAsync(Guid terminId, CancellationToken ct = default)
    {
        return await _osvedceniRepo.GetForTerminAsync(terminId, ct);
    }
}
