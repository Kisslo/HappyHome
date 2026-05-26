using HappyHome.Core.Enums;

namespace HappyHome.Core.Models;

public class Bokning
{
    public int Id { get; set; }
    public int KlientId { get; set; }
    public int TidsluckaId { get; set; }
    public TerapiTyp TerapiTyp { get; set; }
    public string AnledningTillBesok { get; set; } = string.Empty;
    public BokningStatus Status { get; set; } = BokningStatus.Bokad;
    public DateTime Skapad { get; set; } = DateTime.UtcNow;

    public Klient? Klient { get; set; }
    public Tidslucka? Tidslucka { get; set; }
    public Konsultation? Konsultation { get; set; }
}
