using AkademieTrutnov.Core.Entities;
using AkademieTrutnov.Core.Enums;
using AkademieTrutnov.Core.Exceptions;
using AkademieTrutnov.Core.Services;
using AkademieTrutnov.Infrastructure.Data;
using AkademieTrutnov.Infrastructure.Repositories;
using Xunit;

namespace AkademieTrutnov.Tests;

public class BusinessRulesTests
{
    private (RegistrationService regService, AttendanceService attService, CertificateService certService, ReportingService repService, AppDbContext context) 
        CreateServices(string dbName)
    {
        var context = TestDbContextFactory.Create(dbName);
        var uow = new UnitOfWork(context);
        var prihlaskaRepo = new PrihlaskaRepository(context);
        var terminRepo = new TerminRepository(context);
        var studentRepo = new StudentRepository(context);
        var kurzRepo = new KurzRepository(context);
        var osvedceniRepo = new OsvedceniRepository(context);
        var lektorRepo = new LektorRepository(context);
        var dochazkaRepo = new DochazkaRepository(context);

        var regService = new RegistrationService(prihlaskaRepo, terminRepo, studentRepo, uow);
        var attService = new AttendanceService(dochazkaRepo, terminRepo, studentRepo, uow);
        var certService = new CertificateService(osvedceniRepo, prihlaskaRepo, attService, uow);
        var repService = new ReportingService(terminRepo, lektorRepo, studentRepo, prihlaskaRepo);

        return (regService, attService, certService, repService, context);
    }

    private async Task<(Kurz kurz, Ucebna ucebna, Termin termin)> SeedBasicScheduleAsync(AppDbContext context, int kapacita = 2)
    {
        var kurz = new Kurz
        {
            Id = Guid.NewGuid(),
            Nazev = "Testovací kurz C#",
            Anotace = "Anotace testu",
            RozsahHodin = 80,
            Cena = 10000m,
            Akreditovano = true
        };

        var ucebna = new Ucebna
        {
            Id = Guid.NewGuid(),
            Nazev = "Testovací učebna",
            Adresa = "Trutnov",
            Kapacita = kapacita
        };

        var termin = new Termin
        {
            Id = Guid.NewGuid(),
            KurzId = kurz.Id,
            Kurz = kurz,
            UcebnaId = ucebna.Id,
            Ucebna = ucebna,
            DatumOd = DateTime.UtcNow.AddDays(5),
            DatumDo = DateTime.UtcNow.AddDays(25),
            Kapacita = kapacita,
            Stav = StavTerminu.Planovany
        };

        await context.Kurzy.AddAsync(kurz);
        await context.Ucebny.AddAsync(ucebna);
        await context.Terminy.AddAsync(termin);
        await context.SaveChangesAsync();

        return (kurz, ucebna, termin);
    }

    private async Task<Student> CreateStudentAsync(AppDbContext context, string jmeno, string prijmeni)
    {
        var student = new Student
        {
            Id = Guid.NewGuid(),
            Jmeno = jmeno,
            Prijmeni = prijmeni,
            Email = $"{jmeno.ToLower()}.{prijmeni.ToLower()}@test.cz",
            Telefon = "+420 123 456 789"
        };
        await context.Studenti.AddAsync(student);
        await context.SaveChangesAsync();
        return student;
    }

    [Fact]
    public async Task CapacityEnforcement_WhenFull_ShouldPlaceInWaitlistWithOrderedIndex()
    {
        var (regService, _, _, _, context) = CreateServices(nameof(CapacityEnforcement_WhenFull_ShouldPlaceInWaitlistWithOrderedIndex));
        var (_, _, termin) = await SeedBasicScheduleAsync(context, kapacita: 2);

        var s1 = await CreateStudentAsync(context, "Student", "Jedna");
        var s2 = await CreateStudentAsync(context, "Student", "Dva");
        var s3 = await CreateStudentAsync(context, "Student", "Tri");
        var s4 = await CreateStudentAsync(context, "Student", "Ctyri");

        // 1. a 2. student se vejdou do kapacity
        var r1 = await regService.RegisterStudentAsync(s1.Id, termin.Id);
        var r2 = await regService.RegisterStudentAsync(s2.Id, termin.Id);
        Assert.Equal(StavPrihlasky.Prihlasen, r1.Stav);
        Assert.Equal(StavPrihlasky.Prihlasen, r2.Stav);

        // 3. student jde do pořadníku s číslem 1
        var r3 = await regService.RegisterStudentAsync(s3.Id, termin.Id);
        Assert.Equal(StavPrihlasky.Poradnik, r3.Stav);
        Assert.Equal(1, r3.PoradiVPoradniku);

        // 4. student jde do pořadníku s číslem 2
        var r4 = await regService.RegisterStudentAsync(s4.Id, termin.Id);
        Assert.Equal(StavPrihlasky.Poradnik, r4.Stav);
        Assert.Equal(2, r4.PoradiVPoradniku);
    }

