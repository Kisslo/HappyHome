namespace HappyHome.Core.Models;

public class Klient
{
    public int Id { get; set; }
    public string Förnamn { get; set; } = string.Empty;
    public string Efternamn { get; set; } = string.Empty;
    public string Personnummer { get; set; } = string.Empty;
    public string Epost { get; set; } = string.Empty;
    public string Telefon { get; set; } = string.Empty;
    public DateTime Födelsedatum { get; set; }
    public DateTime Skapad { get; set; } = DateTime.UtcNow;

    public ICollection<Bokning> Bokningar { get; set; } = new List<Bokning>();
    public ICollection<Konsultation> Konsultationer { get; set; } = new List<Konsultation>();
}
