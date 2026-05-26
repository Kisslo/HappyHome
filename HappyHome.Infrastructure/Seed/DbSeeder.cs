using Bogus;
using HappyHome.Core.Enums;
using HappyHome.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace HappyHome.Infrastructure.Seed;

public static class DbSeeder
{
    private static readonly (TerapeutRoll Roll, TerapiTyp[] Specs)[] TerapeutProfiler =
    {
        (TerapeutRoll.Psykolog, new[] { TerapiTyp.Individuell, TerapiTyp.Par }),
        (TerapeutRoll.Psykiatriker, new[] { TerapiTyp.Individuell, TerapiTyp.Kris }),
        (TerapeutRoll.Familjeterapeut, new[] { TerapiTyp.Familj, TerapiTyp.Par, TerapiTyp.Grupp }),
        (TerapeutRoll.Beroendeterapeut, new[] { TerapiTyp.Beroende, TerapiTyp.Individuell }),
        (TerapeutRoll.Krisspecialist, new[] { TerapiTyp.Kris, TerapiTyp.Individuell })
    };

    private static readonly (string Förnamn, string Efternamn)[] BlandadeTerapeutNamn =
    {
        ("Sara", "Lindström"),
        ("Amir", "Hassan"),
        ("Elena", "Kowalska"),
        ("James", "Murphy"),
        ("Mei", "Chen")
    };

    private static readonly (string Förnamn, string Efternamn)[] BlandadeKlientNamn =
    {
        ("Erik", "Johansson"),
        ("Fatima", "Ali"),
        ("Liam", "O'Connor"),
        ("Aya", "Osman"),
        ("Maria", "Karlsson"),
        ("Omar", "Ibrahim"),
        ("Sofia", "Andersson"),
        ("Yasmin", "Haddad"),
        ("David", "Nguyen"),
        ("Nina", "Petrova"),
        ("Chen", "Wei"),
        ("Hanna", "Bergström"),
        ("Samir", "Khalil"),
        ("Emma", "Lindqvist"),
        ("Priya", "Sharma")
    };

    public static void Clear(HappyHomeDbContext db)
    {
        if (db.Database.IsRelational() && !IsInMemory(db))
        {
            db.Användare.ExecuteDelete();
            db.Konsultationer.ExecuteDelete();
            db.Bokningar.ExecuteDelete();
            db.Tidsluckor.ExecuteDelete();
            db.Terapeuter.ExecuteDelete();
            db.Klienter.ExecuteDelete();
            return;
        }

        db.Användare.RemoveRange(db.Användare);
        db.Konsultationer.RemoveRange(db.Konsultationer);
        db.Bokningar.RemoveRange(db.Bokningar);
        db.Tidsluckor.RemoveRange(db.Tidsluckor);
        db.Terapeuter.RemoveRange(db.Terapeuter);
        db.Klienter.RemoveRange(db.Klienter);
        db.SaveChanges();
    }

    private static bool IsInMemory(HappyHomeDbContext db) =>
        db.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory";

