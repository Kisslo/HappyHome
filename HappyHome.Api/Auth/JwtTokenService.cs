using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HappyHome.Core.Models;
using Microsoft.IdentityModel.Tokens;

namespace HappyHome.Api.Auth;

// JwtTokenService isolerar signering och claim-byggande på ett enda ställe.
// Konfiguration (issuer/audience/key) läses från appsettings.json så att
// driftsmiljöer kan rotera nyckeln utan att koden byggs om.
public class JwtTokenService
{
    private readonly IConfiguration _config;

    public JwtTokenService(IConfiguration config) => _config = config;

    public (string Token, DateTime UtgårUtc) Skapa(Användare användare)
    {
        var jwt = _config.GetSection("Jwt");
        var nyckel = jwt["Key"] ?? throw new InvalidOperationException("Jwt:Key saknas.");
        var issuer = jwt["Issuer"];
        var audience = jwt["Audience"];
        var minuter = int.TryParse(jwt["ExpireMinutes"], out var m) ? m : 60;

        var nu = DateTime.UtcNow;
        var utgår = nu.AddMinutes(minuter);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, användare.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, användare.Epost),
            new(ClaimTypes.NameIdentifier, användare.Id.ToString()),
            new(ClaimTypes.Email, användare.Epost),
            new(ClaimTypes.Role, användare.Roll.ToString())
        };

        if (användare.KlientId is int klientId)
            claims.Add(new Claim("klientId", klientId.ToString()));
        if (användare.TerapeutId is int terapeutId)
            claims.Add(new Claim("terapeutId", terapeutId.ToString()));

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(nyckel)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: nu,
            expires: utgår,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), utgår);
    }
}
