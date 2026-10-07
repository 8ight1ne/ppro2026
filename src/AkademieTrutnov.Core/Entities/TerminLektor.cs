namespace AkademieTrutnov.Core.Entities;

public class TerminLektor
{
    public Guid TerminId { get; set; }
    public Termin Termin { get; set; } = null!;

    public Guid LektorId { get; set; }
    public Lektor Lektor { get; set; } = null!;

    /// <summary>
    /// Počet reálně odučených/plánovaných hodin tohoto lektora na daném termínu (Rozhodnutí č. 5).
    /// </summary>
    public int PocetHodin { get; set; }

    public string Role { get; set; } = "Hlavní lektor";
}
