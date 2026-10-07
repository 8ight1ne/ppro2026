using AkademieTrutnov.Core.Enums;

namespace AkademieTrutnov.Core.Entities;

public class Prihlaska
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid StudentId { get; set; }
    public Student Student { get; set; } = null!;

    public Guid TerminId { get; set; }
    public Termin Termin { get; set; } = null!;

    public DateTime DatumPrihlaseni { get; set; } = DateTime.UtcNow;

    public StavPrihlasky Stav { get; set; } = StavPrihlasky.Prihlasen;

    /// <summary>
    /// Pořadí v pořadníku náhradníků (1 = první na řadě). Pouze pro Stav == Poradnik.
    /// Pořadí se nesmí přeskakovat (obchodní pravidlo klienta).
    /// </summary>
    public int? PoradiVPoradniku { get; set; }

    /// <summary>
    /// Čas vypršení nabídky uvolněného místa (24h lhůta - Rozhodnutí č. 2). Pouze pro Stav == Nabidnuto.
    /// </summary>
    public DateTime? NabidkaExpirace { get; set; }

    /// <summary>
    /// Zrušené přihlášky se fyzicky nemažou (obchodní pravidlo klienta: uchování historie a řešení sporů).
    /// </summary>
    public DateTime? DatumZruseni { get; set; }
    public string? DuvodZruseni { get; set; }

    /// <summary>
    /// Odůvodnění pro případné pozdní přihlášení administrátorem (Rozhodnutí č. 4).
    /// </summary>
    public string? PoznamkaAdmina { get; set; }

    public Osvedceni? Osvedceni { get; set; }
}