    [Fact]
    public async Task SpotVacated_ShouldAutomaticallyOfferToFirstInWaitlist_AndReindexRemaining()
    {
        var (regService, _, _, _, context) = CreateServices(nameof(SpotVacated_ShouldAutomaticallyOfferToFirstInWaitlist_AndReindexRemaining));
        var (_, _, termin) = await SeedBasicScheduleAsync(context, kapacita: 1);

        var s1 = await CreateStudentAsync(context, "Zapsany", "Student");
        var s2 = await CreateStudentAsync(context, "Prvni", "Nahradnik");
        var s3 = await CreateStudentAsync(context, "Druhy", "Nahradnik");

        var r1 = await regService.RegisterStudentAsync(s1.Id, termin.Id);
        var r2 = await regService.RegisterStudentAsync(s2.Id, termin.Id);
        var r3 = await regService.RegisterStudentAsync(s3.Id, termin.Id);

        Assert.Equal(StavPrihlasky.Prihlasen, r1.Stav);
        Assert.Equal(1, r2.PoradiVPoradniku);
        Assert.Equal(2, r3.PoradiVPoradniku);

        // Zapsaný student zruší účast
        await regService.CancelRegistrationAsync(r1.PrihlaskaId, "Nemohu se zúčastnit");

        // První náhradník dostane nabídku (Stav NABIDNUTO s 24h lhůtou - Rozhodnutí č. 2)
        var p2After = await context.Prihlasky.FindAsync(r2.PrihlaskaId);
        Assert.NotNull(p2After);
        Assert.Equal(StavPrihlasky.Nabidnuto, p2After.Stav);
        Assert.NotNull(p2After.NabidkaExpirace);
        Assert.True(p2After.NabidkaExpirace > DateTime.UtcNow);

        // Druhý náhradník se posune na 1. pozici v pořadníku
        var p3After = await context.Prihlasky.FindAsync(r3.PrihlaskaId);
        Assert.NotNull(p3After);
        Assert.Equal(StavPrihlasky.Poradnik, p3After.Stav);
        Assert.Equal(1, p3After.PoradiVPoradniku);
    }

    [Fact]
    public async Task CancelledRegistration_MustNotBePhysicallyDeleted_MustRetainReasonAndTimestamp()
    {
        var (regService, _, _, _, context) = CreateServices(nameof(CancelledRegistration_MustNotBePhysicallyDeleted_MustRetainReasonAndTimestamp));
        var (_, _, termin) = await SeedBasicScheduleAsync(context, kapacita: 5);
        var student = await CreateStudentAsync(context, "Karel", "Stornovany");

        var reg = await regService.RegisterStudentAsync(student.Id, termin.Id);
        string duvod = "Změna zaměstnání a stěhování do jiného města.";

        await regService.CancelRegistrationAsync(reg.PrihlaskaId, duvod);

        // Záznam v databázi stále existuje (fyzicky se nemaže)
        var prihlaskaInDb = await context.Prihlasky.FindAsync(reg.PrihlaskaId);
        Assert.NotNull(prihlaskaInDb);
        Assert.Equal(StavPrihlasky.Zruseno, prihlaskaInDb.Stav);
        Assert.Equal(duvod, prihlaskaInDb.DuvodZruseni);
        Assert.NotNull(prihlaskaInDb.DatumZruseni);
    }

    [Fact]
    public async Task DuplicateEnrollment_OnSameCourseActiveSchedule_ShouldBeForbidden()
    {
        var (regService, _, _, _, context) = CreateServices(nameof(DuplicateEnrollment_OnSameCourseActiveSchedule_ShouldBeForbidden));
        var (kurz, ucebna, termin1) = await SeedBasicScheduleAsync(context, kapacita: 5);

        // Vytvoříme druhý termín stejného kurzu
        var termin2 = new Termin
        {
            Id = Guid.NewGuid(),
            KurzId = kurz.Id,
            UcebnaId = ucebna.Id,
            DatumOd = DateTime.UtcNow.AddDays(30),
            DatumDo = DateTime.UtcNow.AddDays(50),
            Kapacita = 5,
            Stav = StavTerminu.Planovany
        };
        await context.Terminy.AddAsync(termin2);
        await context.SaveChangesAsync();

        var student = await CreateStudentAsync(context, "Pavel", "Dvojity");

        // První zápis uspěje
        await regService.RegisterStudentAsync(student.Id, termin1.Id);

        // Druhý souběžný zápis na tentýž kurz musí vyhodit výjimku (Rozhodnutí č. 3)
        await Assert.ThrowsAsync<DuplicateEnrollmentException>(async () =>
        {
            await regService.RegisterStudentAsync(student.Id, termin2.Id);
        });
    }

