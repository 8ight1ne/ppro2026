namespace AkademieTrutnov.Core.Entities;

public class Ucebna
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Nazev { get; set; } = string.Empty;
    public string Adresa { get; set; } = string.Empty;
    public int Kapacita { get; set; }

    public ICollection<Termin> Terminy { get; set; } = new List<Termin>();
}
