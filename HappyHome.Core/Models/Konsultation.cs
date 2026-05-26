using HappyHome.Core.Enums;

namespace HappyHome.Core.Models;

public class Konsultation
{
    public int Id { get; set; }
    public int KlientId { get; set; }
    public int TerapeutId { get; set; }
    public int? BokningId { get; set; }
    public TerapiTyp Typ { get; set; }
    public DateTime Datum { get; set; }
    public string Symptom { get; set; } = string.Empty;
    public string Bakgrund { get; set; } = string.Empty;
    public string Anteckningar { get; set; } = string.Empty;
    public string? Diagnosförslag { get; set; }
    public DateTime Skapad { get; set; } = DateTime.UtcNow;

    public Klient? Klient { get; set; }
    public Terapeut? Terapeut { get; set; }
    public Bokning? Bokning { get; set; }
}
