using HappyHome.Core.Enums;

namespace HappyHome.Core.Models;

// En användarpost innehåller bara det som behövs för inloggning + roll.
// Personuppgifter ligger kvar i Klient/Terapeut — Användare är en separat
// identitetstabell så att samma persons inloggning kan kopplas om vid behov.
public class Användare
{
    public int Id { get; set; }
    public string Epost { get; set; } = string.Empty;
    public string LösenordHash { get; set; } = string.Empty;
    public AnvändarRoll Roll { get; set; }

    // En användare är antingen en klient eller en terapeut — eller en admin
    // (då är båda null). Egna properties i stället för en polymorf FK gör
    // queries enkla att läsa.
    public int? KlientId { get; set; }
    public Klient? Klient { get; set; }

    public int? TerapeutId { get; set; }
    public Terapeut? Terapeut { get; set; }

    public DateTime Skapad { get; set; } = DateTime.UtcNow;
}
