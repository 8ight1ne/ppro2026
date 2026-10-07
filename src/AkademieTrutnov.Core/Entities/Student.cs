namespace AkademieTrutnov.Core.Entities;

public class Student
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Jmeno { get; set; } = string.Empty;
    public string Prijmeni { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefon { get; set; } = string.Empty;
    public string? Organizace { get; set; }

    public string CeleJmeno => $"{Jmeno} {Prijmeni}";

    public ICollection<Prihlaska> Prihlasky { get; set; } = new List<Prihlaska>();
    public ICollection<Dochazka> Dochazky { get; set; } = new List<Dochazka>();
}