    public static void Seed(HappyHomeDbContext db, bool force = false)
    {
        if (!force && db.Terapeuter.Any())
            return;

        var nu = DateTime.UtcNow;
        var faker = new Faker();

        var terapeuter = TerapeutProfiler.Select((profil, i) =>
        {
            var (förnamn, efternamn) = BlandadeTerapeutNamn[i];
            return new Terapeut
            {
                Förnamn = förnamn,
                Efternamn = efternamn,
                Epost = SkapaEpost(förnamn, efternamn, "happyhome.se"),
                Roll = profil.Roll,
                Specialiseringar = profil.Specs.ToList(),
                AktivFromDatum = faker.Date.Past(5, nu),
                Aktiv = true,
                Skapad = nu
            };
        }).ToList();

        db.Terapeuter.AddRange(terapeuter);
        db.SaveChanges();

        var valdaKlientNamn = faker.Random.Shuffle(BlandadeKlientNamn).Take(10).ToList();
        var klienter = valdaKlientNamn.Select(namn =>
        {
            var födelsedatum = faker.Date.Between(new DateTime(1965, 1, 1), new DateTime(2005, 12, 31));
            return new Klient
            {
                Förnamn = namn.Förnamn,
                Efternamn = namn.Efternamn,
                Födelsedatum = födelsedatum,
                Personnummer = GenereraPersonnummer(födelsedatum, faker),
                Telefon = faker.Phone.PhoneNumber("07#-### ## ##"),
                Epost = SkapaEpost(namn.Förnamn, namn.Efternamn, "example.com"),
                Skapad = nu
            };
        }).ToList();
        db.Klienter.AddRange(klienter);
        db.SaveChanges();

        var veckansMåndag = NästaVeckansMåndag();
        var luckor = new List<Tidslucka>();
        var luckaIndex = 0;

        for (var dag = 0; dag < 5; dag++)
        {
            for (var terapeutIdx = 0; terapeutIdx < terapeuter.Count; terapeutIdx++)
            {
                if (luckaIndex >= 20) break;

                var start = veckansMåndag.AddDays(dag).AddHours(9 + (luckaIndex % 4));
                luckor.Add(new Tidslucka
                {
                    TerapeutId = terapeuter[terapeutIdx].Id,
                    Start = start,
                    Slut = start.AddHours(1),
                    Status = TidsluckaStatus.Ledig,
                    Skapad = nu
                });
                luckaIndex++;
            }
        }

        db.Tidsluckor.AddRange(luckor);
        db.SaveChanges();

        var bokningsLuckor = luckor.Take(10).ToList();
        var bokningar = new List<Bokning>();

        for (var i = 0; i < 10; i++)
        {
            var lucka = bokningsLuckor[i];
            var terapeut = terapeuter.First(t => t.Id == lucka.TerapeutId);
            var terapiTyp = terapeut.Specialiseringar[faker.Random.Int(0, terapeut.Specialiseringar.Count - 1)];
            var klient = klienter[i % klienter.Count];

            bokningar.Add(new Bokning
            {
                KlientId = klient.Id,
                TidsluckaId = lucka.Id,
                TerapiTyp = terapiTyp,
                AnledningTillBesok = faker.Lorem.Sentence(8),
                Status = BokningStatus.Bokad,
                Skapad = nu
            });

            lucka.Status = TidsluckaStatus.Bokad;
        }

        db.Bokningar.AddRange(bokningar);
        db.SaveChanges();

        var konsultationFaker = new Faker();
        var konsultationer = new List<Konsultation>();

        for (var i = 0; i < 5; i++)
        {
            var bokning = bokningar[i];
            var terapeut = terapeuter.First(t => t.Id == bokningsLuckor[i].TerapeutId);

            konsultationer.Add(new Konsultation
            {
                KlientId = bokning.KlientId,
                TerapeutId = terapeut.Id,
                BokningId = bokning.Id,
                Typ = bokning.TerapiTyp,
                Datum = bokningsLuckor[i].Start,
                Symptom = konsultationFaker.Lorem.Paragraph(2),
                Bakgrund = konsultationFaker.Lorem.Paragraphs(2),
                Anteckningar = konsultationFaker.Lorem.Sentence(),
                Diagnosförslag = konsultationFaker.PickRandom<string?>(null, "Utvärderas vid återbesök", "Stressrelaterad reaktion"),
                Skapad = nu
            });
        }

        db.Konsultationer.AddRange(konsultationer);
        db.SaveChanges();

        SeedaTestanvändare(db, klienter, terapeuter, nu);
    }

    // Tre testkonton för demonstrationen. Alla har samma lösenord ("Demo123!")
    // så att kursdeltagarna snabbt kan logga in i tre olika roller. I ett riktigt
    // system skulle vi aldrig dela lösenord eller sätta dem deterministiskt.
    private static void SeedaTestanvändare(
        HappyHomeDbContext db,
        List<Klient> klienter,
        List<Terapeut> terapeuter,
        DateTime nu)
    {
        const string lösenord = "Demo123!";
        var hash = BCrypt.Net.BCrypt.HashPassword(lösenord);

        db.Användare.AddRange(
            new Användare
            {
                Epost = "klient@happyhome.se",
                LösenordHash = hash,
                Roll = AnvändarRoll.Klient,
                KlientId = klienter[0].Id,
                Skapad = nu
            },
            new Användare
            {
                Epost = "terapeut@happyhome.se",
                LösenordHash = hash,
                Roll = AnvändarRoll.Terapeut,
                TerapeutId = terapeuter[0].Id,
                Skapad = nu
            },
            new Användare
            {
                Epost = "admin@happyhome.se",
                LösenordHash = hash,
                Roll = AnvändarRoll.Admin,
                Skapad = nu
            });

        db.SaveChanges();
    }

    public static async Task<Dictionary<string, int>> CountsAsync(HappyHomeDbContext db, CancellationToken ct = default) =>
        new()
        {
            ["Klienter"] = await db.Klienter.CountAsync(ct),
            ["Terapeuter"] = await db.Terapeuter.CountAsync(ct),
            ["Tidsluckor"] = await db.Tidsluckor.CountAsync(ct),
            ["Bokningar"] = await db.Bokningar.CountAsync(ct),
            ["Konsultationer"] = await db.Konsultationer.CountAsync(ct),
            ["Användare"] = await db.Användare.CountAsync(ct)
        };

    private static DateTime NästaVeckansMåndag()
    {
        var idag = DateTime.Today;
        var dagarTillNästaMåndag = ((int)DayOfWeek.Monday - (int)idag.DayOfWeek + 7) % 7;
        if (dagarTillNästaMåndag == 0) dagarTillNästaMåndag = 7;
        return idag.AddDays(dagarTillNästaMåndag);
    }

    private static string GenereraPersonnummer(DateTime födelsedatum, Faker faker)
    {
        var suffix = faker.Random.Int(1000, 9999);
        return $"{födelsedatum:yyMMdd}-{suffix:D4}";
    }

    private static string SkapaEpost(string förnamn, string efternamn, string domän)
    {
        var lokal = $"{förnamn}.{efternamn}"
            .ToLowerInvariant()
            .Replace("å", "a").Replace("ä", "a").Replace("ö", "o")
            .Replace("é", "e").Replace("ü", "u").Replace("'", "")
            .Replace(" ", ".");
        return $"{lokal}@{domän}";
    }
}
