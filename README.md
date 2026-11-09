# Akademie Trutnov – Správa a evidence kurzů (Zadání C)

Semestrální projekt do předmětu **Pokročilé programování (PPRO)**, zimní semestr 2026/2027, Fakulta informatiky a managementu, Univerzita Hradec Králové (FIM UHK).

> **Jediný zdroj pravdy (Single Source of Truth):**  
> Tento dokument slouží jako kompletní zadání, technická dokumentace a evidence architektonických i doménových rozhodnutí projektu. Jakékoliv změny v návrhu, datovém modelu či business pravidlech se vždy neprodleně zaznamenávají zde.

---

## 1. Zadání klienta

### Klient a kontext
- **Klient:** Ing. Lucie Bartošová, Akademie Trutnov
- **Činnost:** Poskytování rekvalifikačních a firemních kurzů (~40 běhů ročně).
- **Lektoři:** Externí odborníci.

### Jak to mají dnes (Současný stav)
- Přihlášky přicházejí přes webový formulář do e-mailu a ručně se přepisují do tabulkového procesoru (jedna záložka pro každý kurz).
- Kapacity učeben a termínů se hlídají pouze vizuální kontrolou tabulky.
- Pořadník náhradníků se vede jako tužkou psaná poznámka pod seznamem.
- Osvědčení se ručně vypisují v textovém editoru (Word), evidenční čísla se generují odhadem podle počtu vydaných v daném roce (již došlo k duplicitnímu přidělení stejného čísla).
- Kritická slabina: Pokud někdo na poslední chvíli účast odřekne, vedení centra složitě dohledává kontakty na náhradníky a kapacity zůstávají neobsazené.

### Co klient chce (Funkční požadavky)
1. **Kurzy:** Evidence kurzů (název, anotace, rozsah v hodinách, cena, příznak akreditace MŠMT).
2. **Termíny kurzu:** Možnost opakování kurzu v roce. Každý termín má datum od–do, přiřazené místo konání (učebnu) a kapacitu (kapacita kurzu se odvíjí od konkrétní učebny).
3. **Lektoři a jejich přiřazení k termínům:** Jeden lektor může vést více termínů, u vícedenních či delších kurzů se na jednom termínu střídají dva i více lektorů.
4. **Přihlášky studentů na termín:** Jeden student se může účastnit více kurzů, na jednom termínu je více studentů (vazba M:N).
5. **Kapacita a pořadník:** Při naplnění kapacity termínu je další zájemce zařazen do pořadníku náhradníků se zachováním pořadí. Při uvolnění místa je zřejmé, kdo je na řadě.
6. **Docházka:** U vícedenních kurzů evidence denní docházky (kdo byl který den přítomen).
7. **Dokončení a osvědčení:** Po skončení termínu vygenerování seznamu úspěšných absolventů a vystavení osvědčení s jedinečným evidenčním číslem a datem vydání.
8. **Přehledy a reporty:**
   - Vytíženost a naplněnost termínů.
   - Počet reálně odučených hodin jednotlivých lektorů.
   - Seznam studentů, kteří se v daném roce přihlásili na více než jeden kurz.

### Na čem klient trvá (Nepřekročitelná obchodní pravidla)
- **Kapacitu termínu nelze překročit** (žádné ruční přepisování limitu nad kapacitu učebny).
- **Pořadí v pořadníku se nesmí přeskakovat.**
- **Číslo osvědčení je striktně jedinečné** a jednou vydané osvědčení je neměnné (historický audit).
- **Zrušená přihláška zůstává v systému dohledatelná** (z důvodu řešení reklamací a sporů o přihlášení).

### Co klient prohodil mimochodem (Budoucí rozšíření)
- Fakturace a platební brána (zatím neřešit v MVP, ale počítat s budoucím napojením).
- Jednoduché, přehledné UI vhodné i pro méně technicky zdatné externí lektory.
- Automatické e-mailové notifikace (potvrzení přihlášky, výzva pro náhradníka).
- Příprava dat pro export do ministerských výkazů.

---

## 2. Oddíl: Rozhodnutí k otevřeným bodům

Podle pokynů k semestrálnímu projektu je zde jednoznačně zdokumentováno řešení všech nejasností a otevřených bodů klienta:

