using HappyHome.Core.Enums;

namespace HappyHome.Core.Models;

public class Terapeut
{
    public int Id { get; set; }
    public string Förnamn { get; set; } = string.Empty;
    public string Efternamn { get; set; } = string.Empty;
    public string Epost { get; set; } = string.Empty;
    public TerapeutRoll Roll { get; set; }
    public List<TerapiTyp> Specialiseringar { get; set; } = new();
    public DateTime AktivFromDatum { get; set; }
    public bool Aktiv { get; set; } = true;
    public DateTime Skapad { get; set; } = DateTime.UtcNow;

    public ICollection<Tidslucka> Tidsluckor { get; set; } = new List<Tidslucka>();
    public ICollection<Konsultation> Konsultationer { get; set; } = new List<Konsultation>();
}
