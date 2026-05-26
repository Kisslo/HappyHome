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
public class BokningController : ControllerBase
{
    private readonly HappyHomeDbContext _db;
    public BokningController(HappyHomeDbContext db) => _db = db;

    [HttpGet("klient/{klientId:int}")]
    public async Task<ActionResult<IEnumerable<Bokning>>> ForKlient(int klientId)
        => await _db.Bokningar.AsNoTracking()
            .Include(b => b.Tidslucka)
            .Where(b => b.KlientId == klientId)
            .OrderByDescending(b => b.Tidslucka!.Start)
            .ToListAsync();

    // Bokningar för en terapeut: alla bokningar vars tidslucka tillhör terapeuten.
    // Vi inkluderar klienten så att terapeutens vy kan visa "vem och när".
    [HttpGet("terapeut/{terapeutId:int}")]
    public async Task<ActionResult<IEnumerable<Bokning>>> ForTerapeut(int terapeutId)
        => await _db.Bokningar.AsNoTracking()
            .Include(b => b.Tidslucka)
            .Include(b => b.Klient)
            .Where(b => b.Tidslucka!.TerapeutId == terapeutId)
            .OrderBy(b => b.Tidslucka!.Start)
            .ToListAsync();

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Bokning>> GetEn(int id)
    {
        var b = await _db.Bokningar.Include(x => x.Tidslucka).FirstOrDefaultAsync(x => x.Id == id);
        return b is null ? NotFound() : b;
    }

    public record NyBokningInput(int KlientId, int TidsluckaId, TerapiTyp TerapiTyp, string AnledningTillBesok);

    [HttpPost]
    public async Task<ActionResult<Bokning>> Skapa(NyBokningInput input)
    {
        var klient = await _db.Klienter.FindAsync(input.KlientId);
        if (klient is null) return BadRequest("Klienten finns inte.");

        var lucka = await _db.Tidsluckor
            .Include(tl => tl.Terapeut)
            .Include(tl => tl.Bokning)
            .FirstOrDefaultAsync(tl => tl.Id == input.TidsluckaId);
        if (lucka is null) return BadRequest("Tidsluckan finns inte.");
        if (lucka.Status != TidsluckaStatus.Ledig || lucka.Bokning is not null)
            return Conflict("Tidsluckan är redan bokad.");

        if (lucka.Terapeut is null || !lucka.Terapeut.Aktiv)
            return BadRequest("Terapeuten är inte aktiv.");

        if (!lucka.Terapeut.Specialiseringar.Contains(input.TerapiTyp))
            return BadRequest($"Terapeuten erbjuder inte terapityp '{input.TerapiTyp}'.");

        var dagensStart = lucka.Start.Date;
        var dagensSlut = dagensStart.AddDays(1);
        var harAktivSammaDag = await _db.Bokningar
            .Include(b => b.Tidslucka)
            .AnyAsync(b => b.KlientId == input.KlientId
                           && b.Status == BokningStatus.Bokad
                           && b.Tidslucka!.Start >= dagensStart
                           && b.Tidslucka!.Start < dagensSlut);
        if (harAktivSammaDag)
            return Conflict("Klienten har redan en aktiv bokning samma dag.");

        var bokning = new Bokning
        {
            KlientId = input.KlientId,
            TidsluckaId = input.TidsluckaId,
            TerapiTyp = input.TerapiTyp,
            AnledningTillBesok = input.AnledningTillBesok ?? string.Empty,
            Status = BokningStatus.Bokad,
            Skapad = DateTime.UtcNow
        };

        lucka.Status = TidsluckaStatus.Bokad;
        _db.Bokningar.Add(bokning);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetEn), new { id = bokning.Id }, bokning);
    }

    public record StatusInput(BokningStatus Status);

    [HttpPut("{id:int}/status")]
    public async Task<IActionResult> Statusandring(int id, StatusInput input)
    {
        var bokning = await _db.Bokningar.Include(b => b.Tidslucka).FirstOrDefaultAsync(b => b.Id == id);
        if (bokning is null) return NotFound();

        bokning.Status = input.Status;

        if (input.Status == BokningStatus.Avbokad && bokning.Tidslucka is not null)
            bokning.Tidslucka.Status = TidsluckaStatus.Ledig;

        await _db.SaveChangesAsync();
        return NoContent();
    }
}
