using HappyHome.Core.Models;
using HappyHome.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HappyHome.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class KlientController : ControllerBase
{
    private readonly HappyHomeDbContext _db;
    public KlientController(HappyHomeDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Klient>>> GetAlla()
        => await _db.Klienter.AsNoTracking().ToListAsync();

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Klient>> GetEn(int id)
    {
        var k = await _db.Klienter.FindAsync(id);
        return k is null ? NotFound() : k;
    }

    [HttpPost]
    public async Task<ActionResult<Klient>> Skapa(Klient klient)
    {
        klient.Id = 0;
        klient.Skapad = DateTime.UtcNow;
        _db.Klienter.Add(klient);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetEn), new { id = klient.Id }, klient);
    }
}
