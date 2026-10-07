namespace AkademieTrutnov.Core.Entities;

public class Dochazka
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid DenVyukyId { get; set; }
    public DenVyuky DenVyuky { get; set; } = null!;

    public Guid StudentId { get; set; }
    public Student Student { get; set; } = null!;

    public bool Pritomen { get; set; }
    public string? Poznamka { get; set; }
}
