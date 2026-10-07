namespace AkademieTrutnov.Core.Entities;

public class Lektor
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Jmeno { get; set; } = string.Empty;
    public string Prijmeni { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefon { get; set; } = string.Empty;
    public bool Externista { get; set; } = true;
    public string? Specializace { get; set; }

    public string CeleJmeno => $"{Jmeno} {Prijmeni}";

    public ICollection<TerminLektor> TerminLektori { get; set; } = new List<TerminLektor>();
}