    [Fact]
    public async Task Certificate_WhenAttendanceBelow70Percent_MustBeRejected()
    {
        var (regService, attService, certService, _, context) = CreateServices(nameof(Certificate_WhenAttendanceBelow70Percent_MustBeRejected));
        var (_, _, termin) = await SeedBasicScheduleAsync(context, kapacita: 5);
        var student = await CreateStudentAsync(context, "Absence", "Student");

        var reg = await regService.RegisterStudentAsync(student.Id, termin.Id);

        // Vytvoříme 10 dní výuky (každý po 8 hodinách = 80 hodin celkem)
        var dny = new List<DenVyuky>();
        for (int i = 1; i <= 10; i++)
        {
            var den = new DenVyuky
            {
                Id = Guid.NewGuid(),
                TerminId = termin.Id,
                Datum = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(i)),
                PocetHodin = 8
            };
            dny.Add(den);
            await context.DnyVyuky.AddAsync(den);
        }
        await context.SaveChangesAsync();

        // Student byl přítomen pouze na 5 dnech z 10 (50 % docházka < 70 %)
        for (int i = 0; i < 10; i++)
        {
            bool pritomen = i < 5;
            await attService.RecordAttendanceAsync(dny[i].Id, new() { new(student.Id, pritomen, null) });
        }

        // Pokus o vystavení osvědčení musí selhat na nedostatečnou docházku (Rozhodnutí č. 1)
        await Assert.ThrowsAsync<InvalidAttendanceException>(async () =>
        {
            await certService.IssueCertificateAsync(reg.PrihlaskaId, VysledekHodnoceni.Prospel, "Zkušební komisař");
        });
    }

    [Fact]
    public async Task Certificate_WhenAttendanceAtLeast70PercentAndPassed_ShouldIssueWithUniqueAuditNumber()
    {
        var (regService, attService, certService, _, context) = CreateServices(nameof(Certificate_WhenAttendanceAtLeast70PercentAndPassed_ShouldIssueWithUniqueAuditNumber));
        var (_, _, termin) = await SeedBasicScheduleAsync(context, kapacita: 5);
        var student = await CreateStudentAsync(context, "Uspesny", "Student");

        var reg = await regService.RegisterStudentAsync(student.Id, termin.Id);

        // Vytvoříme 10 dní výuky
        var dny = new List<DenVyuky>();
        for (int i = 1; i <= 10; i++)
        {
            var den = new DenVyuky
            {
                Id = Guid.NewGuid(),
                TerminId = termin.Id,
                Datum = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(i)),
                PocetHodin = 8
            };
            dny.Add(den);
            await context.DnyVyuky.AddAsync(den);
        }
        await context.SaveChangesAsync();

        // Student byl přítomen 8 dní z 10 (80 % >= 70 %)
        for (int i = 0; i < 10; i++)
        {
            bool pritomen = i < 8;
            await attService.RecordAttendanceAsync(dny[i].Id, new() { new(student.Id, pritomen, null) });
        }

        // Vystavení osvědčení
        var cert = await certService.IssueCertificateAsync(reg.PrihlaskaId, VysledekHodnoceni.Prospel, "Ing. Lucie Bartošová");

        Assert.NotNull(cert);
        Assert.StartsWith($"AT-{DateTime.UtcNow.Year}-", cert.CisloOsvedceni);
        Assert.Equal(80.0, cert.DosazenaDochazkaProcent);
        Assert.Equal(VysledekHodnoceni.Prospel, cert.Vysledek);

        // Přihláška je označena jako Dokonceno
        var prihlaskaAfter = await context.Prihlasky.FindAsync(reg.PrihlaskaId);
        Assert.Equal(StavPrihlasky.Dokonceno, prihlaskaAfter!.Stav);

        // Neměnnost: opakovaný pokus o vydání osvědčení k téže přihlášce musí selhat
        await Assert.ThrowsAsync<DomainValidationException>(async () =>
        {
            await certService.IssueCertificateAsync(reg.PrihlaskaId, VysledekHodnoceni.Prospel, "Někdo jiný");
        });
    }

    [Fact]
    public async Task OfferAcceptance_WithinTimeLimit_ShouldEnrolStudent()
    {
        var (regService, _, _, _, context) = CreateServices(nameof(OfferAcceptance_WithinTimeLimit_ShouldEnrolStudent));
        var (_, _, termin) = await SeedBasicScheduleAsync(context, kapacita: 1);
        var s1 = await CreateStudentAsync(context, "Prvni", "Clen");
        var s2 = await CreateStudentAsync(context, "Nahradnik", "Akceptujici");

        var r1 = await regService.RegisterStudentAsync(s1.Id, termin.Id);
        var r2 = await regService.RegisterStudentAsync(s2.Id, termin.Id);

        // Uvolnění místa
        await regService.CancelRegistrationAsync(r1.PrihlaskaId, "Storno");

        // Náhradník r2 potvrdí nabídku
        var accepted = await regService.AcceptOfferAsync(r2.PrihlaskaId);
        Assert.Equal(StavPrihlasky.Prihlasen, accepted.Stav);
        Assert.Null(accepted.NabidkaExpirace);
        Assert.Null(accepted.PoradiVPoradniku);
    }
}
