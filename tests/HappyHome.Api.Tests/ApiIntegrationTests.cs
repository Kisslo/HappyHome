using System.Net;
using System.Net.Http.Json;
using HappyHome.Core.Enums;
using HappyHome.Core.Models;
using HappyHome.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HappyHome.Api.Tests;

// Varje test beskriver en affärsregel ur CLAUDE.md.
// Testnamnet säger VAD som ska gälla; kommentaren förklarar VARFÖR regeln finns.
public class ApiIntegrationTests : IClassFixture<HappyHomeApiFactory>
{
    private readonly HappyHomeApiFactory _factory;
    private readonly HttpClient _client;

    public ApiIntegrationTests(HappyHomeApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateSeededClient();
    }

    private async Task ÅterställTillKändSeedAsync()
    {
        var response = await _client.DeleteAsync("/api/dev/reset");
        response.EnsureSuccessStatusCode();
    }

    // ---------- Grundläggande API-kontrakt ----------

    [Fact]
    public async Task DevReset_GerKändStartdata_FörAllaTabeller()
    {
        // Utbildningssystemet behöver en utvecklingsbrygga som garanterar en känd
        // tabellstatus innan varje test. Utan deterministisk seed påverkar tester
        // varandra och fel blir omöjliga att reproducera.
        var response = await _client.DeleteAsync("/api/dev/reset");
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<ResetResponse>(ApiJson.Options);
        Assert.NotNull(body);
        Assert.Equal(10, body.Counts["Klienter"]);
        Assert.Equal(5, body.Counts["Terapeuter"]);
        Assert.Equal(20, body.Counts["Tidsluckor"]);
        Assert.Equal(10, body.Counts["Bokningar"]);
        Assert.Equal(5, body.Counts["Konsultationer"]);
    }

    [Fact]
    public async Task DevStatus_VisarRadantalSomMatchar_KändSeed()
    {
        // Status-endpointen är ett snabbt sätt att verifiera att databasen är
        // intakt mellan tester. Avviker antalet rader har något annat test glömt
        // att städa upp — eller seeden har drivit isär från det dokumenterade.
        await ÅterställTillKändSeedAsync();

        var counts = await _client.GetFromJsonAsync<Dictionary<string, int>>(
            "/api/dev/status", ApiJson.Options);

        Assert.NotNull(counts);
        Assert.Equal(5, counts["Terapeuter"]);
        Assert.Equal(10, counts["Klienter"]);
    }

    [Fact]
    public async Task Terapeut_GetAlla_VisarEnbart_AktivaTerapeuter()
    {
        // Terapeuter raderas aldrig — när någon slutar sätts Aktiv=false så att
        // historiken över vem som höll i en konsultation finns kvar. Listan över
        // tillgängliga terapeuter får ändå aldrig läcka inaktiva poster, annars
        // skulle någon kunna boka in sig hos en person som inte längre arbetar.
        await ÅterställTillKändSeedAsync();

        var terapeuter = await _client.GetFromJsonAsync<List<Terapeut>>(
            "/api/Terapeut", ApiJson.Options);

        Assert.NotNull(terapeuter);
        Assert.Equal(5, terapeuter.Count);
        Assert.All(terapeuter, t => Assert.True(t.Aktiv));
    }

    [Fact]
    public async Task Klient_AllaHar_PersonnummerIGiltigtFormat()
    {
        // Personnummer är klinikens primära identifierare mot resten av vården.
        // Felaktigt format gör att integrationer mot journalsystem och kassa
        // misslyckas — i värsta fall pekar en anteckning på fel person.
        await ÅterställTillKändSeedAsync();

        var klienter = await _client.GetFromJsonAsync<List<Klient>>(
            "/api/Klient", ApiJson.Options);

        Assert.NotNull(klienter);
        Assert.Equal(10, klienter.Count);
        Assert.All(klienter, k =>
        {
            Assert.False(string.IsNullOrWhiteSpace(k.Personnummer));
            Assert.Matches(@"^\d{6}-\d{4}$", k.Personnummer);
        });
    }