| # | Otevřený bod klienta | Přijaté rozhodnutí a jeho zdůvodnění |
|---|---|---|
| **1** | **Kdo rozhoduje o dokončení kurzu** *(70% docházka vs. posouzení lektorem)* | **Dvoustupňové ověření:** Systém vyžaduje splnění minimální docházky $\ge 70\ \%$ jako technickou prerekvizitu (podmínku nutnou pro akreditaci). Finální potvrzení úspěšného absolvování („Prospěl“ / „Neprospěl“) provádí lektor v rozhraní termínu na základě hodnocení praktických dovedností a testu. Tím jsou splněny formální požadavky akreditace i odborný úsudek lektora. |
| **2** | **Automatický posun z pořadníku** *(automatický zápis vs. manuální potvrzení)* | **Automatická nabídka s časovým limitem (Hold/Offer):** Při uvolnění kapacity systém automaticky nabídne místo prvnímu náhradníkovi v pořadníku a přepne jeho přihlášku do stavu `NABÍDNUTO` s lhůtou na potvrzení (24 hodin). Pokud zájemce potvrdí, stav přejde na `PŘIHLÁŠEN`. Pokud odmítne nebo lhůta vyprší, přihláška získá stav `EXPIROVÁNO` a systém osloví dalšího v pořadí. Pořadí se nikdy nepřeskakuje. |
| **3** | **Dva termíny téhož kurzu naráz** *(možnost vícenásobného zápisu)* | **Zákaz souběžného přihlášení na aktivní termíny téhož kurzu:** Jeden student může mít v jednom čase pouze jednu aktivní přihlášku na tentýž kurz, aby se zamezilo spekulativnímu blokování kapacit. Přihláška na nový termín stejného kurzu je možná až po řádném dokončení (nebo neúspěchu) či zrušení předchozí přihlášky. |
| **4** | **Přihlášení po zahájení kurzu** *(„nejde to, leda po domluvě“)* | **Systémový uzávěr s výjimkou administrátora:** Veřejné přihlašování se automaticky uzamyká v okamžiku data zahájení termínu. Dodatečné přihlášení může zadat pouze administrátor/recepce s povinným textovým odůvodněním, a to výhradně do doby, než proběhne 20 % hodinové dotace kurzu, aby student mohl reálně splnit podmínku 70% docházky. |
| **5** | **Jeden lektor, nebo víc / vykazování hodin** *(střídání vs. hodiny na termín)* | **Vazba M:N s rozpisem na dny a přímou evidencí hodin:** Termín může mít více přiřazených lektorů s rozpadem na konkrétní dny výuky (`TerminLektor`). V přehledech a mzdových podkladech se lektorům vykazují reálně odučené hodiny z jejich rozvrhových bloků, nikoliv paušální délka celého termínu. |

---

## 3. Technická architektura a realizace v .NET

