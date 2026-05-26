using System.Text;
using HappyHome.Api.Auth;
using HappyHome.Infrastructure;
using HappyHome.Infrastructure.Seed;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(opt =>
    {
        opt.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
        opt.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<HappyHomeDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("HappyHome")));

builder.Services.AddSingleton<JwtTokenService>();

// CORS-policy: tillåt React-appen att prata med API:et. Vi pinpoint:ar exakta
// origins i stället för AllowAnyOrigin() — det senare bryter mot säkerhetsbest
// practice när vi också skickar credentials/tokens.
const string corsPolicyName = "HappyHomeWeb";
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                     ?? new[] { "http://localhost:5173" };

builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicyName, policy => policy
        .WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod());
});

var jwtCfg = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtCfg["Key"] ?? throw new InvalidOperationException("Jwt:Key saknas i appsettings.");
var nyckelBytes = Encoding.UTF8.GetBytes(jwtKey);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtCfg["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtCfg["Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(nyckelBytes),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// I demon kör React mot http över http — undvik HTTPS-redirect-varningen.
// I produktion skulle vi förstås tvinga HTTPS.
if (!app.Environment.IsDevelopment() && !app.Environment.IsEnvironment("Testing"))
    app.UseHttpsRedirection();

// Ordningen är avgörande: CORS → Authentication → Authorization → Endpoints.
app.UseCors(corsPolicyName);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

if (!app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<HappyHomeDbContext>();
    db.Database.Migrate();
    DbSeeder.Seed(db);
}

app.Run();

public partial class Program;
