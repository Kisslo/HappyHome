using HappyHome.Core.Enums;

namespace HappyHome.Core.Models;

public class Tidslucka
{
    public int Id { get; set; }
    public int TerapeutId { get; set; }
    public DateTime Start { get; set; }
    public DateTime Slut { get; set; }
    public TidsluckaStatus Status { get; set; } = TidsluckaStatus.Ledig;
    public DateTime Skapad { get; set; } = DateTime.UtcNow;

    public Terapeut? Terapeut { get; set; }
    public Bokning? Bokning { get; set; }
}