    [Fact]
    public async Task Tidslucka_Lediga_DoljerLuckorSomRedanArBokade()
    {
        // Klient och receptionist får aldrig se redan bokade luckor i bokningsvyn.
        // Dels skyddar det andra patienters tidsuppgifter, dels förhindrar det
        // dubbelbokningsförsök som ändå kommer att avvisas av API:t.
        await ÅterställTillKändSeedAsync();

        var terapeuter = await _client.GetFromJsonAsync<List<Terapeut>>(
            "/api/Terapeut", ApiJson.Options);
        Assert.NotNull(terapeuter);

        var terapeutId = terapeuter[0].Id;
        var onsdag = NästaVeckansMåndag().AddDays(2);

        var luckor = await _client.GetFromJsonAsync<List<Tidslucka>>(
            $"/api/Tidslucka/lediga?terapeutId={terapeutId}&datum={onsdag:yyyy-MM-dd}",
            ApiJson.Options);

        Assert.NotNull(luckor);
        Assert.NotEmpty(luckor);
        Assert.All(luckor, l => Assert.Equal(TidsluckaStatus.Ledig, l.Status));
    }

    [Fact]
    public async Task Klient_OkantId_GerNotFound_IstalletForServerfel()
    {
        // 404 i stället för 500 är ett medvetet API-kontrakt: frontend kan
        // särskilja "klienten finns inte" från en buggig backend, och interna
        // stack-traces läcker inte ut till okända anropare.
        var response = await _client.GetAsync("/api/Klient/99999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Bokning_ForKlient_VisarEnbart_KlientensEgna()
    {
        // Patientsekretess: en klient ska bara se sina egna bokningar — aldrig
        // någon annans. API:t måste alltid filtrera på klient-id och inte läcka
        // rader som tillhör andra.
        await ÅterställTillKändSeedAsync();

        var klienter = await _client.GetFromJsonAsync<List<Klient>>(
            "/api/Klient", ApiJson.Options);
        Assert.NotNull(klienter);

        var klientId = klienter[0].Id;
        var bokningar = await _client.GetFromJsonAsync<List<Bokning>>(
            $"/api/Bokning/klient/{klientId}", ApiJson.Options);

        Assert.NotNull(bokningar);
        Assert.NotEmpty(bokningar);
        Assert.All(bokningar, b => Assert.Equal(klientId, b.KlientId));
    }

    // ---------- Affärsregler ----------

    [Fact]
    public async Task Bokning_MisslyckasOm_TidsluckaRedanBokad()
    {
        // En tidslucka motsvarar ett fysiskt möte i kalendern. Om två klienter
        // bokar samma lucka kan terapeuten bara ta emot en — den andra möter
        // en stängd dörr. Spärren måste därför ligga i API:t, inte bara i UI.
        await ÅterställTillKändSeedAsync();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HappyHomeDbContext>();

        var redanBokad = db.Bokningar.Include(b => b.Tidslucka).First();
        var datum = redanBokad.Tidslucka!.Start.Date;

        // Välj en annan klient som inte själv har en bokning samma dag, så att
        // det är just "luckan är redan bokad" som faller — inte "samma dag"-regeln.
        var bokningarPerKlientSammaDag = db.Bokningar
            .Include(b => b.Tidslucka)
            .Where(b => b.Status == BokningStatus.Bokad
                        && b.Tidslucka!.Start >= datum
                        && b.Tidslucka.Start < datum.AddDays(1))
            .Select(b => b.KlientId)
            .ToHashSet();

        var annanKlient = db.Klienter
            .First(k => k.Id != redanBokad.KlientId
                        && !bokningarPerKlientSammaDag.Contains(k.Id));

        var dubbel = new
        {
            KlientId = annanKlient.Id,
            TidsluckaId = redanBokad.TidsluckaId,
            TerapiTyp = redanBokad.TerapiTyp,
            AnledningTillBesok = "Försök att dubbelboka samma lucka"
        };

        var response = await _client.PostAsJsonAsync("/api/Bokning", dubbel, ApiJson.Options);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Bokning_MisslyckasOm_KlientHarAnnanBokningSammaDag()
    {
        // En klient som dyker upp på två terapibesök samma dag är nästan alltid
        // ett misstag — ett felklick eller en panikartad ombokning. Regeln skyddar
        // klienten från oavsiktlig dubbelbokning och håller luckor lediga för
        // andra som verkligen behöver dem.
        await ÅterställTillKändSeedAsync();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HappyHomeDbContext>();

        var terapeut = db.Terapeuter.First(t => t.Aktiv);
        var klient = db.Klienter.First();
        var spec = terapeut.Specialiseringar.First();
        var datum = DateTime.Today.AddDays(14);

        var lucka1 = NyLedigLucka(terapeut.Id, datum.AddHours(9));
        var lucka2 = NyLedigLucka(terapeut.Id, datum.AddHours(13));
        db.Tidsluckor.AddRange(lucka1, lucka2);
        db.SaveChanges();

        var första = await _client.PostAsJsonAsync("/api/Bokning", new
        {
            KlientId = klient.Id,
            TidsluckaId = lucka1.Id,
            TerapiTyp = spec,
            AnledningTillBesok = "Inledande samtal"
        }, ApiJson.Options);
        första.EnsureSuccessStatusCode();

        var andra = await _client.PostAsJsonAsync("/api/Bokning", new
        {
            KlientId = klient.Id,
            TidsluckaId = lucka2.Id,
            TerapiTyp = spec,
            AnledningTillBesok = "Försök att lägga till ett extra besök samma dag"
        }, ApiJson.Options);

        Assert.Equal(HttpStatusCode.Conflict, andra.StatusCode);
    }

    [Fact]
    public async Task Bokning_MisslyckasOm_TerapeutenSaknarRattSpecialisering()
    {
        // Terapeuter har olika kompetensområden. En psykolog som inte arbetar
        // med beroende får inte ta beroendebokningar — patientsäkerheten kräver
        // att kompetensen matchar terapityp, annars riskerar klienten att få
        // fel slags behandling.
        await ÅterställTillKändSeedAsync();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HappyHomeDbContext>();

        var terapeut = db.Terapeuter.First(t => t.Aktiv);
        var saknadTyp = Enum.GetValues<TerapiTyp>()
            .First(typ => !terapeut.Specialiseringar.Contains(typ));

        var klient = db.Klienter.First();
        var lucka = NyLedigLucka(terapeut.Id, DateTime.Today.AddDays(21).AddHours(10));
        db.Tidsluckor.Add(lucka);
        db.SaveChanges();

        var response = await _client.PostAsJsonAsync("/api/Bokning", new
        {
            KlientId = klient.Id,
            TidsluckaId = lucka.Id,
            TerapiTyp = saknadTyp,
            AnledningTillBesok = "Försöker boka utanför terapeutens specialisering"
        }, ApiJson.Options);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Bokning_NarAvbokas_FrigorTidsluckanAutomatiskt()
    {
        // När en bokning avbokas måste luckan omedelbart bli bokningsbar igen.
        // Annars förlorar kliniken intäkter och nästa klient i kön får vänta i
        // onödan. Frigörandet ska aldrig vara ett manuellt steg — det är en
        // garanterad sidoeffekt av avbokningen.
        await ÅterställTillKändSeedAsync();

        int luckaId;
        int bokningId;
        using (var setup = _factory.Services.CreateScope())
        {
            var db = setup.ServiceProvider.GetRequiredService<HappyHomeDbContext>();
            var terapeut = db.Terapeuter.First(t => t.Aktiv);
            var klient = db.Klienter.First();
            var spec = terapeut.Specialiseringar.First();
            var lucka = NyLedigLucka(terapeut.Id, DateTime.Today.AddDays(28).AddHours(11));
            db.Tidsluckor.Add(lucka);
            db.SaveChanges();
            luckaId = lucka.Id;

            var skapaSvar = await _client.PostAsJsonAsync("/api/Bokning", new
            {
                KlientId = klient.Id,
                TidsluckaId = lucka.Id,
                TerapiTyp = spec,
                AnledningTillBesok = "Test av avbokning"
            }, ApiJson.Options);
            skapaSvar.EnsureSuccessStatusCode();

            var bokning = await skapaSvar.Content.ReadFromJsonAsync<Bokning>(ApiJson.Options);
            Assert.NotNull(bokning);
            bokningId = bokning.Id;
        }

        // Före avbokning ska luckan vara markerad som bokad.
        using (var verify = _factory.Services.CreateScope())
        {
            var db = verify.ServiceProvider.GetRequiredService<HappyHomeDbContext>();
            var luckaFöre = db.Tidsluckor.Find(luckaId);
            Assert.Equal(TidsluckaStatus.Bokad, luckaFöre!.Status);
        }

        var avbokSvar = await _client.PutAsJsonAsync(
            $"/api/Bokning/{bokningId}/status",
            new { Status = BokningStatus.Avbokad },
            ApiJson.Options);
        avbokSvar.EnsureSuccessStatusCode();

        using (var verify = _factory.Services.CreateScope())
        {
            var db = verify.ServiceProvider.GetRequiredService<HappyHomeDbContext>();
            var luckaEfter = db.Tidsluckor.Find(luckaId);
            Assert.Equal(TidsluckaStatus.Ledig, luckaEfter!.Status);
        }
    }

    [Fact]
    public async Task Konsultation_HarAlltid_KopplingTillTerapeut()
    {
        // En konsultation utan terapeut är inte spårbar — den kan inte signeras,
        // inte faktureras och inte revideras av rätt person. Drop-in är tillåtet
        // (BokningId är nullable), men TerapeutId är obligatorisk och måste peka
        // på en faktiskt existerande terapeut.
        await ÅterställTillKändSeedAsync();

        // Regeln upprätthålls dels på typnivå: TerapeutId är int, inte int?.
        var terapeutIdProp = typeof(Konsultation).GetProperty(nameof(Konsultation.TerapeutId));
        Assert.NotNull(terapeutIdProp);
        Assert.Equal(typeof(int), terapeutIdProp!.PropertyType);

        // ...och dels i praktiken: ingen seedad konsultation saknar terapeut.
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HappyHomeDbContext>();

        var konsultationer = db.Konsultationer.ToList();
        Assert.NotEmpty(konsultationer);

        var terapeutIder = db.Terapeuter.Select(t => t.Id).ToHashSet();
        Assert.All(konsultationer, k =>
        {
            Assert.True(k.TerapeutId > 0, "Konsultation saknar terapeut");
            Assert.Contains(k.TerapeutId, terapeutIder);
        });
    }

    private static Tidslucka NyLedigLucka(int terapeutId, DateTime start) => new()
    {
        TerapeutId = terapeutId,
        Start = start,
        Slut = start.AddHours(1),
        Status = TidsluckaStatus.Ledig,
        Skapad = DateTime.UtcNow
    };

    private static DateTime NästaVeckansMåndag()
    {
        var idag = DateTime.Today;
        var dagarTillNästaMåndag = ((int)DayOfWeek.Monday - (int)idag.DayOfWeek + 7) % 7;
        if (dagarTillNästaMåndag == 0) dagarTillNästaMåndag = 7;
        return idag.AddDays(dagarTillNästaMåndag);
    }

    private sealed record ResetResponse(string Message, Dictionary<string, int> Counts);
}
