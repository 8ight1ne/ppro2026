using AkademieTrutnov.Core.Enums;

namespace AkademieTrutnov.Core.Entities;

public class Termin
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public Guid KurzId { get; set; }
    public Kurz Kurz { get; set; } = null!;

    public Guid UcebnaId { get; set; }
    public Ucebna Ucebna { get; set; } = null!;

    public DateTime DatumOd { get; set; }
    public DateTime DatumDo { get; set; }
    
    /// <summary>
    /// Kapacita termínu je striktně odvozena z kapacity učebny (obchodní pravidlo klienta: nelze překročit).
    /// </summary>
    public int Kapacita { get; set; }

    public StavTerminu Stav { get; set; } = StavTerminu.Planovany;

    public ICollection<TerminLektor> TerminLektori { get; set; } = new List<TerminLektor>();
    public ICollection<Prihlaska> Prihlasky { get; set; } = new List<Prihlaska>();
    public ICollection<DenVyuky> DnyVyuky { get; set; } = new List<DenVyuky>();
}
