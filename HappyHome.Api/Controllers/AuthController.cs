using System.Security.Claims;
using HappyHome.Api.Auth;
using HappyHome.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HappyHome.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly HappyHomeDbContext _db;
    private readonly JwtTokenService _jwt;

    public AuthController(HappyHomeDbContext db, JwtTokenService jwt)
    {
        _db = db;
        _jwt = jwt;
    }

    public record LoginInput(string Epost, string Lösenord);
    public record LoginResultat(string Token, DateTime UtgårUtc, MeResultat Användare);
    public record MeResultat(int Id, string Epost, string Roll, int? KlientId, int? TerapeutId);

    // Login MÅSTE vara anonym — annars går det inte att få sin första token.
    // Allt annat i API:et kräver giltig JWT (se Program.cs och [Authorize] längre ner).
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResultat>> Login(LoginInput input)
    {
        var användare = await _db.Användare
            .FirstOrDefaultAsync(a => a.Epost == input.Epost);

        // Generic felmeddelande: avslöja aldrig OM kontot finns. Annars ger vi
        // en angripare en gratis e-postvalidering.
        if (användare is null ||
            !BCrypt.Net.BCrypt.Verify(input.Lösenord, användare.LösenordHash))
            return Unauthorized(new { message = "Fel epost eller lösenord." });

        var (token, utgår) = _jwt.Skapa(användare);
        return Ok(new LoginResultat(
            token,
            utgår,
            new MeResultat(
                användare.Id,
                användare.Epost,
                användare.Roll.ToString(),
                användare.KlientId,
                användare.TerapeutId)));
    }

    [Authorize]
    [HttpGet("me")]
    public ActionResult<MeResultat> Me()
    {
        var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var epost = User.FindFirstValue(ClaimTypes.Email) ?? "";
        var roll = User.FindFirstValue(ClaimTypes.Role) ?? "";
        var klientId = int.TryParse(User.FindFirstValue("klientId"), out var k) ? k : (int?)null;
        var terapeutId = int.TryParse(User.FindFirstValue("terapeutId"), out var t) ? t : (int?)null;

        return new MeResultat(id, epost, roll, klientId, terapeutId);
    }
}
