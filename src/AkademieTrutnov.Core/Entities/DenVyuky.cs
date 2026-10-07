namespace AkademieTrutnov.Core.Entities;

public class DenVyuky
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TerminId { get; set; }
    public Termin Termin { get; set; } = null!;

    public Guid? LektorId { get; set; }
    public Lektor? Lektor { get; set; }

    public DateOnly Datum { get; set; }
    public int PocetHodin { get; set; } = 8;
    public string? Tema { get; set; }

    public ICollection<Dochazka> Dochazky { get; set; } = new List<Dochazka>();
}
