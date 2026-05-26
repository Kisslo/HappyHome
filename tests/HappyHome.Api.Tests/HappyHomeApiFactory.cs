using System.Net.Http.Headers;
using System.Net.Http.Json;
using HappyHome.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HappyHome.Api.Tests;

public class HappyHomeApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"HappyHomeTests_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            var descriptors = services
                .Where(d =>
                    d.ServiceType == typeof(DbContextOptions<HappyHomeDbContext>) ||
                    d.ServiceType == typeof(HappyHomeDbContext))
                .ToList();

            foreach (var descriptor in descriptors)
                services.Remove(descriptor);

            services.AddDbContext<HappyHomeDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));
        });
    }

    // CreateSeededClient ger en client som är inloggad som admin via JWT.
    // På så sätt fungerar alla affärsregeltester precis som tidigare —
    // skyddade endpoints svarar fortfarande korrekt eftersom vi är admin.
    public HttpClient CreateSeededClient()
    {
        var client = CreateClient();
        var reset = client.DeleteAsync("/api/dev/reset").GetAwaiter().GetResult();
        reset.EnsureSuccessStatusCode();

        var token = LoggaInSomAsync(client, "admin@happyhome.se", "Demo123!").GetAwaiter().GetResult();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public HttpClient CreateAnonymousClient() => CreateClient();

    private static async Task<string> LoggaInSomAsync(HttpClient client, string epost, string lösenord)
    {
        var response = await client.PostAsJsonAsync("/api/Auth/login", new { Epost = epost, Lösenord = lösenord });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>(ApiJson.Options);
        return body!.Token;
    }

    private sealed record LoginResponse(string Token, DateTime UtgårUtc);
}
