using AkademieTrutnov.Core.Enums;

namespace AkademieTrutnov.Core.Entities;

public class Osvedceni
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid PrihlaskaId { get; set; }
    public Prihlaska Prihlaska { get; set; } = null!;

    /// <summary>
    /// Globálně jedinečné evidenční číslo osvědčení (např. "AT-2026-0001").
    /// Číslo je neměnné a nikdy se nesmí opakovat.
    /// </summary>
    public string CisloOsvedceni { get; set; } = string.Empty;

    public DateOnly DatumVydani { get; set; }

    public VysledekHodnoceni Vysledek { get; set; } = VysledekHodnoceni.Prospel;

    public double DosazenaDochazkaProcent { get; set; }

    public string Vystavil { get; set; } = "Ing. Lucie Bartošová";
}
