using HappyHome.Core.Models;
using HappyHome.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HappyHome.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class TerapeutController : ControllerBase
{
    private readonly HappyHomeDbContext _db;
    public TerapeutController(HappyHomeDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Terapeut>>> GetAktiva()
        => await _db.Terapeuter.AsNoTracking().Where(t => t.Aktiv).ToListAsync();

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Terapeut>> GetEn(int id)
    {
        var t = await _db.Terapeuter.FindAsync(id);
        return t is null ? NotFound() : t;
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Terapeut>> Skapa(Terapeut terapeut)
    {
        terapeut.Id = 0;
        terapeut.Skapad = DateTime.UtcNow;
        if (terapeut.AktivFromDatum == default) terapeut.AktivFromDatum = DateTime.UtcNow;
        _db.Terapeuter.Add(terapeut);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetEn), new { id = terapeut.Id }, terapeut);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Uppdatera(int id, Terapeut input)
    {
        var t = await _db.Terapeuter.FindAsync(id);
        if (t is null) return NotFound();

        t.Förnamn = input.Förnamn;
        t.Efternamn = input.Efternamn;
        t.Epost = input.Epost;
        t.Roll = input.Roll;
        t.Specialiseringar = input.Specialiseringar ?? new();
        t.AktivFromDatum = input.AktivFromDatum;
        t.Aktiv = input.Aktiv;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Inaktivera(int id)
    {
        var t = await _db.Terapeuter.FindAsync(id);
        if (t is null) return NotFound();
        t.Aktiv = false;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