Aplikace je implementována v moderním **.NET 8 (C# 12)** a striktně dodržuje třívrstvou architekturu se závislostmi jedním směrem:

### Rozpad projektů v řešení (`AkademieTrutnov.sln`):
1. **`AkademieTrutnov.Core` (Doménová vrstva / Business logika):**
   - Čistá doména nezávislá na databázi či UI frameworks.
   - **Entity:** `Kurz`, `Ucebna`, `Termin`, `Lektor`, `TerminLektor` (M:N), `Student`, `Prihlaska`, `DenVyuky`, `Dochazka`, `Osvedceni`.
   - **Doménové invarianty a výjimky:** `CapacityExceededException`, `DuplicateEnrollmentException`, `InvalidAttendanceException`, `RegistrationLateException`, `WaitlistViolationException`.
   - **Doménové služby:**
     - `RegistrationService`: Zápis studenta, striktní hlídání kapacity učebny, automatické zařazení do pořadníku (1., 2. místo), posun z pořadníku do stavu `NABÍDNUTO` s 24h lhůtou expirace při uvolnění kapacity, archivace zrušených přihlášek (soft-cancellation), zákaz souběžného přihlášení na aktivní termíny téhož kurzu.
     - `AttendanceService`: Záznam denní docházky, výpočet celkového procenta účasti a kontrola hranice $\ge 70\ \%$.
     - `CertificateService`: Dvoustupňové ověření (docházka $\ge 70\ \%$ + lektor "Prospěl"), generování globálně jedinečného evidenčního čísla formátu `AT-YYYY-XXXX`, neměnnost a auditní stopa vydaných osvědčení.
     - `ReportingService`: Report vytíženosti termínů, vykazování reálně odučených hodin lektorů (M:N), seznam studentů přihlášených na více kurzů v daném roce.
   - **Rozhraní repozitářů a UnitOfWork:** `IRepository<T>`, `ITerminRepository`, `IPrihlaskaRepository`, `IStudentRepository`, `IKurzRepository`, `IOsvedceniRepository`, `ILektorRepository`, `IDochazkaRepository`, `IUnitOfWork`.

2. **`AkademieTrutnov.Infrastructure` (Datová vrstva / Perzistence):**
   - Entity Framework Core 8 s PostgreSQL providerem (`Npgsql.EntityFrameworkCore.PostgreSQL`) a SQLite dev fallbackem.
   - `AppDbContext` s Fluent API mapováním (unikátní index na `CisloOsvedceni`, složený klíč pro `TerminLektor`, cizí klíče s patřičným chováním).
   - Verzované migrace schématu (`InitialCreate`).
   - `DatabaseSeeder`: Automatické naplnění syntetickými daty pro demonstraci (kurzy s akreditací MŠMT, učebny, externí i interní lektoři, obsazené termíny s náhradníky v pořadníku i aktivní nabídkou, docházka a vydaná osvědčení).

3. **`AkademieTrutnov.Web` (Prezentační vrstva / UI & REST API):**
   - **Webové UI (Razor Pages):**
     - Moderní, responzivní design s typografií Google Font *Plus Jakarta Sans*, barevnými indikátory stavů a grafickými progress bary kapacity.
     - `Index.cshtml`: Dashboard s klíčovými metrikami (KPI), přehledem aktivních termínů a výzvami pro náhradníky s odpočtem expirace nabídky (Hold 24h).
     - `Kurzy.cshtml`: Katalog kurzů (akreditace MŠMT, cena, hodinová dotace) a zakládání kurzů.
     - `Terminy.cshtml`: Harmonogram termínů, přiřazení učeben (automatické převzetí nepřekročitelné kapacity) a střídání lektorů.
     - `Prihlasky.cshtml`: Přihlašování studentů, interaktivní pořadník náhradníků, dialog pro storno přihlášky s odůvodněním, akceptace/odmítnutí nabídek.
     - `Dochazka.cshtml`: Rozpis výukových dnů, zaškrtávání docházky studentů a barevná indikace splnění 70% podmínky.
     - `Osvedceni.cshtml`: Dvoustupňové vystavení osvědčení, auditní registr a **tiskový layout A4** s ozdobným rámečkem a pečetí.
     - `Reporty.cshtml`: 3 manažerské reporty dle požadavku č. 8 (vytíženost kapacit, odučené hodiny lektorů, vícekurzoví studenti).
   - **REST API + OpenAPI (Swagger UI):**
     - Kompletní CRUD a byznys endpointy pod `/api/kurzy`, `/api/terminy`, `/api/prihlasky`, `/api/dochazka`, `/api/osvedceni`, `/api/reporty`.
     - Dokumentace a interaktivní testování na `/swagger`.

4. **`AkademieTrutnov.Tests` (Automatizované testy):**
   - Testovací framework **xUnit** ověřující kritické doménové invarianty:
     1. Nepřekročitelnost kapacity termínu a řazení do pořadníku náhradníků.
     2. Automatická nabídka prvnímu náhradníkovi v pořadníku při uvolnění místa.
     3. Zachování kontinuity fronty pořadníku a přečíslování po uvolnění/stornu.
     4. Zákaz fyzického mazání zrušených přihlášek (uchování auditu a důvodu).
     5. Zákaz duplicitního souběžného přihlášení na aktivní termíny téhož kurzu.
     6. Odmítnutí vydání osvědčení při docházce $< 70\ \%$.
     7. Úspěšné vystavení osvědčení při docházce $\ge 70\ \%$ a schválení lektorem s globálně unikátním číslem `AT-YYYY-XXXX` a neměnností.

---

## 4. Datový model (Entity a vazby)

Systém pokrývá minimálně 8 klíčových entit s vazbami 1:N i M:N:

```mermaid
erDiagram
    Kurz ||--o{ Termin : "má vypsané"
    Ucebna ||--o{ Termin : "koná se v"
    Termin ||--o{ TerminLektor : "vede"
    Lektor ||--o{ TerminLektor : "vyučuje na"
    Termin ||--o{ Prihlaska : "eviduje"
    Student ||--o{ Prihlaska : "podává"
    Termin ||--o{ DenVyuky : "obsahuje"
    DenVyuky ||--o{ Dochazka : "zaznamenává"
    Student ||--o{ Dochazka : "má záznam"
    Prihlaska ||--o| Osvedceni : "úspěšně zakončena"

    Kurz {
        uuid id PK
        string nazev
        string anotace
        int rozsah_hodin
        decimal cena
        boolean akreditovano
    }

    Ucebna {
        uuid id PK
        string nazev
        string adresa
        int kapacita
    }

    Termin {
        uuid id PK
        uuid kurz_id FK
        uuid ucebna_id FK
        date datum_od
        date datum_do
        int kapacita
        string stav
    }

    Lektor {
        uuid id PK
        string jmeno
        string prijmeni
        string email
        string telefon
        boolean externista
    }

    TerminLektor {
        uuid termin_id FK
        uuid lektor_id FK
        int pocet_hodin
        string role
    }

    Student {
        uuid id PK
        string jmeno
        string prijmeni
        string email
        string telefon
    }

    Prihlaska {
        uuid id PK
        uuid student_id FK
        uuid termin_id FK
        datetime datum_prihlaseni
        string stav "PRIHLASEN | PORADNIK | NABIDNUTO | ZRUSENO | DOKONCENO"
        int poradi_v_poradniku
        datetime nabidka_expirace
    }

    DenVyuky {
        uuid id PK
        uuid termin_id FK
        date datum
        int pocet_hodin
    }

    Dochazka {
        uuid id PK
        uuid den_vyuky_id FK
        uuid student_id FK
        boolean pritomen
        string poznamka
    }

    Osvedceni {
        uuid id PK
        uuid prihlaska_id FK
        string cislo_osvedceni UK "Jedinečné evidenční číslo"
        date datum_vydani
        string vysledek
    }
```

---

## 5. Spuštění projektu

### Varianta A: Spuštění v Dockeru (doporučeno pro odevzdání)
Celé prostředí (PostgreSQL + .NET webová aplikace s automatickými migracemi a seedem) se spustí jediným příkazem:

```bash
docker compose up --build
```
- Webová aplikace: [http://localhost:5000](http://localhost:5000)
- Swagger REST API: [http://localhost:5000/swagger](http://localhost:5000/swagger)

### Varianta B: Lokální spuštění bez Dockeru (.NET SDK)
Díky vestavěnému fallbacku na SQLite lze projekt spustit i lokálně bez běžícího Dockeru:

```bash
dotnet run --project src/AkademieTrutnov.Web
```

### Varianta C: Spuštění automatizovaných testů
```bash
dotnet test
```

---

## 6. Historie a Changelog

| Datum | Změna | Autor |
|---|---|---|
| 2026-09-30 | Inicializace repozitáře, nastavení pravidel asistenta v `AGENTS.md`, vytvoření technické dokumentace a oddílu Rozhodnutí v `README.md`. | Vývojový tým |
| 2026-10-07 | Kompletní implementace 3-vrstvé architektury v .NET 8 (Core, Infrastructure, Web, Tests), zprovoznění pořadníku náhradníků s 24h nabídkami, dvoustupňové certifikace (70% docházka + lektor), EF Core migrací, Docker Compose, Swagger API a automatizovaných xUnit testů. | Vývojový tým |
| 2026-11-09 | Úprava uživatelského rozhraní: odstranění emoji ikon ze všech tlačítek napříč Razor Pages pro čistší a formálnější vzhled. | Vývojový tým |
| 2026-11-09 | Úprava navigace: tlačítko pro přístup k Swagger REST API v horní navigační liště se zobrazuje výhradně v režimu Debug / Development. | Vývojový tým |

