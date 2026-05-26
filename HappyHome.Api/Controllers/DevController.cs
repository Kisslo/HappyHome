using HappyHome.Infrastructure;
using HappyHome.Infrastructure.Seed;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HappyHome.Api.Controllers;

// Dev-endpoints är öppna *men* villkorade på Development/Testing-miljö i koden.
// Det är två lager: ingen [Authorize], och en explicit miljöcheck i varje action.
[ApiController]
[AllowAnonymous]
[Route("api/[controller]")]
public class DevController : ControllerBase
{
    private readonly HappyHomeDbContext _db;
    private readonly IWebHostEnvironment _env;

    public DevController(HappyHomeDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    [HttpDelete("reset")]
    public async Task<IActionResult> Reset(CancellationToken ct)
    {
        if (!IsDevelopment()) return NotFound();

        if (_db.Database.IsRelational())
            await _db.Database.MigrateAsync(ct);
        else
            await _db.Database.EnsureCreatedAsync(ct);

        DbSeeder.Clear(_db);
        DbSeeder.Seed(_db, force: true);

        var counts = await DbSeeder.CountsAsync(_db, ct);
        return Ok(new { message = "Databasen har återställts och seedats på nytt.", counts });
    }

    [HttpGet("status")]
    public async Task<IActionResult> Status(CancellationToken ct)
    {
        if (!IsDevelopment()) return NotFound();

        var counts = await DbSeeder.CountsAsync(_db, ct);
        return Ok(counts);
    }

    private bool IsDevelopment() =>
        _env.IsDevelopment() || _env.IsEnvironment("Testing");
}
