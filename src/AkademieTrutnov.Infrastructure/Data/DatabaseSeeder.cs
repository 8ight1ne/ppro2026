using AkademieTrutnov.Core.Entities;
using AkademieTrutnov.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace AkademieTrutnov.Infrastructure.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        // Kontrola, zda již data existují
        if (await context.Kurzy.AnyAsync())
        {
            return;
        }

        // 1. Kurzy
        var kurzCsharp = new Kurz
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Nazev = "Rekvalifikace: Programátor v jazyce C# a .NET",
            Anotace = "Komplexní rekvalifikační kurz zaměřený na základy C#, objektově orientované programování, EF Core a webový vývoj v ASP.NET Core.",
            RozsahHodin = 160,
            Cena = 24900m,
            Akreditovano = true
        };

        var kurzUcetnictvi = new Kurz
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Nazev = "Rekvalifikace: Účetnictví a daňová evidence",
            Anotace = "Praktický rekvalifikační kurz pro budoucí účetní pokrývající podvojné účetnictví, mzdy a daňová přiznání dle platné legislativy ČR.",
            RozsahHodin = 120,
            Cena = 18500m,
            Akreditovano = true
        };

        var kurzMarketing = new Kurz
        {
            Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            Nazev = "Digitální marketing a sociální sítě",
            Anotace = "Kurz pro marketingové specialisty a podnikatele: SEO, PPC kampaně, obsahový marketing na Meta a LinkedIn.",
            RozsahHodin = 60,
            Cena = 12500m,
            Akreditovano = true
        };

        var kurzManazer = new Kurz
        {
            Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
            Nazev = "Manažerské dovednosti a vedení týmu",
            Anotace = "Intenzivní rozvoj vedení lidí, asertivity, motivace a zvládání krizových situací pro team leadery a vedoucí pracovníky.",
            RozsahHodin = 40,
            Cena = 8900m,
            Akreditovano = false
        };

        await context.Kurzy.AddRangeAsync(kurzCsharp, kurzUcetnictvi, kurzMarketing, kurzManazer);

        // 2. Učebny
        var ucebnaLab = new Ucebna
        {
            Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            Nazev = "Učebna B2 - Počítačová laboratoř",
            Adresa = "Horská 14, Trutnov",
            Kapacita = 5 // Malá kapacita pro přehlednou demonstraci pořadníku
        };

        var ucebnaSal = new Ucebna
        {
            Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
            Nazev = "Učebna A1 - Přednáškový sál",
            Adresa = "Horská 14, Trutnov",
            Kapacita = 20
        };

        var ucebnaSem = new Ucebna
        {
            Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
            Nazev = "Učebna C3 - Seminární místnost",
            Adresa = "Polská 88, Trutnov",
            Kapacita = 12
        };

        await context.Ucebny.AddRangeAsync(ucebnaLab, ucebnaSal, ucebnaSem);

        // 3. Lektoři
        var lektorDvorak = new Lektor
        {
            Id = Guid.Parse("d1111111-1111-1111-1111-111111111111"),
            Jmeno = "Ing. Martin",
            Prijmeni = "Dvořák",
            Email = "m.dvorak@akademietrutnov.cz",
            Telefon = "+420 777 345 678",
            Externista = true,
            Specializace = "Softwarový vývoj a cloudová architektura"
        };

        var lektorNovotny = new Lektor
        {
            Id = Guid.Parse("d2222222-2222-2222-2222-222222222222"),
            Jmeno = "Ing. Petr",
            Prijmeni = "Novotný",
            Email = "p.novotny@akademietrutnov.cz",
            Telefon = "+420 777 123 456",
            Externista = false,
            Specializace = "Finanční účetnictví a daně"
        };

        var lektorkaHorakova = new Lektor
        {
            Id = Guid.Parse("d3333333-3333-3333-3333-333333333333"),
            Jmeno = "Mgr. Eva",
            Prijmeni = "Horáková",
            Email = "e.horakova@akademietrutnov.cz",
            Telefon = "+420 777 234 567",
            Externista = true,
            Specializace = "Online marketing a analytika"
        };

        var lektorkaKralova = new Lektor
        {
            Id = Guid.Parse("d4444444-4444-4444-4444-444444444444"),
            Jmeno = "PhDr. Jitka",
            Prijmeni = "Králová",
            Email = "j.kralova@akademietrutnov.cz",
            Telefon = "+420 777 456 789",
            Externista = true,
            Specializace = "Leadership a personální řízení"
        };

        await context.Lektori.AddRangeAsync(lektorDvorak, lektorNovotny, lektorkaHorakova, lektorkaKralova);

        // 4. Termíny
        // Termín 1: C# - plně obsazen (kapacita 5), probíhá, s náhradníky v pořadníku
        var terminCsharp = new Termin
        {
            Id = Guid.Parse("t1111111-1111-1111-1111-111111111111"),
            KurzId = kurzCsharp.Id,
            UcebnaId = ucebnaLab.Id,
            DatumOd = DateTime.UtcNow.AddDays(-10),
            DatumDo = DateTime.UtcNow.AddDays(30),
            Kapacita = ucebnaLab.Kapacita,
            Stav = StavTerminu.Probiha
        };

        // Termín 2: Účetnictví - plánovaný
        var terminUcetnictvi = new Termin
        {
            Id = Guid.Parse("t2222222-2222-2222-2222-222222222222"),
            KurzId = kurzUcetnictvi.Id,
            UcebnaId = ucebnaSal.Id,
            DatumOd = DateTime.UtcNow.AddDays(14),
            DatumDo = DateTime.UtcNow.AddDays(45),
            Kapacita = ucebnaSal.Kapacita,
            Stav = StavTerminu.Planovany
        };

        // Termín 3: Marketing - již ukončený, s vystavenými osvědčeními
        var terminMarketing = new Termin
        {
            Id = Guid.Parse("t3333333-3333-3333-3333-333333333333"),
            KurzId = kurzMarketing.Id,
            UcebnaId = ucebnaSem.Id,
            DatumOd = DateTime.UtcNow.AddDays(-60),
            DatumDo = DateTime.UtcNow.AddDays(-15),
            Kapacita = ucebnaSem.Kapacita,
            Stav = StavTerminu.Ukoncen
        };

        // Termín 4: Manažerské dovednosti - plánovaný
        var terminManazer = new Termin
        {
            Id = Guid.Parse("t4444444-4444-4444-4444-444444444444"),
            KurzId = kurzManazer.Id,
            UcebnaId = ucebnaSem.Id,
            DatumOd = DateTime.UtcNow.AddDays(30),
            DatumDo = DateTime.UtcNow.AddDays(35),
            Kapacita = ucebnaSem.Kapacita,
            Stav = StavTerminu.Planovany
        };

        await context.Terminy.AddRangeAsync(terminCsharp, terminUcetnictvi, terminMarketing, terminManazer);

        // 5. Přiřazení lektorů k termínům (TerminLektor - M:N)
        // C# kurz má 2 lektory, kteří se střídají (Rozhodnutí č. 5)
        var tl1 = new TerminLektor
        {
            TerminId = terminCsharp.Id,
            LektorId = lektorDvorak.Id,
            PocetHodin = 100,
            Role = "Garant a hlavní lektor programování"
        };
        var tl2 = new TerminLektor
        {
            TerminId = terminCsharp.Id,
            LektorId = lektorNovotny.Id,
            PocetHodin = 60,
            Role = "Lektor databázových systémů"
        };
        var tl3 = new TerminLektor
        {
            TerminId = terminUcetnictvi.Id,
            LektorId = lektorNovotny.Id,
            PocetHodin = 120,
            Role = "Hlavní lektor účetnictví"
        };
        var tl4 = new TerminLektor
        {
            TerminId = terminMarketing.Id,
            LektorId = lektorkaHorakova.Id,
            PocetHodin = 60,
            Role = "Hlavní lektorka"
        };
        var tl5 = new TerminLektor
        {
            TerminId = terminManazer.Id,
            LektorId = lektorkaKralova.Id,
            PocetHodin = 40,
            Role = "Hlavní lektorka"
        };

        await context.TerminLektori.AddRangeAsync(tl1, tl2, tl3, tl4, tl5);

        // 6. Studenti
        var s1 = new Student { Id = Guid.Parse("s1111111-1111-1111-1111-111111111111"), Jmeno = "Jan", Prijmeni = "Novák", Email = "jan.novak@example.cz", Telefon = "+420 601 111 222", Organizace = "Auto Kelly Trutnov" };
        var s2 = new Student { Id = Guid.Parse("s2222222-2222-2222-2222-222222222222"), Jmeno = "Marie", Prijmeni = "Svobodová", Email = "marie.svobodova@example.cz", Telefon = "+420 602 222 333", Organizace = null };
        var s3 = new Student { Id = Guid.Parse("s3333333-3333-3333-3333-333333333333"), Jmeno = "Petr", Prijmeni = "Černý", Email = "petr.cerny@example.cz", Telefon = "+420 603 333 444", Organizace = "Siemens Trutnov" };
        var s4 = new Student { Id = Guid.Parse("s4444444-4444-4444-4444-444444444444"), Jmeno = "Lucie", Prijmeni = "Kučerová", Email = "lucie.kucerova@example.cz", Telefon = "+420 604 444 555", Organizace = null };
        var s5 = new Student { Id = Guid.Parse("s5555555-5555-5555-5555-555555555555"), Jmeno = "David", Prijmeni = "Pospíšil", Email = "david.pospisil@example.cz", Telefon = "+420 605 555 666", Organizace = "Continental" };
        var s6 = new Student { Id = Guid.Parse("s6666666-6666-6666-6666-666666666666"), Jmeno = "Alena", Prijmeni = "Veselá", Email = "alena.vesela@example.cz", Telefon = "+420 606 666 777", Organizace = null };
        var s7 = new Student { Id = Guid.Parse("s7777777-7777-7777-7777-777777777777"), Jmeno = "Michal", Prijmeni = "Mareš", Email = "michal.mares@example.cz", Telefon = "+420 607 777 888", Organizace = null };
        var s8 = new Student { Id = Guid.Parse("s8888888-8888-8888-8888-888888888888"), Jmeno = "Veronika", Prijmeni = "Němcová", Email = "veronika.nemcova@example.cz", Telefon = "+420 608 888 999", Organizace = "Krkonošské papírny" };

        await context.Studenti.AddRangeAsync(s1, s2, s3, s4, s5, s6, s7, s8);

        // 7. Přihlášky pro C# (kapacita 5)
        // 4 řádně přihlášení
        var p1 = new Prihlaska { Id = Guid.NewGuid(), StudentId = s1.Id, TerminId = terminCsharp.Id, DatumPrihlaseni = DateTime.UtcNow.AddDays(-20), Stav = StavPrihlasky.Prihlasen };
        var p2 = new Prihlaska { Id = Guid.NewGuid(), StudentId = s2.Id, TerminId = terminCsharp.Id, DatumPrihlaseni = DateTime.UtcNow.AddDays(-19), Stav = StavPrihlasky.Prihlasen };
        var p3 = new Prihlaska { Id = Guid.NewGuid(), StudentId = s3.Id, TerminId = terminCsharp.Id, DatumPrihlaseni = DateTime.UtcNow.AddDays(-18), Stav = StavPrihlasky.Prihlasen };
        var p4 = new Prihlaska { Id = Guid.NewGuid(), StudentId = s4.Id, TerminId = terminCsharp.Id, DatumPrihlaseni = DateTime.UtcNow.AddDays(-17), Stav = StavPrihlasky.Prihlasen };
        
        // 1 přihláška zrušená (historie zůstává zachována dle obchodního pravidla)
        var pZrusena = new Prihlaska 
        { 
            Id = Guid.NewGuid(), 
            StudentId = s5.Id, 
            TerminId = terminCsharp.Id, 
            DatumPrihlaseni = DateTime.UtcNow.AddDays(-16), 
            Stav = StavPrihlasky.Zruseno,
            DatumZruseni = DateTime.UtcNow.AddHours(-3),
            DuvodZruseni = "Odřeknutí z rodinných důvodů."
        };

        // 1 student, kterému bylo po zrušení nabídnuto uvolněné místo (Rozhodnutí č. 2 - Stav NABIDNUTO s expirací)
        var pNabidnuto = new Prihlaska
        {
            Id = Guid.NewGuid(),
            StudentId = s6.Id,
            TerminId = terminCsharp.Id,
            DatumPrihlaseni = DateTime.UtcNow.AddDays(-15),
            Stav = StavPrihlasky.Nabidnuto,
            NabidkaExpirace = DateTime.UtcNow.AddHours(21) // ještě zbývá 21 hodin na potvrzení
        };

        // 2 náhradníci v pořadníku (1. a 2. místo)
        var pPoradnik1 = new Prihlaska
        {
            Id = Guid.NewGuid(),
            StudentId = s7.Id,
            TerminId = terminCsharp.Id,
            DatumPrihlaseni = DateTime.UtcNow.AddDays(-14),
            Stav = StavPrihlasky.Poradnik,
            PoradiVPoradniku = 1
        };

        var pPoradnik2 = new Prihlaska
        {
            Id = Guid.NewGuid(),
            StudentId = s8.Id,
            TerminId = terminCsharp.Id,
            DatumPrihlaseni = DateTime.UtcNow.AddDays(-13),
            Stav = StavPrihlasky.Poradnik,
            PoradiVPoradniku = 2
        };

        // Přihlášky a osvědčení pro ukončený kurz marketingu
        var pMark1 = new Prihlaska
        {
            Id = Guid.NewGuid(),
            StudentId = s1.Id, // Student Jan Novák je přihlášen na C# i marketing (report více kurzů!)
            TerminId = terminMarketing.Id,
            DatumPrihlaseni = DateTime.UtcNow.AddDays(-70),
            Stav = StavPrihlasky.Dokonceno
        };

        var pMark2 = new Prihlaska
        {
            Id = Guid.NewGuid(),
            StudentId = s2.Id, // Studentka Marie Svobodová je také na obou kurzech!
            TerminId = terminMarketing.Id,
            DatumPrihlaseni = DateTime.UtcNow.AddDays(-69),
            Stav = StavPrihlasky.Dokonceno
        };

        await context.Prihlasky.AddRangeAsync(p1, p2, p3, p4, pZrusena, pNabidnuto, pPoradnik1, pPoradnik2, pMark1, pMark2);

        // 8. Dny výuky a docházka
        // Marketing - proběhlo 5 výukových dnů
        for (int i = 1; i <= 5; i++)
        {
            var den = new DenVyuky
            {
                Id = Guid.NewGuid(),
                TerminId = terminMarketing.Id,
                LektorId = lektorkaHorakova.Id,
                Datum = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-55 + (i * 7))),
                PocetHodin = 12,
                Tema = $"Výukový blok {i}: Praktická marketingová strategie a nástroje"
            };
            await context.DnyVyuky.AddAsync(den);

            // Zápis docházky pro oba studenty (100% účast)
            var doch1 = new Dochazka { Id = Guid.NewGuid(), DenVyukyId = den.Id, StudentId = s1.Id, Pritomen = true };
            var doch2 = new Dochazka { Id = Guid.NewGuid(), DenVyukyId = den.Id, StudentId = s2.Id, Pritomen = true };
            await context.Dochazky.AddRangeAsync(doch1, doch2);
        }

        // C# kurz - proběhly první 2 výukové dny
        var denCs1 = new DenVyuky
        {
            Id = Guid.NewGuid(),
            TerminId = terminCsharp.Id,
            LektorId = lektorDvorak.Id,
            Datum = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7)),
            PocetHodin = 8,
            Tema = "Základy syntaxe C# a objektový návrh"
        };
        var denCs2 = new DenVyuky
        {
            Id = Guid.NewGuid(),
            TerminId = terminCsharp.Id,
            LektorId = lektorDvorak.Id,
            Datum = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
            PocetHodin = 8,
            Tema = "Kolekce, LINQ a práce s daty"
        };
        await context.DnyVyuky.AddRangeAsync(denCs1, denCs2);

        // Zápis docházky na C#
        await context.Dochazky.AddRangeAsync(
            new Dochazka { Id = Guid.NewGuid(), DenVyukyId = denCs1.Id, StudentId = s1.Id, Pritomen = true },
            new Dochazka { Id = Guid.NewGuid(), DenVyukyId = denCs1.Id, StudentId = s2.Id, Pritomen = true },
            new Dochazka { Id = Guid.NewGuid(), DenVyukyId = denCs1.Id, StudentId = s3.Id, Pritomen = true },
            new Dochazka { Id = Guid.NewGuid(), DenVyukyId = denCs1.Id, StudentId = s4.Id, Pritomen = false, Poznamka = "Omluven" },
            new Dochazka { Id = Guid.NewGuid(), DenVyukyId = denCs2.Id, StudentId = s1.Id, Pritomen = true },
            new Dochazka { Id = Guid.NewGuid(), DenVyukyId = denCs2.Id, StudentId = s2.Id, Pritomen = true },
            new Dochazka { Id = Guid.NewGuid(), DenVyukyId = denCs2.Id, StudentId = s3.Id, Pritomen = true },
            new Dochazka { Id = Guid.NewGuid(), DenVyukyId = denCs2.Id, StudentId = s4.Id, Pritomen = true }
        );

        // 9. Vydaná osvědčení pro dokončený marketingový kurz (jedinečné evidenční číslo, neměnnost)
        int rok = DateTime.UtcNow.Year;
        var osvedceni1 = new Osvedceni
        {
            Id = Guid.NewGuid(),
            PrihlaskaId = pMark1.Id,
            CisloOsvedceni = $"AT-{rok}-0001",
            DatumVydani = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-15)),
            Vysledek = VysledekHodnoceni.ProspelSVyznamenanim,
            DosazenaDochazkaProcent = 100.0,
            Vystavil = "Ing. Lucie Bartošová"
        };

        var osvedceni2 = new Osvedceni
        {
            Id = Guid.NewGuid(),
            PrihlaskaId = pMark2.Id,
            CisloOsvedceni = $"AT-{rok}-0002",
            DatumVydani = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-15)),
            Vysledek = VysledekHodnoceni.Prospel,
            DosazenaDochazkaProcent = 100.0,
            Vystavil = "Ing. Lucie Bartošová"
        };

        await context.Osvedceni.AddRangeAsync(osvedceni1, osvedceni2);

        await context.SaveChangesAsync();
    }
}
