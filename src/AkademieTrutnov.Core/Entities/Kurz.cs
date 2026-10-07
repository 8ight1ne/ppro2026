namespace AkademieTrutnov.Core.Entities;

public class Kurz
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Nazev { get; set; } = string.Empty;
    public string Anotace { get; set; } = string.Empty;
    public int RozsahHodin { get; set; }
    public decimal Cena { get; set; }
    public bool Akreditovano { get; set; }

    public ICollection<Termin> Terminy { get; set; } = new List<Termin>();
}
