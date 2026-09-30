# Pravidla a zásady pro AI asistenta (AGENTS.md)

Tento dokument definuje závazná pravidla chování, pracovní postupy a standardy pro AI asistenta pracujícího na tomto repozitáři.

---

## 1. Základní informace o projektu
- **Předmět:** Pokročilé programování (PPRO), ZS 2026/2027, FIM UHK
- **Zadání:** **Zadání C: Kurzy vzdělávacího centra** (Akademie Trutnov – Ing. Lucie Bartošová)
- **Cíl projektu:** Realizace 3-vrstvé webové aplikace pro správu kurzů, termínů, lektorů, přihlášek s pořadníkem, docházky a generování osvědčení.

---

## 2. Jediný zdroj pravdy (Single Source of Truth)
- Soubor [`README.md`](file:///c:/Users/riham/Škola/4.1.%20ZS/PPRO/Cvičení/ppro2026/README.md) slouží jako **jediný zdroj pravdy** pro:
  1. Úplné zadání a specifikaci požadavků.
  2. Technickou a architektonickou dokumentaci projektu.
  3. **Oddíl Rozhodnutí** – evidenci všech rozhodnutí k otevřeným bodům klienta s patřičným zdůvodněním.
  4. Aktuální stav implementace a technologický stack.
- **Pravidlo aktualizace:** Před každým commitem a po každé architektonické či doménové změně musí být `README.md` aktualizován, aby odrážel přesný a reálný stav projektu.

---

## 3. Zásady práce s Gitem

### A. Tvorba commitů
1. **Vždy s přepínačem `-m` a smysluplnou zprávou:**
   - Každý commit musí mít jasnou, popisnou zprávu vysvětlující podstatu provedené změny.
   - Doporučuje se konvence Conventional Commits (např. `feat:`, `fix:`, `docs:`, `refactor:`, `test:`, `chore:`).
   - Příklad: `git commit -m "docs: aktualizace README s rozhodnutími o pořadníku a dokončení kurzu"`
2. **Kontrola před commitem (Pre-commit checklist):**
   - [ ] Kód je syntakticky v pořádku a projekt je sestavitelný.
   - [ ] Byla aktualizována technická dokumentace a oddíl Rozhodnutí v `README.md`.
   - [ ] Do gitu se nepřidávají dočasné soubory, build artefakty ani citlivé údaje (.env, hesla).

### B. Práce se vzdáleným repozitářem (Push)
- **STRIKTNÍ ZÁKAZ AUTOMATICKÉHO PUSHU:**
- Asistent **NIKDY** nespouští `git push` automaticky bez přímého dotazu.
- Asistent se před odesláním změn do remote repozitáře **vždy explicitně zeptá uživatele** na souhlas (např. *„Mám nyní provést `git push origin main`?“*) a vyčká na potvrzení.

---

## 4. Architektonické a technické požadavky (Povinné minimum PPRO)
1. **Třívrstvá architektura:**
   - Striktní oddělení vrstev: Prezentační (API/UI) $\rightarrow$ Aplikační/Doménová (Business logika) $\rightarrow$ Datová (Infrastruktura/Perzistence).
   - Závislosti směřují výhradně jedním směrem.
2. **Relační databáze a migrace v Dockeru:**
   - Databáze běží v kontejneru (Docker).
   - Správa schématu pomocí verzovaných migrací (žádné ruční vytváření tabulek v produkčním běhu).
   - Minimálně **5 entit** a alespoň jedna vazba **M:N**.
3. **Spustitelnost:**
   - Celý projekt musí být reprodukovatelně spustitelný příkazem:
     ```bash
     docker compose up
     ```
4. **Testování:**
   - Projekt musí obsahovat sadu automatizovaných testů ověřujících klíčovou doménovou logiku (především kapacitu termínů, pořadník, docházku a osvědčení).
5. **Syntetická data & Bezpečnost:**
   - Žádná reálná osobní data v databázi ani v testech.
   - Žádné přihlašovací údaje, tokeny či tajemství v repozitáři.

---

## 5. Doménová specifika Zadání C (Kritická místa návrhu)
- **Kde se návrh láme:** Pořadník zájemců a pravidlo pro dokončení kurzu / vydání osvědčení.
- **Obchodní pravidla klienta:**
  - Kapacitu termínu nelze překročit za žádných okolností.
  - Pořadí v pořadníku se nesmí přeskakovat.
  - Číslo osvědčení je globálně jedinečné a již vydané osvědčení je neměnné (auditní stopa).
  - Zrušené přihlášky se fyzicky nemažou (uchování historie a důvodu).
