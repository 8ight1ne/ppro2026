using AkademieTrutnov.Core.DTOs;
using AkademieTrutnov.Core.Entities;
using AkademieTrutnov.Core.Enums;
using AkademieTrutnov.Core.Exceptions;
using AkademieTrutnov.Core.Interfaces;

namespace AkademieTrutnov.Core.Services;

public class RegistrationService : IRegistrationService
{
    private readonly IPrihlaskaRepository _prihlaskaRepo;
    private readonly ITerminRepository _terminRepo;
    private readonly IStudentRepository _studentRepo;
    private readonly IUnitOfWork _unitOfWork;

    public RegistrationService(
        IPrihlaskaRepository prihlaskaRepo,
        ITerminRepository terminRepo,
        IStudentRepository studentRepo,
        IUnitOfWork unitOfWork)
    {
        _prihlaskaRepo = prihlaskaRepo;
        _terminRepo = terminRepo;
        _studentRepo = studentRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<RegistrationResultDto> RegisterStudentAsync(
        Guid studentId, 
        Guid terminId, 
        bool isLateAdminEnrollment = false, 
        string? adminNote = null, 
        CancellationToken ct = default)
    {
        var student = await _studentRepo.GetByIdAsync(studentId, ct)
            ?? throw new EntityNotFoundException(nameof(Student), studentId);

        var termin = await _terminRepo.GetWithDetailsAsync(terminId, ct)
            ?? throw new EntityNotFoundException(nameof(Termin), terminId);

        if (termin.Stav == StavTerminu.Ukoncen || termin.Stav == StavTerminu.Zrusen)
        {
            throw new DomainValidationException($"Na termín ve stavu '{termin.Stav}' se již nelze přihlásit.");
        }

        // Pravidlo Rozhodnutí č. 3: Zákaz souběžného přihlášení na aktivní termíny téhož kurzu
        var activeApplications = await _prihlaskaRepo.GetActiveApplicationsForStudentAndCourseAsync(studentId, termin.KurzId, ct);
        if (activeApplications.Any(p => p.TerminId != terminId || (p.Stav != StavPrihlasky.Zruseno && p.Stav != StavPrihlasky.Expirovano)))
        {
            throw new DuplicateEnrollmentException(
                $"Student {student.CeleJmeno} již má aktivní přihlášku na kurz '{termin.Kurz?.Nazev ?? "tento kurz"}'. Souběžný zápis není povolen.");
        }

        // Pravidlo Rozhodnutí č. 4: Přihlášení po zahájení kurzu
        var now = DateTime.UtcNow;
        if (now >= termin.DatumOd)
        {
            if (!isLateAdminEnrollment)
            {
                throw new RegistrationLateException(
                    "Kurz již byl zahájen. Běžné přihlašování je uzamčeno. Dodatečný zápis může provést pouze administrátor s odůvodněním.");
            }

            if (string.IsNullOrWhiteSpace(adminNote))
            {
                throw new RegistrationLateException(
                    "Při dodatečném zápisu po zahájení kurzu je vyžadováno textové odůvodnění administrátora.");
            }

            // Kontrola pravidla: výhradně do doby, než proběhne 20 % hodinové dotace kurzu
            int totalHours = termin.Kurz?.RozsahHodin ?? (termin.DnyVyuky?.Sum(d => d.PocetHodin) ?? 0);
            if (totalHours > 0)
            {
                var today = DateOnly.FromDateTime(now);
                int passedHours = (termin.DnyVyuky ?? new List<DenVyuky>())
                    .Where(d => d.Datum < today)
                    .Sum(d => d.PocetHodin);

                double passedRatio = (double)passedHours / totalHours;
                if (passedRatio > 0.20)
                {
                    throw new RegistrationLateException(
                        $"Dodatečný zápis nelze provést. Z kurzu již proběhlo {passedRatio:P0} výuky (limit je 20 %), student by nemohl splnit povinnou 70% docházku.");
                }
            }
        }

        // Obchodní pravidlo klienta: Kapacitu termínu nelze překročit
        int activeCount = await _prihlaskaRepo.GetActiveEnrollmentCountForTerminAsync(terminId, ct);

        Prihlaska prihlaska;
        string message;

        if (activeCount < termin.Kapacita)
        {
            // Kapacita je volná - řádný zápis
            prihlaska = new Prihlaska
            {
                Id = Guid.NewGuid(),
                StudentId = studentId,
                TerminId = terminId,
                DatumPrihlaseni = now,
                Stav = StavPrihlasky.Prihlasen,
                PoradiVPoradniku = null,
                NabidkaExpirace = null,
                PoznamkaAdmina = adminNote
            };
            message = "Student byl úspěšně přihlášen do kurzu.";
        }
        else
        {
            // Kapacita je naplněna - zařazení do pořadníku (Rozhodnutí č. 2)
            var waitlist = await _prihlaskaRepo.GetWaitlistForTerminAsync(terminId, ct);
            int nextQueueNumber = waitlist.Count > 0 ? (waitlist.Max(p => p.PoradiVPoradniku ?? 0) + 1) : 1;

            prihlaska = new Prihlaska
            {
                Id = Guid.NewGuid(),
                StudentId = studentId,
                TerminId = terminId,
                DatumPrihlaseni = now,
                Stav = StavPrihlasky.Poradnik,
                PoradiVPoradniku = nextQueueNumber,
                NabidkaExpirace = null,
                PoznamkaAdmina = adminNote
            };
            message = $"Kapacita termínu ({termin.Kapacita} míst) je naplněna. Student byl zařazen do pořadníku náhradníků na {nextQueueNumber}. místo.";
        }

        await _prihlaskaRepo.AddAsync(prihlaska, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return new RegistrationResultDto(
            prihlaska.Id,
            prihlaska.Stav,
            prihlaska.PoradiVPoradniku,
            prihlaska.NabidkaExpirace,
            message
        );
    }

    public async Task<Prihlaska> CancelRegistrationAsync(
        Guid prihlaskaId, 
        string duvod, 
        CancellationToken ct = default)
    {
        var prihlaska = await _prihlaskaRepo.GetWithDetailsAsync(prihlaskaId, ct)
            ?? throw new EntityNotFoundException(nameof(Prihlaska), prihlaskaId);

        if (prihlaska.Stav == StavPrihlasky.Zruseno)
        {
            throw new DomainValidationException("Tato přihláška již byla zrušena.");
        }

        var oldState = prihlaska.Stav;
        int? oldQueuePos = prihlaska.PoradiVPoradniku;

        // Obchodní pravidlo klienta: Zrušená přihláška se NIKDY nemaže, uchovává se historie a důvod
        prihlaska.Stav = StavPrihlasky.Zruseno;
        prihlaska.DatumZruseni = DateTime.UtcNow;
        prihlaska.DuvodZruseni = string.IsNullOrWhiteSpace(duvod) ? "Zrušeno bez udání důvodu" : duvod;
        prihlaska.PoradiVPoradniku = null;
        prihlaska.NabidkaExpirace = null;
        _prihlaskaRepo.Update(prihlaska);

        if (oldState == StavPrihlasky.Prihlasen || oldState == StavPrihlasky.Nabidnuto)
        {
            // Místo se uvolnilo - nabídneme jej prvnímu v pořadníku (Rozhodnutí č. 2)
            await PromoteNextInWaitlistAsync(prihlaska.TerminId, ct);
        }
        else if (oldState == StavPrihlasky.Poradnik && oldQueuePos.HasValue)
        {
            // Pokud zrušil zájemce v pořadníku, posuneme náhradníky za ním (pořadí se nepřeskakuje)
            await ReindexWaitlistAsync(prihlaska.TerminId, ct);
        }

        await _unitOfWork.SaveChangesAsync(ct);
        return prihlaska;
    }

    public async Task<Prihlaska> AcceptOfferAsync(Guid prihlaskaId, CancellationToken ct = default)
    {
        var prihlaska = await _prihlaskaRepo.GetWithDetailsAsync(prihlaskaId, ct)
            ?? throw new EntityNotFoundException(nameof(Prihlaska), prihlaskaId);

        if (prihlaska.Stav != StavPrihlasky.Nabidnuto)
        {
            throw new DomainValidationException($"Přihláška není ve stavu nabídky (aktuální stav: {prihlaska.Stav}).");
        }

        if (prihlaska.NabidkaExpirace.HasValue && prihlaska.NabidkaExpirace.Value < DateTime.UtcNow)
        {
            // Nabídka vypršela
            prihlaska.Stav = StavPrihlasky.Expirovano;
            _prihlaskaRepo.Update(prihlaska);
            await PromoteNextInWaitlistAsync(prihlaska.TerminId, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            throw new DomainValidationException("Lhůta 24 hodin pro potvrzení nabídky vypršela. Místo bylo postoupeno dalšímu náhradníkovi.");
        }

        // Potvrzení nabídky: student se stává řádně přihlášeným
        prihlaska.Stav = StavPrihlasky.Prihlasen;
        prihlaska.NabidkaExpirace = null;
        prihlaska.PoradiVPoradniku = null;
        _prihlaskaRepo.Update(prihlaska);

        await _unitOfWork.SaveChangesAsync(ct);
        return prihlaska;
    }

    public async Task<Prihlaska> DeclineOfferAsync(Guid prihlaskaId, CancellationToken ct = default)
    {
        var prihlaska = await _prihlaskaRepo.GetWithDetailsAsync(prihlaskaId, ct)
            ?? throw new EntityNotFoundException(nameof(Prihlaska), prihlaskaId);

        if (prihlaska.Stav != StavPrihlasky.Nabidnuto)
        {
            throw new DomainValidationException($"Přihláška není ve stavu nabídky (aktuální stav: {prihlaska.Stav}).");
        }

        prihlaska.Stav = StavPrihlasky.Zruseno;
        prihlaska.DatumZruseni = DateTime.UtcNow;
        prihlaska.DuvodZruseni = "Náhradník odmítl nabídku uvolněného místa.";
        prihlaska.NabidkaExpirace = null;
        prihlaska.PoradiVPoradniku = null;
        _prihlaskaRepo.Update(prihlaska);

        // Uvolněné místo nabídneme dalšímu v pořadníku
        await PromoteNextInWaitlistAsync(prihlaska.TerminId, ct);

        await _unitOfWork.SaveChangesAsync(ct);
        return prihlaska;
    }

    public async Task<int> ProcessExpiredOffersAsync(CancellationToken ct = default)
    {
        var expiredOffers = await _prihlaskaRepo.GetExpiredOffersAsync(ct);
        int processedCount = 0;

        foreach (var offer in expiredOffers)
        {
            offer.Stav = StavPrihlasky.Expirovano;
            offer.DuvodZruseni = "Vypršela 24hodinová lhůta na potvrzení nabídky místa.";
            _prihlaskaRepo.Update(offer);

            await PromoteNextInWaitlistAsync(offer.TerminId, ct);
            processedCount++;
        }

        if (processedCount > 0)
        {
            await _unitOfWork.SaveChangesAsync(ct);
        }

        return processedCount;
    }

    private async Task PromoteNextInWaitlistAsync(Guid terminId, CancellationToken ct)
    {
        var nextInQueue = await _prihlaskaRepo.GetFirstWaitlistedForTerminAsync(terminId, ct);
        if (nextInQueue != null)
        {
            // Posuneme do stavu Nabidnuto s 24h lhůtou (Rozhodnutí č. 2)
            nextInQueue.Stav = StavPrihlasky.Nabidnuto;
            nextInQueue.NabidkaExpirace = DateTime.UtcNow.AddHours(24);
            nextInQueue.PoradiVPoradniku = null;
            _prihlaskaRepo.Update(nextInQueue);

            // Přečíslujeme zbývající náhradníky
            await ReindexWaitlistAsync(terminId, ct);
        }
    }

    private async Task ReindexWaitlistAsync(Guid terminId, CancellationToken ct)
    {
        var remainingWaitlist = await _prihlaskaRepo.GetWaitlistForTerminAsync(terminId, ct);
        int newOrder = 1;
        foreach (var waiting in remainingWaitlist.OrderBy(p => p.PoradiVPoradniku ?? int.MaxValue))
        {
            waiting.PoradiVPoradniku = newOrder++;
            _prihlaskaRepo.Update(waiting);
        }
    }
}
