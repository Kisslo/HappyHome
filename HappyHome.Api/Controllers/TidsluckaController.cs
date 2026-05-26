using HappyHome.Core.Enums;
using HappyHome.Core.Models;
using HappyHome.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HappyHome.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class TidsluckaController : ControllerBase
{
    private readonly HappyHomeDbContext _db;
    public TidsluckaController(HappyHomeDbContext db) => _db = db;

    [HttpGet("tillgangliga")]
    [HttpGet("lediga")]
    public async Task<ActionResult<IEnumerable<Tidslucka>>> Tillgangliga(
        [FromQuery] int terapeutId,
        [FromQuery] DateTime datum)
    {
        var dagensSlut = datum.Date.AddDays(1);
        return await _db.Tidsluckor.AsNoTracking()
            .Where(tl => tl.TerapeutId == terapeutId
                         && tl.Status == TidsluckaStatus.Ledig
                         && tl.Start >= datum.Date
                         && tl.Start < dagensSlut)
            .OrderBy(tl => tl.Start)
            .ToListAsync();
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Terapeut")]
    public async Task<ActionResult<Tidslucka>> Skapa(Tidslucka lucka)
    {
        var terapeut = await _db.Terapeuter.FindAsync(lucka.TerapeutId);
        if (terapeut is null) return BadRequest("Terapeuten finns inte.");
        if (!terapeut.Aktiv) return BadRequest("Terapeuten är inaktiverad.");
        if (lucka.Slut <= lucka.Start) return BadRequest("Slut måste vara efter Start.");

        lucka.Id = 0;
        lucka.Status = TidsluckaStatus.Ledig;
        lucka.Skapad = DateTime.UtcNow;

        _db.Tidsluckor.Add(lucka);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Tillgangliga),
            new { terapeutId = lucka.TerapeutId, datum = lucka.Start.Date }, lucka);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,Terapeut")]
    public async Task<IActionResult> TaBort(int id)
    {
        var lucka = await _db.Tidsluckor.Include(tl => tl.Bokning).FirstOrDefaultAsync(tl => tl.Id == id);
        if (lucka is null) return NotFound();
        if (lucka.Bokning is not null) return Conflict("Tidsluckan har en aktiv bokning och kan inte tas bort.");

        _db.Tidsluckor.Remove(lucka);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
